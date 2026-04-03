using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TrafficCameraMarker : MapMarker
{
    public string CameraID { get; private set; }
    public string ImageUrl { get; private set; }

    public void Setup(TrafficCameraProvider.JamCamPlace data, Vector3 worldPosition)
    {
        CameraID = data.id;
        ImageUrl = data.GetPropertyValue("imageUrl");
        transform.position = worldPosition;
        this.name = $"Cam_{data.commonName}";
        gameObject.SetActive(true);
    }

    public override void OnSelect()
    {
        Debug.Log($"[MAP] Camera Selected: {CameraID}");
        // Trigger UI event, e.g., EventManager.OnCameraSelected?.Invoke(ImageUrl);
    }
}