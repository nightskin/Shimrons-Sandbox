using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ChunkManager : MonoBehaviour
{
    public string seed = "";
    public bool getRidOfBlocksCuzTheySuck = true;
    public bool smoothing = false;
    public Gradient landGradient;
    public Color UndergroundColor = Color.yellow;

    public static int voxelsPerChunk = 64;
    public static float voxelSize = 2;

    public static Noise noise;
    public static float chunkSize = 0;
    public static float surfaceLevel = 16;
    public static float noiseScale = 0.0025f;

    [SerializeField] int maxChunkDistance = 10;
    [SerializeField] FirstPersonPlayer player;
    [SerializeField] GameObject chunkPrefab;

    CancellationTokenSource tokenSource;

    public static Dictionary<Vector3,Chunk> allChunks = new Dictionary<Vector3, Chunk>();

    void Awake()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        noise = new Noise(seed.GetHashCode());
        if (!player) player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonPlayer>();
        chunkSize = voxelsPerChunk * voxelSize;
    }

    void Start()
    {
        tokenSource = new CancellationTokenSource();
        for (int x = -maxChunkDistance; x <= maxChunkDistance; x++)
        {
            for (int z = -maxChunkDistance; z <= maxChunkDistance; z++)
            {
                Vector3 position = player.chunkLocation + new Vector3(x * chunkSize, 0, z * chunkSize);
                if(Vector3.Distance(position, player.chunkLocation) <= maxChunkDistance)
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
            for (int x = -maxChunkDistance; x <= maxChunkDistance; x++)
            {
                for (int z = -maxChunkDistance; z <= maxChunkDistance; z++)
                {
                    Vector3 position = player.chunkLocation + new Vector3(x * chunkSize, 0, z * chunkSize);
                    if(Vector3.Distance(position, player.chunkLocation) <= maxChunkDistance)
                    {
                        LoadChunk(position);
                    }
                }
            }
            foreach(Vector3 position in allChunks.Keys)
            {
                if (Vector3.Distance(position, player.chunkLocation) > maxChunkDistance)
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

    public static Vector3Int PositionToIndex(Vector3 pos)
    {
        float x = pos.x / voxelSize / voxelsPerChunk;
        float y = pos.y / voxelSize / voxelsPerChunk;
        float z = pos.z / voxelSize / voxelsPerChunk;

        x = Mathf.Abs(ConvertRange(0, 1, 0, voxelsPerChunk, x));
        y = Mathf.Abs(ConvertRange(0, 1, 0, voxelsPerChunk, y));
        z = Mathf.Abs(ConvertRange(0, 1, 0, voxelsPerChunk, z));

        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
    }
    
    public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }

    void CreateChunk(Vector3 chunkPosition)
    {
        var chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity, transform).GetComponent<Chunk>();
        chunk.gameObject.name = chunkPosition.ToString();
        

        allChunks.Add(chunkPosition, chunk);
        chunk.CreateVoxelData(chunkPosition);
        chunk.CreateMeshData(getRidOfBlocksCuzTheySuck);
        chunk.Draw();
    }

    async void CreateChunkAsync(Vector3 chunkPosition)
    {
        var chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity, transform).GetComponent<Chunk>();
        chunk.gameObject.name = chunkPosition.ToString();


        allChunks.Add(chunkPosition, chunk);
        var result = await Task.Run(() =>
        {
            chunk.CreateVoxelData(chunkPosition);
            chunk.CreateMeshData(getRidOfBlocksCuzTheySuck);
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
