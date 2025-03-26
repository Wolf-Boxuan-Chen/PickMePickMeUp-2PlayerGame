using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private FaceManager faceManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private FaceGenerator faceGenerator; // New reference
    
    [Header("Game Settings")]
    [SerializeField] private float callDuration = 20f;
    [SerializeField] private float resultDuration = 5f;
    [SerializeField] private int maxLives = 2;
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    
    [Header("Result Messages")]
    [SerializeField] private string scammerHungUpMessage = "Scammer Avoided! Good job!";
    [SerializeField] private string scammerCompletedMessage = "You talked to a scammer for 20 seconds. Scammed!";
    [SerializeField] private string friendHungUpMessage = "You hung up on your friend! Friendship damaged!";
    [SerializeField] private string friendCompletedMessage = "Nice chat with your friend! They're happy!";
    
    [Header("Score Tracking")]
    [SerializeField] private TMP_Text leftCallCountText;
    [SerializeField] private TMP_Text rightCallCountText;
    
    [Header("Score Display")]
    [SerializeField] private TMP_Text leftScoreText;
    [SerializeField] private TMP_Text rightScoreText;
    
    private int callCount = 0;
    private float playerMoney;
    
    private int livesRemaining;
    private float currentCallTime;
    private float resultCountdown;
    private bool callActive = false;
    private bool resultActive = false;
    
    private void Start()
    {
        // Set up initial game state
        livesRemaining = maxLives;
        panelManager.UpdateLives(livesRemaining);
        
        // Generate random money amount
        GeneratePlayerMoney();
        
        // Subscribe to input events
        inputManager.OnHangUpTriggered += HangUpCall;
        inputManager.OnPickUpTriggered += StartCall;
        
        // Reset call count for new game
        callCount = 0;
        UpdateCallCountDisplay();
        
        // Initialize the face generator
        if (faceGenerator != null)
        {
            faceGenerator.Initialize();
        }
        else
        {
            Debug.LogError("FaceGenerator not assigned!");
        }
        
        // Show initial incoming call screen
        ShowIncomingCall();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.OnHangUpTriggered -= HangUpCall;
            inputManager.OnPickUpTriggered -= StartCall;
        }
    }
    
    private void Update()
    {
        // Update call timer if active
        if (callActive)
        {
            currentCallTime -= Time.deltaTime;
            
            // Update timer display
            string timeText = Mathf.CeilToInt(currentCallTime).ToString();
            panelManager.UpdateTimerText(timeText);
            
            // Check if time is up
            if (currentCallTime <= 0)
            {
                CompleteCall();
            }
        }
        
        // Update result countdown if active
        if (resultActive)
        {
            resultCountdown -= Time.deltaTime;
    
            // Different message if player has lost all lives
            if (livesRemaining <= 0)
            {
                string countdownText = "Face your score in " + Mathf.CeilToInt(resultCountdown) + "...";
                panelManager.UpdateResultCountdownText(countdownText);
        
                // Load game over scene when countdown completes
                if (resultCountdown <= 0)
                {
                    resultActive = false;
                    SceneManager.LoadScene(gameOverSceneName);
                }
            }
            else
            {
                // Regular countdown
                string countdownText = "Next call in " + Mathf.CeilToInt(resultCountdown) + "...";
                panelManager.UpdateResultCountdownText(countdownText);
        
                // Move to next call when countdown completes
                if (resultCountdown <= 0)
                {
                    resultActive = false;
                    ShowIncomingCall();
                }
            }
        }
    }
    
    // Show incoming call screen and prepare next call
    private void ShowIncomingCall()
    {
        // Increment call counter
        callCount++;
        UpdateCallCountDisplay();
        UpdateScoreDisplay();
        
        // Generate new faces for this call using the new system
        if (faceGenerator != null)
        {
            faceGenerator.GenerateNextRoundFaces();
        }
        else
        {
            // Fallback to old system
            faceManager.PrepareCall();
        }
        
        // Hide faces until call is answered
        faceManager.HideFaces();
        
        // Show incoming call UI
        panelManager.ShowIncomingCallPanel();
        
        // Configure input for pickup phase
        inputManager.SetCallActive(false);
    }
    
    // Update score display texts
    private void UpdateScoreDisplay()
    {
        string scoreText = "No.Call: " + callCount;
        
        if (leftScoreText != null)
            leftScoreText.text = scoreText;
        
        if (rightScoreText != null)
            rightScoreText.text = scoreText;
    }
    
    private void UpdateCallCountDisplay()
    {
        if (leftCallCountText != null)
            leftCallCountText.text = "Calls: " + callCount;
            
        if (rightCallCountText != null)
            rightCallCountText.text = "Calls: " + callCount;
    }
    
    private void GeneratePlayerMoney()
    {
        // Decide if we generate a small or extremely large amount (90% small, 10% large)
        if (Random.value > 0.9f)
        {
            // Generate a huge amount (hundreds of millions to billions)
            playerMoney = Random.Range(100000000f, 3000000000f);
        }
        else
        {
            // Generate a modest amount ($10 to $1000)
            playerMoney = Random.Range(10f, 1000f);
            
            // Add cents for realism
            playerMoney += Random.Range(0.01f, 0.99f);
        }
        
        Debug.Log($"Player starting with ${playerMoney:F2}");
    }
    
    // Start the call (when player picks up)
    private void StartCall()
    {
        // Show the call interface
        panelManager.ShowCallActivePanel();
        
        // Now reveal the faces
        faceManager.ShowFaces();
        
        // Start the call timer
        callActive = true;
        currentCallTime = callDuration;
        
        // Configure input for call phase
        inputManager.SetCallActive(true);
    }
    
    // Player hung up on the call
    private void HangUpCall()
    {
        if (!callActive) return;
        
        callActive = false;
        
        // Check if this was a friend or scammer
        bool isFriend = faceManager.IsFriendCall();
        
        // Show appropriate result message
        string resultMessage;
        bool correct = false;
        
        if (isFriend)
        {
            // Hung up on a friend - lose a life
            resultMessage = friendHungUpMessage;
            livesRemaining--;
            panelManager.UpdateLives(livesRemaining);
        }
        else
        {
            // Hung up on a scammer - good job!
            resultMessage = scammerHungUpMessage;
            correct = true;
        }
        
        // Inform face generator about round completion
        if (faceGenerator != null)
        {
            faceGenerator.OnRoundCompleted(correct);
        }
        
        // Display result (keep faces visible)
        panelManager.ShowResultPanel(resultMessage, correct);
        
        // Start result countdown
        resultActive = true;
        resultCountdown = resultDuration;
        
        // Save player's data for game over screen if they've lost
        if (livesRemaining <= 0)
        {
            SaveGameOverData();
        }
    }
    
    // Call completed without hanging up
    private void CompleteCall()
    {
        callActive = false;
        
        // Check if this was a friend or scammer
        bool isFriend = faceManager.IsFriendCall();
        
        // Show appropriate result message
        string resultMessage;
        bool correct = false;
        
        if (isFriend)
        {
            // Completed call with friend - good!
            resultMessage = friendCompletedMessage;
            correct = true;
        }
        else
        {
            // Didn't hang up on a scammer - lose a life
            resultMessage = scammerCompletedMessage;
            livesRemaining--;
            panelManager.UpdateLives(livesRemaining);
        }
        
        // Inform face generator about round completion
        if (faceGenerator != null)
        {
            faceGenerator.OnRoundCompleted(correct);
        }
        
        // Display result (keep faces visible)
        panelManager.ShowResultPanel(resultMessage, correct);
        
        // Start result countdown
        resultActive = true;
        resultCountdown = resultDuration;
        
        // Save player's data for game over screen if they've lost
        if (livesRemaining <= 0)
        {
            SaveGameOverData();
        }
    }
    
    // Save game over data for the GameOverManager to use
    private void SaveGameOverData()
    {
        PlayerPrefs.SetInt("CallCount", callCount);
        PlayerPrefs.SetFloat("MoneyLost", playerMoney);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved game over data - Calls: {callCount}, Money: ${playerMoney:F2}");
    }
}