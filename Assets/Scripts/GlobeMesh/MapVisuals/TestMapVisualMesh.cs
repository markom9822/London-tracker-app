using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class TestMapVisualMesh : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    
    [ContextMenu("Generate and Save Ship Mesh")]
    public void GenerateShipMesh()
    {
        // 1. Generate the mesh using your existing generator
        Mesh shipMesh = MapVisualMeshGenerator.CreateFlatTriangle();
        
        // 2. Assign it to the filter so you can see it immediately
        if (m_MeshFilter != null)
        {
            m_MeshFilter.mesh = shipMesh;
        }

#if UNITY_EDITOR
        // 3. Define the folder and file name
        string folderPath = "Assets/Art_Assets/MapVisuals";
        string fileName = "ShipFlatIcon.asset";
        string fullPath = Path.Combine(folderPath, fileName);

        // 4. Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 5. Save the mesh as a permanent asset
        // This makes it a real file in your project window
        AssetDatabase.CreateAsset(shipMesh, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully saved ship mesh to: {fullPath}");
#endif
    }
}