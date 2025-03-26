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
        
        // Adjust feature complexity based on round
        if (currentRound <= 3)
        {
            maxFeaturesPerFace = 3;  // First 3 rounds: 3 features + bg + phone
        }
        else if (currentRound <= 8)
        {
            maxFeaturesPerFace = 5;  // Next 5 rounds: 4-5 features + bg + phone
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
            Debug.Log("Generating identical learned faces");
            GenerateIdenticalLearnedFaces();
        }
        else if (rng < 0.6f)
        {
            // 30% chance: Generate faces with differences
            Debug.Log("Generating different learned faces");
            GenerateDifferentLearnedFaces();
        }
        else
        {
            // 40% chance: Use unlearned group/set
            Debug.Log("Generating faces from unlearned group");
            GenerateUnlearnedFaces();
        }
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
        
        // Update tracking
        currentFacesIdentical = true;
        //Track differences between faces so we can mention it when players can't figure it out
        TrackDifferences(leftFace, rightFace);
    }
    
    // Generate faces with 1-2 differences from learned features
    // In GenerateDifferentLearnedFaces method:
    private void GenerateDifferentLearnedFaces()
    {
        Face leftFace = new Face();
        Face rightFace;
        
        // Fill with random learned features
        FillWithLearnedFeatures(leftFace, maxFeaturesPerFace);
        
        // Apply hair consistency to base face
        EnforceHairConsistency(leftFace);
        
        // Clone the left face to the right
        rightFace = leftFace.Clone();
        
        // Decide how many differences (1 or 2)
        int numDifferences = (Random.value < 0.7f) ? 1 : 2;
        
        // Track if we're creating a hair-specific difference
        bool hairDifferenceCreated = false;
        
        // Get categories with features
        List<string> availableCategories = new List<string>();
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            if (leftFace.GetFeature(category) != null && 
                category != "Background" && 
                category != "PhoneCase")
            {
                availableCategories.Add(category);
            }
        }
        
        // Shuffle the list
        ShuffleList(availableCategories);
        
        // Create differences
        for (int i = 0; i < numDifferences && i < availableCategories.Count; i++)
        {
            string category = availableCategories[i];
            
            // Special handling for hair categories
            if (category == "FrontHair" || category == "BackHair")
            {
                // If we already made a hair difference, skip
                if (hairDifferenceCreated)
                    continue;
                    
                // Make a special hair difference
                CreateHairDifference(leftFace, rightFace);
                hairDifferenceCreated = true;
            }
            else
            {
                // Regular difference for non-hair features
                if (Random.value < 0.8f)
                    ReplaceFeatureFromLearnedSet(rightFace, category);
                else
                    ReplaceFeatureRandomly(rightFace, category);
            }
        }
        
        // Apply hair consistency with preservation AFTER differences
        if (!hairDifferenceCreated)
        {
            EnforceHairConsistency(rightFace);
        }
        
        // Set faces in the face manager
        faceManager.SetLeftFace(leftFace);
        faceManager.SetRightFace(rightFace);
        faceManager.SetFacesIdentical(false);
        
        // Track the differences
        TrackDifferences(leftFace, rightFace);
        currentFacesIdentical = false;
        
        //Track differences between faces so we can mention it when players can't figure it out
        TrackDifferences(leftFace, rightFace);
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
        
        // Get next unlearned set from the group
        FaceSet currentSet = selectedGroup.GetNextUnlearnedSet();
        
        if (currentSet == null)
        {
            // Mark group as learned if we got here somehow
            selectedGroup.isLearned = true;
            FaceDatabase.Instance.CheckGroupCompletion(selectedGroup);
            
            // Fall back to differences
            Debug.Log("No unlearned sets in the selected group, falling back to different learned faces");
            GenerateDifferentLearnedFaces();
            return;
        }
        
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
            ApplySetFeatures(leftFace, currentSet.leftPart.features);
            ApplySetFeatures(rightFace, currentSet.rightPart.features);
        }
        else
        {
            ApplySetFeatures(leftFace, currentSet.rightPart.features);
            ApplySetFeatures(rightFace, currentSet.leftPart.features);
        }
        
        EnforceHairConsistency(leftFace, true);
        EnforceHairConsistency(rightFace, true);
        
        // Determine if faces are identical and set in face manager
        bool areIdentical = currentSet.AreFacesIdentical();
        
        // Flip the identical flag if we flipped sides
        if (flipSides)
        {
            // If parts are identical, flipping won't change that
            // If parts are different, flipping sides will maintain the difference
            // So the logic stays the same
        }
        
        faceManager.SetLeftFace(leftFace);
        faceManager.SetRightFace(rightFace);
        faceManager.SetFacesIdentical(areIdentical);
        
        // Update tracking for learning progress
        currentActiveSet = currentSet;
        currentActiveGroup = selectedGroup;
        currentFacesIdentical = areIdentical;
        
        //Track differences between faces so we can mention it when players can't figure it out
        TrackDifferences(leftFace, rightFace);
        
        Debug.Log($"Generated faces from group '{selectedGroup.groupName}', flipped: {flipSides}, identical: {areIdentical}");
    }
    
    // Fill a face with random learned features
    private void FillWithLearnedFeatures(Face face, int maxFeatures)
    {
        // Always include background and phone case
        face.background = FaceDatabase.Instance.GetRandomLearnedFeature("Background");
        face.phoneCase = FaceDatabase.Instance.GetRandomLearnedFeature("PhoneCase");
        
        // List of other features to potentially include
        List<string> categories = new List<string>
        {
            "FaceShape", "Eye", "Nose", "Mouth", "FrontHair", 
            "BackHair", "Ear", "Shoulder"
        };
        
        // Shuffle categories
        ShuffleList(categories);
        
        // Pick random features up to maxFeatures
        int featuresToAdd = Mathf.Min(maxFeatures, categories.Count);
        
        for (int i = 0; i < featuresToAdd; i++)
        {
            string category = categories[i];
            FacialFeature feature = FaceDatabase.Instance.GetRandomLearnedFeature(category);
            
            if (feature != null)
            {
                face.SetFeature(category, feature);
            }
        }
    }
    
    // Apply a list of features to a face
    private void ApplySetFeatures(Face face, List<FacialFeature> features)
    {
        foreach (FacialFeature feature in features)
        {
            face.SetFeature(feature.category, feature);
        }
    }
    
    // Replace a feature from a learned set
    private void ReplaceFeatureFromLearnedSet(Face face, string category)
    {
        // Get all learned sets
        List<FaceSet> learnedSets = FaceDatabase.Instance.GetAllLearnedSets();
        
        if (learnedSets.Count == 0)
        {
            // Fall back to random replacement
            ReplaceFeatureRandomly(face, category);
            return;
        }
        
        // Shuffle the list
        ShuffleList(learnedSets);
        
        // Get the current feature
        FacialFeature currentFeature = face.GetFeature(category);
        
        // Try to find a different feature in the learned sets
        foreach (FaceSet set in learnedSets)
        {
            // Check left part
            foreach (FacialFeature feature in set.leftPart.features)
            {
                if (feature.category == category && feature.id != currentFeature.id)
                {
                    face.SetFeature(category, feature);
                    return;
                }
            }
            
            // Check right part
            foreach (FacialFeature feature in set.rightPart.features)
            {
                if (feature.category == category && feature.id != currentFeature.id)
                {
                    face.SetFeature(category, feature);
                    return;
                }
            }
        }
        
        // Fall back to random replacement if no good match found
        ReplaceFeatureRandomly(face, category);
    }
    
    // Replace a feature with a random different one
    private void ReplaceFeatureRandomly(Face face, string category)
    {
        // Get the current feature
        FacialFeature currentFeature = face.GetFeature(category);
        
        if (currentFeature == null) return;
        
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
        
        if (availableFeatures.Count == 0) return;
        
        // Pick a random different feature
        int randomIndex = Random.Range(0, availableFeatures.Count);
        face.SetFeature(category, availableFeatures[randomIndex]);
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
    public void OnRoundCompleted(bool playerSucceeded)
    {
        // Only mark learning if we used an unlearned set
        if (currentActiveSet != null && !currentActiveSet.isLearned)
        {
            // Mark set as learned
            currentActiveSet.isLearned = true;
            
            // Mark all features in the set as learned
            foreach (FacialFeature feature in currentActiveSet.leftPart.features)
            {
                feature.isLearned = true;
            }
            
            foreach (FacialFeature feature in currentActiveSet.rightPart.features)
            {
                feature.isLearned = true;
            }
            
            Debug.Log("Marked set as learned with its features");
            
            // Check if the group is now fully learned
            if (currentActiveGroup != null)
            {
                FaceDatabase.Instance.CheckGroupCompletion(currentActiveGroup);
            }
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
    private void EnforceHairConsistency(Face face, bool preserveLearningFeatures = true)
    {
        // If both hair types are present, keep only one based on priority
        if (face.frontHair != null && face.backHair != null)
        {
            bool frontHairFromLearningSet = IsFeatureFromLearningSet(face.frontHair);
            bool backHairFromLearningSet = IsFeatureFromLearningSet(face.backHair);
        
            // If we're preserving learning features and at least one is from a learning set
            if (preserveLearningFeatures && (frontHairFromLearningSet || backHairFromLearningSet))
            {
                // Keep the one from the learning set
                if (frontHairFromLearningSet && !backHairFromLearningSet)
                    face.backHair = null;
                else if (!frontHairFromLearningSet && backHairFromLearningSet)
                    face.frontHair = null;
                // If both are from learning sets, keep both (this may be a special case for comparison)
            }
            // Otherwise, randomly choose one (for non-learning features)
            else if (!frontHairFromLearningSet && !backHairFromLearningSet)
            {
                if (Random.value < 0.5f)
                    face.backHair = null;
                else
                    face.frontHair = null;
            }
        }
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
            
        string feedback = "Differences found in: ";
        
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
}