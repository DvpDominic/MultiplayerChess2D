using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.25f;
    [SerializeField] private float deathSpacing = 0.5f;

    [Header("Prefabs && Sprites")] 
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Sprite[] whitePieces;
    [SerializeField] private Sprite[] blackPieces;
    [SerializeField] private Sprite[][] pieceSprites; // using jagged array for more flexibility, but slightly less performance, will improve later
    
    // Constraints
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
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
        PositionAllPieces();
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
            
            // If mouse button is pressed down
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                    }
                }
            }

            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPostion = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPostion.x, previousPostion.y),false);
                    currentlyDragging = null;
                }
                else
                {
                    currentlyDragging = null;
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                
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
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for(int i = 0; i < TILE_COUNT_X; i++){
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }
        
        // black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for(int i = 0; i < TILE_COUNT_X; i++){
            chessPieces[i,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        
        cp.type = type;
        cp.team = team;
        cp.GetComponent<SpriteRenderer>().sprite = pieceSprites[team][(int)type - 1];
        
        return cp;
    }
    
    // Positioning pieces
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(chessPieces[x,y] != null)
                    PositionSinglePiece(x, y, true);
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, y * tileSize, yOffset) - bounds + new Vector3(tileSize/2, tileSize/2, 0);
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
    
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPostion = new Vector2Int(cp.currentX, cp.currentY);
        
        // Checking if there is another piece at target pos
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            
            // if same team
            if (cp.team == ocp.team)
            {
                return false;
            }
            
            // If enemy team
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, -1 * tileSize, yOffset) - bounds + new Vector3(tileSize/2, tileSize/2, 0) + (Vector3.up * (deathSpacing * deadWhites.Count)));
            }
            else
            {
                
            }
            
        }
        
        chessPieces[x, y] = cp;
        chessPieces[previousPostion.x, previousPostion.y] = null;
        PositionSinglePiece(x, y);
        
        return true;
    }
}
