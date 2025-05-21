using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    public Vector3 offset;

    World world;
    Voxel[,,] voxels = null;
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        world = transform.parent.GetComponent<World>();
        if (voxels == null)
        {
            GenerateVoxelData();
        }
        GenerateMeshData();
    }

    void GenerateVoxelData()
    {
        voxels = new Voxel[world.voxelRes, world.voxelRes, world.voxelRes];
        
        for(int x = 0; x < world.voxelRes; x++)
        {
            for (int y = 0; y < world.voxelRes; y++)
            {
                for (int z = 0; z < world.voxelRes; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].position = new Vector3(x, y, z) * world.voxelSpacing;
                    voxels[x, y, z].value = world.EvaluateHeight(new Vector3(x, y, z) + offset);

                    if (y < world.minHeight)
                    {
                        voxels[x, y, z].value -= world.MakeTunnels(new Vector3(x, y, z) + offset);
                    }

                }
            }
        }
    }
    
    void GenerateMeshData()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.MarkDynamic();

        GetComponent<MeshFilter>().mesh = mesh;

        for (int x = world.voxelRes - 1; x > 0; x--)
        {
            for (int y = world.voxelRes - 1; y > 0; y--)
            {
                for (int z = world.voxelRes - 1; z > 0; z--)
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
                    int cubeIndex = Util.GetState(corners, world.isoLevel);
                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if (edgeIndex > -1)
                        {
                            int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                            int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                            Vector3 vertexPos = Util.LerpPoint(corners[a], corners[b], world.isoLevel);
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

    

}
