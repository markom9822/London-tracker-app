using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zeptomoby.OrbitTools;

public class SatelliteInfoClient : MonoBehaviour
{
    [Serializable]
    public class TleEntry
    {
        public string Name;
        [TextArea] public string Line1;
        [TextArea] public string Line2;
    }
    
    public enum SatelliteCategory
    {
        LEO,        // Low Earth Orbit (ISS, Starlink, Hubble)
        MEO,        // Medium Earth Orbit (GPS, Galileo)
        GEO,        // Geostationary (TV, Weather)
        HighOrbit   // Everything beyond GEO
    }
    
    [Serializable]
    public enum InclinationCategory
    {
        Equatorial, // Near 0 degrees
        Tilted,     // Standard orbits (like ISS at 51 degrees)
        Polar,      // Near 90 degrees (Weather/Spy sats)
        Retrograde  // Greater than 90 degrees (Orbits "backwards" against Earth's rotation)
    }

    [Serializable]
    public class InclinationToggle
    {
        [SerializeField] private InclinationCategory m_InclinationCategory;
        [SerializeField] private Toggle m_Toggle;
        [SerializeField] private Color m_GroupColor = Color.white;

        public InclinationCategory InclinationCategory => m_InclinationCategory;
        public Toggle Toggle => m_Toggle;
        public Color GroupColor => m_GroupColor;
    }
    
    [SerializeField] private SatelliteRenderer m_Renderer;
    [SerializeField] private bool m_IsActive = true;
    
    [Header("Data Source")]
    [Tooltip("Drag your CelesTrakData.txt file here")]
    [SerializeField] private TextAsset m_TleTextFile;
    
    [Header("Path Rendering")]
    [SerializeField] private LineRenderer m_PathRenderer;
    [SerializeField] private int m_PathSatelliteIndex = 0;

    [SerializeField] private List<InclinationToggle> m_InclinationToggles;
    
    private Dictionary<InclinationCategory, bool> m_ActiveInclinations = new Dictionary<InclinationCategory, bool>();
    
    private Dictionary<SatelliteCategory, List<Satellite>> m_ByAltitude = new Dictionary<SatelliteCategory, List<Satellite>>();
    private Dictionary<InclinationCategory, List<Satellite>> m_ByInclination = new Dictionary<InclinationCategory, List<Satellite>>();
    
    private List<Satellite> m_AllSatellites = new List<Satellite>();    
    
    private List<SatelliteRenderer.SatelliteData> m_RenderDataList = new List<SatelliteRenderer.SatelliteData>();
   
    private void Start()
    {
        foreach (SatelliteCategory cat in Enum.GetValues(typeof(SatelliteCategory))) 
            m_ByAltitude[cat] = new List<Satellite>();

        foreach (InclinationCategory cat in Enum.GetValues(typeof(InclinationCategory)))
            m_ByInclination[cat] = new List<Satellite>();

        // 2. Parse the file first so we have data to show
        if (m_TleTextFile != null) ParseTleFile();

        // 3. Hook up Toggle listeners automatically
        foreach (InclinationToggle item in m_InclinationToggles)
        {
            if (item.Toggle != null)
            {
                // We use a lambda to pass the change event
                item.Toggle.onValueChanged.AddListener((isOn) => {
                    Debug.Log($"{item.InclinationCategory} is now {(isOn ? "Visible" : "Hidden")}");
                });
            }
        }
        //DrawOrbitPath();
    }
    
    private void Update()
    {
        if (!m_IsActive || m_Renderer == null) return;

        RenderActiveToggles();
    }
    
    private void ParseTleFile()
    {
        string[] lines = m_TleTextFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i + 2 < lines.Length; i += 3)
        {
            try
            {
                TwoLineElements tle = new TwoLineElements(lines[i].Trim(), lines[i+1], lines[i+2]);
                Satellite sat = new Satellite(tle);

                // Altitude
                double alt = sat.Orbit.PerigeeKmRec;
                double period = sat.Orbit.Period.TotalMinutes;
                SatelliteCategory altCat = (period > 1400 && period < 1500) ? SatelliteCategory.GEO : 
                    (alt < 2000) ? SatelliteCategory.LEO : 
                    (alt < 30000) ? SatelliteCategory.MEO : SatelliteCategory.HighOrbit;
                m_ByAltitude[altCat].Add(sat);

                // Inclination
                double inc = tle.InclinationDeg; 
                InclinationCategory incCat;
                if (inc < 10) incCat = InclinationCategory.Equatorial;
                else if (inc >= 80 && inc <= 100) incCat = InclinationCategory.Polar;
                else if (inc > 100) incCat = InclinationCategory.Retrograde;
                else incCat = InclinationCategory.Tilted;

                m_ByInclination[incCat].Add(sat);
            }
            catch { }
        }
        Debug.Log($"Parsed {lines.Length / 3} satellites.");
    }
    
    private void RenderActiveToggles()
    {
        DateTime utcNow = DateTime.UtcNow;
        m_RenderDataList.Clear();

        // 1. Create a temporary array of colors to send to the shader
        Color[] colorsForRenderer = new Color[4];

        for (int i = 0; i < m_InclinationToggles.Count; i++)
        {
            var item = m_InclinationToggles[i];
        
            // Save the color into the array regardless of toggle state 
            // (so the index matches the shader)
            colorsForRenderer[i] = item.GroupColor;

            if (item.Toggle != null && item.Toggle.isOn)
            {
                foreach (var sat in m_ByInclination[item.InclinationCategory])
                {
                    EciTime eci = sat.PositionEci(utcNow);
                    GeoTime geo = new GeoTime(eci);

                    m_RenderDataList.Add(new SatelliteRenderer.SatelliteData()
                    {
                        lat = (float)geo.LatitudeDeg,
                        lon = (float)geo.LongitudeDeg,
                        altitude = (float)(geo.Altitude / 6371.0) * 0.5f,
                        heading = (float)i // This 'i' tells the shader which color to use
                    });
                }
            }
        }

        m_Renderer.SetCategoryColors(colorsForRenderer);
        m_Renderer.UpdateSatellites(m_RenderDataList);
    }
    
    private void DrawOrbitPath()
    {
        if (m_PathRenderer == null || m_AllSatellites.Count == 0)
        {
            return;
        }
        
        int index = Mathf.Clamp(m_PathSatelliteIndex, 0, m_AllSatellites.Count - 1);
        Satellite targetSat = m_AllSatellites[index];

        int segments = 120; // 2 hours of path
        m_PathRenderer.positionCount = segments + 1;
        m_PathRenderer.useWorldSpace = false;

        DateTime startTime = DateTime.UtcNow;

        for (int i = 0; i <= segments; i++)
        {
            // Calculate point every minute
            EciTime futureEci = targetSat.PositionEci(startTime.AddMinutes(i));
            GeoTime futureGeo = new GeoTime(futureEci);

            Vector3 pos = CalculateWorldPosition(
                (float)futureGeo.LatitudeDeg,
                (float)futureGeo.LongitudeDeg,
                (float)futureGeo.Altitude);

            m_PathRenderer.SetPosition(i, pos);
        }
    }
    
    private Vector3 CalculateWorldPosition(float lat, float lon, float altKm)
    {
        float radLat = lat * Mathf.Deg2Rad;
        float radLon = lon * Mathf.Deg2Rad;

        float y = Mathf.Sin(radLat);
        float r = Mathf.Cos(radLat);
        float x = Mathf.Sin(radLon) * r;
        float z = -Mathf.Cos(radLon) * r;

        float radius = 0.5f + ((altKm / 6371.0f) * 0.5f);
        return new Vector3(x, y, z) * radius;
    }
}