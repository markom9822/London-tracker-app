using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TrafficCameraPoint : MonoBehaviour
{
    public string CameraID;
    public string ImageUrl;
    
    /// <summary>
    /// 
    /// </summary>
    public void Setup(TrafficCameraProvider.JamCamPlace data, Vector3 worldPosition)
    {
        CameraID = data.id;
        ImageUrl = data.GetPropertyValue("imageUrl");
        transform.position = worldPosition;
        gameObject.SetActive(true);
        
        // Update visuals (e.g., text label or icon color)
        this.name = $"Marker_{data.commonName}";
    }

    /// <summary>
    /// 
    /// </summary>
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}