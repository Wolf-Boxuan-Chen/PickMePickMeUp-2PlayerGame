using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [Header("Textures")]
    [SerializeField] private Sprite duringCallSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    
    [Header("Scrolling Settings")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float scrollSpeed = 0.2f;
    [Range(0, 360)]
    [SerializeField] private float scrollAngle = 335.0f; // 335 degrees = -25 degrees
    
    [Header("Tiling Settings")]
    [Range(0.05f, 2.0f)]
    [SerializeField] private float tileScale = 0.5f; // Adjust to change tile size
    
    [Header("References")]
    [SerializeField] private RawImage backgroundImage;
    
    private Vector2 uvOffset = Vector2.zero;
    private Vector2 scrollDirection;
    private bool isScrollingUp = true;
    
    private enum BackgroundState { DuringCall, Success, Failure }
    private BackgroundState currentState = BackgroundState.DuringCall;
    
    private void Start()
    {
        // Calculate scroll direction based on angle
        UpdateScrollDirection();
        
        // Set initial background
        SetBackground(BackgroundState.DuringCall);
        
        // Debug info
        Debug.Log($"ScrollingBackground initialized on {gameObject.name}");
        Debug.Log($"Tile scale: {tileScale}, Scroll angle: {scrollAngle}°");
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
            // Move in the calculated direction (for call and success)
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
        
        Debug.Log($"Updated scroll direction to {scrollDirection} based on angle {scrollAngle}°");
    }
    
    // Call this from Inspector when the angle is changed
    public void OnScrollAngleChanged()
    {
        UpdateScrollDirection();
    }
    
    // Call this from your panel manager when state changes
    public void SetState(string state)
    {
        Debug.Log($"Setting background state to: {state} on {gameObject.name}");
        
        switch (state.ToLower())
        {
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
            Debug.Log($"Set background texture to {spriteToUse.name} on {gameObject.name}");
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
        Debug.Log($"Updated tile scale to {tileScale}");
    }
    
    // Add this to make scroll speed adjustable at runtime
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = Mathf.Clamp(newSpeed, 0.01f, 1.0f);
        Debug.Log($"Updated scroll speed to {scrollSpeed}");
    }
    
    // Add this to make scroll angle adjustable at runtime
    public void SetScrollAngle(float newAngle)
    {
        scrollAngle = newAngle % 360f; // Keep within 0-360 range
        UpdateScrollDirection();
    }
    
    // Add test buttons for Inspector debugging
    [ContextMenu("Test DuringCall State")]
    public void TestDuringCallState() => SetState("duringcall");
    
    [ContextMenu("Test Success State")]
    public void TestSuccessState() => SetState("success");
    
    [ContextMenu("Test Failure State")]
    public void TestFailureState() => SetState("failure");
    
    [ContextMenu("Reset UV Offset")]
    public void ResetUVOffset() => uvOffset = Vector2.zero;
}