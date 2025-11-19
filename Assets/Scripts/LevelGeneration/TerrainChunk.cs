using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    bool[,,] voxelData = null;
    World world;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }

    void CreateVoxelData()
    {
        voxelData = new bool[world.chunkResolution, world.chunkResolution, world.chunkResolution];
        for (int x = 0; x < world.chunkResolution; x++)
        {
            for (int y = 0; y < world.chunkResolution; y++)
            {
                for (int z = 0; z < world.chunkResolution; z++)
                {
                    voxelData[x, y, z] = world.Perlin3D(new Vector3(x, y, z) * world.voxelSize + transform.position);
                }
            }
        }
    }

    void CreateMeshData()
    {
        for (int x = 0; x < world.chunkResolution; x++)
        {
            for (int y = 0; y < world.chunkResolution; y++)
            {
                for (int z = 0; z < world.chunkResolution; z++)
                {
                    if (voxelData[x, y, z])
                    {
                        if (x == world.chunkResolution - 1)
                        {
                            DrawQuadRight(new Vector3(x, y, z) * world.voxelSize);
                        }
                        if (x == 0)
                        {
                            DrawQuadLeft(new Vector3(x, y, z) * world.voxelSize);
                        }
                        if (y == world.chunkResolution - 1)
                        {
                            DrawQuadTop(new Vector3(x, y, z) * world.voxelSize);
                        }
                        if (y == 0)
                        {
                            DrawQuadBottom(new Vector3(x, y, z) * world.voxelSize);
                        }
                        if (z == world.chunkResolution - 1)
                        {
                            DrawQuadFront(new Vector3(x, y, z) * world.voxelSize);
                        }
                        if (z == 0)
                        {
                            DrawQuadBack(new Vector3(x, y, z) * world.voxelSize);
                        }

                        if (x < world.chunkResolution - 1)
                        {
                            if (!voxelData[x + 1, y, z])
                            {
                                DrawQuadRight(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                        if (x > 0)
                        {
                            if (!voxelData[x - 1, y, z])
                            {
                                DrawQuadLeft(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                        if (y < world.chunkResolution - 1)
                        {
                            if (!voxelData[x, y + 1, z])
                            {
                                DrawQuadTop(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                        if (y > 0)
                        {
                            if (!voxelData[x, y - 1, z])
                            {
                                DrawQuadBottom(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                        if (z < world.chunkResolution - 1)
                        {
                            if (!voxelData[x, y, z + 1])
                            {
                                DrawQuadFront(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                        if (z > 0)
                        {
                            if (!voxelData[x, y, z - 1])
                            {
                                DrawQuadBack(new Vector3(x, y, z) * world.voxelSize);
                            }
                        }
                    }

                }
            }
        }
    }


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void DrawQuadBottom(Vector3 position)
    {
        verts.Add(new Vector3(0, 0, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 0) * world.voxelSize + position);
        verts.Add(new Vector3(0, 0, 0) * world.voxelSize + position);

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

    void DrawQuadTop(Vector3 position)
    {
        verts.Add(new Vector3(0, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 1, 0) * world.voxelSize + position);
        verts.Add(new Vector3(0, 1, 0) * world.voxelSize + position);

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

    void DrawQuadFront(Vector3 position)
    {
        verts.Add(new Vector3(0, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 1) * world.voxelSize + position);
        verts.Add(new Vector3(0, 0, 1) * world.voxelSize + position);

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

    void DrawQuadBack(Vector3 position)
    {
        verts.Add(new Vector3(0, 1, 0) * world.voxelSize + position);
        verts.Add(new Vector3(1, 1, 0) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 0) * world.voxelSize + position);
        verts.Add(new Vector3(0, 0, 0) * world.voxelSize + position);

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

    void DrawQuadLeft(Vector3 position)
    {
        verts.Add(new Vector3(0, 1, 0) * world.voxelSize + position);
        verts.Add(new Vector3(0, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(0, 0, 1) * world.voxelSize + position);
        verts.Add(new Vector3(0, 0, 0) * world.voxelSize + position);

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

    void DrawQuadRight(Vector3 position)
    {
        verts.Add(new Vector3(1, 1, 0) * world.voxelSize + position);
        verts.Add(new Vector3(1, 1, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 1) * world.voxelSize + position);
        verts.Add(new Vector3(1, 0, 0) * world.voxelSize + position);

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

    


}
