using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayer : MonoBehaviour
{
    // For Input
    Controls controls;
    Controls.PlayerActions actions;

    //Components
    [SerializeField] CharacterController controller;
    [SerializeField] Animator animator;
    [SerializeField] Transform body;
    [SerializeField] Transform head;
    [SerializeField] Transform arm;
    [SerializeField] Image reticle;

    // For basic motion
    Vector3 moveDirection;
    Vector3 velocity = Vector3.zero;
    float moveSpeed;
    [SerializeField] float normalSpeed = 25;
    [SerializeField] float boostSpeed = 100;

    //For Look/Aim
    [SerializeField] float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    //For Combat
    public PlayerWeapon currentWeapon;

    void Start()
    {
        moveSpeed = normalSpeed;
        if(!controller) controller = GetComponent<CharacterController>();
        if(!animator) animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.PrimaryFire.performed += PrimaryFire_performed;
        actions.PrimaryFire.canceled += PrimaryFire_canceled;
        actions.SecondaryFire.performed += SecondaryFire_performed;
        actions.SecondaryFire.canceled += SecondaryFire_canceled;
        actions.Dash.performed += Dash_performed;
        actions.Dash.canceled += Dash_canceled;
    }
    
    void Update()
    {
        Look();
        Movement();

        if(currentWeapon.type == PlayerWeapon.WeaponType.SLASHING && actions.PrimaryFire.IsPressed())
        {
            Vector2 swingVector = actions.Look.ReadValue<Vector2>();
            if(swingVector.magnitude > 0)
            {
                currentWeapon.swingAngle = Mathf.Atan2(swingVector.x, -swingVector.y) * Mathf.Rad2Deg;
            }
            else
            {
                currentWeapon.swingAngle = Random.Range(-180f, 180f);
            }
        }

    }

    void OnDestroy()
    {
        actions.PrimaryFire.performed -= PrimaryFire_performed;
        actions.PrimaryFire.canceled -= PrimaryFire_canceled;
        actions.SecondaryFire.performed -= SecondaryFire_performed;
        actions.SecondaryFire.canceled -= SecondaryFire_canceled;
        actions.Dash.performed -= Dash_performed;
        actions.Dash.canceled -= Dash_canceled;
    }

    private void PrimaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(currentWeapon.type == PlayerWeapon.WeaponType.SLASHING)
        {
            animator.SetTrigger("slash");
        }
        else if(currentWeapon.type == PlayerWeapon.WeaponType.THRUSTING)
        {

        }
        else if(currentWeapon.type == PlayerWeapon.WeaponType.SHOOTING)
        {

        }
    }

    private void PrimaryFire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void SecondaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void SecondaryFire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        moveSpeed = boostSpeed;
    }

    private void Dash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        moveSpeed = normalSpeed;
    }

    private void Movement()
    {
        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float y = actions.VerticalStrafe.ReadValue<float>();
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (head.transform.right * x + head.transform.forward * z + head.transform.up * y).normalized;
        
        if(moveDirection.magnitude > 0)
        {
            velocity = moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, 6 * Time.deltaTime);
        }
        
        controller.Move(velocity);
    }

    private void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        
        //Looking up/down
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);

        //Looking left/right
        yRot += x * lookSpeed * Time.deltaTime;

        body.localRotation = Quaternion.Euler(0, yRot, 0);
        head.localRotation = Quaternion.Euler(xRot, 0, 0);
    }


    //Animation Events
    public void StartSlash()
    {
        currentWeapon.swinging = true;
        arm.localEulerAngles = new Vector3(0, 0, currentWeapon.swingAngle);
    }

    public void EndSlash()
    {
        currentWeapon.swinging = false;
        arm.localEulerAngles = Vector3.zero;
    }

}
