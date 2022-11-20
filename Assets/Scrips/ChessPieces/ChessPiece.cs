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
    private string name;

    private Vector3 desiredPositon;
    private Vector3 desiredScale;
    


    private void Start()
    {
        //este codigo no esta en el video lo implemte porque con las escalas
        //del video las piezas eran muy pequeñas
        name = type.ToString();
        switch (name)
        {
            case "Queen":
                desiredScale = new Vector3(10, 10, 75);
                break;
            case "King":
                desiredScale = new Vector3(10, 10, 75);
                break;
            case "Bishop":
                desiredScale = new Vector3(10, 10, 75);
                break;
            default:
                desiredScale = new Vector3(10, 10, 10);
                break;
        }
    }

    private void Update() 
    {
        
        
        transform.position = Vector3.Lerp(transform.position, desiredPositon, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10); 
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) 
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }

    public virtual void SetPosition(Vector3 position, bool force = false) 
    {
        desiredPositon = position;
        if (force)
            transform.position = desiredPositon;

    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }

    

}
