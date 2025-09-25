using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Globalization;
using UnityEngine.Serialization;

[Serializable]
public struct WeaponUI
{
    [HideInInspector] public string name;
    public PlayerWeaponTypes WeaponType; // The type of weapon this crosshair is associated with
    public Image WeaponCrosshairImage;
    public Sprite WeaponSlotBackgroundImage;
    public Sprite WeaponSlotIconImage;
}

[Serializable]
public struct WeaponSlotInspector
{
    [HideInInspector] public string name;
    public WeaponSlot WeaponSlot;
}

[Serializable]
public struct WeaponSlot
{
    public Image WeaponSlotBackground;
    public Image WeaponSlotIcon;
}

public class HUDManager : Singleton<HUDManager>
{
    [Header("Canvas")]
    [SerializeField] private GameObject _canvasHud;
    [SerializeField] private GameObject _canvasScope;
    [SerializeField] private bool DebugPlayerRaycast = false; // Enable this to draw a debug ray in the scene view

    [Space]

    [Header("Sliders")]
    [SerializeField] private Slider _playerHealthBar;

    [Space]

    [SerializeField] private WeaponUI[] _weaponsUI;
    
    [Space]
    
    [Header("Wave Panel")]
    [SerializeField] private Image _wavePanel;
    [FormerlySerializedAs("_hordePanelScaleUpDuration")] [SerializeField] private float _wavePanelScaleUpDuration = 0.5f;
    [FormerlySerializedAs("_hordePanelScaleUpAmount")] [SerializeField] private float _wavePanelScaleUpAmount = 1.1f;
    [FormerlySerializedAs("_hordePanelScaleDownDuration")] [SerializeField] private float _wavePanelScaleDownDuration = 0.5f;
    [FormerlySerializedAs("_hordePanelScaleDownAmount")] [SerializeField] private float _wavePanelScaleDownAmount = 1.1f;
    [FormerlySerializedAs("_hordePanelScaleBackDuration")] [SerializeField] private float _wavePanelScaleBackDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI _raidersComingText;
    [FormerlySerializedAs("_hordeTimerText")] [SerializeField] private TextMeshProUGUI _waveTimerText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _waveCounterText;
    [SerializeField] private TextMeshProUGUI _raidersRemainingText;
    [SerializeField] private TextMeshProUGUI _raidersRemainingCounterText;
    [SerializeField] private TextMeshProUGUI _waveCompletedText;
    [SerializeField] private TextMeshProUGUI _goGetNewWeaponText;
    
    [Space]

    [Header("Ammo Panels")]
    [SerializeField] private Image _ammoPanel;
    [SerializeField] private TextMeshProUGUI _ammoTextPanel, _magAmmoTextPanel, _gunAmmoTextPanel, _meleeTextPanel;
    
    [Space]
    
    [Header("Ammo pickup text")]
    [SerializeField] private TextMeshProUGUI _ammoPickupText;
    [SerializeField] private float _ammoPickupTextDuration = 2f;
    [SerializeField] private int _maxAmmoPickupTextInstances = 5;

    [Space]
    [SerializeField] private List<WeaponSlotInspector> _availableWeaponSlots;

    private Dictionary<PlayerWeaponTypes, Image> _weaponCrosshairs = new Dictionary<PlayerWeaponTypes, Image>();
    private Dictionary<PlayerWeaponTypes, Color> _crosshairsOriginalColors = new Dictionary<PlayerWeaponTypes, Color>();
    private Dictionary<PlayerWeaponTypes, WeaponSlot> _weaponSlots = new Dictionary<PlayerWeaponTypes, WeaponSlot>();

    private readonly Vector3 _normalScale = Vector3.one;
    private readonly Vector3 _selectedScale = Vector3.one * 1.5f;
    private List<Coroutine> _scaleWeaponSlotsCoroutines = new List<Coroutine>();
    private float _scaleDuration = 0.2f;
    private Image[] _allImages;
    private TextMeshProUGUI[] _allTexts;
    private PlayerCharacterCombatController _playerCharacterCombatController;
    private static bool _enemyOnRange = false;
    private Animator _carivorousPLantCrosshairAnimator;
    private Vector2 _initialHordePanelSize;
    private List<TextMeshProUGUI> _ammoPickupTextList = new List<TextMeshProUGUI>();
    private IEnumerator _currentWaveRunningRoutine;
    private bool _scopeEnabled = false;

    public static bool EnemyOnRange => _enemyOnRange;

    private void Awake()
    {
        OnAwake();
        
        CheckInspectorAssigns();
        
        _initialHordePanelSize = _wavePanel.transform.localScale;
    }

    private void CheckInspectorAssigns()
    {
        // Check if Weapons UI is assigned in the inspector
        foreach (var weaponUI in _weaponsUI)
        {
            if (weaponUI.WeaponCrosshairImage == null)
            {
                Debug.LogError($"Weapon crosshair for {weaponUI.WeaponType} is not assigned in the inspector.");
            }

            if (weaponUI.WeaponSlotBackgroundImage == null)
            {
                Debug.LogError($"Weapon slot background image for {weaponUI.WeaponType} is not assigned in the inspector.");
            }

            if (weaponUI.WeaponSlotIconImage == null)
            {
                Debug.LogError($"Weapon slot icon image for {weaponUI.WeaponType} is not assigned in the inspector.");
            }

            _crosshairsOriginalColors[weaponUI.WeaponType] = weaponUI.WeaponCrosshairImage.color; // Store the original color of the crosshair
        }
        
        if(_wavePanel == null || _raidersComingText == null || _waveTimerText == null || _waveText == null || _waveCounterText == null || _raidersRemainingText == null || _raidersRemainingCounterText == null || _waveCompletedText == null || _goGetNewWeaponText == null)
        {
            Debug.LogError("Horde Panel is not assigned in the inspector.");
        }

        if (_ammoPanel == null)
        {
            Debug.LogError("Ammo Panel is not assigned in the inspector.");
        }

        if (_ammoTextPanel == null || _magAmmoTextPanel == null || _gunAmmoTextPanel == null || _meleeTextPanel == null)
        {
            Debug.LogError("One or more ammo text fields are not assigned in the inspector.");
        }

        if (_availableWeaponSlots.Count == 0)
        {
            Debug.LogError("Weapon slots are not assigned in the inspector.");
        }

        foreach (var weaponSlot in _availableWeaponSlots)
        {
            if (weaponSlot.WeaponSlot.WeaponSlotBackground == null || weaponSlot.WeaponSlot.WeaponSlotIcon == null)
            {
                Debug.LogError($"Weapon slot for {weaponSlot.name} is not assigned in the inspector.");
            }
        }
    }

    void Start()
    {
        if (_playerCharacterCombatController == null)
            _playerCharacterCombatController = GameManager.Player?.GetComponent<PlayerCharacterCombatController>();

        _weaponCrosshairs = _weaponsUI.ToDictionary(
            weapon => weapon.WeaponType,
            weapon => weapon.WeaponCrosshairImage
        );

        // Initialize ammo display
        UpdateAmmoDisplay();
        UpdateCrosshair();
        UpdateWeaponSlots();
        _allImages = GetComponentsInChildren<Image>(true);
        _allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        
        WaveManager.onWaveStatusChanged += UpdateWavePanel;
        UpdateWavePanel(GameManager.WaveStatus);
    }

    void Update()
    {
        if (GameManager.Player && _playerHealthBar) _playerHealthBar.value = GameManager.Player.GetComponent<PlayerCharacter>().Health / 100.0f;

        // Update ammo display every frame
        UpdateAmmoDisplay();

        DetectIfEnemyIsOnRange();

        if (GameManager.WaveStatus == WaveStatus.Preparing)
        {
            int hordeTimer = (int)GameManager.HordeTimer;
            _waveTimerText?.SetText(hordeTimer.ToString(CultureInfo.InvariantCulture));
        }

        if (GameManager.WaveStatus == WaveStatus.Finishing)
        {
            _raidersRemainingCounterText.SetText(GameManager.EnemiesInSceneCount.ToString(CultureInfo.InvariantCulture));
        }
    }
    
    private void UpdateWavePanel(WaveStatus waveStatus)
    {
        switch (waveStatus)
        {
            case WaveStatus.NotStarted:
                _wavePanel?.gameObject.SetActive(false);
                break;
            
            case WaveStatus.Preparing:
                if(_currentWaveRunningRoutine is not null) StopCoroutine(_currentWaveRunningRoutine);
                
                _wavePanel?.gameObject?.SetActive(true);
                _raidersComingText?.gameObject.SetActive(true);
                _waveText?.gameObject.SetActive(false);
                _waveCounterText?.gameObject.SetActive(false);
                _waveTimerText?.gameObject.SetActive(true);
                _raidersRemainingText?.gameObject.SetActive(false);
                _raidersRemainingCounterText?.gameObject.SetActive(false);
                _waveCompletedText?.gameObject.SetActive(false);
                _goGetNewWeaponText.gameObject.SetActive(false);

                _wavePanel.transform.localScale = _initialHordePanelSize;
                break;
            
            case WaveStatus.Running:
                _currentWaveRunningRoutine = WaveRunningRoutine(); 
                StartCoroutine(_currentWaveRunningRoutine);
                break;
            case WaveStatus.Finishing:
                _wavePanel?.gameObject?.SetActive(true);
                _raidersComingText.gameObject.SetActive(false);
                _waveTimerText.gameObject.SetActive(false);
                _waveText.gameObject.SetActive(false);
                _waveCounterText.gameObject.SetActive(false);
                _raidersRemainingText.gameObject.SetActive(true);
                _raidersRemainingCounterText.gameObject.SetActive(true);
                _waveCompletedText.gameObject.SetActive(false);
                _goGetNewWeaponText.gameObject.SetActive(false);

                StartCoroutine(ScaleBackToOriginalSize());
                break;
            
            case WaveStatus.Finished:
                _currentWaveRunningRoutine = WaveCompletedRoutine();
                StartCoroutine(_currentWaveRunningRoutine);
                break;
                
        }
    }
    
    private IEnumerator ScaleUpHordePanel()
    {
        if (_wavePanel == null) yield break;

        var rectTransform = _wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = initialScale * _wavePanelScaleUpAmount;
        float elapsed = 0f;

        while (elapsed < _wavePanelScaleUpDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / _wavePanelScaleUpDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator ScaleDownHordePanel()
    {
        if (_wavePanel == null) yield break;

        var rectTransform = _wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = initialScale * -_wavePanelScaleDownAmount;
        float elapsed = 0f;

        while (elapsed < _wavePanelScaleDownDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / _wavePanelScaleDownDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator ScaleBackToOriginalSize()
    {
        if (_wavePanel == null) yield break;

        var rectTransform = _wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = _initialHordePanelSize;
        float elapsed = 0f;

        while (elapsed < _wavePanelScaleBackDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / _wavePanelScaleBackDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator WaveRunningRoutine()
    {
        _raidersComingText.gameObject.SetActive(false);
        _waveTimerText.gameObject.SetActive(false);
        _waveText.gameObject.SetActive(true);
        _waveCounterText.gameObject.SetActive(true);
        _waveCounterText.SetText((GameManager.CurrentWave).ToString(CultureInfo.InvariantCulture));
        _raidersRemainingText.gameObject.SetActive(false);
        _raidersRemainingCounterText.gameObject.SetActive(false);
        _waveCompletedText.gameObject.SetActive(false);
        _goGetNewWeaponText.gameObject.SetActive(false);
        StartCoroutine(ScaleUpHordePanel());
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(ScaleDownHordePanel());
        _wavePanel?.gameObject.SetActive(false);
    }

    private IEnumerator WaveCompletedRoutine()
    {
        _wavePanel?.gameObject?.SetActive(true);
        _raidersComingText.gameObject.SetActive(false);
        _waveTimerText.gameObject.SetActive(false);
        _waveText.gameObject.SetActive(false);
        _waveCounterText.gameObject.SetActive(false);
        _raidersRemainingText.gameObject.SetActive(false);
        _raidersRemainingCounterText.gameObject.SetActive(false);
        _waveCompletedText.gameObject.SetActive(true);
        _goGetNewWeaponText.gameObject.SetActive(false);
        StartCoroutine(ScaleUpHordePanel());
        yield return new WaitForSeconds(10f);
        _waveCompletedText.gameObject.SetActive(false);
        _goGetNewWeaponText.gameObject.SetActive(true);
    }

    public static void Enable()
    {
        Instance._canvasHud.SetActive(true);
    }

    public static void Disable()
    {
        Instance._canvasHud.SetActive(false);
    }

    private void UpdateCrosshair()
    {
        if (_weaponCrosshairs.Count == 0)
        {
            Debug.LogWarning("Weapon crosshair is not assigned in the inspector.");
            return;
        }

        if (!_playerCharacterCombatController || _playerCharacterCombatController.PlayerWeapons.Count == 0 || _playerCharacterCombatController.EquippedWeapon == null)
        {
            _weaponCrosshairs.Values.ToList().ForEach(crosshair => crosshair.gameObject.SetActive(false)); // Disable all crosshairs if no weapons are available
            return;
        }

        foreach (var crosshair in _weaponCrosshairs)
        {
            if (crosshair.Key == _playerCharacterCombatController.EquippedWeapon.WeaponType)
                crosshair.Value.gameObject.SetActive(true); // Enable the crosshair for the selected weapon                    
            else
                crosshair.Value.gameObject.SetActive(false); // Disable the crosshair for other weapons
        }
    }

    private void DetectIfEnemyIsOnRange()
    {
        if (!_playerCharacterCombatController || _playerCharacterCombatController.EquippedWeapon == null)
            return; // Exit if no player character combat controller or no equipped weapon

        // Deproject a ray from the center of the screen to check if an enemy is in range
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _playerCharacterCombatController.EquippedWeapon.WeaponRange)) // Adjust the distance as needed
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // If an enemy is detected, change the crosshair color to red
                _weaponCrosshairs[_playerCharacterCombatController.EquippedWeapon.WeaponType].color = Color.red;
                _enemyOnRange = true; // Set the flag to true if an enemy is detected
            }
            else
            {
                // If no enemy is detected, reset the crosshair color to its original color
                _weaponCrosshairs[_playerCharacterCombatController.EquippedWeapon.WeaponType].color = _crosshairsOriginalColors[_playerCharacterCombatController.EquippedWeapon.WeaponType];

                _enemyOnRange = false; // Set the flag to false if no enemy is detected
            }
        }
        else
        {
            // If no enemy is detected, reset the crosshair color to its original color                
            _weaponCrosshairs[_playerCharacterCombatController.EquippedWeapon.WeaponType].color = _crosshairsOriginalColors[_playerCharacterCombatController.EquippedWeapon.WeaponType];

            _enemyOnRange = false; // Set the flag to false if no enemy is detected
        }

        // Draw a debug ray in the scene view for visualization
        if (Debug.isDebugBuild && DebugPlayerRaycast) // Only draw the debug ray in debug builds
        {
            Debug.DrawRay(ray.origin, ray.direction * _playerCharacterCombatController.EquippedWeapon.WeaponRange, Color.green);
        }

    }

    /*public static void ScopeEvent(bool scopeEnable)
    {
        if (Instance._scopeCrosshair == null || Instance._weaponsUI == null)
        {
            Debug.LogWarning("Scope or weapon crosshair is not assigned in the inspector.");
            return;
        }
        
        if (Instance._playerCharacterCombatController.EquippedWeapon == null)
        {
            Debug.LogWarning("No equipped weapon found. Cannot toggle scope crosshair.");
            return;
        }
        
        if (!scopeEnable)
        {
            Instance._scopeCrosshair.gameObject.SetActive(false);

            Instance._weaponCrosshairs[Instance._playerCharacterCombatController.EquippedWeapon.WeaponType].gameObject.SetActive(true); // Enable the crosshair for the selected weapon

            // Reset all images in this game object and its children to full alpha            
            foreach (Image img in Instance._allImages)
            {
                if (img != Instance._scopeCrosshair) // Ignore the scope crosshair image
                {
                    Color color = img.color;
                    color.a = 1f; // Set alpha to 100%
                    img.color = color;
                }
            }

            // Reset all text components in this game object and its children to full alpha
            foreach (TextMeshProUGUI text in Instance._allTexts)
            {
                Color color = text.color;
                color.a = 1f; // Set alpha to 100%
                text.color = color;
            }

        }
        else
        {
            Instance._scopeCrosshair.gameObject.SetActive(true);

            Instance._weaponCrosshairs[Instance._playerCharacterCombatController.EquippedWeapon.WeaponType].gameObject.SetActive(false); // Disable the crosshair for the selected weapon


            // Get all images in this game object and its children and set their alpha to 10%
            Image[] images = Instance.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img != Instance._scopeCrosshair.GetComponentsInChildren<Image>().Any<Image>())
                {
                    Color color = img.color;
                    color.a = 0.1f; // Set alpha to 20%
                    img.color = color;
                }
            }

            // Get all text components in this game object and its children and set their alpha to 10%            
            foreach (TextMeshProUGUI text in Instance._allTexts)
            {
                Color color = text.color;
                color.a = 0.1f; // Set alpha to 10%
                text.color = color;
            }
        }
    }*/

    public static void Bite()
    {
        if (!Instance._playerCharacterCombatController || Instance._playerCharacterCombatController.EquippedWeapon == null)
            return;
        
        var biteTrigger = Animator.StringToHash("Bite");

        if (Instance._carivorousPLantCrosshairAnimator == null)
        {
            var crosshair = Instance._weaponCrosshairs[PlayerWeaponTypes.CARNIVOROUSPLANTS];
            if (crosshair == null)
            {
                Debug.LogWarning("Carnivorous plant crosshair not found.");
                return;
            }

            Instance._carivorousPLantCrosshairAnimator = crosshair.GetComponentInChildren<Animator>(true);
            if (Instance._carivorousPLantCrosshairAnimator == null)
            {
                Debug.LogWarning("Carnivorous plant crosshair animator not found.");
                return;
            }
        }
        
        Instance._carivorousPLantCrosshairAnimator.SetTrigger(biteTrigger); // Trigger the bite animation on the carnivorous plant crosshair animator
    }

    private void UpdateAmmoDisplay()
    {
        if (_ammoPanel == null) Debug.LogError("Ammo Panel is not assigned in the inspector.");

        if (_ammoTextPanel == null || _magAmmoTextPanel == null || _gunAmmoTextPanel == null || _meleeTextPanel == null)
        {
            Debug.LogError("One or more ammo text fields are not assigned in the inspector.");
            return;
        }

        if (!_playerCharacterCombatController || _playerCharacterCombatController.PlayerWeapons.Count == 0)
        {
            _ammoPanel.gameObject.SetActive(false);
            _meleeTextPanel.gameObject.SetActive(false);
            _ammoTextPanel.gameObject.SetActive(false);

            return; // Exit if no player character combat controller or no weapons are available
        }


        _ammoPanel.gameObject.SetActive(true);

        if (_playerCharacterCombatController.EquippedWeapon is IEquippedGun equippedGun)
        {
            _meleeTextPanel.gameObject.SetActive(false);
            _ammoTextPanel.gameObject.SetActive(true);

            var magAmmo = 0;
            var totalAmmo = 0;

            if (equippedGun != null)
            {
                // If the equipped weapon is a gun, get its mag ammo
                magAmmo = equippedGun.MagAmmo;
            }


            totalAmmo = _playerCharacterCombatController.PlayerGunsAmmo[equippedGun.AmmoType];

            if (_magAmmoTextPanel != null)
            {
                _magAmmoTextPanel.text = $"{magAmmo}";
            }
            else Debug.LogWarning("Mag Ammo Text is not assigned in the inspector.");

            if (_ammoTextPanel != null)
            {
                _gunAmmoTextPanel.text = $"{totalAmmo}";
            }
            else Debug.LogWarning("Ammo Text is not assigned in the inspector.");

        }
        else
            if (_playerCharacterCombatController.EquippedWeapon is IEquippedMelee)
        {
            _meleeTextPanel.gameObject.SetActive(true);
            _ammoTextPanel.gameObject.SetActive(false);
        }
    }

    private void UpdateWeaponSlots()
    {
        CheckWeaponSlotsCount();

        if (!_playerCharacterCombatController || _playerCharacterCombatController.PlayerWeapons.Count == 0)
        {
            // If no weapons are available, disable all available weapon slots
            _availableWeaponSlots.ForEach(slot =>
            {
                if (slot.WeaponSlot.WeaponSlotBackground != null)
                {
                    slot.WeaponSlot.WeaponSlotBackground.gameObject.SetActive(false);
                }
            });

            return; // Exit if no player character combat controller or no weapons are available
        }

        ChangeWeaponSlotsScale();
        ChangeWeaponSlotsColor();
    }

    private void CheckWeaponSlotsCount()
    {
        if (_weaponSlots.Count == _playerCharacterCombatController.PlayerWeapons.Count)
            return; // If the weapon slots count matches the player weapons count, no need to update

        // make weapon slots count match player weapons count
        foreach (var playerWeapon in _playerCharacterCombatController.PlayerWeapons)
        {
            if (_weaponSlots.ContainsKey(playerWeapon.Key))
            {
                // If the weapon slot already exists, continue to the next weapon
                continue;
            }
            
            _weaponSlots.Add(playerWeapon.Key, _availableWeaponSlots.FirstOrDefault().WeaponSlot);
            _availableWeaponSlots.RemoveAt(0);

            var weaponUI = _weaponsUI.FirstOrDefault(w => w.WeaponType == playerWeapon.Key);

            _weaponSlots[playerWeapon.Key].WeaponSlotBackground.sprite = weaponUI.WeaponSlotBackgroundImage;
            _weaponSlots[playerWeapon.Key].WeaponSlotIcon.sprite = weaponUI.WeaponSlotIconImage;
            _weaponSlots[playerWeapon.Key].WeaponSlotBackground.gameObject.SetActive(true);

            _scaleWeaponSlotsCoroutines.Add(null); // Initialize the coroutine list with null for each weapon slot
        }

        if (_availableWeaponSlots.Count > 0) _availableWeaponSlots.ForEach(slot =>
        {
            if (slot.WeaponSlot.WeaponSlotBackground != null)
            {
                slot.WeaponSlot.WeaponSlotBackground.gameObject.SetActive(false); // Disable unused weapon slots
            }
        });
    }

    private void ChangeWeaponSlotsScale()
    {
        _scaleWeaponSlotsCoroutines.ForEach(routine =>
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        });

        _scaleWeaponSlotsCoroutines.Clear();

        foreach (var weaponSlot in _weaponSlots)
        {
            Vector3 targetScale = weaponSlot.Key == _playerCharacterCombatController.EquippedWeapon?.WeaponType ? _selectedScale : _normalScale;

            _scaleWeaponSlotsCoroutines.Add(StartCoroutine(ScaleWeponSlotsRoutine(weaponSlot.Value.WeaponSlotBackground, targetScale)));
        }
    }

    private void ChangeWeaponSlotsColor()
    {
        foreach (var weaponSlot in _weaponSlots)
        {
            var slotEquippedWeapon = weaponSlot.Key == _playerCharacterCombatController.EquippedWeapon?.WeaponType;

            if (slotEquippedWeapon)
            {
                weaponSlot.Value.WeaponSlotBackground.color = Color.white;
                weaponSlot.Value.WeaponSlotIcon.color = Color.white;
            }
            else
            {
                weaponSlot.Value.WeaponSlotBackground.color = Color.gray;
                weaponSlot.Value.WeaponSlotIcon.color = Color.gray;
            }
        }
    }

    private IEnumerator ScaleWeponSlotsRoutine(Image SlotImage, Vector3 targetScale)
    {
        var time = 0f;
        var initialScale = SlotImage.rectTransform.localScale;
        while (time < _scaleDuration)
        {
            SlotImage.rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, time / _scaleDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        SlotImage.rectTransform.localScale = targetScale;
    }
    
    private void ShowAmmoPickupText(int amount, AmmoTypes ammoType)
    {
        if (_ammoPickupText == null)
        {
            Debug.LogWarning("Ammo pickup text is not assigned in the inspector.");
            return;
        }

        // Clean up null references in the list
        _ammoPickupTextList.Remove(_ammoPickupTextList.Find(ugui => ugui is null));

        var ammoTextInstance = Instantiate(_ammoPickupText, _canvasHud.transform);
        ammoTextInstance.gameObject.SetActive(true);
        ammoTextInstance.text = $"+{amount} {ammoType.ToString()}";
        _ammoPickupTextList.Add(ammoTextInstance);
        
        if(_ammoPickupTextList.Count > _maxAmmoPickupTextInstances)
        {
            Destroy(_ammoPickupTextList.Last());
        }

        if (_ammoPickupTextList.Count > 1)
        {
            for (int i = _ammoPickupTextList.Count - 1; i > 0; i--)
            {
                if(i == 0) break;
            
                if(_ammoPickupTextList[i] && _ammoPickupTextList[i - 1]) _ammoPickupTextList[i].transform.position = _ammoPickupTextList[i - 1].transform.position + new Vector3(0, 30, 0); // Move the text up by 30 units
            }
        }
        
        StartCoroutine(AmmoPickupTextRoutine(ammoTextInstance.gameObject));
    }

    private IEnumerator AmmoPickupTextRoutine(GameObject ammoPickupText)
    {
        yield return new WaitForSeconds(_ammoPickupTextDuration);
        _ammoPickupTextList.Remove(ammoPickupText.GetComponent<TextMeshProUGUI>());
        Destroy(ammoPickupText);
    }

    private void ScopeEvent(float zoomFoV, float zoomSpeed)
    {
        if (!_canvasScope)
        {
            Debug.LogWarning("Scope or weapon crosshair is not assigned in the inspector.");
            return;
        }
        
        _scopeEnabled = !_scopeEnabled;

        if (!_scopeEnabled)
        {
            _canvasHud.gameObject.SetActive(true);
            _canvasScope.gameObject.SetActive(false);
        }
        else
        {
            _canvasHud.gameObject.SetActive(false);
            _canvasScope.gameObject.SetActive(true);
        }
    }

    private void ZoomOut()
    {
        if (!_canvasScope)
        {
            Debug.LogWarning("Scope or weapon crosshair is not assigned in the inspector.");
            return;
        }
        
        _scopeEnabled = false;
        _canvasHud.gameObject.SetActive(true);
        _canvasScope.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerCharacterCombatController.OnSwitchToWeapon += UpdateWeaponSlots;
        PlayerCharacterCombatController.OnSwitchToWeapon += UpdateCrosshair; // Update crosshair when switching weapons
        PlayerCharacterCombatController.OnSwitchToWeapon += ZoomOut;

        GameManager.OnPauseGame += Disable; // Disable HUD when the game is paused
        GameManager.OnResumeGame += Enable; // Enable HUD when the game is resumed
        
        WaveManager.onWaveStatusChanged += UpdateWavePanel; // Update the horde panel when the wave status changes
        
        PlayerCharacterCombatController.OnAmmoPickedUp += ShowAmmoPickupText; // Subscribe to the ammo pickup event
        
        CactusCrossbow.AimEvent += ScopeEvent; // Subscribe to the scope event
        
        PlayerCharacterCombatController.OnPerformReload += ZoomOut;
    }

    private void OnDisable()
    {
        PlayerCharacterCombatController.OnSwitchToWeapon -= UpdateWeaponSlots;
        PlayerCharacterCombatController.OnSwitchToWeapon -= UpdateCrosshair; // Remove the event listener when disabled
        PlayerCharacterCombatController.OnSwitchToWeapon -= ZoomOut;
        AnimationTriggerEvents.onReload -= ZoomOut;

        GameManager.OnPauseGame -= Disable; // Remove the event listener when disabled
        GameManager.OnResumeGame -= Enable; // Remove the event listener when disabled
        
        WaveManager.onWaveStatusChanged -= UpdateWavePanel; // Remove the event listener when disabled
        
        PlayerCharacterCombatController.OnAmmoPickedUp -= ShowAmmoPickupText; // Unsubscribe from the ammo pickup event
        
        CactusCrossbow.AimEvent -= ScopeEvent; // Unsubscribe from the scope event
        
        PlayerCharacterCombatController.OnPerformReload -= ZoomOut;
    }

}
