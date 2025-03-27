using System.Collections.Generic;
using UnityEngine;

public class FaceGenerator : MonoBehaviour
{
    [SerializeField] private FaceManager faceManager;
    
    
    // Tracking current round and complexity
    private int currentRound = 0;
    private int maxFeaturesPerFace = 3;
    
    // Current active set (for learning tracking)
    private FaceSet currentActiveSet;
    private FeatureGroup currentActiveGroup;
    
    // Track whether current faces are identical
    private bool currentFacesIdentical;
    
    // Track Differences in Face 
    private List<string> currentDifferentCategories = new List<string>();
    
    // Initialize the generator
    public void Initialize()
    {
        currentRound = 0;
        currentActiveSet = null;
        currentActiveGroup = null;
    }
    
    // Generate faces for the next round
    public void GenerateNextRoundFaces()
    {
        // Increment round counter
        currentRound++;
        Debug.Log($"Generating faces for round {currentRound}");
        
        string generationMethod = "";
        
        // Adjust feature complexity based on round
        if (currentRound <= 3)
        {
            maxFeaturesPerFace = 4;  // First 3 rounds: 4 features + bg + phone
        }
        else if (currentRound <= 8)
        {
            maxFeaturesPerFace = 5;  // Next 5 rounds: 5 features + bg + phone
        }
        else
        {
            maxFeaturesPerFace = 6;  // After that: 6 features + bg + phone
        }
        
        Debug.Log($"Using max {maxFeaturesPerFace} features per face");
        
        // Decide generation method
        float rng = Random.value;
        
        // Reset current active set
        currentActiveSet = null;
        currentActiveGroup = null;
        
        if (rng < 0.3f)
        {
            // 30% chance: Generate identical faces
            generationMethod = "Identical Learned Faces";
            Debug.Log($"Generating {generationMethod}");
            GenerateIdenticalLearnedFaces();
        }
        else if (rng < 0.6f)
        {
            // 30% chance: Generate faces with differences
            generationMethod = "Different Learned Faces";
            Debug.Log($"Generating {generationMethod}");
            GenerateDifferentLearnedFaces();
        }
        else
        {
            // 40% chance: Use unlearned group/set
            generationMethod = "Unlearned Group/Set Faces";
            Debug.Log($"Generating {generationMethod}");
            GenerateUnlearnedFaces();
        }
        
        // After generation, validate the result
        Debug.Log($"Face generation complete using {generationMethod}");
        Debug.Log($"Are faces identical according to FaceManager: {faceManager.IsFriendCall()}");
    }
    
    // Generate identical faces from learned features
    private void GenerateIdenticalLearnedFaces()
    {
        Face leftFace = new Face();
        Face rightFace;
        
        
        // Fill with random learned features
        FillWithLearnedFeatures(leftFace, maxFeaturesPerFace);
        
        //Enfore to have only one type of hair
        EnforceHairConsistency(leftFace);
        
        // Clone the left face to the right
        rightFace = leftFace.Clone();
        
        // Set faces in the face manager
        faceManager.SetLeftFace(leftFace);
        faceManager.SetRightFace(rightFace);
        faceManager.SetFacesIdentical(true);
        DebugLogFaceGeneration("Different Learned Faces", leftFace, rightFace);
        
        // Update tracking
        currentFacesIdentical = true;
        //Track differences between faces so we can mention it when players can't figure it out
        TrackDifferences(leftFace, rightFace);
    }
    
    // Generate faces with 1-2 differences from learned features
    // In GenerateDifferentLearnedFaces method:
    [SerializeField] private float nonLearningFeatureChangeChance = 0.3f; // 30% chance to change

    // Update the GenerateDifferentLearnedFaces method
    private void GenerateDifferentLearnedFaces()
    {
        Face leftFace = new Face();
        
        // Fill with random learned features
        FillWithLearnedFeatures(leftFace, maxFeaturesPerFace);
        
        // Apply hair consistency to base face
        EnforceHairConsistency(leftFace);
        
        // Clone the left face to the right
        Face rightFace = leftFace.Clone();
        
        // Decide how many differences (1 or 2)
        int numDifferences = (Random.value < 0.7f) ? 1 : 2;
        Debug.Log($"Creating {numDifferences} difference(s) between faces");
        
        // Track if we're creating a hair-specific difference
        bool hairDifferenceCreated = false;
        
        // Get all available feature categories on the face
        List<string> availableCategories = new List<string>();
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            // Only include categories that have features
            if (leftFace.GetFeature(category) != null)
            {
                availableCategories.Add(category);
            }
        }
        
        // Shuffle to randomize which features will be changed
        ShuffleList(availableCategories);
        
        // Used to limit the number of changes we make
        int changesCreated = 0;
        
        // Attempt to create differences
        foreach (string category in availableCategories)
        {
            // Stop if we've made enough differences
            if (changesCreated >= numDifferences)
                break;
                
            // Skip if this is a category we should preserve
            // (Background and PhoneCase often preserved)
            if ((category == "Background" || category == "PhoneCase") && 
                Random.value > nonLearningFeatureChangeChance)
            {
                continue;
            }
            
            // Special handling for hair
            if (category == "FrontHair" || category == "BackHair")
            {
                // Only handle hair once
                if (hairDifferenceCreated)
                    continue;
                    
                // Create a hair difference
                CreateHairDifference(leftFace, rightFace);
                hairDifferenceCreated = true;
                changesCreated++;
            }
            else
            {
                // For non-hair categories, apply the change with probability
                if (Random.value < nonLearningFeatureChangeChance)
                {
                    // 80% chance to use learned features, 20% any feature
                    if (Random.value < 0.8f)
                    {
                        bool changed = ReplaceFeatureFromLearnedSet(rightFace, category);
                        if (changed)
                            changesCreated++;
                    }
                    else
                    {
                        bool changed = ReplaceFeatureRandomly(rightFace, category);
                        if (changed)
                            changesCreated++;
                    }
                }
            }
        }
        
        // If we didn't create any differences, force at least one
        if (changesCreated == 0 && availableCategories.Count > 0)
        {
            string forcedCategory = availableCategories[0];
            ReplaceFeatureFromLearnedSet(rightFace, forcedCategory);
        }
        
        // Apply hair consistency if needed
        if (!hairDifferenceCreated)
        {
            EnforceHairConsistency(rightFace);
        }
        
        // Verify the faces are actually different
        bool actuallyDifferent = !AreFacesIdentical(leftFace, rightFace);
        if (!actuallyDifferent)
        {
            Debug.LogWarning("Generated 'different' faces are actually identical! Forcing a difference...");
            // Force a difference in the first available category
            if (availableCategories.Count > 0)
            {
                ReplaceFeatureRandomly(rightFace, availableCategories[0]);
            }
        }
        
        // Set faces in the face manager
        faceManager.SetLeftFace(leftFace);
        faceManager.SetRightFace(rightFace);
        faceManager.SetFacesIdentical(false);
        DebugLogFaceGeneration("Different Learned Faces", leftFace, rightFace);
        
        // Track differences
        TrackDifferences(leftFace, rightFace);
        currentFacesIdentical = false;
    }
    

    // New method to create hair differences
    private void CreateHairDifference(Face leftFace, Face rightFace)
    {
        // Special case: if one face has front hair and other has back hair
        if (leftFace.frontHair != null && leftFace.backHair == null)
        {
            // Give right face back hair instead
            rightFace.frontHair = null;
            rightFace.backHair = FaceDatabase.Instance.GetRandomLearnedFeature("BackHair");
        }
        else if (leftFace.backHair != null && leftFace.frontHair == null)
        {
            // Give right face front hair instead
            rightFace.backHair = null;
            rightFace.frontHair = FaceDatabase.Instance.GetRandomLearnedFeature("FrontHair");
        }
        else
        {
            // If left face has both or neither hair type, just pick one for each
            if (Random.value < 0.5f)
            {
                // Left face gets front hair, right gets back hair
                leftFace.backHair = null;
                leftFace.frontHair = FaceDatabase.Instance.GetRandomLearnedFeature("FrontHair");
                
                rightFace.frontHair = null;
                rightFace.backHair = FaceDatabase.Instance.GetRandomLearnedFeature("BackHair");
            }
            else
            {
                // Left face gets back hair, right gets front hair
                leftFace.frontHair = null;
                leftFace.backHair = FaceDatabase.Instance.GetRandomLearnedFeature("BackHair");
                
                rightFace.backHair = null;
                rightFace.frontHair = FaceDatabase.Instance.GetRandomLearnedFeature("FrontHair");
            }
        }
    }
    
    // Generate faces from unlearned groups/sets
    // In GenerateUnlearnedFaces method
    private void GenerateUnlearnedFaces()
    {
        // Try to select an unlearned group
        FeatureGroup selectedGroup = SelectUnlearnedGroup();
        
        if (selectedGroup == null)
        {
            // Fall back to differences if no unlearned groups
            Debug.Log("No unlearned groups available, falling back to different learned faces");
            GenerateDifferentLearnedFaces();
            return;
        }
        
        Debug.Log($"Selected unlearned group: {selectedGroup.groupName}");
        
        // Get next unlearned set from the group
        FaceSet currentSet = selectedGroup.GetNextUnlearnedSet();
        
        if (currentSet == null)
        {
            // Mark group as learned if we got here somehow
            selectedGroup.isLearned = true;
            
            // Fall back to differences
            Debug.Log($"No unlearned sets in the selected group '{selectedGroup.groupName}', falling back to different learned faces");
            GenerateDifferentLearnedFaces();
            return;
        }
        
        // Log detailed info about the selected set
        Debug.Log($"Selected set with {currentSet.leftPart.features.Count} left features and {currentSet.rightPart.features.Count} right features");
        
        // Decide if we should flip sides (50% chance)
        bool flipSides = (Random.value < 0.5f);
        
        // Generate ONLY ONE base face with learned features
        Face baseFace = new Face();
        FillWithLearnedFeatures(baseFace, maxFeaturesPerFace);
        
        // Clone the base face for both left and right
        Face leftFace = baseFace.Clone();
        Face rightFace = baseFace.Clone();
        
        // Apply set-specific features (possibly flipping sides)
        if (!flipSides)
        {
            Debug.Log("Applying set features without flipping");
            ApplySetFeatures(leftFace, currentSet.leftPart.features);
            ApplySetFeatures(rightFace, currentSet.rightPart.features);
        }
        else
        {
            Debug.Log("Applying set features with flipping");
            ApplySetFeatures(leftFace, currentSet.rightPart.features);
            ApplySetFeatures(rightFace, currentSet.leftPart.features);
        }
        
        // Apply hair consistency with preservation
        EnforceHairConsistency(leftFace, true);
        EnforceHairConsistency(rightFace, true);
        
        // Determine if faces are identical based on the set
        bool areIdentical = currentSet.AreFacesIdentical();
        
        // Verify with a direct comparison
        bool directComparison = AreFacesIdentical(leftFace, rightFace);
        
        if (areIdentical != directComparison) {
            Debug.LogWarning($"Identity mismatch! Set says {areIdentical} but direct comparison says {directComparison}");
            areIdentical = directComparison;
        }
        
        // Set faces in the face manager
        faceManager.SetLeftFace(leftFace);
        faceManager.SetRightFace(rightFace);
        faceManager.SetFacesIdentical(areIdentical);
        
        // Update tracking for learning progress
        currentActiveSet = currentSet;
        currentActiveGroup = selectedGroup;
        currentFacesIdentical = areIdentical;
        
        // Track differences
        TrackDifferences(leftFace, rightFace);
        
        Debug.Log($"Generated faces from group '{selectedGroup.groupName}', flipped: {flipSides}, identical: {areIdentical}");
    }
    
    // Fill a face with random learned features
    // Modify FillWithLearnedFeatures to ensure it adds enough features
    private void FillWithLearnedFeatures(Face face, int maxFeatures)
    {
        Debug.Log($"Starting to fill face with up to {maxFeatures} features (plus BG and Phone)");
        
        // Always add background and phone case (mandatory)
        face.background = FaceDatabase.Instance.GetRandomLearnedFeature("Background");
        face.phoneCase = FaceDatabase.Instance.GetRandomLearnedFeature("PhoneCase");
        
        // Track already added categories to avoid duplication
        HashSet<string> addedCategories = new HashSet<string>{"Background", "PhoneCase"};
        
        // Get all facial feature categories (excluding background and phone case)
        List<string> facialFeatureCategories = new List<string>();
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            if (category != "Background" && category != "PhoneCase")
            {
                facialFeatureCategories.Add(category);
            }
        }
        
        // Apply weights to categories based on importance
        Dictionary<string, float> categoryWeights = new Dictionary<string, float>
        {
            {"FaceShape", 1.0f},
            {"Eye", 0.9f},
            {"Nose", 0.8f},
            {"Mouth", 0.8f},
            {"FrontHair", 0.6f},
            {"BackHair", 0.5f},
            {"Ear", 0.4f},
            {"Shoulder", 0.7f}
        };
        
        // First pass: add the most important features (face, eyes, mouth)
        List<string> requiredCategories = new List<string>{"FaceShape", "Eye", "Mouth"};
        foreach (string category in requiredCategories)
        {
            if (addedCategories.Count >= maxFeatures + 2)
                break;
                
            if (!addedCategories.Contains(category))
            {
                FacialFeature feature = FaceDatabase.Instance.GetRandomLearnedFeature(category);
                if (feature != null)
                {
                    face.SetFeature(category, feature);
                    addedCategories.Add(category);
                    Debug.Log($"Added required feature: {category}:{feature.partName} (Learned: {feature.isLearned})");
                }
            }
        }
        
        // Second pass: add remaining features based on weights until we hit the max
        List<string> remainingCategories = facialFeatureCategories.FindAll(c => !addedCategories.Contains(c));
        
        // Sort by weight (higher weights first)
        remainingCategories.Sort((a, b) => 
            categoryWeights.ContainsKey(b) && categoryWeights.ContainsKey(a) ? 
            categoryWeights[b].CompareTo(categoryWeights[a]) : 0);
        
        foreach (string category in remainingCategories)
        {
            if (addedCategories.Count >= maxFeatures + 2)
                break;
                
            FacialFeature feature = FaceDatabase.Instance.GetRandomLearnedFeature(category);
            if (feature != null)
            {
                face.SetFeature(category, feature);
                addedCategories.Add(category);
                Debug.Log($"Added feature: {category}:{feature.partName} (Learned: {feature.isLearned})");
            }
        }
        
        int actualFeatureCount = 0;
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            if (face.GetFeature(category) != null)
                actualFeatureCount++;
        }
        
        Debug.Log($"Final face has {actualFeatureCount - 2} facial features (plus BG and Phone)");
        
        // Hair consistency should be handled after this
    }
    
    // Apply a list of features to a face
    private void ApplySetFeatures(Face face, List<FacialFeature> features)
    {
        foreach (FacialFeature feature in features)
        {
            face.SetFeature(feature.category, feature);
        }
    }
    
    // Update ReplaceFeatureFromLearnedSet to return if it successfully changed the feature
    // Replace a feature from a learned set
    private bool ReplaceFeatureFromLearnedSet(Face face, string category)
    {
        // Get all learned sets
        List<FaceSet> learnedSets = FaceDatabase.Instance.GetAllLearnedSets();
    
        if (learnedSets.Count == 0)
        {
            // Fall back to random replacement
            return ReplaceFeatureRandomly(face, category);
        }
    
        // Shuffle the list
        ShuffleList(learnedSets);
    
        // Get the current feature
        FacialFeature currentFeature = face.GetFeature(category);
    
        if (currentFeature == null) return false;
    
        // Try to find a different feature in the learned sets
        foreach (FaceSet set in learnedSets)
        {
            // Check left part
            foreach (FacialFeature feature in set.leftPart.features)
            {
                if (feature.category == category && feature.id != currentFeature.id)
                {
                    face.SetFeature(category, feature);
                    return true;
                }
            }
        
            // Check right part
            foreach (FacialFeature feature in set.rightPart.features)
            {
                if (feature.category == category && feature.id != currentFeature.id)
                {
                    face.SetFeature(category, feature);
                    return true;
                }
            }
        }
    
        // Fall back to random replacement if no good match found
        return ReplaceFeatureRandomly(face, category);
    }
    
    // Replace a feature with a random different one
    private bool ReplaceFeatureRandomly(Face face, string category)
    {
        // Get the current feature
        FacialFeature currentFeature = face.GetFeature(category);
    
        if (currentFeature == null) return false; // Can't replace if it doesn't exist
    
        // Get all features in this category
        List<FacialFeature> allFeatures = FaceDatabase.Instance.GetFeaturesByCategory(category);
    
        // Filter out the current feature
        List<FacialFeature> availableFeatures = new List<FacialFeature>();
    
        foreach (FacialFeature feature in allFeatures)
        {
            if (feature.id != currentFeature.id)
            {
                availableFeatures.Add(feature);
            }
        }
    
        if (availableFeatures.Count == 0) return false; // No alternatives available
    
        // Pick a random different feature
        int randomIndex = Random.Range(0, availableFeatures.Count);
        face.SetFeature(category, availableFeatures[randomIndex]);
    
        return true; // Successfully changed feature
    }
    
    // Select an unlearned group based on weights
    private FeatureGroup SelectUnlearnedGroup()
    {
        List<FeatureGroup> unlearnedGroups = FaceDatabase.Instance.GetUnlearnedGroups();
        
        if (unlearnedGroups.Count == 0)
        {
            return null;
        }
        
        // Calculate total weight
        float totalWeight = 0;
        foreach (FeatureGroup group in unlearnedGroups)
        {
            totalWeight += group.selectionChance;
        }
        
        // Random selection based on weights
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0;
        
        foreach (FeatureGroup group in unlearnedGroups)
        {
            currentWeight += group.selectionChance;
            
            if (randomValue <= currentWeight)
            {
                return group;
            }
        }
        
        // Fallback to first group
        return unlearnedGroups[0];
    }
    
    // Called when a round is completed (success or failure)
    // Ensure learning works properly
    // In FaceGenerator.cs
    public void OnRoundCompleted(bool playerSucceeded)
    {
        Debug.Log($"Round completed, success: {playerSucceeded}");
        
        // Only mark learning if we used an unlearned set
        if (currentActiveSet != null)
        {
            Debug.Log($"Active set found, was learned: {currentActiveSet.isLearned}");
            
            if (!currentActiveSet.isLearned)
            {
                // Mark set as learned
                currentActiveSet.isLearned = true;
                
                // Mark all features in the set as learned
                foreach (FacialFeature feature in currentActiveSet.leftPart.features)
                {
                    Debug.Log($"Marking as learned: {feature.category}:{feature.partName}");
                    feature.isLearned = true;
                }
                
                foreach (FacialFeature feature in currentActiveSet.rightPart.features)
                {
                    Debug.Log($"Marking as learned: {feature.category}:{feature.partName}");
                    feature.isLearned = true;
                }
                
                Debug.Log("Marked set as learned with its features");
                
                // Check if the group is now fully learned
                if (currentActiveGroup != null)
                {
                    bool allSetsLearned = true;
                    foreach (FaceSet set in currentActiveGroup.sets)
                    {
                        if (!set.isLearned)
                        {
                            allSetsLearned = false;
                            break;
                        }
                    }
                    
                    if (allSetsLearned && !currentActiveGroup.isLearned)
                    {
                        currentActiveGroup.isLearned = true;
                        Debug.Log($"Group '{currentActiveGroup.groupName}' is now fully learned");
                    }
                    else
                    {
                        Debug.Log($"Group '{currentActiveGroup.groupName}' still has unlearned sets");
                    }
                }
            }
            else
            {
                Debug.Log("Set was already marked as learned, no changes made");
            }
        }
        else
        {
            Debug.Log("No active set to mark as learned");
        }
    }
    
    // Helper to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    // Add this method to enforce only one hair type per face
    // In FaceGenerator.cs
    // In FaceGenerator.cs
    // In FaceGenerator.cs
    private void EnforceHairConsistency(Face face, bool preserveLearningFeatures = true)
    {
        // Skip if both hair types are null
        if (face.frontHair == null && face.backHair == null)
            return;
        
        // Skip if only one hair type is present
        if ((face.frontHair != null && face.backHair == null) || 
            (face.frontHair == null && face.backHair != null))
            return;
    
        // If we get here, both hair types are present
        Debug.Log("Both hair types present, enforcing consistency");
    
        bool frontHairFromLearningSet = IsFeatureFromLearningSet(face.frontHair);
        bool backHairFromLearningSet = IsFeatureFromLearningSet(face.backHair);
    
        // If preserving learning features and one is from a learning set
        if (preserveLearningFeatures)
        {
            if (frontHairFromLearningSet && !backHairFromLearningSet)
            {
                Debug.Log("Keeping front hair (from learning set)");
                face.backHair = null;
            }
            else if (!frontHairFromLearningSet && backHairFromLearningSet)
            {
                Debug.Log("Keeping back hair (from learning set)");
                face.frontHair = null;
            }
            // If both or neither are from learning sets
            else
            {
                // Randomly choose one
                if (Random.value < 0.5f)
                {
                    Debug.Log("Randomly keeping front hair");
                    face.backHair = null;
                }
                else
                {
                    Debug.Log("Randomly keeping back hair");
                    face.frontHair = null;
                }
            }
        }
        // If not preserving learning features, just randomly choose one
        else
        {
            if (Random.value < 0.5f)
            {
                face.backHair = null;
            }
            else
            {
                face.frontHair = null;
            }
        }
    }

    // Helper method to check if a set part has both hair types
    private bool HasBothHairTypes(SetPart part)
    {
        bool hasFrontHair = false;
        bool hasBackHair = false;
        
        foreach (FacialFeature feature in part.features)
        {
            if (feature.category == "FrontHair")
                hasFrontHair = true;
            else if (feature.category == "BackHair")
                hasBackHair = true;
        }
        
        return hasFrontHair && hasBackHair;
    }

    // Helper method to check if a feature is from a learning set
    private bool IsFeatureFromLearningSet(FacialFeature feature)
    {
        if (feature == null || currentActiveSet == null)
            return false;
        
        // Check if the feature is in the current active set
        foreach (FacialFeature setFeature in currentActiveSet.leftPart.features)
        {
            if (setFeature.id == feature.id)
                return true;
        }
    
        foreach (FacialFeature setFeature in currentActiveSet.rightPart.features)
        {
            if (setFeature.id == feature.id)
                return true;
        }
    
        return false;
    }
    
    // Enhanced validation for face identity
    private bool ValidateFaceIdentity(Face leftFace, Face rightFace)
    {
        bool identical = true;
    
        // Check all categories
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature leftFeature = leftFace.GetFeature(category);
            FacialFeature rightFeature = rightFace.GetFeature(category);
        
            // If one has feature and other doesn't
            if ((leftFeature == null && rightFeature != null) || 
                (leftFeature != null && rightFeature == null))
            {
                identical = false;
                Debug.Log($"Difference found in {category}: One face has it, other doesn't");
                break;
            }
        
            // If both have features but they're different
            if (leftFeature != null && rightFeature != null)
            {
                if (leftFeature.id != rightFeature.id)
                {
                    identical = false;
                    Debug.Log($"Difference found in {category}: {leftFeature.partName} vs {rightFeature.partName}");
                    break;
                }
            }
        }
    
        // Special case for hair - if one has front hair and other has back hair, they're different
        bool leftHasFrontHair = leftFace.frontHair != null;
        bool rightHasFrontHair = rightFace.frontHair != null;
        bool leftHasBackHair = leftFace.backHair != null;
        bool rightHasBackHair = rightFace.backHair != null;
    
        // If hair types don't match between faces
        if (leftHasFrontHair != rightHasFrontHair || leftHasBackHair != rightHasBackHair)
        {
            identical = false;
            Debug.Log("Difference found in hair types");
        }
    
        return identical;
    }
    
    //to Track the difference between faces to remind player where to look for
    private void TrackDifferences(Face leftFace, Face rightFace)
    {
        currentDifferentCategories.Clear();
        
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature leftFeature = leftFace.GetFeature(category);
            FacialFeature rightFeature = rightFace.GetFeature(category);
            
            // If features are different, track the category
            if (leftFeature != null && rightFeature != null && leftFeature.id != rightFeature.id)
            {
                currentDifferentCategories.Add(category);
            }
            else if ((leftFeature == null && rightFeature != null) || 
                    (leftFeature != null && rightFeature == null))
            {
                currentDifferentCategories.Add(category);
            }
        }
        
        // Special check for hair types
        bool leftHasFrontHair = leftFace.frontHair != null;
        bool rightHasFrontHair = rightFace.frontHair != null;
        bool leftHasBackHair = leftFace.backHair != null;
        bool rightHasBackHair = rightFace.backHair != null;
        
        // If hair types don't match between faces
        if ((leftHasFrontHair && !rightHasFrontHair) || (!leftHasFrontHair && rightHasFrontHair))
        {
            if (!currentDifferentCategories.Contains("FrontHair"))
                currentDifferentCategories.Add("FrontHair");
        }
        
        if ((leftHasBackHair && !rightHasBackHair) || (!leftHasBackHair && rightHasBackHair))
        {
            if (!currentDifferentCategories.Contains("BackHair"))
                currentDifferentCategories.Add("BackHair");
        }
    }

    // Add this method for getting feedback
    public string GetDifferenceFeedback()
    {
        if (currentDifferentCategories.Count == 0)
            return "";
            
        string feedback = "Check the ";
        
        // Make category names more user-friendly
        Dictionary<string, string> friendlyNames = new Dictionary<string, string>
        {
            {"FaceShape", "face shape"},
            {"Eye", "eyes"},
            {"Nose", "nose"},
            {"Mouth", "mouth"},
            {"FrontHair", "front hair"},
            {"BackHair", "back hair"},
            {"Ear", "ears"},
            {"Shoulder", "shoulders"},
            {"Background", "background"},
            {"PhoneCase", "phone case"}
        };
        
        for (int i = 0; i < currentDifferentCategories.Count; i++)
        {
            string category = currentDifferentCategories[i];
            string readableName = friendlyNames.ContainsKey(category) ? friendlyNames[category] : category;
            
            if (i > 0)
            {
                if (i == currentDifferentCategories.Count - 1)
                    feedback += " and ";
                else
                    feedback += ", ";
            }
            
            feedback += readableName;
        }
        
        return feedback;
    }
    
    // Check if two faces are identical (all features match)
    private bool AreFacesIdentical(Face leftFace, Face rightFace)
    {
        if (leftFace == null || rightFace == null)
            return false;
        
        // Check all categories
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature leftFeature = leftFace.GetFeature(category);
            FacialFeature rightFeature = rightFace.GetFeature(category);
        
            // If one has feature and other doesn't
            if ((leftFeature == null && rightFeature != null) || 
                (leftFeature != null && rightFeature == null))
            {
                return false;
            }
        
            // If both have features but they're different
            if (leftFeature != null && rightFeature != null)
            {
                if (leftFeature.id != rightFeature.id)
                {
                    return false;
                }
            }
        }
    
        // Special check for hair types (one has front hair and other has back hair)
        bool leftHasFrontHair = leftFace.frontHair != null;
        bool rightHasFrontHair = rightFace.frontHair != null;
        bool leftHasBackHair = leftFace.backHair != null;
        bool rightHasBackHair = rightFace.backHair != null;
    
        if (leftHasFrontHair != rightHasFrontHair || leftHasBackHair != rightHasBackHair)
        {
            return false;
        }
    
        // All features match
        return true;
    }
    
    // Add to FaceGenerator class
    public void DebugLogLearnedFeatures()
    {
        Debug.Log("=== LISTING ALL LEARNED FEATURES ===");
        
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            List<FacialFeature> learnedFeatures = FaceDatabase.Instance.GetLearnedFeatures(category);
            
            Debug.Log($"{category}: {learnedFeatures.Count} learned features");
            
            for (int i = 0; i < learnedFeatures.Count; i++)
            {
                Debug.Log($"  {i+1}. {learnedFeatures[i].partName} (ID: {learnedFeatures[i].id})");
            }
        }
        
        Debug.Log("=== END OF LEARNED FEATURES LIST ===");
    }

    // Add to FaceGenerator class - Detailed debugging for face generation
    private void DebugLogFaceGeneration(string method, Face leftFace, Face rightFace)
    {
        Debug.Log($"=== FACE GENERATION DEBUG: {method} ===");
        
        // Log left face features
        Debug.Log("LEFT FACE FEATURES:");
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature feature = leftFace.GetFeature(category);
            if (feature != null)
            {
                Debug.Log($"  {category}: {feature.partName} (Learned: {feature.isLearned})");
            }
        }
        
        // Log right face features
        Debug.Log("RIGHT FACE FEATURES:");
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature feature = rightFace.GetFeature(category);
            if (feature != null)
            {
                Debug.Log($"  {category}: {feature.partName} (Learned: {feature.isLearned})");
            }
        }
        
        // Identify differences
        Debug.Log("DIFFERENCES:");
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            FacialFeature leftFeature = leftFace.GetFeature(category);
            FacialFeature rightFeature = rightFace.GetFeature(category);
            
            bool different = false;
            
            if ((leftFeature == null && rightFeature != null) ||
                (leftFeature != null && rightFeature == null))
            {
                different = true;
            }
            else if (leftFeature != null && rightFeature != null && 
                    leftFeature.id != rightFeature.id)
            {
                different = true;
            }
            
            if (different)
            {
                string leftDesc = leftFeature != null ? $"{leftFeature.partName} (Learned: {leftFeature.isLearned})" : "None";
                string rightDesc = rightFeature != null ? $"{rightFeature.partName} (Learned: {rightFeature.isLearned})" : "None";
                
                Debug.Log($"  {category}: LEFT = {leftDesc}, RIGHT = {rightDesc}");
            }
        }
        
        Debug.Log("=== END OF FACE GENERATION DEBUG ===");
    }
    
    // Add this to FaceGenerator.cs
    public void DiagnoseCurrentIssues()
    {
        Debug.Log("=== FACE GENERATION DIAGNOSTIC TOOL ===");
        
        // 1. Test learning feature functionality
        Debug.Log("--- Testing feature learning ---");
        TestFeatureLearning();
        
        // 2. Test random feature selection
        Debug.Log("--- Testing feature selection ---");
        TestLearnedFeatureSelection();
        
        // 3. Test face filling with correct number of features
        Debug.Log("--- Testing face filling ---");
        TestFaceFilling();
        
        // 4. Test group selection
        Debug.Log("--- Testing group selection ---");
        TestGroupSelection();
        
        Debug.Log("=== DIAGNOSTIC COMPLETE ===");
    }

    private void TestFeatureLearning()
    {
        // Create a test feature
        FacialFeature testFeature = new FacialFeature("test-id", "Eye", "Test Eye", null, false);
        
        // Create a test set
        FaceSet testSet = new FaceSet();
        testSet.isLearned = false;
        testSet.leftPart.features.Add(testFeature);
        
        // Save current values
        currentActiveSet = testSet;
        currentActiveGroup = null;
        
        // Check before
        Debug.Log($"Before OnRoundCompleted: Feature.isLearned={testFeature.isLearned}, Set.isLearned={testSet.isLearned}");
        
        // Call the method
        OnRoundCompleted(true);
        
        // Check after
        Debug.Log($"After OnRoundCompleted: Feature.isLearned={testFeature.isLearned}, Set.isLearned={testSet.isLearned}");
        
        // Reset test state
        currentActiveSet = null;
    }

    private void TestLearnedFeatureSelection()
    {
        // Create temp database entries
        FacialFeature learned = new FacialFeature("learned-id", "TestCategory", "Learned Feature", null, true);
        FacialFeature unlearned = new FacialFeature("unlearned-id", "TestCategory", "Unlearned Feature", null, false);
        
        // Add to database via temp list
        List<FacialFeature> tempFeatures = new List<FacialFeature>() { learned, unlearned };
        
        // Check what GetRandomLearnedFeature would return
        Debug.Log($"Normal database has {FaceDatabase.Instance.GetLearnedFeatures("Eye").Count} learned Eye features");
        Debug.Log($"Normal database has {FaceDatabase.Instance.GetLearnedFeatures("Nose").Count} learned Nose features");
        
        // Simulate calls
        Debug.Log("Running GetRandomLearnedFeature simulation");
        SimulateGetRandomLearnedFeature("Eye", 5);
        SimulateGetRandomLearnedFeature("Nose", 5);
        
        // Don't actually modify database
    }

    private void SimulateGetRandomLearnedFeature(string category, int count)
    {
        List<FacialFeature> learnedFeatures = FaceDatabase.Instance.GetLearnedFeatures(category);
        Debug.Log($"Category {category} has {learnedFeatures.Count} learned features");
        
        for (int i = 0; i < count; i++)
        {
            if (learnedFeatures.Count > 0)
            {
                int randomIndex = Random.Range(0, learnedFeatures.Count);
                FacialFeature selected = learnedFeatures[randomIndex];
                Debug.Log($"Simulation {i+1}: Would select {selected.partName} (Learned: {selected.isLearned})");
            }
            else
            {
                List<FacialFeature> allFeatures = FaceDatabase.Instance.GetFeaturesByCategory(category);
                if (allFeatures.Count > 0)
                {
                    int randomIndex = Random.Range(0, allFeatures.Count);
                    FacialFeature selected = allFeatures[randomIndex];
                    Debug.Log($"Simulation {i+1}: No learned features, would fall back to {selected.partName} (Learned: {selected.isLearned})");
                }
                else
                {
                    Debug.Log($"Simulation {i+1}: No features available for category {category}");
                }
            }
        }
    }

    private void TestFaceFilling()
    {
        // Test filling with different max counts
        for (int maxCount = 3; maxCount <= 6; maxCount++)
        {
            Face testFace = new Face();
            Debug.Log($"Filling face with max {maxCount} features:");
            
            // Fill face but don't modify UI
            FillWithLearnedFeatures(testFace, maxCount);
            
            // Count actual features
            int actualCount = 0;
            foreach (string category in FaceDatabase.Instance.FeatureCategories)
            {
                if (testFace.GetFeature(category) != null)
                {
                    actualCount++;
                    FacialFeature feature = testFace.GetFeature(category);
                    Debug.Log($"  - Added {category}: {feature.partName} (Learned: {feature.isLearned})");
                }
            }
            
            Debug.Log($"Face filled with {actualCount} total features (expected around {maxCount} + BG + Phone)");
            
            // Check if we have hair inconsistency
            if (testFace.frontHair != null && testFace.backHair != null)
            {
                Debug.LogWarning("Face has both front and back hair!");
            }
        }
    }

    private void TestGroupSelection()
    {
        // Get current groups
        List<FeatureGroup> unlearnedGroups = FaceDatabase.Instance.GetUnlearnedGroups();
        
        Debug.Log($"Database has {unlearnedGroups.Count} unlearned groups:");
        
        foreach (FeatureGroup group in unlearnedGroups)
        {
            int unlearnedSets = 0;
            foreach (FaceSet set in group.sets)
            {
                if (!set.isLearned) unlearnedSets++;
            }
            
            Debug.Log($"  - Group: {group.groupName}, Selection chance: {group.selectionChance}, " +
                     $"Unlearned sets: {unlearnedSets}/{group.sets.Count}");
        }
        
        // Simulate a few selections
        Debug.Log("Simulating 5 group selections:");
        
        for (int i = 0; i < 5; i++)
        {
            FeatureGroup selected = SelectUnlearnedGroup();
            if (selected != null)
            {
                FaceSet nextSet = selected.GetNextUnlearnedSet();
                Debug.Log($"Selection {i+1}: Group '{selected.groupName}', next unlearned set: {(nextSet != null ? "Found" : "None")}");
                
                if (nextSet != null)
                {
                    Debug.Log($"  Left part: {nextSet.leftPart.features.Count} features, Right part: {nextSet.rightPart.features.Count} features");
                    Debug.Log($"  Would create identical faces: {nextSet.AreFacesIdentical()}");
                }
            }
            else
            {
                Debug.Log($"Selection {i+1}: No unlearned groups available");
            }
        }
    }
}