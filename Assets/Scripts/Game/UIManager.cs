using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;

namespace DanniLi
{
    public class UIManager : NetworkBehaviour
    {
        [Header("Game UI Elements")]
        [SerializeField] private GameObject gameHudRoot;
        [Header("Level Intro")]
        [SerializeField] private GameObject levelIntroPanel;
        [SerializeField] private Text levelIntroText;
        [SerializeField] private Button levelIntroStartButton;
        [SerializeField] private Text civiliansText;

        [SerializeField] private Text waveText;
        [SerializeField] private Text aliensKilledText;

        [Header("Win Screen")] [SerializeField]
        private GameObject winScreen;

        [SerializeField] private Text winAliensKilledText;
        [SerializeField] private Text winCivsPercentageText;
        [SerializeField] private Button winNextLevelButton;
        [SerializeField] private Button winMainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button onScreenMenuButton;

        [Header("Lose Screen")] [SerializeField]
        private GameObject loseScreen;

        [SerializeField] private Text loseAliensKilledText;
        [SerializeField] private Text loseCivsPercentageText;
        [SerializeField] private Button loseTryAgainButton;
        [SerializeField] private Button loseMainMenuButton;

        [Header("Item Info Panel")] [SerializeField]
        private GameObject itemInfoPanel;

        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemDescriptionText;

        [Header("References")] [SerializeField]
        private GameManager gameManager;

        [Header("Level Loader")]
        [SerializeField] private LevelLoader levelLoader;

        // Netvar for UI states
        private NetworkVariable<int> networkCiviliansAlive =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkTotalCivilians =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkCurrentWave =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkTotalWaves =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkAliensKilled =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> networkWaveInProgress =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkTotalAliens =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // new netvar to track aliens spawned so far as they dynamically spawn
        private NetworkVariable<int> networkAliensSpawned =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            if (levelLoader == null)
                levelLoader = FindObjectOfType<LevelLoader>();

            // Subscribe to netvar changes
            networkCiviliansAlive.OnValueChanged += OnCiviliansAliveChanged;
            networkTotalCivilians.OnValueChanged += OnTotalCiviliansChanged;
            networkCurrentWave.OnValueChanged += OnCurrentWaveChanged;
            networkTotalWaves.OnValueChanged += OnTotalWavesChanged;
            networkAliensKilled.OnValueChanged += OnAliensKilledChanged;
            networkTotalAliens.OnValueChanged += OnTotalAliensChanged;
            networkWaveInProgress.OnValueChanged += OnWaveProgressChanged;
            networkAliensSpawned.OnValueChanged += OnAliensSpawnedChanged;

            // Subscribe to game events (server only)
            if (IsServer && gameManager != null)
            {
                gameManager.WinGameOver_Event += OnWinGameOver;
                gameManager.LoseGameOver_Event += OnLoseGameOver;
            }

            //button listeners (all clients)
            SetupButtonListeners();
            // // initialize
            // UpdateAllUI();
            // // hide screens initially
            // HideAllScreens();
            // HideItemPanel();
            // // if (IsServer && gameManager != null && networkTotalWaves.Value <= 0)
            // // {
            // //     gameManager.ForceInitializeUI(this);
            // // }
            // if (IsServer)
            // {
            //     var gameManager = FindObjectOfType<DanniLi.GameManager>();
            //     gameManager?.ForceInitializeUI(this); // resync if we spawned after GM started the wave
            // }
            // hide overlay screens initially
            HideAllScreens();
            HideItemPanel();
            if (gameHudRoot != null)
                gameHudRoot.SetActive(false);
            if (levelIntroPanel != null)
                levelIntroPanel.SetActive(false);
            if (levelIntroStartButton != null)
                levelIntroStartButton.onClick.AddListener(OnLevelIntroStartClicked);
            if (IsServer)
            {
                var gm = FindObjectOfType<DanniLi.GameManager>();
                gm?.ForceInitializeUI(this); // resync if we spawned after GM started the wave
            }
        }

        public override void OnNetworkDespawn()
        {
            networkCiviliansAlive.OnValueChanged -= OnCiviliansAliveChanged;
            networkTotalCivilians.OnValueChanged -= OnTotalCiviliansChanged;
            networkCurrentWave.OnValueChanged -= OnCurrentWaveChanged;
            networkTotalWaves.OnValueChanged -= OnTotalWavesChanged;
            networkAliensKilled.OnValueChanged -= OnAliensKilledChanged;
            networkTotalAliens.OnValueChanged -= OnTotalAliensChanged;
            networkWaveInProgress.OnValueChanged -= OnWaveProgressChanged;
            networkAliensSpawned.OnValueChanged -= OnAliensSpawnedChanged;
            if (gameManager != null)
            {
                gameManager.WinGameOver_Event -= OnWinGameOver;
                gameManager.LoseGameOver_Event -= OnLoseGameOver;
            }
            if (levelIntroStartButton != null)
                levelIntroStartButton.onClick.RemoveListener(OnLevelIntroStartClicked);
            base.OnNetworkDespawn();
        }

        #region Network Variable Callbacks

        private void OnCiviliansAliveChanged(int oldValue, int newValue)
        {
            UpdateCiviliansUI();
        }

        private void OnTotalCiviliansChanged(int oldValue, int newValue)
        {
            UpdateCiviliansUI();
        }

        private void OnCurrentWaveChanged(int oldValue, int newValue)
        {
            UpdateWaveUI();
        }

        private void OnTotalWavesChanged(int oldValue, int newValue)
        {
            UpdateWaveUI();
        }

        private void OnAliensKilledChanged(int oldValue, int newValue)
        {
            UpdateAliensKilledUI();
        }

        private void OnTotalAliensChanged(int oldValue, int newValue)
        {
            UpdateAliensKilledUI();
        }

        private void OnWaveProgressChanged(bool oldV, bool newV)
        {
            UpdateWaveUI();
        }

        private void OnAliensSpawnedChanged(int oldValue, int newValue)
        {
            UpdateAliensKilledUI();
        }

        #endregion

        #region UI Update Methods

        private void UpdateAllUI()
        {
            UpdateCiviliansUI();
            UpdateWaveUI();
            UpdateAliensKilledUI();
        }

        private void UpdateCiviliansUI()
        {
            if (civiliansText != null)
            {
                civiliansText.text = $"Civilians Alive: {networkCiviliansAlive.Value}/{networkTotalCivilians.Value}";
            }
        }

        private void UpdateWaveUI()
        {
            if (waveText == null) return;
            if (networkTotalWaves.Value <= 0)
            {
                waveText.text = "Preparing waves…";
                return;
            }

            if (networkWaveInProgress.Value)
            {
                waveText.text = $"Wave {networkCurrentWave.Value} in Progress";
                return;
            }

            int wavesRemaining = Mathf.Max(0, networkTotalWaves.Value - networkCurrentWave.Value);
            if (wavesRemaining > 0)
                waveText.text = $"Wave {networkCurrentWave.Value + 1} Begins Soon ({wavesRemaining} waves remaining)";
            else
                waveText.text = "All Waves Complete";
        }

        private void UpdateAliensKilledUI()
        {
            if (aliensKilledText != null)
            {
                if (networkAliensSpawned.Value > 0)
                {
                    // "Aliens Killed: 5/8 (Spawned: 6/12)"
                    aliensKilledText.text =
                        $"Aliens Killed: {networkAliensKilled.Value}/{networkTotalAliens.Value} (Spawned: {networkAliensSpawned.Value}/{networkTotalAliens.Value})";
                }
                else
                {
                    // fallback to old if spawned count isn't tracked
                    aliensKilledText.text = $"Aliens Killed: {networkAliensKilled.Value}/{networkTotalAliens.Value}";
                }
            }
        }
        
        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        private void StartLevelIntroHideAndShowHudRpc()
        {
            if (levelIntroPanel != null)
                levelIntroPanel.SetActive(false);

            if (gameHudRoot != null)
                gameHudRoot.SetActive(true);
        }


        #endregion

        #region Server-Only Update Methods

        public void InitializeUI(int totalCivs, int aliveCivs, int totalWaves, int totalAliens)
        {
            if (!IsServer) return;

            networkTotalCivilians.Value = totalCivs;
            networkCiviliansAlive.Value = aliveCivs;
            networkTotalWaves.Value = totalWaves;
            networkCurrentWave.Value = 0;
            networkAliensKilled.Value = 0;
            networkWaveInProgress.Value = false;
            networkTotalAliens.Value = totalAliens;
            networkAliensSpawned.Value = 0;
        }

        public void OnCivilianDeath(int currentAlive)
        {
            if (!IsServer) return;
            networkCiviliansAlive.Value = currentAlive;
        }

        public void OnAlienKilled()
        {
            if (!IsServer) return;
            networkAliensKilled.Value++;
        }

        public void OnWaveStart(int waveNumber)
        {
            if (!IsServer) return;
            // Debug.Log($"[UI][SERVER] OnWaveStart({waveNumber}) NO={GetComponent<NetworkObject>().NetworkObjectId}");
            networkCurrentWave.Value = waveNumber;
            networkWaveInProgress.Value = true;
        }

        public void OnWaveEnd(int waveNumber)
        {
            if (!IsServer) return;
            networkCurrentWave.Value = waveNumber;
            networkWaveInProgress.Value = false;
        }

        public void UpdateTotalAliens(int newTotal)
        {
            if (!IsServer) return;
            networkTotalAliens.Value = newTotal;
        }

        public void UpdateAlienProgress(int spawned, int total)
        {
            if (!IsServer) return;
            networkAliensSpawned.Value = spawned;
            if (total > networkTotalAliens.Value)
            {
                networkTotalAliens.Value = total;
            }
        }

        #endregion

        #region Game Win/Lose Screens

        private void OnWinGameOver()
        {
            if (!IsServer) return;
            ShowWinScreenRpc();
        }

        private void OnLoseGameOver()
        {
            if (!IsServer) return;
            ShowLoseScreenRpc();
        }

        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        private void ShowWinScreenRpc()
        {
            ShowWinScreen();
        }

        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        private void ShowLoseScreenRpc()
        {
            ShowLoseScreen();
        }
        
        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        public void ShowLevelIntroRpc(int percentCiviliansToSave)
        {
            ShowLevelIntro(percentCiviliansToSave);
        }

        private void ShowLevelIntro(int percentCiviliansToSave)
        {
            if (levelIntroText != null)
            {
                levelIntroText.text =
                    $"Your mission: save at least {percentCiviliansToSave}% of civilians in this level!";
            }
            if (levelIntroPanel != null)
                levelIntroPanel.SetActive(true);

            if (gameHudRoot != null)
                gameHudRoot.SetActive(false);
        }

        [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        public void HideAllScreensRpc()
        {
	        Debug.Log("Hiding all screens");
            HideAllScreens();
        }

        private void HideAllScreens()
        {
            if (winScreen != null) winScreen.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);
            if (levelIntroPanel != null) levelIntroPanel.SetActive(false);
        }

        private void ShowWinScreen()
        {
            HideAllScreens();
            if (winScreen != null) winScreen.SetActive(true);
            UpdateWinScreenStats();

        }

        private void ShowLoseScreen()
        {
            HideAllScreens();
            if (loseScreen != null) loseScreen.SetActive(true);
            UpdateLoseScreenStats();
        }

        private void UpdateWinScreenStats()
        {
            if (winAliensKilledText != null)
            {
                winAliensKilledText.text =
                    $"Aliens Killed: {networkAliensKilled.Value}/{networkTotalAliens.Value}";
            }
            if (winCivsPercentageText != null)
            {
                int total = networkTotalCivilians.Value;
                int alive = networkCiviliansAlive.Value;

                float percentage = total > 0
                    ? (alive / (float)total) * 100f
                    : 0f;

                winCivsPercentageText.text =
                    $"Civilians Saved: {alive}/{total} ({percentage:F1}%)";
            }
        }

        private void UpdateLoseScreenStats()
        {
            if (loseAliensKilledText != null)
            {
                loseAliensKilledText.text =
                    $"Aliens Killed: {networkAliensKilled.Value}/{networkTotalAliens.Value}";
            }
            if (loseCivsPercentageText != null)
            {
                int total = networkTotalCivilians.Value;
                int alive = networkCiviliansAlive.Value;

                float percentage = total > 0
                    ? (alive / (float)total) * 100f
                    : 0f;

                loseCivsPercentageText.text =
                    $"Civilians Saved: {alive}/{total} ({percentage:F1}%)";
            }
        }

        public void ShowItemPanel(string nameString, string descString)
        {
            if (itemInfoPanel) itemInfoPanel.SetActive(true);
            if (itemNameText) itemNameText.text = nameString ?? string.Empty;
            if (itemDescriptionText) itemDescriptionText.text = descString ?? string.Empty;
        }

        public void HideItemPanel()
        {
            if (itemInfoPanel) itemInfoPanel.SetActive(false);
        }

        #endregion

        #region Button Setup and Handlers

        private void SetupButtonListeners()
        {
            // win screen buttons
            if (winNextLevelButton != null)
                winNextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (winMainMenuButton != null)
                winMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // lose screen buttons
            if (loseTryAgainButton != null)
                loseTryAgainButton.onClick.AddListener(OnTryAgainClicked);
            if (loseMainMenuButton != null)
                loseMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            if (onScreenMenuButton != null)
                onScreenMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnNextLevelClicked()
        {
            if (levelLoader != null)
            {
                levelLoader.NextLevelServerRpc();
            }
        }

        private void OnTryAgainClicked()
        {
            if (levelLoader != null)
            {
                levelLoader.ReloadCurrentServerRpc();
            }
        }

        private void OnMainMenuClicked()
        {
            if (levelLoader != null)
            {
                levelLoader.LoadMainMenuServerRpc();
            }
        }
        
        /// called locally when the player clicks the "Start Game" button on the intro panel
        private void OnLevelIntroStartClicked()
        {
            // host only
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
                return;

            if (gameManager != null)
            {
                gameManager.BeginLevelServerRpc();
            }

            // hide intro + show HUD on ALL clients
            StartLevelIntroHideAndShowHudRpc();
        }

        private void QuitGame()
            #endregion
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
            
        }
    }
}
