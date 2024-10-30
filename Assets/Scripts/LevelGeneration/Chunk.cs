using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    World world;
    Voxel[] data = null;
    Mesh mesh;

    bool loaded = false;

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();
    int indexBuffer = 0;


    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        CreateVoxelData(transform.position);
        CreateVoxels();
        loaded = true;
    }

    void OnValidate()
    {
        if(loaded)
        {
            verts.Clear();
            tris.Clear();
            colors.Clear();
            indexBuffer = 0;

            CreateVoxelData(transform.position);
            CreateVoxels();
        }
    }

    public void RemoveBlock(Vector3 point)
    {
        Vector3Int i3d = world.PositionToIndex3D(point);
        if (i3d.x >= world.voxelsInSingleChunk || i3d.y >= world.voxelsInSingleChunk || i3d.z >= world.voxelsInSingleChunk || i3d.x < 0 || i3d.y < 0 || i3d.z < 0)
        {
            Debug.Log(i3d.x + "," + i3d.y + "," + i3d.z + " Out of Range");
            return;
        }
        int i = world.Index3DToIndex(i3d);
        if (data[i].active)
        {
            data[i].active = false;
            verts.Clear();
            tris.Clear();
            colors.Clear();
            indexBuffer = 0;

            CreateVoxels();
            Debug.Log("Block Removed");
        }
        else
        {
            Debug.Log("Block Already Gone");
        }
    }

    void CreateQuadBack(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(0 + indexBuffer);
        tris.Add(1 + indexBuffer);
        tris.Add(2 + indexBuffer);

        tris.Add(0 + indexBuffer);
        tris.Add(3 + indexBuffer);
        tris.Add(1 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;
    }

    void CreateQuadFront(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(1 + indexBuffer);
        tris.Add(0 + indexBuffer);
        tris.Add(2 + indexBuffer);

        tris.Add(3 + indexBuffer);
        tris.Add(0 + indexBuffer);
        tris.Add(1 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;
    }

    void CreateQuadRight(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(3 + indexBuffer);
        tris.Add(0 + indexBuffer);
        tris.Add(1 + indexBuffer);

        tris.Add(1 + indexBuffer);
        tris.Add(2 + indexBuffer);
        tris.Add(3 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;
    }

    void CreateQuadLeft(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(0 + indexBuffer);
        tris.Add(3 + indexBuffer);
        tris.Add(1 + indexBuffer);

        tris.Add(2 + indexBuffer);
        tris.Add(1 + indexBuffer);
        tris.Add(3 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;
    }

    void CreateQuadBottom(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);

        tris.Add(1 + indexBuffer);
        tris.Add(0 + indexBuffer);
        tris.Add(2 + indexBuffer);

        tris.Add(3 + indexBuffer);
        tris.Add(1 + indexBuffer);
        tris.Add(2 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;

    }

    void CreateQuadTop(Vector3 position, float size, Color color)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(0 + indexBuffer);
        tris.Add(1 + indexBuffer);
        tris.Add(2 + indexBuffer);

        tris.Add(1 + indexBuffer);
        tris.Add(3 + indexBuffer);
        tris.Add(2 + indexBuffer);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indexBuffer += 4;

    }

    void CreateVoxelData(Vector3 center)
    {
        data = new Voxel[world.voxelsInSingleChunk * world.voxelsInSingleChunk * world.voxelsInSingleChunk];
        for (int i = 0; i < (world.voxelsInSingleChunk * world.voxelsInSingleChunk * world.voxelsInSingleChunk); i++)
        {
            Vector3Int index3d = world.IndexToIndex3D(i);

            data[i] = new Voxel();
            data[i].color = Color.white;
            data[i].x = index3d.x;
            data[i].y = index3d.y;
            data[i].z = index3d.z;
            data[i].position = new Vector3(index3d.x, index3d.y, index3d.z) * world.voxelSize;

            float height = World.Evaluate2D(data[i].position + center) * world.voxelsInSingleChunk;


            if (height < data[i].position.y)
            {
                data[i].active = false;
            }
            else
            {
                data[i].active = true;
            }
        }
    } 

    void CreateVoxels()
    {
        for (int i = 0; i < (world.voxelsInSingleChunk * world.voxelsInSingleChunk * world.voxelsInSingleChunk); i++)
        {
            Vector3Int pos = world.IndexToIndex3D(i);
            if (data[i].active)
            {
                if (pos.x < world.voxelsInSingleChunk - 1)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x + 1, pos.y, pos.z));
                    if (!data[index].active) CreateQuadRight(data[i].position, world.voxelSize, data[i].color);
                }
                if (pos.x > 0)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x - 1, pos.y, pos.z));
                    if (!data[index].active) CreateQuadLeft(data[i].position, world.voxelSize, data[i].color);
                }

                if (pos.z < world.voxelsInSingleChunk - 1)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x, pos.y, pos.z + 1));
                    if (!data[index].active) CreateQuadFront(data[i].position, world.voxelSize, data[i].color);
                }
                if (pos.z > 0)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x, pos.y, pos.z - 1));
                    if (!data[index].active) CreateQuadBack(data[i].position, world.voxelSize, data[i].color);
                }

                if (pos.y < world.voxelsInSingleChunk - 1)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x, pos.y + 1, pos.z));
                    if (!data[index].active) CreateQuadTop(data[i].position, world.voxelSize, data[i].color);
                }
                if (pos.y > 0)
                {
                    int index = world.Index3DToIndex(new Vector3Int(pos.x, pos.y - 1, pos.z));
                    if (!data[index].active) CreateQuadBottom(data[i].position, world.voxelSize, data[i].color);
                }


                if (pos.x == 0)
                {
                    CreateQuadLeft(data[i].position, world.voxelSize, data[i].color);
                }
                if (pos.x == world.voxelsInSingleChunk - 1)
                {
                    CreateQuadRight(data[i].position, world.voxelSize, data[i].color);
                }

                if (pos.z == 0)
                {
                    CreateQuadBack(data[i].position,   world.voxelSize, data[i].color);
                }
                if (pos.z == world.voxelsInSingleChunk - 1)
                {
                    CreateQuadFront(data[i].position, world.voxelSize, data[i].color);
                }

                if (pos.y == 0)
                {
                    CreateQuadBottom(data[i].position, world.voxelSize, data[i].color);
                }
                if (pos.y == world.voxelsInSingleChunk - 1)
                {
                    CreateQuadTop(data[i].position, world.voxelSize, data[i].color);
                }
            }
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }




}
