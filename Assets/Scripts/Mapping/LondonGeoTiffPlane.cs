using System;
using UnityEngine;

public class LondonGeoTiffPlane : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float m_MapScale = 0.01f; // 1 unit = 100 meters
    
    private const double minX = -22228.9760;
    private const double maxX = -5844.9760;
    private const double minY = 6706475.0107;
    private const double maxY = 6716715.0107;

    private const double EARTH_RADIUS = 6378137.0;

    // Geometric Center in EPSG:3857 meters
    private double centerX = (minX + maxX) / 2.0;
    private double centerY = (minY + maxY) / 2.0;

    private void Start()
    {
        SetupPlane();
    }

    [ContextMenu("Setup Plane")]
    public void SetupPlane()
    {
        float width = (float)(maxX - minX);
        float height = (float)(maxY - minY);

        // Apply the scale to the physical object
        transform.localScale = new Vector3((width / 10f) * m_MapScale, 1f, (height / 10f) * m_MapScale);
        transform.position = Vector3.zero; 
    }

    /// <summary>
    /// 
    /// </summary>
    public Vector3 LatLonToWorldPosition(double lat, double lon)
    {
        // 1. Convert Lat/Lon to Global Web Mercator Meters
        double xMeters = EARTH_RADIUS * (lon * Mathf.Deg2Rad);
        double latRad = lat * Mathf.Deg2Rad;
        double yMeters = EARTH_RADIUS * Math.Log(Math.Tan(Math.PI / 4.0 + latRad / 2.0));

        float localX = (float)(xMeters - centerX) * m_MapScale;
        float localZ = (float)(yMeters - centerY) * m_MapScale;

        return new Vector3(localX, 0.1f * m_MapScale, localZ);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public Vector2 WorldPositionToLatLon(Vector3 worldPos)
    {
        // We assume the plane is at Vector3.zero as per SetupPlane()
        double xMeters = (worldPos.x / m_MapScale) + centerX;
        double yMeters = (worldPos.z / m_MapScale) + centerY;

        // 2. Reverse Longitude: x / Radius
        double lonRad = xMeters / EARTH_RADIUS;
        double lon = lonRad * Mathf.Rad2Deg;

        // 3. Reverse Latitude: Inverse of the Mercator log/tan formula
        // This is: lat = 2 * atan(exp(y / R)) - PI / 2
        double latRad = 2.0 * Math.Atan(Math.Exp(yMeters / EARTH_RADIUS)) - (Math.PI / 2.0);
        double lat = latRad * Mathf.Rad2Deg;

        return new Vector2((float)lat, (float)lon);
    }
}