using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 1.0f;
    [SerializeField] private float dragOffset = 1.0f;
    

    [Header("Prefabs & materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;


    //LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWrites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    //mi codigo para estaclecer la dismenciones de la figura
    private Vector3 piezescale;

    private void Awake() {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera) {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            //GET the indexes of the tile i've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);


            //If we're hovering a tile after not hovering any tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //if we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValiedMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //If we press down on the mouse
            if (Input.GetMouseButtonDown(0))
            {   
                if (chessPieces[hitPosition.x, hitPosition.y] != null)

                    // Is it our turn?
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];


                        //get a list of where i can go, highligt titles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    }
            }

            //if we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTi1eCenter(previousPosition.x, previousPosition.y));
                }
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValiedMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTi1eCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        // If we're dragging a piece
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentlyDragging.SetPosition(ray.GetPoint(distance) + new Vector3(0, 0, 0.5f));
        }
    }


    // Generate Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");

        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0; int blackTeam = 1;

        //White team
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        //White Queen
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        //
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        //White Knight
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        //White Rook
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);


        //Black team
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        //Black Queen
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        //Black Knight
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        //Black Rook
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);

    }
    
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece piece = Instantiate(prefabs[(int)type - 1]).GetComponent<ChessPiece>();

        piece.type = type;
        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = teamMaterials[team];


        return piece;
    }

    //Position 
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x,y,true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTi1eCenter(x, y), force);
    }

    private Vector3 GetTi1eCenter(int x, int y)
    {
        return new Vector3(x * tileSize, 0.7f, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
      }

    //Highlight Tiles
    private void HighlightTiles() 
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        availableMoves.Clear();
    }

    //operations
    private bool ContainsValiedMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValiedMove(ref availableMoves, new Vector2(x,y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //Is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            Vector3 escala;

            switch (ocp.name)
            {
                case "Queen":
                    escala = new Vector3(5, 5, 38);
                    break;
                case "King":
                    escala = new Vector3(5, 5, 38);
                    break;
                case "Bishop":
                    escala = new Vector3(5, 5, 38);
                    break;
                default:
                    escala = new Vector3(5, 5, 5);
                    break;
            }

            if (cp.team == ocp.team)
                return false;
            //if its the enemy team
            if (ocp.team == 0)
            {
                deadWrites.Add(ocp);
                ocp.SetScale(escala);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize/ 2)
                    + (Vector3.forward * deathSpacing) * deadWrites.Count);
            }
            else
            {
                deadBlacks.Add(ocp);
                ocp.SetScale(escala);
                ocp.SetPosition(
                    new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null ;

        PositionSinglePiece(x,y);

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo) 
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; 
    }
}
