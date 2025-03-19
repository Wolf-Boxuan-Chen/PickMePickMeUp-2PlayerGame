using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private FaceManager faceManager;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private InputManager inputManager;
    
    [Header("Game Settings")]
    [SerializeField] private float callDuration = 20f;
    [SerializeField] private float resultDuration = 5f; // Time to show result before next call
    [SerializeField] private int maxLives = 2;
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    
    [Header("Result Messages")]
    [SerializeField] private string scammerHungUpMessage = "Scammer Avoided! Good job!";
    [SerializeField] private string scammerCompletedMessage = "You talked to a scammer for 20 seconds. Scammed!";
    [SerializeField] private string friendHungUpMessage = "You hung up on your friend! Friendship damaged!";
    [SerializeField] private string friendCompletedMessage = "Nice chat with your friend! They're happy!";
    
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
        
        // Subscribe to input events
        inputManager.OnHangUpTriggered += HangUpCall;
        inputManager.OnPickUpTriggered += StartCall;
        
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
            
            // Update countdown display
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
    
    // Show incoming call screen and prepare next call
    private void ShowIncomingCall()
    {
        // Generate new face content for this call
        faceManager.PrepareCall();
        
        // Hide faces until call is answered
        faceManager.HideFaces();
        
        // Show incoming call UI
        panelManager.ShowIncomingCallPanel();
        
        // Configure input for pickup phase
        inputManager.SetCallActive(false);
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
        
        // Display result (keep faces visible)
        panelManager.ShowResultPanel(resultMessage, correct);
        
        // Start result countdown
        resultActive = true;
        resultCountdown = resultDuration;
        
        // Check for game over
        if (livesRemaining <= 0)
        {
            resultActive = false;
            StartCoroutine(GameOver());
            return;
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
        
        // Display result (keep faces visible)
        panelManager.ShowResultPanel(resultMessage, correct);
        
        // Start result countdown
        resultActive = true;
        resultCountdown = resultDuration;
        
        // Check for game over
        if (livesRemaining <= 0)
        {
            resultActive = false;
            StartCoroutine(GameOver());
            return;
        }
    }
    
    // Game over sequence
    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(gameOverSceneName);
    }
}