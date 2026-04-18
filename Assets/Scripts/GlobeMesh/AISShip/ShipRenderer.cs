using UnityEngine;
using System.Collections.Generic;

public class ShipRenderer : MonoBehaviour
{
    public Mesh shipMesh;
    public Material shipMat;
    [SerializeField] private Transform m_GlobeTransform;
    [SerializeField] private Camera m_MapCamera;

    // Max I can handle
    public int shipCount = 5000;

    /// <summary>
    /// 
    /// </summary>
    public Transform GlobeTransform => m_GlobeTransform;
    
    private ComputeBuffer shipDataBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    private List<ShipData> currentShips = new List<ShipData>();
    private readonly object shipLock = new object();

    private List<ShipData> visibleShipList;
    private ShipData[] visibleShipArray;

    /// <summary>
    /// 
    /// </summary>
    public struct ShipData {
        public float lat;
        public float lon;
        public float heading;
    }

    void Start() {
        visibleShipList = new List<ShipData>(shipCount);
        visibleShipArray = new ShipData[shipCount];
        shipDataBuffer = new ComputeBuffer(shipCount, 12);
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void UpdateShips(List<ShipData> newShipData) {
        lock(shipLock) {
            currentShips = new List<ShipData>(newShipData);
        }
    }

    void LateUpdate() {
        if (shipMesh == null || shipMat == null || m_MapCamera == null) return;
        
        List<ShipData> shipsToRender;
        lock(shipLock) {
            shipsToRender = new List<ShipData>(currentShips);
        }
        
        if (shipsToRender.Count == 0) return;

        // 1. Perform Culling
        visibleShipList.Clear();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_MapCamera);
        Vector3 camPos = m_MapCamera.transform.position;
        Vector3 globePos = m_GlobeTransform.position;

        for (int i = 0; i < shipsToRender.Count; i++) {
            Vector3 worldPos = GetShipWorldPos(shipsToRender[i].lat, shipsToRender[i].lon);

            // A. Horizon Culling
            Vector3 dirToShip = (worldPos - camPos).normalized;
            Vector3 globeNormal = (worldPos - globePos).normalized;
            if (Vector3.Dot(dirToShip, globeNormal) > 0.1f) continue;

            // B. Frustum Culling
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(worldPos, Vector3.one * 0.5f))) {
                visibleShipList.Add(shipsToRender[i]);
            }
        }

        int visibleCount = visibleShipList.Count;
        if (visibleCount == 0) return;

        // 2. Prepare Data for GPU
        // Copy visible list to array for SetData
        for (int i = 0; i < visibleCount; i++) {
            visibleShipArray[i] = visibleShipList[i];
        }
        shipDataBuffer.SetData(visibleShipArray, 0, 0, visibleCount);

        // 3. Update Indirect Arguments
        args[0] = shipMesh.GetIndexCount(0);
        args[1] = (uint)visibleCount; // Only draw visible instances
        args[2] = shipMesh.GetIndexStart(0);
        args[3] = shipMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);

        // 4. Render
        shipMat.SetBuffer("_ShipDataBuffer", shipDataBuffer);
        shipMat.SetMatrix("_GlobeMatrix", m_GlobeTransform.localToWorldMatrix);

        // Use a bounds that covers the whole globe area
        Bounds drawBounds = new Bounds(globePos, Vector3.one * 20f);
        Graphics.DrawMeshInstancedIndirect(shipMesh, 0, shipMat, drawBounds, argsBuffer);
    }

    private Vector3 GetShipWorldPos(float lat, float lon) {
        float radLat = lat * Mathf.Deg2Rad;
        float radLon = lon * Mathf.Deg2Rad;

        // Matching your longitudeLatitudeToPoint math
        float y = Mathf.Sin(radLat);
        float r = Mathf.Cos(radLat); 
        float x = Mathf.Sin(radLon) * r;
        float z = -Mathf.Cos(radLon) * r;

        // 0.5 radius (for scale 10 globe) + small offset
        Vector3 localPos = new Vector3(x, y, z) * 0.5f;
        return m_GlobeTransform.TransformPoint(localPos);
    }

    void OnDisable() {
        shipDataBuffer?.Release();
        argsBuffer?.Release();
    }
}