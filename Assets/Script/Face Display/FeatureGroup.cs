using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FeatureGroup
{
    public string id;
    public string groupName;
    public List<FaceSet> sets = new List<FaceSet>();
    public bool isLearned = false;
    public float selectionChance = 1.0f;
    public bool flipSides = false;
    
    // Add a set to this group
    public void AddSet(FaceSet set)
    {
        if (set != null && !sets.Contains(set))
        {
            sets.Add(set);
        }
    }
    
    // Check if all sets in group are learned
    public bool AreAllSetsLearned()
    {
        if (sets.Count == 0) return false;
        
        foreach (var set in sets)
        {
            if (!set.isLearned)
                return false;
        }
        
        return true;
    }
    
    // Get next unlearned set
    // In FeatureGroup.cs
    public FaceSet GetNextUnlearnedSet()
    {
        // Log details for debugging
        int unlearnedSetsCount = 0;
        foreach (var set in sets)
        {
            if (!set.isLearned)
                unlearnedSetsCount++;
        }
        Debug.Log($"Looking for next unlearned set in group {groupName} ({unlearnedSetsCount} unlearned sets)");
    
        foreach (var set in sets)
        {
            if (!set.isLearned)
            {
                Debug.Log($"Found unlearned set in group {groupName}");
                return set;
            }
        }
    
        Debug.LogWarning($"No unlearned sets found in group {groupName}, but group is not marked as learned");
        return null; // All sets learned
    }
}