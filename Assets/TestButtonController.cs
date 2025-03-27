using UnityEngine;
using UnityEngine.UI;

public class DiagnosticButton : MonoBehaviour
{
    [SerializeField] private FaceGenerator faceGenerator;
    [SerializeField] private Button button;
    
    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(RunDiagnostic);
    }
    
    private void RunDiagnostic()
    {
        if (faceGenerator != null)
        {
            faceGenerator.DiagnoseCurrentIssues();
        }
        else
        {
            Debug.LogError("FaceGenerator not assigned!");
        }
    }
}