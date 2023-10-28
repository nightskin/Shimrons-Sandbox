using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayer : MonoBehaviour
{
    //For Terrain
    public Chunk chunkLocation;


    // For Input
    Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    public Transform camera;
    [SerializeField] CharacterController controller;

    // For basic motion
    Vector3 moveDirection;
    public float speed = 15;


    //For Look/Aim
    public Vector2 lookSpeed = new Vector2(100f, 100);
    float xRot = 0;
    float yRot = 0;

    // For Jumping
    [SerializeField] float jumpHeight = 5;
    [SerializeField] Vector3 velocity = new Vector3();



    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
    }




    void Update()
    {
        Look();
        Movement();
    }

    void Movement()
    {
        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        //Move Normally
        moveDirection = camera.transform.right * x + camera.transform.forward * z;
        controller.Move(moveDirection * speed * Time.deltaTime);

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed.y * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed.x * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    
}
