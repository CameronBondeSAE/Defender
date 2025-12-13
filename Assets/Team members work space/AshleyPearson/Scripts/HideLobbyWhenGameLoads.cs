using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace DanniLi
{
    /// <summary>
    /// Local Lobby UI hider for clients
    /// </summary>
    public class HideLobbyWhenGameLoads : MonoBehaviour
    {
        // any scene that starts with this word is a game level
        [SerializeField] private string gameScenePrefix = "Level"; 

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // When any level loads, hide this lobby canvas locally
            if (scene.name.StartsWith(gameScenePrefix))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
