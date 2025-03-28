using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip callIncomingRingtone;
    [SerializeField] private AudioClip basicBGM_NoSticks;
    [SerializeField] private AudioClip basicBGM_WithSticks;
    [SerializeField] private AudioClip duringCallBGM;
    [SerializeField] private AudioClip successGoodCallSFX;
    [SerializeField] private AudioClip successGreatCallSFX;
    [SerializeField] private AudioClip failureSFX;
    
    [Header("Volume Settings")]
    [Range(0f, 2f)] [SerializeField] private float masterVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float musicMasterVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float sfxMasterVolume = 1.0f;
    
    [Header("Individual Clip Volumes")]
    [Range(0f, 2f)] [SerializeField] private float callIncomingVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float basicNoSticksVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float basicWithSticksVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float duringCallVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float goodCallVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float greatCallVolume = 1.0f;
    [Range(0f, 2f)] [SerializeField] private float failureVolume = 1.0f;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 2.5f; // Duration of music transition in seconds
    
    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private AudioClip currentClip;
    private float currentClipVolume;
    
    // Track successful calls for great call feature
    private int goodCallCounter = 0;
    private float greatCallChance = 0.2f; // 20% chance
    private int greatCallThreshold = 5;   // After 5 good calls
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Set up two audio sources for music crossfading
        if (musicSourceA == null)
        {
            GameObject sourceA = new GameObject("MusicSourceA");
            sourceA.transform.parent = transform;
            musicSourceA = sourceA.AddComponent<AudioSource>();
            musicSourceA.loop = true;
        }
        
        if (musicSourceB == null)
        {
            GameObject sourceB = new GameObject("MusicSourceB");
            sourceB.transform.parent = transform;
            musicSourceB = sourceB.AddComponent<AudioSource>();
            musicSourceB.loop = true;
        }
        
        // Set initial current music source
        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;
        
        // Set up SFX source
        if (sfxSource == null)
        {
            GameObject sourceSFX = new GameObject("SFXSource");
            sourceSFX.transform.parent = transform;
            sfxSource = sourceSFX.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
        
        // Start with BasicBGM for intro
        PlayStartScreenBGM();
    }
    
    // Apply volume changes in Update to catch inspector changes
    private void Update()
    {
        // Check if volume parameters changed in inspector
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            float targetVolume = CalculateMusicVolume(currentClip, currentClipVolume);
            // Only update if there's a significant difference (avoids constant updating)
            if (Mathf.Abs(currentMusicSource.volume - targetVolume) > 0.01f)
            {
                currentMusicSource.volume = targetVolume;
                Debug.Log($"Volume updated to {targetVolume} (Master: {masterVolume}, Music: {musicMasterVolume}, Clip: {currentClipVolume})");
            }
        }
    }
    
    // Start/Menu Music
    public void PlayStartScreenBGM()
    {
        PlayMusic(basicBGM_WithSticks, basicWithSticksVolume);
    }
    
    // Game Over/Credits Music
    public void PlayEndScreenBGM()
    {
        PlayMusic(basicBGM_WithSticks, basicWithSticksVolume);
    }
    
    // Play Incoming Call Ringtone
    public void PlayIncomingRingtone(bool loop = true)
    {
        currentMusicSource.loop = loop;
        PlayMusic(callIncomingRingtone, callIncomingVolume);
    }
    
    // Play During Call BGM
    public void PlayDuringCallBGM()
    {
        PlayMusic(duringCallBGM, duringCallVolume);
    }
    
    // Transition to End Call BGM (call this when showing success/failure results)
    public void TransitionToResultBGM()
    {
        StartCoroutine(CrossFadeToMusic(basicBGM_WithSticks, basicWithSticksVolume));
    }
    
    // Play Success SFX
    public void PlaySuccessSFX()
    {
        goodCallCounter++;
        
        // Check if eligible for great call
        if (goodCallCounter >= greatCallThreshold && Random.value <= greatCallChance)
        {
            PlaySFX(successGreatCallSFX, greatCallVolume);
            // Reset counter after a great call
            goodCallCounter = 0;
        }
        else
        {
            PlaySFX(successGoodCallSFX, goodCallVolume);
        }
    }
    
    // Play Failure SFX
    public void PlayFailureSFX()
    {
        PlaySFX(failureSFX, failureVolume);
    }
    
    // Stop all audio
    public void StopAll()
    {
        musicSourceA.Stop();
        musicSourceB.Stop();
        sfxSource.Stop();
    }
    
    // Helper method to calculate music volume with all multipliers
    private float CalculateMusicVolume(AudioClip clip, float baseVolume)
    {
        return baseVolume * musicMasterVolume * masterVolume;
    }
    
    // Private helper methods
    private void PlayMusic(AudioClip clip, float clipVolume)
    {
        // Stop both sources
        musicSourceA.Stop();
        musicSourceB.Stop();
        
        // Reset source selection
        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;
        
        // Store current clip info for volume updates
        currentClip = clip;
        currentClipVolume = clipVolume;
        
        // Apply all volume multipliers
        float calculatedVolume = CalculateMusicVolume(clip, clipVolume);
        
        // Play on current source
        currentMusicSource.clip = clip;
        currentMusicSource.volume = calculatedVolume;
        currentMusicSource.Play();
        
        Debug.Log($"Playing {clip.name} at volume {calculatedVolume} (Base: {clipVolume}, Music: {musicMasterVolume}, Master: {masterVolume})");
    }
    
    // Crossfade coroutine for smooth transitions
    private IEnumerator CrossFadeToMusic(AudioClip newClip, float newClipVolume)
    {
        // Store for future reference
        currentClip = newClip;
        currentClipVolume = newClipVolume;
        
        // Set up the next source with new clip
        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f; // Start at zero volume
        nextMusicSource.Play();
        
        float startVolume = currentMusicSource.volume;
        float targetVolume = CalculateMusicVolume(newClip, newClipVolume);
        float timeElapsed = 0f;
        
        while (timeElapsed < transitionDuration)
        {
            // Fade out current music
            currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / transitionDuration);
            
            // Fade in new music
            nextMusicSource.volume = Mathf.Lerp(0f, targetVolume, timeElapsed / transitionDuration);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final volumes are set correctly
        currentMusicSource.volume = 0f;
        currentMusicSource.Stop();
        nextMusicSource.volume = targetVolume;
        
        // Swap the sources for next transition
        AudioSource temp = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = temp;
        
        Debug.Log($"Completed transition to {newClip.name} at volume {targetVolume}");
    }
    
    private void PlaySFX(AudioClip clip, float clipVolume)
    {
        // Calculate final volume with all multipliers
        float finalVolume = clipVolume * sfxMasterVolume * masterVolume;
        sfxSource.PlayOneShot(clip, finalVolume);
        
        Debug.Log($"Playing SFX {clip.name} at volume {finalVolume}");
    }
    
    // Master volume control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, 2f);
        Debug.Log($"Master volume set to {masterVolume}");
        
        // No need to update here - will be caught in Update
    }
    
    // Music volume control
    public void SetMusicVolume(float volume)
    {
        musicMasterVolume = Mathf.Clamp(volume, 0f, 2f);
        Debug.Log($"Music volume set to {musicMasterVolume}");
        
        // No need to update here - will be caught in Update
    }
    
    // SFX volume control
    public void SetSFXVolume(float volume)
    {
        sfxMasterVolume = Mathf.Clamp(volume, 0f, 2f);
        Debug.Log($"SFX volume set to {sfxMasterVolume}");
    }
    
    // Getter methods
    public float GetMasterVolume() { return masterVolume; }
    public float GetMusicVolume() { return musicMasterVolume; }
    public float GetSFXVolume() { return sfxMasterVolume; }
    
    // Individual volume controls for each clip
    public void SetCallIncomingVolume(float volume)
    {
        callIncomingVolume = Mathf.Clamp(volume, 0f, 2f);
        if (currentClip == callIncomingRingtone)
            currentClipVolume = callIncomingVolume;
    }
    
    public void SetBasicNoSticksVolume(float volume)
    {
        basicNoSticksVolume = Mathf.Clamp(volume, 0f, 2f);
        if (currentClip == basicBGM_NoSticks)
            currentClipVolume = basicNoSticksVolume;
    }
    
    public void SetBasicWithSticksVolume(float volume)
    {
        basicWithSticksVolume = Mathf.Clamp(volume, 0f, 2f);
        if (currentClip == basicBGM_WithSticks)
            currentClipVolume = basicWithSticksVolume;
    }
    
    public void SetDuringCallVolume(float volume)
    {
        duringCallVolume = Mathf.Clamp(volume, 0f, 2f);
        if (currentClip == duringCallBGM)
            currentClipVolume = duringCallVolume;
    }
    
    public void SetGoodCallVolume(float volume)
    {
        goodCallVolume = Mathf.Clamp(volume, 0f, 2f);
    }
    
    public void SetGreatCallVolume(float volume)
    {
        greatCallVolume = Mathf.Clamp(volume, 0f, 2f);
    }
    
    public void SetFailureVolume(float volume)
    {
        failureVolume = Mathf.Clamp(volume, 0f, 2f);
    }
}