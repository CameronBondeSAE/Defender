using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficultyConfig", menuName = "Game/Difficulty Config")]
public class DifficultyConfig : ScriptableObject
{
   [Header("Enemy Scaling")] 
   public float enemyCountMultiplier = 1f;
   public float waveCountMultiplier = 1f;
}

public interface IDifficultyApplier
{
   void ApplyDifficulty(float enemyCountMultiplier, float waveCountMultiplier);
}
