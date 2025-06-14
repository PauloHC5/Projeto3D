using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class HUD : MonoBehaviour
{
    [SerializeField] private Slider playerHealthBar;    
    [SerializeField] private Image[] weaponCrosshairs = new Image[4];
    [SerializeField] private Image scopeCrosshair;

    [Header("Weapon Text Properties")]
    [SerializeField] private Image ammoPanel;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI magAmmoText;
    [SerializeField] private TextMeshProUGUI gunAmmoText;
    [SerializeField] private TextMeshProUGUI meleeText;
      
    [SerializeField] private List<Image> weaponSlots = new List<Image>();

    private readonly Vector3 normalScale = Vector3.one;
    private readonly Vector3 selectedScale = Vector3.one * 1.5f;
    private Coroutine[] scaleWeaponSlotsCoroutines;
    private float scaleDuration = 0.2f;
    private Image[] allImages;
    private TextMeshProUGUI[] allTexts;
    private int CrosshairIndex;
    private PlayerCharacterCombatController playerCharacterCombatController;
    private bool enemyOnRange = false;

    public bool EnemyOnRange => enemyOnRange;

    private Color[] crosshairsOriginalColors = new Color[4];

    private void Awake()
    {        
        scaleWeaponSlotsCoroutines = new Coroutine[weaponSlots.Count];        

        // Initialize crosshairs original colors
        for (int i = 0; i < weaponCrosshairs.Length; i++)
        {
            if (weaponCrosshairs[i] != null)
            {
                crosshairsOriginalColors[i] = weaponCrosshairs[i].color;
            }
            else
            {
                Debug.LogWarning($"Weapon crosshair at index {i} is not assigned in the inspector.");
            }
        }
    }

    void Start()
    {
        if (playerCharacterCombatController == null)
            playerCharacterCombatController = GameManager.Instance?.Player?.GetComponent<PlayerCharacterCombatController>();

        // Initialize ammo display
        UpdateAmmoDisplay();
        UpdateCrosshair();
        UpdateWeaponSlots();
        allImages = GetComponentsInChildren<Image>(true);
        allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
    }
    
    void Update()
    {
        if(playerCharacterCombatController == null)
            playerCharacterCombatController = GameManager.Instance?.Player?.GetComponent<PlayerCharacterCombatController>();

        if (playerHealthBar) playerHealthBar.value = GameManager.Instance.Player.GetComponent<PlayerCharacter>().Health / 100.0f;
        else Debug.LogWarning("Player Health Bar is not assigned in the inspector.");

        // Update ammo display every frame
        UpdateAmmoDisplay();

        DetectIfEnemyIsOnRange();
    }

    private void UpdateCrosshair()
    {               
        if(playerCharacterCombatController)
        {
            if (weaponCrosshairs.Length == 0)
            {
                Debug.LogWarning("Weapon crosshair is not assigned in the inspector.");
                return;
            }

            CrosshairIndex = (int)playerCharacterCombatController.WeaponSelected; // get the current weapon index from the player character combat controller

            // Ensure the index is within bounds
            if (CrosshairIndex < 0 || CrosshairIndex >= weaponCrosshairs.Length)
            {
                Debug.LogWarning("Crosshair index is out of bounds.");
                return;
            }

            // Set the active crosshair based on the current weapon
            for (int i = 0; i < weaponCrosshairs.Length; i++) // loop through all crosshairs
            {
                // Check if the crosshair is assigned in the inspector
                if (weaponCrosshairs[i] == null)
                {
                    Debug.LogWarning($"Weapon crosshair at index {i} is not assigned in the inspector.");
                    continue; // Skip to the next iteration if the crosshair is not assigned
                }

                // Set the active state of each crosshair based on the current weapon index
                // If the index matches, set it active; otherwise, set it inactive
                weaponCrosshairs[i].gameObject.SetActive(i == CrosshairIndex);
            }
        }
        else
        {
           if(weaponCrosshairs.Length > 0)
            {
                for (int i = 0; i < weaponCrosshairs.Length; i++) // loop through all crosshairs and disable them                
                {                    
                    weaponCrosshairs[i]?.gameObject.SetActive(false);
                }
            }
        }


    }

    private void DetectIfEnemyIsOnRange()
    {        
        if (playerCharacterCombatController?.EquippedWeapon != null)
        {
            // Deproject a ray from the center of the screen to check if an enemy is in range
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, playerCharacterCombatController.EquippedWeapon.WeaponRange)) // Adjust the distance as needed
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    // If an enemy is detected, change the crosshair color to red
                    if (CrosshairIndex >= 0 && CrosshairIndex < weaponCrosshairs.Length)
                    {
                        weaponCrosshairs[CrosshairIndex].color = Color.red;
                        enemyOnRange = true; // Set the flag to true if an enemy is detected
                    }
                }
                else
                {
                    // If no enemy is detected, reset the crosshair color to its original color
                    if (CrosshairIndex >= 0 && CrosshairIndex < weaponCrosshairs.Length)
                    {
                        weaponCrosshairs[CrosshairIndex].color = crosshairsOriginalColors[CrosshairIndex];
                        enemyOnRange = false; // Set the flag to false if no enemy is detected
                    }
                }
            }
            else
            {
                // If no enemy is detected, reset the crosshair color to its original color
                if (CrosshairIndex >= 0 && CrosshairIndex < weaponCrosshairs.Length)
                {
                    weaponCrosshairs[CrosshairIndex].color = crosshairsOriginalColors[CrosshairIndex];
                    enemyOnRange = false; // Set the flag to false if no enemy is detected
                }
            }

            // Draw a debug ray in the scene view for visualization
            if (Debug.isDebugBuild) // Only draw the debug ray in debug builds
            {
                Debug.DrawRay(ray.origin, ray.direction * playerCharacterCombatController.EquippedWeapon.WeaponRange, Color.green);
            }
        }
    }

    

    public void ScopeEvent(bool scopeEnable)
    {
        if (scopeCrosshair == null || weaponCrosshairs == null)
        {
            Debug.LogWarning("Scope or weapon crosshair is not assigned in the inspector.");
            return;
        }

        if (!scopeEnable)
        {
            scopeCrosshair.gameObject.SetActive(false);

            // Check if index is within bounds before accessing the array
            if (CrosshairIndex < 0 || CrosshairIndex >= weaponCrosshairs.Length)
            {
                Debug.LogWarning("Crosshair index is out of bounds.");
            }
            else
            {                
                weaponCrosshairs[CrosshairIndex].gameObject.SetActive(true);
            }

            // Reset all images in this game object and its children to full alpha            
            foreach (Image img in allImages)
            {
                if (img != scopeCrosshair) // Ignore the scope crosshair image
                {
                    Color color = img.color;
                    color.a = 1f; // Set alpha to 100%
                    img.color = color;
                }
            }

            // Reset all text components in this game object and its children to full alpha
            foreach (TextMeshProUGUI text in allTexts)
            {
                Color color = text.color;
                color.a = 1f; // Set alpha to 100%
                text.color = color;
            }

        }
        else
        {
            scopeCrosshair.gameObject.SetActive(true);

            // Check if index is within bounds before accessing the array
            if (CrosshairIndex < 0 || CrosshairIndex >= weaponCrosshairs.Length)
            {
                Debug.LogWarning("Crosshair index is out of bounds.");                
            }
            else
            {                
                weaponCrosshairs[CrosshairIndex].gameObject.SetActive(false);
            }
            

            // Get all images in this game object and its children and set their alpha to 10%
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img != scopeCrosshair)
                {
                    Color color = img.color;
                    color.a = 0.1f; // Set alpha to 20%
                    img.color = color;
                }
            }

            // Get all text components in this game object and its children and set their alpha to 10%            
            foreach (TextMeshProUGUI text in allTexts)
            {
                Color color = text.color;
                color.a = 0.1f; // Set alpha to 10%
                text.color = color;
            }
        }
    }

    public void Bite()
    {
        int BiteTrigger = Animator.StringToHash("Bite");

        if (CrosshairIndex == (int)WeaponTypes.Melee)
        {
            weaponCrosshairs[CrosshairIndex].GetComponent<Animator>().SetTrigger(BiteTrigger);
        }
    }

    private void UpdateAmmoDisplay()
    {
        if(ammoPanel == null) Debug.LogWarning("Ammo Panel is not assigned in the inspector.");

        if (ammoText == null || magAmmoText == null || gunAmmoText == null || meleeText == null)
        {
            Debug.LogWarning("One or more ammo text fields are not assigned in the inspector.");
            return;
        }        
        
        if (playerCharacterCombatController)
        {
            ammoPanel.gameObject.SetActive(true);

            if (playerCharacterCombatController.EquippedWeapon is IEquippedGun equippedGun)
            {
                meleeText.gameObject.SetActive(false);
                ammoText.gameObject.SetActive(true);

                var magAmmo = 0;
                var totalAmmo = 0;

                if (equippedGun != null)
                {
                    // If the equipped weapon is a gun, get its mag ammo
                    magAmmo = equippedGun.MagAmmo;
                }


                totalAmmo = playerCharacterCombatController.WeaponAmmo[equippedGun.AmmoType];

                if (magAmmoText != null)
                {
                    magAmmoText.text = $"{magAmmo}";
                }
                else Debug.LogWarning("Mag Ammo Text is not assigned in the inspector.");

                if (ammoText != null)
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
        else
        {
            ammoPanel.gameObject.SetActive(false);
            meleeText.gameObject.SetActive(false);
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

        if(playerCharacterCombatController)
        {
            foreach(var weaponSlot in weaponSlots)
            {
                if(weaponSlot != null) weaponSlot.gameObject.SetActive(true);                
            }

            WeaponTypes weaponType = playerCharacterCombatController.WeaponSelected;

            ChangeWeaponSlotsColor(weaponType);

            for (int i = 0; i < weaponSlots.Count; i++)
            {
                Vector3 target = (i == (int)weaponType) ? selectedScale : normalScale;

                if (scaleWeaponSlotsCoroutines[i] != null)
                    StopCoroutine(scaleWeaponSlotsCoroutines[i]);
                scaleWeaponSlotsCoroutines[i] = StartCoroutine(ScaleWeponSlotsRoutine(weaponSlots[i], target));
            }
        }
        else
        {
            foreach (var weaponSlot in weaponSlots)
            {
                if (weaponSlot != null) weaponSlot.gameObject.SetActive(false);
            }
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
        PlayerCharacterCombatController.onSwitchToWeapon += UpdateCrosshair; // Update crosshair when switching weapons
    }

    private void OnDisable()
    {
        PlayerCharacterCombatController.onSwitchToWeapon -= UpdateWeaponSlots;
        PlayerCharacterCombatController.onSwitchToWeapon -= UpdateCrosshair; // Remove the event listener when disabled
    }

}
