using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayer : MonoBehaviour
{
    //For Terrain
    [SerializeField] ChunkManager chunkManager;
    public Vector3 prevChunkLocation;
    public Vector3 chunkLocation;


    // For Input
    Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    public Transform camera;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraBob;

    // For basic motion
    Vector3 moveDirection;
    public float speed = 15;


    //For Look/Aim
    public float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;


    void Awake()
    {
        chunkManager = GameObject.Find("ChunkManager").GetComponent<ChunkManager>();
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Dash.performed += Dash_performed;
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
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, 300))
        {
            if(hit.transform.GetComponent<Chunk>())
            {
                Vector3 pointInTargetBlock = hit.point;
                int chunkSize = chunkManager.voxelsPerChunk * chunkManager.voxelSize;

                int chunkPosX = Mathf.FloorToInt(pointInTargetBlock.x / chunkManager.voxelsPerChunk) * chunkManager.voxelsPerChunk;
                int chunkPosZ = Mathf.FloorToInt(pointInTargetBlock.z / chunkManager.voxelsPerChunk) * chunkManager.voxelsPerChunk;

                Chunk chunk = ChunkManager.allChunks[new Vector3(chunkPosX, 0, chunkPosZ)];
                
                int x = Mathf.FloorToInt(pointInTargetBlock.x) - (chunkPosX+1);
                int y = Mathf.FloorToInt(pointInTargetBlock.y);
                int z = Mathf.FloorToInt(pointInTargetBlock.z) - (chunkPosZ+1);


                chunk.RemoveBlock(x, y, z);
            }
        }
    }

    private void SecondaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {

    }

    void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    void Movement()
    {

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float y = actions.StrafeY.ReadValue<float>();
        float z = actions.Move.ReadValue<Vector2>().y;

        //Move Normally
        moveDirection = camera.transform.right * x + camera.transform.forward * z + camera.transform.up * y;
        controller.Move(moveDirection * speed * Time.deltaTime);


        if (actions.Move.ReadValue<Vector2>().magnitude > 0)
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

        //Looking up/down
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);

        //Looking left/right
        yRot += x * lookSpeed * Time.deltaTime;

        transform.localEulerAngles = new Vector3(0, yRot, 0);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);

    }

    
}
