using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletShape
    {
        NONE,
        SPHERE,
        BOX
    }
    public BulletShape shape;

    public float lifeTime = 3;
    public float blastRadius = 10;
    public GameObject owner;
    public Vector3 direction;
    public float speed = 100;
    
    private Vector3 previousPosition;
    private float timer;

    void Awake()
    {
        if(GetComponent<SphereCollider>())
        {
            shape = BulletShape.SPHERE;
        }
        else if(GetComponent<BoxCollider>()) 
        {
            shape = BulletShape.BOX;
        }
        else
        {
            shape = BulletShape.NONE;
        }
    }

    void OnEnable()
    {
        GetComponent<TrailRenderer>().Clear();
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

        if(shape == BulletShape.NONE)
        {
            if (Physics.Raycast(previousPosition, direction, out RaycastHit hit, distance))
            {
                if (hit.transform.tag == "Asteroid")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, blastRadius);
                    gameObject.SetActive(false);
                }
                else if (hit.transform.tag == "Planet")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, 1);
                    gameObject.SetActive(false);
                }
            }
        }
        else if(shape == BulletShape.SPHERE) 
        {
            if (Physics.SphereCast(previousPosition, GetComponent<SphereCollider>().radius, direction, out RaycastHit hit, distance))
            {
                if (hit.transform.tag == "Asteroid")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, blastRadius);
                    gameObject.SetActive(false);
                }
                else if (hit.transform.tag == "Planet")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, 1);
                    gameObject.SetActive(false);
                }
            }
        }
        else if(shape == BulletShape.BOX)
        {
            if (Physics.BoxCast(previousPosition, GetComponent<BoxCollider>().size, direction,out RaycastHit hit, transform.rotation ,distance))
            {
                if (hit.transform.tag == "Asteroid")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, blastRadius);
                    gameObject.SetActive(false);
                }
                else if (hit.transform.tag == "Planet")
                {
                    hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, 1);
                    gameObject.SetActive(false);
                }
            }
        }

    }

}
