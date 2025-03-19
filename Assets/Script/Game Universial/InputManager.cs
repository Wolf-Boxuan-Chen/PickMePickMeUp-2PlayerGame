using System;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public event Action OnHangUpTriggered;
    public event Action OnPickUpTriggered;
    
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
    
    // Track current game phase
    private bool inCallPhase = false;
    
    // Track button press state
    private bool hangUpPressed = false;
    private bool pickUpPressed = false;
    
    private void Update()
    {
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
        }
        // Process pick up button in between calls
        else
        {
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
}