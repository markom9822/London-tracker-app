using TMPro;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class LatLongUIController : MonoBehaviour
{
    [SerializeField] private MapCameraController m_MapCameraController;
    [SerializeField] private TextMeshProUGUI m_LatLongText;

    private void Start()
    {
        m_MapCameraController.OnTargetPositionChanged += HandleOnTargetPositionChanged;
    }

    private void OnDestroy()
    {
        m_MapCameraController.OnTargetPositionChanged -= HandleOnTargetPositionChanged;
    }

    private void HandleOnTargetPositionChanged(float lat, float lon, Vector3 targetPos)
    {
        m_LatLongText.text = $"LAT: {lat} LON: {lon}";
    }
}