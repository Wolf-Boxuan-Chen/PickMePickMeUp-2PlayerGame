using UnityEngine;
using System.Collections.Generic; // Add this line

[System.Serializable]
public class FaceSet
{
    public SetPart leftPart = new SetPart();
    public SetPart rightPart = new SetPart();
    public bool isLearned = false;
    
     // Update FaceSet.AreFacesIdentical to be more robust
    public bool AreFacesIdentical()
    {
        // Compare features by category - now more accurate
        Dictionary<string, string> leftFeatureIds = new Dictionary<string, string>();
        Dictionary<string, string> rightFeatureIds = new Dictionary<string, string>();
        
        foreach (FacialFeature feature in leftPart.features)
        {
            leftFeatureIds[feature.category] = feature.id;
        }
        
        foreach (FacialFeature feature in rightPart.features)
        {
            rightFeatureIds[feature.category] = feature.id;
        }
        
        // Check if all categories match
        foreach (string category in FaceDatabase.Instance.FeatureCategories)
        {
            bool leftHasFeature = leftFeatureIds.ContainsKey(category);
            bool rightHasFeature = rightFeatureIds.ContainsKey(category);
            
            // If one has the feature but the other doesn't
            if (leftHasFeature != rightHasFeature)
                return false;
                
            // If both have the feature, compare IDs
            if (leftHasFeature && rightHasFeature)
            {
                if (leftFeatureIds[category] != rightFeatureIds[category])
                    return false;
            }
        }
        
        return true;
    }
}