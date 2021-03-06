﻿using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: if time left rework this script to not create an audio source for every single clip, but use an array in sound class to have multiple audio clips of same category e.g. PlayerDeathSounds, FootSteps etc.
// Contains methods to play music and sounds, change volume levels
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public AudioMixer mixer;
    public AudioMixerGroup soundMixer;
    public AudioMixerGroup musicMixer;

    public Sound[] sounds;

    public Sound[] menuMusic;
    public Sound[] gameMusic;
    public Sound[] bossMusic;

    // Only ever need 1 music clip to be playing
    private AudioSource _currentMusicClip = null;
    private MusicType _currentMusicType = MusicType.MenuMusic;

    private AudioSource _pauseMenuSource = null;
    private Dictionary<string, Sound> _soundEffects;

    private bool _isGamePaused = false;

    // Singleton, need to initialize all the sound clips and make sure the audio
    // manager doesnt get destroyed between scenes to keep music playing
    // using singleton as game should only ever have one audio manager playing sounds
    // i.e. prevents duplication of sounds.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Tried to create another instance of " + this.name);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // For sounds put them in a dictionary to improve finding them
        _soundEffects = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = soundMixer;
            _soundEffects.Add(s.name, s);
        }

        // for music it will be played randomly most of the time and very rarely by their actual name
        // i.e. should not require a dictionary to improve finding elements and would only reduce performance due to hashing
        foreach (Sound s in menuMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = musicMixer;
        }

        foreach (Sound s in gameMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = musicMixer;
        }

        foreach (Sound s in bossMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = musicMixer;
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            PlayMusic("Music1", MusicType.MenuMusic);
        else
            PlayRandomMusic(MusicType.GameMusic);
    }

    private void Update()
    {
        if(!_isGamePaused && !_currentMusicClip.isPlaying)
        {
            PlayRandomMusic(_currentMusicType);
        }
    }

    public void PlaySound(string name)
    {
        //Sound s = Array.Find(sounds, sound => sound.name == name);
        Sound s = _soundEffects[name];
        if (s == null)
        {
            Debug.LogWarning("Missing sound: " + name);
            return;
        }

        if(s.source.isPlaying)
        {
            return;
        }

        s.source.Play();
    }

    public void PauseSound(string name)
    {
        //Sound s = Array.Find(sounds, sound => sound.name == name);
        Sound s = _soundEffects[name];
        if (s == null)
        {
            Debug.LogWarning("Missing sound: " + name);
            return;
        }

        if (!s.source.isPlaying)
        {
            return;
        }

        s.source.Pause();
    }

    public void StopSound(string name)
    {
        //Sound s = Array.Find(sounds, sound => sound.name == name);
        Sound s = _soundEffects[name];
        if (s == null)
        {
            Debug.LogWarning("Missing sound: " + name);
            return;
        }

        if (!s.source.isPlaying)
        {
            return;
        }

        //s.source.Pause();
        s.source.Stop();
    }

    public void PlayOneShotSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Missing sound: " + name);
            return;
        }

        s.source.PlayOneShot(s.source.clip);
    }

    public void PlayRandomMusic(MusicType musicType)
    {
        Sound s = null;
        int randomNum = 0;
        if(musicType == MusicType.MenuMusic)
        {
            randomNum = UnityEngine.Random.Range(0, menuMusic.Length);
            s = menuMusic[randomNum];

            if (_currentMusicClip == s.source)
                s = menuMusic[(randomNum + 1) % menuMusic.Length];
        }
        else if(musicType == MusicType.GameMusic)
        {
            randomNum = UnityEngine.Random.Range(0, gameMusic.Length);
            s = gameMusic[randomNum];

            if (_currentMusicClip == s.source)
                s = gameMusic[(randomNum + 1) % gameMusic.Length];
        }
        else if(musicType == MusicType.BossMusic)
        {
            randomNum = UnityEngine.Random.Range(0, bossMusic.Length);
            s = bossMusic[randomNum];

            if (_currentMusicClip == s.source)
                s = bossMusic[(randomNum + 1) % bossMusic.Length];
        }

        if (s == null)
        {
            Debug.LogWarning("Missing music: " + name);
            return;
        }

        _currentMusicType = musicType;
        if (_currentMusicClip != null)
            _currentMusicClip.Stop();

        _currentMusicClip = s.source;
        _currentMusicClip.Play();
    }

    public void PlayMusic(string name, MusicType musicType)
    {
        Sound[] whichMusicClips;
        switch(musicType)
        {
            case MusicType.MenuMusic:
                whichMusicClips = menuMusic;
                break;
            case MusicType.GameMusic:
                whichMusicClips = gameMusic;
                break;
            case MusicType.BossMusic:
                whichMusicClips = bossMusic;
                break;
            default:
                return;
        }

        Sound s = Array.Find(whichMusicClips, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Missing music: " + name);
            return;
        }

        _currentMusicType = musicType;
        if (_currentMusicClip != null)
            _currentMusicClip.Stop();

        _currentMusicClip = s.source;
        _currentMusicClip.Play();
    }

    public void ResumeMusic()
    {
        _isGamePaused = false;
        if (_currentMusicClip != null)
        {
            _currentMusicClip.Play();
        }
    }

    public void PauseMusic()
    {
        _isGamePaused = true;
        if (_currentMusicClip != null)
        {
            _currentMusicClip.Pause();
        }
    }

    public void StopMusic()
    {
        if (_currentMusicClip != null)
            _currentMusicClip.Stop();
    }

    public void PlayOneShotMusic(string name, MusicType musicType)
    {
        Sound[] whichMusicClips;
        switch (musicType)
        {
            case MusicType.MenuMusic:
                whichMusicClips = menuMusic;
                break;
            case MusicType.GameMusic:
                whichMusicClips = gameMusic;
                break;
            case MusicType.BossMusic:
                whichMusicClips = bossMusic;
                break;
            default:
                return;
        }

        Sound s = Array.Find(whichMusicClips, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Missing music: " + name);
            return;
        }

        s.source.PlayOneShot(s.source.clip);
    }

    public void PlayPauseMenuMusic()
    {
        Sound s = null;
        int randomNum = 0;
        randomNum = UnityEngine.Random.Range(0, menuMusic.Length);
        s = menuMusic[randomNum];

        if (s == null)
        {
            Debug.LogWarning("Missing music: " + name);
            return;
        }

        _pauseMenuSource = s.source;
        _pauseMenuSource.Play();
    }

    public void StopPauseMenuMusic()
    {
        if (_pauseMenuSource == null)
            return;

        _pauseMenuSource.Stop();
    }

    public void UpdateMasterVolume(float masterVolume)
    {
        mixer.SetFloat("masterVolume", masterVolume);
    }

    public void UpdateMusicVolume(float musicVolume)
    {
        mixer.SetFloat("musicVolume", musicVolume);
    }

    public void UpdateEffectsVolume(float effectsVolume)
    {
        mixer.SetFloat("effectsVolume", effectsVolume);
    }
}

public enum MusicType
{
    MenuMusic,
    GameMusic,
    BossMusic
}
