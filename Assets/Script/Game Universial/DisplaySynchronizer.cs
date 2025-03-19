using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySynchronizer : MonoBehaviour
{
    [System.Serializable]
    public class SyncedDisplays
    {
        public GameObject leftMonitorElement;
        public GameObject rightMonitorElement;
    }
    
    [Header("UI Elements to Sync")]
    [SerializeField] private SyncedDisplays incomingCallPanel;
    [SerializeField] private SyncedDisplays callActivePanel;
    [SerializeField] private SyncedDisplays resultPanel;
    [SerializeField] private SyncedDisplays nextCallPanel;
    [SerializeField] private SyncedDisplays timerPanel;
    
    [Header("Lives Display")]
    [SerializeField] private Image[] leftLivesImages;
    [SerializeField] private Image[] rightLivesImages;
    [SerializeField] private Sprite activeLifeSprite;
    [SerializeField] private Sprite inactiveLifeSprite;
    
    [Header("Text Elements to Sync")]
    [SerializeField] private TMP_Text leftTimerText;
    [SerializeField] private TMP_Text rightTimerText;
    [SerializeField] private TMP_Text leftResultText;
    [SerializeField] private TMP_Text rightResultText;
    [SerializeField] private TMP_Text leftNextCallText;
    [SerializeField] private TMP_Text rightNextCallText;
    
    // Methods to sync panels across both displays
    public void SetIncomingCallPanelActive(bool active)
    {
        SetPanelActive(incomingCallPanel, active);
    }
    
    public void SetCallActivePanelActive(bool active)
    {
        SetPanelActive(callActivePanel, active);
    }
    
    public void SetResultPanelActive(bool active)
    {
        SetPanelActive(resultPanel, active);
    }
    
    public void SetNextCallPanelActive(bool active)
    {
        SetPanelActive(nextCallPanel, active);
    }
    
    public void SetTimerPanelActive(bool active)
    {
        SetPanelActive(timerPanel, active);
    }
    
    private void SetPanelActive(SyncedDisplays panels, bool active)
    {
        if (panels.leftMonitorElement != null)
            panels.leftMonitorElement.SetActive(active);
            
        if (panels.rightMonitorElement != null)
            panels.rightMonitorElement.SetActive(active);
    }
    
    // Method to update lives display
    public void UpdateLives(int currentLives, int maxLives)
    {
        // Update left monitor lives
        for (int i = 0; i < leftLivesImages.Length; i++)
        {
            if (leftLivesImages[i] != null)
            {
                if (i < currentLives)
                    leftLivesImages[i].sprite = activeLifeSprite;
                else
                    leftLivesImages[i].sprite = inactiveLifeSprite;
            }
        }
        
        // Update right monitor lives
        for (int i = 0; i < rightLivesImages.Length; i++)
        {
            if (rightLivesImages[i] != null)
            {
                if (i < currentLives)
                    rightLivesImages[i].sprite = activeLifeSprite;
                else
                    rightLivesImages[i].sprite = inactiveLifeSprite;
            }
        }
    }
    
    // Methods to sync text across both displays
    public void SetTimerText(string text)
    {
        SetText(leftTimerText, rightTimerText, text);
    }
    
    public void SetResultText(string text)
    {
        SetText(leftResultText, rightResultText, text);
    }
    
    public void SetNextCallText(string text)
    {
        SetText(leftNextCallText, rightNextCallText, text);
    }
    
    private void SetText(TMP_Text leftText, TMP_Text rightText, string text)
    {
        if (leftText != null)
            leftText.text = text;
            
        if (rightText != null)
            rightText.text = text;
    }
}