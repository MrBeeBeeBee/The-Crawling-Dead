using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using static UnityEngine.ParticleSystem;

public class InsideOutGameManager : MonoBehaviour
{
    public static InsideOutGameManager instance;
    public GameObject miniGame;
    public Defender[] defenders;
    public InsideOutPlayer larva;
    public Transform pellets;
    public Variant currentVariant;
    public bool isGameActive = false;
    public float timeRemaining = 30.0f;
    public float pupationBonus = 1.0f;
    public float maxMultiplier = 2.4f;
    public int pelletsEaten;
    public int defendersEaten;
    public bool diedRecently = false;
    public Animator larvaAnimator;

    [Header("UI Elements")]
    public GameObject startMenu;
    public GameObject endMenu;
    public GameObject infoMenu;
    public GameObject UI;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private GameObject lifeOne;
    [SerializeField] private GameObject lifeTwo;
    [SerializeField] private GameObject lifeThree;
    [SerializeField] private GameObject lifeFour;
    [SerializeField] private TMP_Text pelletsEatenText;
    [SerializeField] private TMP_Text defendersEatenText;
    [SerializeField] private TMP_Text livesLeftText;
    public GameObject arrowLB;
    public GameObject arrowLT;
    public GameObject arrowRB;
    public GameObject arrowRT;

    [Header("Audio Elements")]
    public AudioSource larvaDyingSFX;
    public AudioSource defenderEatenSFX;
    public AudioSource smallPelletEatenSFX;
    public AudioSource largePelletEatenSFX;

    // if someone needs to, the score and lives can be found and retrieved,
    // but are only manually set through in-game actions
    public float score { get; private set; }
    public int lives { get; private set; } = 3;
    public float defenderMultiplier { get; private set; } = 1.0f;

    [Header("Traits")]
    [SerializeField]
    private GameObject intPupationTrait;
    [SerializeField]
    private Sprite intPupationActive;
    [SerializeField]
    private Sprite intPupationInactive;
    [SerializeField]
    private TMP_Text intPupationText;

    [SerializeField]
    private GameObject extPupationTrait;
    [SerializeField]
    private Sprite extPupationActive;
    [SerializeField]
    private Sprite extPupationInactive;
    [SerializeField]
    private TMP_Text extPupationText;

    public GameObject ruleTileScene;
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        // Shenanigans from Inside Out's Rule Tiles requires it be active on the first frame
        ruleTileScene.SetActive(false);
    }

    
    public void OnEnable()
    {
        if(MiniGameHandler.instance != null && MiniGameHandler.instance.variant != null)
        {
            currentVariant = MiniGameHandler.instance.variant;
            larva.traits = currentVariant.traits;
            if(currentVariant.traits != null)
            {
                IntroTextHandler();
            }
            // Enable Start Menu
            startMenu.SetActive(true);
            // Disable End Menu
            endMenu.SetActive(false);
            // Set point worth according to traits
            
            //Instantiate new game
            SetScore(0);
            PelletAlignment();
            pelletsEaten = 0;
            defendersEaten = 0;
        }
    }

    // Counts down timer
    public void Update()
    {
        if (isGameActive && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = Mathf.Round(timeRemaining).ToString();
            scoreText.text = Mathf.Round(this.score).ToString();

            LifeUIUpdater();

            if (this.larva.empowered)
            {
                arrowLB.SetActive(true);
                arrowLT.SetActive(true);
                arrowRB.SetActive(true);
                arrowRT.SetActive(true);
            }
            else
            {
                arrowLB.SetActive(false);
                arrowLT.SetActive(false);
                arrowRB.SetActive(false);
                arrowRT.SetActive(false);
            }
        }
        else if (isGameActive && timeRemaining <= 0)
        {
            GameOver();
        }
    }

    public void StartButton()
    {
        // Start timer for minigame
        isGameActive = true;
        // Close down start menu
        startMenu.SetActive(false);

        // Turn on UI elements - TODO
        UI.SetActive(true);

        // Enable movement
        if (larva != null)
        {
            larva.movement.StartGame();
        }
        for (int i = 0; i < this.defenders.Length;i++)
        {
            // Call function in Defender script that enables start behavior
            this.defenders[i].ResetState();
            this.defenders[i].movement.StartGame();
        }
    }

    public void IntroTextHandler()
    {
        SpriteRenderer internalSprite = intPupationTrait.GetComponent<SpriteRenderer>();
        SpriteRenderer externalSprite = extPupationTrait.GetComponent<SpriteRenderer>();

        string intro = $"<b>" + currentVariant.title + "</b> has found many " +
            $"hosts within a meadow-like field. Your job is to eat all of the fat body to complete " +
            $"development and not kill the host too soon. As a wasp larva, you must also avoid the " +
            $"immune system of the host or you will be eliminated.";
        
        if (larva.traits["InternalP"])
        {
            internalSprite.sprite = intPupationActive;
            intPupationText.alpha = 1f;
            SetLives(4);
        }
        else
        {
            internalSprite.sprite = intPupationInactive;
            intPupationText.alpha = 0.5f;
            SetLives(3);
        }
        if (larva.traits["ExternalP"])
        {
            externalSprite.sprite = extPupationActive;
            extPupationText.alpha = 1f;
            pupationBonus = 1.25f;
        }
        else
        {
            externalSprite.sprite = extPupationInactive;
            extPupationText.alpha = 0.5f;
            pupationBonus = 1.0f;
        }
            introText.text = intro;
    }

    public void QuitButton()
    {
        // Award rewards
        GameManager.instance.genomePoints += (int) score;
        // Enable Start Menu
        startMenu.SetActive(true);
        // Disable End Menu
        endMenu.SetActive(false);
        // Set the minigame to inactive
        miniGame.SetActive(false);
        // Reset timer
        timeRemaining = 30.0f;
        // Pause timer for minigame
        isGameActive = false;
        // Unpause global timer
        GlobalTimer.Instance.isPaused = false;
        // Allow the player to interact with the main game
        MiniGameHandler.instance.EnableMainGameInteractables();
        this.larva.ResetState();
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

    // Instantiates all pellets and resets entity positions
    public void PelletAlignment()
    {
        //Debug.Log("New Round");
        foreach (Transform pellet in this.pellets)
        {
            pellet.gameObject.SetActive(true);
        }
    }

    // Resets defender multiplier as well as position of larva and defenders
    public void DeathReset()
    {
        if(isGameActive)
        {
            this.larva.gameObject.SetActive(false);
            ResetDefenderMultiplier();

            for (int i = 0; i < this.defenders.Length; i++)
            {
                this.defenders[i].ResetState();
                this.defenders[i].movement.StartGame();
            }

            this.larva.ResetState();
            larva.movement.StartGame();
            diedRecently = false;
        }
    }

    // Closes down the minigame
    public void GameOver()
    {
        //Debug.Log("Game Over");
        if (larva != null)
        {
            larva.movement.EndGame();
        }
        for (int i = 0; i < this.defenders.Length; i++)
        {
            // Call function in Defender script that enables start behavior
            this.defenders[i].movement.EndGame();
        }

        // Pause game activities
        isGameActive = false;
        // Set UI to inactive
        UI.SetActive(false);
        endMenu.SetActive(true);
        // End Text Here
        finalScoreText.text = this.score.ToString();
        pelletsEatenText.text = this.pelletsEaten.ToString();
        defendersEatenText.text = this.defendersEaten.ToString();
        livesLeftText.text = this.lives.ToString();
    }

    private void SetScore(float score)
    {
        this.score = score;
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
    }

    // Adds points for killing a defender and increases points for killing more
    // in a short amount of time.
    public void DefenderDies(Defender defender)
    {
        defenderEatenSFX.Play();
        float points = defender.defenderPoints * this.defenderMultiplier;
        SetScore(this.score + (defender.defenderPoints * this.defenderMultiplier));
        defendersEaten++;

        this.defenderMultiplier += 0.6f;
        if (this.defenderMultiplier >= maxMultiplier)
            this.defenderMultiplier = maxMultiplier;
    }

    // Decrements the amount of lives and resets entity positions
    public void LarvaDies()
    {
        if (!diedRecently)
        {
            SetLives(this.lives - 1);
            larvaAnimator.Play("LarvaDead", 0);
            larvaDyingSFX.Play();

            if (larva != null)
            {
                larva.movement.EndGame();
            }
            for (int i = 0; i < this.defenders.Length; i++)
            {
                this.defenders[i].movement.EndGame();
            }

            if (this.lives > 0)
            {
                Invoke(nameof(DeathReset), 3.0f);
            }
            else
            {
                GameOver();
            }
        }

        diedRecently = true;
    }

    // Awards points for eating a pellet
    public void PelletEaten(Pellet pellet)
    {
        smallPelletEatenSFX.Play();
        pellet.gameObject.SetActive(false);
        pelletsEaten++;

        SetScore(this.score + Mathf.Round(pellet.points * pupationBonus));

        if (!HasRemainingPellets())
        {
            this.larva.gameObject.SetActive(false);
            Invoke(nameof(GameOver), 3.0f);
        }
    }

    // Awards points for eating a large pellet and puts defenders in scared mode
    public void LargePelletEaten(LargePellet pellet)
    {
        largePelletEatenSFX.Play();
        for (int i = 0; i < this.defenders.Length; i++)
        {
            this.defenders[i].scared.Enable(pellet.aggressionDuration);
        }
        PelletEaten(pellet);
        this.larva.empowered = true;

        // If duration timer is already in progress when another large pellet is consumed,
        // the timer is renewed.
        CancelInvoke();
        Invoke(nameof(ResetDefenderMultiplier), pellet.aggressionDuration);
    }

    // Checks for any remaining pellets, returns true if it finds even one, returns false if
    // whole list has been iterated through and no active pellet was found
    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in this.pellets)
        {
            if (pellet.gameObject.activeSelf)
                return true;
        }

        return false;
    }

    private void ResetDefenderMultiplier()
    {
        this.defenderMultiplier = 1.0f;
    }

    private void LifeUIUpdater()
    {
        if (this.lives >= 4)
        {
            lifeFour.SetActive(true);
        }
        else
            lifeFour.SetActive(false);

        if (this.lives >= 3)
        {
            lifeThree.SetActive(true);
        }
        else
            lifeThree.SetActive(false);

        if (this.lives >= 2)
        {
            lifeTwo.SetActive(true);
        }
        else
            lifeTwo.SetActive(false);

        if (this.lives >= 1)
        {
            lifeOne.SetActive(true);
        }
        else
            lifeOne.SetActive(false);
    }
}
