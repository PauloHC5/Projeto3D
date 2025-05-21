using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI magAmmoText;
    [SerializeField] private TextMeshProUGUI gunAmmoText;
    [SerializeField] private TextMeshProUGUI meleeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize ammo display
        UpdateAmmoDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        // Update ammo display every frame
        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
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

}
