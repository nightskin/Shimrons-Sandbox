using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // For Input
    Controls controls;

    //Components
    [SerializeField] CharacterController controller;

    // For basic motion
    [SerializeField] float speed = 25.0f;

    //For Look/Aim
    [SerializeField] float lookSpeed = 100;
    [SerializeField][Range(0, 1)] float lookThreshold = 0.1f;

    void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }

    void Start()
    {
        if(!controller) controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        Look();
        Move();
    }

    
    void Move()
    {
        //Basic Motion
        float x = controls.Player.Move.ReadValue<Vector2>().x;
        float y = controls.Player.VerticalStrafe.ReadValue<float>();
        float z = controls.Player.Move.ReadValue<Vector2>().y;

        Vector3 moveDirection = (transform.right * x + transform.forward * z + transform.up * y).normalized;
        controller.Move(moveDirection * speed * Time.deltaTime);
    }

    void Look()
    {
        float x = controls.Player.Look.ReadValue<Vector2>().x;
        float y = controls.Player.Look.ReadValue<Vector2>().y;
        float z = controls.Player.LookZ.ReadValue<float>();


        if(x > lookThreshold || x < -lookThreshold) transform.rotation *= Quaternion.AngleAxis(lookSpeed * x * Time.deltaTime, Vector3.up);
        if(y > lookThreshold || y < -lookThreshold) transform.rotation *= Quaternion.AngleAxis(lookSpeed * y * Time.deltaTime, Vector3.left);
        if(z > lookThreshold || z < -lookThreshold) transform.rotation *= Quaternion.AngleAxis(lookSpeed * z * Time.deltaTime, Vector3.forward);

    }

}
