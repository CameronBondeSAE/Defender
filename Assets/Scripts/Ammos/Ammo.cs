using UnityEngine;

public abstract class Ammo : ScriptableObject, IAmmo
{
    public string ammoName;
    public float damage;
    public float range;
    public abstract void Hit(GameObject target);
}
