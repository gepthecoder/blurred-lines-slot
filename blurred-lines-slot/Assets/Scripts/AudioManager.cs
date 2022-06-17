using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance_;

    [SerializeField] private AudioMixer slot_audio_mixer_;
    [Header("SFX")]
    [Space(5)]
    [SerializeField] private AudioSource sfx_source_reel_stop_;
    [SerializeField] private AudioSource sfx_source_spin_;
    [SerializeField] private AudioSource sfx_source_win_;

    private bool is_audio_muted_ = false;

    private void Awake()
    {
        instance_ = this;
    }

    public void HandleAudio(out bool muted)
    {
        is_audio_muted_ = !is_audio_muted_;
        int decibels = is_audio_muted_ ? -80 : 0;
        slot_audio_mixer_.SetFloat("MasterVolume", decibels);

        muted = is_audio_muted_;
    }

    public void PlaySpinSound()
    {
        sfx_source_spin_.Play();
    }

    public void PlayReelStopSound()
    {
        sfx_source_reel_stop_.Play();
    }

    public void PlayWinSound(bool play)
    {
        if (play)
            sfx_source_win_.Play();
        else
            sfx_source_win_.Stop();
    }
}
