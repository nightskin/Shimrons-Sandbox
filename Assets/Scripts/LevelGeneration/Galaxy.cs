using NUnit.Framework;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public string seed = "";
    [Min(1)] public Vector2Int size = Vector2Int.one;
    [Min(1)] public static float qaudrantSize = 250;
    public static Noise noise;

    [SerializeField] GameObject asteroidFieldPrefab;

    void Awake()
    {
        noise = new Noise(seed.GetHashCode());
        Random.InitState(seed.GetHashCode());

        for (int x = -size.x/2; x < size.x/2; x++) 
        {
            for (int z = -size.y/2; z < size.y; z++) 
            {
                int i = Random.Range(0, 2);

                if(i == 1)
                { 
                    if(asteroidFieldPrefab)
                    {
                        float y = noise.Evaluate(new Vector3(x, 0, z));
                        Vector3 pos = new Vector3(x, y, z) * qaudrantSize;
                        GameObject obj = Instantiate(asteroidFieldPrefab, pos, Quaternion.identity, transform);
                        obj.isStatic = true;
                    }
                }
            }
        }

    }

}
