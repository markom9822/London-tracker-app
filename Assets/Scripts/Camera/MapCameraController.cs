using UnityEngine;

/// <summary>
/// 
/// </summary>
public class MapCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;

    [Header("Offset Settings")]
    [Tooltip("The rotation of the camera (e.g., 45, 0, 0 for a tilted view)")]
    [SerializeField] private Vector3 m_RotationOffset = new Vector3(45f, 0f, 0f);
    
    [Tooltip("How far back from the Lat/Lon point the camera should sit")]
    [SerializeField] private float m_ViewDistance = 500f;

    [Header("Movement")]
    [SerializeField] private float m_LerpSpeed = 5f;
    
    [Header("Initialisation")]
    [SerializeField] private float m_StartLatitude;
    [SerializeField] private float m_StartLongitude;
    
    private Vector3 m_TargetPivotPoint;
    private float m_TargetLatitude;
    private float m_TargetLongitude;
    
    /// <summary>
    /// 
    /// </summary>
    public Vector3 TargetPivotPoint => m_TargetPivotPoint;

    /// <summary>
    /// 
    /// </summary>
    public float TargetLatitude => m_TargetLatitude;
    
    /// <summary>
    /// 
    /// </summary>
    public float TargetLongitude => m_TargetLongitude;

    private void Start()
    {
        JumpToLatLon(m_StartLatitude, m_StartLongitude);
    }

    /// <summary>
    /// Instantly snaps the camera using the offset logic
    /// </summary>
    public void JumpToLatLon(float lat, float lon)
    {
        m_TargetLatitude = lat;
        m_TargetLongitude = lon;
        
        m_TargetPivotPoint = m_LondonMapPlane.LatLonToWorldPosition(lat, lon);
        
        transform.rotation = Quaternion.Euler(m_RotationOffset);
        transform.position = m_TargetPivotPoint - (transform.forward * m_ViewDistance);
    }

    /// <summary>
    /// Updates the pivot point so the Update loop can lerp the camera
    /// </summary>
    public void FlyToLatLon(float lat, float lon)
    {
        m_TargetLatitude = lat;
        m_TargetLongitude = lon;
        m_TargetPivotPoint = m_LondonMapPlane.LatLonToWorldPosition(lat, lon);
    }

    private void Update()
    {
        Quaternion targetRotation = Quaternion.Euler(m_RotationOffset);
        
        Vector3 desiredPosition = m_TargetPivotPoint - (targetRotation * Vector3.forward * m_ViewDistance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * m_LerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_LerpSpeed);
    }
}