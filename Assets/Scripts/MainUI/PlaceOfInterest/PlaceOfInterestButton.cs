using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class PlaceOfInterestButton : MonoBehaviour
{
    [SerializeField] private Button m_Button;

    [Header("Place of Interest Location")]
    [SerializeField] private float m_Latitude;
    [SerializeField] private float m_Longitude;
    
    /// <summary>
    /// 
    /// </summary>
    public event Action<float, float> OnButtonPressed;

    private void Start()
    {
        m_Button.onClick.AddListener(HandleButtonPressed);
    }

    private void OnDestroy()
    {
        m_Button.onClick.AddListener(HandleButtonPressed);
    }

    private void HandleButtonPressed()
    {
        OnButtonPressed?.Invoke(m_Latitude, m_Longitude);
    }
}