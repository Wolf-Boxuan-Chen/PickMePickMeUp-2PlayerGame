using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class FeatureEditorUI : EditorWindow
{
    // FaceDatabase reference
    private FaceDatabase faceDatabase;
    
    // UI state
    private int selectedTab = 0;
    private string[] tabNames = { "Features", "Groups", "Sets" };
    private Vector2 scrollPosition;
    
    // Feature editing
    private string newFeatureName = "";
    private string selectedCategory = "FaceShape";
    private Sprite newFeatureSprite;
    private bool newFeatureIsLearned = false;
    
    // Group editing
    private FeatureGroup selectedGroup;
    private string newGroupName = "";
    
    // Set editing
    private FaceSet selectedSet;
    
    // Feature selection for sets
    private Dictionary<string, List<FacialFeature>> categoryFeatures = new Dictionary<string, List<FacialFeature>>();
    private Dictionary<string, FacialFeature> selectedLeftFeatures = new Dictionary<string, FacialFeature>();
    private Dictionary<string, FacialFeature> selectedRightFeatures = new Dictionary<string, FacialFeature>();
    
    [MenuItem("Game/Feature Editor")]
    public static void ShowWindow()
    {
        GetWindow<FeatureEditorUI>("Face Feature Editor");
    }
    
    private void OnEnable()
    {
        // Find FaceDatabase in scene
        faceDatabase = FindFirstObjectByType<FaceDatabase>();
    }
    
    private void OnGUI()
    {
        // Check if FaceDatabase exists
        if (faceDatabase == null)
        {
            EditorGUILayout.HelpBox("FaceDatabase not found in scene. Please open a scene with a FaceDatabase component.", MessageType.Error);
            
            if (GUILayout.Button("Find FaceDatabase"))
            {
                faceDatabase = FindFirstObjectByType<FaceDatabase>();
            }
            
            return;
        }
        
        // Tabs
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (selectedTab)
        {
            case 0: DrawFeaturesTab(); break;
            case 1: DrawGroupsTab(); break;
            case 2: DrawSetsTab(); break;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    // Update the DrawFeaturesTab method in FeatureEditorUI.cs to fix the delete functionality
    // and improve the UI layout

    private void DrawFeaturesTab()
    {
        EditorGUILayout.LabelField("Add New Feature", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        selectedCategory = EditorGUILayout.TextField("Category:", selectedCategory);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        newFeatureName = EditorGUILayout.TextField("Name:", newFeatureName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        newFeatureSprite = (Sprite)EditorGUILayout.ObjectField("Sprite:", newFeatureSprite, typeof(Sprite), false);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        newFeatureIsLearned = EditorGUILayout.Toggle("Is Learned:", newFeatureIsLearned);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Add Feature"))
        {
            if (string.IsNullOrEmpty(newFeatureName))
            {
                EditorUtility.DisplayDialog("Error", "Feature name cannot be empty", "OK");
            }
            else if (newFeatureSprite == null)
            {
                EditorUtility.DisplayDialog("Error", "Feature sprite cannot be null", "OK");
            }
            else
            {
                // Add feature to database
                faceDatabase.AddFeature(selectedCategory, newFeatureName, newFeatureSprite, newFeatureIsLearned);
                
                // Clear fields
                newFeatureName = "";
                newFeatureSprite = null;
                
                EditorUtility.SetDirty(faceDatabase);
            }
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.LabelField("Existing Features", EditorStyles.boldLabel);
        
        // Get all feature categories
        string[] categories = faceDatabase.FeatureCategories;
        
        // Display features by category
        foreach (string category in categories)
        {
            EditorGUILayout.LabelField(category, EditorStyles.boldLabel);
            
            List<FacialFeature> features = faceDatabase.GetFeaturesByCategory(category);
            List<FacialFeature> featuresToRemove = new List<FacialFeature>(); // Track features to remove
            
            if (features.Count == 0)
            {
                EditorGUILayout.LabelField("No features in this category");
            }
            else
            {
                // Update the feature list UI in DrawFeaturesTab
                foreach (FacialFeature feature in features)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Name with fixed width
                    EditorGUILayout.LabelField(feature.partName, GUILayout.Width(120));
                    
                    // Display sprite preview
                    if (feature.sprite != null && feature.sprite.texture != null)
                    {
                        Rect spriteRect = GUILayoutUtility.GetRect(60, 60, GUILayout.ExpandWidth(false));
                        GUI.DrawTexture(spriteRect, feature.sprite.texture);
                    }
                    else
                    {
                        Rect spriteRect = GUILayoutUtility.GetRect(60, 60, GUILayout.ExpandWidth(false));
                        EditorGUI.DrawRect(spriteRect, Color.gray); // Draw placeholder
                    }
                    
                    // Learned label and toggle RIGHT NEXT TO EACH OTHER
                    EditorGUILayout.LabelField("Learned", GUILayout.Width(50));
                    bool isLearned = EditorGUILayout.Toggle("", feature.isLearned, GUILayout.Width(20));
                    if (isLearned != feature.isLearned)
                    {
                        feature.isLearned = isLearned;
                        EditorUtility.SetDirty(faceDatabase);
                    }
                    
                    // Add flexible space to push delete button to the far right
                    GUILayout.FlexibleSpace();
                    
                    // Delete button (at the far right)
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete feature '{feature.partName}'?", "Yes", "No"))
                        {
                            // Add to removal list (can't remove while iterating)
                            featuresToRemove.Add(feature);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                // Remove features that were marked for deletion
                if (featuresToRemove.Count > 0)
                {
                    // Implement the deletion logic
                    foreach (FacialFeature featureToRemove in featuresToRemove)
                    {
                        // Remove from database
                        RemoveFeatureFromDatabase(featureToRemove);
                    }
                    
                    EditorUtility.SetDirty(faceDatabase);
                }
            }
            
            EditorGUILayout.Space(10);
        }
    }

    // Add this method to FeatureEditorUI.cs to handle feature deletion
    private void RemoveFeatureFromDatabase(FacialFeature feature)
    {
        if (faceDatabase == null || feature == null) return;
        
        // Get all features list via reflection (since it's private)
        var featuresField = typeof(FaceDatabase).GetField("allFeatures", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (featuresField != null)
        {
            var allFeatures = featuresField.GetValue(faceDatabase) as List<FacialFeature>;
            if (allFeatures != null)
            {
                // Remove the feature
                allFeatures.Remove(feature);
                
                // Also need to remove it from any sets that might use it
                RemoveFeatureFromAllSets(feature);
                
                Debug.Log($"Removed feature: {feature.partName} ({feature.category})");
            }
        }
    }

    // Add this method to FeatureEditorUI.cs to remove the feature from all sets
    private void RemoveFeatureFromAllSets(FacialFeature feature)
    {
        if (faceDatabase == null || feature == null) return;
        
        // Get all groups list via reflection
        var groupsField = typeof(FaceDatabase).GetField("allGroups", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (groupsField != null)
        {
            var allGroups = groupsField.GetValue(faceDatabase) as List<FeatureGroup>;
            if (allGroups != null)
            {
                foreach (var group in allGroups)
                {
                    foreach (var set in group.sets)
                    {
                        // Remove from left part
                        if (set.leftPart != null && set.leftPart.features != null)
                        {
                            set.leftPart.features.RemoveAll(f => f != null && f.id == feature.id);
                        }
                        
                        // Remove from right part
                        if (set.rightPart != null && set.rightPart.features != null)
                        {
                            set.rightPart.features.RemoveAll(f => f != null && f.id == feature.id);
                        }
                    }
                }
            }
        }
    }
    
    private void DrawGroupsTab()
    {
        EditorGUILayout.LabelField("Create New Group", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        newGroupName = EditorGUILayout.TextField("Group Name:", newGroupName);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Create Group"))
        {
            if (string.IsNullOrEmpty(newGroupName))
            {
                EditorUtility.DisplayDialog("Error", "Group name cannot be empty", "OK");
            }
            else
            {
                // Create new group
                faceDatabase.CreateGroup(newGroupName);
                
                // Clear field
                newGroupName = "";
                
                EditorUtility.SetDirty(faceDatabase);
            }
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.LabelField("Existing Groups", EditorStyles.boldLabel);
        
        // Display all groups
        var allGroups = faceDatabase.GetLearnedGroups();
        allGroups.AddRange(faceDatabase.GetUnlearnedGroups());
        
        // List to track groups to remove
        List<FeatureGroup> groupsToRemove = new List<FeatureGroup>();
        
        if (allGroups.Count == 0)
        {
            EditorGUILayout.LabelField("No groups defined yet");
        }
        else
        {
            for (int i = 0; i < allGroups.Count; i++)
            {
                FeatureGroup group = allGroups[i];
                
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(group.groupName, GUILayout.Width(150));
                
                EditorGUILayout.LabelField("Learned", GUILayout.Width(50));
                bool isLearned = EditorGUILayout.Toggle("", group.isLearned, GUILayout.Width(20));
                if (isLearned != group.isLearned)
                {
                    group.isLearned = isLearned;
                    EditorUtility.SetDirty(faceDatabase);
                }
                
                float selectionChance = EditorGUILayout.Slider("Selection Chance", group.selectionChance, 0.5f, 1.0f, GUILayout.Width(300));
                if (selectionChance != group.selectionChance)
                {
                    group.selectionChance = selectionChance;
                    EditorUtility.SetDirty(faceDatabase);
                }
                
                // Up button (move group up in the list)
                GUI.enabled = i > 0;
                if (GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    // Swap with previous group
                    if (i > 0)
                    {
                        SwapGroups(allGroups, i, i - 1);
                        EditorUtility.SetDirty(faceDatabase);
                    }
                }
                GUI.enabled = i < allGroups.Count - 1;
                
                // Down button (move group down in the list)
                if (GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    // Swap with next group
                    if (i < allGroups.Count - 1)
                    {
                        SwapGroups(allGroups, i, i + 1);
                        EditorUtility.SetDirty(faceDatabase);
                    }
                }
                GUI.enabled = true;
                
                if (GUILayout.Button("Edit Sets", GUILayout.Width(80)))
                {
                    selectedGroup = group;
                    selectedSet = null;
                }
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete group '{group.groupName}'?", "Yes", "No"))
                    {
                        groupsToRemove.Add(group);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Display sets if this group is selected
                // Update the part in DrawGroupsTab where we handle set display and deletion
                if (selectedGroup == group)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.LabelField("Sets in this Group:", EditorStyles.boldLabel);
                    
                    // Track sets to remove rather than removing immediately
                    List<FaceSet> setsToRemove = new List<FaceSet>();
                    
                    for (int j = 0; j < group.sets.Count; j++)
                    {
                        FaceSet set = group.sets[j];
                        
                        EditorGUILayout.BeginHorizontal();
                        
                        EditorGUILayout.LabelField($"Set {j+1}", GUILayout.Width(50));
                        
                        EditorGUILayout.LabelField("Learned", GUILayout.Width(50));
                        bool setLearned = EditorGUILayout.Toggle("", set.isLearned, GUILayout.Width(20));
                        if (setLearned != set.isLearned)
                        {
                            set.isLearned = setLearned;
                            EditorUtility.SetDirty(faceDatabase);
                        }
                        
                        EditorGUILayout.LabelField($"Left: {set.leftPart.features.Count} features, Right: {set.rightPart.features.Count} features", GUILayout.Width(250));
                        
                        if (GUILayout.Button("Edit", GUILayout.Width(60)))
                        {
                            selectedSet = set;
                            // Clear feature selections
                            selectedLeftFeatures.Clear();
                            selectedRightFeatures.Clear();
                        }
                        
                        if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        {
                            if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete set {j+1}?", "Yes", "No"))
                            {
                                // Mark for removal instead of removing immediately
                                setsToRemove.Add(set);
                            }
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    // Process set removals after loop is complete
                    if (setsToRemove.Count > 0)
                    {
                        foreach (FaceSet setToRemove in setsToRemove)
                        {
                            group.sets.Remove(setToRemove);
                            
                            // If the deleted set was selected, deselect it
                            if (selectedSet == setToRemove)
                            {
                                selectedSet = null;
                            }
                        }
                        
                        EditorUtility.SetDirty(faceDatabase);
                    }
                    
                    if (GUILayout.Button("Add New Set"))
                    {
                        FaceSet newSet = new FaceSet();
                        newSet.leftPart = new SetPart();
                        newSet.rightPart = new SetPart();
                        group.sets.Add(newSet);
                        
                        // Select the new set for editing
                        selectedSet = newSet;
                        selectedLeftFeatures.Clear();
                        selectedRightFeatures.Clear();
                        
                        EditorUtility.SetDirty(faceDatabase);
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            // Process group deletions
            if (groupsToRemove.Count > 0)
            {
                foreach (FeatureGroup groupToRemove in groupsToRemove)
                {
                    RemoveGroupFromDatabase(groupToRemove);
                }
                
                // If the deleted group was selected, deselect it
                if (groupsToRemove.Contains(selectedGroup))
                {
                    selectedGroup = null;
                    selectedSet = null;
                }
                
                EditorUtility.SetDirty(faceDatabase);
            }
        }
    }

    // Add this method to enable group reordering
    private void SwapGroups(List<FeatureGroup> groups, int index1, int index2)
    {
        if (index1 < 0 || index1 >= groups.Count || index2 < 0 || index2 >= groups.Count)
            return;
            
        FeatureGroup temp = groups[index1];
        groups[index1] = groups[index2];
        groups[index2] = temp;
    }

    // Add this method to implement group deletion
    private void RemoveGroupFromDatabase(FeatureGroup group)
    {
        if (faceDatabase == null || group == null) return;
        
        // Get all groups list via reflection
        var groupsField = typeof(FaceDatabase).GetField("allGroups", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (groupsField != null)
        {
            var allGroups = groupsField.GetValue(faceDatabase) as List<FeatureGroup>;
            if (allGroups != null)
            {
                // Remove the group
                allGroups.Remove(group);
                
                Debug.Log($"Removed group: {group.groupName}");
            }
        }
    }
    
    private void DrawSetsTab()
    {
        if (selectedSet == null)
        {
            EditorGUILayout.LabelField("Select a set to edit from the Groups tab");
            return;
        }
        
        EditorGUILayout.LabelField("Edit Set", EditorStyles.boldLabel);
        
        // Display set information
        FeatureGroup parentGroup = faceDatabase.FindGroupContainingSet(selectedSet);
        
        if (parentGroup != null)
        {
            EditorGUILayout.LabelField($"Group: {parentGroup.groupName}");
        }
        
        bool isLearned = EditorGUILayout.Toggle("Set Is Learned", selectedSet.isLearned);
        if (isLearned != selectedSet.isLearned)
        {
            selectedSet.isLearned = isLearned;
            EditorUtility.SetDirty(faceDatabase);
        }
        
        EditorGUILayout.Space(20);
        
        // Left and right parts in columns
        EditorGUILayout.BeginHorizontal();
        
        // Left part
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        
        EditorGUILayout.LabelField("Left Face Features", EditorStyles.boldLabel);
        
        DrawSetPartEditor(selectedSet.leftPart, selectedLeftFeatures);
        
        EditorGUILayout.EndVertical();
        
        // Right part
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        
        EditorGUILayout.LabelField("Right Face Features", EditorStyles.boldLabel);
        
        DrawSetPartEditor(selectedSet.rightPart, selectedRightFeatures);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSetPartEditor(SetPart part, Dictionary<string, FacialFeature> selectedFeatures)
    {
        // Current features in this part
        EditorGUILayout.LabelField("Current Features:");
        
        if (part.features.Count == 0)
        {
            EditorGUILayout.LabelField("No features added yet");
        }
        else
        {
            foreach (FacialFeature feature in part.features)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{feature.category}: {feature.partName}");
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    part.features.Remove(feature);
                    EditorUtility.SetDirty(faceDatabase);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Add new features
        EditorGUILayout.LabelField("Add Features:");
        
        // Feature categories
        string[] categories = faceDatabase.FeatureCategories;
        
        foreach (string category in categories)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(category + ":", GUILayout.Width(100));
            
            // Get all features in this category
            List<FacialFeature> categoryFeaturesList = faceDatabase.GetFeaturesByCategory(category);
            
            if (categoryFeaturesList.Count == 0)
            {
                EditorGUILayout.LabelField("No features available");
            }
            else
            {
                // Create feature names array for dropdown
                string[] featureNames = new string[categoryFeaturesList.Count + 1];
                featureNames[0] = "None";
                
                for (int i = 0; i < categoryFeaturesList.Count; i++)
                {
                    featureNames[i + 1] = categoryFeaturesList[i].partName;
                }
                
                // Get current selection index
                int currentIndex = 0;
                
                if (selectedFeatures.ContainsKey(category))
                {
                    FacialFeature selectedFeature = selectedFeatures[category];
                    
                    for (int i = 0; i < categoryFeaturesList.Count; i++)
                    {
                        if (categoryFeaturesList[i].id == selectedFeature.id)
                        {
                            currentIndex = i + 1;
                            break;
                        }
                    }
                }
                
                // Dropdown for selection
                int selectedIndex = EditorGUILayout.Popup(currentIndex, featureNames);
                
                if (selectedIndex != currentIndex)
                {
                    if (selectedIndex == 0)
                    {
                        // Remove selection
                        if (selectedFeatures.ContainsKey(category))
                        {
                            selectedFeatures.Remove(category);
                        }
                    }
                    else
                    {
                        // Set new selection
                        selectedFeatures[category] = categoryFeaturesList[selectedIndex - 1];
                    }
                }
                
                // Add button
                if (selectedIndex > 0 && GUILayout.Button("Add", GUILayout.Width(60)))
                {
                    FacialFeature featureToAdd = categoryFeaturesList[selectedIndex - 1];
                    
                    // Check if already added
                    bool alreadyAdded = false;
                    foreach (FacialFeature existingFeature in part.features)
                    {
                        if (existingFeature.id == featureToAdd.id)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }
                    
                    if (!alreadyAdded)
                    {
                        part.features.Add(featureToAdd);
                        EditorUtility.SetDirty(faceDatabase);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Feature Already Added", "This feature is already in the set", "OK");
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif