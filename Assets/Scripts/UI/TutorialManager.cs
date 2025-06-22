using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public enum WeaponTutorialType
{
    CARNIVOROUSPLANT,
    ACORN,
    BANANASHOTGUN,
    CACTUSCROSSBOW
}

[Serializable]
public struct VideoFile
{
    public string fileName;
    public WeaponTutorialType weaponTutorialType;
}

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if(_instance == null)
            {
                // Find any TutorialManager objects in the scene
                var foundInstances = FindObjectsByType<TutorialManager>(FindObjectsSortMode.None);
                _instance = foundInstances.Length > 0 ? foundInstances[0] : null;

                if(_instance == null)
                {
                    Debug.LogError("TutorialManager instance not found in the scene. Please ensure there is a TutorialManager object.");
                }

                // If there are multiple TutorialManager instances, destroy them
                if (foundInstances.Length > 1)
                {
                    for (int i = 1; i < foundInstances.Length; i++)
                    {
                        Destroy(foundInstances[i].gameObject);
                    }
                }
            }
            return _instance;
        }
    }

    [Header("Tutorial Properties")]
    [SerializeField] private GameObject _canvasTutorial;

    [SerializeField] public VideoPlayer videoPlayer;

    [SerializeField] public List<VideoFile> videosFileNames;

    [SerializeField] public GameObject[] weaponsTexts;

    [SerializeField] public GameObject nextButtom, prevButtom, exitButtom;
    
    private WeaponTutorialType currentWeaponTutorial;

    private void Awake()
    {
        if(_canvasTutorial == null) Debug.LogError("CanvasTutorial is not assigned in the TutorialManager.");

        if(_canvasTutorial.activeSelf)
        {
            _canvasTutorial.SetActive(false);
        }
    }    

    public static void StartTutorial()
    {
        Instance._canvasTutorial.SetActive(true);
        Instance.PlayTutorial(WeaponTutorialType.CARNIVOROUSPLANT);
    }

    public void PlayTutorial(WeaponTutorialType weaponTutorial)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned in the TutorialManager.");
            return;
        }

        switch (weaponTutorial)
        {
            case WeaponTutorialType.CARNIVOROUSPLANT:
                
                PlayWeaponVideoTutorial(WeaponTutorialType.CARNIVOROUSPLANT);   

                weaponsTexts[0].SetActive(true);
                weaponsTexts[1].SetActive(false);
                weaponsTexts[2].SetActive(false);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(false);
                exitButtom.SetActive(false);
                break;

            case WeaponTutorialType.ACORN:
                
                PlayWeaponVideoTutorial(WeaponTutorialType.ACORN);

                weaponsTexts[0].SetActive(false);
                weaponsTexts[1].SetActive(true);
                weaponsTexts[2].SetActive(false);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(true);
                exitButtom.SetActive(false);
                break;

            case WeaponTutorialType.BANANASHOTGUN:
                
                PlayWeaponVideoTutorial(WeaponTutorialType.BANANASHOTGUN);

                weaponsTexts[0].SetActive(false);
                weaponsTexts[1].SetActive(false);
                weaponsTexts[2].SetActive(true);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(true);
                exitButtom.SetActive(false);
                break;

            case WeaponTutorialType.CACTUSCROSSBOW:
                
                PlayWeaponVideoTutorial(WeaponTutorialType.CACTUSCROSSBOW);

                weaponsTexts[0].SetActive(false);
                weaponsTexts[1].SetActive(false);
                weaponsTexts[2].SetActive(false);
                weaponsTexts[3].SetActive(true);
                nextButtom.SetActive(false);
                prevButtom.SetActive(true);
                exitButtom.SetActive(true);
                break;
        }
    }

    private void PlayWeaponVideoTutorial(WeaponTutorialType weaponTutorial)
    {
        string videoFileName = videosFileNames.FirstOrDefault(v => v.weaponTutorialType == weaponTutorial).fileName;

        if (string.IsNullOrEmpty(videoFileName))
        {
            Debug.LogError($"Video file name not found for weapon tutorial: {weaponTutorial} \n Please check in your inspector if you assigned the name correctly");
            return;
        }

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);        
        
        videoPlayer.url = videoPath;
        videoPlayer.Play();
    }

    public void NextTutorial()
    {
        currentWeaponTutorial++;
        if (currentWeaponTutorial > WeaponTutorialType.CACTUSCROSSBOW)
        {
            currentWeaponTutorial = WeaponTutorialType.CARNIVOROUSPLANT; // Loop back to the first tutorial
        }
        PlayTutorial(currentWeaponTutorial);
    }

    public void PreviousTutorial()
    {
        currentWeaponTutorial--;
        if (currentWeaponTutorial < WeaponTutorialType.CARNIVOROUSPLANT)
        {
            currentWeaponTutorial = WeaponTutorialType.CACTUSCROSSBOW; // Loop back to the last tutorial
        }
        PlayTutorial(currentWeaponTutorial);
    }

    public void ExitTutorial()
    {
        gameObject.SetActive(false);
        videoPlayer.Stop();        
        GameManager.StartGame();
    }
}
