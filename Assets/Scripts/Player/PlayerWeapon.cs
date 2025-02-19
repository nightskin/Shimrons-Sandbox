using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public enum WeaponType
    {
        SHOOTING,
        SLASHING,
        THRUSTING,
    }

    public WeaponType function;
    public int damage = 1;


    public bool swinging = false;

    void OnTriggerEnter(Collider other)
    {
        if(swinging)
        {
            
        }
    }

}
