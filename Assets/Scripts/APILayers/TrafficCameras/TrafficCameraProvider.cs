using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 
/// </summary>
public class TrafficCameraProvider : MapLayerDataProvider
{
    [SerializeField] private MapCameraController m_MapCameraController;
    [SerializeField] private float m_SearchRadius = 5f;
    
    private const string BASE_URL = "https://api.tfl.gov.uk/Place/Type/JamCam/";
    
    private bool m_IsActive = false;
    private bool m_IsLoading = false;
    
    public string ProviderID => "JAM_CAMS";
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsActive => m_IsActive;

    /// <summary>
    /// 
    /// </summary>
    public override void EnableLayer()
    {
        m_IsActive = true;
        StartCoroutine(FetchData());
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DisableLayer()
    {
        StopAllCoroutines();
        m_IsActive = false;
        m_IsLoading = false;
        ClearMapIcons();
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

    private void ClearMapIcons() { /* Logic to remove sprites from UI */ }
}