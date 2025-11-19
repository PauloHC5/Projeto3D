using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] private GameObject _canvasFade;
    public float _fadeDuration = 1.0f; // Duration of the fade effect in seconds

    private Image _fadeImage;

    private event Action OnFadeInComplete;
    private event Action OnFadeOutComplete;

    private void Awake()
    {
        OnAwake();
        
        _fadeImage = _canvasFade.GetComponentInChildren<Image>(true);
    }

    private void Start()
    {
        _canvasFade.SetActive(false); // Ensure the fade canvas is inactive at start
    }

    public static void FadeIn(Action onFadeInComplete)
    {
        // Unsubscribe first to avoid duplicate subscriptions if needed
        Instance.OnFadeInComplete -= onFadeInComplete;
        Instance.OnFadeInComplete += onFadeInComplete;

        if (!Instance._canvasFade.gameObject.activeSelf) Instance._canvasFade.gameObject.SetActive(true);

        Instance._fadeImage.color = new Color(Instance._fadeImage.color.r, Instance._fadeImage.color.g, Instance._fadeImage.color.b, 1.0f);
        Instance.StartCoroutine(Instance.Fade(0.0f, true)); // Fixed: Use Instance.StartCoroutine
    }

    public static void FadeOut(Action onFadeOutComplete)
    {
        // Unsubscribe first to avoid duplicate subscriptions if needed
        Instance.OnFadeOutComplete -= onFadeOutComplete;
        Instance.OnFadeOutComplete += onFadeOutComplete;

        if (!Instance._canvasFade.gameObject.activeSelf) Instance._canvasFade.gameObject.SetActive(true);

        Instance._fadeImage.color = new Color(Instance._fadeImage.color.r, Instance._fadeImage.color.g, Instance._fadeImage.color.b, 0.0f);
        Instance.StartCoroutine(Instance.Fade(1.0f, false)); // Fixed: Use Instance.StartCoroutine
    }

    private IEnumerator Fade(float targetAlpha, bool isFadeIn)
    {
        float startAlpha = _fadeImage.color.a;
        float elapsedTime = 0.0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / _fadeDuration);
            _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, newAlpha);
            yield return null;
        }
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, targetAlpha);

        if (isFadeIn)
        {
            OnFadeInComplete?.Invoke();

            // Unsubscribe to avoid memory leaks
            OnFadeInComplete = null;

            // Hide the fade canvas after fade-in is complete
            _canvasFade.SetActive(false);
        }
        else
        {
            OnFadeOutComplete?.Invoke();
            // Unsubscribe to avoid memory leaks
            OnFadeOutComplete = null;
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _canvasFade.SetActive(false); // Hide the fade canvas when a new level is loaded
    }
}
