using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayer : MonoBehaviour
{
    //For Terrain
    public Vector3 prevChunkLocation;
    public Vector3 chunkLocation;


    // For Input
    Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    public Transform camera;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform groundCheck;

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
    [SerializeField] float gravity = -9.81f;
    [SerializeField] LayerMask groundMask;

    bool isGrounded = false;
    float groundDistance = 0.4f;



    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Jump.performed += Jump_performed;
        actions.PrimaryFire.performed += PrimaryFire_performed;
        actions.SecondaryFire.performed += SecondaryFire_performed;
    }

    void Update()
    {
        Look();
        Movement();
    }

    private void PrimaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(camera.position, camera.forward, out RaycastHit hit))
        {

        }

    }

    private void SecondaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, 500, groundMask))
        {
            if(hit.collider.tag == "Ground")
            {
                Chunk chunk = hit.collider.GetComponent<Chunk>();
                if(chunk)
                {
                    chunk.RemoveBlock(hit.point);
                }
            }
        }
    }

    void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        //Move Normally
        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * speed * Time.deltaTime);

        //Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);



        if(actions.Move.ReadValue<Vector2>().magnitude > 0)
        {
            prevChunkLocation = chunkLocation;

            //Calculate ChunkPos
            float chunkPosX = Mathf.Round(transform.position.x / ChunkManager.chunkSize) * ChunkManager.chunkSize;
            float chunkPosZ = Mathf.Round(transform.position.z / ChunkManager.chunkSize) * ChunkManager.chunkSize;
            chunkLocation = new Vector3(chunkPosX, 0, chunkPosZ);
        }



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
