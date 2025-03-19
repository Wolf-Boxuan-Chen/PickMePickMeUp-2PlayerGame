using UnityEngine;
 
[System.Serializable]
public class FacePart
{
    public Sprite sprite;
    public string partName;
    [HideInInspector] public int partID; // Auto-assigned, hidden in Inspector
}