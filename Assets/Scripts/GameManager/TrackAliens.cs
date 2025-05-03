using UnityEngine;

public class TrackAliens : MonoBehaviour
{
    public int remainingAliens;

    public void RegisterAllAliens()
    {
        GameObject[] Aliens = GameObject.FindGameObjectsWithTag("Alien");
        remainingAliens = Aliens.Length;

        foreach (GameObject Alien in Aliens)
        {
            Health health = Alien.GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath -= OnAlienDeath;
                health.OnDeath += OnAlienDeath;
            }
        }
    }

    private void OnAlienDeath()
    {
        remainingAliens--;

        if (remainingAliens <= 0)
        {
            GameManager.Instance.UpdateGameState(GameState.Win);
        }
    }
}
