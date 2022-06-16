using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance_;

    [SerializeField] private AudioMixer slot_audio_mixer_;

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
}
