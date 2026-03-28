using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class BottomPanelController : MonoBehaviour
{
    [SerializeField] private MainUIManager m_MainUIManager;
    [SerializeField] private List<PlaceOfInterestButton> m_PlaceOfInterestButtons;


    private void Start()
    {
        foreach (PlaceOfInterestButton placeOfInterestButton in m_PlaceOfInterestButtons)
        {
            placeOfInterestButton.OnButtonPressed += HandlePlaceOfInterestButtonPressed;
        }
    }

    private void OnDestroy()
    {
        foreach (PlaceOfInterestButton placeOfInterestButton in m_PlaceOfInterestButtons)
        {
            placeOfInterestButton.OnButtonPressed += HandlePlaceOfInterestButtonPressed;
        }
    }

    private void HandlePlaceOfInterestButtonPressed(float lat, float lon)
    {
        m_MainUIManager.CameraController.FlyToLatLon(lat, lon);
    }
}