using UnityEngine;

/// <summary>
/// 
/// </summary>
public class InformationLayerOption : MonoBehaviour
{
    [SerializeField] private InformationLayerToggleButton m_InformationLayerToggleButton;
    [SerializeField] private GameObject m_ButtonIndicator;
    [SerializeField] private MapLayerDataProvider m_MapLayerDataProvider;

    private void Start()
    {
        m_ButtonIndicator.SetActive(false);
        m_InformationLayerToggleButton.OnToggleButtonChanged += HandleToggleButtonChanged;
    }

    private void OnDestroy()
    {
        m_InformationLayerToggleButton.OnToggleButtonChanged -= HandleToggleButtonChanged;
    }
    
    private void HandleToggleButtonChanged(bool isOn)
    {
        m_ButtonIndicator.SetActive(isOn);
        if (isOn)
        {
            m_MapLayerDataProvider.EnableLayer();
        }
        else
        {
            m_MapLayerDataProvider.DisableLayer();
        }
        
    }
}