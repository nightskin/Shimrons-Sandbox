using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public string seed = "";
    public static int voxelsInSingleChunk = 64;
    public static float voxelSize = 1;
    public static Noise noise;


    public static float noiseScale = 0.01f;
    public static float strength = 0.25f;
    public static float baseRoughness = 1;
    public static float roughness = 2;
    public static float persistance = 0.5f;
    public static float minValue = 0.25f;
    public static int layers = 2;


    [SerializeField] GameObject player;
    [SerializeField] GameObject chunkPrefab;
    Dictionary<Vector3, GameObject> chunks;

    void Start()
    {
        noise = new Noise(seed.GetHashCode());
        if (!player) player = GameObject.FindWithTag("Player");


        

    }

    
    void FixedUpdate()
    {
        
    }
}
