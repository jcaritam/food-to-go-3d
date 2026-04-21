using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private RectTransform logoTransform;
    [SerializeField] private TextMeshProUGUI menuCursor;
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private int _selectedIndex = 0;

    private void Awake()
    {
        if (playButton == null)
        {
            Debug.LogError("[MainMenuUI] playButton no asignado en el Inspector.");
            return;
        }

        if (quitButton == null)
        {
            Debug.LogError("[MainMenuUI] quitButton no asignado en el Inspector.");
            return;
        }

        playButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.LevelSelectScene);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void Start()
    {
        if (logoTransform != null)
            StartCoroutine(ScaleBounce(logoTransform, duration: 0.6f, overshoot: 1.15f));

        if (playButton != null)
            StartCoroutine(ScaleBounce(playButton.GetComponent<RectTransform>(), duration: 0.5f, overshoot: 1.2f, delay: 0.3f));

        UpdateCursorPosition();
    }

    private void Update()
    {
        bool downPressed = Keyboard.current != null &&
            (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame);
        bool upPressed = Keyboard.current != null &&
            (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame);
        bool confirmPressed = Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        if (downPressed)
        {
            _selectedIndex = (_selectedIndex + 1) % 2;
            UpdateCursorPosition();
        }
        else if (upPressed)
        {
            _selectedIndex = ((_selectedIndex - 1) + 2) % 2;
            UpdateCursorPosition();
        }

        if (confirmPressed)
        {
            if (_selectedIndex == 0)
                playButton.onClick.Invoke();
            else
                quitButton.onClick.Invoke();
        }
    }

    private void UpdateCursorPosition()
    {
        if (menuCursor == null) return;

        Button targetButton = _selectedIndex == 0 ? playButton : quitButton;
        if (targetButton == null) return;

        RectTransform buttonRect = targetButton.GetComponent<RectTransform>();
        RectTransform cursorRect = menuCursor.GetComponent<RectTransform>();

        Vector2 buttonPos = buttonRect.anchoredPosition;
        cursorRect.anchoredPosition = new Vector2(
            buttonPos.x - buttonRect.rect.width * 0.5f - 10f,
            buttonPos.y
        );
    }

    private IEnumerator ScaleBounce(RectTransform target, float duration, float overshoot, float delay = 0f)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        target.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scale = t < 0.7f
                ? Mathf.Lerp(0f, overshoot, t / 0.7f)
                : Mathf.Lerp(overshoot, 1f, (t - 0.7f) / 0.3f);

            target.localScale = Vector3.one * scale;
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}
