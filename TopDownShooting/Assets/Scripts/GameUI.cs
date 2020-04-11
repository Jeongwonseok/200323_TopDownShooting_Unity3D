using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    // Wave UI
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
	public Text newWaveLifeCount;

    // Score UI
    public Text scoreUI;
    public Text gameOverScoreUI;
    public Text comboUI;

    // HP UI
    public RectTransform healthBar;

    // Pause UI
    public GameObject pauseUI;

    // Info UI
    public GameObject InfoUI;

    ScoreKeeper keeper;

    Spawner spawner;
    Player player;

    private bool isPause;

    void Start()
    {
        pauseUI.SetActive(false);
        InfoUI.SetActive(false);

        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
        keeper = GetComponent<ScoreKeeper>();
    }

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Update()
    {
        // Score & HP 갱신
        scoreUI.text = ScoreKeeper.score.ToString("D6");
        float healthPercent = 0;
        if(player != null)
        { 
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);

        // Combo 갱신
        comboUI.text = ScoreKeeper.streakCount.ToString();

        OnPause();
    }

    public void OnPause()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            isPause = !isPause;
        }

        if(isPause)
        {
            Cursor.visible = true;
            pauseUI.SetActive(true);
            Time.timeScale = 0;
        }

        if (!isPause)
        {
            pauseUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void Info()
    {
        pauseUI.SetActive(false);
        InfoUI.SetActive(true);
    }

    public void Cancel()
    {
        Cursor.visible = false;
        isPause = !isPause;
    }

    public void Cancel_info()
    {
        InfoUI.SetActive(false);
        pauseUI.SetActive(true);
    }

    // 웨이브 전환 UI
    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "");
        newWaveEnemyCount.text = "Enemies Count: " + enemyCountString;
		string playerHP = spawner.waves[waveNumber - 1].hitsToKillPlayer + "";
		newWaveLifeCount.text = "Player HP: " + playerHP;

		StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    // 게임 오버 실행 메서드
    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color(0,0,0,0.95f), 1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
    }

    // Wave UI Animation
    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while(animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if(animatePercent >= 1)
            {
                animatePercent = 1;
                if(Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 45, animatePercent);
            yield return null;
        }

    }

    // 페이드 아웃 구현 코루틴
    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    // Game return
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    // Menu return
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
