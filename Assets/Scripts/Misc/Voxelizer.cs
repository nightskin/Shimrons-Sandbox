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

    public Vector3 voxelSize;
    public Bounds bounds;
    Mesh mesh;
    public Voxel[] data;
    public List<Vector3> verts = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public int indexBuffer = 0;


    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
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

            CreateMeshData();
            mesh.Clear();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();

            GetComponent<MeshCollider>().sharedMesh = mesh;

        }

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

    public void CreateVoxelData()
    {
        mesh = GetComponent<MeshCollider>().sharedMesh;
        data = new Voxel[resolution * resolution * resolution];
        bounds = mesh.bounds;
        voxelSize = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) / (resolution);
        
        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            Vector3Int i3d = Util.IndexToIndex3D(i, resolution);

            float xp = Util.ConvertRange(0, resolution, bounds.min.x, bounds.max.x, i3d.x);
            float yp = Util.ConvertRange(0, resolution, bounds.min.y, bounds.max.y, i3d.y);
            float zp = Util.ConvertRange(0, resolution, bounds.min.z, bounds.max.z, i3d.z);
            data[i] = new Voxel();
            data[i].position = new Vector3(xp, yp, zp);
            data[i].index = i;
            data[i].active = PointInMesh(data[i].position + transform.position);
        }

        if(BlocksGone())
        {
            Debug.Log("Mesh Data Generation Failed");
        }
        else
        {
            Debug.Log("Mesh Data Generation Succeded");
        }

    }

    public void CreateMeshData()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        indexBuffer = 0;

        for (int i = 0; i < resolution * resolution * resolution; i++)
        {
            if (data[i].active)
            {
                Vector3Int i3d = Util.IndexToIndex3D(i, resolution);

                if (i3d.x < resolution - 1)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x + 1, i3d.y, i3d.z), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadRight(data[i].position, voxelSize);
                    }
                }
                if (i3d.x > 0)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x - 1, i3d.y, i3d.z), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadLeft(data[i].position, voxelSize);
                    }
                }
                if (i3d.x == 0)
                {
                    CreateQuadLeft(data[i].position, voxelSize);
                }
                if (i3d.x == resolution - 1)
                {
                    CreateQuadRight(data[i].position, voxelSize);
                }

                if (i3d.y < resolution - 1)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x, i3d.y + 1, i3d.z), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadTop(data[i].position, voxelSize);
                    }
                }
                if (i3d.y > 0)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x, i3d.y - 1, i3d.z), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadBottom(data[i].position, voxelSize);
                    }
                }
                if (i3d.y == 0)
                {
                    CreateQuadBottom(data[i].position, voxelSize);
                }
                if (i3d.y == resolution - 1)
                {
                    CreateQuadTop(data[i].position, voxelSize);
                }

                if (i3d.z < resolution - 1)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z + 1), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadFront(data[i].position, voxelSize);
                    }
                }
                if (i3d.z > 0)
                {
                    int t = Util.Index3dToIndex(new Vector3Int(i3d.x, i3d.y, i3d.z - 1), resolution);
                    if (!data[t].active)
                    {
                        CreateQuadBack(data[i].position, voxelSize);
                    }
                }
                if (i3d.z == 0)
                {
                    CreateQuadBack(data[i].position, voxelSize);
                }
                if (i3d.z == resolution - 1)
                {
                    CreateQuadFront(data[i].position, voxelSize);
                }

            }
        }
        
    }

    void CreateQuadBack(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;
    }

    void CreateQuadFront(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;
    }

    void CreateQuadRight(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;
    }

    void CreateQuadLeft(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;
    }

    void CreateQuadBottom(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;

    }

    void CreateQuadTop(Vector3 position, Vector3 size)
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

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indexBuffer += 4;

    }


}
