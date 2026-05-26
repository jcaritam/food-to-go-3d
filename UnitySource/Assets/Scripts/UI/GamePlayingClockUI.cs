using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayingClockUI : MonoBehaviour
{
    [Header("Visual components")]
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Animator clockAnimator;

    [Header("Configuración de Animación")]
    [Tooltip("El valor normalizado (0 a 1) donde empieza a parpadear")]
    [SerializeField] private float warningThreshold = 0.25f; 
    [Tooltip("El valor normalizado (0 a 1) donde se sacude violentamente")]
    [SerializeField] private float criticalThreshold = 0.10f;
    
    private int lastSecondRemaining = -1;
    private bool isWarning = false;
    private bool isCritical = false;

    private void Start()
    {
        // 1. Nos aseguramos de que el texto empiece de color blanco
        if(timeText != null)
        {
            timeText.color = Color.white;   
        }
    }

    private void Update()
    {
        float timeRemaining = KitchenGameManager.Instance.GetGamePlayingTimer();
        int secondRemaining = Mathf.CeilToInt(timeRemaining);

        if (secondRemaining != lastSecondRemaining)
        {
            lastSecondRemaining = secondRemaining;
            UpdateTextDisplay(secondRemaining);
        }

        // Solo calculamos alertas y animaciones SI el juego ya empezó a jugarse
        if (KitchenGameManager.Instance.IsGamePlaying())
        {
            float maxTime = KitchenGameManager.Instance.GetGamePlayingTimerMax();
            float remainingPercentage = timeRemaining / maxTime;

            HandleAnimations(remainingPercentage);
        }
    }

    private void UpdateTextDisplay(int totalSeconds)
    {
        if (timeText == null) return;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void HandleAnimations(float remainingPercentage)
    {
        if (clockAnimator == null || timeText == null) return;

        // Si el porcentaje restante es menor o igual a 10%
        if (remainingPercentage <= criticalThreshold && !isCritical)
        {
            isCritical = true;
            clockAnimator.SetTrigger("IsCritical");
            timeText.color = Color.red;
        } 
        // Si el porcentaje restante es menor o igual a 25% (pero mayor que 10%)
        else if (remainingPercentage <= warningThreshold && remainingPercentage > criticalThreshold && !isWarning)
        {
            isWarning = true;
            clockAnimator.SetTrigger("IsWarning");
            timeText.color =  new Color(1f, 0.5f, 0f); // Naranja
        }
    }
}