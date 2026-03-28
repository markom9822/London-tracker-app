using UnityEngine;

/// <summary>
/// 
/// </summary>
public class MapCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;

    [Header("Settings")]
    [SerializeField] private float m_ViewAltitude = 2000f; // Height above the map
    [SerializeField] private float m_LerpSpeed = 5f;      // Smoothness of movement

    private Vector3 m_TargetPosition;

    private void Start()
    {
        // Initialize position to center of map
        m_TargetPosition = new Vector3(0, m_ViewAltitude, 0);
        transform.position = m_TargetPosition;
        transform.rotation = Quaternion.Euler(90, 0, 0); // Look straight down
    }

    /// <summary>
    /// Instantly snaps the camera to a Lat/Lon
    /// </summary>
    public void JumpToLatLon(double lat, double lon)
    {
        Vector3 mapPos = m_LondonMapPlane.LatLonToWorldPosition(lat, lon);
        m_TargetPosition = new Vector3(mapPos.x, m_ViewAltitude, mapPos.z);
        transform.position = m_TargetPosition;
    }

    /// <summary>
    /// Smoothly moves the camera to a Lat/Lon (call this from a UI button)
    /// </summary>
    public void FlyToLatLon(double lat, double lon)
    {
        Vector3 mapPos = m_LondonMapPlane.LatLonToWorldPosition(lat, lon);
        m_TargetPosition = new Vector3(mapPos.x, m_ViewAltitude, mapPos.z);
    }

    private void Update()
    {
        // Smoothly move towards target if you're using FlyToLatLon
        transform.position = Vector3.Lerp(transform.position, m_TargetPosition, Time.deltaTime * m_LerpSpeed);
    }
}
