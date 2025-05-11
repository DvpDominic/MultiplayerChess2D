using System;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs && Sprites")] 
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Sprite[] whitePieces;
    [SerializeField] private Sprite[] blackPieces;
    [SerializeField] private Sprite[][] pieceSprites; // using jagged array for more flexibility, but slightly less performance, will improve later
    
    // Constraints
    private ChessPiece[,] chessPieces;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    
    private void Awake()
    {
        pieceSprites = new Sprite[2][];
        pieceSprites[0] = whitePieces;
        pieceSprites[1] = blackPieces;
        
        GenerateTiles(tileSize,8,8);

        SpawnAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        Vector2 mousePos = currentCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 100, LayerMask.GetMask("Tile","Hover"));
        if (hit.collider != null)
        {
            // Get indexes of the tile hit
            Vector2Int hitPosition = LookupTileIndex(hit.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }

    // Board Generation
    private void GenerateTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX/2) * tileSize,(tileCountX/2) * tileSize,0) + boardCenter;
        
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
        vertices[0] = new Vector3(x * tileSize, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, (y+1) * tileSize) - bounds;

        int[] tris = new int[]{ 0, 1, 2, 1, 3, 2 };
        
        mesh.vertices = vertices;
        mesh.triangles = tris;
        
        mesh.RecalculateNormals();
        
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider2D>();
        
        
        return tileObject;
    }
    
    // Piece Generation
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        
        int whiteTeam = 0, blackTeam = 1;
        
        // white team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6,0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7,0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for(int i = 0; i < TILE_COUNT_X; i++){
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }
        
        // black team
        
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        
        cp.type = type;
        cp.team = team;
        cp.GetComponent<SpriteRenderer>().sprite = pieceSprites[team][(int)type - 1];
        
        return cp;
    }
    
    // Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x,y);
            }
        }
        
        return -Vector2Int.one;
    }
}
