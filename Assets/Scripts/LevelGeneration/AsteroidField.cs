using UnityEngine;

public class AsteroidField : MonoBehaviour
{
    [SerializeField][Min(1)] int minAsteroids = 5;
    [SerializeField][Min(2)] int maxAsteroids = 10;
    [SerializeField] int numberOfAsteroids;
    [SerializeField] GameObject[] asteroidPrefabs;
    
    void Start()
    {
        numberOfAsteroids = Random.Range(minAsteroids, maxAsteroids + 1);
        for (int i = 0; i < numberOfAsteroids; i++)
        {
            Vector3 pos = Random.insideUnitSphere * Galaxy.qaudrantSize;
            Quaternion rot = Random.rotation;
            Vector3 scale = Vector3.one * Random.Range(1.0f, 5.0f);

            int a = Random.Range(0, asteroidPrefabs.Length);
            GameObject obj = Instantiate(asteroidPrefabs[a], transform);
            obj.GetComponent<Voxelizer>().transform.localPosition = pos;
            obj.GetComponent<Voxelizer>().rotation = rot;
            obj.GetComponent<Voxelizer>().scale = scale;
            
        }
    }

}
