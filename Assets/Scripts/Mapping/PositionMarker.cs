using UnityEngine;

/// <summary>
/// 
/// </summary>
public class PositionMarker : MonoBehaviour
{
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;
    [SerializeField] private float m_Latitude;
    [SerializeField] private float m_Longitude;

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Position at Long Lat")]
    public void PositionAtLongLat()
    {
        Vector3 worldMapPos = m_LondonMapPlane.LatLonToWorldPosition(m_Latitude, m_Longitude);
        transform.position = worldMapPos;
    }

}