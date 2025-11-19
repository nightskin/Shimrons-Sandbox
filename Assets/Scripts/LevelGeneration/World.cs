using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField] Transform player;
    public string seed = string.Empty;

    [Min(1)] public int worldSizeInChunks = 1;

    static float isoLevel = 0;
    [Min(1)] public int chunkResolution = 32;
    [Min(0)] public float voxelSize = 10;

    float noiseScale;

    static Noise noise;

    void Awake()
    {
        float chunkSize = chunkResolution * voxelSize;
        noise = new Noise(seed.GetHashCode());
        noiseScale = 1 / voxelSize / chunkResolution;
        if (terrainChunkPrefab)
        {
            for (int x = -worldSizeInChunks; x < worldSizeInChunks; x++)
            {
                for (int y = -worldSizeInChunks; y < worldSizeInChunks; y++)
                {
                    for (int z = -worldSizeInChunks; z < worldSizeInChunks; z++)
                    {
                        var chunk = Instantiate(terrainChunkPrefab, new Vector3(x, y, z) * (chunkSize - voxelSize), Quaternion.identity, transform);
                        chunk.name = x.ToString() + "," + y.ToString() + "," + z.ToString();
                    }
                }
            }
        }



    }
    
    public bool Perlin3D(Vector3 position)
    {
        float v = noise.Evaluate(position * noiseScale);
        if (v > isoLevel) return true;
        else return false;
    }

}
