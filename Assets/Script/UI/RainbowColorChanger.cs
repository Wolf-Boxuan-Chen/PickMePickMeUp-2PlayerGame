using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RainbowColorChanger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image leftMonitorImage;
    [SerializeField] private Image rightMonitorImage;
    
    [Header("Color Settings")]
    [SerializeField] private float colorChangeSpeed = 4f;
    [SerializeField] private float saturationMultiplier = 1f;
    
    // The custom colors you provided
    private List<Color> customColors = new List<Color>();
    private int currentColorIndex = 0;
    private float colorLerpTime = 0f;
    
    // Game state reference
    private GameManager gameManager;
    
    void Start()
    {
        // Find game manager
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Add your custom colors (from the hex values you provided)
        AddCustomColors();
    }
    
    void Update()
    {
        // Check if we can access the incoming call state
        if (IsIncomingCallActive())
        {
            UpdateColors();
        }
    }
    
    private void AddCustomColors()
    {
        // Add the colors from your set (less saturated versions)
        customColors.Add(HexToColor("FF5F5F", saturationMultiplier)); // Red
        customColors.Add(HexToColor("FFC75F", saturationMultiplier)); // Orange
        customColors.Add(HexToColor("DCFF5F", saturationMultiplier)); // Yellow-green
        customColors.Add(HexToColor("74FF5F", saturationMultiplier)); // Green
        customColors.Add(HexToColor("5FFFBA", saturationMultiplier)); // Teal
        customColors.Add(HexToColor("5FBAFF", saturationMultiplier)); // Blue
        customColors.Add(HexToColor("8C5FFF", saturationMultiplier)); // Purple
        //customColors.Add(HexToColor("FF5FCC", saturationMultiplier)); // Pink
        //customColors.Add(HexToColor("FF5F7C", saturationMultiplier)); // Red-pink
    }
    
    private void UpdateColors()
    {
        // Increase the lerp time
        colorLerpTime += Time.deltaTime * colorChangeSpeed;
        
        // Check if we should move to the next color
        if (colorLerpTime >= 1.0f)
        {
            // Reset lerp time
            colorLerpTime = 0f;
            
            // Move to next color
            currentColorIndex = (currentColorIndex + 1) % customColors.Count;
        }
        
        // Calculate the next color index
        int nextColorIndex = (currentColorIndex + 1) % customColors.Count;
        
        // Lerp between current and next color
        Color lerpedColor = Color.Lerp(
            customColors[currentColorIndex], 
            customColors[nextColorIndex], 
            colorLerpTime
        );
        
        // Apply color to both images
        if (leftMonitorImage != null)
            leftMonitorImage.color = lerpedColor;
            
        if (rightMonitorImage != null)
            rightMonitorImage.color = lerpedColor;
    }
    
    // Helper method to check if incoming call is active
    private bool IsIncomingCallActive()
    {
        // Modified to use direct field access to match your GameManager structure
        if (gameManager != null)
        {
            // Check if we're in the incoming call state
            // Assuming the call is incoming when callActive is false and resultActive is false
            bool incomingActive = false;
            
            // Use reflection to safely access private fields
            System.Reflection.FieldInfo callActiveField = typeof(GameManager).GetField("callActive", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            System.Reflection.FieldInfo resultActiveField = typeof(GameManager).GetField("resultActive", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            if (callActiveField != null && resultActiveField != null)
            {
                bool isCallActive = (bool)callActiveField.GetValue(gameManager);
                bool isResultActive = (bool)resultActiveField.GetValue(gameManager);
                
                // Incoming call is active when neither call nor result is active
                incomingActive = !isCallActive && !isResultActive;
                
                //Debug.Log($"Call active: {isCallActive}, Result active: {isResultActive}, Incoming: {incomingActive}");
            }
            else
            {
                Debug.LogWarning("Could not access GameManager state fields");
            }
            
            return incomingActive;
        }
        
        // Alternative: Check if the incoming call panel is active
        // If you have references to the panels, you could check:
        // return incomingCallPanelLeft.activeSelf && incomingCallPanelRight.activeSelf;
        
        return true; // Default fallback
    }
    
    // Helper method to convert hex color to Unity Color
    private Color HexToColor(string hex, float satMult = 1.0f)
    {
        // Remove # if present
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);
            
        // Parse the hex values
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        
        // Convert to HSV to reduce saturation
        Color.RGBToHSV(new Color(r, g, b), out float h, out float s, out float v);
        
        // Apply saturation multiplier
        s *= satMult;
        
        // Convert back to RGB
        return Color.HSVToRGB(h, s, v);
    }
}