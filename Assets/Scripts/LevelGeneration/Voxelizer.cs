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
    Voxel[] data;
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

        for(int i = 0; i < resolution * resolution * resolution; i++)
        {
            if (data[i].active)
            {
                if (Vector3.Distance(point, data[i].position) < radius)
                {
                    data[i].active = false;
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

            for (int i = 0; i < resolution * resolution * resolution; i++)
            {
                if (data[i].active)
                {
                    Vector3Int i3d = IndexToIndex3D(i);

                    if (i3d.x < resolution - 1)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x + 1, i3d.y, i3d.z));
                        if (!data[t].active)
                        {
                            CreateQuadRight(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.x > 0)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x - 1, i3d.y, i3d.z));
                        if (!data[t].active)
                        {
                            CreateQuadLeft(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.x == 0)
                    {
                        CreateQuadLeft(data[i].position, voxelSize, data[i].color);
                    }
                    if (i3d.x == resolution - 1)
                    {
                        CreateQuadRight(data[i].position, voxelSize, data[i].color);
                    }

                    if (i3d.y < resolution - 1)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y + 1, i3d.z));
                        if (!data[t].active)
                        {
                            CreateQuadTop(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.y > 0)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y - 1, i3d.z));
                        if (!data[t].active)
                        {
                            CreateQuadBottom(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.y == 0)
                    {
                        CreateQuadBottom(data[i].position, voxelSize, data[i].color);
                    }
                    if (i3d.y == resolution - 1)
                    {
                        CreateQuadTop(data[i].position, voxelSize, data[i].color);
                    }

                    if (i3d.z < resolution - 1)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z + 1));
                        if (!data[t].active)
                        {
                            CreateQuadFront(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.z > 0)
                    {
                        int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z - 1));
                        if (!data[t].active)
                        {
                            CreateQuadBack(data[i].position, voxelSize, data[i].color);
                        }
                    }
                    if (i3d.z == 0)
                    {
                        CreateQuadBack(data[i].position, voxelSize, data[i].color);
                    }
                    if (i3d.z == resolution - 1)
                    {
                        CreateQuadFront(data[i].position, voxelSize, data[i].color);
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
        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            if (data[i].active)
            {
                return false;
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
        data = new Voxel[resolution * resolution * resolution];
        bounds = mesh.bounds;
        voxelSize = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) / (resolution);
        
        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            Vector3Int i3d = IndexToIndex3D(i);

            float xp = ConvertRange(0, resolution, bounds.min.x, bounds.max.x, i3d.x);
            float yp = ConvertRange(0, resolution, bounds.min.y, bounds.max.y, i3d.y);
            float zp = ConvertRange(0, resolution, bounds.min.z, bounds.max.z, i3d.z);
            data[i] = new Voxel();
            data[i].position = new Vector3(xp, yp, zp);
            data[i].color = Color.white;
            data[i].index = i;
            data[i].active = PointInMesh(data[i].position + transform.position);
        }

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            if (data[i].active)
            {
                Vector3Int i3d = IndexToIndex3D(i);

                if (i3d.x < resolution - 1)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x + 1, i3d.y, i3d.z));
                    if (!data[t].active)
                    {
                        CreateQuadRight(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.x > 0)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x - 1, i3d.y, i3d.z));
                    if (!data[t].active)
                    {
                        CreateQuadLeft(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.x == 0)
                {
                    CreateQuadLeft(data[i].position, voxelSize, data[i].color);
                }
                if (i3d.x == resolution - 1)
                {
                    CreateQuadRight(data[i].position, voxelSize, data[i].color);
                }

                if (i3d.y < resolution - 1)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y + 1, i3d.z));
                    if (!data[t].active)
                    {
                        CreateQuadTop(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.y > 0)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y - 1, i3d.z));
                    if (!data[t].active)
                    {
                        CreateQuadBottom(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.y == 0)
                {
                    CreateQuadBottom(data[i].position, voxelSize, data[i].color);
                }
                if (i3d.y == resolution - 1)
                {
                    CreateQuadTop(data[i].position, voxelSize, data[i].color);
                }

                if (i3d.z < resolution - 1)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z + 1));
                    if (!data[t].active)
                    {
                        CreateQuadFront(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.z > 0)
                {
                    int t = Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z - 1));
                    if (!data[t].active)
                    {
                        CreateQuadBack(data[i].position, voxelSize, data[i].color);
                    }
                }
                if (i3d.z == 0)
                {
                    CreateQuadBack(data[i].position, voxelSize, data[i].color);
                }
                if (i3d.z == resolution - 1)
                {
                    CreateQuadFront(data[i].position, voxelSize, data[i].color);
                }

            }
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
    }

    int Index3dToIndex(Vector3Int i3d)
    {
        return i3d.x + (i3d.z * resolution) + (i3d.y * (resolution * resolution));
    }

    Vector3Int IndexToIndex3D(int i)
    {
        int x = i % resolution;
        int z = (i / resolution) % resolution;
        int y = ((i / resolution)/resolution) % resolution;
        return new Vector3Int(x, y, z);
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
