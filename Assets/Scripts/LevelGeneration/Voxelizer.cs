using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Voxelizer : MonoBehaviour
{
    public int resolution = 32;
    public Vector3 scale = Vector3.one;
    public Quaternion rotation = Quaternion.identity;

    Vector3 voxelSize;
    Bounds bounds;
    Mesh mesh;
    Voxel[,,] data;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();
    int indexBuffer = 0;


    void Start()
    {
        ConvertToVoxels();
        GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.localScale = scale;
        transform.localRotation = rotation;
    }

    public void Teraform(Vector3 point, float radius)
    {
        point = transform.InverseTransformPoint(point);

        float scaler = Mathf.Max(voxelSize.x, voxelSize.y, voxelSize.z);
        radius *= scaler;

        for(int x = 0; x < resolution; x++)
        {
            for(int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    if (data[x,y,z].active)
                    {
                        if(Vector3.Distance(point, data[x,y,z].position) < radius)
                        {
                            data[x, y, z].active = false;
                        }
                    }
                }
            }
        }


        if (BlocksGone())
        {
            Destroy(gameObject);
        }
        else
        {
            // rebuild mesh
            verts.Clear();
            tris.Clear();
            colors.Clear();
            indexBuffer = 0;
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        if (data[x, y, z].active)
                        {
                            if (x < resolution - 1)
                            {
                                if (!data[x + 1, y, z].active)
                                {
                                    CreateQuadRight(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (x > 0)
                            {
                                if (!data[x - 1, y, z].active)
                                {
                                    CreateQuadLeft(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (x == 0)
                            {
                                CreateQuadLeft(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                            if (x == resolution - 1)
                            {
                                CreateQuadRight(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }

                            if (y < resolution - 1)
                            {
                                if (!data[x, y + 1, z].active)
                                {
                                    CreateQuadTop(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (y > 0)
                            {
                                if (!data[x, y - 1, z].active)
                                {
                                    CreateQuadBottom(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (y == 0)
                            {
                                CreateQuadBottom(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                            if (y == resolution - 1)
                            {
                                CreateQuadTop(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }

                            if (z < resolution - 1)
                            {
                                if (!data[x, y, z + 1].active)
                                {
                                    CreateQuadFront(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (z > 0)
                            {
                                if (!data[x, y, z - 1].active)
                                {
                                    CreateQuadBack(data[x, y, z].position, voxelSize, data[x, y, z].color);
                                }
                            }
                            if (z == 0)
                            {
                                CreateQuadBack(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                            if (z == resolution - 1)
                            {
                                CreateQuadFront(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }

                        }
                    }
                }
            }
            mesh.Clear();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.colors = colors.ToArray();
            mesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

    }

    float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }

    bool BlocksGone()
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    if (data[x, y, z].active)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    bool PointInMesh(Vector3 point)
    {
        Vector3 dir = Vector3.forward;
        float maxBound = Mathf.Max(new float[3] { mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z});
        if (Physics.Raycast(point, dir, maxBound) && Physics.Raycast(point, -dir, maxBound))
        {
            return true;
        }
        return false;
    }

    void ConvertToVoxels()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        data = new Voxel[resolution, resolution, resolution];
        bounds = mesh.bounds;
        voxelSize = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) / (resolution);
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float xp = ConvertRange(0, resolution, bounds.min.x, bounds.max.x, x);
                    float yp = ConvertRange(0, resolution, bounds.min.y, bounds.max.y, y);
                    float zp = ConvertRange(0, resolution, bounds.min.z, bounds.max.z, z);
                    data[x, y, z] = new Voxel();
                    data[x, y, z].position = new Vector3(xp, yp, zp);
                    data[x, y, z].color = Color.white;
                    data[x, y, z].index = new Vector3Int(x, y, z);
                    data[x, y, z].active = PointInMesh(data[x, y, z].position + transform.position);
                }
            }
        }

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    if (data[x,y,z].active) 
                    {
                        if(x < resolution - 1)
                        {
                            if (!data[x + 1, y, z].active)
                            {
                                CreateQuadRight(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if(x > 0)
                        {
                            if (!data[x - 1, y, z].active)
                            {
                                CreateQuadLeft(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if (x == 0)
                        {
                            CreateQuadLeft(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }
                        if (x == resolution - 1)
                        {
                            CreateQuadRight(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }

                        if (y < resolution - 1)
                        {
                            if (!data[x, y + 1, z].active)
                            {
                                CreateQuadTop(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if (y > 0)
                        {
                            if (!data[x, y - 1, z].active)
                            {
                                CreateQuadBottom(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if (y == 0)
                        {
                            CreateQuadBottom(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }
                        if (y == resolution - 1)
                        {
                            CreateQuadTop(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }

                        if (z < resolution - 1)
                        {
                            if (!data[x, y, z + 1].active)
                            {
                                CreateQuadFront(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if (z > 0)
                        {
                            if (!data[x, y, z - 1].active)
                            {
                                CreateQuadBack(data[x, y, z].position, voxelSize, data[x, y, z].color);
                            }
                        }
                        if (z == 0)
                        {
                            CreateQuadBack(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }
                        if (z == resolution - 1)
                        {
                            CreateQuadFront(data[x, y, z].position, voxelSize, data[x, y, z].color);
                        }

                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

    void CreateQuadBack(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, -0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, -0.5f * size.z) + position);

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

    void CreateQuadFront(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, -0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);

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

    void CreateQuadRight(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, -0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, -0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, -0.5f * size.z) + position);

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

    void CreateQuadLeft(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, 0.5f  * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, -0.5f  * size.z) + position);

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

    void CreateQuadBottom(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, -0.5f * size.y, 0.5f  * size.z) + position);
        verts.Add(new Vector3(0.5f  * size.x, -0.5f  * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f  * size.x, -0.5f  * size.y, 0.5f  * size.z) + position);

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

    void CreateQuadTop(Vector3 position, Vector3 size, Color color)
    {
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(-0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, -0.5f * size.z) + position);
        verts.Add(new Vector3(0.5f * size.x, 0.5f * size.y, 0.5f * size.z) + position);

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


}
