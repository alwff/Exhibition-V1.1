using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SpecimenCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage preview;
    public TextMeshProUGUI nameText;
    public Button button;

    public string collection;

    private string specimenID;
    private Vector3 originalScale;
    private bool isHovered = false;

    private System.Action<SpecimenCardUI> onSelected;

    void Start()
    {
        originalScale = transform.localScale;
        
    }

    public void Setup(
        string id,
        string name,
        string collectionParam,
        Texture2D image,
        System.Action<string> onClick,
        System.Action<SpecimenCardUI> onSelectedCallback)
    {
        specimenID = id;

        collection = collectionParam;

        nameText.text = name;

        if (image != null)
            preview.texture = image;

        onSelected = onSelectedCallback;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            onSelected?.Invoke(this);
            onClick?.Invoke(specimenID);
        });
    }

    void Update()
    {
        float target = isHovered ? 1.08f : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale * target, Time.deltaTime * 10f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        GetComponent<Image>().color = new Color(0.9f, 0.95f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        GetComponent<Image>().color = Color.white;
    }
}