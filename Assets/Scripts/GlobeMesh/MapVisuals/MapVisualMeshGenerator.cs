using UnityEngine;

public static class MapVisualMeshGenerator
{
    public static Mesh CreateFlatTriangle(float width = 0.5f, float height = 0.8f)
    {
        Mesh mesh = new Mesh();
        mesh.name = "FlatShipIcon";

        float halfWidth = width * 0.5f;

        // Built on the XZ plane (Y is 0)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, 0, 0),      // Bottom Left
            new Vector3(halfWidth, 0, 0),       // Bottom Right
            new Vector3(0, 0, height)           // Tip (Pointing Forward)
        };

        int[] triangles = new int[] { 0, 2, 1 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}