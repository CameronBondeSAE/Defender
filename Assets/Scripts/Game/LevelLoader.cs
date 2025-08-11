using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Xml;
using DanniLi;
using Unity.VisualScripting;
using UnityEditor;

public class LevelLoader : NetworkBehaviour
{
   public DanniLi.LevelInfo[] levelOrder;
   [Header("Refs")]
   [SerializeField] private GameManager gameManager;

   private string currentAdditiveSceneName = null;
   private string pendingSetActiveSceneName = null;

   private void Awake()
   {
      if(gameManager == null)
         gameManager = FindObjectOfType<GameManager>();
   }

   public override void OnNetworkSpawn()
   {
      base.OnNetworkSpawn();
      if (gameManager != null)
      {
         gameManager.WinGameOver_Event += OnWin;
         gameManager.LoseGameOver_Event += OnLose;
      }

      if (IsServer)
         NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
   }

   public override void OnDestroy()
   {
      base.OnNetworkSpawn();
      if (gameManager != null)
      {
         gameManager.WinGameOver_Event -= OnWin;
         gameManager.LoseGameOver_Event -= OnLose;
      }
      if(NetworkManager && NetworkManager.SceneManager != null)
         NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
   }

   private void OnWin()
   {
      if (!IsServer) return;
     DoLoadNext();
   }
   
   private void OnLose()
   {
     if(!IsServer) return;
     DoReloadCurrent();
   }

   private DanniLi.LevelInfo[] GetLevels()
   {
      if (gameManager != null && gameManager.levelSOs != null && gameManager.levelSOs.Length > 0)
         return gameManager.levelSOs;
      return levelOrder;
   }

   private void DoLoadNext()
   {
      var levels = GetLevels();
      if (levels == null || levels.Length == 0)
      {
         Debug.LogWarning("No levels configured.");
         return;
      }
      int nextIndex = (gameManager != null) ? gameManager.currentLevelIndex + 1 : 1;
      if (nextIndex >= levels.Length) nextIndex = 0; // loop for testing

      var next = levels[nextIndex];
      if (next == null || string.IsNullOrEmpty(next.sceneName))
      {
         Debug.LogError("LevelInfo is missing or sceneName is empty.");
         return;
      }
      if (gameManager != null) gameManager.currentLevelIndex = nextIndex;
     
      // unload current additive
      if (!string.IsNullOrEmpty(currentAdditiveSceneName))
      {
         var toUnload = SceneManager.GetSceneByName(currentAdditiveSceneName);
         if(toUnload.IsValid() && toUnload.isLoaded)
         {
            NetworkManager.SceneManager.UnloadScene(toUnload);
         }
      }
      
      // load next level additively & mark to set active scene when done
      pendingSetActiveSceneName = next.sceneName;
      NetworkManager.SceneManager.LoadScene(next.sceneName, LoadSceneMode.Additive);
      // don't set active here, wait fot event (below)
   }
   
   private void OnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
   {
      if (loadscenemode != LoadSceneMode.Additive) return;
      if (string.IsNullOrEmpty(pendingSetActiveSceneName)) return;
      if (!string.Equals(scenename, pendingSetActiveSceneName)) return;
      
      var loaded = SceneManager.GetSceneByName(scenename);
      if (loaded.IsValid() && loaded.isLoaded)
      {
         SceneManager.SetActiveScene(loaded); // set active after load completes
         currentAdditiveSceneName = scenename; // this is now the current additive
      }
      pendingSetActiveSceneName = scenename;
   }

   private void DoReloadCurrent()
   {
      var levels = GetLevels();
      if (levels == null || levels.Length == 0)
      {
         Debug.LogWarning("No levels configured.");
         return;
      }

      int idx = (gameManager != null) ? gameManager.currentLevelIndex : 0;
      var current = levels[Mathf.Clamp(idx, 0, levels.Length - 1)];
      if (current == null || string.IsNullOrEmpty(current.sceneName))
      {
         Debug.LogError("LevelInfo missing or sceneName empty.");
         return;
      }

      NetworkManager.SceneManager.LoadScene(current.sceneName, LoadSceneMode.Additive);
   }

   [Rpc(SendTo.Server, RequireOwnership = false)]
   public void NextLevelServerRpc()
   {
      DoLoadNext();
   }
   
   [Rpc(SendTo.Server, RequireOwnership = false)]
   public void ReloadCurrentServerRpc()
   {
      // DoReloadCurrent();
      if(!IsServer) return;
      if (!string.IsNullOrEmpty(currentAdditiveSceneName))
      {
         var scene = SceneManager.GetSceneByName(currentAdditiveSceneName);
         if (scene.IsValid() && scene.isLoaded)
         {
            NetworkManager.SceneManager.UnloadScene(scene);
         }
      }

      if (!string.IsNullOrEmpty(currentAdditiveSceneName))
      {
         pendingSetActiveSceneName = currentAdditiveSceneName;
         NetworkManager.SceneManager.LoadScene(currentAdditiveSceneName, LoadSceneMode.Additive);
      }
   }
}
