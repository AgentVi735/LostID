using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseHolder;
    [SerializeField] private GameObject settingsHolder;

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool isActive;

    public void Setup()
    {
        bgmSlider.value = SaveSystem.save.bgmVolume;
        sfxSlider.value = SaveSystem.save.sfxVolume;
    }

    public void OpenPause()
    {
        isActive = !isActive;
        pauseHolder.SetActive(true);
        settingsHolder.SetActive(false);
        pauseMenu.SetActive(isActive);
    }

    public void OnContinue() => OpenPause();

    public void OnSettings(bool toggle)
    {
        if (!toggle)
            SaveSystem.Save();
        pauseHolder.SetActive(!toggle);
        settingsHolder.SetActive(toggle);
    }

    public void OnVolumeChange()
    {
        if (!isActive) return;
        SaveSystem.save.bgmVolume = bgmSlider.value;
        SaveSystem.save.sfxVolume = sfxSlider.value;
    }

    public void OnQuit() => dialogueManager.GoToMainMenu(true, false);
}
