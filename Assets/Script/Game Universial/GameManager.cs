using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;

#if UNITY_EDITOR


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
    [SerializeField] private float failureResultDuration = 7f;
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
	
	[Header("Sound Manager")]
	[SerializeField] private SoundManager soundManager;

	[Header("Reminder UI")]
	[SerializeField] private GameObject leftReminderPanel;
	[SerializeField] private GameObject rightReminderPanel;
	[SerializeField] private TMP_Text leftReminderText;
	[SerializeField] private TMP_Text rightReminderText;

    [Header("Call Info manager")]
    [SerializeField] private IncomingCallInfoManager callInfoManager;
    
    [Header("Phone Effect")]
    [SerializeField] private PhoneEffects phoneEffects;

	private bool redButtonPressedOnce = false;
    
    private int callCount = 0;
    private float playerMoney;
    private float flashTimer = 0f;
    
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
		
		// Make sure we have a reference to the SoundManager
    	if (soundManager == null)
        soundManager = SoundManager.Instance;
        
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
		
		// In the Start method, subscribe to the new events:
		inputManager.OnIncomingCallRedButtonPressed += HandleIncomingCallRedButton;
		inputManager.OnActiveCallGreenButtonPressed += HandleActiveCallGreenButton;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.OnHangUpTriggered -= HangUpCall;
            inputManager.OnPickUpTriggered -= StartCall;
			inputManager.OnIncomingCallRedButtonPressed -= HandleIncomingCallRedButton;
    		inputManager.OnActiveCallGreenButtonPressed -= HandleActiveCallGreenButton;
        }
    }
    
    private void Update()
    {
        // Update call timer if active
        if (callActive)
        {
            currentCallTime -= Time.deltaTime;
            
            // Determine timer format based on remaining time
            string timeText;
            
            if (currentCallTime <= 5.1f)
            {
                // Under 5.1 seconds - show with 2 decimal places and flash between bright and dark red
                timeText = currentCallTime.ToString("F2") + " s";
                
                // Update flash timer - slower flashing
                flashTimer += Time.deltaTime;
                if (flashTimer >= 0.5f) { // Half-second flash cycle
                    flashTimer = 0f;
                }
                
                // Flash effect - alternate between bright red and darker red
                if (flashTimer < 0.25f)
                    panelManager.UpdateTimerText("<color=#FF0000>" + timeText + "</color>"); // Bright red
                else
                    panelManager.UpdateTimerText("<color=#AA0000>" + timeText + "</color>"); // Darker red
            }
            else if (currentCallTime <= 10f)
            {
                // Between 10 and 5.1 seconds - show with 2 decimal places
                timeText = currentCallTime.ToString("F2") + "S";
                panelManager.UpdateTimerText(timeText);
            }
            else
            {
                // Above 10 seconds - show as whole number
                timeText = Mathf.CeilToInt(currentCallTime).ToString() + "S";
                panelManager.UpdateTimerText(timeText);
            }
            
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
        
        // Generate a new random caller name
        if (callInfoManager != null)
        callInfoManager.GenerateRandomCallerName();
        
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
        // Start phone vibration effect
        if (phoneEffects != null)
        {
            phoneEffects.ResetAllPanels();
            phoneEffects.StartVibrationEffect();
        }
		
		soundManager.PlayIncomingRingtone();
        
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
		
		// Add this after configuring call interface
		soundManager.PlayDuringCallBGM();
        
        // Now reveal the faces
        faceManager.ShowFaces();
        
        // Start the call timer
        callActive = true;
        currentCallTime = callDuration;
        
        // Stop vibration and start face bobbing
        if (phoneEffects != null)
        {
            // Stop vibration
            phoneEffects.ResetAllPanels();
            // Start face panel bobbing
            phoneEffects.StartFacePanelBobbing();
        }
        
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
			soundManager.PlayFailureSFX();
        }
        else
        {
            // Hung up on a scammer - good job!
            resultMessage = scammerHungUpMessage;
            correct = true;
			soundManager.PlaySuccessSFX();
        }
		
		// Transition music
    	soundManager.TransitionToResultBGM();
        
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
			soundManager.PlaySuccessSFX();
        }
        else
        {
            // Didn't hang up on a scammer - lose a life
            string differenceFeedback = faceGenerator.GetDifferenceFeedback();
        	resultMessage = scammerCompletedMessage;
        
        	if (!string.IsNullOrEmpty(differenceFeedback))
            	resultMessage += "\n<color=red><b>" + differenceFeedback + "!</b></color>";

            livesRemaining--;
            panelManager.UpdateLives(livesRemaining);
			soundManager.PlayFailureSFX();
        }

		soundManager.TransitionToResultBGM();
        
        // Inform face generator about round completion
        if (faceGenerator != null)
        {
            faceGenerator.OnRoundCompleted(correct);
        }
        
        // Display result (keep faces visible)
        panelManager.ShowResultPanel(resultMessage, correct);
        
        // Start result countdown
        resultActive = true;
        // Use longer duration (7 seconds) when player fails by not hanging up on a scammer
        if (!isFriend && !correct) {
            resultCountdown = failureResultDuration; // 7 seconds for failure
            Debug.Log("Player failed - showing result for 7 seconds");
        } else {
            resultCountdown = resultDuration; // Default duration for other cases
        }
        
        // Save player's data for game over screen if they've lost
        if (livesRemaining <= 0)
        {
            SaveGameOverData();
        }
    }
    
	// Then add these methods:
    private void HandleIncomingCallRedButton() {
        Debug.Log("HandleIncomingCallRedButton called");
        if (!redButtonPressedOnce) {
            Debug.Log("First red button press - showing reminder");
            ShowReminder("Press Red Button Again to Restart the Game", 3f);
            redButtonPressedOnce = true;
            Invoke("ResetRedButton", 3f);
        } else {
            Debug.Log("Second red button press - restarting game");
            SceneManager.LoadScene("StartInstructionScene");
        }
    }

    private void HandleActiveCallGreenButton() {
        Debug.Log("HandleActiveCallGreenButton called");
        ShowReminder("Finish the call with your \"Friend\"!", 3f);
    }

    private void ShowReminder(string message, float duration) {
        Debug.Log("ShowReminder called with message: " + message);
        Debug.Log("leftReminderPanel null? " + (leftReminderPanel == null));
        Debug.Log("rightReminderPanel null? " + (rightReminderPanel == null));
        Debug.Log("leftReminderText null? " + (leftReminderText == null));
        Debug.Log("rightReminderText null? " + (rightReminderText == null));
        
        if (leftReminderText != null && rightReminderText != null) {
            leftReminderText.text = message;
            rightReminderText.text = message;
        }
        
        if (leftReminderPanel != null && rightReminderPanel != null) {
            leftReminderPanel.SetActive(true);
            rightReminderPanel.SetActive(true);
        }
        
        Invoke("HideReminder", duration);
    }

    private void HideReminder() {
        leftReminderPanel.SetActive(false);
        rightReminderPanel.SetActive(false);
    }

    private void ResetRedButton() {
        redButtonPressedOnce = false;
    }
    // Save game over data for the GameOverManager to use
    private void SaveGameOverData()
    {
        PlayerPrefs.SetInt("CallCount", callCount);
        PlayerPrefs.SetFloat("MoneyLost", playerMoney);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved game over data - Calls: {callCount}, Money: ${playerMoney:F2}");
    }
	
	
	// In GameManager.cs
	[ContextMenu("Force Learn All Features")]
	public void ForceLearnAllFeatures()
	{
    	var features = FindFirstObjectByType<FaceDatabase>().GetFeaturesByCategory("All");
    	foreach (var feature in features)
    	{
        	feature.isLearned = true;
    	}
    	Debug.Log($"Forced {features.Count} features to learned state");
    	EditorUtility.SetDirty(FindFirstObjectByType<FaceDatabase>());
	}

	[ContextMenu("Show Face Feature Counts")]
	public void ShowFeatureCounts()
	{
    	var db = FindFirstObjectByType<FaceDatabase>();
    	foreach (string category in db.FeatureCategories)
    	{
        	int total = db.GetFeaturesByCategory(category).Count;
        	int learned = db.GetLearnedFeatures(category).Count;
        	Debug.Log($"{category}: {learned}/{total} learned");
    	}
	}
}
#endif