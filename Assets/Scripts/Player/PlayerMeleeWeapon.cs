using UnityEngine;

public class PlayerMeleeWeapon : PlayerWeapon
{
    [Min(1)] public int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        if (slashing)
        {

        }
    }
}
