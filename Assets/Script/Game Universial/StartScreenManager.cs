using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [Header("Display Manager")]
    [SerializeField] private DualDisplayManager displayManager;
    
    [Header("Settings")]
    [SerializeField] private int slideCount = 4;
    [SerializeField] private float skipHoldTime = 2.0f;
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Sound Manager")]
    [SerializeField] private SoundManager soundManager;

    
    private int currentSlideIndex = 0;
    private bool titleScreenActive = true;
    private bool tutorialActive = false;
    
    // Key press tracking
    private bool rightArrowPressed = false;
    private bool leftArrowPressed = false;
    
    // Skip tracking
    private bool isSkipHolding = false;
    private float skipHoldStartTime = 0f;
    private KeyCode holdingKey = KeyCode.None;
    
    private void Start()
    {
        // Find display manager if not assigned
        if (displayManager == null)
            displayManager = FindAnyObjectByType<DualDisplayManager>();
            
        // Show title screen initially
        ShowTitleScreen();
		// IMPORTANT: Update SoundManager reference retrieval
    	// Try to get the persistent SoundManager instance first
    	// SoundManager handling - critical fix
    	soundManager = SoundManager.Instance; // Try to get the persisted instance first
    
    	// If no instance found, we need to create a new SoundManager
    	if (soundManager == null)
    	{
        	Debug.Log("No SoundManager instance found - creating a new one");
        
        	// Look for an inactive SoundManager prefab in the scene
        	GameObject soundManagerObj = GameObject.Find("SoundManager");
        
        	// Make sure we have a reference to the SoundManager
			if (soundManager == null)
    			soundManager = SoundManager.Instance;
    
			// Play end screen music
			soundManager.PlayEndScreenBGM();	
    	}
    	else
    	{
        	Debug.Log("Found existing SoundManager instance");
        	soundManager.PlayStartScreenBGM(); // Start the music immediately
    	}
    }
    
    private void Update()
    {
        // Handle title screen input
        if (titleScreenActive && Input.anyKeyDown)
        {
            StartTutorial();
            return;
        }
        
        if (tutorialActive)
        {
            // Handle any key being pressed down
            HandleKeyDown();
            
            // Track hold duration of any key
            TrackKeyHolding();
            
            // Handle key up events
            HandleKeyUp();
        }
    }
    
    private void HandleKeyDown()
    {
        // Check for any key press
        if (!isSkipHolding)
        {
            // Right Arrow
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                rightArrowPressed = true;
                StartHolding(KeyCode.RightArrow);
            }
            // Left Arrow
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                leftArrowPressed = true;
                StartHolding(KeyCode.LeftArrow);
            }
            // Space or Mouse
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                StartHolding(KeyCode.Space);
            }
        }
    }
    
    private void StartHolding(KeyCode key)
    {
        isSkipHolding = true;
        skipHoldStartTime = Time.time;
        holdingKey = key;
        
        // Show the skip progress indicator
        displayManager.SetSkipBarsActive(true);
        Debug.Log($"Started holding key: {key} at time: {skipHoldStartTime}");
    }
    
    private void TrackKeyHolding()
    {
        // If we're currently holding a key
        if (isSkipHolding)
        {
            bool stillHolding = false;
            
            // Check if the specific key is still being held
            if (holdingKey == KeyCode.RightArrow && Input.GetKey(KeyCode.RightArrow))
                stillHolding = true;
            else if (holdingKey == KeyCode.LeftArrow && Input.GetKey(KeyCode.LeftArrow))
                stillHolding = true;
            else if (holdingKey == KeyCode.Space && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
                stillHolding = true;
                
            if (stillHolding)
            {
                // Calculate how long we've been holding
                float holdDuration = Time.time - skipHoldStartTime;
                float progress = holdDuration / skipHoldTime;
                
                // Update the progress bar
                displayManager.UpdateSkipProgress(Mathf.Clamp01(progress));
                
                // Log progress every 0.25 seconds for debugging
                if (Mathf.FloorToInt(holdDuration * 4) > Mathf.FloorToInt((holdDuration - Time.deltaTime) * 4))
                {
                    Debug.Log($"Holding {holdingKey} for {holdDuration:F2}s ({progress:P0})");
                }
                
                // Skip to game if held long enough
                if (holdDuration >= skipHoldTime)
                {
                    Debug.Log("Skip threshold reached!");
                    CancelHolding();
                    ForceStartGame();
                }
            }
            else
            {
                // Key was released early
                CancelHolding();
            }
        }
    }
    
    private void HandleKeyUp()
    {
        // Handle right arrow release (next slide)
        if (rightArrowPressed && Input.GetKeyUp(KeyCode.RightArrow))
        {
            rightArrowPressed = false;
            
            // Only navigate if we didn't hold long enough for skip
            if (!isSkipHolding)
            {
                NextSlide();
            }
        }
        
        // Handle left arrow release (previous slide)
        if (leftArrowPressed && Input.GetKeyUp(KeyCode.LeftArrow))
        {
            leftArrowPressed = false;
            
            // Only navigate if we didn't hold long enough for skip
            if (!isSkipHolding)
            {
                PreviousSlide();
            }
        }
    }
    
    private void CancelHolding()
    {
        float heldDuration = Time.time - skipHoldStartTime;
        Debug.Log($"Canceled holding {holdingKey} after {heldDuration:F2}s");
        
        isSkipHolding = false;
        holdingKey = KeyCode.None;
        displayManager.SetSkipBarsActive(false);
    }
    
    private void ShowTitleScreen()
    {
        displayManager.SetTitleScreenActive(true);
        displayManager.SetTutorialActive(false);
        
        titleScreenActive = true;
        tutorialActive = false;
    }
    
    private void StartTutorial()
    {
        displayManager.SetTitleScreenActive(false);
        displayManager.SetTutorialActive(true);
        
        titleScreenActive = false;
        tutorialActive = true;
        
        // Show first slide
        currentSlideIndex = 0;
        displayManager.ShowTutorialSlide(currentSlideIndex);
    }
    
    private void NextSlide()
    {
        if (currentSlideIndex < slideCount - 1)
        {
            currentSlideIndex++;
            displayManager.ShowTutorialSlide(currentSlideIndex);
            Debug.Log($"Going to next slide: {currentSlideIndex}");
        }
        else
        {
            Debug.Log("At last slide, starting game");
            ForceStartGame();
        }
    }
    
    private void PreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            displayManager.ShowTutorialSlide(currentSlideIndex);
            Debug.Log($"Going to previous slide: {currentSlideIndex}");
        }
        else
        {
            Debug.Log("At first slide, returning to title");
            ShowTitleScreen();
        }
    }
    
    private void ForceStartGame()
    {
        Debug.Log("FORCING GAME START: " + gameSceneName);
        soundManager.StopAll();
        SceneManager.LoadScene("GameScene");
    }
		

	private IEnumerator WaitForSoundManagerInit()
	{
    	// Wait a frame to ensure the SoundManager has time to initialize
    	yield return null;
    
    	// Now play the music
    	if (soundManager != null)
    	{
        	Debug.Log("Starting music after SoundManager creation");
        	soundManager.PlayStartScreenBGM();
    	}	
    	else
    	{
        	Debug.LogError("SoundManager reference lost after creation!");
    	}
	}
    
}