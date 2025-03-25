using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet", menuName = "Ammos/GrenadeData")]
public class GrenadeData : AmmoData
{
    public float explosionRadius;
    public float explosionDelay;
    public float speed;
}
