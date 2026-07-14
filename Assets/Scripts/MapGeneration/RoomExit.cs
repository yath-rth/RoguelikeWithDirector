using UnityEngine;

public enum Direction
{
    NORTH, SOUTH, EAST, WEST
}
[System.Serializable]
public struct RoomExit
{
    public Direction exitDirection;
    public int width;
    public Vector2Int position;
}