using UnityEngine;

/// <summary>
/// 
/// </summary>
public class InformationLayerOption : MonoBehaviour
{
    [SerializeField] private InformationLayerToggleButton m_InformationLayerToggleButton;
    [SerializeField] private MapLayerDataProvider m_MapLayerDataProvider;

    private void Start()
    {
        m_InformationLayerToggleButton.OnToggleButtonChanged += HandleToggleButtonChanged;
    }

    private void OnDestroy()
    {
        m_InformationLayerToggleButton.OnToggleButtonChanged -= HandleToggleButtonChanged;
    }
    
    private void HandleToggleButtonChanged(bool isOn)
    {
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