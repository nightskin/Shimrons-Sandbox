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
    float moveSpeed;
    [SerializeField] float normalSpeed = 25;
    [SerializeField] float boostSpeed = 100;

    //For Look/Aim
    [SerializeField] float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    //For Combat
    float atkAngle = 45;
    Vector2 atkDirection = Vector2.zero;
    public List<PlayerWeapon> inventory;
    int selected = 0;

    //For Collision
    CollisionFlags flags;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Use1.performed += Use1_performed;
        actions.Use2.performed += Use2_performed;
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
        actions.Use1.performed -= Use1_performed;
        actions.Use2.performed -= Use2_performed;
        actions.Dash.performed -= Dash_performed;
        actions.Dash.canceled -= Dash_canceled;
        actions.ToggleWeapons.performed -= ToggleWeapons_performed;
    }

    private void ToggleWeapons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(obj.ReadValue<float>() > 0)
        {
            if (selected < inventory.Count - 1)
            {
                selected++;
            }
            else
            {
                selected = 0;
            }
        }
        else if(obj.ReadValue<float>() < 0)
        {
            if(selected > 0)
            {
                selected--;
            }
            else 
            {
                selected = inventory.Count - 1;
            }
        }
    }

    private void Use1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (inventory[selected] is PlayerMeleeWeapon)
        {
            animator.SetTrigger("slash");
        }
    }

    private void Use2_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (inventory[selected] is PlayerMeleeWeapon)
        {
            animator.SetTrigger("slash");
        }
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
        atkDirection = actions.Look.ReadValue<Vector2>().normalized;
        if (atkDirection.magnitude > 0)
        {
            atkAngle = Mathf.Atan2(atkDirection.x, -atkDirection.y) * Mathf.Rad2Deg;
        }
    }

    private void Move()
    {
        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float y = actions.VerticalStrafe.ReadValue<float>();
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (head.transform.right * x + head.transform.forward * z + head.transform.up * y).normalized;
        flags = controller.Move(moveDirection * moveSpeed * Time.deltaTime);
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
        if(atkDirection.magnitude == 0)
        {
            atkAngle = Random.Range(0f, 360f);
        }

        inventory[selected].slashing = true;
        arm.localEulerAngles = new Vector3(0, 0, atkAngle);
    }

    public void EndSlash()
    {
        inventory[selected].slashing = false;
        arm.localEulerAngles = Vector3.zero;
    }

}
