using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField] Transform player;
    public string seed = string.Empty;

    [SerializeField][Min(1)] int worldSizeInChunks = 1;


    public int voxelResolution = 64;
    public float voxelSize = 10;


    [Range(0f, 1f)] public float isoLevel = 0.5f;
    [Range(0f, 1f)] public float noiseScale = 10;

    Noise noise;

    void Start()
    {
        float chunkSize = voxelResolution * voxelSize;
        noise = new Noise(seed.GetHashCode());

        if(terrainChunkPrefab)
        {
            for (int x = -worldSizeInChunks; x < worldSizeInChunks; x++)
            {
                for(int y = -worldSizeInChunks; y < worldSizeInChunks; y++)
                {
                    for (int z = -worldSizeInChunks; z < worldSizeInChunks; z++)
                    {
                        var chunk = Instantiate(terrainChunkPrefab, new Vector3(x, y, z) * (chunkSize - voxelSize), Quaternion.identity, transform);
                        chunk.name = x.ToString() + "," + y.ToString() + "," + z.ToString();
                        chunk.GetComponent<TerrainChunk>().offset = new Vector3(x, y, z) * (voxelResolution - 1);

                    }
                }

            }
        }



    }

    void FixedUpdate()
    {
        
    }

    public float Perlin3D(Vector3 position)
    {
       return noise.Evaluate(position * noiseScale);
    }

    public int ToVoxelIndex(Vector3 position)
    {
        return ((int)(position.x / voxelSize)) + ((int)(position.y / voxelSize) * voxelResolution) + ((int)(position.z / voxelSize) * voxelResolution * voxelResolution);
    }

    public Vector3 ToPosition(int i)
    {
        int x = i % voxelResolution;
        int y = i / voxelResolution % voxelResolution;
        int z = i / voxelResolution / voxelResolution % voxelResolution;
        return new Vector3(x, y, z) * voxelSize;
    }

    public Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y) / voxelSize;
            uvs[1] = new Vector2(b.z, b.y) / voxelSize;
            uvs[2] = new Vector2(c.z, c.y) / voxelSize;
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y) / voxelSize;
            uvs[1] = new Vector2(b.x, b.y) / voxelSize;
            uvs[2] = new Vector2(c.x, c.y) / voxelSize;
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z) / voxelSize;
            uvs[1] = new Vector2(b.x, b.z) / voxelSize;
            uvs[2] = new Vector2(c.x, c.z) / voxelSize;
        }

        return uvs;
    }
}
