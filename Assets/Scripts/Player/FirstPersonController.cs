using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // For Input
    Controls controls;

    //Components
    [SerializeField] CharacterController controller;
    [SerializeField] Camera camera;

    // For basic motion
    [SerializeField] float speed = 25.0f;
    //For Look/Aim
    [SerializeField] float lookSpeed = 100;

    void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }

    void Start()
    {
        if(!controller) controller = GetComponent<CharacterController>();
        if(!camera) camera = Camera.main;
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
        float y = controls.Player.MoveY.ReadValue<float>();
        float z = controls.Player.Move.ReadValue<Vector2>().y;

        Vector3 moveDirection = (camera.transform.right * x + camera.transform.forward * z + camera.transform.up * y).normalized;
        controller.Move(moveDirection * speed * Time.deltaTime);
    }

    void Look()
    {
        float x = controls.Player.Look.ReadValue<Vector2>().x;
        float y = controls.Player.Look.ReadValue<Vector2>().y;
        float z = controls.Player.LookZ.ReadValue<float>();

        transform.rotation *= Quaternion.AngleAxis(x * lookSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(y * lookSpeed * Time.deltaTime, Vector3.left);
        transform.rotation *= Quaternion.AngleAxis(z * lookSpeed * Time.deltaTime, Vector3.forward);
    }

}
