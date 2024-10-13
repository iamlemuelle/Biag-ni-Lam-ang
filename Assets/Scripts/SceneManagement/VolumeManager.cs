  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
public class VolumeManager : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] float musicValue;
    [SerializeField] Slider musicSlider;
    [SerializeField] float sfxValue;
    [SerializeField] Slider sfxSlider;
    
    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("music");
        sfxSlider.value = PlayerPrefs.GetFloat("sfx");
    }

    void Update()
    {
        myMixer.SetFloat("music", Mathf.Log10(musicValue) * 20f);
        PlayerPrefs.SetFloat("music", musicValue);

        myMixer.SetFloat("sfx", Mathf.Log10(sfxValue) * 20f);
        PlayerPrefs.SetFloat("sfx", sfxValue);
    }
    public void SetMusicVolume(float level)
    {
        musicValue = level;
        // myMixer.SetFloat("music", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        sfxValue = level;
        // myMixer.SetFloat("sfx", Mathf.Log10(level) * 20f);
    }

    public void SetMute(bool muted)
    {
        if(muted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }
    }

}
