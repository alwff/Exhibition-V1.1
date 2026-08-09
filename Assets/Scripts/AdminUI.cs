using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdminUI : MonoBehaviour
{
    public GameObject adminCanvas;
    public TextMeshProUGUI instructionsText;
    [Header("Assignment Panel")]
    public TextMeshProUGUI selectedSpecimenText;
    public TextMeshProUGUI selectedSlotText;
    public GameObject resetConfirmation;
    public Button assignButton;
    public GalleryManager galleryManager;
    public SpecimenAPIClient apiClient;
    public Transform contentParent;
    public GameObject specimenCardPrefab;
    public TMP_Dropdown collectionDropdown;
    public Transform slotContentParent;
    public GameObject slotButtonPrefab;
    public SpecimenViewerUI specimenViewer;
    private int currentSlot = -1;
    private SpecimenCardUI selectedCard = null;
    private LoadedSpecimen selectedSpecimen;

    private readonly Vector2[] slotMapPositions =
    {
        // Norte
        new Vector2(-300f,  62f),   // Slot 1
        new Vector2(   0f,  62f),   // Slot 2
        new Vector2( 300f,  62f),   // Slot 3

        // Este
        new Vector2( 540f,  45f),   // Slot 4
        new Vector2( 540f,   0f),   // Slot 5
        new Vector2( 540f, -45f),   // Slot 6

        // Sur
        new Vector2( 300f, -62f),   // Slot 7
        new Vector2(   0f, -62f),   // Slot 8 
        new Vector2(-300f, -62f),   // Slot 9

        // Oeste
        new Vector2(-540f, -45f),   // Slot 10
        new Vector2(-540f,   0f),   // Slot 11
        new Vector2(-540f,  45f),   // Slot 12

        // Centro 
        new Vector2(-65f,   0f),    // Slot 13
        new Vector2(-155f,  0f),    // Slot 14
        new Vector2( 65f,   0f),    // Slot 15
        new Vector2(155f,   0f)     // Slot 16
    };

    void Start()
    {
        // Crear selector de cuadros
        for (int i = 0; i < galleryManager.slots.Length; i++)
        {
            GameObject slotObj = Instantiate(
                slotButtonPrefab,
                slotContentParent
            );

            slotObj
                .GetComponent<SlotButtonUI>()
                .Setup(i, OnSlotSelected);

            RectTransform rect =
                slotObj.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(72f, 28f);

            if (i < slotMapPositions.Length)
                rect.anchoredPosition = slotMapPositions[i];
        }

        // Cargar lista de especímenes
        StartCoroutine(

            apiClient.GetAllSpecimens(

                data =>
                {
                    HashSet<string> collections =
                        new HashSet<string>();

                    foreach (var s in data)
                    {
                        collections.Add(s.collection);

                        GameObject card =
                            Instantiate(
                                specimenCardPrefab,
                                contentParent
                            );

                        StartCoroutine(

                            apiClient.LoadCompleteSpecimen(

                                s.id,

                                loaded =>
                                {
                                    card
                                        .GetComponent<SpecimenCardUI>()
                                        .Setup(
                                            loaded.data.id,
                                            loaded.data.name,
                                            loaded.data.collection,
                                            loaded.preview,
                                            OnSpecimenSelected,
                                            SelectCard
                                        );
                                }

                            )

                        );
                    }

                    SetupDropdown(collections);
                    UpdateUI();
                }

            )

        );

        UpdateAssignmentPanel();
    }

    void SelectCard(SpecimenCardUI card)
    {
        if (selectedCard != null)
            selectedCard.GetComponent<Image>().color = Color.white;

        selectedCard = card;

        selectedCard.GetComponent<Image>().color =
            new Color(0.7f, 0.9f, 1f);
    }

    void OnSpecimenSelected(string id)
    {
        StartCoroutine(

            apiClient.LoadCompleteSpecimen(

                id,

                specimen =>
                {
                    selectedSpecimen = specimen;

                    if (selectedSpecimenText != null)
                        selectedSpecimenText.text = specimen.data.name;

                    specimenViewer.Show(specimen);

                    UpdateAssignmentPanel();
                }

            )

        );

    }

    public void AssignSelectedSpecimen()
    {
        if (selectedSpecimen == null)
        {
            Debug.LogWarning("No hay espécimen seleccionado.");
            return;
        }

        if (currentSlot < 0)
        {
            Debug.LogWarning("No hay cuadro seleccionado.");
            return;
        }

        galleryManager.AssignSpecimen(
            currentSlot,
            selectedSpecimen
        );

        Debug.Log(
            $"Specimen '{selectedSpecimen.data.id}' asignado al cuadro {currentSlot + 1}"
        );
    }

    public void ClearSelectedSlot()
    {
        if (currentSlot < 0)
        {
            Debug.LogWarning("No hay cuadro seleccionado.");
            return;
        }

        galleryManager.ClearSlot(currentSlot);

        Debug.Log(
            $"Cuadro {currentSlot + 1} limpiado."
        );
    }

    public void OpenResetConfirmation()
    {
        if (resetConfirmation != null)
            resetConfirmation.SetActive(true);
    }

    public void CloseResetConfirmation()
    {
        if (resetConfirmation != null)
            resetConfirmation.SetActive(false);
    }

    public void ConfirmResetGallery()
    {
        galleryManager.ClearAll();

        CloseResetConfirmation();

        Debug.Log("Galería completa restablecida.");
    }

    void OnSlotSelected(int index)
    {
        currentSlot = index;

        if (selectedSlotText != null)
            selectedSlotText.text = "Slot " + (currentSlot + 1);

        UpdateUI();
        UpdateAssignmentPanel();
    }

    void SetupDropdown(HashSet<string> collections)
    {
        List<string> options = new List<string>();
        options.Add("All");
        options.AddRange(collections);

        collectionDropdown.ClearOptions();
        collectionDropdown.AddOptions(options);

        collectionDropdown.onValueChanged.AddListener(OnCollectionChanged);
    }

    void OnCollectionChanged(int index)
    {
        string selected = collectionDropdown.options[index].text;

        foreach (Transform child in contentParent)
        {
            var card = child.GetComponent<SpecimenCardUI>();

            if (card != null)
            {
                bool match = selected == "All" || card.collection == selected;
                child.gameObject.SetActive(match);
            }
        }
    }

    void UpdateUI()
    {
        instructionsText.text =
            "Selecciona un cuadro en el mapa para comenzar con la asignación.";
    }

    void UpdateAssignmentPanel()
    {
        bool hasSpecimen = selectedSpecimen != null;
        bool hasSlot = currentSlot >= 0;

        if (selectedSpecimenText != null && !hasSpecimen)
            selectedSpecimenText.text = "Ningún espécimen seleccionado";

        if (selectedSlotText != null && !hasSlot)
            selectedSlotText.text = "Ningún cuadro seleccionado";

        if (assignButton != null)
            assignButton.interactable = hasSpecimen && hasSlot;
    }
}