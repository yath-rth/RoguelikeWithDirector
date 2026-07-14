using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewMapConfig", menuName = "MapGeneration/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Rooms")]
    public int roomPadding = 1;
    public int minRoomsPerChunk = 1;
    public int maxRoomsPerChunk = 3;
    public int maxPlacementAttempts = 50;

    [Header("Corridors")]
    public int minCorridorLength = 5;
    public int maxCorridorLength = 15;
    public TileBase corridorFloorTile;
    public TileBase corridorWallTile;

    [Header("Room Templates")]
    public GameObject[] roomPrefabs;

    void OnValidate()
    {
        if (minRoomsPerChunk < 0) minRoomsPerChunk = 0;
        if (maxRoomsPerChunk < minRoomsPerChunk)
        {
            Debug.LogWarning("[MapConfig] maxRoomsPerChunk must be >= minRoomsPerChunk.");
            maxRoomsPerChunk = minRoomsPerChunk;
        }
        if (maxPlacementAttempts <= 0)
        {
            Debug.LogWarning("[MapConfig] maxPlacementAttempts must be > 0, clamping to 1.");
            maxPlacementAttempts = 1;
        }
        if (roomPadding < 0) roomPadding = 0;
    }
}
