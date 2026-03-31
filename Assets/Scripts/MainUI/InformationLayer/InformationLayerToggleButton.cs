using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class InformationLayerToggleButton : MonoBehaviour
{
    [Header("Button Components")]
    [SerializeField] private Button m_OnButton;
    [SerializeField] private Button m_OffButton;
    
    [Header("Background Components")]
    [SerializeField] private Image m_OnBackground;
    [SerializeField] private Image m_OffBackground;
    
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI m_OnText;
    [SerializeField] private TextMeshProUGUI m_OffText;

    [Header("Visual Settings")]
    [SerializeField] private Color m_ActiveColor = new Color(0f, 1f, 1f); 
    [SerializeField] private Color m_InactiveColor = new Color(0.3f, 0.3f, 0.3f);
    [Range(0f, 1f)] [SerializeField] private float m_PressedAlpha = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float m_ReleasedAlpha = 0.2f;

    private bool m_IsOn = false;

    /// <summary>
    /// 
    /// </summary>
    public bool IsOn => m_IsOn;

    private void Start()
    {
        m_OnButton.onClick.AddListener(() => SetState(true));
        m_OffButton.onClick.AddListener(() => SetState(false));

        ApplyVisuals();
    }
    
    private void OnDestroy()
    {
        m_OnButton.onClick.RemoveAllListeners();
        m_OffButton.onClick.RemoveAllListeners();
    }

    private void SetState(bool newState)
    {
        if (m_IsOn == newState)
        {
            return;
        }

        m_IsOn = newState;

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        m_OnText.color = m_IsOn ? m_ActiveColor : m_InactiveColor;
        m_OffText.color = !m_IsOn ? m_ActiveColor : m_InactiveColor;

        SetImageAlpha(m_OnBackground, m_IsOn ? m_PressedAlpha : m_ReleasedAlpha);
        SetImageAlpha(m_OffBackground, !m_IsOn ? m_PressedAlpha : m_ReleasedAlpha);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
