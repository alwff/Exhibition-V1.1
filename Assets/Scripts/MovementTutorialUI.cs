using System.Collections;
using UnityEngine;

public class MovementTutorialUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject tutorialPanel;
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    public float fadeInDuration = 0.35f;
    public float visibleDuration = 4.0f;
    public float fadeOutDuration = 0.65f;

    private Coroutine tutorialCoroutine;

    private void Start()
    {
        ShowTutorial();
    }

    public void ShowTutorial()
    {
        if (tutorialCoroutine != null)
            StopCoroutine(tutorialCoroutine);

        tutorialPanel.SetActive(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        tutorialCoroutine = StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // Fade in
        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.unscaledDeltaTime;

            canvasGroup.alpha =
                Mathf.Clamp01(time / fadeInDuration);

            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Visible
        yield return new WaitForSecondsRealtime(visibleDuration);

        // Fade out
        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;

            canvasGroup.alpha =
                1f - Mathf.Clamp01(time / fadeOutDuration);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        tutorialPanel.SetActive(false);

        tutorialCoroutine = null;
    }
}