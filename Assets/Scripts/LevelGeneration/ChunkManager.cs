using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static string seed;
    public static Noise noise;

    static GameObject player;
    [SerializeField] GameObject chunkPrefab;
    static int chunksX = 4;
    static int chunksZ = 4;

    static Vector3 chunkOffset = new Vector3();
    public static Chunk[,] chunks;

    void Awake()
    {
        seed = System.DateTime.Now.ToString();
        noise = new Noise(seed.GetHashCode());
        if (!player) player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        Chunk chunk = chunkPrefab.GetComponent<Chunk>();
        chunkOffset.x = chunk.tilesX * chunk.tileSize - 10;
        chunkOffset.z = chunk.tilesZ * chunk.tileSize - 10;

        chunks = new Chunk[chunksX, chunksZ];

        for (int x = 0; x < chunksX; x++)
        {
            for (int z = 0; z < chunksZ; z++)
            {
                Vector3 pos = player.transform.position + new Vector3(x * chunkOffset.x, 0, z * chunkOffset.z);
                var c = Instantiate(chunkPrefab, pos, Quaternion.identity);
                c.GetComponent<Chunk>().center = pos;
                c.name = x.ToString() + z.ToString();
                chunks[x, z] = c.GetComponent<Chunk>();
            }
        }
    }

    public static void RegenerateChunks()
    {
        
    }


    void Update()
    {
        
    }

}
