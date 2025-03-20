using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private const string SCORE_COUNT_KEY = "ScoreCount";
    private const string SCORE_KEY_PREFIX = "Score_";
    
    // Save a new score
    public static void SaveScore(int score)
    {
        List<int> scores = GetAllScores();
        
        // Add new score if not already present
        if (!scores.Contains(score))
        {
            scores.Add(score);
        }
        
        // Sort scores (higher is better)
        scores.Sort((a, b) => b.CompareTo(a));
        
        // Save back to PlayerPrefs
        PlayerPrefs.SetInt(SCORE_COUNT_KEY, scores.Count);
        
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetInt(SCORE_KEY_PREFIX + i, scores[i]);
        }
        
        PlayerPrefs.Save();
    }
    
    // Get all saved scores
    public static List<int> GetAllScores()
    {
        int count = PlayerPrefs.GetInt(SCORE_COUNT_KEY, 0);
        List<int> scores = new List<int>();
        
        for (int i = 0; i < count; i++)
        {
            scores.Add(PlayerPrefs.GetInt(SCORE_KEY_PREFIX + i, 0));
        }
        
        // Ensure scores are sorted (higher is better)
        scores.Sort((a, b) => b.CompareTo(a));
        
        return scores;
    }
    
    // Get the rank of a score
    public static int GetRank(int score)
    {
        List<int> scores = GetAllScores();
        
        // Find position (add 1 because ranks start at 1)
        return scores.IndexOf(score) + 1;
    }
}