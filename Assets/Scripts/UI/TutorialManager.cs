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
    public static TutorialManager Instance;

    [Header("Tutorial Properties")]
    [SerializeField] public VideoPlayer videoPlayer;

    [SerializeField] public List<VideoFile> videosFileNames;

    [SerializeField] public GameObject[] weaponsTexts;

    [SerializeField] public GameObject nextButtom, prevButtom, exitButtom;

    private WeaponTutorialType currentWeaponTutorial;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = UnityEngine.Object.FindFirstObjectByType<TutorialManager>();
        }        
    }

    private void Start()
    {
        PlayTutorial(WeaponTutorialType.CARNIVOROUSPLANT);

        if(SoundManager.CurrentMusicType != MusicType.AMBIENCE) SoundManager.PlayMusic(MusicType.AMBIENCE, true);
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
