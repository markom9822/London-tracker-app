using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class MeasureOnLondonMap : MonoBehaviour
{
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;
    [SerializeField] private Vector2 m_LatLongPoint1;
    [SerializeField] private Transform m_TestPointTransform;

    [SerializeField] private Vector3 m_LatLongPosition1;
    [SerializeField] private float m_DistanceBetweenPoints;
    
    /// <summary>
    /// 
    /// </summary>
    public float CalculateHaversineDistance(Vector2 p1, Vector2 p2)
    {
        double R = 6371000.0; // Earth's radius in meters
    
        // x = Latitude, y = Longitude
        double dLat = (p2.x - p1.x) * Mathf.Deg2Rad;
        double dLon = (p2.y - p1.y) * Mathf.Deg2Rad;

        double lat1 = p1.x * Mathf.Deg2Rad;
        double lat2 = p2.x * Mathf.Deg2Rad;

        double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                   Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);
              
        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        return (float)(R * c);
    }
    
    private void OnDrawGizmos()
    {
        // point 1
        m_LatLongPosition1 = m_LondonMapPlane.LatLonToWorldPosition(m_LatLongPoint1.x, m_LatLongPoint1.y);
        Vector2 latLon = m_LondonMapPlane.WorldPositionToLatLon(m_TestPointTransform.position);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_LatLongPosition1, 0.2f);
        Gizmos.DrawLine(m_LatLongPosition1, m_TestPointTransform.position);
        Gizmos.color = Color.crimson;
        Gizmos.DrawSphere(m_TestPointTransform.position, 0.2f);

        m_DistanceBetweenPoints = CalculateHaversineDistance(m_LatLongPoint1, latLon);
    }
}