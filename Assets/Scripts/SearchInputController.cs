using UnityEngine;
using TMPro;

public class SearchInputController : MonoBehaviour
{
    public TMP_InputField searchInput;
    public Transform contentParent;

    void Start()
    {
        searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    void OnSearchChanged(string text)
    {
        string lower = text.ToLower();

        foreach (Transform child in contentParent)
        {
            var card = child.GetComponent<SpecimenCardUI>();

            if (card != null)
            {
                bool match = card.nameText.text.ToLower().Contains(lower);
                child.gameObject.SetActive(match);
            }
        }
    }
}