using UnityEngine;

public static class MapVisualMeshGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public static Mesh CreateArrow(float stemLength = 1.0f, float stemRadius = 0.1f, float headLength = 0.4f, float headRadius = 0.25f)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralArrow";

        // Shaft (Cylinder) and Head (Cone) vertices
        // For an extremely simple "flat" arrow (2D) for map icons:
        Vector3[] vertices = new Vector3[]
        {
            // The "Tail" of the arrow (Stem)
            new Vector3(-stemRadius, 0, 0),
            new Vector3(stemRadius, 0, 0),
            new Vector3(-stemRadius, stemLength, 0),
            new Vector3(stemRadius, stemLength, 0),

            // The "Head" (Triangle)
            new Vector3(-headRadius, stemLength, 0),
            new Vector3(headRadius, stemLength, 0),
            new Vector3(0, stemLength + headLength, 0)
        };

        int[] triangles = new int[]
        {
            0, 2, 1, 1, 2, 3, // Stem
            4, 6, 5           // Head
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}