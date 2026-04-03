using UnityEngine;

/// <summary>
/// 
/// </summary>
public class MainUIManager : MonoBehaviour
{
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;
    [SerializeField] private MapCameraController m_CameraController;
    
    /// <summary>
    /// 
    /// </summary>
    public LondonGeoTiffPlane LondonMapPlane => m_LondonMapPlane;
    
    
    /// <summary>
    /// 
    /// </summary>
    public MapCameraController CameraController => m_CameraController;


}