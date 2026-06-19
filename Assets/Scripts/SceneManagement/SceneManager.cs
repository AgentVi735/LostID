using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime;

    [Header("Logo")]
    [SerializeField] private Image logo;
    private bool isInitialized;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string cafeScene;

    private void Start()
    {
        logo.gameObject.SetActive(true);
        logo.color = Color.white;
        ChangeScene(Scenes.MainMenu, Scenes.None);
    }

    public void ChangeScene(Scenes sceneToLoad, Scenes sceneToUnload) => StartCoroutine(ChangeSceneCoroutine(sceneToLoad, sceneToUnload));

    private IEnumerator ChangeSceneCoroutine(Scenes sceneToLoad, Scenes sceneToUnload)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            fadeImage.color = Color.Lerp(Color.clear, Color.black, i / fadeTime);
            yield return null;
        }

        fadeImage.color = Color.black;

        switch (sceneToUnload)
        {
            default:
            case Scenes.None:
                break;
            case Scenes.MainMenu:
                yield return SceneManager.UnloadSceneAsync(mainMenuScene);
                break;
            case Scenes.Cafe:
                yield return SceneManager.UnloadSceneAsync(cafeScene);
                break;
        }

        switch (sceneToLoad)
        {
            default:
            case Scenes.None:
                break;
            case Scenes.MainMenu:
                yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
                break;
            case Scenes.Cafe:
                yield return SceneManager.LoadSceneAsync(cafeScene, LoadSceneMode.Additive);
                break;
        }

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            fadeImage.color = Color.Lerp(Color.black, Color.clear, i / fadeTime);
            yield return null;
        }

        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false);

        if (isInitialized) yield break;
        isInitialized = true;
        logo.gameObject.SetActive(false);
    }

    private IEnumerator StartChangeScene()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            fadeImage.color = Color.Lerp(Color.clear, Color.black, i / fadeTime);
            yield return null;
        }

        fadeImage.color = Color.black;

        yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);

        Color clearWhite = new(255, 255, 255, 0);
        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float amt = i / fadeTime;
            fadeImage.color = Color.Lerp(Color.black, Color.clear, amt);
            logo.color = Color.Lerp(Color.white, clearWhite, amt);
            yield return null;
        }

        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false);

        fadeImage.color = clearWhite;
        isInitialized = true;
        logo.gameObject.SetActive(false);
    }
}
