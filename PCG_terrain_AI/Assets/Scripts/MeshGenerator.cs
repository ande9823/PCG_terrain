using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    //Generattes a mesh for a terrain
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        //Gets top left point for vertices and uvs
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //Allows for changing the detail of the mesh
        int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        //Creates meshdata
        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;
        //Loops through width & height and sets the vertices and uvs
        for (int y = 0; y < height; y+= meshSimplificationIncrement) {
            for (int x = 0; x < width; x+= meshSimplificationIncrement) {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y])*heightMultiplier, topLeftZ - y); //vertices of mesh based of heightmap
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                //Add triangles for the meshdata
                if (x < width - 1 && y < height - 1) {
                    meshData.AddTriangles(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangles(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;
    
    //Constructor of the class
    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1)*6];
    }
    //Adds a triangle between 3 indexes
    public void AddTriangles(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }
    //Creates the mesh
    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
