using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Voxel[,,] map = null;
    public Mesh mesh;
    ChunkManager chunkManager;


    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();

    int buffer = 0;

    void Awake()
    {
        GameObject chunkManagerObj = GameObject.Find("ChunkManager");
        if (chunkManagerObj) chunkManager = chunkManagerObj.GetComponent<ChunkManager>();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        if(!chunkManager)
        {
            CreateVoxelData(transform.position);
            CreateMeshData(ChunkManager.getRidOfBlocksCuzTheySuck);
            Draw();
        }

    }

    float Evaluate3D(Vector3 point)
    {
        return Mathf.Abs(ChunkManager.noise.Evaluate(point * ChunkManager.noiseScale));
    }

    float Evaluate2D(Vector3 point)
    {
        Vector3 p = new Vector3(point.x, 0, point.z);
        return Mathf.Abs(ChunkManager.noise.Evaluate(p * ChunkManager.noiseScale));
    }

    public void RemoveBlock(Vector3 point)
    {
        Voxel closestPoint = new Voxel();
        for(int x = 0; x < ChunkManager.voxelsPerChunk; x++)
        {
            for (int z = 0; z < ChunkManager.voxelsPerChunk; z++)
            {
                for (int y = 0; y < ChunkManager.voxelsPerChunk; y++)
                {
                    if(Vector3.Distance(closestPoint.position, point) > Vector3.Distance(map[x,y,z].position, point))
                    {
                        closestPoint = map[x, y, z];
                    }
                }
            }
        }

        map[closestPoint.x, closestPoint.y, closestPoint.z].active = false;
        verts.Clear();
        tris.Clear();
        colors.Clear();
        buffer = 0;

        if (ChunkManager.getRidOfBlocksCuzTheySuck) MarchingCubes();
        else CreateVoxels();
        Draw();
    }

    public void RemoveBlock(int x, int y, int z)
    {
        if(x >= ChunkManager.voxelsPerChunk || y >= ChunkManager.voxelsPerChunk || z >= ChunkManager.voxelsPerChunk || x < 0 || y < 0 || z < 0)
        {
            Debug.Log(x + "," + y + "," + z + " Out of Range");
            return;
        }


        if (map[x,y,z].active)
        {
            map[x, y, z].active = false;
            verts.Clear();
            tris.Clear();
            colors.Clear();
            buffer = 0;

            if (ChunkManager.getRidOfBlocksCuzTheySuck) MarchingCubes();
            else CreateVoxels();
            Draw();

            Debug.Log(x + "," + y + "," + z + " Removed");
        }
        else
        {
            Debug.Log(x + "," + y + "," + z + " Is already Removed");
        }
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
        map = new Voxel[ChunkManager.voxelsPerChunk + 1, ChunkManager.voxelsPerChunk + 1, ChunkManager.voxelsPerChunk + 1];
        for (int x = 0; x < ChunkManager.voxelsPerChunk + 1; x++)
        {
            for (int z = 0; z < ChunkManager.voxelsPerChunk + 1; z++)
            {
                for (int y = 0; y < ChunkManager.voxelsPerChunk + 1; y++)
                {
                    map[x, y, z] = new Voxel();
                    map[x, y, z].x = x;
                    map[x, y, z].y = y;
                    map[x, y, z].z = z;

                    map[x, y, z].position = new Vector3(x, y, z) * ChunkManager.voxelSize;
                    map[x, y, z].color = new Color(0, 0.5f, 0);

                    if(y < ChunkManager.surfaceLevel)
                    {
                        map[x,y,z].height = ChunkManager.surfaceLevel;
                    }
                    else
                    {
                        map[x, y, z].height = Evaluate2D(map[x, y, z].position + center) * ChunkManager.voxelsPerChunk;
                    }



                    if (map[x,y,z].height >= y)
                    {
                        map[x,y,z].active = true;
                    }
                    else
                    {
                        map[x, y, z].active = false;
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

    public void CreateVoxels()
    {
        for (int x = 0; x < ChunkManager.voxelsPerChunk; x++)
        {
            for (int y = 0; y < ChunkManager.voxelsPerChunk; y++)
            {
                for (int z = 0; z < ChunkManager.voxelsPerChunk; z++)
                {
                    if (map[x, y, z].active)
                    {
                        if (x < ChunkManager.voxelsPerChunk - 1)
                        {
                            if (!map[x + 1, y, z].active) CreateQuadRight(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }
                        if (x > 0)
                        {
                            if (!map[x - 1, y, z].active) CreateQuadLeft(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }
                        if (y < ChunkManager.voxelsPerChunk - 1)
                        {
                            if (!map[x, y + 1, z].active) CreateQuadTop(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }
                        if (y > 0)
                        {
                            if (!map[x, y - 1, z].active) CreateQuadBottom(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }
                        if (z < ChunkManager.voxelsPerChunk - 1)
                        {
                            if (!map[x, y, z + 1].active) CreateQuadFront(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }
                        if (z > 0)
                        {
                            if (!map[x, y, z - 1].active) CreateQuadBack(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        }

                        if (x == 0) CreateQuadLeft(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        if (y == 0) CreateQuadBottom(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        if (z == 0) CreateQuadBack(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        if (x == ChunkManager.voxelsPerChunk - 1) CreateQuadRight(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        if (y == ChunkManager.voxelsPerChunk - 1) CreateQuadTop(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);
                        if (z == ChunkManager.voxelsPerChunk - 1) CreateQuadFront(map[x, y, z].position, ChunkManager.voxelSize, map[x, y, z].color);

                    }
                }
            }
        }
    }

    public void MarchingCubes()
    {
        for (int x = ChunkManager.voxelsPerChunk; x > 0; x--)
        {
            for (int y = ChunkManager.voxelsPerChunk; y > 0; y--)
            {
                for (int z = ChunkManager.voxelsPerChunk; z > 0; z--)
                {
                    Voxel[] points = new Voxel[]
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
                            if (ChunkManager.smoothing)
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

    int GetState(Voxel[] points)
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

    Color GetMidPointColor(Voxel point1, Voxel point2)
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
