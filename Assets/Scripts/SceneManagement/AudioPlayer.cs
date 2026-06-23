using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private bool isSFX;
    [SerializeField] private bool isBGM;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (SaveSystem.save == null) return;
        if (isSFX)
            audioSource.volume = SaveSystem.save.sfxVolume;
        else if (isBGM)
            audioSource.volume = SaveSystem.save.bgmVolume;
    }
}
