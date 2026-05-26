using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }
    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    [SerializeField] public int levelId = 1;
    [SerializeField] public LevelConfigSO levelConfig;

    private enum State
    {
        WAITING_TO_START,
        COUNTDOWN_TO_START,
        GAME_PLAYING,
        GAME_OVER
    }

    private State state;
    private bool isGamePaused = false;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 180f;

    private void Awake()
    {
        state = State.WAITING_TO_START;
        gamePlayingTimer  = gamePlayingTimerMax;
        Instance = this;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WAITING_TO_START:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f)
                {
                    state = State.COUNTDOWN_TO_START;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.COUNTDOWN_TO_START:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GAME_PLAYING;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GAME_PLAYING:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GAME_OVER;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GAME_OVER:
                break;
        }
    }

    public void TogglePause()
    {
        if (!IsGamePlaying() && !IsCountdownToStartActive()) return;
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsGamePaused() => isGamePaused;
    public bool IsGamePlaying() => state == State.GAME_PLAYING;
    public bool IsCountdownToStartActive() => state == State.COUNTDOWN_TO_START;
    public bool IsGameOver() => state == State.GAME_OVER;
    public float GetCountdownToStartTimer() => countdownToStartTimer;
    public float GetGamePlayingTimerNormalized() => 1 - (gamePlayingTimer / gamePlayingTimerMax);
    public float GetGamePlayingTimerMax() => gamePlayingTimerMax;

    public float GetGamePlayingTimer() => gamePlayingTimer;
}
