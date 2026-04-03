using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 
/// </summary>
public class TrafficCameraProvider : MapLayerDataProvider
{
    [Serializable]
    public class JamCamResponse
    {
        public JamCamPlace[] places;
    }

    [Serializable]
    public class JamCamPlace
    {
        public string id;
        public string commonName;
        public float lat;
        public float lon;
        public AdditionalProperty[] additionalProperties;

        // Helper method to extract data from the nested properties list
        public string GetPropertyValue(string key)
        {
            if (additionalProperties == null) return string.Empty;
            foreach (var prop in additionalProperties)
            {
                if (prop.key == key) return prop.value;
            }
            return string.Empty;
        }
    }

    [Serializable]
    public class AdditionalProperty
    {
        public string key;
        public string value;
    }
    
    private const string BASE_URL = "https://api.tfl.gov.uk/Place?type=JamCam";
    
    [SerializeField] private MapCameraController m_MapCameraController;
    [SerializeField] private LondonGeoTiffPlane m_MapPlane;
    [SerializeField] private LondonMapAreaCircle m_MapAreaCircle;
    [SerializeField, Tooltip("Search Radius in meters from the target")] private float m_SearchRadius = 500f;
    [SerializeField] private GameObject m_VisualPrefab;
    [SerializeField] private Transform m_VisualsContainer;
    
    private bool m_IsActive = false;
    private bool m_IsLoading = false;
    private List<TrafficCameraPoint> m_ActiveMarkers = new List<TrafficCameraPoint>();
    private Stack<TrafficCameraPoint> m_MarkerPool = new Stack<TrafficCameraPoint>();
    
    public string ProviderID => "JAM_CAMS";
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsActive => m_IsActive;

    private void Start()
    {
        m_MapAreaCircle.gameObject.SetActive(false);
        m_MapCameraController.OnTargetPositionChanged += HandleTargetPositionChanged;
    }

    private void OnDestroy()
    {
        m_MapCameraController.OnTargetPositionChanged -= HandleTargetPositionChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void EnableLayer()
    {
        m_IsActive = true;
        StartCoroutine(FetchData());
        ShowMapAreaVisual(m_MapCameraController.TargetPivotPoint);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DisableLayer()
    {
        StopAllCoroutines();
        m_IsActive = false;
        m_IsLoading = false;
        HideMapAreaVisual();
    }

    private IEnumerator FetchData()
    {
        m_IsLoading = true;

        float lat = m_MapCameraController.TargetLatitude;
        float lon = m_MapCameraController.TargetLongitude;

        string requestUrl = $"{BASE_URL}&lat={lat}&lon={lon}&radius={m_SearchRadius}";

        Debug.Log(requestUrl);
        Debug.Log($"<color=cyan>[SYSTEM]</color> Requesting {ProviderID} at: {lat}, {lon}");

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.timeout = 10; 
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessJson(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ERROR] {ProviderID} Sync Failed: {request.error} | URL: {requestUrl}");
            }
        }

        m_IsLoading = false;
    }
    
    private void ProcessJson(string json)
    {
        try
        {
            // 1. Parse the root object
            JamCamResponse response = JsonUtility.FromJson<JamCamResponse>(json);

            if (response != null && response.places != null)
            {
                Debug.Log($"<color=green>[SUCCESS]</color> Found {response.places.Length} cameras.");

                foreach (JamCamPlace cam in response.places)
                {
                    // 2. Extract specific data using our helper
                    string imageUrl = cam.GetPropertyValue("imageUrl");
                    bool isAvailable = cam.GetPropertyValue("available") == "true";
                    string viewDescription = cam.GetPropertyValue("view");

                    Debug.Log($"Camera: {cam.commonName} | Available: {isAvailable} | Lat: {cam.lat}");
                    
                    Vector3 worldPos = m_MapPlane.LatLonToWorldPosition(cam.lat, cam.lon);
                    TrafficCameraPoint marker = GetCameraPoint();
                    marker.Setup(cam, worldPos);
                    m_ActiveMarkers.Add(marker);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON Parsing Error: {e.Message}");
        }
    }
    
    private TrafficCameraPoint GetCameraPoint()
    {
        if (m_MarkerPool.Count > 0)
        {
            return m_MarkerPool.Pop();
        }
        return Instantiate(m_VisualPrefab, m_VisualsContainer).GetComponent<TrafficCameraPoint>();
    }

    private void ClearMarkers()
    {
        foreach (TrafficCameraPoint marker in m_ActiveMarkers)
        {
            marker.Deactivate();
            m_MarkerPool.Push(marker);
        }
        m_ActiveMarkers.Clear();
    }

    private void HandleTargetPositionChanged(float lat, float lon, Vector3 targetPivotPos)
    {
        if (!m_IsActive)
        {
            return;
        }
        ShowMapAreaVisual(targetPivotPos);
    }

    private void ShowMapAreaVisual(Vector3 targetPivotPos)
    {
        m_MapAreaCircle.gameObject.SetActive(true);
        m_MapAreaCircle.transform.position = targetPivotPos;
        m_MapAreaCircle.SetAreaCircleScale(m_SearchRadius);
    }

    private void HideMapAreaVisual()
    {
        m_MapAreaCircle.gameObject.SetActive(false);
        ClearMarkers();
    }
}