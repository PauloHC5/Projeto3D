using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeBehaviour : MonoBehaviour
{
    public float _fadeDuration = 1.0f; // Duration of the fade effect in seconds

    private Image _fadeImage;

    public event Action OnFadeInComplete;
    public event Action OnFadeOutComplete;

    private void Awake()
    {
        _fadeImage = GetComponent<Image>();
    }    

    public void FadeIn()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1.0f);
        StartCoroutine(Fade(0.0f, true));
    }

    public void FadeOut()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0.0f);
        StartCoroutine(Fade(1.0f, false));
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
            OnFadeInComplete?.Invoke();
        else
            OnFadeOutComplete?.Invoke();
    }
}
