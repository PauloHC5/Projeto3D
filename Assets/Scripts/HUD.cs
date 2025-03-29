using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;

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
        if (GameManager.Instance != null && GameManager.Player != null)
        {
            var player = GameManager.Player;
            var equippedGun = player.EquippedWeapon as Gun;
            var dualWieldGun = player.EquippedWeapon as DualWieldGun;
            var weaponSelected = player.WeaponSelected;

            if (equippedGun != null)
            {
                var magAmmo = 0;
                var totalAmmo = 0;

                if (dualWieldGun)
                {
                    magAmmo = dualWieldGun.MagAmmo;
                }
                else
                {
                    magAmmo = equippedGun.MagAmmo;
                }                
                
                
                totalAmmo = player.WeaponAmmo[equippedGun.WeaponType];

                ammoText.text = $"{weaponSelected} \n Ammo: {magAmmo} / {totalAmmo}";
            }
            else
            {
                ammoText.text = $"{weaponSelected}";
            }
        }
    }
}
