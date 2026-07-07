using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private float soundThreshold = 0.05f;
    private float lastSoundValue;

    [Header("Audio Channels")]
    [SerializeField] private AudioSource sliderClickSource; // SliderTick_Channel
    [SerializeField] private AudioSource musicAudioSource;  // Music_Channel
    [SerializeField] private AudioSource clickSfxSource;    // ClickSFX_Channel
    [SerializeField] private AudioSource forwardSfxSource;  // ForwardSFX_Channel (New!)
    [SerializeField] private AudioSource backwardSfxSource; // BackwardSFX_Channel (New!)

    public void Initialize()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0.0001f;
            volumeSlider.maxValue = 1f;
            lastSoundValue = volumeSlider.value;
        }

        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    public void PlayClickSound()
    {
        if (clickSfxSource != null)
        {
            clickSfxSource.pitch = Random.Range(0.95f, 1.05f);
            clickSfxSource.Play();
        }
    }

    // New: Play sound when opening a sub-menu or going forward
    public void PlayForwardSound()
    {
        if (forwardSfxSource != null)
        {
            forwardSfxSource.pitch = Random.Range(0.98f, 1.02f);
            forwardSfxSource.Play();
        }
    }

    // New: Play sound when closing a sub-menu or going back
    public void PlayBackwardSound()
    {
        if (backwardSfxSource != null)
        {
            backwardSfxSource.pitch = Random.Range(0.95f, 1.05f);
            backwardSfxSource.Play();
        }
    }

    public void SetVolume(float sliderValue)
    {
        if (myMixer != null)
        {
            float decibelValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
            myMixer.SetFloat("Volume", decibelValue);
        }

        if (Mathf.Abs(sliderValue - lastSoundValue) >= soundThreshold)
        {
            PlaySliderTickSound();
            lastSoundValue = sliderValue;
        }
    }

    public void PlayOnFirstClick()
    {
        if (volumeSlider != null) lastSoundValue = volumeSlider.value;
        PlaySliderTickSound();
    }

    private void PlaySliderTickSound()
    {
        if (sliderClickSource != null)
        {
            sliderClickSource.pitch = Random.Range(0.95f, 1.05f);
            sliderClickSource.Play();
        }
    }
}