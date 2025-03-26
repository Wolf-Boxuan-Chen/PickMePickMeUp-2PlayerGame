using UnityEngine;

[System.Serializable]
public class FacialFeature
{
    public string id;
    public string category;
    public string partName;
    public Sprite sprite;
    public bool isLearned = false;
    
    // Constructor for easier creation
    public FacialFeature(string id, string category, string partName, Sprite sprite, bool isLearned = false)
    {
        this.id = id;
        this.category = category;
        this.partName = partName;
        this.sprite = sprite;
        this.isLearned = isLearned;
    }
}