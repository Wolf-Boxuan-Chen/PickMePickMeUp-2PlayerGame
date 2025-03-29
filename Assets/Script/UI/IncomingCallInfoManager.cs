using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class IncomingCallInfoManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text leftCurrentTimeText;
    [SerializeField] private TMP_Text rightCurrentTimeText;
    [SerializeField] private TMP_Text leftCallerNameText;
    [SerializeField] private TMP_Text rightCallerNameText;
    
    [Header("Settings")]
    [SerializeField] private bool use24HourFormat = false;
    [SerializeField] private bool showSeconds = true;
    
    // List of short names (male, female, and nonbinary)
    private List<string> shortNames = new List<string>
    {
        // Male names
        "Tom", "Sam", "Ben", "Max", "Joe", "Ian", "Leo", "Jay", "Kai", "Dan",
        // Female names
        "Amy", "Kim", "Eve", "Zoe", "Joy", "Ann", "Ava", "Mae", "Sue", "Fay",
        // Nonbinary/Neutral names
        "Alex", "Sam", "Ray", "Ash", "Sky", "Pat", "Kai", "Tay", "Jess", "Ari"
    };
    
    private string currentCallerName;
    
    private void Start()
    {
        // Generate a random caller name at start
        GenerateRandomCallerName();
    }
    
    private void Update()
    {
        // Update the time text every frame
        UpdateTimeText();
    }
    
    // Update both time text fields with current time
    private void UpdateTimeText()
    {
        // Get current time
        DateTime now = DateTime.Now;
        
        // Format time string
        string timeFormat = use24HourFormat ? "HH:mm" : "h:mm tt";
        
        // Add seconds if needed
        if (showSeconds)
        {
            timeFormat = use24HourFormat ? "HH:mm:ss" : "h:mm:ss tt";
        }
        
        string timeString = now.ToString(timeFormat);
        
        // Update text objects
        if (leftCurrentTimeText != null)
            leftCurrentTimeText.text = timeString;
            
        if (rightCurrentTimeText != null)
            rightCurrentTimeText.text = timeString;
    }
    
    // Generate a random caller name
    public void GenerateRandomCallerName()
    {
        // Pick a random name from the list
        int randomIndex = UnityEngine.Random.Range(0, shortNames.Count);
        currentCallerName = shortNames[randomIndex];
        
        // Update text objects
        UpdateCallerNameText();
    }
    
    // Update caller name on both screens
    private void UpdateCallerNameText()
    {
        if (leftCallerNameText != null)
            leftCallerNameText.text = currentCallerName;
            
        if (rightCallerNameText != null)
            rightCallerNameText.text = currentCallerName;
    }
    
    // Public method to get current caller name
    public string GetCurrentCallerName()
    {
        return currentCallerName;
    }
}