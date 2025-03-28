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
    
    // Additional sources for amplification
    private AudioSource musicAmplifierA;
    private AudioSource musicAmplifierB;
    private AudioSource sfxAmplifier;
    
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
    [SerializeField] private float transitionDuration = 2.0f; // Duration of music transition in seconds
    
    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private AudioSource currentMusicAmplifier;
    private AudioSource nextMusicAmplifier;
    
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
        // Set up music sources if not assigned
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
        
        // Set up SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sourceSFX = new GameObject("SFXSource");
            sourceSFX.transform.parent = transform;
            sfxSource = sourceSFX.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
        
        // Create amplifier sources
        GameObject ampA = new GameObject("MusicAmplifierA");
        ampA.transform.parent = transform;
        musicAmplifierA = ampA.AddComponent<AudioSource>();
        musicAmplifierA.loop = true;
        
        GameObject ampB = new GameObject("MusicAmplifierB");
        ampB.transform.parent = transform;
        musicAmplifierB = ampB.AddComponent<AudioSource>();
        musicAmplifierB.loop = true;
        
        GameObject sfxAmp = new GameObject("SFXAmplifier");
        sfxAmp.transform.parent = transform;
        sfxAmplifier = sfxAmp.AddComponent<AudioSource>();
        sfxAmplifier.loop = false;
        
        // Set initial current music source
        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;
        currentMusicAmplifier = musicAmplifierA;
        nextMusicAmplifier = musicAmplifierB;
        
        // Start with BasicBGM for intro
        PlayStartScreenBGM();
    }
    
    // Apply volume changes in Update
    private void Update()
    {
        // Update music volume if playing
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            ApplyAmplifiedVolume(currentMusicSource, currentMusicAmplifier, 
                                CalculateMusicVolume(currentClip, currentClipVolume));
        }
        
        // Update next music source if in transition
        if (nextMusicSource != null && nextMusicSource.isPlaying)
        {
            // Only update if volume > 0 (in transition)
            if (nextMusicSource.volume > 0)
            {
                float currentVol = nextMusicSource.volume;
                ApplyAmplifiedVolume(nextMusicSource, nextMusicAmplifier, 
                                    CalculateMusicVolume(nextMusicSource.clip, currentClipVolume) * 
                                    (currentVol > 0 ? currentVol : 1));
            }
        }
    }
    
    // Helper method to apply amplified volume
    private void ApplyAmplifiedVolume(AudioSource mainSource, AudioSource amplifier, float targetVolume)
    {
        if (targetVolume <= 1.0f)
        {
            // Normal volume range
            mainSource.volume = targetVolume;
            
            // Disable amplifier
            if (amplifier.isPlaying)
            {
                amplifier.Stop();
            }
        }
        else
        {
            // Set main source to maximum
            mainSource.volume = 1.0f;
            
            // Setup amplifier
            amplifier.clip = mainSource.clip;
            amplifier.time = mainSource.time; // Sync playback position
            amplifier.volume = targetVolume - 1.0f; // Additional volume
            
            // Ensure amplifier is playing in sync
            if (!amplifier.isPlaying)
            {
                amplifier.Play();
            }
        }
    }
    
    // Calculate music volume with all multipliers
    private float CalculateMusicVolume(AudioClip clip, float baseVolume)
    {
        return baseVolume * musicMasterVolume * masterVolume;
    }
    
    // Calculate SFX volume with all multipliers
    private float CalculateSFXVolume(AudioClip clip, float baseVolume)
    {
        return baseVolume * sfxMasterVolume * masterVolume;
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
        // Stop main sources
        musicSourceA.Stop();
        musicSourceB.Stop();
        sfxSource.Stop();
        
        // Stop amplifiers
        musicAmplifierA.Stop();
        musicAmplifierB.Stop();
        sfxAmplifier.Stop();
    }
    
    // Play music with volume control
    private void PlayMusic(AudioClip clip, float clipVolume)
    {
        // Stop all music sources
        musicSourceA.Stop();
        musicSourceB.Stop();
        musicAmplifierA.Stop();
        musicAmplifierB.Stop();
        
        // Reset source selection
        currentMusicSource = musicSourceA;
        nextMusicSource = musicSourceB;
        currentMusicAmplifier = musicAmplifierA;
        nextMusicAmplifier = musicAmplifierB;
        
        // Store current clip info for volume updates
        currentClip = clip;
        currentClipVolume = clipVolume;
        
        // Calculate volume
        float calculatedVolume = CalculateMusicVolume(clip, clipVolume);
        
        // Set clip to main source
        currentMusicSource.clip = clip;
        
        // Apply amplified volume
        ApplyAmplifiedVolume(currentMusicSource, currentMusicAmplifier, calculatedVolume);
        
        // Play main source
        currentMusicSource.Play();
        
        Debug.Log($"Playing {clip.name} at volume {calculatedVolume}");
    }
    
    // Crossfade coroutine with amplification support
    private IEnumerator CrossFadeToMusic(AudioClip newClip, float newClipVolume)
    {
        // Store for future reference
        currentClip = newClip;
        currentClipVolume = newClipVolume;
        
        // Set up the next sources
        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicAmplifier.clip = newClip;
        nextMusicAmplifier.volume = 0f;
        
        // Start playing
        nextMusicSource.Play();
        
        float startVolume = currentMusicSource.volume + (currentMusicAmplifier.isPlaying ? currentMusicAmplifier.volume : 0f);
        float targetVolume = CalculateMusicVolume(newClip, newClipVolume);
        float timeElapsed = 0f;
        
        while (timeElapsed < transitionDuration)
        {
            // Calculate current progress
            float t = timeElapsed / transitionDuration;
            
            // Calculate current transition volumes
            float oldVol = Mathf.Lerp(startVolume, 0f, t);
            float newVol = Mathf.Lerp(0f, targetVolume, t);
            
            // Apply volumes with amplification
            if (oldVol <= 1.0f)
            {
                currentMusicSource.volume = oldVol;
                currentMusicAmplifier.volume = 0;
            }
            else
            {
                currentMusicSource.volume = 1.0f;
                currentMusicAmplifier.volume = oldVol - 1.0f;
            }
            
            // Apply new volumes with amplification
            if (newVol <= 1.0f)
            {
                nextMusicSource.volume = newVol;
                nextMusicAmplifier.volume = 0;
            }
            else
            {
                nextMusicSource.volume = 1.0f;
                nextMusicAmplifier.volume = newVol - 1.0f;
                
                // Ensure amplifier is playing
                if (!nextMusicAmplifier.isPlaying)
                {
                    nextMusicAmplifier.clip = newClip;
                    nextMusicAmplifier.time = nextMusicSource.time;
                    nextMusicAmplifier.Play();
                }
            }
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final volumes are set correctly
        currentMusicSource.volume = 0f;
        currentMusicAmplifier.volume = 0f;
        currentMusicSource.Stop();
        currentMusicAmplifier.Stop();
        
        // Apply final volume to new source
        ApplyAmplifiedVolume(nextMusicSource, nextMusicAmplifier, targetVolume);
        
        // Swap the sources for next transition
        AudioSource tempSource = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = tempSource;
        
        AudioSource tempAmp = currentMusicAmplifier;
        currentMusicAmplifier = nextMusicAmplifier;
        nextMusicAmplifier = tempAmp;
        
        Debug.Log($"Completed transition to {newClip.name} at volume {targetVolume}");
    }
    
    // Play SFX with amplification
    private void PlaySFX(AudioClip clip, float clipVolume)
    {
        float calculatedVolume = CalculateSFXVolume(clip, clipVolume);
        
        if (calculatedVolume <= 1.0f)
        {
            // Standard volume - use normal PlayOneShot
            sfxSource.PlayOneShot(clip, calculatedVolume);
        }
        else
        {
            // Need amplification
            sfxSource.PlayOneShot(clip, 1.0f);
            
            // Use amplifier for additional volume
            sfxAmplifier.clip = clip;
            sfxAmplifier.volume = calculatedVolume - 1.0f;
            sfxAmplifier.Play();
        }
        
        Debug.Log($"Playing SFX {clip.name} at volume {calculatedVolume}");
    }
    
    // Master volume control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, 2f);
        Debug.Log($"Master volume set to {masterVolume}");
    }
    
    // Music volume control
    public void SetMusicVolume(float volume)
    {
        musicMasterVolume = Mathf.Clamp(volume, 0f, 2f);
        Debug.Log($"Music volume set to {musicMasterVolume}");
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
    
    // Individual volume controls
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
    
    // Method to manually test amplification
    [ContextMenu("Test Amplification")]
    private void TestAmplification()
    {
        // Toggle between normal and high volume
        if (masterVolume <= 1.0f)
        {
            SetMasterVolume(2.0f);
        }
        else
        {
            SetMasterVolume(0.5f);
        }
        
        Debug.Log($"TEST: Set master volume to {masterVolume}");
    }
}