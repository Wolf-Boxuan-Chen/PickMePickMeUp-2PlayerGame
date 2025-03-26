using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FaceDatabase : MonoBehaviour
{
    // Static accessor
    private static FaceDatabase _instance;
    public static FaceDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<FaceDatabase>(); // Changed from FindObjectOfType
                if (_instance == null)
                {
                    Debug.LogError("No FaceDatabase found in scene!");
                }
            }
            return _instance;
        }
    }
    
    // Feature categories
    public readonly string[] FeatureCategories = {
        "FaceShape", "Eye", "Nose", "Mouth", "FrontHair", 
        "BackHair", "Ear", "Shoulder", "Background", "PhoneCase"
    };
    
    // Features and groups collections
    [SerializeField] private List<FacialFeature> allFeatures = new List<FacialFeature>();
    [SerializeField] private List<FeatureGroup> allGroups = new List<FeatureGroup>();
    
    // Add a feature to the database
    public FacialFeature AddFeature(string category, string partName, Sprite sprite, bool isLearned = false)
    {
        string id = System.Guid.NewGuid().ToString();
        FacialFeature feature = new FacialFeature(id, category, partName, sprite, isLearned);
        allFeatures.Add(feature);
        return feature;
    }
    
    // Create a new group
    public FeatureGroup CreateGroup(string groupName)
    {
        FeatureGroup group = new FeatureGroup();
        group.id = System.Guid.NewGuid().ToString();
        group.groupName = groupName;
        allGroups.Add(group);
        return group;
    }
    
    // Get features by category
    public List<FacialFeature> GetFeaturesByCategory(string category)
    {
        return allFeatures.Where(f => f.category == category).ToList();
    }
    
    // Get learned features by category
    public List<FacialFeature> GetLearnedFeatures(string category)
    {
        return allFeatures.Where(f => f.category == category && f.isLearned).ToList();
    }
    
    // Get unlearned features by category
    public List<FacialFeature> GetUnlearnedFeatures(string category)
    {
        return allFeatures.Where(f => f.category == category && !f.isLearned).ToList();
    }
    
    // Get all learned groups
    public List<FeatureGroup> GetLearnedGroups()
    {
        return allGroups.Where(g => g.isLearned).ToList();
    }
    
    // Get all unlearned groups
    public List<FeatureGroup> GetUnlearnedGroups()
    {
        return allGroups.Where(g => !g.isLearned).ToList();
    }
    
    // Get a random learned feature from a category
    public FacialFeature GetRandomLearnedFeature(string category)
    {
        var features = GetLearnedFeatures(category);
        
        if (features.Count == 0)
        {
            // Fallback to any feature if none learned
            features = GetFeaturesByCategory(category);
            
            if (features.Count == 0)
            {
                Debug.LogWarning($"No features available for category: {category}");
                return null;
            }
        }
        
        int randomIndex = Random.Range(0, features.Count);
        return features[randomIndex];
    }
    
    // Check if a group has all sets learned and update its learned status
    public void CheckGroupCompletion(FeatureGroup group)
    {
        if (group == null) return;
        
        bool allLearned = group.AreAllSetsLearned();
        
        if (allLearned && !group.isLearned)
        {
            group.isLearned = true;
            Debug.Log($"Group '{group.groupName}' is now fully learned");
        }
    }
    
    // Helper to find the group containing a set
    public FeatureGroup FindGroupContainingSet(FaceSet set)
    {
        if (set == null) return null;
        
        foreach (var group in allGroups)
        {
            if (group.sets.Contains(set))
            {
                return group;
            }
        }
        
        return null;
    }
    
    // Get all learned sets from all groups
    public List<FaceSet> GetAllLearnedSets()
    {
        List<FaceSet> learnedSets = new List<FaceSet>();
        
        foreach (var group in allGroups)
        {
            foreach (var set in group.sets)
            {
                if (set.isLearned)
                {
                    learnedSets.Add(set);
                }
            }
        }
        
        return learnedSets;
    }
}