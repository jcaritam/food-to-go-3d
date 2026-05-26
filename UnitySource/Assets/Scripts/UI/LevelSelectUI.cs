using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public LevelButtonVisual visual;
        public int levelIndex;
    }

    [SerializeField] private LevelButton[] levelButtons;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float scaleSpeed = 12f;
    [SerializeField] private GameObject loadingIndicator;

    private int selectedIndex = 0;
    private float[] targetScales;
    private bool ready = false;

    private void Start()
    {
        targetScales = new float[levelButtons.Length];
        for (int i = 0; i < targetScales.Length; i++)
            targetScales[i] = normalScale;

        SetButtonsInteractable(false);
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        StartCoroutine(WaitForService());
    }

    private IEnumerator WaitForService()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while ((CloudProgressService.Instance == null || !CloudProgressService.Instance.IsReady) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        SetButtonsInteractable(true);
        yield return RefreshAllAsync();
        SelectButton(0);
        ready = true;
    }

    private System.Collections.IEnumerator RefreshAllAsync()
    {
        if (CloudProgressService.Instance != null)
        {
            foreach (var lb in levelButtons)
            {
                var task = CloudProgressService.Instance.LoadLevelRecordAsync(lb.levelIndex);
                yield return new WaitUntil(() => task.IsCompleted);
            }
        }
        RefreshLockStates();
    }

    private void Update()
    {
        if (!ready) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveSelection(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveSelection(-1);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            ActivateSelected();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Transform t = levelButtons[i].button.transform;
            float current = t.localScale.x;
            float next = Mathf.Lerp(current, targetScales[i], Time.deltaTime * scaleSpeed);
            t.localScale = Vector3.one * next;
        }
    }

    private void MoveSelection(int direction)
    {
        int next = selectedIndex + direction;
        if (next < 0 || next >= levelButtons.Length) return;
        SelectButton(next);
    }

    private void SelectButton(int index)
    {
        for (int i = 0; i < levelButtons.Length; i++)
            targetScales[i] = i == index ? selectedScale : normalScale;

        selectedIndex = index;
        EventSystem.current.SetSelectedGameObject(levelButtons[index].button.gameObject);
    }

    private void ActivateSelected()
    {
        var lb = levelButtons[selectedIndex];
        if (lb.button.interactable)
            lb.button.onClick.Invoke();
    }

    public void RefreshLockStates()
    {
        foreach (var lb in levelButtons)
        {
            bool unlocked = LevelProgressData.IsLevelUnlocked(lb.levelIndex);
            if (lb.visual != null)
            {
                lb.visual.SetLocked(!unlocked);
                if (unlocked)
                {
                    var record = LevelProgressData.GetLevelRecord(lb.levelIndex);
                    lb.visual.SetStars(record.stars);
                }
            }
        }
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (var lb in levelButtons)
            lb.button.interactable = value;
    }
}
