using UnityEngine;

[System.Serializable]
public class Face
{
    public FacePart faceShape;
    public FacePart eyes;
    public FacePart nose;
    public FacePart mouth;
    public FacePart hair;
    public FacePart shoulder;

    public Face Clone()
    {
        return new Face
        {
            faceShape = this.faceShape,
            eyes = this.eyes,
            nose = this.nose,
            mouth = this.mouth,
            hair = this.hair,
            shoulder = this.shoulder
        };
    }

    public bool IsEqual(Face other)
    {
        return this.faceShape.partID == other.faceShape.partID &&
               this.eyes.partID == other.eyes.partID &&
               this.nose.partID == other.nose.partID &&
               this.mouth.partID == other.mouth.partID &&
               this.hair.partID == other.hair.partID &&
               this.shoulder.partID == other.shoulder.partID;
    }
}