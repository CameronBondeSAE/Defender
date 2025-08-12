using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace DanniLi
{
    public class UIManager : NetworkBehaviour
    {
        [Header("Game UI Elements")]
        [SerializeField] private Text civiliansText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text aliensKilledText;
        
        [Header("Win Screen")]
        [SerializeField] private GameObject winScreen;
        [SerializeField] private Text winAliensKilledText;
        [SerializeField] private Text winCivsPercentageText;
        [SerializeField] private Button winNextLevelButton;
        [SerializeField] private Button winMainMenuButton;
        
        [Header("Lose Screen")]
        [SerializeField] private GameObject loseScreen;
        [SerializeField] private Text loseAliensKilledText;
        [SerializeField] private Text loseCivsPercentageText;
        [SerializeField] private Button loseTryAgainButton;
        [SerializeField] private Button loseMainMenuButton;
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private LevelLoader levelLoader;
        
        // Netvar for UI states
        private NetworkVariable<int> networkCiviliansAlive = new NetworkVariable<int>();
        private NetworkVariable<int> networkTotalCivilians = new NetworkVariable<int>();
        private NetworkVariable<int> networkCurrentWave = new NetworkVariable<int>();
        private NetworkVariable<int> networkTotalWaves = new NetworkVariable<int>();
        private NetworkVariable<int> networkAliensKilled = new NetworkVariable<int>();
        private NetworkVariable<bool> networkWaveInProgress = new NetworkVariable<bool>();
        
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
            networkWaveInProgress.OnValueChanged += OnWaveProgressChanged;
            
            // Subscribe to game events (server only)
            if (IsServer && gameManager != null)
            {
                gameManager.WinGameOver_Event += OnWinGameOver;
                gameManager.LoseGameOver_Event += OnLoseGameOver;
            }
            
            //button listeners (all clients)
            SetupButtonListeners();
            
            // initialize
            UpdateAllUI();
            
            // hide screens initially
            HideAllScreens();
        }
        
        public override void OnNetworkDespawn()
        {
            networkCiviliansAlive.OnValueChanged -= OnCiviliansAliveChanged;
            networkTotalCivilians.OnValueChanged -= OnTotalCiviliansChanged;
            networkCurrentWave.OnValueChanged -= OnCurrentWaveChanged;
            networkTotalWaves.OnValueChanged -= OnTotalWavesChanged;
            networkAliensKilled.OnValueChanged -= OnAliensKilledChanged;
            networkWaveInProgress.OnValueChanged -= OnWaveProgressChanged;
            if (gameManager != null)
            {
                gameManager.WinGameOver_Event -= OnWinGameOver;
                gameManager.LoseGameOver_Event -= OnLoseGameOver;
            }
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
        
        private void OnWaveProgressChanged(bool oldValue, bool newValue)
        {
            UpdateWaveUI();
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
            if (waveText != null)
            {
                if (networkWaveInProgress.Value)
                {
                    waveText.text = $"Wave {networkCurrentWave.Value} in Progress";
                }
                else
                {
                    int wavesRemaining = networkTotalWaves.Value - networkCurrentWave.Value;
                    if (wavesRemaining > 0)
                    {
                        waveText.text = $"Wave {networkCurrentWave.Value + 1} Begins Soon ({wavesRemaining} waves remaining)";
                    }
                    else
                    {
                        waveText.text = "All Waves Complete";
                    }
                }
            }
        }
        
        private void UpdateAliensKilledUI()
        {
            if (aliensKilledText != null)
            {
                aliensKilledText.text = $"Aliens Killed: {networkAliensKilled.Value}";
            }
        }
        #endregion
        
        #region Server-Only Update Methods
        // [ServerRpc(RequireOwnership = false)]
        // public void UpdateCiviliansServerRpc(int alive, int total)
        // {
        //     networkCiviliansAlive.Value = alive;
        //     networkTotalCivilians.Value = total;
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void UpdateWaveServerRpc(int currentWave, int totalWaves, bool inProgress)
        // {
        //     networkCurrentWave.Value = currentWave;
        //     networkTotalWaves.Value = totalWaves;
        //     networkWaveInProgress.Value = inProgress;
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void UpdateAliensKilledServerRpc(int killed)
        // {
        //     networkAliensKilled.Value = killed;
        // }
        
        // called by GameManager when initializing
        public void InitializeUI(int totalCivs, int aliveCivs, int totalWaves)
        {
            if (!IsServer) return;
            
            networkTotalCivilians.Value = totalCivs;
            networkCiviliansAlive.Value = aliveCivs;
            networkTotalWaves.Value = totalWaves;
            networkCurrentWave.Value = 0;
            networkAliensKilled.Value = 0;
            networkWaveInProgress.Value = false;
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
            networkCurrentWave.Value = waveNumber;
            networkWaveInProgress.Value = true;
        }
        
        public void OnWaveEnd(int waveNumber)
        {
            if (!IsServer) return;
            networkCurrentWave.Value = waveNumber;
            networkWaveInProgress.Value = false;
        }
        #endregion
        
        #region Game Over Screens
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
        public void HideAllScreensRpc()
        {
            HideAllScreens();
        }
        private void HideAllScreens()
        {
            if (winScreen != null) winScreen.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);
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
                winAliensKilledText.text = $"Aliens Killed: {networkAliensKilled.Value}";
                
            if (winCivsPercentageText != null)
            {
                float percentage = networkTotalCivilians.Value > 0 ? 
                    (networkCiviliansAlive.Value / (float)networkTotalCivilians.Value) * 100f : 0f;
                winCivsPercentageText.text = $"Civilians Saved: {networkCiviliansAlive.Value}/{networkTotalCivilians.Value} ({percentage:F1}%)";
            }
        }
        
        private void UpdateLoseScreenStats()
        {
            if (loseAliensKilledText != null)
                loseAliensKilledText.text = $"Aliens Killed: {networkAliensKilled.Value}";
                
            if (loseCivsPercentageText != null)
            {
                float percentage = networkTotalCivilians.Value > 0 ? 
                    (networkCiviliansAlive.Value / (float)networkTotalCivilians.Value) * 100f : 0f;
                loseCivsPercentageText.text = $"Civilians Saved: {networkCiviliansAlive.Value}/{networkTotalCivilians.Value} ({percentage:F1}%)";
            }
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
        
        /// moveed these to LevelLoader
        // [ServerRpc(RequireOwnership = false)]
        // private void LoadMainMenuServerRpc()
        // {
        //     LoadMainMenuRpc();
        // }
        //
        // [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
        // private void LoadMainMenuRpc()
        // {
        //     // Replace "MainMenu" with your actual main menu scene name
        //     SceneManager.LoadScene("MainMenu");
        // }
        #endregion
    }
}
