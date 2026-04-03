using UnityEngine;

/// <summary>
/// 
/// </summary>
public class LondonMapAreaCircle : MonoBehaviour
{
    [SerializeField] private LondonGeoTiffPlane m_LondonMapPlane;
    [SerializeField] private float m_ScaleToRadiusRatio = 30.5f;
    
    /// <summary>
    /// 
    /// </summary>
    public void SetAreaCircleScale(float areaRadius)
    {
        float scale = areaRadius / m_ScaleToRadiusRatio;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}