using UnityEngine;

public class TestMapVisualMesh : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    
    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Generate Ship Mesh")]
    public void GenerateShipMesh()
    {
        Mesh shipMesh = MapVisualMeshGenerator.CreateArrow();
        m_MeshFilter.mesh = shipMesh;
    }
    
    
}