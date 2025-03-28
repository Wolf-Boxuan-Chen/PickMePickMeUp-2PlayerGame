// This is a helper script for setting up the InactivityUI in a scene
using UnityEngine;
using UnityEngine.UI;

public class InactivityUISetup : MonoBehaviour
{
    [SerializeField] private Canvas inactivityCanvas;
    [SerializeField] private GameObject inactivityPanel;
    
    private void Start()
    {
        // Ensure the canvas is set to screen space overlay
        if (inactivityCanvas != null)
        {
            inactivityCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            inactivityCanvas.sortingOrder = 1000; // Ensure it's on top of everything
        }
        
        // If InactivityManager exists, assign references
        InactivityManager manager = InactivityManager.Instance;
        if (manager != null && inactivityPanel != null)
        {
            // Use reflection to set the references
            var panelField = typeof(InactivityManager).GetField("inactivityPanel", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
            if (panelField != null)
                panelField.SetValue(manager, inactivityPanel);
                
            // Find and assign the text components
            var leftTextField = typeof(InactivityManager).GetField("leftPromptText", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
            var rightTextField = typeof(InactivityManager).GetField("rightPromptText", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
            var dimOverlayField = typeof(InactivityManager).GetField("dimOverlay", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
                                
            if (leftTextField != null)
                leftTextField.SetValue(manager, inactivityPanel.transform.Find("LeftPanel/PromptText").GetComponent<TMPro.TMP_Text>());
                
            if (rightTextField != null)
                rightTextField.SetValue(manager, inactivityPanel.transform.Find("RightPanel/PromptText").GetComponent<TMPro.TMP_Text>());
                
            if (dimOverlayField != null)
                dimOverlayField.SetValue(manager, inactivityPanel.transform.Find("DimOverlay").GetComponent<Image>());
        }
        
        // Initially hide the panel
        if (inactivityPanel != null)
            inactivityPanel.SetActive(false);
    }
}