using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpecimenViewerUI : MonoBehaviour
{
    [Header("UI")]

    public RawImage previewImage;

    public TextMeshProUGUI nameText;

    public TextMeshProUGUI descriptionText;

    public TextMeshProUGUI portalText;

public TextMeshProUGUI collectionText;

public TextMeshProUGUI statusText;

    public void Show(LoadedSpecimen specimen)
    {
        if (specimen == null)
            return;

        if (previewImage != null)
        {
            previewImage.texture = specimen.preview;
            previewImage.color = Color.white;
        }

        if (nameText != null)
            nameText.text = specimen.data.name;

        if (descriptionText != null)
            descriptionText.text = specimen.data.description;

        ShowMetadata(specimen);

        StartCoroutine(FadeIn());
    }


    IEnumerator FadeIn()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();

        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }

        group.alpha = 0;

        float t = 0;

        while (t < 0.25f)
        {
            t += Time.deltaTime;

            group.alpha = Mathf.Lerp(0, 1, t / 0.25f);

            yield return null;
        }

        group.alpha = 1;
    }

    public void ShowMinimal(LoadedSpecimen specimen)
    {
        if (specimen == null)
            return;

        if (previewImage != null)
            previewImage.texture = specimen.preview;

        if (nameText != null)
            nameText.text = specimen.data.name;

        if (descriptionText != null)
            descriptionText.text = "";
    }

    public void ShowMetadata(LoadedSpecimen specimen)
    {
        if (specimen == null)
            return;

        if (portalText != null)
            portalText.text = specimen.data.id;

        if (collectionText != null)
            collectionText.text = specimen.data.collection;

        if (statusText != null)
            statusText.text = specimen.data.status;
    }
    

    public void Clear()
    {
        if (previewImage != null)
        {
            previewImage.texture = null;
            previewImage.color = Color.clear;
        }
        
        if (nameText != null)
            nameText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        if (portalText != null)
            portalText.text = "";

        if (collectionText != null)
            collectionText.text = "";

        if (statusText != null)
            statusText.text = "";
    }
}