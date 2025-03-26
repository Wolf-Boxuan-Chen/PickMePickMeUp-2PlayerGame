using UnityEngine;
using System.Collections.Generic;

// Place this script on any GameObject for testing
public class FaceStructureTest : MonoBehaviour
{
    // Reference to database (for test features)
    private FaceDatabase faceDatabase;
    
    // Test results output
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
        TestFacialFeature();
        TestSetPart();
        TestFaceSet();
        TestFace();
        
        // Print all results
        Debug.Log("=== TEST RESULTS ===");
        foreach (string result in testResults)
        {
            Debug.Log(result);
        }
    }
    
    private void TestFacialFeature()
    {
        Debug.Log("Testing FacialFeature...");
        
        // Create a test feature
        string id = System.Guid.NewGuid().ToString();
        FacialFeature feature = new FacialFeature(id, "Eye", "Test Eye", null, false);
        
        // Test properties
        bool test1 = feature.id == id;
        bool test2 = feature.category == "Eye";
        bool test3 = feature.partName == "Test Eye";
        bool test4 = feature.isLearned == false;
        
        // Toggle learned
        feature.isLearned = true;
        bool test5 = feature.isLearned == true;
        
        // Log results
        testResults.Add($"FacialFeature Tests: {(test1 && test2 && test3 && test4 && test5 ? "PASSED" : "FAILED")}");
    }
    
    private void TestSetPart()
    {
        Debug.Log("Testing SetPart...");
        
        // Create a test set part
        SetPart part = new SetPart();
        
        // Test initial state
        bool test1 = part.features.Count == 0;
        
        // Add features
        FacialFeature feature1 = new FacialFeature(System.Guid.NewGuid().ToString(), "Eye", "Test Eye 1", null);
        FacialFeature feature2 = new FacialFeature(System.Guid.NewGuid().ToString(), "Nose", "Test Nose", null);
        
        part.AddFeature(feature1);
        part.AddFeature(feature2);
        
        // Test after adding
        bool test2 = part.features.Count == 2;
        bool test3 = part.features.Contains(feature1);
        bool test4 = part.features.Contains(feature2);
        
        // Test removing
        part.RemoveFeature(feature1);
        bool test5 = part.features.Count == 1;
        bool test6 = !part.features.Contains(feature1);
        bool test7 = part.features.Contains(feature2);
        
        // Log results
        testResults.Add($"SetPart Tests: {(test1 && test2 && test3 && test4 && test5 && test6 && test7 ? "PASSED" : "FAILED")}");
    }
    
    private void TestFaceSet()
    {
        Debug.Log("Testing FaceSet...");
        
        // Create a test face set
        FaceSet set = new FaceSet();
        
        // Test initial state
        bool test1 = set.leftPart != null;
        bool test2 = set.rightPart != null;
        bool test3 = !set.isLearned;
        
        // Add identical features to both sides
        FacialFeature eye = new FacialFeature(System.Guid.NewGuid().ToString(), "Eye", "Test Eye", null);
        
        set.leftPart.AddFeature(eye);
        set.rightPart.AddFeature(eye);
        
        // Test identical detection
        bool test4 = set.AreFacesIdentical() == true;
        
        // Add different feature to right side
        FacialFeature differentEye = new FacialFeature(System.Guid.NewGuid().ToString(), "Eye", "Different Eye", null);
        set.rightPart.features.Clear();
        set.rightPart.AddFeature(differentEye);
        
        // Test difference detection
        bool test5 = set.AreFacesIdentical() == false;
        
        // Log results
        testResults.Add($"FaceSet Tests: {(test1 && test2 && test3 && test4 && test5 ? "PASSED" : "FAILED")}");
    }
    
    private void TestFace()
    {
        Debug.Log("Testing Face...");
        
        // Create a test face
        Face face = new Face();
        
        // Create test features
        FacialFeature eye = new FacialFeature(System.Guid.NewGuid().ToString(), "Eye", "Test Eye", null);
        FacialFeature nose = new FacialFeature(System.Guid.NewGuid().ToString(), "Nose", "Test Nose", null);
        
        // Set features
        face.SetFeature("Eye", eye);
        face.SetFeature("Nose", nose);
        
        // Test getting features
        bool test1 = face.GetFeature("Eye") == eye;
        bool test2 = face.GetFeature("Nose") == nose;
        bool test3 = face.GetFeature("Mouth") == null;
        
        // Test cloning
        Face clonedFace = face.Clone();
        bool test4 = clonedFace != face; // Different instance
        bool test5 = clonedFace.GetFeature("Eye") == face.GetFeature("Eye"); // Same feature
        bool test6 = clonedFace.GetFeature("Nose") == face.GetFeature("Nose"); // Same feature
        
        // Test equality
        bool test7 = face.IsEqual(clonedFace) == true;
        
        // Change cloned face
        clonedFace.SetFeature("Eye", new FacialFeature(System.Guid.NewGuid().ToString(), "Eye", "Different Eye", null));
        bool test8 = face.IsEqual(clonedFace) == false;
        
        // Log results
        testResults.Add($"Face Tests: {(test1 && test2 && test3 && test4 && test5 && test6 && test7 && test8 ? "PASSED" : "FAILED")}");
    }
}