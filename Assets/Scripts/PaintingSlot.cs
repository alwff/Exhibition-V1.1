using System.Collections;
using UnityEngine;
using SUPERCharacter;

public class PaintingSlot : MonoBehaviour, IInteractable
{
    public Image360Viewer viewer;
    public string specimenID;

    public Renderer targetRenderer;
    public SpecimenAPIClient apiClient;

    private LoadedSpecimen loadedSpecimen;
    private bool isLoadingImages = false;

    private int materialIndex = -1;

    void Start()
    {

        if (string.IsNullOrEmpty(specimenID))
            return;

        if (apiClient == null)
            return;

        StartCoroutine(

            apiClient.LoadSpecimenPreview(

                specimenID,

                specimen =>
                {
                    loadedSpecimen = specimen;

                    apiClient.ApplyPreview(
                        loadedSpecimen,
                        targetRenderer,
                        ref materialIndex
                    );
                }

            )

        );
    }

    public bool CanInteract()
    {
        return viewer != null &&
            apiClient != null &&
            !string.IsNullOrEmpty(specimenID) &&
            loadedSpecimen != null;
    }

    public bool Interact()
    {
        if (
            viewer == null ||
            apiClient == null
        )
        {
            Debug.LogError(
                "Falta asignar viewer o apiClient"
            );

            return false;
        }


        if (
            string.IsNullOrEmpty(specimenID) ||
            loadedSpecimen == null
        )
        {
            Debug.LogWarning(
                "El espécimen todavía no está disponible."
            );

            return false;
        }


        // La secuencia ya está cargada.
        if (
            loadedSpecimen.imagesReady &&
            loadedSpecimen.images != null &&
            loadedSpecimen.images.Length > 0
        )
        {
            viewer.Show(
                loadedSpecimen
            );

            return true;
        }


        // Evitar iniciar varias descargas si el
        // usuario interactúa repetidamente.
        if (isLoadingImages)
        {
            Debug.Log(
                "La secuencia todavía se está cargando."
            );

            return true;
        }


        StartCoroutine(
            LoadImagesAndOpenViewer()
        );

        return true;
    }

    private IEnumerator LoadImagesAndOpenViewer()
    {
        isLoadingImages = true;

        viewer.ShowLoading();

        LoadedSpecimen completeSpecimen = null;


        yield return apiClient.LoadCompleteSpecimen(

            specimenID,

            specimen =>
            {
                completeSpecimen = specimen;
            }

        );


        isLoadingImages = false;

        viewer.HideLoading();


        if (
            completeSpecimen == null ||
            completeSpecimen.images == null ||
            completeSpecimen.images.Length == 0
        )
        {
            Debug.LogWarning(
                "No fue posible cargar la secuencia del espécimen."
            );

            yield break;
        }


        loadedSpecimen =
            completeSpecimen;


        viewer.Show(
            loadedSpecimen
        );
    }

    public void SetSpecimen(LoadedSpecimen specimen)
    {
        loadedSpecimen = specimen;

        if (loadedSpecimen == null)
            return;

        apiClient.ApplyPreview(
            loadedSpecimen,
            targetRenderer,
            ref materialIndex
        );

        specimenID = loadedSpecimen.data.id;
    }

    public void ClearSpecimen()
    {
        loadedSpecimen = null;
        specimenID = "";
        isLoadingImages = false;

        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.materials;

        if (materialIndex < 0)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.ToLower().Contains("picture"))
                {
                    materialIndex = i;
                    break;
                }
            }

            if (materialIndex < 0 && mats.Length > 1)
                materialIndex = 1;
        }

        if (materialIndex >= 0 && materialIndex < mats.Length)
        {
            mats[materialIndex].mainTexture = null;
            targetRenderer.materials = mats;
        }

        Debug.Log($"Cuadro '{gameObject.name}' limpiado.");
    }

}