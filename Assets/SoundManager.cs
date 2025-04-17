using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] popSfx;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Play(string name)
    {
        if (name == "Pop") audioSource.PlayOneShot(popSfx[0]);
        else audioSource.PlayOneShot(popSfx[1]);
    }
}
