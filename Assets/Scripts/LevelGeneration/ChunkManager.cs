using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public string seed = "";

    public static Noise noise;
    public static float chunkSize = 0;
    public static bool useVoxels = false;

    [SerializeField] float maxViewDistance = 500;
    [SerializeField] FirstPersonPlayer player;
    [SerializeField] GameObject chunkPrefab;
    
    int chunksXZ;
    

    public static Dictionary<Vector3,Chunk> chunks = new Dictionary<Vector3, Chunk>();

    void Awake()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        noise = new Noise(seed.GetHashCode());
        if (!player) player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonPlayer>();
        chunkSize = Chunk.tilesXZ * Chunk.tileSize - 10;
        chunksXZ = Mathf.RoundToInt(maxViewDistance / chunkSize);

    }

    void Start()
    {
        for (int x = -chunksXZ; x <= chunksXZ; x++)
        {
            for (int z = -chunksXZ; z <= chunksXZ; z++)
            {
                Vector3 position = player.transform.position + new Vector3(x * chunkSize, 0, z * chunkSize);
                CreateChunk(position);
            }
        }

        foreach (Vector3 chunkPosition in chunks.Keys)
        {
            if (Vector3.Distance(chunkPosition, player.chunkLocation) > maxViewDistance)
            {
                UnloadChunk(chunkPosition);
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
                    Vector3 position = player.transform.position + new Vector3(x * chunkSize, 0, z * chunkSize);
                    LoadChunk(position);
                }
            }

            foreach(Vector3 chunkPosition in chunks.Keys)
            {
                if(Vector3.Distance(chunkPosition, player.chunkLocation) > maxViewDistance)
                {
                    UnloadChunk(chunkPosition);
                }
            }
        }
    }

    void CreateChunk(Vector3 position)
    {
        var c = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
        c.GetComponent<Chunk>().center = position;
        c.name = position.ToString();
        chunks.Add(position, c.GetComponent<Chunk>());
    }

    void LoadChunk(Vector3 position)
    {
        if(chunks.ContainsKey(position))
        {
            chunks[position].gameObject.SetActive(true);
        }
        else
        {
            CreateChunk(position);
        }
    }
    
    void UnloadChunk(Vector3 position)
    {
        if (chunks.ContainsKey(position))
        {
            chunks[position].gameObject.SetActive(false);
        }
    }
}
