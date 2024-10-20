using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : Singleton<PlayerController>
{
    public bool FacingLeft { get { return facingLeft; } }

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;
    [SerializeField] private Transform weaponCollider;

    [SerializeField] private int currentExperience; // current exp
    [SerializeField] private int maxExperience;     // exp needed to level up
    [SerializeField] private int currentLevel = 1;  // Current level
    [SerializeField] private int baseExperience = 100;  // Base XP required for level 2
    [SerializeField] private float exponent = 1.5f;  // Exponent for XP curve
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private AudioClip playerDashSFX;
    [SerializeField] private AudioClip playerLevelUpSFX;
    [SerializeField] private int currentAppearanceLevel = 1; // Tracks the currently active appearance level


    private int experienceToNextLevel;
    

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private Knockback knockBack;
    private float startingMoveSpeed;

    private Slider experienceSlider;

    // New fields for sprite and animator changes
    [Header("Level Up Sprites and Animations")]
    [SerializeField] private Sprite[] levelSprites; // Array of sprites for each level
    [SerializeField] private RuntimeAnimatorController[] levelAnimators; // Array of animator controllers for each level

    private bool facingLeft = false;
    private bool isDashing = false;
    const string EXPERIENCE_SLIDER_TEXT = "Experience Slider";

    protected override void Awake() {
        base.Awake();
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        knockBack = GetComponent<Knockback>();

        Debug.Log($"Animator found: {myAnimator != null}");
        Debug.Log($"SpriteRenderer found: {mySpriteRenderer != null}");

        // Load player data from PlayerPrefs
        LoadPlayerData();
    }

    private void Start() {
        playerControls.Combat.Dash.performed += _ => Dash();
        startingMoveSpeed = moveSpeed;

        // Create Experience instance if it's not in the scene
        if (Experience.Instance == null) {
            GameObject experienceObject = new GameObject("Experience");
            experienceObject.AddComponent<Experience>();
        }

        // Initialize the experience slider
        InitializeExperienceSlider();

        if (levelText == null) {
            levelText = GameObject.Find("LevelText").GetComponent<TMP_Text>();
        }

        // Update the level display on start
        UpdateLevelText();
        UpdatePlayerAppearance(); // Update appearance based on initial level
    }

    private void OnEnable() {
        playerControls.Enable();
        StartCoroutine(WaitForExperienceInstance());
    }

    private IEnumerator WaitForExperienceInstance() {
        while (Experience.Instance == null) {
            yield return null; // Wait for the next frame
        }
        Experience.Instance.OnExperienceChange += HandleExperienceChange;
    }

    private void OnDisable() {
        playerControls.Disable();
        if (Experience.Instance != null) {
            Experience.Instance.OnExperienceChange -= HandleExperienceChange;
        }
    }

    private void Update() {
        PlayerInput();
        if (movement == Vector2.zero) {
            OnStop();
        }
    }

    private void FixedUpdate() {
        AdjustPlayerFacingDirection();
        Move();
    }

    public Transform GetWeaponCollider() {
        return weaponCollider;
    }

    private void PlayerInput() {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
    }

    public void SavePlayerState() {
        Vector3 playerPosition = transform.position;
        PlayerPrefs.SetFloat("PlayerPosX", playerPosition.x);
        PlayerPrefs.SetFloat("PlayerPosY", playerPosition.y);
        PlayerPrefs.SetFloat("PlayerPosZ", playerPosition.z);
        PlayerPrefs.SetInt("CurrentScene", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
    }

    public void OnStop() {
        SavePlayerState();
    }

    private void Move() {
        if (knockBack.GettingKnockedBack) { return; }
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection() {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (movement.x  < 0) {
            mySpriteRenderer.flipX = true;
            facingLeft = true;
        } else if (movement.x > 0) {
            mySpriteRenderer.flipX = false;
            facingLeft = false;
        }
    }

    private void Dash() {
        if (!isDashing && Stamina.Instance.CurrentStamina > 0) {
            Stamina.Instance.UseStamina();
            isDashing = true;
            moveSpeed *= dashSpeed;
            SoundFXManager.instance.PlaySoundFXClip(playerDashSFX, transform, 1f);
            myTrailRenderer.emitting = true;
            StartCoroutine(EndDashRoutine());
        }
    }

    private IEnumerator EndDashRoutine() {
        float dashTime = .2f;
        float dashCD = .25f;
        yield return new WaitForSeconds(dashTime);
        moveSpeed /= startingMoveSpeed;
        myTrailRenderer.emitting = false;
        yield return new WaitForSeconds(dashCD);
        isDashing = false;
    }

    private void HandleExperienceChange(int newExperience) {
        currentExperience += newExperience;
        Debug.Log("Player experience updated: " + currentExperience + " / " + experienceToNextLevel);

        while (currentExperience >= experienceToNextLevel) {
            LevelUp();
        }

        UpdateExperienceSlider();
        UpdateLevelText();
        SavePlayerData();
    }

    private void UpdateLevelText() {
        levelText.text = currentLevel.ToString();
    }

    private void InitializeExperienceSlider() {
        if (experienceSlider == null) {
            experienceSlider = GameObject.Find(EXPERIENCE_SLIDER_TEXT).GetComponent<Slider>();
        }

        experienceSlider.maxValue = experienceToNextLevel;
        experienceSlider.value = currentExperience;
    }

    private void UpdateExperienceSlider() {
        if (experienceSlider == null) {
            experienceSlider = GameObject.Find(EXPERIENCE_SLIDER_TEXT).GetComponent<Slider>();
        }

        experienceSlider.maxValue = experienceToNextLevel;
        experienceSlider.value = currentExperience;
    }

    private void CalculateExperienceToNextLevel() {
        experienceToNextLevel = Mathf.FloorToInt(baseExperience * Mathf.Pow(currentLevel, exponent));
        Debug.Log("Experience required for next level: " + experienceToNextLevel);
    }

    private void LevelUp() 
    {
        currentLevel++;
        currentExperience -= experienceToNextLevel;

        // Recalculate experience for next level
        CalculateExperienceToNextLevel();

        Debug.Log("Leveled up! Current Level: " + currentLevel + ", Next Max Exp: " + experienceToNextLevel);
        
        // Update experience slider
        UpdateExperienceSlider();
        SoundFXManager.instance.PlaySoundFXClip(playerLevelUpSFX, transform, 1f); // PLAY SOUNDFX

        // Update player appearance based on new level
        UpdatePlayerAppearance();

        // Save player data when leveling up
        SavePlayerData();
    }

    private void UpdatePlayerAppearance() 
    {
        Debug.Log($"Current Level: {currentLevel}, Current Appearance Level: {currentAppearanceLevel}");

        if (currentLevel == 5 && currentAppearanceLevel != 5) {
            currentAppearanceLevel = 5; 
            Debug.Log("Changing appearance to level 5");
            UpdateAppearanceForLevel(currentAppearanceLevel);
        }
        else if (currentLevel == 15 && currentAppearanceLevel != 15) {
            currentAppearanceLevel = 15; 
            Debug.Log("Changing appearance to level 15");
            UpdateAppearanceForLevel(currentAppearanceLevel);
        }
        else if (currentLevel < 5) {
            currentAppearanceLevel = 1; 
            UpdateAppearanceForLevel(currentAppearanceLevel);
        }
    }

    private void UpdateAppearanceForLevel(int level) {
    Debug.Log($"Updating appearance for level: {level}");

    // Update the sprite and animator controller based on the specified level
    if (level - 1 < levelSprites.Length) {
        mySpriteRenderer.sprite = levelSprites[level - 1]; // Update sprite
        Debug.Log($"Updated sprite: {levelSprites[level - 1].name}");
    } else {
        Debug.LogWarning($"No sprite available for level {level}");
    }

    if (level - 1 < levelAnimators.Length) {
        myAnimator.runtimeAnimatorController = levelAnimators[level - 1]; // Update animator
        Debug.Log($"Updated animator: {levelAnimators[level - 1].name}");
    } else {
        Debug.LogWarning($"No animator available for level {level}");
    }
}


    private void SavePlayerData() {
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerExperience", currentExperience);
        PlayerPrefs.Save();
    }

    private void LoadPlayerData() {
        if (PlayerPrefs.HasKey("PlayerLevel")) {
            currentLevel = PlayerPrefs.GetInt("PlayerLevel");
            currentExperience = PlayerPrefs.GetInt("PlayerExperience");
            CalculateExperienceToNextLevel();
        }
    }
}
