using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField] Transform player;
    public string seed = string.Empty;

    [SerializeField][Min(1)] int worldSizeInChunks = 1;


    [Range(2, 64)] public float maxHeight = 64;
    [Range(2, 63)] public float minHeight = 16;

    [SerializeField][Min(1)] float baseRoughness = 1.0f;
    [SerializeField][Min(1)] float roughness = 2.0f;
    [SerializeField][Range(0, 1)] float persistance = 0.5f;
    [SerializeField][Range(1, 10)] int layers = 1;


    [HideInInspector] public int voxelRes = 64;
    [HideInInspector] public float voxelSpacing = 10;


    [Range(0f, 1f)] public float isoLevel = 0.5f;
    [Range(0f, 1f)] public float noiseScale = 10;
    [Range(0f, 1f)] public float tunnelScale = 0.0001f;

    Noise noise;

    void Awake()
    {
        float chunkSize = voxelRes * voxelSpacing;
        noise = new Noise(seed.GetHashCode());
        Random.InitState(seed.GetHashCode());

        if(terrainChunkPrefab)
        {
            for (int x = -worldSizeInChunks; x < worldSizeInChunks; x++)
            {
                for (int z = -worldSizeInChunks; z < worldSizeInChunks; z++)
                {
                    var chunk = Instantiate(terrainChunkPrefab, new Vector3(x, 0, z) * (chunkSize - voxelSpacing), Quaternion.identity, transform);
                    chunk.name = x.ToString() + "," + z.ToString();
                    chunk.GetComponent<TerrainChunk>().offset = new Vector3(x, 0, z) * (voxelRes - 1);

                    if (x == 0 && z == 0)
                    {
                        //player.position = new Vector3(x, 0, z);
                    }

                }
            }
        }



    }
    


    float Perlin2D(float x, float z)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for(int i = 0; i < layers; i++)
        {
            float v = Mathf.Pow(noise.Evaluate(new Vector3(x, 0, z) * frequency), 4);
            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }


        return Util.ConvertRange(0, layers, 0, 1, noiseValue);
    }

    public float MakeTunnels(Vector3 point)
    {
        return Util.ConvertRange(-1, 1, 0, 1, noise.Evaluate(point * tunnelScale));
    }

    public float EvaluateHeight(Vector3 pos)
    {
        float gap = maxHeight - minHeight;
        float height = (Perlin2D(pos.x * noiseScale, pos.z * noiseScale) * gap) + minHeight;
        return Mathf.Clamp01(height - pos.y);
    }

}
