using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ExhibitionAppearanceSettings
{
    public string wall_texture;
    public string floor_texture;
    public string ceiling_texture;
}

public class ExhibitionAppearanceManager : MonoBehaviour
{
    [Header("Exhibition Renderer")]
    public Renderer exhibitionRenderer;

    [Header("Material Slots")]
    public int wallMaterialIndex = 0;
    public int floorMaterialIndex = 1;
    public int ceilingMaterialIndex = 2;

    private Material wallMaterial;
    private Material floorMaterial;
    private Material ceilingMaterial;

    private Texture originalWallTexture;
    private Texture originalFloorTexture;
    private Texture originalCeilingTexture;


    IEnumerator Start()
    {
        PrepareMaterials();

        yield return LoadAppearance();
    }


    void PrepareMaterials()
    {
        if (exhibitionRenderer == null)
        {
            Debug.LogError(
                "ExhibitionAppearanceManager: " +
                "Exhibition Renderer no asignado."
            );

            return;
        }

        Material[] materials =
            exhibitionRenderer.materials;

        if (materials.Length < 3)
        {
            Debug.LogError(
                "ExhibitionAppearanceManager: " +
                "se esperaban al menos 3 materiales."
            );

            return;
        }

        wallMaterial = new Material(
            materials[wallMaterialIndex]
        );

        floorMaterial = new Material(
            materials[floorMaterialIndex]
        );

        ceilingMaterial = new Material(
            materials[ceilingMaterialIndex]
        );

        materials[wallMaterialIndex] =
            wallMaterial;

        materials[floorMaterialIndex] =
            floorMaterial;

        materials[ceilingMaterialIndex] =
            ceilingMaterial;

        exhibitionRenderer.materials =
            materials;


        originalWallTexture =
            wallMaterial.mainTexture;

        originalFloorTexture =
            floorMaterial.mainTexture;

        originalCeilingTexture =
            ceilingMaterial.mainTexture;
    }


    IEnumerator LoadAppearance()
    {
        if (exhibitionRenderer == null)
            yield break;

        using (
            UnityWebRequest request =
                UnityWebRequest.Get(ApiRoutes.ExhibitionSettings)
        )
        {
            yield return request.SendWebRequest();

            if (
                request.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogError(
                    "Error cargando apariencia: "
                    + request.error
                );

                yield break;
            }

            ExhibitionAppearanceSettings settings =
                JsonUtility.FromJson
                <ExhibitionAppearanceSettings>(
                    request.downloadHandler.text
                );

            if (settings == null)
            {
                Debug.LogWarning(
                    "Configuración de apariencia inválida."
                );

                yield break;
            }


            yield return ApplyTexture(
                settings.wall_texture,
                wallMaterial,
                originalWallTexture,
                "paredes"
            );

            yield return ApplyTexture(
                settings.floor_texture,
                floorMaterial,
                originalFloorTexture,
                "piso"
            );

            yield return ApplyTexture(
                settings.ceiling_texture,
                ceilingMaterial,
                originalCeilingTexture,
                "techo"
            );


            Debug.Log(
                "Apariencia de exhibición cargada correctamente."
            );
        }
    }


    IEnumerator ApplyTexture(
        string url,
        Material targetMaterial,
        Texture defaultTexture,
        string surfaceName
    )
    {
        if (targetMaterial == null)
            yield break;


        // Sin textura personalizada:
        // conservar/restaurar la original del build.
        if (string.IsNullOrEmpty(url))
        {
            targetMaterial.mainTexture =
                defaultTexture;

            Debug.Log(
                $"Textura predeterminada aplicada: {surfaceName}"
            );

            yield break;
        }


        using (
            UnityWebRequest request =
                UnityWebRequestTexture.GetTexture(url)
        )
        {
            yield return request.SendWebRequest();

            if (
                request.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogWarning(
                    $"No se pudo cargar textura de " +
                    $"{surfaceName}: {request.error}"
                );

                targetMaterial.mainTexture =
                    defaultTexture;

                yield break;
            }


            Texture2D downloadedTexture =
                DownloadHandlerTexture.GetContent(
                    request
                );

            targetMaterial.mainTexture =
                downloadedTexture;


            Debug.Log(
                $"Textura personalizada aplicada: {surfaceName}"
            );
        }
    }
}