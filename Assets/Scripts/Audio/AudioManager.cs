using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer;
    [SerializeField] private float fadeDuration = 0.75f;

    private float _originalMusicVolume;
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _originalMusicVolume = musicPlayer.volume;
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null)
        {
            return;
        }
        
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }
        
        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
        
        if (fade)
        {
            yield return musicPlayer.DOFade(_originalMusicVolume, fadeDuration).WaitForCompletion();
        }
    }
}
