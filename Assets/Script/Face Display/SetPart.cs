using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SetPart
{
    public List<FacialFeature> features = new List<FacialFeature>();
    
    // Add a feature to this part
    public void AddFeature(FacialFeature feature)
    {
        if (feature != null && !features.Contains(feature))
        {
            features.Add(feature);
        }
    }
    
    // Remove a feature from this part
    public void RemoveFeature(FacialFeature feature)
    {
        features.Remove(feature);
    }
}