using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public string seed = "";
    public bool getRidOfBlocksCuzTheySuck = true;
    public Gradient landGradient;
    public float maxHeight = 16;
    public int tilesPerChunkXZ = 32;
    public int tilesPerChunkY = 32;
    public int tileSize = 3;

    public static Noise noise;
    public static float chunkSize = 0;
    public float noiseScale = 0.0025f;
    public float noiseThreshold = 0.5f;

    float maxViewDistance;
    [SerializeField] FirstPersonPlayer player;
    [SerializeField] GameObject chunkPrefab;

    CancellationTokenSource tokenSource;

    int chunksXZ;
    public static Dictionary<Vector3,Chunk> allChunks = new Dictionary<Vector3, Chunk>();

    void Awake()
    {
        maxViewDistance = Camera.main.farClipPlane;
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        noise = new Noise(seed.GetHashCode());
        if (!player) player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonPlayer>();
        chunkSize = tilesPerChunkXZ * tileSize - 10;
        chunksXZ = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Start()
    {
        tokenSource = new CancellationTokenSource();
        for (int x = -chunksXZ; x <= chunksXZ; x++)
        {
            for (int z = -chunksXZ; z <= chunksXZ; z++)
            {
                Vector3 position = player.chunkLocation + new Vector3(x * chunkSize, 0, z * chunkSize);
                if(Vector3.Distance(position, player.chunkLocation) <= maxViewDistance)
                {
                    CreateChunk(position);
                }
            }
        }
    }

    void Update()
    {
        if(player.prevChunkLocation != player.chunkLocation)
        {
            for (int x = -chunksXZ; x <= chunksXZ; x++)
            {
                for (int z = -chunksXZ; z <= chunksXZ; z++)
                {
                    Vector3 position = player.chunkLocation + new Vector3(x * chunkSize, 0, z * chunkSize);
                    if(Vector3.Distance(position, player.chunkLocation) <= maxViewDistance)
                    {
                        LoadChunk(position);
                    }
                }
            }
            foreach(Vector3 position in allChunks.Keys)
            {
                if (Vector3.Distance(position, player.chunkLocation) > maxViewDistance)
                {
                    UnloadChunk(position);
                }
            }
        }
    }

    void OnDisable()
    {
        tokenSource.Cancel();
    }

    void CreateChunk(Vector3 chunkPosition)
    {
        var chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity, transform).GetComponent<Chunk>();
        allChunks.Add(chunkPosition, chunk);
        chunk.CreateMeshData(chunkPosition);
        chunk.Generate(getRidOfBlocksCuzTheySuck);
        chunk.Draw();
    }

    async void CreateChunkAsync(Vector3 chunkPosition)
    {
        var chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity, transform).GetComponent<Chunk>();
        allChunks.Add(chunkPosition, chunk);
        var result = await Task.Run(() =>
        {
            chunk.CreateMeshData(chunkPosition);
            chunk.Generate(getRidOfBlocksCuzTheySuck);
            if (tokenSource.IsCancellationRequested)
            {
                return chunk;
            }
            return chunk;
        }, tokenSource.Token);
        chunk.Draw();
        if (tokenSource.IsCancellationRequested)
        {
            return;
        }

    }

    void LoadChunk(Vector3 chunkPosition)
    {
        if (allChunks.ContainsKey(chunkPosition))
        {
            allChunks[chunkPosition].gameObject.SetActive(true);
        }
        else
        {
            CreateChunkAsync(chunkPosition);
        }
    }

    void UnloadChunk(Vector3 chunkPosition)
    {
        if (allChunks.ContainsKey(chunkPosition))
        {
            allChunks[chunkPosition].gameObject.SetActive(false);
        }
    }
}
