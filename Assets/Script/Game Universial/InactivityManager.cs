using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InactivityManager : MonoBehaviour
{
    // Singleton instance
    public static InactivityManager Instance { get; private set; }
    
    [Header("Inactivity Settings")]
    [SerializeField] private float inactivityTime = 60.0f; // Time in seconds before showing prompt
    [SerializeField] private float responseTime = 15.0f;   // Time to respond to the prompt
    
    [Header("Scene Navigation")]
    [SerializeField] private string startSceneName = "StartScene"; // Name of start scene to return to
    
    // References to UI elements
    private GameObject leftInactivityPanel;
    private GameObject rightInactivityPanel;
    private TMP_Text leftPromptText;
    private TMP_Text rightPromptText;
    private Image leftDimOverlay;
    private Image rightDimOverlay;
    
    // References to title screens
    private GameObject leftTitleScreen;
    private GameObject rightTitleScreen;
    
    private float lastInputTime;
    private bool promptActive = false;
    private bool isExiting = false;
    private Coroutine responseCoroutine;
    
    private void Awake()
    {
        // Singleton implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Initialize the last input time
        ResetInactivityTimer();
        
        // Register for scene changes to find UI components
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Find UI components in the current scene
        FindUIComponents();
    }
    
    private void OnDestroy()
    {
        // Unregister event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Give time for all objects to initialize
        StartCoroutine(FindUIComponentsDelayed());
        
        // Reset the timer when a new scene loads
        ResetInactivityTimer();
        
        // Cancel any active prompt
        if (promptActive)
        {
            CancelExit();
        }
    }
    
    private IEnumerator FindUIComponentsDelayed()
    {
        // Wait for a frame to ensure all objects are initialized
        yield return null;
        
        // Find the inactivity panels in the new scene
        FindUIComponents();
    }
    
    // Find UI components in the current scene
    private void FindUIComponents()
    {
        // Find inactivity panels by name instead of tag
        leftInactivityPanel = GameObject.Find("InactivityPanel_L");
        rightInactivityPanel = GameObject.Find("InactivityPanel_R");
        
        // Find text components and overlays if panels exist
        if (leftInactivityPanel != null)
        {
            leftPromptText = leftInactivityPanel.GetComponentInChildren<TMP_Text>();
            Transform overlayTransform = leftInactivityPanel.transform.Find("DimOverlay");
            if (overlayTransform != null)
                leftDimOverlay = overlayTransform.GetComponent<Image>();
                
            // Ensure panel starts inactive
            leftInactivityPanel.SetActive(false);
        }
        
        if (rightInactivityPanel != null)
        {
            rightPromptText = rightInactivityPanel.GetComponentInChildren<TMP_Text>();
            Transform overlayTransform = rightInactivityPanel.transform.Find("DimOverlay");
            if (overlayTransform != null)
                rightDimOverlay = overlayTransform.GetComponent<Image>();
                
            // Ensure panel starts inactive
            rightInactivityPanel.SetActive(false);
        }
        
        // Find title screens by name
        leftTitleScreen = GameObject.Find("TitleScreen_L");
        rightTitleScreen = GameObject.Find("TitleScreen_R");
            
        Debug.Log($"Inactivity UI components found: Left Panel: {leftInactivityPanel != null}, Right Panel: {rightInactivityPanel != null}");
        Debug.Log($"Title screens found: Left: {leftTitleScreen != null}, Right: {rightTitleScreen != null}");
    }
    
    private void Update()
    {
        // Skip inactivity check if title screens are active
        bool titleScreensActive = (leftTitleScreen != null && leftTitleScreen.activeInHierarchy) || 
                                 (rightTitleScreen != null && rightTitleScreen.activeInHierarchy);
        
        if (titleScreensActive)
        {
            // Reset timer while on title screen
            ResetInactivityTimer();
            return;
        }
            
        // Check for any input
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // If the prompt is active and the user presses right arrow, cancel the exit
            if (promptActive && Input.GetKeyDown(KeyCode.RightArrow))
            {
                CancelExit();
                return;
            }
            
            // Otherwise, reset the timer on any input
            if (!promptActive)
            {
                ResetInactivityTimer();
            }
        }
        
        // If we're not already prompting and time has elapsed
        if (!promptActive && !isExiting && Time.time - lastInputTime > inactivityTime)
        {
            ShowExitPrompt();
        }
    }
    
    // Reset the inactivity timer
    public void ResetInactivityTimer()
    {
        lastInputTime = Time.time;
    }
    
    // Show the exit prompt
    private void ShowExitPrompt()
    {
        // If panels don't exist, try to find them again
        if (leftInactivityPanel == null || rightInactivityPanel == null)
        {
            FindUIComponents();
            
            // If still not found, create them
            if (leftInactivityPanel == null || rightInactivityPanel == null)
            {
                CreateInactivityPanels();
            }
        }
        
        promptActive = true;
        
        // Show the panels - log before and after states for debugging
        if (leftInactivityPanel != null)
        {
            Debug.Log($"Left panel before activation: {leftInactivityPanel.activeSelf}");
            leftInactivityPanel.SetActive(true);
            Debug.Log($"Left panel after activation: {leftInactivityPanel.activeSelf}");
        }
            
        if (rightInactivityPanel != null)
        {
            Debug.Log($"Right panel before activation: {rightInactivityPanel.activeSelf}");
            rightInactivityPanel.SetActive(true);
            Debug.Log($"Right panel after activation: {rightInactivityPanel.activeSelf}");
        }
            
        // Set the prompt text
        string promptMessage = "Press green button in 15 seconds to tell me you're still here!";
        
        if (leftPromptText != null)
            leftPromptText.text = promptMessage;
            
        if (rightPromptText != null)
            rightPromptText.text = promptMessage;
            
        // Set the dim overlays
        if (leftDimOverlay != null)
        {
            Color dimColor = leftDimOverlay.color;
            dimColor.a = 0.7f; // 70% dimming
            leftDimOverlay.color = dimColor;
        }
        
        if (rightDimOverlay != null)
        {
            Color dimColor = rightDimOverlay.color;
            dimColor.a = 0.7f; // 70% dimming
            rightDimOverlay.color = dimColor;
        }
        
        // Start the response timer
        responseCoroutine = StartCoroutine(WaitForResponse());
    }
    
    // Create inactivity panels if they don't exist
    private void CreateInactivityPanels()
    {
        Debug.Log("Creating inactivity panels dynamically");
        
        // Find the left and right canvases
        GameObject leftCanvas = GameObject.Find("Display_monitor_left/Canvas");
        GameObject rightCanvas = GameObject.Find("Display_monitor_right/Canvas");
        
        if (leftCanvas != null)
        {
            leftInactivityPanel = CreateInactivityPanel(leftCanvas.transform, "InactivityPanel_L");
        }
        
        if (rightCanvas != null)
        {
            rightInactivityPanel = CreateInactivityPanel(rightCanvas.transform, "InactivityPanel_R");
        }
    }
    
    // Helper to create inactivity panel with required components
    private GameObject CreateInactivityPanel(Transform canvasTransform, string panelName)
    {
        // Create panel object
        GameObject panel = new GameObject(panelName);
        panel.transform.SetParent(canvasTransform, false);
        
        // Add RectTransform and set to fill
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add dim overlay
        GameObject overlay = new GameObject("DimOverlay");
        overlay.transform.SetParent(panel.transform, false);
        
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.7f);
        
        // Add prompt text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMP_Text promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press green button in 15 seconds to tell me you're still here!";
        promptText.color = Color.white;
        promptText.fontSize = 36;
        promptText.alignment = TextAlignmentOptions.Center;
        
        // Store references
        if (panelName == "InactivityPanel_L")
        {
            leftPromptText = promptText;
            leftDimOverlay = overlayImage;
        }
        else
        {
            rightPromptText = promptText;
            rightDimOverlay = overlayImage;
        }
        
        // Make inactive initially
        panel.SetActive(false);
        
        Debug.Log($"Created inactivity panel: {panelName}");
        return panel;
    }
    
    // Wait for the player to respond
    private IEnumerator WaitForResponse()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < responseTime)
        {
            // Update the countdown if text objects exist
            int secondsLeft = Mathf.CeilToInt(responseTime - elapsedTime);
            string countdownText = $"Press green button in {secondsLeft} seconds to tell me you're still here!";
            
            if (leftPromptText != null)
                leftPromptText.text = countdownText;
                
            if (rightPromptText != null)
                rightPromptText.text = countdownText;
            
            // Check for right arrow key (green button)
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                CancelExit();
                yield break;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Time expired without response - return to start scene
        ReturnToStartScene();
    }
    
    // Cancel the exit process
    private void CancelExit()
    {
        promptActive = false;
        
        // Stop the response coroutine if it's running
        if (responseCoroutine != null)
        {
            StopCoroutine(responseCoroutine);
            responseCoroutine = null;
        }
        
        // Hide the panels
        if (leftInactivityPanel != null)
            leftInactivityPanel.SetActive(false);
            
        if (rightInactivityPanel != null)
            rightInactivityPanel.SetActive(false);
            
        // Reset the timer
        ResetInactivityTimer();
        
        Debug.Log("Exit canceled - player is still active");
    }
    
    // Return to start scene instead of quitting
    private void ReturnToStartScene()
    {
        isExiting = true;
        Debug.Log("Player inactive - returning to start scene");
        
        // Hide panels before transitioning
        if (leftInactivityPanel != null)
            leftInactivityPanel.SetActive(false);
            
        if (rightInactivityPanel != null)
            rightInactivityPanel.SetActive(false);
            
        // Load the start scene
        SceneManager.LoadScene(startSceneName);
        
        // Reset state for next time
        promptActive = false;
        isExiting = false;
        ResetInactivityTimer();
    }
}