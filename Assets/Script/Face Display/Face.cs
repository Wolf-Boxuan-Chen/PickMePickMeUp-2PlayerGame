using UnityEngine;

[System.Serializable]
public class Face
{
    // Facial features by category
    public FacialFeature faceShape;
    public FacialFeature eyes;
    public FacialFeature nose;
    public FacialFeature mouth;
    public FacialFeature frontHair;
    public FacialFeature backHair;
    public FacialFeature ear;
    public FacialFeature shoulder;
    public FacialFeature background;
    public FacialFeature phoneCase;
    
    // Clone the face
    public Face Clone()
    {
        Face clone = new Face();
        clone.faceShape = this.faceShape;
        clone.eyes = this.eyes;
        clone.nose = this.nose;
        clone.mouth = this.mouth;
        clone.frontHair = this.frontHair;
        clone.backHair = this.backHair;
        clone.ear = this.ear;
        clone.shoulder = this.shoulder;
        clone.background = this.background;
        clone.phoneCase = this.phoneCase;
        
        return clone;
    }
    
    // Check if this face is identical to another
    public bool IsEqual(Face other)
    {
        if (other == null) return false;
        
        return AreEqual(faceShape, other.faceShape) &&
               AreEqual(eyes, other.eyes) &&
               AreEqual(nose, other.nose) &&
               AreEqual(mouth, other.mouth) &&
               AreEqual(frontHair, other.frontHair) &&
               AreEqual(backHair, other.backHair) &&
               AreEqual(ear, other.ear) &&
               AreEqual(shoulder, other.shoulder) &&
               AreEqual(background, other.background) &&
               AreEqual(phoneCase, other.phoneCase);
    }
    
    // Helper to compare features, handling nulls
    private bool AreEqual(FacialFeature a, FacialFeature b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.id == b.id;
    }
    
    // Set a feature by category
    public void SetFeature(string category, FacialFeature feature)
    {
        switch (category)
        {
            case "FaceShape": faceShape = feature; break;
            case "Eye": eyes = feature; break;
            case "Nose": nose = feature; break;
            case "Mouth": mouth = feature; break;
            case "FrontHair": frontHair = feature; break;
            case "BackHair": backHair = feature; break;
            case "Ear": ear = feature; break;
            case "Shoulder": shoulder = feature; break;
            case "Background": background = feature; break;
            case "PhoneCase": phoneCase = feature; break;
        }
    }
    
    // Get a feature by category
    public FacialFeature GetFeature(string category)
    {
        switch (category)
        {
            case "FaceShape": return faceShape;
            case "Eye": return eyes;
            case "Nose": return nose;
            case "Mouth": return mouth;
            case "FrontHair": return frontHair;
            case "BackHair": return backHair;
            case "Ear": return ear;
            case "Shoulder": return shoulder;
            case "Background": return background;
            case "PhoneCase": return phoneCase;
            default: return null;
        }
    }
}