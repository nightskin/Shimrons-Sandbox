using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        Create();
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

            Create();
        }
    }

    public void RemoveBlock(Vector3 point)
    {
        Vector3Int i3d = PositionToIndex3D(point);
        if (i3d.x >= World.voxelsInSingleChunk || i3d.y >= World.voxelsInSingleChunk || i3d.z >= World.voxelsInSingleChunk || i3d.x < 0 || i3d.y < 0 || i3d.z < 0)
        {
            Debug.Log(i3d.x + "," + i3d.y + "," + i3d.z + " Out of Range");
            return;
        }
        int i = Index3DToIndex(i3d);
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

    float Evaluate2D(Vector3 point)
    {
        point = new Vector3(point.x, 0, point.z) * World.noiseScale;
        float noiseValue = 0;
        float frequency = World.baseRoughness;
        float amplitude = 1;


        for(int i = 0; i < World.layers; i++)
        {
            float v = World.noise.Evaluate(point * frequency);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= World.roughness;
            amplitude *= World.persistance;
        }

        noiseValue = Mathf.Max(World.minValue, noiseValue - World.minValue);
        return noiseValue * World.strength;
    }

    void CreateVoxelData(Vector3 center)
    {
        data = new Voxel[World.voxelsInSingleChunk * World.voxelsInSingleChunk * World.voxelsInSingleChunk];
        for (int i = 0; i < (World.voxelsInSingleChunk * World.voxelsInSingleChunk * World.voxelsInSingleChunk); i++)
        {
            Vector3Int index3d = IndexToIndex3D(i);

            data[i] = new Voxel();
            data[i].color = Color.white;
            data[i].x = index3d.x;
            data[i].y = index3d.y;
            data[i].z = index3d.z;
            data[i].position = new Vector3(index3d.x, index3d.y, index3d.z) * World.voxelSize;

            float height = Evaluate2D(data[i].position + center) * World.voxelsInSingleChunk;


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
        for (int i = 0; i < (World.voxelsInSingleChunk * World.voxelsInSingleChunk * World.voxelsInSingleChunk); i++)
        {
            Vector3Int pos = IndexToIndex3D(i);
            if (data[i].active)
            {
                if (pos.x < World.voxelsInSingleChunk - 1)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x + 1, pos.y, pos.z));
                    if (!data[index].active) CreateQuadRight(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.x > 0)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x - 1, pos.y, pos.z));
                    if (!data[index].active) CreateQuadLeft(data[i].position, World.voxelSize, data[i].color);
                }

                if (pos.z < World.voxelsInSingleChunk - 1)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x, pos.y, pos.z + 1));
                    if (!data[index].active) CreateQuadFront(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.z > 0)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x, pos.y, pos.z - 1));
                    if (!data[index].active) CreateQuadBack(data[i].position, World.voxelSize, data[i].color);
                }

                if (pos.y < World.voxelsInSingleChunk - 1)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x, pos.y + 1, pos.z));
                    if (!data[index].active) CreateQuadTop(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.y > 0)
                {
                    int index = Index3DToIndex(new Vector3Int(pos.x, pos.y - 1, pos.z));
                    if (!data[index].active) CreateQuadBottom(data[i].position, World.voxelSize, data[i].color);
                }


                if (pos.x == 0)
                {
                    CreateQuadLeft(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.x == World.voxelsInSingleChunk - 1)
                {
                    CreateQuadRight(data[i].position, World.voxelSize, data[i].color);
                }

                if (pos.z == 0)
                {
                    CreateQuadBack(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.z == World.voxelsInSingleChunk - 1)
                {
                    CreateQuadFront(data[i].position, World.voxelSize, data[i].color);
                }

                if (pos.y == 0)
                {
                    CreateQuadBottom(data[i].position, World.voxelSize, data[i].color);
                }
                if (pos.y == World.voxelsInSingleChunk - 1)
                {
                    CreateQuadTop(data[i].position, World.voxelSize, data[i].color);
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

    public void Create()
    {
        CreateVoxelData(transform.position);
        CreateVoxels();
    }

    Vector3Int IndexToIndex3D(int index)
    {
        int x = index % World.voxelsInSingleChunk;
        int z = (index / World.voxelsInSingleChunk) % World.voxelsInSingleChunk;
        int y = ((index / World.voxelsInSingleChunk) / World.voxelsInSingleChunk) % World.voxelsInSingleChunk;

        return new Vector3Int(x, y, z);
    }

    int Index3DToIndex(Vector3Int pos)
    {
        return pos.x + (pos.z * World.voxelsInSingleChunk) + (pos.y * (World.voxelsInSingleChunk * World.voxelsInSingleChunk));
    }

    public Vector3Int PositionToIndex3D(Vector3 pos)
    {
        float x = pos.x / World.voxelSize / World.voxelsInSingleChunk;
        float y = pos.y / World.voxelSize / World.voxelsInSingleChunk;
        float z = pos.z / World.voxelSize / World.voxelsInSingleChunk;

        x = Mathf.Abs(ConvertRange(0, 1, 0, World.voxelsInSingleChunk, x));
        y = Mathf.Abs(ConvertRange(0, 1, 0, World.voxelsInSingleChunk, y));
        z = Mathf.Abs(ConvertRange(0, 1, 0, World.voxelsInSingleChunk, z));

        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
    }

    public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }
}
