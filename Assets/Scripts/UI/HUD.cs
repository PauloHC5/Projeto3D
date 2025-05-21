using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class HUD : MonoBehaviour
{
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Image miniMap;

    [Header("Weapon Text Properties")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI magAmmoText;
    [SerializeField] private TextMeshProUGUI gunAmmoText;
    [SerializeField] private TextMeshProUGUI meleeText;
      
    [SerializeField] private List<Image> weaponSlots = new List<Image>();

    private readonly Vector3 normalScale = Vector3.one;
    private readonly Vector3 selectedScale = Vector3.one * 1.5f;
    private Coroutine[] scaleWeaponSlotsCoroutines;
    private float scaleDuration = 0.2f;

    private void Awake()
    {
        scaleWeaponSlotsCoroutines = new Coroutine[weaponSlots.Count];
    }

    void Start()
    {       
        // Initialize ammo display
        UpdateAmmoDisplay();        
    }
    
    void Update()
    {
        if (playerHealthBar) playerHealthBar.value = GameManager.Instance.Player.GetComponent<PlayerCharacter>().Health / 100.0f;
        else Debug.LogWarning("Player Health Bar is not assigned in the inspector.");

        // Update ammo display every frame
        UpdateAmmoDisplay();

        if(miniMap)
        {
            // Rotate the mini map according to the player's rotation
            miniMap.transform.rotation = Quaternion.Euler(0, 0, GameManager.Instance.Player.transform.eulerAngles.y);
        }
    }

    private void UpdateAmmoDisplay()
    {
        if(ammoText == null || magAmmoText == null || gunAmmoText == null || meleeText == null)
        {
            Debug.LogWarning("One or more ammo text fields are not assigned in the inspector.");
            return;
        }

        PlayerCharacterCombatController player = GameManager.Instance.Player.GetComponent<PlayerCharacterCombatController>();
        var equippedGun = player.EquippedWeapon as Gun;
        var dualWieldGun = player.EquippedWeapon as DualWieldGun;
        var weaponSelected = player.WeaponSelected;

        if (player.WeaponSelected != WeaponTypes.Melee)
        {
            meleeText.gameObject.SetActive(false);
            ammoText.gameObject.SetActive(true);

            var magAmmo = 0;
            var totalAmmo = 0;

            if (dualWieldGun)
            {
                magAmmo = dualWieldGun.MagAmmo;
            }
            else
            {
                if(equippedGun) magAmmo = equippedGun.MagAmmo;
            }


            if(equippedGun) totalAmmo = player.WeaponAmmo[equippedGun.WeaponType];
            else totalAmmo = player.WeaponAmmo[WeaponTypes.Shotgun];

            if (magAmmoText != null)
            {
                magAmmoText.text = $"{magAmmo}";
            }
            else Debug.LogWarning("Mag Ammo Text is not assigned in the inspector.");

            if(ammoText != null)
            {
                gunAmmoText.text = $"{totalAmmo}";
            }
            else Debug.LogWarning("Ammo Text is not assigned in the inspector.");

        }
        else
        {
            meleeText.gameObject.SetActive(true);            
            ammoText.gameObject.SetActive(false);

        }
    }

    private void UpdateWeaponSlots()
    {
        if (weaponSlots.Count == 0)
        {
            Debug.LogWarning("Weapon slots are not assigned in the inspector.");
            return;
        }

        WeaponTypes weaponType = GameManager.Instance.Player.GetComponent<PlayerCharacterCombatController>().WeaponSelected;

        ChangeWeaponSlotsColor(weaponType);        

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Vector3 target = (i == (int)weaponType) ? selectedScale : normalScale;            

            if (scaleWeaponSlotsCoroutines[i] != null)
                StopCoroutine(scaleWeaponSlotsCoroutines[i]);
            scaleWeaponSlotsCoroutines[i] = StartCoroutine(ScaleWeponSlotsRoutine(weaponSlots[i], target));
        }
    }

    private void ChangeWeaponSlotsColor(WeaponTypes weaponType)
    {                
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (i == (int)weaponType)
            {
                weaponSlots[i].color = Color.white;

                // Set the color of the child image to white
                Image[] childImages = weaponSlots[i].GetComponentsInChildren<Image>();
                if (childImages.Length == 2 && childImages[1] != null)
                {
                    childImages[1].color = Color.white;
                }
            }
            else
            {
                weaponSlots[i].color = Color.gray;

                // Set the color of the child image to gray
                Image[] childImages = weaponSlots[i].GetComponentsInChildren<Image>();
                if (childImages.Length == 2 && childImages[1] != null)
                {
                    childImages[1].color = Color.gray;
                }
            }
        }
    }

    private IEnumerator ScaleWeponSlotsRoutine(Image slot, Vector3 targetScale)
    {
        float time = 0f;
        Vector3 initialScale = slot.rectTransform.localScale;
        while (time < scaleDuration)
        {
            slot.rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, time / scaleDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        slot.rectTransform.localScale = targetScale;
    }

    private void OnEnable()
    {
        PlayerCharacterCombatController.onSwitchToWeapon += UpdateWeaponSlots;
    }

    private void OnDisable()
    {
        PlayerCharacterCombatController.onSwitchToWeapon -= UpdateWeaponSlots;
    }

}
