using System.Collections.Generic;
using UnityEngine;

public class SatelliteRenderer : MonoBehaviour
{
    private static readonly int SatelliteDataBufferID = Shader.PropertyToID("_SatelliteDataBuffer");
    
    [SerializeField] private Mesh m_SatelliteMesh;
    [SerializeField] private Material m_SatelliteMat;
    [SerializeField] private Transform m_GlobeTransform;
    [SerializeField] private Camera m_MapCamera;

    // Max I can handle
    [SerializeField] private int m_MaxCount = 5000;

    /// <summary>
    /// 
    /// </summary>
    public Transform GlobeTransform => m_GlobeTransform;
    
    private ComputeBuffer m_SatelliteDataBuffer;
    private ComputeBuffer m_ArgsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    private List<SatelliteData> m_CurrentSatellites = new List<SatelliteData>();
    private readonly object m_SatelliteLock = new object();

    private List<SatelliteData> m_VisibleSatelliteList;
    private SatelliteData[] m_VisibleSatelliteArray;
    private Material m_InstanceMaterial; 
    private Color[] m_CategoryColorArray = new Color[4];

    /// <summary>
    /// 
    /// </summary>
    public struct SatelliteData {
        public float lat;
        public float lon;
        public float heading;
        public float altitude;
    }

    private void Start() 
    {
        m_InstanceMaterial = new Material(m_SatelliteMat);
        
        m_VisibleSatelliteList = new List<SatelliteData>(m_MaxCount);
        m_VisibleSatelliteArray = new SatelliteData[m_MaxCount];
        m_SatelliteDataBuffer = new ComputeBuffer(m_MaxCount, 16);
        m_ArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void UpdateSatellites(List<SatelliteData> newSatelliteData) {
        lock(m_SatelliteLock) {
            m_CurrentSatellites = new List<SatelliteData>(newSatelliteData);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void SetCategoryColors(Color[] colors)
    {
        if (colors == null || colors.Length != 4)
        {
            return;
        }
        m_CategoryColorArray = colors;
    }

    private void LateUpdate() {
        if (m_SatelliteMesh == null || m_SatelliteMat == null || m_MapCamera == null) return;
        
        List<SatelliteData> satelliteToRender;
        lock(m_SatelliteLock) {
            satelliteToRender = new List<SatelliteData>(m_CurrentSatellites);
        }
        
        if (satelliteToRender.Count == 0) return;

        // 1. Perform Culling
        m_VisibleSatelliteList.Clear();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_MapCamera);
        Vector3 camPos = m_MapCamera.transform.position;
        Vector3 globePos = m_GlobeTransform.position;

        for (int i = 0; i < satelliteToRender.Count; i++) {
            Vector3 worldPos = GetSatelliteWorldPos(
                satelliteToRender[i].lat, 
                satelliteToRender[i].lon, 
                satelliteToRender[i].altitude
            );
            
            // A. Horizon Culling
            Vector3 dirToSatellite = (worldPos - camPos).normalized;
            Vector3 globeNormal = (worldPos - globePos).normalized;
            if (Vector3.Dot(dirToSatellite, globeNormal) > 0.1f) continue;

            // B. Frustum Culling
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(worldPos, Vector3.one * 0.5f))) {
                m_VisibleSatelliteList.Add(satelliteToRender[i]);
            }
        }

        int visibleCount = m_VisibleSatelliteList.Count;
        if (visibleCount == 0) return;

        // 2. Prepare Data for GPU
        // Copy visible list to array for SetData
        for (int i = 0; i < visibleCount; i++) {
            m_VisibleSatelliteArray[i] = m_VisibleSatelliteList[i];
        }
        m_SatelliteDataBuffer.SetData(m_VisibleSatelliteArray, 0, 0, visibleCount);

        // 3. Update Indirect Arguments
        args[0] = m_SatelliteMesh.GetIndexCount(0);
        args[1] = (uint)visibleCount; // Only draw visible instances
        args[2] = m_SatelliteMesh.GetIndexStart(0);
        args[3] = m_SatelliteMesh.GetBaseVertex(0);
        m_ArgsBuffer.SetData(args);

        // 4. Render
        m_InstanceMaterial.SetBuffer(SatelliteDataBufferID, m_SatelliteDataBuffer);
        m_InstanceMaterial.SetMatrix("_GlobeMatrix", m_GlobeTransform.localToWorldMatrix);
        m_InstanceMaterial.SetColorArray("_CategoryColors", m_CategoryColorArray);
        
        // Use a bounds that covers the whole globe area
        Bounds drawBounds = new Bounds(globePos, Vector3.one * 20f);
        Graphics.DrawMeshInstancedIndirect(m_SatelliteMesh, 0, m_InstanceMaterial, drawBounds, m_ArgsBuffer);
    }

    private Vector3 GetSatelliteWorldPos(float lat, float lon, float altitude) {
        float radLat = lat * Mathf.Deg2Rad;
        float radLon = lon * Mathf.Deg2Rad;

        float y = Mathf.Sin(radLat);
        float r = Mathf.Cos(radLat); 
        float x = Mathf.Sin(radLon) * r;
        float z = -Mathf.Cos(radLon) * r;

        // Use the altitude here so the Culling bounds match the Shader position
        float radius = 0.5f + altitude;
        Vector3 localPos = new Vector3(x, y, z) * radius;
        return m_GlobeTransform.TransformPoint(localPos);
    }

    private void OnDestroy() 
    {
        if (m_InstanceMaterial != null)
        {
            Destroy(m_InstanceMaterial);
        }
        m_SatelliteDataBuffer?.Release();
        m_ArgsBuffer?.Release();
    }
}