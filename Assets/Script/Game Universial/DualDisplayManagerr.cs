using UnityEngine;
using UnityEngine.UI;

public class DualDisplayManager : MonoBehaviour
{
    [System.Serializable]
    public class DualElements
    {
        public GameObject leftElement;
        public GameObject rightElement;
    }

    [Header("Main Panels")]
    [SerializeField] private DualElements titleScreenPanels;
    [SerializeField] private DualElements tutorialPanels;
    
    [Header("Tutorial Slides")]
    [SerializeField] private DualElements[] tutorialSlides;
    
    [Header("Skip Bar")]
    [SerializeField] private DualElements skipBars;
    
    // Show/hide title screen on both monitors
    public void SetTitleScreenActive(bool active)
    {
        SetElementsActive(titleScreenPanels, active);
    }
    
    // Show/hide tutorial on both monitors
    public void SetTutorialActive(bool active)
    {
        SetElementsActive(tutorialPanels, active);
    }
    
    // Show specific tutorial slide on both monitors
    // Alternative implementation for ShowTutorialSlide
    public void ShowTutorialSlide(int slideIndex)
    {
        Debug.Log("Attempting to show slide " + slideIndex);
    
        // Hide all slides using direct activation
        for (int i = 0; i < tutorialSlides.Length; i++)
        {
            if (tutorialSlides[i].leftElement != null)
            {
                tutorialSlides[i].leftElement.SetActive(false);
                Debug.Log($"Direct deactivation of {tutorialSlides[i].leftElement.name}, now: {tutorialSlides[i].leftElement.activeSelf}");
            }
        
            if (tutorialSlides[i].rightElement != null)
            {
                tutorialSlides[i].rightElement.SetActive(false);
                Debug.Log($"Direct deactivation of {tutorialSlides[i].rightElement.name}, now: {tutorialSlides[i].rightElement.activeSelf}");
            }
        }
    
        // Show requested slide using direct activation
        if (slideIndex >= 0 && slideIndex < tutorialSlides.Length)
        {
            if (tutorialSlides[slideIndex].leftElement != null)
            {
                tutorialSlides[slideIndex].leftElement.SetActive(true);
                Debug.Log($"Direct activation of {tutorialSlides[slideIndex].leftElement.name}, now: {tutorialSlides[slideIndex].leftElement.activeSelf}");
            }
        
            if (tutorialSlides[slideIndex].rightElement != null)
            {
                tutorialSlides[slideIndex].rightElement.SetActive(true);
                Debug.Log($"Direct activation of {tutorialSlides[slideIndex].rightElement.name}, now: {tutorialSlides[slideIndex].rightElement.activeSelf}");
            }
        }
        else
        {
            Debug.LogError("Slide index out of range: " + slideIndex);
        }
    }
    
    // Show/hide skip bars on both monitors
    public void SetSkipBarsActive(bool active)
    {
        Debug.Log($"Setting skip bars active: {active}");
        
        if (skipBars.leftElement != null)
        {
            skipBars.leftElement.SetActive(active);
            Debug.Log($"Left skip bar activated: {skipBars.leftElement.activeSelf}");
        }
        else
        {
            Debug.LogError("Left skip bar reference missing!");
        }
        
        if (skipBars.rightElement != null)
        {
            skipBars.rightElement.SetActive(active);
            Debug.Log($"Right skip bar activated: {skipBars.rightElement.activeSelf}");
        }
        else
        {
            Debug.LogError("Right skip bar reference missing!");
        }
    }

    // Update skip bar progress on both monitors
    public void UpdateSkipProgress(float progress)
    {
        // Make sure to log details about what we're updating
        Debug.Log($"Updating skip progress: {progress:P0}");
        
        // Get the first Image component in each skip bar
        if (skipBars.leftElement != null)
        {
            // Try to find ANY Image component, not just direct children
            Image[] leftImages = skipBars.leftElement.GetComponentsInChildren<Image>();
            
            if (leftImages.Length > 0)
            {
                // Use the LAST Image component (assuming it's the progress circle)
                Image progressCircle = leftImages[leftImages.Length - 1];
                progressCircle.fillAmount = progress;
                Debug.Log($"Updated left progress circle: {progress:P0}, Image name: {progressCircle.name}");
            }
            else
            {
                Debug.LogError("No Image components found in left skip bar!");
            }
        }
        
        if (skipBars.rightElement != null)
        {
            // Try to find ANY Image component, not just direct children
            Image[] rightImages = skipBars.rightElement.GetComponentsInChildren<Image>();
            
            if (rightImages.Length > 0)
            {
                // Use the LAST Image component (assuming it's the progress circle)
                Image progressCircle = rightImages[rightImages.Length - 1];
                progressCircle.fillAmount = progress;
                Debug.Log($"Updated right progress circle: {progress:P0}, Image name: {progressCircle.name}");
            }
            else
            {
                Debug.LogError("No Image components found in right skip bar!");
            }
        }
    }
    // Helper to set both elements' active state
    private void SetElementsActive(DualElements elements, bool active)
    {
        if (elements.leftElement != null)
        {
            elements.leftElement.SetActive(active);
            Debug.Log($"SetActive {active} on {elements.leftElement.name}, is now {elements.leftElement.activeSelf}");
        }
    
        if (elements.rightElement != null)
        {
            elements.rightElement.SetActive(active);
            Debug.Log($"SetActive {active} on {elements.rightElement.name}, is now {elements.rightElement.activeSelf}");
        }
    }
}