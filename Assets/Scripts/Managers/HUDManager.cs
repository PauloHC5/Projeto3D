using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;
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
    
    [Header("Blood Screen Effect Settings")]
    [SerializeField] private Image bloodScreenImage;
    [SerializeField] private float bloodScreenFadeDuration = 0.5f;
    [SerializeField] private float bloodScreenTimeOut = 0.25f;
    
    [Space]
    
    [Header("Gas Poisoning Effect Settings")]
    [SerializeField] private Image gasPoisoningImage;
    [SerializeField] private float gasPoisoningPulseDuration = 0.5f;
    [SerializeField] private float gasPoisoningTimeOut = 3f;
    
    [Range(0f, 1f)]
    [SerializeField] private float gasPoisoningMaxAlpha = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float gasPoisoningMinAlpha = 0f;

    [Space]

    [Header("Sliders")]
    [SerializeField] private Slider _playerHealthBar;

    [Space]

    [SerializeField] private WeaponUI[] _weaponsUI;
    
    [Space]
    
    [Header("Wave Panel")]
    [SerializeField] private Image wavePanel;
    [SerializeField] private float wavePanelScaleUpDuration = 0.5f;
    [SerializeField] private float wavePanelScaleUpAmount = 1.1f;
    [SerializeField] private float wavePanelScaleDownDuration = 0.5f;
    [SerializeField] private float wavePanelScaleDownAmount = 1.1f;
    [SerializeField] private float wavePanelScaleBackDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI raidersComingText;
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI waveCounterText;
    [SerializeField] private TextMeshProUGUI raidersRemainingText;
    [SerializeField] private TextMeshProUGUI raidersRemainingCounterText;
    [SerializeField] private TextMeshProUGUI waveCompletedText;
    [SerializeField] private TextMeshProUGUI goGetNewWeaponText;
    
    [Space]

    [Header("Ammo Panels")]
    [SerializeField] private Image ammoPanel;
    [SerializeField] private TextMeshProUGUI ammoTextPanel, magAmmoTextPanel, gunAmmoTextPanel, meleeTextPanel;
    
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
    private PlayerCharacter _playerCharacter;
    private IEnumerator _gasPoisoningEffectRoutine;

    public static bool EnemyOnRange => _enemyOnRange;

    private void Awake()
    {
        OnAwake();
        
        CheckInspectorAssigns();
        
        _initialHordePanelSize = wavePanel.transform.localScale;
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
        
        if(wavePanel == null || raidersComingText == null || waveTimerText == null || waveText == null || waveCounterText == null || raidersRemainingText == null || raidersRemainingCounterText == null || waveCompletedText == null || goGetNewWeaponText == null)
        {
            Debug.LogError("Horde Panel is not assigned in the inspector.");
        }

        if (ammoPanel == null)
        {
            Debug.LogError("Ammo Panel is not assigned in the inspector.");
        }

        if (ammoTextPanel == null || magAmmoTextPanel == null || gunAmmoTextPanel == null || meleeTextPanel == null)
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
        _weaponCrosshairs = _weaponsUI.ToDictionary(
            weapon => weapon.WeaponType,
            weapon => weapon.WeaponCrosshairImage
        );

        // Initialize ammo display
        UpdateAmmoDisplay();
        UpdateCrosshair();
        _allImages = GetComponentsInChildren<Image>(true);
        _allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        
        WaveManager.onWaveStatusChanged += UpdateWavePanel;
        UpdateWavePanel(GameManager.WaveStatus);
        
        _playerCharacter = GameObject.FindAnyObjectByType<PlayerCharacter>();
        if (_playerCharacter) 
            _playerCharacter.OnRegeneration += OnReGeneration;
        else
            Debug.LogWarning("PlayerCharacter component not found.");
    }

    void Update()
    {
        if (_playerCharacter) _playerHealthBar.value = _playerCharacter.Health / 100.0f;
        else
        {
            _playerCharacter = GameManager.Instance?.Player?.GetComponent<PlayerCharacter>();
        }

        // Update ammo display every frame
        UpdateAmmoDisplay();

        DetectIfEnemyIsOnRange();

        if (GameManager.WaveStatus == WaveStatus.Preparing)
        {
            int hordeTimer = (int)GameManager.HordeTimer;
            waveTimerText?.SetText(hordeTimer.ToString(CultureInfo.InvariantCulture));
        }

        if (GameManager.WaveStatus == WaveStatus.Finishing)
        {
            raidersRemainingCounterText.SetText(GameManager.EnemiesInSceneCount.ToString(CultureInfo.InvariantCulture));
        }
    }
    
    private void UpdateWavePanel(WaveStatus waveStatus)
    {
        switch (waveStatus)
        {
            case WaveStatus.NotStarted:
                wavePanel?.gameObject.SetActive(false);
                break;
            
            case WaveStatus.Preparing:
                wavePanel?.gameObject?.SetActive(true);
                raidersComingText?.gameObject.SetActive(true);
                waveText?.gameObject.SetActive(false);
                waveCounterText?.gameObject.SetActive(false);
                waveTimerText?.gameObject.SetActive(true);
                raidersRemainingText?.gameObject.SetActive(false);
                raidersRemainingCounterText?.gameObject.SetActive(false);
                waveCompletedText?.gameObject.SetActive(false);
                goGetNewWeaponText.gameObject.SetActive(false);

                wavePanel.transform.localScale = _initialHordePanelSize;
                break;
            
            case WaveStatus.Running:
                _currentWaveRunningRoutine = WaveRunningRoutine(); 
                StartCoroutine(_currentWaveRunningRoutine);
                break;
            case WaveStatus.Finishing:
                wavePanel?.gameObject?.SetActive(true);
                raidersComingText.gameObject.SetActive(false);
                waveTimerText.gameObject.SetActive(false);
                waveText.gameObject.SetActive(false);
                waveCounterText.gameObject.SetActive(false);
                raidersRemainingText.gameObject.SetActive(true);
                raidersRemainingCounterText.gameObject.SetActive(true);
                waveCompletedText.gameObject.SetActive(false);
                goGetNewWeaponText.gameObject.SetActive(false);

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
        if (wavePanel == null) yield break;

        var rectTransform = wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = initialScale * wavePanelScaleUpAmount;
        float elapsed = 0f;

        while (elapsed < wavePanelScaleUpDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / wavePanelScaleUpDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator ScaleDownHordePanel()
    {
        if (wavePanel == null) yield break;

        var rectTransform = wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = initialScale * -wavePanelScaleDownAmount;
        float elapsed = 0f;

        while (elapsed < wavePanelScaleDownDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / wavePanelScaleDownDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator ScaleBackToOriginalSize()
    {
        if (wavePanel == null) yield break;

        var rectTransform = wavePanel.rectTransform;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = _initialHordePanelSize;
        float elapsed = 0f;

        while (elapsed < wavePanelScaleBackDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / wavePanelScaleBackDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    private IEnumerator WaveRunningRoutine()
    {
        raidersComingText.gameObject.SetActive(false);
        waveTimerText.gameObject.SetActive(false);
        waveText.gameObject.SetActive(true);
        waveCounterText.gameObject.SetActive(true);
        waveCounterText.SetText((GameManager.CurrentWave).ToString(CultureInfo.InvariantCulture));
        raidersRemainingText.gameObject.SetActive(false);
        raidersRemainingCounterText.gameObject.SetActive(false);
        waveCompletedText.gameObject.SetActive(false);
        goGetNewWeaponText.gameObject.SetActive(false);
        StartCoroutine(ScaleUpHordePanel());
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(ScaleDownHordePanel());
        wavePanel?.gameObject.SetActive(false);
    }

    private IEnumerator WaveCompletedRoutine()
    {
        wavePanel?.gameObject?.SetActive(true);
        raidersComingText.gameObject.SetActive(false);
        waveTimerText.gameObject.SetActive(false);
        waveText.gameObject.SetActive(false);
        waveCounterText.gameObject.SetActive(false);
        raidersRemainingText.gameObject.SetActive(false);
        raidersRemainingCounterText.gameObject.SetActive(false);
        waveCompletedText.gameObject.SetActive(true);
        goGetNewWeaponText.gameObject.SetActive(false);
        StartCoroutine(ScaleUpHordePanel());
        yield return new WaitForSeconds(10f);
        if(GameManager.WaveStatus != WaveStatus.Finished) yield break;
        waveCompletedText.gameObject.SetActive(false);
        goGetNewWeaponText.gameObject.SetActive(true);
    }

    public static void Enable()
    {
        Instance._canvasHud.SetActive(true);
        Instance.bloodScreenImage.transform.parent.gameObject.SetActive(true);
        Instance.gasPoisoningImage.transform.parent.gameObject.SetActive(true);
    }

    public static void Disable()
    {
        Instance._canvasHud.SetActive(false);
        Instance._canvasScope.SetActive(false);
        Instance.bloodScreenImage.transform.parent.gameObject.SetActive(false);
        Instance.gasPoisoningImage.transform.parent.gameObject.SetActive(false);
    }

    private void OnGameOver()
    {
        Instance._canvasHud.SetActive(false);
        Instance._canvasScope.SetActive(false);
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
        if (ammoPanel == null) Debug.LogError("Ammo Panel is not assigned in the inspector.");

        if (ammoTextPanel == null || magAmmoTextPanel == null || gunAmmoTextPanel == null || meleeTextPanel == null)
        {
            Debug.LogError("One or more ammo text fields are not assigned in the inspector.");
            return;
        }

        if (!_playerCharacterCombatController || _playerCharacterCombatController.PlayerWeapons.Count == 0)
        {
            ammoPanel.gameObject.SetActive(false);
            meleeTextPanel.gameObject.SetActive(false);
            ammoTextPanel.gameObject.SetActive(false);

            return; // Exit if no player character combat controller or no weapons are available
        }


        ammoPanel.gameObject.SetActive(true);

        if (_playerCharacterCombatController.EquippedWeapon is IEquippedGun equippedGun)
        {
            meleeTextPanel.gameObject.SetActive(false);
            ammoTextPanel.gameObject.SetActive(true);

            var magAmmo = 0;
            var totalAmmo = 0;

            if (equippedGun != null)
            {
                // If the equipped weapon is a gun, get its mag ammo
                magAmmo = equippedGun.MagAmmo;
            }


            totalAmmo = _playerCharacterCombatController.PlayerGunsAmmo[equippedGun.AmmoType];

            if (magAmmoTextPanel != null)
            {
                magAmmoTextPanel.text = $"{magAmmo}";
            }
            else Debug.LogWarning("Mag Ammo Text is not assigned in the inspector.");

            if (ammoTextPanel != null)
            {
                gunAmmoTextPanel.text = $"{totalAmmo}";
            }
            else Debug.LogWarning("Ammo Text is not assigned in the inspector.");

        }
        else
            if (_playerCharacterCombatController.EquippedWeapon is IEquippedMelee)
        {
            meleeTextPanel.gameObject.SetActive(true);
            ammoTextPanel.gameObject.SetActive(false);
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
        if (!_playerCharacterCombatController)
        {
            _playerCharacterCombatController = GameManager.Instance?.Player?.GetComponent<PlayerCharacterCombatController>();
            
            if (!_playerCharacterCombatController)
            {
                Debug.LogWarning("PlayerCharacterCombatController component not found.");
                return;
            }
        }
        
        // If there are more weapon slots than player weapons, set the extra slots to inactive
        if (_weaponSlots.Count > _playerCharacterCombatController.PlayerWeapons.Count)
        {
            var keysToRemove = _weaponSlots.Keys.Except(_playerCharacterCombatController.PlayerWeapons.Keys).ToList();
            foreach (var key in keysToRemove)
            {
                _weaponSlots[key].WeaponSlotBackground.gameObject.SetActive(false);
            }
        }

        // make weapon slots count match player weapons count
        foreach (var playerWeapon in _playerCharacterCombatController.PlayerWeapons)
        {
            if (_weaponSlots.ContainsKey(playerWeapon.Key))
            {
                _weaponSlots[playerWeapon.Key].WeaponSlotBackground.gameObject.SetActive(true);
                continue; // If the weapon slot already exists, skip to the next weapon
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
            
                if(_ammoPickupTextList[i] && _ammoPickupTextList[i - 1]) _ammoPickupTextList[i].transform.position = _ammoPickupTextList[i - 1].transform.position + new Vector3(0, 50, 0); // Move the text up by 50 units
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
    
    public static void ShowBloodScreen()
    {
        if (Instance.bloodScreenImage == null)
        {   
            Debug.LogWarning("Blood screen image is not assigned in the inspector.");
            return;
        }
        
        Instance.bloodScreenImage.gameObject.SetActive(true);

        Instance.StartCoroutine(Instance.FadeInBloodScreenRoutine());
    }

    private IEnumerator FadeInBloodScreenRoutine()
    {
        var elapsedTime = 0f;
        var duration = bloodScreenFadeDuration;
        var noTranparencyColor = bloodScreenImage.color;
        if(_playerHealthBar.value > 75f / 100f) 
            noTranparencyColor.a = 0.25f;
        else
            if(_playerHealthBar.value <= 75f / 100f && _playerHealthBar.value > 50f / 100f) 
                noTranparencyColor.a = 0.5f;
        else
            noTranparencyColor.a = 1.0f;
        
        while (elapsedTime < duration)
        {
            bloodScreenImage.color = Color.Lerp(bloodScreenImage.color, noTranparencyColor, (elapsedTime / duration));
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(bloodScreenTimeOut);
        
        if(_playerHealthBar.value <= 15f / 100f) yield break;

        StartCoroutine(FadeOutBloodScreenRoutine());
    }
    
    private IEnumerator FadeOutBloodScreenRoutine()
    {
        if(!bloodScreenImage.gameObject.activeSelf) yield break;
        
        var elapsedTime = 0f;
        var duration = 0.5f;
        var noTranparencyColor = bloodScreenImage.color;
        
        if(_playerHealthBar.value <= 75f / 100f && _playerHealthBar.value > 50f / 100f)
            noTranparencyColor.a = 0.25f;
        else
            if(_playerHealthBar.value <= 50f / 100f && _playerHealthBar.value > 25f / 100f)
                noTranparencyColor.a = 0.5f;
        else
            if(_playerHealthBar.value <= 25f / 100f && _playerHealthBar.value > 15f / 100f)
                noTranparencyColor.a = 0.75f;
        else
            noTranparencyColor.a = 0.0f;

        while (elapsedTime < duration)
        {
            bloodScreenImage.color = Color.Lerp(bloodScreenImage.color, noTranparencyColor, (elapsedTime / duration));
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if(bloodScreenImage.color.a > 75f / 100f) bloodScreenImage.gameObject.SetActive(false);
    }
    
    public static void ShowGasPoisoningScreen()
    {
        if (Instance.gasPoisoningImage == null)
        {   
            Debug.LogWarning("Gas poisoning image is not assigned in the inspector.");
            return;
        }

        Instance.gasPoisoningImage.gameObject.SetActive(true);
        Instance.StartCoroutine(Instance.GasPoisoningEffectRoutine());
    }
    
    private IEnumerator GasPoisoningEffectRoutine()
    {
        if (_gasPoisoningEffectRoutine != null)
        {
            yield break;
        }
        _gasPoisoningEffectRoutine = GasPoisoningEffect();
        
        StartCoroutine(_gasPoisoningEffectRoutine);
        
        yield return new WaitForSeconds(gasPoisoningTimeOut);
        
        StopCoroutine(_gasPoisoningEffectRoutine);
        _gasPoisoningEffectRoutine = null;
        
        // Fade out the gas poisoning effect
        var elapsedTime = 0f;
        var duration = 0.5f;
        var transparentColor = gasPoisoningImage.color;
        transparentColor.a = 0.0f;
        while (elapsedTime < duration)
        {
            gasPoisoningImage.color = Color.Lerp(gasPoisoningImage.color, transparentColor, (elapsedTime / duration));
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator GasPoisoningEffect()
    {
        // Interpolate the alpha value of the gas poisoning image to create a pulsing effect
        var elapsedTime = 0f;
        var pulseDuration = gasPoisoningPulseDuration; // Duration of one pulse cycle (fade in + fade out)
        var maxAlpha = gasPoisoningImage.color;
        maxAlpha.a = gasPoisoningMaxAlpha;
        var minAlpha = gasPoisoningImage.color;
        minAlpha.a = gasPoisoningMinAlpha;
        var fadingOut = true;
        
        while (true)
        {
            while (elapsedTime < pulseDuration / 2f)
            {
                gasPoisoningImage.color = Color.Lerp(fadingOut ? maxAlpha : minAlpha, fadingOut ? minAlpha : maxAlpha, (elapsedTime / (pulseDuration / 2f)));
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            elapsedTime = 0f;
            fadingOut = !fadingOut; // Toggle between fading in and out
        }
    }
    
    private void OnReGeneration()
    {
        StartCoroutine(FadeOutBloodScreenRoutine());
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bloodScreenImage.color = new Color(bloodScreenImage.color.r, bloodScreenImage.color.g, bloodScreenImage.color.g, 0f);
        bloodScreenImage.gameObject.SetActive(false);
        gasPoisoningImage.gameObject.SetActive(false);
        
        _playerCharacter = GameObject.FindAnyObjectByType<PlayerCharacter>();
        if (_playerCharacter) 
            _playerCharacter.OnRegeneration += OnReGeneration;
        else
            Debug.LogWarning("PlayerCharacter component not found.");
    }

    private void OnEnable()
    {
        PlayerCharacterCombatController.OnSwitchToWeapon += UpdateWeaponSlots;
        PlayerCharacterCombatController.OnSwitchToWeapon += UpdateCrosshair; // Update crosshair when switching weapons
        PlayerCharacterCombatController.OnSwitchToWeapon += ZoomOut;

        GameManager.OnPauseGame += Disable; // Disable HUD when the game is paused
        GameManager.OnResumeGame += Enable; // Enable HUD when the game is resumed
        GameManager.OnGameOver += OnGameOver; // Disable HUD when the game is over
        
        WaveManager.onWaveStatusChanged += UpdateWavePanel; // Update the horde panel when the wave status changes
        
        PlayerCharacterCombatController.OnAmmoPickedUp += ShowAmmoPickupText; // Subscribe to the ammo pickup event
        
        CactusCrossbow.AimEvent += ScopeEvent; // Subscribe to the scope event
        
        PlayerCharacterCombatController.OnPerformReload += ZoomOut;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        PlayerCharacterCombatController.OnSwitchToWeapon -= UpdateWeaponSlots;
        PlayerCharacterCombatController.OnSwitchToWeapon -= UpdateCrosshair; // Remove the event listener when disabled
        PlayerCharacterCombatController.OnSwitchToWeapon -= ZoomOut;
        AnimationTriggerEvents.onReload -= ZoomOut;

        GameManager.OnPauseGame -= Disable; // Remove the event listener when disabled
        GameManager.OnResumeGame -= Enable; // Remove the event listener when disabled
        GameManager.OnGameOver -= OnGameOver; // Remove the event listener when disabled
        
        WaveManager.onWaveStatusChanged -= UpdateWavePanel; // Remove the event listener when disabled
        
        PlayerCharacterCombatController.OnAmmoPickedUp -= ShowAmmoPickupText; // Unsubscribe from the ammo pickup event
        
        CactusCrossbow.AimEvent -= ScopeEvent; // Unsubscribe from the scope event
        
        PlayerCharacterCombatController.OnPerformReload -= ZoomOut;
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
