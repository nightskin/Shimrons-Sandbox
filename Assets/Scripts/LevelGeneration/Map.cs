using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Map : MonoBehaviour
{
    [SerializeField][Min(5)] int resolution = 50;
    [SerializeField][Min(0.01f)] float voxelSpacing = 50;

    [SerializeField] string seed;
    [SerializeField] int steps = 100;
    [SerializeField] bool indoor = true;

    float isoLevel = 0;

    public static Voxel[,,] voxels = null;
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        if (voxels == null)
        {
            if (seed == string.Empty) seed = Random.value.ToString();
            Random.InitState(seed.GetHashCode());
            GenerateVoxelData();
        }
        GenerateMeshData();
    }

    void GenerateVoxelData()
    {
        voxels = new Voxel[resolution + 1, resolution + 1, resolution + 1];
        for(int x = 0; x < resolution + 1; x++)
        {
            for (int y = 0; y < resolution + 1; y++)
            {
                for (int z = 0; z < resolution + 1; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].index = new Vector3Int(x, y, z);
                    voxels[x, y, z].position = new Vector3(x - (resolution + 1) / 2, y - (resolution + 1) / 2, z - (resolution + 1) / 2) * voxelSpacing;
                    if(indoor) voxels[x, y, z].value = 1;
                    else voxels[x,y,z].value = -1;
                }
            }
        }

        Vector3Int current = new Vector3Int(resolution / 2, resolution / 2, resolution / 2);
        for(int s = 0; s < steps; s++) 
        {
            ActivateBox(current, Vector3Int.one);
            int directionIndex = Random.Range(0, 6);
            Vector3Int direction = Vector3Int.zero;
            if (directionIndex == 0) direction = Vector3Int.forward;
            if (directionIndex == 1) direction = Vector3Int.back;
            if (directionIndex == 2) direction = Vector3Int.left;
            if (directionIndex == 3) direction = Vector3Int.right;
            if (directionIndex == 4) direction = Vector3Int.down;
            if (directionIndex == 5) direction = Vector3Int.up;

            current += direction;
        }

    }

    void GenerateMeshData()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.MarkDynamic();

        GetComponent<MeshFilter>().mesh = mesh;

        for (int x = resolution; x > 0; x--)
        {
            for (int y = resolution; y > 0; y--)
            {
                for (int z = resolution; z > 0; z--)
                {
                    Voxel[] corners = new Voxel[]
                    {
                        voxels[x,y,z-1],
                        voxels[x-1,y,z-1],
                        voxels[x-1,y,z],
                        voxels[x,y,z],
                        voxels[x,y-1,z-1],
                        voxels[x-1,y-1,z-1],
                        voxels[x-1,y-1,z],
                        voxels[x,y-1,z],
                    };
                    int cubeIndex = Util.GetState(corners, isoLevel);
                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if (edgeIndex > -1)
                        {
                            int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                            int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                            Vector3 vertexPos = Util.LerpPoint(corners[a], corners[b], isoLevel);
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

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        for (int v = 0; v < verts.Count - 2; v += 3)
        {
            Vector2[] uvForTri = Util.GetUVs(verts[v], verts[v + 1], verts[v + 2]);
            uvs.AddRange(uvForTri);
        }
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void ActivateBox(Vector3Int index, Vector3Int size)
    {
        if (voxels == null) return;

        for (int x = -size.x; x < size.x; x++)
        {
            for (int y = -size.y; y < size.y; y++)
            {
                for (int z = -size.z; z < size.z; z++)
                {
                    if (index.x + x > 0 && index.x + x < resolution)
                    {
                        if (index.y + y > 0 && index.y + y < resolution)
                        {
                            if (index.z + z > 0 && index.z + z < resolution)
                            {
                               if(indoor) voxels[index.x + x, index.y + y, index.z + z].value = -1;
                               else voxels[index.x + x, index.y + y, index.z + z].value = 1;
                            }
                        }
                    }
                }
            }
        }

    }

}
