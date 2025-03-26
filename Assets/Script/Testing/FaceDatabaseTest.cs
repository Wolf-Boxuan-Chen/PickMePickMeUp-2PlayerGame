using UnityEngine;
using System.Collections.Generic;

public class FaceDatabaseTest : MonoBehaviour
{
    private FaceDatabase faceDatabase;
    private List<string> testResults = new List<string>();
    
    private void Start()
    {
        // Find database
        faceDatabase = FindFirstObjectByType<FaceDatabase>();
        
        if (faceDatabase == null)
        {
            Debug.LogError("FaceDatabase not found in scene! Add one before testing.");
            return;
        }
        
        // Run tests
        TestAddingFeatures();
        TestGroupManagement();
        TestFeatureQueries();
        
        // Print all results
        Debug.Log("=== DATABASE TEST RESULTS ===");
        foreach (string result in testResults)
        {
            Debug.Log(result);
        }
    }
    
    private void TestAddingFeatures()
    {
        Debug.Log("Testing adding features to database...");
        
        // Count features before
        int initialEyeCount = faceDatabase.GetFeaturesByCategory("Eye").Count;
        
        // Add test feature
        Sprite testSprite = Resources.Load<Sprite>("TestSprite"); // Replace with an actual sprite path
        FacialFeature feature = faceDatabase.AddFeature("Eye", "Test Feature", testSprite, false);
        
        // Count features after
        int afterEyeCount = faceDatabase.GetFeaturesByCategory("Eye").Count;
        
        // Check results
        bool test1 = afterEyeCount == initialEyeCount + 1;
        
        // Verify feature
        bool test2 = feature != null;
        bool test3 = feature.category == "Eye";
        bool test4 = feature.partName == "Test Feature";
        bool test5 = !feature.isLearned;
        
        // Log results
        testResults.Add($"Feature Addition Tests: {(test1 && test2 && test3 && test4 && test5 ? "PASSED" : "FAILED")}");
    }
    
    private void TestGroupManagement()
    {
        Debug.Log("Testing group management...");
        
        // Count groups before
        int initialGroupCount = faceDatabase.GetLearnedGroups().Count + faceDatabase.GetUnlearnedGroups().Count;
        
        // Create test group
        FeatureGroup group = faceDatabase.CreateGroup("Test Group");
        
        // Count groups after
        int afterGroupCount = faceDatabase.GetLearnedGroups().Count + faceDatabase.GetUnlearnedGroups().Count;
        
        // Check results
        bool test1 = afterGroupCount == initialGroupCount + 1;
        
        // Verify group
        bool test2 = group != null;
        bool test3 = group.groupName == "Test Group";
        bool test4 = !group.isLearned;
        bool test5 = group.selectionChance == 1.0f;
        bool test6 = group.sets.Count == 0;
        
        // Create a set for this group
        FaceSet set = new FaceSet();
        group.AddSet(set);
        
        bool test7 = group.sets.Count == 1;
        
        // Get unlearned groups
        List<FeatureGroup> unlearnedGroups = faceDatabase.GetUnlearnedGroups();
        bool test8 = unlearnedGroups.Contains(group);
        
        // Mark group as learned
        group.isLearned = true;
        
        // Get learned groups
        List<FeatureGroup> learnedGroups = faceDatabase.GetLearnedGroups();
        bool test9 = learnedGroups.Contains(group);
        bool test10 = !faceDatabase.GetUnlearnedGroups().Contains(group);
        
        // Log results
        testResults.Add($"Group Management Tests: {(test1 && test2 && test3 && test4 && test5 && test6 && test7 && test8 && test9 && test10 ? "PASSED" : "FAILED")}");
    }
    
    private void TestFeatureQueries()
    {
        Debug.Log("Testing feature queries...");
        
        // Create learned and unlearned features
        Sprite testSprite = Resources.Load<Sprite>("TestSprite"); // Replace with an actual sprite path
        FacialFeature learned = faceDatabase.AddFeature("Nose", "Learned Feature", testSprite, true);
        FacialFeature unlearned = faceDatabase.AddFeature("Nose", "Unlearned Feature", testSprite, false);
        
        // Test getting learned features
        List<FacialFeature> learnedFeatures = faceDatabase.GetLearnedFeatures("Nose");
        bool test1 = learnedFeatures.Contains(learned);
        bool test2 = !learnedFeatures.Contains(unlearned);
        
        // Test getting unlearned features
        List<FacialFeature> unlearnedFeatures = faceDatabase.GetUnlearnedFeatures("Nose");
        bool test3 = !unlearnedFeatures.Contains(learned);
        bool test4 = unlearnedFeatures.Contains(unlearned);
        
        // Test getting random learned feature
        FacialFeature randomLearned = faceDatabase.GetRandomLearnedFeature("Nose");
        bool test5 = randomLearned != null;
        bool test6 = randomLearned.isLearned;
        
        // Log results
        testResults.Add($"Feature Query Tests: {(test1 && test2 && test3 && test4 && test5 && test6 ? "PASSED" : "FAILED")}");
    }
}