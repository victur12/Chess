using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ChessPieceType
{ 
    none = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPositon;
    private Vector3 desiredScale;
}
