using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FaceDatabase : MonoBehaviour
{
    // Static accessor
    private static FaceDatabase _instance;
    
    // Add weights to the FaceDatabase for each category
    public Dictionary<string, float> CategoryWeights = new Dictionary<string, float>()
    {
        {"FaceShape", 10f},
        {"Eye", 1.5f},
        {"Nose", 1.2f},
        {"Mouth", 1.5f},
        {"FrontHair", 0.5f},
        {"BackHair", 0.5f},
        {"Ear", 0.4f},
        {"Shoulder", 0.6f},
        {"Background", 1.0f},
        {"PhoneCase", 1.0f}
    };
    // Update feature selection method to use weights
    
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
    // Fix for FaceDatabase.cs - GetRandomLearnedFeature
    public FacialFeature GetRandomLearnedFeature(string category)
    {
        List<FacialFeature> learnedFeatures = GetLearnedFeatures(category);

        // First, try to find a learned feature
        if (learnedFeatures.Count > 0)
        {
            int randomIndex = Random.Range(0, learnedFeatures.Count);
            return learnedFeatures[randomIndex];
        }

        // Log a warning if no learned features are available
        Debug.LogWarning($"No learned {category} features available! Skipping this feature category.");
    
        // Return null instead of falling back to unlearned features
        return null;
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
    // Add to FaceDatabase.cs
    public void MarkFeatureAsLearned(FacialFeature feature)
    {
        if (feature != null && !feature.isLearned)
        {
            feature.isLearned = true;
            Debug.Log($"Marked feature as learned: {feature.category}:{feature.partName}");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this.gameObject);
#endif
        }
    }

    public void MarkSetAsLearned(FaceSet set)
    {
        if (set != null && !set.isLearned)
        {
            set.isLearned = true;
        
            // Mark all features in the set as learned
            foreach (FacialFeature feature in set.leftPart.features)
            {
                MarkFeatureAsLearned(feature);
            }
        
            foreach (FacialFeature feature in set.rightPart.features)
            {
                MarkFeatureAsLearned(feature);
            }
        
            Debug.Log("Marked set as learned with all its features");
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this.gameObject);
#endif
        }
    }

    public void CheckAndUpdateGroupLearningStatus(FeatureGroup group)
    {
        if (group != null)
        {
            bool wasLearned = group.isLearned;
            group.isLearned = group.AreAllSetsLearned();
        
            if (!wasLearned && group.isLearned)
            {
                Debug.Log($"Group '{group.groupName}' is now fully learned");
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this.gameObject);
#endif
            }
        }
    }
    // Add to FaceDatabase.cs
    
    // Replace or remove the SaveLearningState method in FaceDatabase.cs
    public void SaveLearningState()
    {
#if UNITY_EDITOR
    // Only set dirty the database gameObject itself
    UnityEditor.EditorUtility.SetDirty(this.gameObject);
    
    // DON'T try to set dirty individual features, groups, or sets
    // as they don't inherit from UnityEngine.Object
    
    // Force Unity to save all assets
    UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}