using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FaceDatabase : MonoBehaviour
{
    // Static accessor
    private static FaceDatabase _instance;
    
    // track learned features
    private Dictionary<string, bool> runtimeLearnedFeatures = new Dictionary<string, bool>();
    private Dictionary<string, bool> runtimeLearnedSets = new Dictionary<string, bool>();
    private Dictionary<string, bool> runtimeLearnedGroups = new Dictionary<string, bool>();
    
    
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
    
    // Initialize the runtime cache on Start
    private void Start()
    {
        InitializeRuntimeCache();
    }
    
    private void InitializeRuntimeCache()
    {
        // Initialize from current state
        runtimeLearnedFeatures.Clear();
        runtimeLearnedSets.Clear();
        runtimeLearnedGroups.Clear();
        
        // Cache all feature learned states
        foreach (FacialFeature feature in allFeatures)
        {
            if (feature != null)
            {
                runtimeLearnedFeatures[feature.id] = feature.isLearned;
            }
        }
        
        // Cache all group and set learned states
        foreach (FeatureGroup group in allGroups)
        {
            if (group != null)
            {
                runtimeLearnedGroups[group.id] = group.isLearned;
                
                foreach (FaceSet set in group.sets)
                {
                    if (set != null)
                    {
                        // Generate a unique ID for the set (since it doesn't have one)
                        string setId = group.id + "_" + group.sets.IndexOf(set);
                        runtimeLearnedSets[setId] = set.isLearned;
                    }
                }
            }
        }
        
        Debug.Log($"Runtime cache initialized: {runtimeLearnedFeatures.Count} features, {runtimeLearnedSets.Count} sets, {runtimeLearnedGroups.Count} groups");
    }

    // New methods to update and check the runtime cache
    public void MarkFeatureLearnedRuntime(FacialFeature feature)
    {
        if (feature == null) return;
        
        // Update both the original and the cache
        feature.isLearned = true;
        runtimeLearnedFeatures[feature.id] = true;
        
        Debug.Log($"Runtime marked as learned: {feature.category}:{feature.partName}");
    }

    public void MarkSetLearnedRuntime(FeatureGroup group, FaceSet set)
    {
        if (group == null || set == null) return;
        
        // Update the set
        set.isLearned = true;
        
        // Generate the set ID
        string setId = group.id + "_" + group.sets.IndexOf(set);
        runtimeLearnedSets[setId] = true;
        
        // Mark all features in the set as learned
        foreach (FacialFeature feature in set.leftPart.features)
        {
            MarkFeatureLearnedRuntime(feature);
        }
        
        foreach (FacialFeature feature in set.rightPart.features)
        {
            MarkFeatureLearnedRuntime(feature);
        }
        
        Debug.Log("Runtime marked set as learned with all its features");
        
        // Check if the group is now fully learned
        CheckGroupLearnedStateRuntime(group);
    }

    public void CheckGroupLearnedStateRuntime(FeatureGroup group)
    {
        if (group == null) return;
        
        // Check if all sets in the group are learned
        bool allSetsLearned = true;
        
        foreach (FaceSet set in group.sets)
        {
            if (set != null)
            {
                string setId = group.id + "_" + group.sets.IndexOf(set);
                
                if (!runtimeLearnedSets.ContainsKey(setId) || !runtimeLearnedSets[setId])
                {
                    allSetsLearned = false;
                    break;
                }
            }
        }
        
        // Update the group's learned state
        group.isLearned = allSetsLearned;
        runtimeLearnedGroups[group.id] = allSetsLearned;
        
        if (allSetsLearned)
        {
            Debug.Log($"Runtime marked group '{group.groupName}' as fully learned");
        }
    }
    
    
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
    // Override the existing methods to use our runtime cache
    public List<FacialFeature> GetLearnedFeatures(string category)
    {
        return allFeatures.Where(f => 
            f.category == category && 
            (f.isLearned || (runtimeLearnedFeatures.ContainsKey(f.id) && runtimeLearnedFeatures[f.id]))
        ).ToList();
    }

    public List<FacialFeature> GetUnlearnedFeatures(string category)
    {
        return allFeatures.Where(f => 
            f.category == category && 
            !(f.isLearned || (runtimeLearnedFeatures.ContainsKey(f.id) && runtimeLearnedFeatures[f.id]))
        ).ToList();
    }

    public List<FeatureGroup> GetLearnedGroups()
    {
        return allGroups.Where(g => 
            g.isLearned || (runtimeLearnedGroups.ContainsKey(g.id) && runtimeLearnedGroups[g.id])
        ).ToList();
    }

    public List<FeatureGroup> GetUnlearnedGroups()
    {
        return allGroups.Where(g => 
            !(g.isLearned || (runtimeLearnedGroups.ContainsKey(g.id) && runtimeLearnedGroups[g.id]))
        ).ToList();
    }
    
    
    // Get a random learned feature from a category
    // Fix for FaceDatabase.cs - GetRandomLearnedFeature
    // In GetRandomLearnedFeature method in FaceDatabase.cs, adjust to report cache status
    public FacialFeature GetRandomLearnedFeature(string category)
    {
        List<FacialFeature> learnedFeatures = GetLearnedFeatures(category);

        // First, try to find a learned feature
        if (learnedFeatures.Count > 0)
        {
            int randomIndex = Random.Range(0, learnedFeatures.Count);
            FacialFeature selected = learnedFeatures[randomIndex];
            // Use cache-aware method to check learned status
            bool isCacheLearned = selected.isLearned;
            Debug.Log($"Selected learned feature: {category}:{selected.partName} (Object.isLearned={selected.isLearned}, Cache={isCacheLearned})");
            return selected;
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
    
    // Add this debug method to track learning status
    public void DebugLearningStatus(bool verbose = false)
    {
        Debug.Log("=== LEARNING STATUS DEBUG ===");
        
        // Count learned features by category
        foreach (string category in FeatureCategories)
        {
            List<FacialFeature> learnedFeatures = GetLearnedFeatures(category);
            int totalFeatures = GetFeaturesByCategory(category).Count;
            
            Debug.Log($"{category}: {learnedFeatures.Count}/{totalFeatures} learned");
            
            // If verbose, list individual learned features
            if (verbose && learnedFeatures.Count > 0)
            {
                foreach (FacialFeature feature in learnedFeatures)
                {
                    // Show both the actual isLearned property and the cached status
                    bool isCacheLearned = feature.isLearned; // Adjust to your actual method name
                    Debug.Log($"  - {feature.partName} (Object.isLearned={feature.isLearned}, Cache={cachedStatus})");
                }
            }
        }
        
        // Count learned groups and sets
        int learnedGroups = GetLearnedGroups().Count;
        int totalGroups = GetLearnedGroups().Count + GetUnlearnedGroups().Count;
        
        Debug.Log($"Groups: {learnedGroups}/{totalGroups} learned");
        
        // If verbose, show details for each group
        if (verbose)
        {
            foreach (FeatureGroup group in GetLearnedGroups())
            {
                Debug.Log($"  Learned Group: {group.groupName}");
            }
            
            foreach (FeatureGroup group in GetUnlearnedGroups())
            {
                // Count learned/unlearned sets in this group
                int learnedSets = 0;
                foreach (FaceSet set in group.sets)
                {
                    if (set.isLearned) // Adjust to your actual method name
                    {
                        learnedSets++;
                    }
                }
                
                Debug.Log($"  Unlearned Group: {group.groupName} ({learnedSets}/{group.sets.Count} sets learned)");
            }
        }
        
        Debug.Log("=== END OF LEARNING STATUS DEBUG ===");
    }
    
}