using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField][Min(1)] float baseRoughness = 1.0f;
    [SerializeField][Min(1)] float roughness = 2.0f;
    [SerializeField][Range(0, 1)] float persistance = 0.5f;
    [SerializeField][Min(1)] int layers = 1;

    public string seed = string.Empty;

    [SerializeField][Range(1,64)] float maxHeight = 32;
    [SerializeField][Range(0,63)] float minHeight = 15;

    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField] Transform player;
    [SerializeField][Min(1)] int worldSizeInChunks = 1;

    [HideInInspector] public int voxelRes = 64;
    [HideInInspector] public float voxelSpacing = 10;


    public float isoLevel = 0.5f;
    public float noiseScale = 10;

    public static Noise noise;

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
                }
            }
        }
    }
    
    float BetterPerlin2D(float x, float z)
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

    public float EvaluateHeight(Vector3 pos)
    {
        float gap = maxHeight - minHeight;
        float height = (BetterPerlin2D(pos.x * noiseScale, pos.z * noiseScale) * gap) + minHeight;
        return Mathf.Clamp01(height - pos.y);
    }

}
