using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    public Vector3 offset;
    World world;
    Voxel[] voxels = null;
    [SerializeField] float isoLevel = 0;
    [HideInInspector] public float radius;

    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;
    
    void Start()
    {
        world = transform.root.GetComponent<World>();
        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }
    
    public void Teraform(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / world.voxelSize) * world.voxelSize;
        pos.y = Mathf.Round(pos.y / world.voxelSize) * world.voxelSize;
        pos.z = Mathf.Round(pos.z / world.voxelSize) * world.voxelSize;
        int i = world.ToVoxelIndex(pos);
        
        //If voxel is already deactivated check the next one
        if(voxels[i].value > isoLevel)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * world.voxelSize / 2));
            pos.x = Mathf.Round(pos.x / world.voxelSize) * world.voxelSize;
            pos.y = Mathf.Round(pos.y / world.voxelSize) * world.voxelSize;
            pos.z = Mathf.Round(pos.z / world.voxelSize) * world.voxelSize; 
            i = world.ToVoxelIndex(pos);
            voxels[i].value -= Time.deltaTime;
        }
        else
        {
            voxels[i].value -= Time.deltaTime;
        }

        
        
        if (BlocksGone())
        {
            Destroy(gameObject);
        }
        else
        {

            verts.Clear();
            tris.Clear();
            uvs.Clear();
            buffer = 0;
            CreateMeshData();
            UpdateMesh();
        }
    }

    void CreateVoxelData()
    {
        radius = Random.Range(world.voxelSize * (world.voxelResolution - 1) / 4, world.voxelSize * (world.voxelResolution - 1) / 2);

        voxels = new Voxel[(int)Mathf.Pow(world.voxelResolution, 3)];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = world.ToPosition(i);
            voxels[i].value = world.Perlin3D(voxels[i].position + transform.position);
        }
    }
    
    void CreateMeshData()
    {
        for (int i = voxels.Length; i > 0; i--)
        {
            Vector3 position = world.ToPosition(i);
            Voxel[] points = new Voxel[]
            {
                    voxels[world.ToVoxelIndex(position + new Vector3(0,0,-1))],
                    voxels[world.ToVoxelIndex(position +  new Vector3(-1, 0, -1))],
                    voxels[world.ToVoxelIndex(position +  new Vector3(-1, 0, 0))],
                    voxels[world.ToVoxelIndex(position)],
                    voxels[world.ToVoxelIndex(position + new Vector3(0, -1, -1))],
                    voxels[world.ToVoxelIndex(position + new Vector3(-1,-1,-1))],
                    voxels[world.ToVoxelIndex(position + new Vector3(-1,-1, 0))],
                    voxels[world.ToVoxelIndex(position + new Vector3(0, -1, 0))]
            };
            
            int cubeIndex = Voxel.GetState(points, isoLevel);
            int[] triangulation = MarchingCubesTables.triTable[cubeIndex];

            Vector3[] triVerts = new Vector3[3];
            int triIndex = 0;

            foreach (int edgeIndex in triangulation)
            {
                if (edgeIndex > -1)
                {
                    int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                    int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                    Vector3 vertexPos = Voxel.LerpPoint(points[a], points[b], world.isoLevel);
                    verts.Add(vertexPos);
                    tris.Add(buffer);
                    
                    if(triIndex == 0)
                    {
                        triVerts[0] = vertexPos;
                        triIndex++;
                    }
                    else if(triIndex == 1)
                    {
                        triVerts[1] = vertexPos;
                        triIndex++;
                    }
                    else if(triIndex == 2)
                    {
                        triVerts[2] = vertexPos;
                        uvs.AddRange(world.GetUVs(triVerts[0], triVerts[1], triVerts[2]));
                        triIndex = 0;
                    }

                    buffer++;
                }
                else
                {
                    break;
                }
            }
        }

    }
    
    bool BlocksGone()
    {
        for (int i = 0; i < world.voxelResolution * world.voxelResolution * world.voxelResolution; i++)
        {
            if (voxels[i].value > isoLevel) return false;
        }

        return true;
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



}
