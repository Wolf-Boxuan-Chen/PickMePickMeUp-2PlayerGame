using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InputManager : MonoBehaviour
{
    public event Action OnHangUpTriggered;
    public event Action OnPickUpTriggered;
    
    public event Action OnIncomingCallRedButtonPressed;
    public event Action OnActiveCallGreenButtonPressed;
    
    [Header("Key Bindings")]
    [SerializeField] private KeyCode hangUpKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode pickUpKey = KeyCode.RightArrow;
    
    [Header("Left Monitor Button Images")]
    [SerializeField] private Image leftHangUpButtonImage;
    [SerializeField] private Image leftPickUpButtonImage;
    
    [Header("Right Monitor Button Images")]
    [SerializeField] private Image rightHangUpButtonImage;
    [SerializeField] private Image rightPickUpButtonImage;
    
    [Header("Button Sprites")]
    [SerializeField] private Sprite hangUpNormalSprite;
    [SerializeField] private Sprite hangUpPressedSprite;
    [SerializeField] private Sprite pickUpNormalSprite;
    [SerializeField] private Sprite pickUpPressedSprite;
	
    [Header("Input Settings")]
    [SerializeField] private float initialInputDelay = 0.5f; // Delay before accepting input

    // Track current game phase
    private bool inCallPhase = false;
    
    // Track button press state
    private bool hangUpPressed = false;
    private bool pickUpPressed = false;

    // Flag to enable/disable input processing
    private bool inputEnabled = false;
    
    private void Start()
    {
        // Initially disable input
        inputEnabled = false;
        
        // Enable input after delay
        StartCoroutine(EnableInputAfterDelay());
    }
	
    private System.Collections.IEnumerator EnableInputAfterDelay()
    {
        Debug.Log("Input disabled for initial delay...");
        yield return new WaitForSeconds(initialInputDelay);
        
        // Now enable input processing
        inputEnabled = true;
        Debug.Log("Input enabled after delay");
    }

    private void Update()
    {
        // Skip input processing if not yet enabled
        if (!inputEnabled) return;

        // Process hang up button in call phase
        if (inCallPhase)
        {
            if (Input.GetKeyDown(hangUpKey))
            {
                hangUpPressed = true;
                UpdateButtonSprites();
            }
            else if (Input.GetKeyUp(hangUpKey) && hangUpPressed)
            {
                hangUpPressed = false;
                UpdateButtonSprites();
                OnHangUpTriggered?.Invoke();
            }
            
            // Check for green button press during active call
            if (Input.GetKeyDown(pickUpKey)) 
            {
                Debug.Log("Green button pressed during active call");
                OnActiveCallGreenButtonPressed?.Invoke();
            }
        }
        // Process pick up button in between calls
        else
        {
            // Check for red button press during incoming call
            if (Input.GetKeyDown(hangUpKey))
            {
                Debug.Log("Red button pressed during incoming call");
                hangUpPressed = true;
                UpdateButtonSprites();
                //OnIncomingCallRedButtonPressed?.Invoke();
            }
            else if (Input.GetKeyUp(hangUpKey) && hangUpPressed)
            {
                hangUpPressed = false;
                UpdateButtonSprites();
            }

            if (Input.GetKeyDown(pickUpKey))
            {
                pickUpPressed = true;
                UpdateButtonSprites();
            }
            else if (Input.GetKeyUp(pickUpKey) && pickUpPressed)
            {
                pickUpPressed = false;
                UpdateButtonSprites();
                OnPickUpTriggered?.Invoke();
            }
        }
    }
    
    // Update the button visuals based on current state
    private void UpdateButtonSprites()
    {
        // Update left monitor button images
        if (leftHangUpButtonImage != null)
            leftHangUpButtonImage.sprite = hangUpPressed ? hangUpPressedSprite : hangUpNormalSprite;
            
        if (leftPickUpButtonImage != null)
            leftPickUpButtonImage.sprite = pickUpPressed ? pickUpPressedSprite : pickUpNormalSprite;
            
        // Update right monitor button images
        if (rightHangUpButtonImage != null)
            rightHangUpButtonImage.sprite = hangUpPressed ? hangUpPressedSprite : hangUpNormalSprite;
            
        if (rightPickUpButtonImage != null)
            rightPickUpButtonImage.sprite = pickUpPressed ? pickUpPressedSprite : pickUpNormalSprite;
    }
    
    // Set the current game phase
    public void SetCallActive(bool active)
    {
        inCallPhase = active;
        hangUpPressed = false;
        pickUpPressed = false;
        UpdateButtonSprites();
    }
	
    // Public method to enable/disable input processing
    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
        Debug.Log($"Input processing {(enable ? "enabled" : "disabled")}");
    }
}