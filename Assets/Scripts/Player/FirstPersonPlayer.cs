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
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit))
        {
            if (hit.transform.tag == "Asteroid")
            {
                hit.transform.GetComponent<Voxelizer>().Teraform(hit.point, 5);
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
