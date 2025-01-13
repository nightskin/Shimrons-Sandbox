using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3;
    public float blastRadius = 10;
    public float radius = 0.5f;
    public GameObject owner;
    public Vector3 direction;
    public float speed = 100;



    private Vector3 previousPosition;
    private float timer;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void OnEnable()
    {
        timer = lifeTime;
    }

    void Update()
    {
        previousPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;
        
        CheckCollisions();  

        timer -= Time.deltaTime;
        if(timer < 0) 
        {
            gameObject.SetActive(false);
        }

    }

    void CheckCollisions()
    {
        float distance = Vector3.Distance(previousPosition, transform.position);
        if(Physics.SphereCast(previousPosition, radius, direction , out RaycastHit hit ,distance))
        {
            if(hit.transform.tag == "Asteroid")
            {
                hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, blastRadius);
                gameObject.SetActive(false);
            }
            else if(hit.transform.tag == "Planet")
            {
                hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, 1);
                gameObject.SetActive(false);
            }
        }
    }

}
