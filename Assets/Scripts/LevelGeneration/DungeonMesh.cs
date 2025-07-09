using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class DungeonMesh : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Player GameObjects That will be placed in the level on Runtime")] public Transform player;
    
    [Tooltip("Determines max size of the level")][Min(3)] public Vector3Int gridSize = Vector3Int.one * 100;
    [Tooltip("Controls how far apart everything is")][Min(1)] public float tileSize = 5;
    public string seed = string.Empty;
    public enum LevelGenerationAlgorithm
    {
        RANDOM_WALKER,
        TINY_KEEP,
        HYBRID,
    }   
    [SerializeField] LevelGenerationAlgorithm levelGeneration = LevelGenerationAlgorithm.HYBRID;
    public enum MeshGenerationAlgorithm
    {
        VOXEL_MESH,
        MARCHING_CUBES,
    }
    public float isoLevel = 0.05f;
    [SerializeField] MeshGenerationAlgorithm meshGeneration = MeshGenerationAlgorithm.MARCHING_CUBES;

    Voxel[,,] grid = null;
    List<Vector3> verts;
    List<Vector2> uvs;
    List<int> tris;
    int buffer;
    Mesh mesh;

    [Space]
    [Header("RANDOM_WALKER Parameters")]
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField]bool walk3D = false;

    [Space]
    [Header("TINY_KEEP Parameters")]
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int hallwaySize = 2;
    [SerializeField][Min(2)] int roomHeight = 2;
    [SerializeField][Min(2)] int minRoomSize = 2;
    [SerializeField][Min(3)] int maxRoomSize = 10;

    Room[] rooms = null;

    void Awake()
    {
        Init();
        GenerateDungeon(levelGeneration);
        GenerateMesh();
        PlacePlayer();
    }
        
    void Init()
    {
        if (!player) player = GameObject.FindWithTag("Player").transform;
        if(seed == string.Empty) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());

        grid = new Voxel[gridSize.x, gridSize.y, gridSize.z];
        verts = new List<Vector3>();
        uvs = new List<Vector2>();
        tris = new List<int>();
        buffer = 0;

        for (int x = 0;  x < gridSize.x; x++) 
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    grid[x, y, z] = new Voxel();
                    grid[x, y, z].position = new Vector3(x, y, z) * tileSize;
                    grid[x, y, z].value = -1;
                }
            }
        }
    }

    void GenerateDungeon(LevelGenerationAlgorithm algorithm)
    {
        if(algorithm == LevelGenerationAlgorithm.RANDOM_WALKER)
        {
            RandomWalker();
        }
        else if(algorithm == LevelGenerationAlgorithm.HYBRID)
        {
            Hybrid(numberOfRooms);
        }
        else if(algorithm == LevelGenerationAlgorithm.TINY_KEEP)
        {
            TinyKeep(numberOfRooms);
        }
    }

    void ActivateBox(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (grid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x < maxX; x++)
        {
            for (int y = -maxY; y < maxY; y++)
            {
                for (int z = -maxZ; z < maxZ; z++)
                {
                    if (cell.x + x >= gridSize.x - 1|| cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= gridSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= gridSize.z - 1|| cell.z + z <= 0)
                    {
                        continue;
                    }

                    grid[cell.x + x, cell.y + y, cell.z + z].value = 1;
                }
            }
        }
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        if(meshGeneration == MeshGenerationAlgorithm.MARCHING_CUBES)
        {
            for (int x = 0; x < gridSize.x - 1; x++)
            {
                for (int y = 0; y < gridSize.y - 1; y++)
                {
                    for (int z = 0; z < gridSize.z - 1; z++)
                    {

                        Voxel[] pointsInBox = new Voxel[]
                        {
                            grid[x,y,z+1],
                            grid[x+1,y,z+1],
                            grid[x+1,y,z],
                            grid[x,y,z],
                            grid[x,y+1,z+1],
                            grid[x+1,y+1,z+1],
                            grid[x+1,y+1,z],
                            grid[x,y+1,z],
                        };

                        int cubeIndex = Voxel.GetState(pointsInBox, isoLevel);
                        Vector3[] triVerts = new Vector3[3];
                        int triIndex = 0;

                        int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                        foreach (int edgeIndex in triangulation)
                        {
                            if (edgeIndex > -1)
                            {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                                Vector3 vertexPos = Voxel.LerpPoint(pointsInBox[a], pointsInBox[b], isoLevel);
                                verts.Add(vertexPos);
                                tris.Add(buffer);
                                buffer++;

                                if (triIndex == 0)
                                {
                                    triVerts[0] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 1)
                                {
                                    triVerts[1] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 2)
                                {
                                    triVerts[2] = vertexPos;
                                    uvs.AddRange(GetUVs(triVerts[0], triVerts[1], triVerts[2]));
                                    triIndex = 0;
                                }

                            }
                            else
                            {
                                break;
                            }
                    }
                    }
                }
            }            
        } 
        else if(meshGeneration == MeshGenerationAlgorithm.VOXEL_MESH)
        {
            for(int x = 0; x < gridSize.x; x++)
            {
                for(int y = 0; y < gridSize.y; y++)
                {
                    for(int z = 0; z < gridSize.z; z++)
                    {
                        if(grid[x,y,z].value > isoLevel)
                        {
                            if(y > 0)
                            {
                                if(grid[x,y - 1,z].value <= isoLevel)
                                {
                                    DrawQuadBottom(grid[x,y,z].position);
                                }
                            }
                            if(y < gridSize.y - 1)
                            {
                                if(grid[x,y + 1,z].value <= isoLevel)
                                {
                                    DrawQuadTop(grid[x, y, z].position);
                                }
                            }
                            if(x > 0)
                            {
                                if(grid[x - 1,y,z].value <= isoLevel)
                                {
                                    DrawQuadLeft(grid[x, y, z].position);
                                }
                            }
                            if(x < gridSize.x - 1)
                            {
                                if(grid[x + 1,y,z].value <= isoLevel)
                                {
                                    DrawQuadRight(grid[x, y, z].position);
                                }
                            }
                            if(z > 0)
                            {
                                if(grid[x,y,z - 1].value <= isoLevel)
                                {
                                    DrawQuadBack(grid[x, y, z].position);
                                }
                            }
                            if(z < gridSize.z - 1)
                            {
                                if(grid[x,y,z + 1].value <= isoLevel)
                                {
                                    DrawQuadFront(grid[x, y, z].position);
                                }
                            }
                        }
                    }
                }
            }
        }


        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
    void RandomWalker()
    {
            Vector3Int currentIndex = new Vector3Int(gridSize.x/2, gridSize.y/2, gridSize.z/2);

            for (int step = 0; step < numberOfSteps; step++)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if(walk3D) y = UnityEngine.Random.Range(-1,2);
                int z = UnityEngine.Random.Range(-1, 2);

                currentIndex += new Vector3Int(x,y,z);
                ActivateBox(currentIndex, hallwaySize, hallwaySize, hallwaySize);
            }

            if(player) player.transform.position =  grid[currentIndex.x, currentIndex.y, currentIndex.z].position;

    }   

    void TinyKeep(int numberOfRooms)
    {
        //Create Rooms
        rooms = new Room[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++) 
        {
            int xi = UnityEngine.Random.Range(0, gridSize.x);
            int yi = UnityEngine.Random.Range(0, gridSize.y);
            int zi = UnityEngine.Random.Range(0, gridSize.z);
            int roomSizeX = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);
            int roomSizeZ = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);
            rooms[r] = new Room(new Vector3Int(xi, yi, zi), new Vector3Int(roomSizeX, roomHeight, roomSizeZ));
            ActivateBox(new Vector3Int(xi, yi, zi), roomSizeX, roomHeight, roomSizeZ);
        }

        //Create Hallways
        for (int r = 0; r < numberOfRooms; r++)
        {
            if (r < numberOfRooms - 1)
            {
                Vector3Int start = rooms[r].GetNearestExit(rooms[r + 1].indexPosition);
                Vector3Int end = rooms[r + 1].GetNearestExit(rooms[r].indexPosition);
                GenerateHallway(start, end);
            }
        }
    }
    
    void Hybrid(int numberOfRooms)
    {
        //Create Rooms
        rooms = new Room[numberOfRooms];
        for (int r = 0; r < numberOfRooms; r++)
        {
            int xi = UnityEngine.Random.Range(0, gridSize.x);
            int yi = UnityEngine.Random.Range(0, gridSize.y);
            int zi = UnityEngine.Random.Range(0, gridSize.z);

            Vector3Int currentIndex = new Vector3Int(xi, yi, zi);

            rooms[r] = new Room(new Vector3Int(xi, yi, zi));
            for (int s = 0; s < numberOfSteps; s++) 
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if(walk3D) y = UnityEngine.Random.Range(-1,2);
                int z = UnityEngine.Random.Range(-1, 2);

                currentIndex += new Vector3Int(x,y,z);
                ActivateBox(currentIndex,hallwaySize, hallwaySize,hallwaySize);
            }
        }
        

        //Create Hallways
        for (int r = 0; r < numberOfRooms; r++)
        {
            if (r < numberOfRooms - 1)
            {
                GenerateHallway(rooms[r].indexPosition, rooms[r+1].indexPosition);
            }
        }
    }

    void GenerateHallway(Vector3Int start, Vector3Int end)
    {
        Vector3Int currentPos = start;

        while (currentPos != end)
        {
            Vector3Int[] possibleDirections =
            {
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.forward,
                Vector3Int.back,
                new Vector3Int(1,0,1),
                new Vector3Int(-1,0,1),
                new Vector3Int(1,0,-1),
                new Vector3Int(-1,0,-1),
                new Vector3Int(1,1,1),
                new Vector3Int(-1,1,1),
                new Vector3Int(1,1,-1),
                new Vector3Int(-1,1,-1),
                new Vector3Int(1,-1,1),
                new Vector3Int(-1,-1,1),
                new Vector3Int(1,-1,-1),
                new Vector3Int(-1,-1,-1),
            };
            Vector3Int chosenDirection = possibleDirections[0];
            foreach (Vector3Int possibleDirection in possibleDirections)
            {
                if (Vector3Int.Distance(currentPos + chosenDirection, end) > Vector3Int.Distance(currentPos + possibleDirection, end))
                {
                    chosenDirection = possibleDirection;
                }
            }

            currentPos += chosenDirection;
            ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
        }
    }

    Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y) / tileSize;
            uvs[1] = new Vector2(b.z, b.y) / tileSize;
            uvs[2] = new Vector2(c.z, c.y) / tileSize;
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y) / tileSize;
            uvs[1] = new Vector2(b.x, b.y) / tileSize;
            uvs[2] = new Vector2(c.x, c.y) / tileSize;
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z) / tileSize;
            uvs[1] = new Vector2(b.x, b.z) / tileSize;
            uvs[2] = new Vector2(c.x, c.z) / tileSize;
        }

        return uvs;
    }

    void DrawQuadBottom(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadTop(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f,  0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f,  0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadFront(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f,  0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadBack(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadLeft(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadRight(Vector3 position)
    {
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void PlacePlayer()
    {
        if(player)
        { 
            if(levelGeneration == LevelGenerationAlgorithm.TINY_KEEP)
            {
                player.transform.position =  grid[rooms[0].indexPosition.x, rooms[0].indexPosition.y, rooms[0].indexPosition.z].position;
            }
            else if(levelGeneration == LevelGenerationAlgorithm.HYBRID)
            {
                player.transform.position =  grid[rooms[0].indexPosition.x, rooms[0].indexPosition.y, rooms[0].indexPosition.z].position;
            }
            else if(levelGeneration == LevelGenerationAlgorithm.RANDOM_WALKER)
            {
                for(int x = 0; x < gridSize.x; x++)
                {
                    for(int y = 0; y < gridSize.y; y++)
                    {
                        for(int z = 0; z < gridSize.z; z++)
                        {
                            if(grid[x,y,z].value >= isoLevel)
                            {
                                player.transform.position = grid[x,y,z].position;
                                return;
                            }
                        }
                    }
                }
            }


            Physics.Raycast(player.position, Vector3.down, out RaycastHit hit);
            player.transform.position = hit.point;

        }
    }
}