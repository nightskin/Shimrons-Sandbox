using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float blastRadius = 10;
    public float radius = 0.5f;
    public GameObject owner;
    public Vector3 direction;
    public float speed = 100;



    private Vector3 previousPosition;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void Start()
    {
        
    }

    void Update()
    {
        previousPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;
        
        CheckCollisions();  

        if(Vector3.Distance(transform.position, owner.transform.position) > Camera.main.farClipPlane)
        {
            gameObject.SetActive(false);
        }

    }

    void CheckCollisions()
    {
        float distance = Vector3.Distance(previousPosition, transform.position);
        if(Physics.SphereCast(previousPosition, radius, direction , out RaycastHit hit ,distance))
        {
            if(hit.transform.tag == "VoxelMesh")
            {
                hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, blastRadius);
                gameObject.SetActive(false);
            }
        }
    }

}
