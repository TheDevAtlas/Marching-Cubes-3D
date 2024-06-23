using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScroll : MonoBehaviour
{
    public int cubeCount = 10; // Number of cubes along each axis
    public float cubeSize = 1f; // Size of each cube
    public float scrollSpeed = 1f; // Speed of Perlin noise scrolling
    public Material material; // Material to assign to cubes

    private GameObject[,,] cubes;
    private Vector3[,,] originalPositions;
    private float noiseOffset;

    void Start()
    {
        cubes = new GameObject[cubeCount, cubeCount, cubeCount];
        originalPositions = new Vector3[cubeCount, cubeCount, cubeCount];

        // Create cubes in a grid
        for (int x = 0; x < cubeCount; x++)
        {
            for (int y = 0; y < cubeCount; y++)
            {
                for (int z = 0; z < 1; z++)
                {
                    Vector3 position = new Vector3(
                        (x - cubeCount / 2) * cubeSize,
                        (y - cubeCount / 2) * cubeSize,
                        (z - 1 / 2) * cubeSize
                    );

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = position;
                    cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
                    cubes[x, y, z] = cube;
                    originalPositions[x, y, z] = position;

                    // Assign material and set initial alpha based on 3D Perlin noise
                    Material cubeMaterial = new Material(material);
                    float alpha = PerlinNoise3D(position.x, position.y, position.z);
                    Color color = cubeMaterial.color;
                    color.a = alpha;
                    cubeMaterial.color = color;
                    cube.GetComponent<Renderer>().material = cubeMaterial;
                }
            }
        }
    }

    void Update()
    {
        // Scroll Perlin noise over time
        noiseOffset += Time.deltaTime * scrollSpeed;

        // Update alpha of each cube based on scrolled Perlin noise
        for (int x = 0; x < cubeCount; x++)
        {
            for (int y = 0; y < cubeCount; y++)
            {
                for (int z = 0; z < 1; z++)
                {
                    Vector3 position = originalPositions[x, y, z];
                    Material cubeMaterial = cubes[x, y, z].GetComponent<Renderer>().material;
                    float alpha = PerlinNoise3D(position.x + noiseOffset, position.y + noiseOffset, position.z + noiseOffset);
                    Color color = cubeMaterial.color;
                    color.a = (alpha - 0.3f) * 2f - 0.25f;
                    cubeMaterial.color = color;
                }
            }
        }
    }

    float PerlinNoise3D(float x, float y, float z)
    {
        // Calculate 3D Perlin noise
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);

        float yx = Mathf.PerlinNoise(y, x); // Perlin noise is symmetric, so swap arguments

        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        // Calculate final noise value as average of all six faces
        float xyz = (xy + yz + xz + yx + zx + zy) / 6.0f;

        return xyz;
    }
}
