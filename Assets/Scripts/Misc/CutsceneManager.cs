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
    [SerializeField] public List<CutsceneVideoFile> videosFileNames;

    private GameObject _canvasCutscene;
    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;

    private void Awake()
    {
        _canvasCutscene = GetComponentInChildren<Canvas>(true).gameObject;
        _videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        _renderTexture = GetComponentInChildren<RawImage>(true).texture as RenderTexture;

        if(_canvasCutscene.activeSelf) _canvasCutscene.SetActive(false);

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

        Instance._videoPlayer.url = videoPath;
        Instance._videoPlayer.Play();

        // Wait until the video finishes playing
        yield return new WaitUntil(() => !Instance._videoPlayer.isPlaying);     
        
        Instance._canvasCutscene.SetActive(false);
    }
    
}
