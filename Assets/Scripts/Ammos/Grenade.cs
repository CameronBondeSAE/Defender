using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet", menuName = "Ammos/Grenade")]
public class Grenade : Ammo
{
    public float explosionRadius;
    public float explosionForce;
    public override void Hit(GameObject target)
    {
        
    }
}
