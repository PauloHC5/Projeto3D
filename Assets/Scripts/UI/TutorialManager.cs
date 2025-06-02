using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial Properties")]
    [SerializeField] public VideoPlayer videoPlayer;

    [SerializeField] public VideoClip[] videoClips;

    [SerializeField] public GameObject[] weaponsTexts;

    [SerializeField] public GameObject nextButtom, prevButtom, exitButtom;

    public int currentTutorialIndex = 0;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = UnityEngine.Object.FindFirstObjectByType<TutorialManager>();
        }
    }

    private void Update()
    {
        switch (currentTutorialIndex)
        {
            case 0:
                videoPlayer.clip = videoClips[0];
                weaponsTexts[0].SetActive(true);
                weaponsTexts[1].SetActive(false);
                weaponsTexts[2].SetActive(false);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(false);
                exitButtom.SetActive(false);
                break;

            case 1:
                videoPlayer.clip = videoClips[1];
                weaponsTexts[0].SetActive(false);
                weaponsTexts[1].SetActive(true);
                weaponsTexts[2].SetActive(false);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(true);
                exitButtom.SetActive(false);
                break;
            
            case 2:
                videoPlayer.clip = videoClips[2];
                weaponsTexts[0].SetActive(false);
                weaponsTexts[1].SetActive(false);
                weaponsTexts[2].SetActive(true);
                weaponsTexts[3].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(true);
                exitButtom.SetActive(false);
                break;

            case 3:
                videoPlayer.clip = videoClips[3];
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

    public void NextTutorial()
    {
        if (currentTutorialIndex < videoClips.Length - 1)
        {
            currentTutorialIndex++;
            videoPlayer.Play();
        }
    }

    public void PreviousTutorial()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            videoPlayer.Play();
        }
    }

    public void ExitTutorial()
    {
        gameObject.SetActive(false);
        videoPlayer.Stop();
        currentTutorialIndex = 0;
        GameManager.StartGame();
    }
}
