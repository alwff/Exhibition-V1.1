using UnityEngine;
using SUPERCharacter;

public class PaintingSlot : MonoBehaviour, IInteractable
{
    public Image360Viewer viewer;
    public string specimenID;

    public Renderer targetRenderer;
    public SpecimenAPIClient apiClient;

    private LoadedSpecimen loadedSpecimen;

    private int materialIndex = -1;

    void Start()
    {
        if (string.IsNullOrEmpty(specimenID))
            return;

        if (apiClient == null)
            return;

        StartCoroutine(

            apiClient.LoadCompleteSpecimen(

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

    public bool Interact()
    {
        if (viewer == null || apiClient == null)
        {
            Debug.LogError("Falta asignar viewer o apiClient");
            return false;
        }

        if (string.IsNullOrEmpty(specimenID))
        {
            Debug.LogError("ID inválido");
            return false;
        }

        if (loadedSpecimen == null)
        {
            Debug.LogWarning("Specimen aún no ha terminado de cargarse.");
            return false;
        }    

        viewer.Show(loadedSpecimen);

        return true;
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