using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform m_GlobeTransform;
    [SerializeField] private Transform m_CameraTransform;
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private GameObject m_CursorMarker;

    [Header("Inertia & Rotation")]
    [SerializeField] private float m_BaseRotationSpeed = 0.2f;
    [SerializeField] private float m_InertiaDamping = 0.94f; // Higher = slides longer
    [SerializeField] private float m_VelocitySmoothSpeed = 10f;

    [Header("Zoom & Orbit Settings")]
    [SerializeField] private float m_Damping = 12.0f; 
    [Range(0.01f, 0.5f)]
    [SerializeField] private float m_ZoomSensitivity = 0.15f;
    [SerializeField] private float m_MinZoomBuffer = 0.05f;
    [SerializeField] private float m_MarkerBaseScale = 0.015f;
    [SerializeField] private float m_ScrollLockDuration = 0.4f;

    [Header("Lock-On Tuning")]
    [SerializeField] private float m_MaxOrbitSharpness = 10f;
    [SerializeField] private float m_MinOrbitMultiplier = 0.5f;

    private float m_MaxDistance;
    private float m_MinDistance;
    private float m_TargetDistance;
    private float m_CurrentDistance;
    private Vector2 m_LastMousePosition;
    private bool m_IsDragging;

    // Inertia Variables
    private Vector2 m_RotationVelocity;
    private Vector3? m_LockedLocalPoint = null;
    private float m_LastScrollTime;

    void Start()
    {
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        if (m_CameraTransform == null && m_MainCamera != null) m_CameraTransform = m_MainCamera.transform;

        if (m_GlobeTransform && m_CameraTransform)
        {
            MeshRenderer globeRenderer = m_GlobeTransform.GetComponentInChildren<MeshRenderer>();
            float globeRadius = 0.5f;
            if (globeRenderer != null)
            {
                Vector3 extents = globeRenderer.bounds.extents;
                globeRadius = Mathf.Max(extents.x, extents.y, extents.z);
            }
            m_MinDistance = globeRadius + m_MinZoomBuffer;
            m_CurrentDistance = Vector3.Distance(m_CameraTransform.position, m_GlobeTransform.position);
            m_TargetDistance = m_CurrentDistance;
            m_MaxDistance = m_CurrentDistance;
        }
    }

    void Update()
    {
        UpdateCursorMarker();
        HandleManualRotation();
        HandleZoomInput();
        UpdateCameraTransform();
        ApplyInertia();

        if (Time.time - m_LastScrollTime > m_ScrollLockDuration)
        {
            m_LockedLocalPoint = null;
        }
    }

    private void HandleManualRotation()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 currentMousePos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            m_IsDragging = true;
            m_LastMousePosition = currentMousePos;
            m_LockedLocalPoint = null;
            m_RotationVelocity = Vector2.zero; // Stop any current sliding
        }

        if (mouse.leftButton.isPressed && m_IsDragging)
        {
            Vector2 delta = currentMousePos - m_LastMousePosition;
            
            // Calculate instantaneous velocity to use for inertia later
            // We use Lerp to smooth out frame-rate spikes in mouse movement
            Vector2 frameVelocity = delta / Time.deltaTime;
            m_RotationVelocity = Vector2.Lerp(m_RotationVelocity, frameVelocity, Time.deltaTime * m_VelocitySmoothSpeed);

            RotateGlobe(delta);
            m_LastMousePosition = currentMousePos;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            m_IsDragging = false;
        }
    }

    private void ApplyInertia()
    {
        // Don't apply inertia if we are currently grabbing or if velocity is near zero
        if (m_IsDragging || m_RotationVelocity.magnitude < 0.1f) return;

        // Apply the stored velocity
        RotateGlobe(m_RotationVelocity * Time.deltaTime);

        // Gradually slow down (Friction)
        m_RotationVelocity *= m_InertiaDamping;
    }

    private void RotateGlobe(Vector2 delta)
    {
        float zoomT = Mathf.InverseLerp(m_MinDistance, m_MaxDistance, m_CurrentDistance);
        float adaptiveSpeed = m_BaseRotationSpeed * (zoomT + 0.1f);

        m_GlobeTransform.Rotate(Vector3.up, -delta.x * adaptiveSpeed, Space.World);
        m_GlobeTransform.Rotate(m_CameraTransform.right, delta.y * adaptiveSpeed, Space.World);
    }

    private void HandleZoomInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scrollValue = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            m_LastScrollTime = Time.time;
            if (scrollValue > 0 && !m_LockedLocalPoint.HasValue && !m_IsDragging)
            {
                CaptureZoomPoint();
            }
            
            float scrollDirection = Mathf.Sign(scrollValue); 
            float zoomFactor = 1.0f - (scrollDirection * m_ZoomSensitivity);
            m_TargetDistance *= zoomFactor;
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, m_MinDistance, m_MaxDistance);
        }
    }

    private void UpdateCameraTransform()
    {
        if (m_CameraTransform == null || m_GlobeTransform == null) return;

        m_CurrentDistance = Mathf.Lerp(m_CurrentDistance, m_TargetDistance, Time.deltaTime * m_Damping);
        Vector3 currentDir = (m_CameraTransform.position - m_GlobeTransform.position).normalized;
        if (currentDir == Vector3.zero) currentDir = Vector3.back;

        if (m_LockedLocalPoint.HasValue)
        {
            Vector3 worldTargetPoint = m_GlobeTransform.TransformPoint(m_LockedLocalPoint.Value);
            Vector3 targetDir = (worldTargetPoint - m_GlobeTransform.position).normalized;

            float zoomProgress = Mathf.InverseLerp(m_MaxDistance, m_MinDistance, m_CurrentDistance);
            float dynamicSharpness = Mathf.Lerp(m_Damping * m_MinOrbitMultiplier, m_MaxOrbitSharpness, zoomProgress);

            currentDir = Vector3.Slerp(currentDir, targetDir, Time.deltaTime * dynamicSharpness);
            if (Vector3.Angle(currentDir, targetDir) < 0.1f) currentDir = targetDir;
        }

        m_CameraTransform.position = m_GlobeTransform.position + (currentDir * m_CurrentDistance);
        m_CameraTransform.LookAt(m_GlobeTransform);
    }

    private void CaptureZoomPoint()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, m_MaxDistance * 2.0f))
        {
            if (hit.transform == m_GlobeTransform || hit.transform.IsChildOf(m_GlobeTransform))
            {
                m_LockedLocalPoint = m_GlobeTransform.InverseTransformPoint(hit.point);
                m_RotationVelocity = Vector2.zero; // Stop spinning if we start a lock-on zoom
            }
        }
    }

    private void UpdateCursorMarker()
    {
        if (m_CursorMarker == null || m_MainCamera == null) return;
        Vector3? markerPos = null;

        if (m_LockedLocalPoint.HasValue)
        {
            markerPos = m_GlobeTransform.TransformPoint(m_LockedLocalPoint.Value);
        }
        else
        {
            Ray ray = m_MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, m_MaxDistance * 2.0f))
            {
                if (hit.transform == m_GlobeTransform || hit.transform.IsChildOf(m_GlobeTransform))
                {
                    markerPos = hit.point;
                    m_CursorMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
            }
        }

        if (markerPos.HasValue)
        {
            m_CursorMarker.SetActive(true);
            m_CursorMarker.transform.position = markerPos.Value;
            m_CursorMarker.transform.localScale = Vector3.one * (m_CurrentDistance * m_MarkerBaseScale);
        }
        else m_CursorMarker.SetActive(false);
    }
}