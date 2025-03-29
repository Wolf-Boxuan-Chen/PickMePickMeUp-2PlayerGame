using UnityEngine;
using System.Collections;

public class PhoneEffects : MonoBehaviour
{
    [Header("Vibration References")]
    [SerializeField] private RectTransform leftPhonePanel;
    [SerializeField] private RectTransform rightPhonePanel;
    
    [Header("Face Panel References")]
    [SerializeField] private RectTransform leftFacePanel;
    [SerializeField] private RectTransform rightFacePanel;
    
    [Header("Vibration Settings")]
    [SerializeField] private float vibrationIntensity = 2.0f;
    [SerializeField] private float vibrationSpeed = 80f;
    [SerializeField] private float vibrationDuration = 1.0f;
    [SerializeField] private float pauseDuration = 0.5f;
    [SerializeField] private float movementAmount = 30f;
    
    [Header("Face Panel Movement")]
    [SerializeField] private float normalBobAmount = 10f;
    [SerializeField] private float normalBobSpeed = 1f;
    [SerializeField] private float urgentBobAmount = 15f;
    [SerializeField] private float urgentBobSpeed = 2f;
    [SerializeField] private float urgentThreshold = 10f;
    
    [Header("Sound")]
    [SerializeField] private AudioSource vibrationAudioSource; // Add direct AudioSource
    [SerializeField] private AudioClip vibrationSound; // Add direct AudioClip
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Track states
    private bool isVibrating = false;
    private bool isBobbingFacePanel = false;
    private Vector3 leftPhoneOriginalPosition;
    private Vector3 rightPhoneOriginalPosition;
    private Quaternion leftPhoneOriginalRotation;
    private Quaternion rightPhoneOriginalRotation;
    private Vector3 leftFaceOriginalPosition;
    private Vector3 rightFaceOriginalPosition;
    private Vector2 currentMovementDirection;
    
    // Call timer reference
    private float currentCallTime = 20f;
    
    private void Start()
    {
        // Store original positions and rotations
        if (leftPhonePanel != null)
        {
            leftPhoneOriginalPosition = leftPhonePanel.localPosition;
            leftPhoneOriginalRotation = leftPhonePanel.localRotation;
        }
        
        if (rightPhonePanel != null)
        {
            rightPhoneOriginalPosition = rightPhonePanel.localPosition;
            rightPhoneOriginalRotation = rightPhonePanel.localRotation;
        }
        
        if (leftFacePanel != null)
        {
            leftFaceOriginalPosition = leftFacePanel.localPosition;
        }
        
        if (rightFacePanel != null)
        {
            rightFaceOriginalPosition = rightFacePanel.localPosition;
        }
        
        // Initialize random direction
        PickNewRandomDirection();
        
        // Create AudioSource if not assigned
        if (vibrationAudioSource == null)
        {
            vibrationAudioSource = gameObject.AddComponent<AudioSource>();
            vibrationAudioSource.playOnAwake = false;
            vibrationAudioSource.loop = false;
        }
    }
    
    public void SetCurrentCallTime(float time)
    {
        currentCallTime = time;
    }
    
    // Call this method to start the vibration effect
    public void StartVibrationEffect()
    {
        // Only start if not already vibrating
        if (!isVibrating)
        {
            isVibrating = true;
            LogDebug("Starting vibration effect!");
            StartCoroutine(VibratePhonePanelsWithPauses());
        }
    }
    
    // Call this to start the face panel bobbing
    public void StartFacePanelBobbing()
    {
        if (!isBobbingFacePanel)
        {
            isBobbingFacePanel = true;
            LogDebug("Starting face panel bobbing!");
            StartCoroutine(BobFacePanels());
        }
    }
    
    // For testing in the editor
    [ContextMenu("Test Vibration")]
    public void TestVibration()
    {
        LogDebug("Testing vibration from context menu");
        StartVibrationEffect();
    }
    
    [ContextMenu("Test Face Panel Bobbing")]
    public void TestFacePanelBobbing()
    {
        LogDebug("Testing face panel bobbing from context menu");
        StartFacePanelBobbing();
    }
    
    // Reset all panels to original positions
    public void ResetAllPanels()
    {
        StopAllCoroutines();
        isVibrating = false;
        isBobbingFacePanel = false;
        
        // Stop any playing vibration sound
        if (vibrationAudioSource != null && vibrationAudioSource.isPlaying)
        {
            vibrationAudioSource.Stop();
        }
        
        ResetPhonePanels();
        ResetFacePanels();
    }
    
    private void ResetPhonePanels()
    {
        if (leftPhonePanel != null)
        {
            leftPhonePanel.localPosition = leftPhoneOriginalPosition;
            leftPhonePanel.localRotation = leftPhoneOriginalRotation;
        }
        
        if (rightPhonePanel != null)
        {
            rightPhonePanel.localPosition = rightPhoneOriginalPosition;
            rightPhonePanel.localRotation = rightPhoneOriginalRotation;
        }
    }
    
    private void ResetFacePanels()
    {
        if (leftFacePanel != null)
        {
            leftFacePanel.localPosition = leftFaceOriginalPosition;
        }
        
        if (rightFacePanel != null)
        {
            rightFacePanel.localPosition = rightFaceOriginalPosition;
        }
    }
    
    private void PickNewRandomDirection()
    {
        // Generate random direction
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentMovementDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    // Play vibration sound
    private void PlayVibrationSound()
    {
        if (vibrationAudioSource != null && vibrationSound != null)
        {
            // Only play if not already playing
            if (!vibrationAudioSource.isPlaying)
            {
                vibrationAudioSource.clip = vibrationSound;
                vibrationAudioSource.Play();
                LogDebug("Playing vibration sound");
            }
        }
        else
        {
            LogDebug("WARNING: Vibration sound or audio source not assigned!");
        }
    }
    
    // Stop vibration sound
    private void StopVibrationSound()
    {
        if (vibrationAudioSource != null && vibrationAudioSource.isPlaying)
        {
            vibrationAudioSource.Stop();
            LogDebug("Stopped vibration sound");
        }
    }
    
    private IEnumerator VibratePhonePanelsWithPauses()
    {
        LogDebug("Starting vibration with pauses");
        
        // Keep vibrating until stopped externally
        while (isVibrating)
        {
            // Start vibration sound for this burst
            PlayVibrationSound();
            
            // Pick a new random direction for this vibration burst
            PickNewRandomDirection();
            
            // Vibrate for the specified duration
            float timer = 0f;
            while (timer < vibrationDuration)
            {
                timer += Time.deltaTime;
                
                // Calculate rotation based on sine wave
                float rotationAngle = Mathf.Sin(timer * vibrationSpeed) * vibrationIntensity;
                
                // Calculate movement based on current direction (reduced amount)
                Vector3 moveStep = new Vector3(
                    currentMovementDirection.x, 
                    currentMovementDirection.y, 
                    0) * movementAmount * Time.deltaTime;
                
                // Apply effects to left panel
                if (leftPhonePanel != null)
                {
                    // Apply vibration rotation
                    leftPhonePanel.localRotation = leftPhoneOriginalRotation * Quaternion.Euler(0, 0, rotationAngle);
                    
                    // Apply small movement
                    leftPhonePanel.localPosition = leftPhoneOriginalPosition + 
                        new Vector3(Mathf.Sin(timer * vibrationSpeed * 0.5f) * movementAmount * currentMovementDirection.x,
                                   Mathf.Cos(timer * vibrationSpeed * 0.5f) * movementAmount * currentMovementDirection.y,
                                   0);
                }
                
                // Apply effects to right panel
                if (rightPhonePanel != null)
                {
                    // Apply vibration rotation
                    rightPhonePanel.localRotation = rightPhoneOriginalRotation * Quaternion.Euler(0, 0, rotationAngle);
                    
                    // Apply small movement
                    rightPhonePanel.localPosition = rightPhoneOriginalPosition + 
                        new Vector3(Mathf.Sin(timer * vibrationSpeed * 0.5f) * movementAmount * currentMovementDirection.x,
                                   Mathf.Cos(timer * vibrationSpeed * 0.5f) * movementAmount * currentMovementDirection.y,
                                   0);
                }
                
                yield return null;
            }
            
            // Stop sound at end of vibration burst
            StopVibrationSound();
            
            // Reset positions between vibrations but keep rotation to create natural stop
            ResetPhonePanels();
            
            // Pause between vibrations
            LogDebug($"Pausing for {pauseDuration} seconds");
            yield return new WaitForSeconds(pauseDuration);
        }
    }
    
    private IEnumerator BobFacePanels()
    {
        LogDebug("Starting face panel bobbing");
        
        float timer = 0f;
        
        while (isBobbingFacePanel)
        {
            timer += Time.deltaTime;
            
            // Determine bobbing parameters based on call time
            float bobAmount = (currentCallTime <= urgentThreshold) ? urgentBobAmount : normalBobAmount;
            float bobSpeed = (currentCallTime <= urgentThreshold) ? urgentBobSpeed : normalBobSpeed;
            
            // Calculate bobbing position using sine wave
            float verticalOffset = Mathf.Sin(timer * bobSpeed) * bobAmount;
            
            // Apply to left face panel
            if (leftFacePanel != null)
            {
                leftFacePanel.localPosition = leftFaceOriginalPosition + new Vector3(0, verticalOffset, 0);
            }
            
            // Apply to right face panel
            if (rightFacePanel != null)
            {
                rightFacePanel.localPosition = rightFaceOriginalPosition + new Vector3(0, verticalOffset, 0);
            }
            
            yield return null;
        }
        
        // Reset positions when stopped
        ResetFacePanels();
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PhoneEffects] {message}");
        }
    }
}