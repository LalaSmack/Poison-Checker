using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayClick()
    {
        if (clickClip == null) return;
        sfxSource.PlayOneShot(clickClip);
    }
}