using System.Collections.Generic;
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
    public List<PlayerWeapon> inventory;
    int inventoryIndex = 0;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Attack.performed += Attack_performed;
        actions.Attack.canceled += Attack_canceled;
        actions.Defend.performed += Defend_performed;
        actions.Defend.canceled += Defend_canceled;
        actions.Dash.performed += Dash_performed;
        actions.Dash.canceled += Dash_canceled;
        actions.ToggleWeapons.performed += ToggleWeapons_performed;
    }

    void Start()
    {
        moveSpeed = normalSpeed;
        if(!controller) controller = GetComponent<CharacterController>();
        if(!animator) animator = GetComponent<Animator>();


    }
    
    void Update()
    {
        Look();
        Move();
        Fight();
    }

    void OnDestroy()
    {
        actions.Attack.performed -= Attack_performed;
        actions.Attack.canceled -= Attack_canceled;
        actions.Defend.performed -= Defend_performed;
        actions.Defend.canceled -= Defend_canceled;
        actions.Dash.performed -= Dash_performed;
        actions.Dash.canceled -= Dash_canceled;
        actions.ToggleWeapons.performed -= ToggleWeapons_performed;
    }

    private void ToggleWeapons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(obj.ReadValue<float>() > 0)
        {
            if (inventoryIndex < inventory.Count - 1)
            {
                inventoryIndex++;
            }
            else
            {
                inventoryIndex = 0;
            }
        }
        else if(obj.ReadValue<float>() < 0)
        {
            if(inventoryIndex > 0)
            {
                inventoryIndex--;
            }
            else 
            {
                inventoryIndex = inventory.Count - 1;
            }
        }
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (inventory[inventoryIndex].function == PlayerWeapon.WeaponType.SLASHING)
        {
            animator.SetTrigger("slash");
        }
        else if (inventory[inventoryIndex].function == PlayerWeapon.WeaponType.THRUSTING)
        {

        }
        else if (inventory[inventoryIndex].function == PlayerWeapon.WeaponType.SHOOTING)
        {

        }
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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

    private void Fight()
    {
        if (inventory[inventoryIndex].function == PlayerWeapon.WeaponType.SLASHING)
        {
            if (actions.Attack.IsPressed())
            {
                Vector2 swingVector = actions.Look.ReadValue<Vector2>();
                if (swingVector.magnitude > 0)
                {
                    inventory[inventoryIndex].swingAngle = Mathf.Atan2(swingVector.x, -swingVector.y) * Mathf.Rad2Deg;
                }
                else
                {
                    inventory[inventoryIndex].swingAngle = Random.Range(0f, 360f);
                }
            }
        }


    }

    private void Move()
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
        inventory[inventoryIndex].swinging = true;
        arm.localEulerAngles = new Vector3(0, 0, inventory[inventoryIndex].swingAngle);
    }

    public void EndSlash()
    {
        inventory[inventoryIndex].swinging = false;
        arm.localEulerAngles = Vector3.zero;
    }

}
