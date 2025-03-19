using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceManager : MonoBehaviour
{
    [Header("Face Panels")]
    [SerializeField] private GameObject leftFacePanel;  // Parent container for left face
    [SerializeField] private GameObject rightFacePanel; // Parent container for right face
    
    [Header("Face Images (Left)")]
    [SerializeField] private Image leftFaceShape;
    [SerializeField] private Image leftEyes;
    [SerializeField] private Image leftNose;
    [SerializeField] private Image leftMouth;
    [SerializeField] private Image leftHair;
    [SerializeField] private Image leftShoulder;
    
    [Header("Face Images (Right)")]
    [SerializeField] private Image rightFaceShape;
    [SerializeField] private Image rightEyes;
    [SerializeField] private Image rightNose;
    [SerializeField] private Image rightMouth;
    [SerializeField] private Image rightHair;
    [SerializeField] private Image rightShoulder;
    
    [Header("Face Sprites")]
    [SerializeField] private List<Sprite> faceShapeSprites;
    [SerializeField] private List<Sprite> eyesSprites;
    [SerializeField] private List<Sprite> noseSprites;
    [SerializeField] private List<Sprite> mouthSprites;
    [SerializeField] private List<Sprite> hairSprites;
    [SerializeField] private List<Sprite> shoulderSprites;
    
    // Track if this is a friend or scammer
    private bool isFriend;
    
    // Add debug helper
    private int roundCount = 0;
    
    private void Start()
    {
        // Ensure faces are hidden at start
        SetFacesVisible(false);
    }
    
    // Called before the call starts - prepare faces but keep them hidden
    public void PrepareCall()
    {
        roundCount++;
        Debug.Log("Preparing call for round " + roundCount);
        
        // Hide faces
        SetFacesVisible(false);
        
        // Decide if this is a friend or scammer call
        isFriend = Random.value > 0.5f;
        Debug.Log("This call is from a " + (isFriend ? "friend" : "scammer"));
        
        // Generate face for left display
        GenerateLeftFace();
        
        // Generate face for right display (either identical or slightly different)
        if (isFriend)
        {
            // Friend call - make identical faces
            CopyLeftFaceToRight();
        }
        else
        {
            // Scammer call - make slightly different faces
            GenerateRightFaceWithDifferences();
        }
    }
    
    // Show the faces when call becomes active
    public void ShowFaces()
    {
        SetFacesVisible(true);
    }
    
    // Hide the faces when call ends
    public void HideFaces()
    {
        SetFacesVisible(false);
    }
    
    // Control visibility of both face panels
    private void SetFacesVisible(bool visible)
    {
        if (leftFacePanel != null) leftFacePanel.SetActive(visible);
        if (rightFacePanel != null) rightFacePanel.SetActive(visible);
    }
    
    // Generate random face for left display
    private void GenerateLeftFace()
    {
        if (leftFaceShape != null && faceShapeSprites.Count > 0) 
            leftFaceShape.sprite = faceShapeSprites[Random.Range(0, faceShapeSprites.Count)];
            
        if (leftEyes != null && eyesSprites.Count > 0)
            leftEyes.sprite = eyesSprites[Random.Range(0, eyesSprites.Count)];
            
        if (leftNose != null && noseSprites.Count > 0)
            leftNose.sprite = noseSprites[Random.Range(0, noseSprites.Count)];
            
        if (leftMouth != null && mouthSprites.Count > 0)
            leftMouth.sprite = mouthSprites[Random.Range(0, mouthSprites.Count)];
            
        if (leftHair != null && hairSprites.Count > 0)
            leftHair.sprite = hairSprites[Random.Range(0, hairSprites.Count)];
            
        if (leftShoulder != null && shoulderSprites.Count > 0)
            leftShoulder.sprite = shoulderSprites[Random.Range(0, shoulderSprites.Count)];
            
        Debug.Log("Generated new left face for round " + roundCount);
    }
    
    // Copy left face to right (for friend calls)
    private void CopyLeftFaceToRight()
    {
        if (rightFaceShape != null && leftFaceShape != null) 
            rightFaceShape.sprite = leftFaceShape.sprite;
            
        if (rightEyes != null && leftEyes != null)
            rightEyes.sprite = leftEyes.sprite;
            
        if (rightNose != null && leftNose != null)
            rightNose.sprite = leftNose.sprite;
            
        if (rightMouth != null && leftMouth != null)
            rightMouth.sprite = leftMouth.sprite;
            
        if (rightHair != null && leftHair != null)
            rightHair.sprite = leftHair.sprite;
            
        if (rightShoulder != null && leftShoulder != null)
            rightShoulder.sprite = leftShoulder.sprite;
            
        Debug.Log("Copied left face to right (identical faces for friend)");
    }
    
    // Generate right face with 1-2 differences (for scammer calls)
    private void GenerateRightFaceWithDifferences()
    {
        // First copy left face as a base
        CopyLeftFaceToRight();
        
        // Decide how many differences to make (1 or 2)
        int numDifferences = Random.Range(1, 3);
        Debug.Log("Creating scammer face with " + numDifferences + " differences");
        
        // Create a list of features we can modify
        List<int> features = new List<int> { 0, 1, 2, 3, 4, 5 }; // 0=shape, 1=eyes, 2=nose, etc.
        
        // Select random features to change
        for (int i = 0; i < numDifferences; i++)
        {
            if (features.Count == 0) break;
            
            int featureIndex = Random.Range(0, features.Count);
            int feature = features[featureIndex];
            features.RemoveAt(featureIndex);
            
            string featureName = "";
            
            // Change the selected feature
            switch (feature)
            {
                case 0: // Face shape
                    featureName = "face shape";
                    if (rightFaceShape != null && faceShapeSprites.Count > 1)
                    {
                        Sprite currentSprite = rightFaceShape.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = faceShapeSprites[Random.Range(0, faceShapeSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightFaceShape.sprite = newSprite;
                    }
                    break;
                    
                case 1: // Eyes
                    featureName = "eyes";
                    if (rightEyes != null && eyesSprites.Count > 1)
                    {
                        Sprite currentSprite = rightEyes.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = eyesSprites[Random.Range(0, eyesSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightEyes.sprite = newSprite;
                    }
                    break;
                    
                case 2: // Nose
                    featureName = "nose";
                    if (rightNose != null && noseSprites.Count > 1)
                    {
                        Sprite currentSprite = rightNose.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = noseSprites[Random.Range(0, noseSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightNose.sprite = newSprite;
                    }
                    break;
                    
                case 3: // Mouth
                    featureName = "mouth";
                    if (rightMouth != null && mouthSprites.Count > 1)
                    {
                        Sprite currentSprite = rightMouth.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = mouthSprites[Random.Range(0, mouthSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightMouth.sprite = newSprite;
                    }
                    break;
                    
                case 4: // Hair
                    featureName = "hair";
                    if (rightHair != null && hairSprites.Count > 1)
                    {
                        Sprite currentSprite = rightHair.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = hairSprites[Random.Range(0, hairSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightHair.sprite = newSprite;
                    }
                    break;
                    
                case 5: // Shoulder
                    featureName = "shoulder";
                    if (rightShoulder != null && shoulderSprites.Count > 1)
                    {
                        Sprite currentSprite = rightShoulder.sprite;
                        Sprite newSprite;
                        do
                        {
                            newSprite = shoulderSprites[Random.Range(0, shoulderSprites.Count)];
                        } while (newSprite == currentSprite);
                        
                        rightShoulder.sprite = newSprite;
                    }
                    break;
            }
            
            Debug.Log("Changed " + featureName + " for scammer face");
        }
    }
    
    // Return whether this is a friend call or scammer call
    public bool IsFriendCall()
    {
        return isFriend;
    }
}