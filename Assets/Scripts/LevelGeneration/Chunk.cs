using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    Point[,,] map = null;

    [SerializeField] bool useVoxels = false;
    public Vector3 center = Vector3.zero;
    public float maxHeight = 15;

    [Min(1)] public  int tilesX = 16;
    [Min(1)] public  int tilesY = 32;
    [Min(1)] public  int tilesZ = 16;
    public int tileSize = 5;
    

    public float noiseScale = 0.001f;
    [SerializeField] float noiseThreshold = 0.5f;

    Mesh mesh;

    List<Vector3> verts = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        map = new Point[tilesX, tilesY, tilesZ];

        GetComponent<BoxCollider>().size = new Vector3(tilesX - 2, tilesY - 2, tilesZ - 2) * tileSize;
        GetComponent<BoxCollider>().center = new Vector3(-5, -5, -5);
        Generate();

    }

    void OnValidate()
    {
        if (mesh) Generate();
    }


    float Evaluate3D(Vector3 point)
    {
        return ChunkManager.noise.Evaluate(point * noiseScale);
    }
    
    float Evaluate2D(Vector3 point)
    {
        return Mathf.PerlinNoise((point.x + 0.1f) * noiseScale, (point.z + 0.1f) * noiseScale);
    }

    public void RemoveBlock(Vector3 hit)
    {
        Vector3 localPos = hit - transform.position;
        Point closestSpace = map[0, 0, 0];

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if (map[x, y, z].active)
                    {
                        if (Vector3.Distance(map[x, y, z].position, localPos) < Vector3.Distance(closestSpace.position, localPos))
                        {
                            closestSpace = map[x, y, z];
                        }
                    }
                }
            }
        }

        map[closestSpace.x, closestSpace.y, closestSpace.z].active = false;

        if (BlocksGone())
        {
            Destroy(gameObject);
        }
        else
        {
            uvs.Clear();
            verts.Clear();
            tris.Clear();
            colors.Clear();
            buffer = 0;

            if (useVoxels) CreateVoxels();
            else MarchingCubes();
            UpdateMesh();
        }

    }

    public void AddBlock(Vector3 hit)
    {
        Vector3 localPos = hit - transform.position;
        Point closestSpace = map[0, 0, 0];

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if (!map[x, y, z].active)
                    {
                        if (Vector3.Distance(map[x, y, z].position, localPos) < Vector3.Distance(closestSpace.position, localPos))
                        {
                            closestSpace = map[x, y, z];
                        }
                    }
                }
            }
        }

        map[closestSpace.x, closestSpace.y, closestSpace.z].active = true;

        uvs.Clear();
        verts.Clear();
        tris.Clear();
        colors.Clear();
        buffer = 0;

        if (useVoxels) CreateVoxels();
        else MarchingCubes();
        UpdateMesh();
    }

    public void Generate()
    {
        uvs.Clear();
        verts.Clear();
        tris.Clear();
        colors.Clear();
        buffer = 0;

        CreateMap();
        if (useVoxels) CreateVoxels();
        else MarchingCubes();
        UpdateMesh();

    }

    void CreateQuadBack(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-1, 1, -1) * size + position);
        verts.Add(new Vector3(1, -1, -1) * size + position);
        verts.Add(new Vector3(-1, -1, -1) * size + position);
        verts.Add(new Vector3(1, 1, -1) * size + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadFront(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-1, 1, 1) * size + position);
        verts.Add(new Vector3(1, -1, 1) * size + position);
        verts.Add(new Vector3(-1, -1, 1) * size + position);
        verts.Add(new Vector3(1, 1, 1) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadRight(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(1, 1, 1) * size + position);
        verts.Add(new Vector3(1, -1, 1) * size + position);
        verts.Add(new Vector3(1, -1, -1) * size + position);
        verts.Add(new Vector3(1, 1, -1) * size + position);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        tris.Add(1 + buffer);
        tris.Add(2 + buffer);
        tris.Add(3 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadLeft(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-1, 1, 1) * size + position);
        verts.Add(new Vector3(-1, -1, 1) * size + position);
        verts.Add(new Vector3(-1, -1, -1) * size + position);
        verts.Add(new Vector3(-1, 1, -1) * size + position);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        tris.Add(2 + buffer);
        tris.Add(1 + buffer);
        tris.Add(3 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadBottom(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-1, -1, -1) * size + position);
        verts.Add(new Vector3(-1, -1, 1) * size + position);
        verts.Add(new Vector3(1, -1, -1) * size + position);
        verts.Add(new Vector3(1, -1, 1) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void CreateQuadTop(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-1, 1, -1) * size + position);
        verts.Add(new Vector3(-1, 1, 1) * size + position);
        verts.Add(new Vector3(1, 1, -1) * size + position);
        verts.Add(new Vector3(1, 1, 1) * size + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void CreateMap()
    {
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    map[x, y, z] = new Point();
                    map[x, y, z].x = x;
                    map[x, y, z].y = y;
                    map[x, y, z].z = z;

                    map[x, y, z].position = new Vector3(x - (tilesX / 2), y - (tilesY / 2), z - (tilesZ / 2)) * tileSize;
                    map[x, y, z].height = Evaluate2D(map[x, 0, z].position + center) * maxHeight;
                    map[x, y, z].value = Evaluate3D(map[x, y, z].position + center); 

                    if(y <= map[x,y,z].height)
                    {
                        if(map[x,y,z].height >= noiseThreshold)
                        {
                            map[x, y, z].active = true;
                        }
                    }

                    if(map[x,y,z].value >= noiseThreshold)
                    {
                        map[x, y, z].active = false;
                    }

                    if(y == 0)
                    {
                        map[x, y, z].active = true;
                    }

                }
            }
        }

    }

    void CreateVoxels()
    {
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if (map[x, y, z].active)
                    {
                        if (x < tilesX - 1)
                        {
                            if (!map[x + 1, y, z].active) CreateQuadRight(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }
                        if (x > 0)
                        {
                            if (!map[x - 1, y, z].active) CreateQuadLeft(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }
                        if (y < tilesY - 1)
                        {
                            if (!map[x, y + 1, z].active) CreateQuadTop(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }
                        if (y > 0)
                        {
                            if (!map[x, y - 1, z].active) CreateQuadBottom(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }
                        if (z < tilesZ - 1)
                        {
                            if (!map[x, y, z + 1].active) CreateQuadFront(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }
                        if (z > 0)
                        {
                            if (!map[x, y, z - 1].active) CreateQuadBack(map[x, y, z].position, tileSize, map[x, y, z].color);
                        }

                        if (x == 0) CreateQuadLeft(map[x, y, z].position, tileSize, map[x, y, z].color);
                        if (y == 0) CreateQuadBottom(map[x, y, z].position, tileSize, map[x, y, z].color);
                        if (z == 0) CreateQuadBack(map[x, y, z].position, tileSize, map[x, y, z].color);
                        if (x == tilesX - 1) CreateQuadRight(map[x, y, z].position, tileSize, map[x, y, z].color);
                        if (y == tilesY - 1) CreateQuadTop(map[x, y, z].position, tileSize, map[x, y, z].color);
                        if (z == tilesZ - 1) CreateQuadFront(map[x, y, z].position, tileSize, map[x, y, z].color);

                    }
                }
            }
        }
    }

    void MarchingCubes()
    {

        for (int x = tilesX; x > 0; x--)
        {
            for (int y = tilesY; y > 0; y--)
            {
                for (int z = tilesZ; z > 0; z--)
                {
                    if (x < tilesX - 1 && y < tilesY - 1 && z < tilesZ - 1)
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

                                Vector3 vertexPos = GetMidPoint(points[a], points[b]);

                                verts.Add(vertexPos);
                                tris.Add(buffer);
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

    Vector3 GetMidPoint(Point point1, Point point2)
    {
        return (point1.position + point2.position) / 2;
    }

    bool BlocksGone()
    {
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if (map[x, y, z].active) return false;
                }
            }
        }

        return true;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
