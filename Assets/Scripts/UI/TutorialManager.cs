using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum WeaponTutorialType
{
    CARNIVOROUSPLANT,
    ACORN,  
    BANANASHOTGUN,
    CACTUSCROSSBOW,

    NONE // Default value, used when no tutorial is selected
}

[Serializable]
public struct VideoFile
{
    public string fileName;
    public WeaponTutorialType weaponTutorialType;
}

[Serializable]
public struct WeaponTextTutorial
{
    [HideInInspector] public string name;
    public WeaponTutorialType tutorialType;
    public GameObject textGameObject;
}

[ExecuteInEditMode]
public class TutorialManager : Singleton<TutorialManager>   
{    

    [Header("Tutorial Properties")]
    [SerializeField] private GameObject _canvasTutorial;    

    [SerializeField] private VideoFile[] _videosFileNames;

    [SerializeField] private WeaponTextTutorial[] _weaponsTexts;

    [SerializeField] private GameObject _closeButtom;

    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;
    private WeaponTutorialType _currentWeaponTutorial;

    private static Dictionary<WeaponTutorialType, bool> _tutorialCompleted = new Dictionary<WeaponTutorialType, bool>
    {
        { WeaponTutorialType.CARNIVOROUSPLANT, false },
        { WeaponTutorialType.ACORN, false },
        { WeaponTutorialType.BANANASHOTGUN, false },
        { WeaponTutorialType.CACTUSCROSSBOW, false }
    };


    private void Start()
    {
        if(Application.isPlaying)
        {            
            if (_canvasTutorial == null) Debug.LogError("CanvasTutorial is not assigned in the TutorialManager.");

            if (_canvasTutorial.activeSelf)
            {
                _canvasTutorial.SetActive(false);
            }

            _videoPlayer = _canvasTutorial.GetComponentInChildren<VideoPlayer>(true);
            if (_videoPlayer == null)
            {
                Debug.LogError("VideoPlayer component not found in the TutorialManager's canvas.");
                return;
            }

            _renderTexture = _canvasTutorial.GetComponentInChildren<RawImage>(true).texture as RenderTexture;
            if (_renderTexture == null)
            {
                Debug.LogError("RenderTexture component not found in the TutorialManager's canvas.");
                return;
            }
        }        
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        var names = Enum.GetNames(typeof(WeaponTutorialType));

        Array.Resize(ref _videosFileNames, names.Length);
        for (int i = 0; i < _videosFileNames.Length; i++)
        {
            if (i < names.Length)
            {
                _videosFileNames[i].weaponTutorialType = (WeaponTutorialType)i;                
            }
            else
            {
                Debug.LogWarning($"VideoFile at index {i} does not have a corresponding WeaponTutorialType. Please check your configuration.");
            }
        }


        Array.Resize(ref _weaponsTexts, names.Length);
        for (int i = 0; i < _weaponsTexts.Length; i++)
        {
            if (i < names.Length)
            {
                _weaponsTexts[i].name = names[i];
                _weaponsTexts[i].tutorialType = (WeaponTutorialType)i;
            }
            else
            {
                Debug.LogWarning($"WeaponTextTutorial at index {i} does not have a corresponding WeaponTutorialType. Please check your configuration.");
            }
        }
    }
#endif


    public static void PlayTutorial(WeaponTutorialType weaponTutorial)
    {
        if (_tutorialCompleted[weaponTutorial]) return;

        if (Instance._videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned in the TutorialManager.");
            return;
        }

        Instance._canvasTutorial.SetActive(true);
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.UI);
        Time.timeScale = 0f;

        Instance.PlayWeaponVideoTutorial(weaponTutorial);
        Instance.ShowWeaponTutorialText(weaponTutorial);
        Instance._currentWeaponTutorial = weaponTutorial;
    }

    private void PlayWeaponVideoTutorial(WeaponTutorialType weaponTutorial)
    {
        string videoFileName = _videosFileNames.FirstOrDefault(v => v.weaponTutorialType == weaponTutorial).fileName;

        if (string.IsNullOrEmpty(videoFileName))
        {
            Debug.LogError($"Video file name not found for weapon tutorial: {weaponTutorial} \n Please check in your inspector if you assigned the name correctly");
            return; 
        }

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);

        // Reset (clear) the render texture before playing the video
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = Instance._renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = activeRT;

        _videoPlayer.url = videoPath;
        _videoPlayer.Play();
    }

    private void ShowWeaponTutorialText(WeaponTutorialType weaponTutorialType)
    {
        for (int i = 0; i < Instance._weaponsTexts.Length; i++)
        {
            if (Instance._weaponsTexts[i].tutorialType == WeaponTutorialType.NONE)
                continue;

            if (Instance._weaponsTexts[i].tutorialType == weaponTutorialType)
            {
                Instance._weaponsTexts[i].textGameObject.SetActive(true);
            }
            else
            {
                Instance._weaponsTexts[i].textGameObject.SetActive(false);
            }
        }
    }    

    public void ExitTutorial()
    {
        _videoPlayer.Stop();
        PlayerCharacterController.SwitchPlayerControlType(PlayerControlTypes.GAMEPLAY);
        Time.timeScale = 1f;
        _tutorialCompleted[_currentWeaponTutorial] = true; // Mark the tutorial as completed
        gameObject.SetActive(false);        
    }
}
