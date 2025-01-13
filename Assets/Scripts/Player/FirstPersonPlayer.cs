using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{


    
    // For Input
    Controls controls;
    public Controls.PlayerActions actions;

    //Components
    Camera camera;
    CharacterController controller;
    ObjectPool bulletPool;

    // For basic motion
    Vector3 moveDirection;
    float speed;
    [SerializeField] float normalSpeed = 15;
    [SerializeField] float dashSpeed = 50;

    //For Look/Aim
    [SerializeField] float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    //For Shooting
    [SerializeField] Vector3 shootOffset = new Vector3(0, -0.1f, 0.1f);


    void Awake()
    {
        speed = normalSpeed;
        controller = GetComponent<CharacterController>();
        camera = Camera.main;
        bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Dash.performed += Dash_performed;
        actions.Dash.canceled += Dash_canceled;
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
        Vector3 shootPoint = camera.transform.position + shootOffset;
        GameObject p = bulletPool.Spawn(shootPoint);
        p.GetComponent<Bullet>().owner = gameObject;

        if (Physics.Raycast(shootPoint, camera.transform.forward, out RaycastHit hit ,camera.farClipPlane))
        {
            p.GetComponent<Bullet>().direction = (hit.point - shootPoint).normalized;
        }
        else
        {
            p.GetComponent<Bullet>().direction = camera.transform.forward;
        }
    }

    private void SecondaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {

    }

    void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        speed = dashSpeed;
    }

    private void Dash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        speed = normalSpeed;
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
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);

    }


}
