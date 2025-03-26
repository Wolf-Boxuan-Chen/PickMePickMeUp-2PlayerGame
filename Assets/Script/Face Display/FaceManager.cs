using UnityEngine;
using UnityEngine.UI;

public class FaceManager : MonoBehaviour
{
    [Header("Face Panels")]
    [SerializeField] private GameObject leftFacePanel;
    [SerializeField] private GameObject rightFacePanel;
    
    [Header("Face Image Components (Left)")]
    [SerializeField] private Image leftFaceShape;
    [SerializeField] private Image leftEyes;
    [SerializeField] private Image leftNose;
    [SerializeField] private Image leftMouth;
    [SerializeField] private Image leftFrontHair;
    [SerializeField] private Image leftBackHair;
    [SerializeField] private Image leftEar;
    [SerializeField] private Image leftShoulder;
    [SerializeField] private Image leftBackground;
    [SerializeField] private Image leftPhoneCase;
    
    [Header("Face Image Components (Right)")]
    [SerializeField] private Image rightFaceShape;
    [SerializeField] private Image rightEyes;
    [SerializeField] private Image rightNose;
    [SerializeField] private Image rightMouth;
    [SerializeField] private Image rightFrontHair;
    [SerializeField] private Image rightBackHair;
    [SerializeField] private Image rightEar;
    [SerializeField] private Image rightShoulder;
    [SerializeField] private Image rightBackground;
    [SerializeField] private Image rightPhoneCase;
    
    // Current faces
    private Face leftFace;
    private Face rightFace;
    private bool facesAreIdentical;
    
    // Internal round counter for debugging
    private int roundCount = 0;
    
    // Set left face
    public void SetLeftFace(Face face)
    {
        leftFace = face;
        UpdateLeftFaceVisuals();
    }
    
    // Set right face
    public void SetRightFace(Face face)
    {
        rightFace = face;
        UpdateRightFaceVisuals();
    }
    
    // Set whether faces are identical
    public void SetFacesIdentical(bool identical)
    {
        facesAreIdentical = identical;
        Debug.Log($"Faces are {(identical ? "identical" : "different")}");
    }
    
    // Update visuals for left face
    private void UpdateLeftFaceVisuals()
    {
        if (leftFace == null) return;
        
        SetSpriteIfExists(leftFaceShape, leftFace.faceShape?.sprite);
        SetSpriteIfExists(leftEyes, leftFace.eyes?.sprite);
        SetSpriteIfExists(leftNose, leftFace.nose?.sprite);
        SetSpriteIfExists(leftMouth, leftFace.mouth?.sprite);
        SetSpriteIfExists(leftFrontHair, leftFace.frontHair?.sprite);
        SetSpriteIfExists(leftBackHair, leftFace.backHair?.sprite);
        SetSpriteIfExists(leftEar, leftFace.ear?.sprite);
        SetSpriteIfExists(leftShoulder, leftFace.shoulder?.sprite);
        SetSpriteIfExists(leftBackground, leftFace.background?.sprite);
        SetSpriteIfExists(leftPhoneCase, leftFace.phoneCase?.sprite);
    }
    
    // Update visuals for right face
    private void UpdateRightFaceVisuals()
    {
        if (rightFace == null) return;
        
        SetSpriteIfExists(rightFaceShape, rightFace.faceShape?.sprite);
        SetSpriteIfExists(rightEyes, rightFace.eyes?.sprite);
        SetSpriteIfExists(rightNose, rightFace.nose?.sprite);
        SetSpriteIfExists(rightMouth, rightFace.mouth?.sprite);
        SetSpriteIfExists(rightFrontHair, rightFace.frontHair?.sprite);
        SetSpriteIfExists(rightBackHair, rightFace.backHair?.sprite);
        SetSpriteIfExists(rightEar, rightFace.ear?.sprite);
        SetSpriteIfExists(rightShoulder, rightFace.shoulder?.sprite);
        SetSpriteIfExists(rightBackground, rightFace.background?.sprite);
        SetSpriteIfExists(rightPhoneCase, rightFace.phoneCase?.sprite);
    }
    
    // Helper to set sprite if image exists
    private void SetSpriteIfExists(Image image, Sprite sprite)
    {
        if (image != null)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
                image.enabled = true;
            }
            else
            {
                image.enabled = false;
            }
        }
    }
    
    // Return whether faces are identical (for game logic)
    public bool IsFriendCall()
    {
        return facesAreIdentical;
    }
    
    // Called before the call starts - now just a wrapper for the game manager
    public void PrepareCall()
    {
        roundCount++;
        Debug.Log("Preparing call for round " + roundCount);
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
}