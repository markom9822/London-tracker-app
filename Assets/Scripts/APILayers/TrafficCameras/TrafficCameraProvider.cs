using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 
/// </summary>
public class TrafficCameraProvider : MapLayerDataProvider
{
    [SerializeField] private MapCameraController m_MapCameraController;
    [SerializeField] private LondonMapAreaCircle m_MapAreaCircle;
    [SerializeField, Tooltip("Search Radius in meters from the target")] private float m_SearchRadius = 500f;
    
    private const string BASE_URL = "https://api.tfl.gov.uk/Place/Type/JamCam/";
    
    private bool m_IsActive = false;
    private bool m_IsLoading = false;
    
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

        string requestUrl = $"{BASE_URL}?lat={lat}&lon={lon}&radius={m_SearchRadius}";

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
        Debug.Log($"<color=green>[SUCCESS]</color> {ProviderID} Data Received.");
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
    }
}