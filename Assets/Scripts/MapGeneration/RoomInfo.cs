using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration
{
    public class RoomInfo : MonoBehaviour
    {
        static readonly List<RoomInfo> allRooms = new();
        public static IReadOnlyList<RoomInfo> AllRooms => allRooms;

        public RectInt Bounds { get; private set; }
        public BiomeDefinition Biome { get; private set; }
        public Vector2Int ChunkCoord { get; private set; }
        public Vector2Int Origin { get; private set; }
        public RoomDefinition Definition { get; private set; }

        bool[] exitUsed;
        bool[] exitSealed;

        public void Initialize(RectInt bounds, BiomeDefinition biome, Vector2Int chunkCoord, Vector2Int origin, RoomDefinition definition)
        {
            Bounds = bounds;
            Biome = biome;
            ChunkCoord = chunkCoord;
            Origin = origin;
            Definition = definition;

            int exitCount = definition != null && definition.exits != null ? definition.exits.Length : 0;
            exitUsed = new bool[exitCount];
            exitSealed = new bool[exitCount];
        }

        public bool IsExitUsed(int exitIndex) => exitUsed != null && exitUsed[exitIndex];

        public void MarkExitUsed(int exitIndex)
        {
            if (exitUsed != null)
                exitUsed[exitIndex] = true;
        }

        public bool IsExitSealed(int exitIndex) => exitSealed != null && exitSealed[exitIndex];

        public void MarkExitSealed(int exitIndex)
        {
            if (exitSealed != null)
                exitSealed[exitIndex] = true;
        }

        void Awake() => allRooms.Add(this);
        void OnDestroy() => allRooms.Remove(this);

        public static void ClearAll() => allRooms.Clear();

        public static List<RoomInfo> GetRoomsInChunk(Vector2Int chunkCoord)
        {
            List<RoomInfo> result = new();
            for (int i = 0; i < allRooms.Count; i++)
            {
                if (allRooms[i].ChunkCoord == chunkCoord)
                    result.Add(allRooms[i]);
            }
            return result;
        }

        public bool ContainsPosition(Vector2Int position) => Bounds.Contains(position);

        public bool ContainsWorldPosition(Vector3 worldPosition) => Bounds.Contains(Vector2Int.RoundToInt(worldPosition));
    }
}
