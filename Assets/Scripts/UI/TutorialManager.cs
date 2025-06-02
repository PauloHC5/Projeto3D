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

    [SerializeField] public GameObject nextButtom, prevButtom;

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
                //weaponsTexts[1].SetActive(false);
                //weaponsTexts[2].SetActive(false);
                nextButtom.SetActive(true);
                prevButtom.SetActive(false);
                break;
        }

    }
}
