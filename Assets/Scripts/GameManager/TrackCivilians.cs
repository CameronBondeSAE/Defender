using UnityEngine;

public class TrackCivilians : MonoBehaviour
{
    public int remainingCivilians;

    public void RegisterAllCivilians()
    {
        GameObject[] civilians = GameObject.FindGameObjectsWithTag("Civilian");
        remainingCivilians = civilians.Length;

        foreach (GameObject civilian in civilians)
        {
            Health health = civilian.GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath -= OnCivilianDeath;
                health.OnDeath += OnCivilianDeath;
            }
        }
    }

    private void OnCivilianDeath()
    {
        remainingCivilians--;

        if (remainingCivilians <= 0)
        {
            GameManager.Instance.UpdateGameState(GameState.Lose);
        }
    }
}
