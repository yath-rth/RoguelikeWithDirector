using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



namespace MapGeneration
{
    


    
    public class MapGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] MapConfig config;
        [SerializeField] Transform roomParent;
        [SerializeField] int seed = 42;
        [SerializeField] bool useRandomSeed = true;

        [Header("World Tilemaps")]
        [SerializeField] Tilemap groundTilemap;
        [SerializeField] Tilemap wallsTilemap;
        [SerializeField] Tilemap collisionDecorTilemap;
        [SerializeField] Tilemap nonCollisionDecorTilemap;

        [Header("Corridor Tilemaps")]
        [SerializeField] Tilemap corridorGroundTilemap;
        [SerializeField] Tilemap corridorWallTilemap;

        [Header("Streaming")]
        [SerializeField] Transform playerTransform;
        [SerializeField] int chunkSize = 40;
        [SerializeField] int generationRadius = 2;

        readonly HashSet<Vector2Int> generatedChunks = new();
        readonly HashSet<Vector2Int> corridorFloorTiles = new();
        readonly HashSet<ulong> connectedChunkPairs = new();
        int activeSeed;
        
        static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public int ActiveSeed => activeSeed;

        void Start()
        {
            InitializeSeed();

            if (playerTransform == null)
                GenerateChunk(Vector2Int.zero);
        }

        void Update()
        {
            if (playerTransform != null)
                StreamChunks();
        }

        void InitializeSeed()
        {
            activeSeed = useRandomSeed ? Random.Range(0, int.MaxValue) : seed;
            Debug.Log($"[MapGenerator] Seed: {activeSeed}");
        }

        void StreamChunks()
        {
            Vector2Int playerChunk = WorldToChunk(playerTransform.position);

            for (int x = -generationRadius; x <= generationRadius; x++)
            {
                for (int y = -generationRadius; y <= generationRadius; y++)
                {
                    Vector2Int chunk = playerChunk + new Vector2Int(x, y);
                    if (!generatedChunks.Contains(chunk))
                        GenerateChunk(chunk);
                }
            }
        }

        Vector2Int WorldToChunk(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / chunkSize),
                Mathf.FloorToInt(worldPos.y / chunkSize)
            );
        }

        System.Random ChunkRng(Vector2Int chunkCoord)
        {
            unchecked
            {
                int hash = activeSeed;
                hash = hash * 31 + chunkCoord.x;
                hash = hash * 31 + chunkCoord.y;
                return new System.Random(hash);
            }
        }

        void GenerateChunk(Vector2Int chunkCoord)
        {
            if (!generatedChunks.Add(chunkCoord))
                return;

            if (config == null)
                return;

            if (config.roomPrefabs == null || config.roomPrefabs.Length == 0)
            {
                Debug.LogWarning("[MapGenerator] No room prefabs assigned.");
                return;
            }

            System.Random rng = ChunkRng(chunkCoord);
            Vector2Int chunkOrigin = chunkCoord * chunkSize;

            PlaceFirstRoomInChunk(rng, chunkCoord, chunkOrigin);

            List<RoomInfo> rooms = RoomInfo.GetRoomsInChunk(chunkCoord);

            if (rooms.Count == 0)
                return;

            int targetRooms = Mathf.Clamp(
                rng.Next(config.minRoomsPerChunk, config.maxRoomsPerChunk + 1),
                1,
                Mathf.Max(1, config.maxRoomsPerChunk));

            targetRooms = Mathf.Max(targetRooms, rooms.Count);

            int attempts = 0;
            int failuresInRow = 0;
            int maxAttempts = Mathf.Max(
                config.maxPlacementAttempts,
                targetRooms * config.maxPlacementAttempts);

            while (attempts < maxAttempts)
            {
                attempts++;

                rooms = RoomInfo.GetRoomsInChunk(chunkCoord);

                if (rooms.Count >= targetRooms)
                    break;

                bool placed = TryGrowCorridor(rng, chunkCoord);

                if (placed)
                {
                    failuresInRow = 0;
                    continue;
                }

                failuresInRow++;

                if (failuresInRow >= config.maxPlacementAttempts)
                    break;
            }

            ConnectToAdjacentChunks(chunkCoord);

            SealResolvedExits(chunkCoord);
        }

        void PlaceFirstRoomInChunk(System.Random chunkRng, Vector2Int chunkCoord, Vector2Int chunkOrigin)
        {
            if (config.roomPrefabs == null || config.roomPrefabs.Length == 0) return;

            for (int attempt = 0; attempt < config.maxPlacementAttempts; attempt++)
            {
                int prefabIndex = chunkRng.Next(0, config.roomPrefabs.Length);
                GameObject prefab = config.roomPrefabs[prefabIndex];
                RoomDefinition roomDef = prefab.GetComponent<RoomDefinition>();
                if (roomDef == null) continue;

                Vector2Int pos = new Vector2Int(
                    chunkOrigin.x + chunkRng.Next(0, chunkSize),
                    chunkOrigin.y + chunkRng.Next(0, chunkSize)
                );

                if (!OverlapsAnyRooms(WorldBounds(roomDef, pos)))
                {
                    PlaceRoom(prefab, roomDef, pos, chunkCoord);
                    return;
                }
            }
        }

        bool TryGrowCorridor(System.Random chunkRng, Vector2Int chunkCoord)
        {
            List<RoomInfo> rooms = RoomInfo.GetRoomsInChunk(chunkCoord);
            if (rooms.Count == 0)
                return false;

            List<RoomInfo> candidates = new List<RoomInfo>(rooms);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = chunkRng.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            foreach (RoomInfo room in candidates)
            {
                if (room == null || room.Definition == null)
                    continue;

                RoomExit[] exits = room.Definition.exits;
                if (exits == null || exits.Length == 0)
                    continue;

                List<int> shuffledExitIndices = new List<int>(exits.Length);
                for (int i = 0; i < exits.Length; i++)
                    shuffledExitIndices.Add(i);

                for (int i = shuffledExitIndices.Count - 1; i > 0; i--)
                {
                    int j = chunkRng.Next(i + 1);
                    (shuffledExitIndices[i], shuffledExitIndices[j]) = (shuffledExitIndices[j], shuffledExitIndices[i]);
                }

                foreach (int exitIndex in shuffledExitIndices)
                {
                    if (room.IsExitUsed(exitIndex))
                        continue;

                    RoomExit exit = exits[exitIndex];

                    if (!CanGrowFromExit(room, exit))
                        continue;

                    Vector2Int dir = DirectionToVector(exit.exitDirection);

                    Vector2Int start = FindExteriorTile(room, exit);

                    int desiredLength = chunkRng.Next(
                        config.minCorridorLength,
                        config.maxCorridorLength + 1);

                    if (TryCarveCorridor(
                            chunkRng,
                            chunkCoord,
                            start,
                            dir,
                            desiredLength))
                    {
                        room.MarkExitUsed(exitIndex);
                        return true;
                    }
                }
            }

            return false;
        }
        
        bool CanGrowFromExit(RoomInfo room, RoomExit exit)
        {
            Vector2Int outside = FindExteriorTile(room, exit);

            if (IsInRoom(outside))
                return false;

            if (corridorFloorTiles.Contains(outside))
                return false;

            return true;
        }

        bool TryCarveCorridor(
            System.Random chunkRng,
            Vector2Int chunkCoord,
            Vector2Int start,
            Vector2Int dirVec,
            int targetLength)
        {
            List<Vector2Int> corridor = new(targetLength);

            Vector2Int current = start;
            bool connectedToExisting = false;

            for (int i = 0; i < targetLength; i++)
            {
                if (IsInRoom(current))
                    return false;

                if (corridorFloorTiles.Contains(current))
                {
                    connectedToExisting = true;
                    break;
                }

                corridor.Add(current);
                current += dirVec;
            }

            if (corridor.Count == 0)
                return false;

            Vector2Int attachmentTile = corridor[^1];

            bool roomPlaced =
                TryPlaceRoomAtEnd(
                    chunkRng,
                    chunkCoord,
                    attachmentTile,
                    VectorToDirection(dirVec));

            if (!roomPlaced && !connectedToExisting)
                return false;

            foreach (Vector2Int tile in corridor)
            {
                if (corridorFloorTiles.Add(tile))
                {
                    PlaceCorridorGround(tile);
                    PlaceCorridorWalls(tile);
                }
            }

            return true;
        }
        
        static Direction VectorToDirection(Vector2Int v)
        {
            if (v == Vector2Int.up)
                return Direction.NORTH;

            if (v == Vector2Int.down)
                return Direction.SOUTH;

            if (v == Vector2Int.left)
                return Direction.WEST;

            if (v == Vector2Int.right)
                return Direction.EAST;

            return Direction.NORTH;
        }

        bool TryPlaceRoomAtEnd(
            System.Random chunkRng,
            Vector2Int chunkCoord,
            Vector2Int corridorEnd,
            Direction corridorDirection)
        {
            if (config.roomPrefabs == null || config.roomPrefabs.Length == 0)
                return false;

            Direction requiredExit = OppositeDirection(corridorDirection);
            Vector2Int dir = DirectionToVector(corridorDirection);

            List<GameObject> prefabs = new(config.roomPrefabs);

            for (int i = prefabs.Count - 1; i > 0; i--)
            {
                int j = chunkRng.Next(i + 1);
                (prefabs[i], prefabs[j]) = (prefabs[j], prefabs[i]);
            }

            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null)
                    continue;

                RoomDefinition roomDef = prefab.GetComponent<RoomDefinition>();

                if (roomDef == null || roomDef.exits == null)
                    continue;

                for (int exitIndex = 0; exitIndex < roomDef.exits.Length; exitIndex++)
                {
                    RoomExit exit = roomDef.exits[exitIndex];

                    if (exit.exitDirection != requiredExit)
                        continue;

                    Vector2Int exteriorTile = corridorEnd;

                    Vector2Int roomOrigin =
                        CalculateRoomOrigin(
                            roomDef,
                            exit,
                            exteriorTile);

                    RectInt bounds = WorldBounds(roomDef, roomOrigin);

                    if (OverlapsAnyRooms(bounds))
                        continue;

                    if (RoomIntersectsCorridor(bounds))
                        continue;

                    Vector2Int worldDoor =
                        roomOrigin + exit.position;

                    if (worldDoor - dir != corridorEnd)
                        continue;

                    RoomInfo newRoom = PlaceRoom(
                        prefab,
                        roomDef,
                        roomOrigin,
                        chunkCoord);

                    newRoom?.MarkExitUsed(exitIndex);

                    return true;
                }
            }

            return false;
        }

        Vector2Int CalculateRoomOrigin(
            RoomDefinition room,
            RoomExit exit,
            Vector2Int exteriorTile)
        {
            Vector2Int dir = DirectionToVector(OppositeDirection(exit.exitDirection));

            return exteriorTile + dir - exit.position;
        }

        
        Vector2Int FindExteriorTile(RoomInfo room, RoomExit exit)
        {
            Vector2Int dir = DirectionToVector(exit.exitDirection);

            Vector2Int local = exit.position;

            int maxSteps = Mathf.Max(room.Bounds.width, room.Bounds.height) + 8;

            for (int i = 0; i < maxSteps; i++)
            {
                local += dir;

                if (!HasRoomTile(room.Definition, local))
                    return room.Origin + local;
            }

            Debug.LogError(
                $"Failed to find exterior tile for room '{room.Definition.name}'.");

            return room.Origin + exit.position + dir;
        }
        
        bool HasRoomTile(RoomDefinition room, Vector2Int localPos)
        {
            if (room == null || room.Layers == null)
                return false;

            Vector3Int cell = new(localPos.x, localPos.y, 0);

            foreach (MapLayer layer in room.Layers)
            {
                if (layer.definition == null)
                    continue;

                if (layer.name != "Ground" &&
                    layer.name != "Walls" &&
                    layer.name != "CollisionDecor")
                    continue;

                if (layer.definition.GetTile(cell) != null)
                    return true;
            }

            return false;
        }
        
        bool RoomIntersectsCorridor(RectInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    if (corridorFloorTiles.Contains(new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }
        
        bool IsInRoom(Vector2Int position)
        {
            for (int i = 0; i < RoomInfo.AllRooms.Count; i++)
            {
                if (RoomInfo.AllRooms[i].ContainsPosition(position))
                    return true;
            }
            return false;
        }

        bool OverlapsAnyRooms(RectInt bounds)
        {
            RectInt padded = new RectInt(
                bounds.x - config.roomPadding,
                bounds.y - config.roomPadding,
                bounds.width + config.roomPadding * 2,
                bounds.height + config.roomPadding * 2
            );

            for (int i = 0; i < RoomInfo.AllRooms.Count; i++)
            {
                if (padded.Overlaps(RoomInfo.AllRooms[i].Bounds))
                    return true;
            }
            return false;
        }

        static Vector2Int DirectionToVector(Direction dir)
        {
            return dir switch
            {
                Direction.NORTH => Vector2Int.up,
                Direction.SOUTH => Vector2Int.down,
                Direction.EAST => Vector2Int.right,
                Direction.WEST => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        static Direction OppositeDirection(Direction dir)
        {
            return dir switch
            {
                Direction.NORTH => Direction.SOUTH,
                Direction.SOUTH => Direction.NORTH,
                Direction.EAST => Direction.WEST,
                Direction.WEST => Direction.EAST,
                _ => dir
            };
        }

        RoomInfo PlaceRoom(
            GameObject prefab,
            RoomDefinition definition,
            Vector2Int position,
            Vector2Int chunkCoord)
        {
            if (definition == null)
                return null;

            if (definition.Layers != null)
            {
                foreach (MapLayer layer in definition.Layers)
                {
                    if (layer.definition == null)
                        continue;

                    Tilemap destination = FindWorldTilemap(layer.name);

                    if (destination == null)
                        continue;

                    CopyTilemap(
                        layer.definition,
                        destination,
                        position);
                }
            }
            
            Tilemap FindWorldTilemap(string layerName)
            {
                if (string.IsNullOrWhiteSpace(layerName))
                    return null;

                switch (layerName.Trim().ToLowerInvariant())
                {
                    case "ground":
                        return groundTilemap;

                    case "walls":
                        return wallsTilemap;

                    case "collisiondecor":
                        return collisionDecorTilemap;

                    case "noncollisiondecor":
                        return nonCollisionDecorTilemap;

                    default:
                        Debug.LogWarning($"Unknown map layer '{layerName}'.");
                        return null;
                }
            }

            RectInt bounds = WorldBounds(definition, position);

            RemoveCorridorTiles(bounds);

            ClearCorridorWallsInBounds(bounds);

            GameObject roomObject = new GameObject($"Room_{RoomInfo.AllRooms.Count}");

            if (roomParent != null)
                roomObject.transform.SetParent(roomParent, false);

            roomObject.transform.position =
                new Vector3(position.x, position.y, 0);

            RoomInfo info = roomObject.AddComponent<RoomInfo>();

            info.Initialize(
                bounds,
                definition.biome,
                chunkCoord,
                position,
                definition);

            return info;
        }
        
        void RemoveCorridorTiles(RectInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector2Int p = new(x, y);

                    corridorFloorTiles.Remove(p);

                    corridorGroundTilemap.SetTile(
                        new Vector3Int(x, y, 0),
                        null);
                }
            }

            RectInt expanded = new RectInt(
                bounds.xMin - 1,
                bounds.yMin - 1,
                bounds.width + 2,
                bounds.height + 2);

            for (int x = expanded.xMin; x < expanded.xMax; x++)
            {
                for (int y = expanded.yMin; y < expanded.yMax; y++)
                {
                    RefreshWallTile(new Vector2Int(x, y));
                }
            }
        }

        void CopyTilemap(
            Tilemap source,
            Tilemap destination,
            Vector2Int offset)
        {
            if (source == null || destination == null)
                return;

            source.CompressBounds();

            BoundsInt bounds = source.cellBounds;

            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                TileBase tile = source.GetTile(cell);

                if (tile == null)
                    continue;

                destination.SetTile(
                    cell + new Vector3Int(offset.x, offset.y, 0),
                    tile);
            }
        }

        static RectInt WorldBounds(RoomDefinition def, Vector2Int origin)
        {
            return new RectInt(
                origin.x + def.bounds.x,
                origin.y + def.bounds.y,
                def.bounds.width,
                def.bounds.height
            );
        }

        void PlaceCorridorGround(Vector2Int pos)
        {
            Vector3Int p = new Vector3Int(pos.x, pos.y, 0);
            if (config.corridorFloorTile != null)
                corridorGroundTilemap.SetTile(p, config.corridorFloorTile);
            corridorWallTilemap.SetTile(p, null);
        }

        void PlaceCorridorWalls(Vector2Int corridorTile)
        {
            if (config.corridorWallTile == null)
                return;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    RefreshWallTile(corridorTile + new Vector2Int(x, y));
                }
            }
        }
        
        void RefreshWallTile(Vector2Int tile)
        {
            Vector3Int cell = new(tile.x, tile.y, 0);

            if (groundTilemap.GetTile(cell) != null)
            {
                corridorWallTilemap.SetTile(cell, null);
                return;
            }

            if (wallsTilemap.GetTile(cell) != null)
            {
                corridorWallTilemap.SetTile(cell, null);
                return;
            }

            if (corridorFloorTiles.Contains(tile))
            {
                corridorWallTilemap.SetTile(cell, null);
                return;
            }

            bool adjacentToCorridor = false;

            foreach (Vector2Int dir in CardinalDirections)
            {
                if (corridorFloorTiles.Contains(tile + dir))
                {
                    adjacentToCorridor = true;
                    break;
                }
            }

            if (!adjacentToCorridor)
            {
                corridorWallTilemap.SetTile(cell, null);
                return;
            }

            corridorWallTilemap.SetTile(
                cell,
                config.corridorWallTile);
        }

        void ClearCorridorWallsInBounds(RectInt bounds)
        {
            RectInt expanded = new RectInt(
                bounds.xMin - 1,
                bounds.yMin - 1,
                bounds.width + 2,
                bounds.height + 2);

            for (int x = expanded.xMin; x < expanded.xMax; x++)
            {
                for (int y = expanded.yMin; y < expanded.yMax; y++)
                {
                    RefreshWallTile(new Vector2Int(x, y));
                }
            }
        }


        static ulong ChunkConnectionKey(Vector2Int a, Vector2Int b)
        {
            if (a.x > b.x || (a.x == b.x && a.y > b.y))
                (a, b) = (b, a);

            unchecked
            {
                ulong ax = (ushort)a.x;
                ulong ay = (ushort)a.y;
                ulong bx = (ushort)b.x;
                ulong by = (ushort)b.y;

                return
                    ax |
                    (ay << 16) |
                    (bx << 32) |
                    (by << 48);
            }
        }

        bool MarkChunkConnection(Vector2Int a, Vector2Int b)
        {
            return connectedChunkPairs.Add(
                ChunkConnectionKey(a, b));
        }
        
        void ConnectToAdjacentChunks(Vector2Int chunkCoord)
        {
            Vector2Int[] neighbours =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            Direction[] directions =
            {
                Direction.NORTH,
                Direction.SOUTH,
                Direction.WEST,
                Direction.EAST
            };

            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector2Int adjacentChunk = chunkCoord + neighbours[i];

                if (!generatedChunks.Contains(adjacentChunk))
                    continue;

                if (!MarkChunkConnection(chunkCoord, adjacentChunk))
                    continue;

                if (!TryConnectChunks(
                        chunkCoord,
                        adjacentChunk,
                        directions[i]))
                {
                    connectedChunkPairs.Remove(
                        ChunkConnectionKey(
                            chunkCoord,
                            adjacentChunk));
                }
            }
        }
        bool TryConnectChunks(
    Vector2Int chunkA,
    Vector2Int chunkB,
    Direction outwardDirection)
{
    List<RoomInfo> roomsA =
        RoomInfo.GetRoomsInChunk(chunkA);

    List<RoomInfo> roomsB =
        RoomInfo.GetRoomsInChunk(chunkB);

    if (roomsA.Count == 0 || roomsB.Count == 0)
        return false;

    Direction opposite =
        OppositeDirection(outwardDirection);

    float bestDistance = float.MaxValue;

    RoomInfo bestRoomA = null;
    RoomInfo bestRoomB = null;
    int bestExitIndexA = -1;
    int bestExitIndexB = -1;

    bool found = false;

    foreach (RoomInfo roomA in roomsA)
    {
        if (roomA.Definition?.exits == null)
            continue;

        for (int exitIndexA = 0; exitIndexA < roomA.Definition.exits.Length; exitIndexA++)
        {
            RoomExit exitA = roomA.Definition.exits[exitIndexA];

            if (exitA.exitDirection != outwardDirection || roomA.IsExitUsed(exitIndexA))
                continue;

            Vector2Int worldA = FindExteriorTile(roomA, exitA);

            foreach (RoomInfo roomB in roomsB)
            {
                if (roomB.Definition?.exits == null)
                    continue;

                for (int exitIndexB = 0; exitIndexB < roomB.Definition.exits.Length; exitIndexB++)
                {
                    RoomExit exitB = roomB.Definition.exits[exitIndexB];

                    if (exitB.exitDirection != opposite || roomB.IsExitUsed(exitIndexB))
                        continue;

                    Vector2Int worldB = FindExteriorTile(roomB, exitB);

                    float distance =
                        (worldB - worldA).sqrMagnitude;

                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    bestRoomA = roomA;
                    bestRoomB = roomB;
                    bestExitIndexA = exitIndexA;
                    bestExitIndexB = exitIndexB;
                    found = true;
                }
            }
        }
    }

    if (!found)
    {
        foreach (RoomInfo roomA in roomsA)
        {
            if (roomA.Definition?.exits == null)
                continue;

            for (int exitIndexA = 0; exitIndexA < roomA.Definition.exits.Length; exitIndexA++)
            {
                if (roomA.IsExitUsed(exitIndexA))
                    continue;

                RoomExit exitA = roomA.Definition.exits[exitIndexA];

                Vector2Int worldA = FindExteriorTile(roomA, exitA);

                foreach (RoomInfo roomB in roomsB)
                {
                    if (roomB.Definition?.exits == null)
                        continue;

                    for (int exitIndexB = 0; exitIndexB < roomB.Definition.exits.Length; exitIndexB++)
                    {
                        if (roomB.IsExitUsed(exitIndexB))
                            continue;

                        RoomExit exitB = roomB.Definition.exits[exitIndexB];

                        Vector2Int worldB = FindExteriorTile(roomB, exitB);

                        float distance =
                            (worldB - worldA).sqrMagnitude;

                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        bestRoomA = roomA;
                        bestRoomB = roomB;
                        bestExitIndexA = exitIndexA;
                        bestExitIndexB = exitIndexB;
                        found = true;
                    }
                }
            }
        }
    }

    if (!found)
        return false;

    RoomExit bestExitA = bestRoomA.Definition.exits[bestExitIndexA];
    RoomExit bestExitB = bestRoomB.Definition.exits[bestExitIndexB];

    CarveCorridorBetweenChunks(bestRoomA, bestExitA, bestRoomB, bestExitB);

    bestRoomA.MarkExitUsed(bestExitIndexA);
    bestRoomB.MarkExitUsed(bestExitIndexB);

    return true;
}

        void CarveCorridorBetweenChunks(
            RoomInfo roomA,
            RoomExit exitA,
            RoomInfo roomB,
            RoomExit exitB)
        {
            Vector2Int extA = FindExteriorTile(roomA, exitA);
            Vector2Int extB = FindExteriorTile(roomB, exitB);

            Vector2Int clearA = ClearRoomBounds(roomA, extA, DirectionToVector(exitA.exitDirection));
            Vector2Int clearB = ClearRoomBounds(roomB, extB, DirectionToVector(exitB.exitDirection));

            CarveManhattanPath(extA, clearA);
            CarveManhattanPath(clearA, clearB);
            CarveManhattanPath(clearB, extB);
        }

        Vector2Int ClearRoomBounds(RoomInfo room, Vector2Int start, Vector2Int dirVec)
        {
            if (dirVec == Vector2Int.zero)
                return start;

            Vector2Int current = start;
            int maxSteps = Mathf.Max(room.Bounds.width, room.Bounds.height) + 8;

            for (int i = 0; i < maxSteps; i++)
            {
                if (!room.Bounds.Contains(current))
                    return current;

                current += dirVec;
            }

            return current;
        }

        void CarveManhattanPath(Vector2Int from, Vector2Int to)
        {
            Vector2Int current = from;

            while (current.x != to.x)
            {
                CommitCorridorTile(current);

                current.x +=
                    current.x < to.x ? 1 : -1;
            }

            while (current.y != to.y)
            {
                CommitCorridorTile(current);

                current.y +=
                    current.y < to.y ? 1 : -1;
            }

            CommitCorridorTile(to);
        }
        void CommitCorridorTile(Vector2Int tile)
        {
            if (IsInRoom(tile))
                return;

            if (!corridorFloorTiles.Add(tile))
                return;

            PlaceCorridorGround(tile);
            PlaceCorridorWalls(tile);
        }
        void SealResolvedExits(Vector2Int chunkCoord)
        {
            Vector2Int[] neighbours =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            Direction[] directions =
            {
                Direction.NORTH,
                Direction.SOUTH,
                Direction.WEST,
                Direction.EAST
            };

            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector2Int adjacentChunk = chunkCoord + neighbours[i];

                if (!generatedChunks.Contains(adjacentChunk))
                    continue;

                SealExitsFacing(chunkCoord, directions[i]);
                SealExitsFacing(adjacentChunk, OppositeDirection(directions[i]));
            }
        }

        void SealExitsFacing(Vector2Int chunkCoord, Direction direction)
        {
            List<RoomInfo> rooms = RoomInfo.GetRoomsInChunk(chunkCoord);

            foreach (RoomInfo room in rooms)
            {
                if (room == null || room.Definition == null || room.Definition.exits == null)
                    continue;

                for (int exitIndex = 0; exitIndex < room.Definition.exits.Length; exitIndex++)
                {
                    if (room.IsExitUsed(exitIndex) || room.IsExitSealed(exitIndex))
                        continue;

                    RoomExit exit = room.Definition.exits[exitIndex];

                    if (exit.exitDirection != direction)
                        continue;

                    SealExit(room, exit);
                    room.MarkExitSealed(exitIndex);
                }
            }
        }

        void SealExit(RoomInfo room, RoomExit exit)
        {
            bool horizontal =
                exit.exitDirection == Direction.NORTH ||
                exit.exitDirection == Direction.SOUTH;

            int width = Mathf.Max(1, exit.width);

            TileBase sealTile = PickSealTile(room.Origin, exit, horizontal, width);

            if (sealTile == null)
                return;

            for (int i = 0; i < width; i++)
            {
                Vector2Int local = horizontal
                    ? new Vector2Int(exit.position.x + i, exit.position.y)
                    : new Vector2Int(exit.position.x, exit.position.y + i);

                Vector2Int world = room.Origin + local;
                Vector3Int cell = new(world.x, world.y, 0);

                corridorFloorTiles.Remove(world);
                corridorGroundTilemap.SetTile(cell, null);
                corridorWallTilemap.SetTile(cell, null);

                wallsTilemap.SetTile(cell, sealTile);
            }
        }

        TileBase PickSealTile(Vector2Int origin, RoomExit exit, bool horizontal, int width)
        {
            Vector2Int before = horizontal
                ? origin + exit.position + Vector2Int.left
                : origin + exit.position + Vector2Int.down;

            Vector2Int after = horizontal
                ? origin + exit.position + new Vector2Int(width, 0)
                : origin + exit.position + new Vector2Int(0, width);

            TileBase tile = wallsTilemap.GetTile(new Vector3Int(before.x, before.y, 0));

            if (tile == null)
                tile = wallsTilemap.GetTile(new Vector3Int(after.x, after.y, 0));

            if (tile == null)
                tile = config.corridorWallTile;

            return tile;
        }

        [ContextMenu("Generate Map")]
        public void GenerateMap()
        {
            ClearMap();
            InitializeSeed();

            if (playerTransform != null)
                StreamChunks();
            else
                GenerateChunk(Vector2Int.zero);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (groundTilemap != null) groundTilemap.ClearAllTiles();
            if (wallsTilemap != null) wallsTilemap.ClearAllTiles();
            if (collisionDecorTilemap != null) collisionDecorTilemap.ClearAllTiles();
            if (nonCollisionDecorTilemap != null) nonCollisionDecorTilemap.ClearAllTiles();
            if (corridorGroundTilemap != null) corridorGroundTilemap.ClearAllTiles();
            if (corridorWallTilemap != null) corridorWallTilemap.ClearAllTiles();

            if (roomParent != null)
            {
                for (int i = roomParent.childCount - 1; i >= 0; i--)
                    DestroyImmediate(roomParent.GetChild(i).gameObject);
            }

            generatedChunks.Clear();
            corridorFloorTiles.Clear();
            RoomInfo.ClearAll();
        }
    }
}
