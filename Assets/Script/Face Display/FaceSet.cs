using UnityEngine;
using System.Collections.Generic; // Add this line

[System.Serializable]
public class FaceSet
{
    public SetPart leftPart = new SetPart();
    public SetPart rightPart = new SetPart();
    public bool isLearned = false;
    
    // Check if the parts are identical
    public bool AreFacesIdentical()
    {
        // If counts differ, not identical
        if (leftPart.features.Count != rightPart.features.Count)
            return false;
        
        // Create lookup by category
        Dictionary<string, string> leftFeatures = new Dictionary<string, string>();
        
        foreach (var feature in leftPart.features)
        {
            leftFeatures[feature.category] = feature.id;
        }
        
        // Compare with right features
        foreach (var feature in rightPart.features)
        {
            if (!leftFeatures.ContainsKey(feature.category))
                return false;
                
            if (leftFeatures[feature.category] != feature.id)
                return false;
        }
        
        return true;
    }
}