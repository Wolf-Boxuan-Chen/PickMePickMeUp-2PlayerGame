using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RainbowColorChanger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image leftMonitorImage;
    [SerializeField] private Image rightMonitorImage;
    
    [Header("Color Settings")]
    [SerializeField] private float colorChangeSpeed = 1.0f;
    [SerializeField] private bool useCustomColors = true;
    
    // The custom colors you provided
    private List<Color> customColors = new List<Color>();
    private int currentColorIndex = 0;
    private float colorLerpTime = 0f;
    
    // Game state reference
    private GameManager gameManager;
    
    void Start()
    {
        // Find game manager
        gameManager = FindObjectOfType<GameManager>();
        
        // Add your custom colors (from the hex values you provided)
        AddCustomColors();
    }
    
    void Update()
    {
        // Only update during incoming call
        if (IsIncomingCallActive())
        {
            UpdateColors();
        }
    }
    
    private void AddCustomColors()
    {
        // Add the colors from your set (less saturated versions)
        customColors.Add(HexToColor("FF5F5F", 0.8f)); // Red
        customColors.Add(HexToColor("FFC75F", 0.8f)); // Orange
        customColors.Add(HexToColor("DCFF5F", 0.8f)); // Yellow-green
        customColors.Add(HexToColor("74FF5F", 0.8f)); // Green
        customColors.Add(HexToColor("5FFFB4", 0.8f)); // Teal
        customColors.Add(HexToColor("5FC2FF", 0.8f)); // Blue
        customColors.Add(HexToColor("7F5FFF", 0.8f)); // Purple
        customColors.Add(HexToColor("FF5FFA", 0.8f)); // Pink
        customColors.Add(HexToColor("FF5F7C", 0.8f)); // Red-pink
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
        // You may need to modify this based on your actual game state tracking
        // This is a placeholder implementation
        
        // Option 1: Using callActive state in GameManager (if accessible)
        if (gameManager != null)
            return !gameManager.IsCallActive() && !gameManager.IsResultActive();
            
        // Option 2: Checking if the incoming call panel is active
        // This would need references to those panels
        return true; // Default fallback
    }
    
    // Helper method to convert hex color to Unity Color
    private Color HexToColor(string hex, float saturationMultiplier = 1.0f)
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
        s *= saturationMultiplier;
        
        // Convert back to RGB
        return Color.HSVToRGB(h, s, v);
    }
}