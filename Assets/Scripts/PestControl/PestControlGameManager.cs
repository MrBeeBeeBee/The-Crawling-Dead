using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using static UnityEngine.ParticleSystem;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class PestControlGameManager : MonoBehaviour
{
    public static PestControlGameManager instance;
    public GameObject miniGame;
    public PestControlPlayer player;
    public Variant variant;
    public bool isGameActive = false;

    [Header("UI Elements")]
    public GameObject startMenu;
    public GameObject endMenu;
    public GameObject infoMenu;
    public GameObject UI;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private GameObject venomTrait;
    [SerializeField] private GameObject tempParalysisTrait;
    [SerializeField] private GameObject scoreMeter;
    [SerializeField] private GameObject meterBacking;
    [SerializeField] public Slider sequenceTimerSlider;
    [SerializeField] private Sprite venomActive;
    [SerializeField] private Sprite venomInactive;
    [SerializeField] private TMP_Text venomText;
    [SerializeField] private Sprite tempParalysisActive;
    [SerializeField] private Sprite tempParalysisInactive;
    [SerializeField] private TMP_Text tempParalysisText;
    [SerializeField] private TMP_Text correctSequencesText;
    [SerializeField] private TMP_Text correctLettersText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private GameObject caterpillar;
    [SerializeField] private GameObject catSpotOne;
    [SerializeField] private GameObject catSpotTwo;
    public Animator caterpillarAnimator;

    [Header("Letter Block Image Data")]
    private List<GameObject> letterBlockPrefabs = new List<GameObject>();
    private Dictionary<char, GameObject> letterBlockDictionary = new Dictionary<char, GameObject>();
    private List<GameObject> instantiatedBlocks = new List<GameObject>();
    [SerializeField] private float sequenceStartGap;
    [SerializeField] private float blockSpacing;

    [SerializeField] private GameObject letterSourcePosition;
    [SerializeField] private GameObject letterSourcePosition1;
    [SerializeField] private GameObject letterSourcePosition2;

    [Header("Player Data")]
    // Stores player traits
    public Dictionary<string, bool> traits = new Dictionary<string, bool>();
    public List<char> playerInputList = new List<char>();
    public int correctCounter = 0;

    [Header("Game Data")]
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public bool playerControlsActive = false;
    public List<char> correctStringList = new List<char>();
    [SerializeField] public int sequenceLength;
    private int maxScore = 4500;
    [SerializeField] public float meterAdjustment;
    [SerializeField] public float score;
    [SerializeField] private float scoreDecayRate;
    private float timeRemaining = 30.0f;
    private float sequenceTimer;
    private float timerReference;
    private float blockActionTimer = 0.0f;
    private bool pointsChanged = false;
    private bool allowInputs = true;
    public int correctLetters;
    public int correctSequences;
    [SerializeField] private float letterGap;

    [Header("Audio Elements")]
    public AudioSource blockRightSFX;
    public AudioSource blockWrongSFX;
    public AudioSource correctSequenceSFX;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        NewGame();
    }

    private void OnEnable()
    {
        variant = MiniGameHandler.instance.variant;
        IntroTextHandler();
    }
    public void NewGame()
    {
        // Enable Start Menu
        startMenu.SetActive(true);

        // Disable End Menu
        endMenu.SetActive(false);

        //Instantiate new game
        SetScore(0);
    }

    // Counts down timer
    public void Update()
    {
        if (isGameActive && timeRemaining > 0)
        {
            // Start the timer for the game and individual sequence
            timeRemaining -= Time.deltaTime;
            timerText.text = Mathf.Round(timeRemaining).ToString();
            // timerText.text = Mathf.Round((timeRemaining * 100.0f) * 0.01f).ToString();

            sequenceTimer -= Time.deltaTime;
            sequenceTimer = Mathf.Clamp(sequenceTimer, 0, timerReference);
            sequenceTimerSlider.value = Mathf.Clamp01(sequenceTimer / timerReference);

            // Updates score UI information
            UpdateScoreUI();

            // Move caterpillar graphic over time
            caterpillar.transform.position = Vector3.Lerp(catSpotOne.transform.position, 
                catSpotTwo.transform.position, ((30 - timeRemaining)) / 30);

            if (playerControlsActive)
            {
                // Slowly decay the player's score depending on traits equipped
                if (score >= 100)
                    score -= scoreDecayRate * Time.deltaTime;

                // Adds an uppercase letter to playerInputList
                foreach (char c in Input.inputString.ToUpper())
                {
                    if (allowInputs)
                    {
                        playerInputList.Add(c);
                        if (playerInputList[correctCounter] == correctStringList[correctCounter])
                        {
                            correctCounter++;
                            correctLetters++;
                            blockRightSFX.Play();
                        }
                        Debug.Log($"Current Input: {string.Join("", playerInputList)}");
                    }
                }

                if (sequenceTimer <= 0)
                {
                    Debug.Log("You ran out of time for that sequence...");
                    allowInputs = false;

                    if (!pointsChanged)
                    {
                        if (this.score >= 450)
                        {
                            blockWrongSFX.Play();
                            SetScore(this.score - 450);
                            pointsChanged = true;
                        }
                        else
                        {
                            blockWrongSFX.Play();
                            SetScore(0);
                            pointsChanged = true;
                        }
                    }
                    // Visually changes incorrectly pressed letter to red
                    for (int i = 0; i < sequenceLength; i++)
                    {
                        SpriteRenderer spriteRenderer = instantiatedBlocks[i].GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.color = Color.red;
                        }
                    }

                    blockActionTimer += Time.deltaTime;
                    if (blockActionTimer >= 0.5f)
                    {
                        DestroyLetterBlocks();
                        InstantiateLetterBlocks();
                    }
                }

                // If the player has input a letter, proceed. If the list is empty, don't bother
                if (playerInputList.Count > 0)
                {
                    // Colors the corresponding image for each correct input as green
                    if (playerInputList[playerInputList.Count - 1] == correctStringList[playerInputList.Count - 1])
                    {
                        // Mark that the image is right. Maybe tint the image green?
                        SpriteRenderer spriteRenderer = instantiatedBlocks[playerInputList.Count - 1].GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.color = Color.green;
                        }
                    }
                    // Turns the image red if not and deducts points
                    else
                    {
                        Debug.Log("Incorrect input detected, resetting...");
                        allowInputs = false;

                        if (!pointsChanged)
                        {
                            if (this.score >= 450)
                            {
                                blockWrongSFX.Play();
                                SetScore(this.score - 450);
                                pointsChanged = true;
                            }
                            else
                            {
                                blockWrongSFX.Play();
                                SetScore(0);
                                pointsChanged = true;
                            }
                        }

                        SpriteRenderer spriteRenderer = instantiatedBlocks[playerInputList.Count - 1].GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.color = Color.red;
                        }

                        blockActionTimer += Time.deltaTime;
                        if (blockActionTimer >= 1.0f)
                        {
                            DestroyLetterBlocks();
                            InstantiateLetterBlocks();
                        }
                    }

                    // Checks if the whole sequence is correct
                    if (correctCounter == sequenceLength)
                    {
                        Debug.Log("Correct sequence detected, resetting...");
                        allowInputs = false;

                        if (!pointsChanged)
                        {
                            SetScore(this.score + (125 * sequenceLength));
                            correctSequenceSFX.Play();
                            caterpillarAnimator.Play("Cat Shake", 0);
                            correctSequences++;
                            pointsChanged = true;
                        }

                        blockActionTimer += Time.deltaTime;
                        if (blockActionTimer >= 0.5f)
                        {
                            DestroyLetterBlocks();
                            InstantiateLetterBlocks();
                        }

                        if (score >= 4500)
                        {
                            this.score = 4500;
                            GameOver();
                        }
                    }
                }
            }
        }
        else if (isGameActive && timeRemaining <= 0)
        {
            GameOver();
        }
    }

    public void StartButton()
    {
        // Close down start menu
        startMenu.SetActive(false);

        // Establishes a dictionary of letterblocks
        InitializeLetterBlockDictionary();

        // Instantiates the letter blocks
        InstantiateLetterBlocks();

        // Let Player See The Game First

        // Start timer for minigame
        isGameActive = true;

        // Activate player controls
        playerControlsActive = true;


        if (variant.traits["Venom1"])
        {
            scoreDecayRate = 22.5f;
        }
        else
        {
            scoreDecayRate = 45.0f;
        }
        if (variant.traits["TempParalysis"])
        {
            timerReference = 12.0f;
        }
        else
        {
            timerReference = 9.0f;
        }
        sequenceTimer = timerReference;

        // Turn on UI elements
        UI.SetActive(true);
        scoreMeter.transform.position = meterBacking.transform.position;
    }

    public void QuitButton()
    {
        //Debug.Log("SMQB");
        // Award rewards - TODO
        GameManager.instance.genomePoints += (int)score;

        // Reset UI to a starting state
        startMenu.SetActive(true);
        endMenu.SetActive(false);
        miniGame.SetActive(false);
        timeRemaining = 30.0f;
        score = 0;
        correctLetters = 0;
        correctSequences = 0;

        // Freeze everything
        isGameActive = false;
        playerControlsActive = false;
        // Reset point worth - TODO

        // Allow the player to interact with the main game
        GlobalTimer.Instance.isPaused = false;
        MiniGameHandler.instance.EnableMainGameInteractables();
        MiniGameHandler.instance.tv.GetComponent<TvController>().turnOn = false;
    }

    public void InfoButton()
    {
        startMenu.SetActive(false);
        infoMenu.SetActive(true);
    }

    public void BackToStartButton()
    {
        infoMenu.SetActive(false);
        startMenu.SetActive(true);
    }

    // Provides the player with situational information
    public void IntroTextHandler()
    {
        SpriteRenderer venomSprite = venomTrait.GetComponent<SpriteRenderer>();
        SpriteRenderer tempParalysisSprite = tempParalysisTrait.GetComponent<SpriteRenderer>();

        string intro = $"<b>" + variant.title + "</b> has found many " +
            $"pest hosts within a farmer's crop field that are attacking an important food crop. " +
            $"Your job is to not let the host develop any further by quickly pressing all " +
            $"of the letters you see before the time runs out.";

        if (variant.traits["Venom1"])
        {
            venomSprite.sprite = venomActive;
            venomText.alpha = 1f;
        }
        else
        {
            venomSprite.sprite = venomInactive;
            venomText.alpha = 0.5f;
        }
        if (variant.traits["TempParalysis"])
        {
            tempParalysisSprite.sprite = tempParalysisActive;
            tempParalysisText.alpha = 1f;
        }
        else
        {
            tempParalysisSprite.sprite = tempParalysisInactive;
            tempParalysisText.alpha = 0.5f;
        }

        introText.text = intro;
    }

    // Closes down the minigame
    public void GameOver()
    {
        Debug.Log("Game Over");

        // Destroy the instanced blocks
        DestroyLetterBlocks();

        // Pause game activities
        isGameActive = false;
        // Set UI to inactive
        UI.SetActive(false);
        endMenu.SetActive(true);
        // End Text Here
        finalScoreText.text = Mathf.Round(this.score).ToString();
        correctLettersText.text = this.correctLetters.ToString();
        correctSequencesText.text = this.correctSequences.ToString();
        finalTimeText.text = timerText.text + " seconds";
    }

    private void SetScore(float score)
    {
        //Debug.Log("Set Score");
        this.score = score;
    }

    public void GenerateSequence()
    {
        sequenceLength = Random.Range(4, 8);
        List<char> transferList = RandomString(sequenceLength);
        this.correctStringList.Clear();
        this.correctStringList.AddRange(transferList);

        for(int i = 0; i < sequenceLength; i++)
        {
            this.correctStringList[i] = transferList[i];
        }
    }

    public static List<char> RandomString(int length)
    {
        List<char> builtStringList = new List<char>();
        for (int i=0; i<length; i++)
        {
            builtStringList.Add(chars[Random.Range(0, chars.Length)]);
        }

        return builtStringList;
    }

    // What it sounds like, fills a dictionary with the proper game objects
    public void InitializeLetterBlockDictionary()
    {
        Debug.Log("Initializing letter block dictionary...");

        letterBlockDictionary.Clear();

        Debug.Log($"Characters in string: {chars.Length}");

        foreach (char letter in chars) // Add more characters as needed
        {
            Debug.Log($"My brother in christ we are at least in here");
            try
            {
                GameObject letterPrefab = Resources.Load<GameObject>($"PestControl/{letter}");
                if (letterPrefab != null)
                {
                    letterBlockDictionary.Add(letter, letterPrefab);
                    Debug.Log($"Loaded prefab for {letter}");
                }
                else
                {
                    Debug.LogWarning($"Prefab for '{letter}' not found in Resources.");
                }
            }
            catch
            {
                Debug.LogError($"Error in InitializeLetterBlockDictionary");
            }
        }
    }

    public void InstantiateLetterBlocks()
    {
        Debug.Log("Instantiating objects...");

        // Ensure we have a valid source position
        if (letterSourcePosition == null)
        {
            Debug.LogError("Letter source position is not assigned!");
            return;
        }

        // Check if dictionary is properly populated
        if (letterBlockDictionary.Count == 0)
        {
            Debug.LogError("Letter block dictionary is empty! Did you call InitializeLetterBlockDictionary()?");
            return;
        }

        // Regenerates a new correct string
        GenerateSequence();

        // Resets the users input
        playerInputList.Clear();

        // Loops through the generated correct string list
        if (letterSourcePosition == null) return; // Ensure reference exists

        // Assign start gap and space between letters based on string length
        // Longer strings should move further left (more gap) but have less space (less block space)
        // Shorter strings should move less to the left (less gap) and have more space between letters (more block space)
        int count = correctStringList.Count;

        sequenceStartGap = (count * 1.0f) - 1.0f;

        Vector3 startPosition = letterSourcePosition.transform.position; // Base position
        startPosition = letterSourcePosition1.transform.position;
        letterGap = letterSourcePosition2.transform.position.x - letterSourcePosition1.transform.position.x;
        blockSpacing = letterGap / (float)count;

        Debug.Log($"Correct string list count: {count}");

        for (int i = 0; i < count; i++)
        {
            char character = correctStringList[i];

            if (letterBlockDictionary.TryGetValue(character, out GameObject letterPrefab) && letterPrefab != null)
            {
                // Calculate new position
                Vector3 newPosition = startPosition + new Vector3((i +0.5f) * blockSpacing, 0, 0); // Adjust spacing on X-axis
                
                // Instantiate at calculated position and track them in a list
                GameObject instance = Instantiate(letterPrefab, newPosition, Quaternion.identity, this.transform);
                instantiatedBlocks.Add(instance);

                Debug.Log($"Instantiated {character} at {newPosition}");
            }
            else
            {
                Debug.LogWarning($"Letter '{character}' not found in dictionary!");
            }
        }

        // Resets values that need to change upon a reinstantiation of the blocks
        correctCounter = 0;
        blockActionTimer = 0.0f;
        pointsChanged = false;
        allowInputs = true;
        sequenceTimer = timerReference;
    }

    public void DestroyLetterBlocks()
    {
        foreach (GameObject obj in instantiatedBlocks)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        instantiatedBlocks.Clear();
    }

    // Manages the updating of the score text and position/scale of the score meter itself
    public void UpdateScoreUI()
    {
        scoreText.text = Mathf.Round(this.score).ToString();
        float scorePercentage = (score / maxScore);
        scoreMeter.transform.localScale = new Vector3(220, 204 * scorePercentage, 0);
        scoreMeter.transform.position = new Vector3(meterBacking.transform.position.x,
            (meterBacking.transform.position.y + Mathf.Lerp(meterAdjustment, 0.0f, scorePercentage)), meterBacking.transform.position.z);
    }
}

