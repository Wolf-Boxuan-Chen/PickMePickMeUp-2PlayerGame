using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{
    [SerializeField] private Button clearDataButton;
    
    private void Start()
    {
        // Initialize button listener
        if (clearDataButton != null)
        {
            clearDataButton.onClick.AddListener(ClearGameData);
        }
    }
    
    public void ClearGameData()
    {
        // Delete all PlayerPrefs data
        PlayerPrefs.DeleteAll();
        
        // Log confirmation
        Debug.Log("All game data has been cleared!");
        
        // Optional: Show confirmation to player
        ShowConfirmationMessage();
    }
    
    private void ShowConfirmationMessage()
    {
        // You can implement this to show a UI message
        // For example, activate a text message for a few seconds
    }
    
    private void OnDestroy()
    {
        // Clean up listener when script is destroyed
        if (clearDataButton != null)
        {
            clearDataButton.onClick.RemoveListener(ClearGameData);
        }
    }
}