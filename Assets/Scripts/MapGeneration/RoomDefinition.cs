using UnityEngine;

public class RoomDefinition : MonoBehaviour
{

    public MapLayer[] Layers;
    public RectInt spawnableArea;
    public RectInt bounds;
    public BiomeDefinition biome;
    public RoomExit[] exits;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawRectInt(bounds, transform.position);

        Gizmos.color = Color.green;
        DrawRectInt(spawnableArea, transform.position);

        if (exits == null) return;

        foreach (var exit in exits)
        {
            Vector3 worldPos = transform.position + new Vector3(exit.position.x, exit.position.y, 0);
            Vector3 exitSize = GetExitSize(exit);

            Gizmos.color = Color.yellow;
            Vector3 min = worldPos;
            Vector3 max = worldPos + new Vector3(exitSize.x, exitSize.y, 0);
            Vector3 bl = min, br = new Vector3(max.x, min.y, 0), tl = new Vector3(min.x, max.y, 0), tr = max;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);

            Gizmos.color = Color.red;
            Vector3 arrowDir = GetExitDirection(exit.exitDirection);
            Gizmos.DrawLine(worldPos + exitSize * 0.5f, worldPos + exitSize * 0.5f + arrowDir * 2f);
        }
    }

    Vector3 GetExitSize(RoomExit exit)
    {
        return exit.exitDirection switch
        {
            Direction.NORTH or Direction.SOUTH => new Vector3(exit.width, 1, 0),
            Direction.EAST or Direction.WEST => new Vector3(1, exit.width, 0),
            _ => Vector3.one
        };
    }

    Vector3 GetExitDirection(Direction dir)
    {
        return dir switch
        {
            Direction.NORTH => Vector3.up,
            Direction.SOUTH => Vector3.down,
            Direction.EAST => Vector3.right,
            Direction.WEST => Vector3.left,
            _ => Vector3.zero
        };
    }

    void DrawRectInt(RectInt r, Vector3 origin)
    {
        Vector3 min = origin + new Vector3(r.xMin, r.yMin, 0);
        Vector3 max = origin + new Vector3(r.xMax, r.yMax, 0);
        Vector3 size = max - min;
        Vector3 center = (min + max) * 0.5f;
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0.1f));
    }
}
