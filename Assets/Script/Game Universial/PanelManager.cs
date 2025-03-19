using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelManager : MonoBehaviour
{
    [System.Serializable]
    public class DualPanels
    {
        public GameObject leftPanel;
        public GameObject rightPanel;
    }
    
    [Header("Game Panels")]
    [SerializeField] private DualPanels incomingCallPanels;
    [SerializeField] private DualPanels callActivePanels;
    [SerializeField] private DualPanels resultPanels;
    [SerializeField] private DualPanels timerPanels;
    [SerializeField] private DualPanels livesPanels; // Added separate lives panels
    
    [Header("Result Elements")]
    [SerializeField] private TextMeshProUGUI leftResultText;
    [SerializeField] private TextMeshProUGUI rightResultText;
    [SerializeField] private TextMeshProUGUI leftCountdownText;
    [SerializeField] private TextMeshProUGUI rightCountdownText;
    [SerializeField] private Image leftResultIcon;
    [SerializeField] private Image rightResultIcon;
    [SerializeField] private Sprite successIcon;
    [SerializeField] private Sprite failureIcon;
    
    [Header("Time Display")]
    [SerializeField] private TextMeshProUGUI leftTimerText;
    [SerializeField] private TextMeshProUGUI rightTimerText;
    
    [Header("Lives Display")]
    [SerializeField] private Image[] leftLifeImages;
    [SerializeField] private Image[] rightLifeImages;
    [SerializeField] private Sprite activeLifeSprite;
    [SerializeField] private Sprite inactiveLifeSprite;
    
    [Header("Incoming Call Elements")]
    [SerializeField] private TextMeshProUGUI leftIncomingCallText;
    [SerializeField] private TextMeshProUGUI rightIncomingCallText;
    
    // Show incoming call screen
    public void ShowIncomingCallPanel()
    {
        SetPanelsActive(incomingCallPanels, true);
        SetPanelsActive(callActivePanels, false);
        SetPanelsActive(resultPanels, false);
        SetPanelsActive(timerPanels, false);
        SetPanelsActive(livesPanels, false); // Hide lives during incoming call
    }
    
    // Show active call screen
    public void ShowCallActivePanel()
    {
        SetPanelsActive(incomingCallPanels, false);
        SetPanelsActive(callActivePanels, true);
        SetPanelsActive(resultPanels, false);
        SetPanelsActive(timerPanels, true);
        SetPanelsActive(livesPanels, true); // Show lives during active call
    }
    
    // Show result panel
    public void ShowResultPanel(string resultMessage, bool isSuccess)
    {
        SetPanelsActive(incomingCallPanels, false);
        SetPanelsActive(callActivePanels, false);
        SetPanelsActive(resultPanels, true);
        SetPanelsActive(timerPanels, false);
        SetPanelsActive(livesPanels, true); // Show lives during result
        
        // Set result text
        if (leftResultText != null) leftResultText.text = resultMessage;
        if (rightResultText != null) rightResultText.text = resultMessage;
        
        // Set result icon
        Sprite iconToUse = isSuccess ? successIcon : failureIcon;
        if (leftResultIcon != null) leftResultIcon.sprite = iconToUse;
        if (rightResultIcon != null) rightResultIcon.sprite = iconToUse;
        
        // Initialize countdown text
        if (leftCountdownText != null) leftCountdownText.text = "Next call in 5...";
        if (rightCountdownText != null) rightCountdownText.text = "Next call in 5...";
    }
    
    // Update countdown text for next call
    public void UpdateResultCountdownText(string countdownText)
    {
        if (leftCountdownText != null) leftCountdownText.text = countdownText;
        if (rightCountdownText != null) rightCountdownText.text = countdownText;
    }
    
    // Helper to set both panels' active state
    private void SetPanelsActive(DualPanels panels, bool active)
    {
        if (panels.leftPanel != null) panels.leftPanel.SetActive(active);
        if (panels.rightPanel != null) panels.rightPanel.SetActive(active);
    }
    
    // Update timer text on both screens
    public void UpdateTimerText(string timeText)
    {
        if (leftTimerText != null) leftTimerText.text = timeText;
        if (rightTimerText != null) rightTimerText.text = timeText;
    }
    
    // Update lives display
    public void UpdateLives(int livesRemaining)
    {
        // Update left lives
        for (int i = 0; i < leftLifeImages.Length; i++)
        {
            if (leftLifeImages[i] != null)
                leftLifeImages[i].sprite = (i < livesRemaining) ? activeLifeSprite : inactiveLifeSprite;
        }
        
        // Update right lives
        for (int i = 0; i < rightLifeImages.Length; i++)
        {
            if (rightLifeImages[i] != null)
                rightLifeImages[i].sprite = (i < livesRemaining) ? activeLifeSprite : inactiveLifeSprite;
        }
    }
}