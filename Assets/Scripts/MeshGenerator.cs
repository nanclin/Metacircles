using UnityEngine;
using System.Collections;

public class MeshGenerator : MonoBehaviour {
    
    [SerializeField] private MeshFilter MeshFilter = null;

    private Vector3[] Vertices;
    private int[] Triangles;

    void Start() {
        int height = 10;
        int width = 10;
        int triangleIndex = 0;

        Vertices = new Vector3[width * height];
        Triangles = new int[(width - 1) * 6 * (height - 1)];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                
                int vertexIndex = y * width + x;

                Vertices[vertexIndex] = new Vector3(x, Mathf.Sin(x * 0.2f * Mathf.PI), y) + Vector3.one * 0.5f;

                int verticesPerLine = width;

                if (x < width - 1 && y < height - 1) {
                    Triangles[triangleIndex + 0] = vertexIndex + 0;
                    Triangles[triangleIndex + 1] = vertexIndex + verticesPerLine + 1;
                    Triangles[triangleIndex + 2] = vertexIndex + 1;
                    
                    Triangles[triangleIndex + 3] = vertexIndex + 0;
                    Triangles[triangleIndex + 4] = vertexIndex + verticesPerLine;
                    Triangles[triangleIndex + 5] = vertexIndex + verticesPerLine + 1;

                    triangleIndex += 6;
                }
            }
        }
        MeshFilter.mesh.vertices = Vertices;
        MeshFilter.mesh.triangles = Triangles;
        MeshFilter.mesh.RecalculateNormals();
    }
}
