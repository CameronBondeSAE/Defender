using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DanniLi;
using Unity.VisualScripting;
using UnityEditor;

/// <summary>
/// This class loads/unloads additive level scenes, this is server-authoritative
/// - the server runs all scene loads/unloads and sets the active scene when a load completes
/// - clients just follow along via netcode's scene manager
/// - public functions (and server rpcs too) for ui buttons: next level, reload, back to main menu
/// </summary>
public class LevelLoader : NetworkBehaviour
{
   public DanniLi.LevelInfo[] levelOrder;
   [Header("Refs")]
   [SerializeField] private GameManager gameManager;
   [SerializeField] private UIManager uiManager;
   
   [Header("Scene Configs")]
   public string managerSceneName = "ManagerScene";
   public string mainMenuSceneName = "MainMenu";
   private List<string> loadedLevelScenes = new List<string>();

   private string currentAdditiveSceneName = null;
   private string pendingSetActiveSceneName = null;

   private void Awake()
   {
      if(gameManager == null)
         gameManager = FindObjectOfType<GameManager>();
      if(uiManager == null)
         uiManager = FindObjectOfType<UIManager>();
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
      
      InitializeLoadedScenes();
   }

   private void InitializeLoadedScenes()
   {
      // track all scenes that are already loaded except manager scene
      for (int i = 0; i < SceneManager.sceneCount; i++)
      {
         Scene scene = SceneManager.GetSceneAt(i);
         if (scene.isLoaded && scene.name != managerSceneName && !scene.name.Contains("DontDestroyOnLoad"))
         {
            if (!loadedLevelScenes.Contains(scene.name))
            {
               loadedLevelScenes.Add(scene.name);
            }
         }
      }
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
     // DoLoadNext();
   }
   
   private void OnLose()
   {
     if(!IsServer) return;
     // DoReloadCurrent();
   }

   private DanniLi.LevelInfo[] GetLevels()
   {
      if (gameManager != null && gameManager.levelSOs != null && gameManager.levelSOs.Length > 0)
         return gameManager.levelSOs;
      return levelOrder;
   }
   
   // Below are public methods for UI to call
   public void LoadNextLevel()
   {
      if (!IsServer) return;
      DoLoadNext();
   }
   public void ReloadCurrentLevel()
   {
      if (!IsServer) return;
      DoReloadCurrent();
   }
   public void LoadMainMenu()
   {
      if (!IsServer) return;
      LoadMainMenuRpc();
   }

   /// <summary>
   /// get next level index, hide ui, unload old level scenes, and additive-load the next scene
   /// once the load completes, OnLoadEventCompleted sets it active
   private void DoLoadNext()
   {
      var levels = GetLevels();
      if (levels == null || levels.Length == 0)
      {
         Debug.LogWarning("No levels configured.");
         return;
      }
      int currentIndex = (gameManager != null) ? gameManager.currentLevelIndex : 0;
      int nextIndex = currentIndex + 1;
      // check for completion and go to main menu
      if (nextIndex >= levels.Length) 
      {
         // when the whole game finishes, could go to credits or main menu
         Debug.Log("All levels completed!");
         LoadMainMenuRpc();
         return;
      }

      var next = levels[nextIndex];
      if (next == null || string.IsNullOrEmpty(next.sceneName))
      {
         Debug.LogError("LevelInfo is missing or sceneName is empty.");
         return;
      }
      if (gameManager != null) gameManager.currentLevelIndex = nextIndex;
      
      // hide UI screens before loading
      if (uiManager != null)
         uiManager.HideAllScreensRpc();
      
      // unload all level scenes save the manager scene
      UnloadAllLevelScenes();
        
      // load next level additively & mark to set active scene when done
      pendingSetActiveSceneName = next.sceneName;
      NetworkManager.SceneManager.LoadScene(next.sceneName, LoadSceneMode.Additive);
     
      // // unload current additive
      // if (!string.IsNullOrEmpty(currentAdditiveSceneName))
      // {
      //    var toUnload = SceneManager.GetSceneByName(currentAdditiveSceneName);
      //    if(toUnload.IsValid() && toUnload.isLoaded)
      //    {
      //       NetworkManager.SceneManager.UnloadScene(toUnload);
      //    }
      // }
      //
      // // load next level additively & mark to set active scene when done
      // pendingSetActiveSceneName = next.sceneName;
      // NetworkManager.SceneManager.LoadScene(next.sceneName, LoadSceneMode.Additive);
      // // don't set active here, wait fot event (below)
   }
   
   /// <summary>
   /// unload every tracked level scene except the manager, also safety checks so we never leave zero scenes loaded
   /// </summary>
   private void UnloadAllLevelScenes()
   {
      // check if we have enough scenes to safely unload
      int totalScenes = SceneManager.sceneCount;
      int scenesToUnload = 0;
      
      // count scenes that's gonna be unloaded
      for (int i = loadedLevelScenes.Count - 1; i >= 0; i--)
      {
         string sceneName = loadedLevelScenes[i];
         if (sceneName == managerSceneName) 
            continue;
                
         var scene = SceneManager.GetSceneByName(sceneName);
         if (scene.IsValid() && scene.isLoaded)
         {
            scenesToUnload++;
         }
      }
      
      // if unloading means no scene left, don't unload the last one
      if (totalScenes - scenesToUnload <= 0)
      {
         Debug.LogWarning("Cannot unload all scenes - would leave no active scenes. Skipping unload.");
         return;
      }
      
      // process unloading
      for (int i = loadedLevelScenes.Count - 1; i >= 0; i--)
      {
         string sceneName = loadedLevelScenes[i];
         if (sceneName == managerSceneName) 
            continue;
                
         var scene = SceneManager.GetSceneByName(sceneName);
         if (scene.IsValid() && scene.isLoaded)
         {
            Debug.Log($"Unloading level scene: {sceneName}");
            NetworkManager.SceneManager.UnloadScene(scene);
         }
      }
        
      // clear list but keep manager if it was tracked
      loadedLevelScenes.RemoveAll(sceneName => sceneName != managerSceneName);
   }
   
   /// <summary>
   /// server callback when a networked scene load completes. if this was the scene we were waiting for,
   /// set it active, unload older levels, and tell the game manager a new level is ready
   /// </summary>
   private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
   {
      if (loadscenemode != LoadSceneMode.Additive) return;
      // if (string.IsNullOrEmpty(pendingSetActiveSceneName)) return;
      // if (!string.Equals(scenename, pendingSetActiveSceneName)) return;
      // var loaded = SceneManager.GetSceneByName(scenename);
      // if (loaded.IsValid() && loaded.isLoaded)
      // {
      //    SceneManager.SetActiveScene(loaded); // set active after load completes
      //    currentAdditiveSceneName = scenename; // this is now the current additive
      // }
      // pendingSetActiveSceneName = scenename;
      var loadedScene = SceneManager.GetSceneByName(sceneName);
      if (!loadedScene.IsValid() || !loadedScene.isLoaded) return;
      if (sceneName != managerSceneName && !loadedLevelScenes.Contains(sceneName))
      {
         loadedLevelScenes.Add(sceneName);
      }
      if (!string.IsNullOrEmpty(pendingSetActiveSceneName) && 
          string.Equals(sceneName, pendingSetActiveSceneName))
      {
         if (sceneName != managerSceneName)
         {
            SceneManager.SetActiveScene(loadedScene);
            UnloadOldLevelScenes(sceneName);
            // notify manager
            if (gameManager != null)
            {
               gameManager.OnLevelLoaded();
            }
         }
         pendingSetActiveSceneName = null;
      }
   }
   
   /// <summary>
   /// unload any tracked level scenes except the one we want to keep (like the manager scene)
   /// </summary>
   private void UnloadOldLevelScenes(string keepSceneName)
   {
      // unload all tracked level scenes except the one we want to keep and the manager
      for (int i = loadedLevelScenes.Count - 1; i >= 0; i--)
      {
         string sceneName = loadedLevelScenes[i];
         if (sceneName == managerSceneName || sceneName == keepSceneName) 
            continue;
                
         var scene = SceneManager.GetSceneByName(sceneName);
         if (scene.IsValid() && scene.isLoaded)
         {
            Debug.Log($"Unloading old level scene: {sceneName}");
            NetworkManager.SceneManager.UnloadScene(scene);
            loadedLevelScenes.RemoveAt(i);
         }
      }
   }
   /// <summary>
   /// reload the current level: hides ui, unloads the existing copy if loaded, then additive-loads a fresh copy - was doing me head in this part xD
   /// OnLoadEventCompleted will make it active
   /// </summary>
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
      
      if (uiManager != null)
         uiManager.HideAllScreensRpc();
      
      // for reload, load the scene first, then unload the old version
      pendingSetActiveSceneName = current.sceneName;
      
      // if current scene is already loaded, handle differently
      var existingScene = SceneManager.GetSceneByName(current.sceneName);
      if (existingScene.IsValid() && existingScene.isLoaded)
      {
         // unload the existing version first, then load fresh
         NetworkManager.SceneManager.UnloadScene(existingScene);
         // remove from tracking temporarily
         loadedLevelScenes.Remove(current.sceneName);
      }
      
      // Load the scene
      NetworkManager.SceneManager.LoadScene(current.sceneName, LoadSceneMode.Additive);
   }

   [Rpc(SendTo.Server, RequireOwnership = false)]
   public void NextLevelServerRpc()
   {
      LoadNextLevel();
   }

   [Rpc(SendTo.Server, RequireOwnership = false)]
   public void ReloadCurrentServerRpc()
   {
      ReloadCurrentLevel();
   }
   
   [Rpc(SendTo.Server, RequireOwnership = false)]
   public void LoadMainMenuServerRpc()
   {
      LoadMainMenu();
   }
   
   [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
   private void LoadMainMenuRpc()
   {
      // // disconnect from session first
      // if (NetworkManager.Singleton != null)
      // {
      //    if (NetworkManager.Singleton.IsHost)
      //    {
      //       NetworkManager.Singleton.Shutdown();
      //    }
      //    else if (NetworkManager.Singleton.IsClient)
      //    {
      //       NetworkManager.Singleton.Shutdown();
      //    }
      // }
      // // load main menu scene
      // SceneManager.LoadScene(mainMenuSceneName);
      StartCoroutine(ShutdownAndLoadMenu());
   }
   
   private IEnumerator ShutdownAndLoadMenu()
   {
      // undo scene callbacks
      if (NetworkManager != null && NetworkManager.SceneManager != null)
      {
         NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
      }
      
      if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
      {
         NetworkManager.Singleton.Shutdown();
      }
      yield return null; // one frame for unity transport
      // until fully stopped
      while (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
         yield return null;

      // nuke persistent networkmanager so a fresh one can spawn later
      var networkManager = NetworkManager.Singleton;
      if (networkManager != null && networkManager.gameObject.scene.name == "DontDestroyOnLoad")
      {
         Destroy(networkManager.gameObject);
         yield return null;
      }
      loadedLevelScenes.Clear();
      currentAdditiveSceneName = null;
      pendingSetActiveSceneName = null;
      yield return SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
   }
   
   // helper to manually add scenes to tracking if we need
   public void TrackAdditionalScene(string sceneName)
   {
      if (!string.IsNullOrEmpty(sceneName) && 
          sceneName != managerSceneName && 
          !loadedLevelScenes.Contains(sceneName))
      {
         loadedLevelScenes.Add(sceneName);
      }
   }
    
   // helper to check if a scene is being tracked
   public bool IsSceneTracked(string sceneName)
   {
      return loadedLevelScenes.Contains(sceneName);
   }
}
