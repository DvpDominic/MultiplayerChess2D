using System;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Material tileMaterial;
    
    // Constraints
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    
    private void Awake()
    {
        GenerateTiles(1,8,8);
    }

    private void GenerateTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0},Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, (y+1) * tileSize);

        int[] tris = new int[]{ 0, 1, 2, 1, 3, 2 };
        
        mesh.vertices = vertices;
        mesh.triangles = tris;

        tileObject.AddComponent<BoxCollider2D>();
        
        
        return tileObject;
    }
}
