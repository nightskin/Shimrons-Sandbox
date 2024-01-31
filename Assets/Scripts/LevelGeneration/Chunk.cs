using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Point[,,] map = null;
    public Mesh mesh;
    ChunkManager chunkManager;


    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();

    int buffer = 0;

    void Awake()
    {
        chunkManager = GameObject.Find("ChunkManager").GetComponent<ChunkManager>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    float Evaluate3D(Vector3 point)
    {
        return Mathf.Abs(ChunkManager.noise.Evaluate(point * chunkManager.noiseScale));
    }

    float Evaluate2D(Vector3 point)
    {
        Vector3 p = new Vector3(point.x, 0, point.z);
        return Mathf.Abs(ChunkManager.noise.Evaluate(p * chunkManager.noiseScale));
    }

    public void RemoveBlocks(Vector3 hit)
    {
        float radius = chunkManager.tileSize;
        Vector3 localPos = hit - transform.position;

        for (int x = 0; x < chunkManager.tilesPerChunkXZ; x++)
        {
            for (int y = 0; y < chunkManager.tilesPerChunkY; y++)
            {
                for (int z = 0; z < chunkManager.tilesPerChunkXZ; z++)
                {
                    if (map[x, y, z].active)
                    {
                        if (Vector3.Distance(map[x, y, z].position, localPos) <= radius)
                        {
                            map[x, y, z].active = false;
                        }
                    }
                }
            }
        }

        verts.Clear();
        tris.Clear();
        colors.Clear();
        buffer = 0;

        if (chunkManager.getRidOfBlocksCuzTheySuck) MarchingCubes();
        else CreateVoxels();
        Draw();

    }

    public void CreateMeshData(bool noVoxels = true)
    {
        verts.Clear();
        tris.Clear();
        colors.Clear();
        buffer = 0;

        if (noVoxels) MarchingCubes();
        else CreateVoxels();
    }

    public void CreateVoxelData(Vector3 center)
    {
        map = new Point[chunkManager.tilesPerChunkXZ, chunkManager.tilesPerChunkY, chunkManager.tilesPerChunkXZ];
        for (int x = 0; x < chunkManager.tilesPerChunkXZ; x++)
        {
            for (int z = 0; z < chunkManager.tilesPerChunkXZ; z++)
            {
                for (int y = 0; y < chunkManager.tilesPerChunkY; y++)
                {
                    map[x, y, z] = new Point();
                    map[x, y, z].x = x;
                    map[x, y, z].y = y;
                    map[x, y, z].z = z;

                    map[x, y, z].position = new Vector3(x - (chunkManager.tilesPerChunkXZ / 2), y - (chunkManager.tilesPerChunkY / 2), z - (chunkManager.tilesPerChunkXZ / 2)) * (chunkManager.tileSize);
                    map[x, y, z].color = chunkManager.landGradient.Evaluate((float)y / (float)chunkManager.tilesPerChunkY);

                    if(y <= chunkManager.surfaceLevel)
                    {
                        map[x,y,z].height = chunkManager.surfaceLevel;
                    }
                    else
                    {
                        map[x, y, z].height = Evaluate2D(map[x, y, z].position + center) * (chunkManager.tilesPerChunkY - 2);
                    }


                    if (map[x,y,z].height >= y)
                    {
                        map[x,y,z].active = true;
                    }
                    else
                    {
                        map[x, y, z].active = false;
                    }
                    
                    if(x == 0 || x == chunkManager.tilesPerChunkXZ - 2 || z == 0 || z == chunkManager.tilesPerChunkXZ - 2)
                    {
                        map[x,y,z].active = false;
                    }

                }
            }
        }
    }

    void CreateQuadBack(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadFront(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadRight(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        tris.Add(1 + buffer);
        tris.Add(2 + buffer);
        tris.Add(3 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadLeft(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        tris.Add(2 + buffer);
        tris.Add(1 + buffer);
        tris.Add(3 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadBottom(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void CreateQuadTop(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void CreateVoxels()
    {
        for (int x = 0; x < chunkManager.tilesPerChunkXZ; x++)
        {
            for (int y = 0; y < chunkManager.tilesPerChunkY; y++)
            {
                for (int z = 0; z < chunkManager.tilesPerChunkXZ; z++)
                {
                    if (map[x, y, z].active)
                    {
                        if (x < chunkManager.tilesPerChunkXZ - 1)
                        {
                            if (!map[x + 1, y, z].active) CreateQuadRight(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }
                        if (x > 0)
                        {
                            if (!map[x - 1, y, z].active) CreateQuadLeft(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }
                        if (y < chunkManager.tilesPerChunkY - 1)
                        {
                            if (!map[x, y + 1, z].active) CreateQuadTop(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }
                        if (y > 0)
                        {
                            if (!map[x, y - 1, z].active) CreateQuadBottom(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }
                        if (z < chunkManager.tilesPerChunkXZ - 1)
                        {
                            if (!map[x, y, z + 1].active) CreateQuadFront(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }
                        if (z > 0)
                        {
                            if (!map[x, y, z - 1].active) CreateQuadBack(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        }

                        if (x == 0) CreateQuadLeft(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        if (y == 0) CreateQuadBottom(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        if (z == 0) CreateQuadBack(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        if (x == chunkManager.tilesPerChunkXZ - 1) CreateQuadRight(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        if (y == chunkManager.tilesPerChunkY - 1) CreateQuadTop(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);
                        if (z == chunkManager.tilesPerChunkXZ - 1) CreateQuadFront(map[x, y, z].position, chunkManager.tileSize, map[x, y, z].color);

                    }
                }
            }
        }
    }

    void MarchingCubes()
    {

        for (int x = chunkManager.tilesPerChunkXZ; x > 0; x--)
        {
            for (int y = chunkManager.tilesPerChunkY; y > 0; y--)
            {
                for (int z = chunkManager.tilesPerChunkXZ; z > 0; z--)
                {
                    if (x < chunkManager.tilesPerChunkXZ - 1 && y < chunkManager.tilesPerChunkY - 1 && z < chunkManager.tilesPerChunkXZ - 1)
                    {
                        Point[] points = new Point[]
                        {
                            map[x,y,z-1],
                            map[x-1,y,z-1],
                            map[x-1,y,z],
                            map[x,y,z],
                            map[x,y-1,z-1],
                            map[x-1,y-1,z-1],
                            map[x-1,y-1,z],
                            map[x,y-1,z],
                        };
                        int cubeIndex = GetState(points);
                        int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                        foreach (int edgeIndex in triangulation)
                        {
                            if (edgeIndex > -1)
                            {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos;
                                if(chunkManager.smoothing)
                                {
                                    float level = Mathf.Max(points[a].y, points[b].y);
                                    float amount = (level - points[a].height) / (points[b].height - points[a].height);
                                    vertexPos = Vector3.Lerp(points[a].position, points[b].position, amount);
                                }
                                else
                                {
                                    vertexPos = (points[a].position + points[b].position) / 2;
                                }


                                verts.Add(vertexPos);
                                tris.Add(buffer);
                                colors.Add(GetMidPointColor(points[a], points[b]));
                                buffer++;
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
    }

    int GetState(Point[] points)
    {
        int state = 0;
        if (points[0].active) state |= 1;
        if (points[1].active) state |= 2;
        if (points[2].active) state |= 4;
        if (points[3].active) state |= 8;
        if (points[4].active) state |= 16;
        if (points[5].active) state |= 32;
        if (points[6].active) state |= 64;
        if (points[7].active) state |= 128;
        return state;
    }

    Color GetMidPointColor(Point point1, Point point2)
    {
        return (point1.color + point2.color) / 2;
    }

    public void Draw()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
