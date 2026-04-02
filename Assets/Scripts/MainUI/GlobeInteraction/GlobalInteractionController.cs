using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// </summary>
public class GlobalInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform m_GlobeTransform;
    [SerializeField] private Transform m_CameraTransform;
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private GameObject m_CursorMarker;

    [Header("Rotation Settings")]
    [SerializeField] private float m_BaseRotationSpeed = 0.2f;
    [SerializeField] private float m_Damping = 12.0f; // Snappy but smooth
    
    [Header("Zoom Settings")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float m_ZoomSensitivity = 0.15f; // Percentage of distance to move per scroll
    [SerializeField] private float m_MinZoomBuffer = 0.05f;
    [SerializeField] private float m_MarkerBaseScale = 0.015f;
    [Range(1f, 40f)]
    [SerializeField] private float m_MaxRotationSharpness = 25f;

    private float m_MaxDistance;
    private float m_MinDistance;
    private float m_TargetDistance; // We zoom toward a target, then Lerp current
    private float m_CurrentDistance;
    private Vector2 m_LastMousePosition;
    private bool m_IsDragging;

    private Vector3? m_LockedLocalPoint = null;
    private Vector3 m_LockedViewDir; 
    private float m_LastScrollTime;
    private const float m_ScrollLockDuration = 0.4f;

    void Start()
    {
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        if (m_CameraTransform == null && m_MainCamera != null) m_CameraTransform = m_MainCamera.transform;

        if (m_GlobeTransform && m_CameraTransform)
        {
            MeshRenderer globeRenderer = m_GlobeTransform.GetComponentInChildren<MeshRenderer>();
            float globeRadius = globeRenderer != null ? globeRenderer.bounds.extents.magnitude : 0.5f;
            m_MinDistance = globeRadius + m_MinZoomBuffer;
            
            m_CurrentDistance = Vector3.Distance(m_CameraTransform.position, m_GlobeTransform.position);
            if (m_CurrentDistance <= m_MinDistance) m_CurrentDistance = m_MinDistance + 2.0f;
            
            m_TargetDistance = m_CurrentDistance;
            m_MaxDistance = m_CurrentDistance;
        }
    }

    void Update()
    {
        UpdateCursorMarker();
        HandleRotation();
        HandleZoom();
        
        if (m_LockedLocalPoint.HasValue)
        {
            RotateTowardLockedPoint();
        }

        UpdateCameraPosition();

        if (Time.time - m_LastScrollTime > m_ScrollLockDuration)
        {
            m_LockedLocalPoint = null;
        }
    }

    private void HandleZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scrollValue = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            m_LastScrollTime = Time.time;
            
            if (scrollValue > 0) 
            {
                if (!m_LockedLocalPoint.HasValue) CaptureZoomPoint();
            }

            // EXPONENTIAL ZOOM LOGIC:
            // Instead of subtracting, we multiply the distance.
            // scrollValue is usually 120 or -120. We normalize this.
            float scrollDirection = Mathf.Sign(scrollValue); 
            
            // This creates a scaling factor (e.g., 0.85 for zoom in, 1.15 for zoom out)
            float zoomFactor = 1.0f - (scrollDirection * m_ZoomSensitivity);
            
            m_TargetDistance *= zoomFactor;
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, m_MinDistance, m_MaxDistance);
        }
    }

    private void UpdateCameraPosition()
    {
        if (m_CameraTransform == null || m_GlobeTransform == null) return;

        // Smoothly approach the target distance
        m_CurrentDistance = Mathf.Lerp(m_CurrentDistance, m_TargetDistance, Time.deltaTime * m_Damping);

        Vector3 direction = (m_CameraTransform.position - m_GlobeTransform.position).normalized;
        if (direction == Vector3.zero) direction = Vector3.back;

        Vector3 targetPos = m_GlobeTransform.position + (direction * m_CurrentDistance);

        // Smoothly move position
        m_CameraTransform.position = Vector3.Lerp(m_CameraTransform.position, targetPos, Time.deltaTime * m_Damping);
        m_CameraTransform.LookAt(m_GlobeTransform);
    }

    // ... Rest of the helper methods (CaptureZoomPoint, RotateTowardLockedPoint, UpdateCursorMarker) 
    // remain the same as your previous logic, using m_CurrentDistance for calculations.

    private void CaptureZoomPoint()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, m_MaxDistance * 2.0f))
        {
            if (hit.transform == m_GlobeTransform || hit.transform.IsChildOf(m_GlobeTransform))
            {
                m_LockedLocalPoint = m_GlobeTransform.InverseTransformPoint(hit.point);
                m_LockedViewDir = (hit.point - m_CameraTransform.position).normalized;
            }
        }
    }

    private void RotateTowardLockedPoint()
    {
        if (!m_LockedLocalPoint.HasValue) return;
        Vector3 currentWorldPoint = m_GlobeTransform.TransformPoint(m_LockedLocalPoint.Value);
        Vector3 dirToPoint = (currentWorldPoint - m_GlobeTransform.position).normalized;
        Vector3 targetPointOnRay = m_CameraTransform.position + (m_LockedViewDir * Vector3.Distance(m_CameraTransform.position, currentWorldPoint));
        Vector3 dirToTarget = (targetPointOnRay - m_GlobeTransform.position).normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(dirToPoint, dirToTarget) * m_GlobeTransform.rotation;
        float zoomProgress = Mathf.InverseLerp(m_MaxDistance, m_MinDistance, m_CurrentDistance);
        float rotationStrength = Mathf.Lerp(m_Damping, m_MaxRotationSharpness, zoomProgress);
        m_GlobeTransform.rotation = Quaternion.Slerp(m_GlobeTransform.rotation, targetRotation, Time.deltaTime * rotationStrength);
    }

    private void UpdateCursorMarker()
    {
        if (m_CursorMarker == null || m_MainCamera == null) return;
        if (m_LockedLocalPoint.HasValue)
        {
            m_CursorMarker.SetActive(true);
            m_CursorMarker.transform.position = m_GlobeTransform.TransformPoint(m_LockedLocalPoint.Value);
            m_CursorMarker.transform.localScale = Vector3.one * (m_CurrentDistance * m_MarkerBaseScale);
            return;
        }
        Ray ray = m_MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, m_MaxDistance * 2.0f))
        {
            if (hit.transform == m_GlobeTransform || hit.transform.IsChildOf(m_GlobeTransform))
            {
                m_CursorMarker.SetActive(true);
                m_CursorMarker.transform.position = hit.point;
                m_CursorMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                m_CursorMarker.transform.localScale = Vector3.one * (m_CurrentDistance * m_MarkerBaseScale);
                return;
            }
        }
        m_CursorMarker.SetActive(false);
    }

    private void HandleRotation()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) { m_IsDragging = false; return; }
        m_LockedLocalPoint = null; 
        Vector2 currentMousePos = mouse.position.ReadValue();
        if (!m_IsDragging) { m_IsDragging = true; m_LastMousePosition = currentMousePos; return; }
        Vector2 delta = currentMousePos - m_LastMousePosition;
        float zoomT = Mathf.InverseLerp(m_MinDistance, m_MaxDistance, m_CurrentDistance);
        float adaptiveSpeed = m_BaseRotationSpeed * (zoomT + 0.1f);
        m_GlobeTransform.Rotate(Vector3.up, -delta.x * adaptiveSpeed, Space.World);
        m_GlobeTransform.Rotate(m_CameraTransform.right, delta.y * adaptiveSpeed, Space.World);
        m_LastMousePosition = currentMousePos;
    }
}