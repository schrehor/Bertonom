using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioData> sfxList;
    
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer;
    [SerializeField] private float fadeDuration = 0.75f;

    private AudioClip _currentMusic;
    private float _originalMusicVolume;
    private Dictionary<AudioId, AudioData> _sfxLookup;
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _originalMusicVolume = musicPlayer.volume;

        _sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null)
        {
            return;
        }

		if (pauseMusic)
		{
			musicPlayer.Pause();
            StartCoroutine(UnpauseMusic(clip.length));
		}

		sfxPlayer.PlayOneShot(clip);
    }
    
    public void PlaySfx(AudioId audioId, bool pauseMusic = false)
    {
        if (!_sfxLookup.ContainsKey(audioId))
        {
            return;
        }
        
        var audioData = _sfxLookup[audioId];
        PlaySfx(audioData.clip, pauseMusic);
    }
    
            
    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == _currentMusic)
        {
            return;
        }

        _currentMusic = clip;
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

    IEnumerator UnpauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(_originalMusicVolume, fadeDuration);
    }
}

public enum AudioId { UISelect, Hit, Faint, ExpGain, ItemObtained, PokemonObtained }

[Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
