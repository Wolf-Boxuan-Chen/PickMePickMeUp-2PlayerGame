using UnityEngine;
using UnityEngine.UI;

public class TutorialSkipBar : MonoBehaviour
{
    [SerializeField] private float requiredHoldTime = 2.0f;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject skipBarContainer;
    
    private bool isHolding = false;
    private float holdStartTime = 0f;
    
    private void Start()
    {
        // Hide skip bar initially
        skipBarContainer.SetActive(false);
    }
    
    private void Update()
    {
        // Check for hold input (space key or mouse button)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            holdStartTime = Time.time;
            isHolding = true;
            skipBarContainer.SetActive(true);
        }
        
        if (isHolding && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
        {
            // Calculate and update progress
            float holdDuration = Time.time - holdStartTime;
            float progress = Mathf.Clamp01(holdDuration / requiredHoldTime);
            progressBar.fillAmount = progress;
        }
        
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))
        {
            isHolding = false;
            skipBarContainer.SetActive(false);
            progressBar.fillAmount = 0f;
        }
    }
}