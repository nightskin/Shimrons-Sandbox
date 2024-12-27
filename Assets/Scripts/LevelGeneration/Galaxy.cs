using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public string seed = "";
    [Min(1)] public Vector2Int size = Vector2Int.one;
    [Min(1)] public static float qaudrantSize = 250;
    public static Noise noise;

    [SerializeField] GameObject asteroidFieldPrefab;
    [SerializeField] GameObject planetPrefab;
    [SerializeField] GameObject enemyFleatPrefab;

    List<GameObject> quadrants;

    void Awake()
    {
        quadrants = new List<GameObject>();
        noise = new Noise(seed.GetHashCode());
        Random.InitState(seed.GetHashCode());
        
        for (int x = -size.x/2; x < size.x/2; x++) 
        {
            for (int z = -size.y/2; z < size.y; z++) 
            {
                int i = Random.Range(0, 4);
                // 0 == nothing
                // 1 == asteroid field
                // 2 == planet
                // 3 == enemyFleet
                if(i == 0)
                {
                    continue;
                }
                else if(i == 1)
                { 
                    if(asteroidFieldPrefab)
                    {
                        float y = noise.Evaluate(new Vector3(x, 0, z));
                        Vector3 pos = new Vector3(x, y, z) * qaudrantSize;
                        GameObject obj = Instantiate(asteroidFieldPrefab, pos, Quaternion.identity, transform);
                        quadrants.Add(obj);
                    }
                }
                else if(i == 2)
                {   
                    if(planetPrefab) 
                    {
                        float y = noise.Evaluate(new Vector3(x, 0, z));
                        Vector3 pos = new Vector3(x, y, z) * qaudrantSize;
                        GameObject obj = Instantiate(planetPrefab, pos, Quaternion.identity, transform);
                        quadrants.Add(obj);
                    } 
                }
                else if( i == 3)
                {
                    if(enemyFleatPrefab) 
                    {
                        float y = noise.Evaluate(new Vector3(x, 0, z));
                        Vector3 pos = new Vector3(x, y, z) * qaudrantSize;
                        GameObject obj = Instantiate(enemyFleatPrefab, pos, Quaternion.identity, transform);
                        quadrants.Add(obj);
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        
    }

}
