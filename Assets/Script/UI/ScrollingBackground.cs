using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScrollingBackground : MonoBehaviour
{
    [Header("Textures")]
    [SerializeField] private Sprite backgroundSprite; // New dedicated background sprite
    [SerializeField] private Sprite duringCallSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    
    [Header("Scrolling Settings")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float scrollSpeed = 0.2f;
    [Range(0, 360)]
    [SerializeField] private float scrollAngle = 335.0f;
    
    [Header("Tiling Settings")]
    [Range(0.05f, 2.0f)]
    [SerializeField] private float tileScale = 0.5f;
    
    [Header("References")]
    [SerializeField] private RawImage backgroundImage;
    
    // Scene names to check against
    private readonly string startSceneName = "StartInstructionScene";
    private readonly string gameOverSceneName = "GameOverScene";
    
    private Vector2 uvOffset = Vector2.zero;
    private Vector2 scrollDirection;
    private bool isScrollingUp = true;
    
    private enum BackgroundState { Background, DuringCall, Success, Failure }
    private BackgroundState currentState = BackgroundState.Background;
    
    private void Start()
    {
        // Calculate scroll direction based on angle
        UpdateScrollDirection();
        
        // Auto-detect current scene and set appropriate background
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == startSceneName || currentScene == gameOverSceneName)
        {
            // Start and GameOver scenes use the standard background
            SetBackground(BackgroundState.Background);
            Debug.Log($"ScrollingBackground: Using dedicated background sprite for {currentScene}");
        }
        else
        {
            // Game scene - set initial background (will be updated by game logic)
            SetBackground(BackgroundState.DuringCall);
        }
    }
    
    private void Update()
    {
        if (backgroundImage == null)
        {
            Debug.LogError("No RawImage assigned to ScrollingBackground!");
            return;
        }
        
        // Update UV offset based on direction and speed
        if (isScrollingUp)
        {
            // Move in the calculated direction (for background, call and success)
            uvOffset += scrollDirection * (scrollSpeed * Time.deltaTime);
        }
        else
        {
            // Move in the opposite direction (for failure)
            uvOffset -= scrollDirection * (scrollSpeed * Time.deltaTime);
        }
        
        // Apply UV offset to the image with tiling scale
        backgroundImage.uvRect = new Rect(uvOffset, Vector2.one * tileScale);
    }
    
    // Update scroll direction when angle changes
    public void UpdateScrollDirection()
    {
        // Convert angle to radians (Unity uses radians for trig functions)
        float radians = scrollAngle * Mathf.Deg2Rad;
        
        // Calculate direction using sine and cosine
        scrollDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
    
    // Call this from Inspector when the angle is changed
    public void OnScrollAngleChanged()
    {
        UpdateScrollDirection();
    }
    
    // Call this from your panel manager when state changes
    public void SetState(string state)
    {
        switch (state.ToLower())
        {
            case "background":
                SetBackground(BackgroundState.Background);
                isScrollingUp = true;
                break;
                
            case "call":
            case "duringcall":
                SetBackground(BackgroundState.DuringCall);
                isScrollingUp = true;
                break;
                
            case "success":
                SetBackground(BackgroundState.Success);
                isScrollingUp = true;
                break;
                
            case "failure":
                SetBackground(BackgroundState.Failure);
                isScrollingUp = false; // Moving in opposite direction
                break;
                
            default:
                Debug.LogWarning("Unknown background state: " + state);
                break;
        }
    }
    
    private void SetBackground(BackgroundState state)
    {
        if (backgroundImage == null)
        {
            Debug.LogError("Cannot set background: No RawImage assigned!");
            return;
        }
        
        currentState = state;
        Sprite spriteToUse = null;
        
        switch (state)
        {
            case BackgroundState.Background:
                spriteToUse = backgroundSprite;
                break;
                
            case BackgroundState.DuringCall:
                spriteToUse = duringCallSprite;
                break;
                
            case BackgroundState.Success:
                spriteToUse = successSprite;
                break;
                
            case BackgroundState.Failure:
                spriteToUse = failureSprite;
                break;
        }
        
        // Set sprite for this panel
        if (spriteToUse != null)
        {
            backgroundImage.texture = spriteToUse.texture;
            backgroundImage.color = Color.white; // Ensure color is set to white
        }
        else
        {
            Debug.LogError($"Cannot set background: No sprite assigned for state {state}!");
        }
    }
    
    // Add this to make tile scale adjustable at runtime
    public void SetTileScale(float newScale)
    {
        tileScale = Mathf.Clamp(newScale, 0.05f, 2.0f);
    }
    
    // Add this to make scroll speed adjustable at runtime
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = Mathf.Clamp(newSpeed, 0.01f, 1.0f);
    }
    
    // Add this to make scroll angle adjustable at runtime
    public void SetScrollAngle(float newAngle)
    {
        scrollAngle = newAngle % 360f; // Keep within 0-360 range
        UpdateScrollDirection();
    }
}