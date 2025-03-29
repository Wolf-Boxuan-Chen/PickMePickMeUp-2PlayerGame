using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [System.Serializable]
    public class ScoreEntry
    {
        public int score;
        public long id; // Timestamp to make entries unique
        
        public ScoreEntry(int score, long id)
        {
            this.score = score;
            this.id = id;
        }
    }

    [System.Serializable]
    public class DualTextElements
    {
        public TMP_Text leftMonitorText;
        public TMP_Text rightMonitorText;
    }

    [System.Serializable]
    public class DualPanels
    {
        public GameObject leftPanel;
        public GameObject rightPanel;
    }

    [Header("Game Over UI Elements")]
    [SerializeField] private DualTextElements gameOverText;
    [SerializeField] private DualTextElements rankingText;
    [SerializeField] private DualTextElements highScoresText;
    [SerializeField] private DualTextElements restartPromptText;
    [SerializeField] private DualPanels gameOverPanels;
    
    [Header("Credits UI Elements")]
    [SerializeField] private DualPanels creditsPanels;
    [SerializeField] private DualTextElements resultInText;
    
    [Header("Navigation")]
    [SerializeField] private string startSceneName = "StartInstructionScene";
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Settings")]
    [SerializeField] private int topScoresToShow = 5;
    [SerializeField] private int maxStoredScores = 300;
	
	[Header("Sound Manager")]
	[SerializeField] private SoundManager soundManager;

    
    private int currentCallCount;
    private float moneyLost;
    
    // Various result text options
    private string[] scamResultTexts = new string[] {
        "\"I promise to NEVER get scammed again after being scammed to <color=red>lose my one and only friend</color> and all my money - <color=red>${0}</color> - in just <color=red>{1}</color> phone calls.\"",
        "\"I thought I was so smart until I got scammed and <color=red>lost everything I ever cared about</color> plus <color=red>${0}</color> after just <color=red>{1}</color> phone calls.\"",
        "\"I'm never going to trust any call again. I got scammed and <color=red>lost my only friend</color> plus <color=red>${0}</color> after just <color=red>{1}</color> phone calls.\""
    };
    
    // Current panel state tracking
    private enum PanelState { GameOver, Credits }
    private PanelState currentState = PanelState.GameOver;
    
    private void Start()
    {
        // Get the player's data from PlayerPrefs
        currentCallCount = PlayerPrefs.GetInt("CallCount", 0);
        moneyLost = PlayerPrefs.GetFloat("MoneyLost", 0);
        
        // Update the score and save to high scores
        UpdateHighScores(currentCallCount);
        
        // Show game over info first
        ShowGameOverPanel();
        
        // Listen for input
        StartCoroutine(WaitForRestartInput());
		
		// Make sure we have a reference to the SoundManager
    	if (soundManager == null)
        	soundManager = SoundManager.Instance;
        
    	// Play end screen music
    	soundManager.PlayEndScreenBGM();
    }
    
    private void ShowGameOverPanel()
    {
        // Display game over information
        string gameOverMessage = "You lost your friend, your house, your car, your pet, your everything " +
                                "after <color=red>" + currentCallCount + "</color> phone calls." ;
        
        // Show game over panel text
        SetDualText(gameOverText, gameOverMessage);
        
        // Show ranking with proper format
        List<ScoreEntry> allScores = GetAllScores();
        int rank = DetermineRank(currentCallCount, allScores);
        string rankText = $"Your Rank: {rank}/{allScores.Count}";
        SetDualText(rankingText, rankText);
        
        // Show top scores with arrow under player's score
        string scoresText = GenerateHighScoresText(allScores, rank);
        SetDualText(highScoresText, scoresText);
        
        // Set prompt for credits
        SetDualText(restartPromptText, "Press Green Button to Continue →");
        
        // Show the game over panels, hide credits panels
        SetPanelsActive(gameOverPanels, true);
        SetPanelsActive(creditsPanels, false);
        
        currentState = PanelState.GameOver;
    }

    private void ShowCreditsPanel()
    {
        // Hide game over panels, show credits panels
        SetPanelsActive(gameOverPanels, false);
        SetPanelsActive(creditsPanels, true);
        
        // Randomly select one of the result text options
        int randomTextIndex = Random.Range(0, scamResultTexts.Length);
        string selectedResultText = string.Format(scamResultTexts[randomTextIndex], 
                                                 moneyLost.ToString("F2"), 
                                                 currentCallCount);
        
        // Set result text
        SetDualText(resultInText, selectedResultText );
        
        currentState = PanelState.Credits;
    }
    
    // Helper to set both panels' active state
    private void SetPanelsActive(DualPanels panels, bool active)
    {
        if (panels.leftPanel != null)
            panels.leftPanel.SetActive(active);
            
        if (panels.rightPanel != null)
            panels.rightPanel.SetActive(active);
    }

    // Helper to set text on both monitors
    private void SetDualText(DualTextElements elements, string text)
    {
        if (elements.leftMonitorText != null)
        {
            elements.leftMonitorText.text = text;
            // Enable rich text for formatting
            elements.leftMonitorText.richText = true;
        }
            
        if (elements.rightMonitorText != null)
        {
            elements.rightMonitorText.text = text;
            // Enable rich text for formatting
            elements.rightMonitorText.richText = true;
        }
    }
    
    private IEnumerator WaitForRestartInput()
    {
        // Short delay to prevent accidental input
        yield return new WaitForSeconds(1.0f);
        
        while (true)
        {
            // Handle right arrow (green button)
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                switch (currentState)
                {
                    case PanelState.GameOver:
                        // Show credits panel
                        ShowCreditsPanel();
                        // Add a small delay to prevent double-input
                        yield return new WaitForSeconds(0.5f);
                        break;
                        
                    case PanelState.Credits:
                        // Restart game
                        RestartGame();
                        break;
                }
            }
            
            // Handle left arrow (red button) - only in credits panel
            if (Input.GetKeyDown(KeyCode.LeftArrow) && currentState == PanelState.Credits)
            {
                // Go to start screen
                GoToStartScreen();
            }
            
            yield return null;
        }
    }
    
    private void RestartGame()
    {
		// Stop the music before loading the new scene
        soundManager.StopAll();
		// Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    private void GoToStartScreen()
    {
		// Stop the music before loading the new scene
		soundManager.StopAll();
        // Load the start scene
        SceneManager.LoadScene(startSceneName);
    }
    
    private void UpdateHighScores(int newScore)
    {
        // Get existing scores
        List<ScoreEntry> scores = GetAllScores();
        
        // Add the new score with timestamp to make it unique
        ScoreEntry newEntry = new ScoreEntry(newScore, System.DateTime.Now.Ticks);
        scores.Add(newEntry);
        
        // Sort scores in descending order (higher is better)
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Limit to maximum number of stored scores
        if (scores.Count > maxStoredScores)
        {
            scores = scores.GetRange(0, maxStoredScores);
        }
        
        // Save back to PlayerPrefs
        SaveScores(scores);
        
        Debug.Log($"Number of scores saved: {scores.Count}");
    }
    
    private List<ScoreEntry> GetAllScores()
    {
        // Get the number of saved scores
        int count = PlayerPrefs.GetInt("ScoreCount", 0);
        List<ScoreEntry> scores = new List<ScoreEntry>();
        
        // Load each score
        for (int i = 0; i < count; i++)
        {
            int score = PlayerPrefs.GetInt("Score_" + i, 0);
            long id;
            
            // Try to parse the ID, or use a default if parsing fails
            if (!long.TryParse(PlayerPrefs.GetString("ScoreID_" + i, "0"), out id))
            {
                id = i; // Default to index if parsing fails
            }
            
            scores.Add(new ScoreEntry(score, id));
        }
        
        Debug.Log($"Number of scores loaded: {scores.Count}");
        return scores;
    }
    
    private void SaveScores(List<ScoreEntry> scores)
    {
        // Save the number of scores
        PlayerPrefs.SetInt("ScoreCount", scores.Count);
        PlayerPrefs.Save();
        
        // Save each score
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetInt("Score_" + i, scores[i].score);
            PlayerPrefs.SetString("ScoreID_" + i, scores[i].id.ToString());
        }
        
        // Final save to ensure everything is written
        PlayerPrefs.Save();
    }
    
    private int DetermineRank(int currentScore, List<ScoreEntry> allScores)
    {
        // Sort scores in descending order
        allScores.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Find current player's score (the most recent one with this value)
        int rank = 1;
        bool found = false;
        
        for (int i = 0; i < allScores.Count; i++)
        {
            if (allScores[i].score > currentScore)
            {
                rank++;
            }
            else if (allScores[i].score == currentScore && !found)
            {
                found = true;
                break;
            }
        }
        
        return rank;
    }
    
    private string GenerateHighScoresText(List<ScoreEntry> scores, int playerRank)
    {
        // Sort scores in descending order
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Build text for top scores
        string scoresText = "Top No.Calls before broke today:\n";
        
        // Find the player's entry (most recent with current score)
        ScoreEntry playerEntry = null;
        int playerIndex = -1;
        
        for (int i = 0; i < scores.Count; i++)
        {
            // Find the player's most recent score
            if (scores[i].score == currentCallCount && playerEntry == null)
            {
                playerEntry = scores[i];
                playerIndex = i;
                break;
            }
        }
        
        // Verify if player is in top scores
        bool playerInTopScores = playerIndex < topScoresToShow;
        
        // Show top scores
        int numToShow = Mathf.Min(topScoresToShow, scores.Count);
        
        // First line with scores
        for (int i = 0; i < numToShow; i++)
        {
            if (i > 0)
                scoresText += "    ";
                
            // Highlight if this is the player's score
            if (i == playerIndex)
                scoresText += "<color=red>" + scores[i].score.ToString() + "</color>";
            else
                scoresText += scores[i].score.ToString();
        }
        
        // Always add ellipsis if we have more than topScoresToShow scores
        if (scores.Count > topScoresToShow)
        {
            scoresText += "  ...  ";
            
            // If player is not in top scores, also show their score after ellipsis
            if (!playerInTopScores && playerIndex >= 0)
            {
                scoresText += "<color=red>" + scores[playerIndex].score.ToString() + "</color>";
            }
        }
        
        // Second line with arrow under player's score
        scoresText += "\n";
        
        if (playerInTopScores && playerIndex >= 0)
        {
            // Player is in top scores - position arrow under their score
            int arrowPosition = 0;
            
            for (int i = 0; i < playerIndex; i++)
            {
                arrowPosition += scores[i].score.ToString().Length + 4;  // 4 spaces between scores
            }
            
            // Add half the player's score length to center the arrow
            arrowPosition += scores[playerIndex].score.ToString().Length / 2;
            
            // Add spaces before arrow
            for (int i = 0; i < arrowPosition; i++)
            {
                scoresText += " ";
            }
            
            // Add arrow
            scoresText += "↑";
        }
        else if (playerIndex >= 0)
        {
            // Player is after ellipsis - calculate position
            int arrowPosition = 0;
            
            // Calculate spaces for all top scores + ellipsis + half of player score
            for (int i = 0; i < numToShow; i++)
            {
                arrowPosition += scores[i].score.ToString().Length + 4;
            }
            
            // Add for "  ...  "
            arrowPosition += 7;
            
            // Position arrow under middle of player score
            arrowPosition += scores[playerIndex].score.ToString().Length / 2;
            
            // Add spaces before arrow
            for (int i = 0; i < arrowPosition; i++)
            {
                scoresText += " ";
            }
            
            // Add arrow
            scoresText += "↑";
        }
        
        return scoresText;
    }
}