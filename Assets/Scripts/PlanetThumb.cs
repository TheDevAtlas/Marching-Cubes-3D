using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetThumb : MonoBehaviour
{
    public int chunkSize = 32;
    public float planetRadius = 10f;
    public float coreRadius = 2.5f;
    public float terrainSurface = 0.5f;  // Threshold for terrain surface generation
    public float noiseScale = 0.1f;  // Perlin noise scale for terrain generation
    public float dirtThreshold = 0.3f;  // Threshold for dirt material
    public float grassThreshold = 0.6f;  // Threshold for grass material

    [Header("Noise Settings")]
    public float dirtNoiseScale = 0.1f;
    public float grassNoiseScale = 0.15f;
    public Vector3 offset;

    List<Vector3> vertices = new List<Vector3>();
    List<int>[] submeshTriangles;
    float[,,] dataMap;
    int[,,] materialMap;

    MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        submeshTriangles = new List<int>[3]; // Create three lists for core, dirt, and grass
        for (int i = 0; i < submeshTriangles.Length; i++)
            submeshTriangles[i] = new List<int>();

        CreatePlanetData();
        GenerateMesh();
    }

    void CreatePlanetData()
    {
        dataMap = new float[chunkSize + 1, chunkSize + 1, chunkSize + 1];
        materialMap = new int[chunkSize + 1, chunkSize + 1, chunkSize + 1];
        Vector3 center = new Vector3((chunkSize + 1) / 2, (chunkSize + 1) / 2, (chunkSize + 1) / 2);

        for (int i = 0; i <= chunkSize; i++)
        {
            for (int j = 0; j <= chunkSize; j++)
            {
                for (int k = 0; k <= chunkSize; k++)
                {
                    Vector3 pos = new Vector3(i, j, k);
                    float distance = Vector3.Distance(pos, center);
                    float dirtNoise = Perlin3D(i * dirtNoiseScale + offset.x, j * dirtNoiseScale + offset.y, k * dirtNoiseScale + offset.z);
                    float grassNoise = Perlin3D(i * grassNoiseScale + offset.x, j * grassNoiseScale + offset.y, k * grassNoiseScale + offset.z);

                    if (distance < coreRadius)
                    {
                        materialMap[i, j, k] = 0; // Core
                        dataMap[i, j, k] = 0;  // Core layer with noise
                    }
                    else if (distance < planetRadius)
                    {
                        if (dirtNoise > dirtThreshold)
                        {
                            materialMap[i, j, k] = 2; // Dirt
                            dataMap[i, j, k] = 0; // General terrain
                        }   
                        else if (grassNoise > grassThreshold)
                        {
                            materialMap[i, j, k] = 3; // Grass
                            dataMap[i, j, k] = 0; // General terrain
                        }
                        else
                        {
                            materialMap[i, j, k] = 1; // Rock
                            dataMap[i, j, k] = 1; // General terrain
                        }
                            

                        //dataMap[i, j, k] = 0; // General terrain
                    }
                    else
                    {
                        materialMap[i, j, k] = 1; // Outside material
                        dataMap[i, j, k] = 1; // Outside the planet
                    }
                }
            }
        }
    }

    void GenerateMesh()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float[] cube = new float[8];
                    int[] materialCube = new int[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + March.CornerTable[i];
                        cube[i] = dataMap[corner.x, corner.y, corner.z];
                        materialCube[i] = materialMap[corner.x, corner.y, corner.z];
                    }

                    MarchCube(new Vector3(x, y, z) - new Vector3(chunkSize, chunkSize, chunkSize) / 2f, cube, materialCube);
                }
            }
        }

        BuildMesh();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 3;

        for (int i = 0; i < 3; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i);
        }

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }


    void MarchCube(Vector3 position, float[] cube, int[] materialCube)
    {
        int configIndex = GetCubeConfiguration(cube);
        if (configIndex == 0 || configIndex == 255) return;
        int submeshIndex = DetermineSubmeshIndex(materialCube);
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = March.TriangleTable[configIndex, edgeIndex];
                if (indice == -1) return;

                Vector3 vert1 = position + March.EdgeTable[indice, 0];
                Vector3 vert2 = position + March.EdgeTable[indice, 1];
                Vector3 vertPosition = (vert1 + vert2) / 2f;
                vertices.Add(vertPosition);

                //int submeshIndex = DetermineSubmeshIndex(materialCube);
                submeshTriangles[submeshIndex].Add(vertices.Count - 1);
                edgeIndex++;
            }
        }
    }

    int GetCubeConfiguration(float[] cube)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > terrainSurface)
                configurationIndex |= 1 << i;
        }
        return configurationIndex;
    }

    // Determines the submesh index based on material cube values
    int DetermineSubmeshIndex(int[] materialCube)
    {
        bool hasDirt = false;
        bool hasGrass = false;

        for (int i = 0; i < materialCube.Length; i++)
        {
            if (materialCube[i] == 2) hasDirt = true;
            if (materialCube[i] == 3) hasGrass = true;
        }

        if (hasGrass) return 2; // Grass
        if (hasDirt) return 1; // Dirt
        return 0; // Core
    }
    float Perlin3D(float x, float y, float z)
    {
        // Using the y value for vertical slices and a combination of x, z for horizontal slices
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        // Using the z coordinate to blend the above noise patterns into a 3D noise
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        // Average the values to smooth out the distribution
        float result = (AB + BC + AC + BA + CB + CA) / 6f;
        return result;
    }
}