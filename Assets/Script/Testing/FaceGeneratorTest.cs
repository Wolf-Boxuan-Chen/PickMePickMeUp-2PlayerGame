using UnityEngine;
using System.Collections;

public class FaceGeneratorTest : MonoBehaviour
{
    [SerializeField] private FaceGenerator faceGenerator;
    [SerializeField] private FaceManager faceManager;
    [SerializeField] private FaceDatabase faceDatabase;
    
    [Header("Test Settings")]
    [SerializeField] private bool autoTest = true;
    [SerializeField] private float delayBetweenTests = 2f;
    [SerializeField] private int testRounds = 10;
    
    private int currentRound = 0;
    
    private void Start()
    {
        if (faceGenerator == null)
            faceGenerator = FindFirstObjectByType<FaceGenerator>();
            
        if (faceManager == null)
            faceManager = FindFirstObjectByType<FaceManager>();
            
        if (faceDatabase == null)
            faceDatabase = FindFirstObjectByType<FaceDatabase>();
            
        // Ensure we have the components
        if (faceGenerator == null || faceManager == null || faceDatabase == null)
        {
            Debug.LogError("Missing required components! Make sure FaceGenerator, FaceManager, and FaceDatabase exist in the scene.");
            return;
        }
        
        // Initialize the generator
        faceGenerator.Initialize();
        
        // Start auto testing if enabled
        if (autoTest)
        {
            StartCoroutine(RunAutoTests());
        }
    }
    
    private IEnumerator RunAutoTests()
    {
        Debug.Log("=== Starting Face Generator Tests ===");
        
        // Wait a moment to let things initialize
        yield return new WaitForSeconds(1.0f);
        
        // Run multiple generation tests
        for (int i = 0; i < testRounds; i++)
        {
            currentRound = i + 1;
            Debug.Log($"Test Round {currentRound} / {testRounds}");
            
            // Generate new faces
            faceGenerator.GenerateNextRoundFaces();
            
            // Show the faces
            faceManager.ShowFaces();
            
            // Record if they're identical
            bool areIdentical = faceManager.IsFriendCall();
            Debug.Log($"Round {currentRound}: Faces are {(areIdentical ? "identical" : "different")}");
            
            // Simulate round completion (randomly succeed or fail)
            bool success = Random.value > 0.5f;
            faceGenerator.OnRoundCompleted(success);
            Debug.Log($"Round {currentRound}: Simulated {(success ? "success" : "failure")}");
            
            // Wait for next test
            yield return new WaitForSeconds(delayBetweenTests);
            
            // Hide faces before next round
            faceManager.HideFaces();
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("=== Face Generator Tests Complete ===");
    }
    
    // Manual testing functions
    public void GenerateNewFaces()
    {
        currentRound++;
        Debug.Log($"Manual Test Round {currentRound}");
        
        faceGenerator.GenerateNextRoundFaces();
        faceManager.ShowFaces();
        
        bool areIdentical = faceManager.IsFriendCall();
        Debug.Log($"Round {currentRound}: Faces are {(areIdentical ? "identical" : "different")}");
    }
    
    public void CompleteRoundSuccess()
    {
        faceGenerator.OnRoundCompleted(true);
        Debug.Log($"Round {currentRound}: Completed with success");
    }
    
    public void CompleteRoundFailure()
    {
        faceGenerator.OnRoundCompleted(false);
        Debug.Log($"Round {currentRound}: Completed with failure");
    }
}