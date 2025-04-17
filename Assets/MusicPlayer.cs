using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    AudioSource _audioSource;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    public void Play()
    {
        _audioSource.Play();
    }
}
