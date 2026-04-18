using System.Collections.Generic;
using UnityEngine;

public class SatelliteInfoClient : MonoBehaviour
{
    [SerializeField] private SatelliteRenderer m_Renderer;
    [SerializeField] private bool m_IsActive = true;
    [SerializeField] private int m_TestSatelliteCount = 50;

    private List<SatelliteRenderer.SatelliteData> m_Satellites = new List<SatelliteRenderer.SatelliteData>();
    
    private void Start()
    {
        // Initialize our mock satellites with random starting orbits
        for (int i = 0; i < m_TestSatelliteCount; i++)
        {
            m_Satellites.Add(new SatelliteRenderer.SatelliteData()
            {
                lat = Random.Range(-80f, 80f),
                lon = Random.Range(-180f, 180f),
                altitude = Random.Range(0.05f, 0.2f), // Height above the 0.5 radius
                heading = Random.Range(0f, 360f)
            });
        }
    }

    private void Update()
    {
        if (!m_IsActive || m_Renderer == null) return;

        // Simulate orbital movement
        for (int i = 0; i < m_Satellites.Count; i++)
        {
            var sat = m_Satellites[i];
            
            // Move longitude to simulate West-to-East orbit
            sat.lon += Time.deltaTime * 5f; 
            if (sat.lon > 180) sat.lon -= 360;

            // Optional: Wobble the latitude
            sat.lat += Mathf.Sin(Time.time + i) * 0.1f;

            m_Satellites[i] = sat;
        }

        // Push to the renderer
        m_Renderer.UpdateSatellites(m_Satellites);
    }
}