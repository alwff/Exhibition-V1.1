using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdminUI : MonoBehaviour
{
    public GameObject adminCanvas;
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI instructionsText;
    public GalleryManager galleryManager;
    public SpecimenAPIClient apiClient;
    public Transform contentParent;
    public GameObject specimenCardPrefab;
    public TMP_Dropdown collectionDropdown;
    public Transform slotContentParent;
    public GameObject slotButtonPrefab;
    public SpecimenViewerUI specimenViewer;
    private int currentSlot = 0;
    private SpecimenCardUI selectedCard = null;
    private LoadedSpecimen selectedSpecimen;

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

                    specimenViewer.Show(specimen);
                }

            )

        );

    }

    public void AssignSelectedSpecimen()
    {
        if (selectedSpecimen == null)
        {
            Debug.LogWarning("No hay espécimen cargado.");
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

    void OnSlotSelected(int index)
    {
        currentSlot = index;
        UpdateUI();
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
        slotText.text = "Cuadro: " + (currentSlot + 1);

        instructionsText.text =
            "Click: seleccionar espécimen\n" +
            "Selecciona slot abajo\n" +
            "ESC: salir";
    }
}