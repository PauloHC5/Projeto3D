using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum CutsceneType
{
    INTRO,
    OUTRO,
    GAMEPLAY
}

[Serializable]
public struct CutsceneVideoFile
{
    public string fileName;
    public CutsceneType cutsceneType;
}

public class CutsceneManager : Singleton<CutsceneManager>
{
    [SerializeField] private GameObject _canvasCutscene;
    [SerializeField] public List<CutsceneVideoFile> videosFileNames;
    [Range(0.5f, 1.5f)]
    [SerializeField] private float _skipFillSpeed = 1.0f; // seconds to fill

    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private Slider _skipCutsceneSlider;

    [Header("Skip Slider Hide Settings")]
    [SerializeField] private float _skipSliderHideDelay = 3.0f; // seconds to wait before hiding slider

    private float _skipSliderInactivityTimer = 0f;

    private void Update()
    {
        if (_videoPlayer.isPlaying && !_skipCutsceneSlider.gameObject.activeSelf && Input.anyKeyDown)
        {
            _skipCutsceneSlider.gameObject.SetActive(true);
            _skipSliderInactivityTimer = 0f; // Reset timer when slider is shown
        }

        if (_skipCutsceneSlider.gameObject.activeSelf && _canvasCutscene.activeSelf)
        {
            HandleSkipCutscene();

            // Reset timer if any input is detected
            if (Input.anyKey)
            {
                _skipSliderInactivityTimer = 0f;
            }
            else
            {
                _skipSliderInactivityTimer += Time.deltaTime;
                if (_skipSliderInactivityTimer >= _skipSliderHideDelay)
                {
                    _skipCutsceneSlider.gameObject.SetActive(false);
                    _skipSliderInactivityTimer = 0f;
                }
            }
        }
    }

    private void HandleSkipCutscene()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            // Fill the slider over _skipFillSpeed seconds
            _skipCutsceneSlider.value += (_skipCutsceneSlider.maxValue / _skipFillSpeed) * Time.deltaTime;

            if (_skipCutsceneSlider.value >= _skipCutsceneSlider.maxValue)
            {
                _skipCutsceneSlider.value = _skipCutsceneSlider.maxValue;
                // Skip the cutscene
                _videoPlayer.Stop();
                _canvasCutscene.SetActive(false);
            }
        }
        else
        {
            // Reset if space is released
            _skipCutsceneSlider.value = 0f;
        }
    }

    public static IEnumerator StartCutscene(CutsceneType cutsceneType)
    {
        string videoFileName = Instance.videosFileNames.FirstOrDefault(v => v.cutsceneType == cutsceneType).fileName;

        if (string.IsNullOrEmpty(videoFileName))
        {
            Debug.LogError($"Video file name not found for cutscene: {cutsceneType} \n Please check in your inspector if you assigned the name correctly");
            yield break;
        }

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        // Reset (clear) the render texture before playing the video
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = Instance._renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = activeRT;

        Instance._canvasCutscene.SetActive(true);
        Instance._skipCutsceneSlider.gameObject.SetActive(false);
        Instance._skipSliderInactivityTimer = 0f; // Reset timer when cutscene starts

        Instance._videoPlayer.url = videoPath;
        Instance._videoPlayer.Play();

        // Wait until the video finishes playing
        yield return new WaitUntil(() => !Instance._videoPlayer.isPlaying);

        Instance._canvasCutscene.SetActive(false);
    }
}
