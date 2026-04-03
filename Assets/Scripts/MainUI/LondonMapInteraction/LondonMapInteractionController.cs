using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// </summary>
public class LondonMapInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private LayerMask m_MarkerLayer; 
    private void Update()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200, m_MarkerLayer))
        {
            
        }
    }
}