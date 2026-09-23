using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class ConfigData
{
    public string configID;

    public int version = 1;

    public string exhibitionName = "Colecciones UVG";

    public string modifiedBy = "admin";

    public string lastModified;

    public string[] slots;
}


public class ConfigManager : MonoBehaviour
{
    public GalleryManager gallery;

    public SpecimenAPIClient apiClient;

    [Header("Save Feedback")]
    public TextMeshProUGUI saveStatusText;

    public float retryDelay = 1.0f;

    [Header("Admin Session")]
    public AdminAuth adminAuth;

    private string configID = Guid.NewGuid().ToString();

    IEnumerator Start()
    {
        yield return null;

        yield return Load();
    }

    public void Save()
    {
        StartCoroutine(
            SaveWebConfig()
        );
    }
    public IEnumerator Load()
    {
        yield return LoadWebConfig();
    }
    void SetSaveStatus(string message)
    {
        if (saveStatusText != null)
            saveStatusText.text = message;
    }

    IEnumerator ClearSaveStatusAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        SetSaveStatus("");
    }

    IEnumerator SaveWebConfig()
    {
        if (gallery == null)
        {
            Debug.LogError(
                "Gallery no asignado en ConfigManager"
            );

            yield break;
        }


        if (string.IsNullOrEmpty(AdminAuth.AdminToken))
        {
            SetSaveStatus(
                "Debes iniciar sesión como administrador."
            );

            yield break;
        }


        string[] raw =
            gallery.GetCurrentAssignments();

        string[] clean =
            new string[raw.Length];


        for (int i = 0; i < raw.Length; i++)
        {
            clean[i] =
                string.IsNullOrEmpty(raw[i])
                ? ""
                : raw[i];
        }


        ConfigData data =
            new ConfigData();

        data.slots = clean;


        string json =
            JsonUtility.ToJson(data);

        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(json);


        SetSaveStatus(
            "Guardando configuración..."
        );


        bool retryNeeded = false;


        // =========================
        // PRIMER INTENTO
        // =========================

        using (
            UnityWebRequest request =
                CreateSaveRequest(bodyRaw)
        )
        {
            yield return request.SendWebRequest();


            // SESIÓN EXPIRADA
            if (request.responseCode == 401)
            {
                HandleExpiredSession();
                yield break;
            }


            // ÉXITO
            if (
                request.result ==
                UnityWebRequest.Result.Success
            )
            {
                HandleSaveSuccess();
                yield break;
            }


            // Solo reintentamos errores temporales.
            if (
                request.responseCode == 0 ||
                request.responseCode == 502 ||
                request.responseCode == 503 ||
                request.responseCode == 504
            )
            {
                retryNeeded = true;
            }
            else
            {
                Debug.LogError(
                    "Error guardando configuración Web: " +
                    request.error +
                    "\n" +
                    request.downloadHandler.text
                );

                SetSaveStatus(
                    "No fue posible guardar la configuración."
                );

                yield break;
            }
        }


        if (!retryNeeded)
            yield break;


        // =========================
        // REINTENTO
        // =========================

        SetSaveStatus(
            "Problema de conexión. Reintentando..."
        );

        yield return new WaitForSeconds(
            retryDelay
        );


        using (
            UnityWebRequest request =
                CreateSaveRequest(bodyRaw)
        )
        {
            yield return request.SendWebRequest();


            if (request.responseCode == 401)
            {
                HandleExpiredSession();
                yield break;
            }


            if (
                request.result ==
                UnityWebRequest.Result.Success
            )
            {
                HandleSaveSuccess();
                yield break;
            }


            Debug.LogError(
                "Error guardando configuración Web " +
                "después del reintento: " +
                request.error +
                "\n" +
                request.downloadHandler.text
            );


            SetSaveStatus(
                "No fue posible guardar. Intenta nuevamente."
            );
        }
    }

    UnityWebRequest CreateSaveRequest(
        byte[] bodyRaw
    )
    {
        UnityWebRequest request =
            new UnityWebRequest(
                ApiRoutes.ExhibitionConfig,
                UnityWebRequest.kHttpVerbPUT
            );


        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();


        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );


        if (
            !string.IsNullOrEmpty(
                AdminAuth.AdminToken
            )
        )
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + AdminAuth.AdminToken
            );
        }


        return request;
    }

    void HandleSaveSuccess()
    {
        Debug.Log(
            "Configuración de exhibición " +
            "guardada en servidor."
        );

        SetSaveStatus(
            "Configuración guardada."
        );

        StartCoroutine(
            ClearSaveStatusAfter(3.0f)
        );
    }

    void HandleExpiredSession()
    {
        Debug.LogWarning(
            "La sesión administrativa expiró."
        );

        AdminAuth.ClearAdminToken();

        SetSaveStatus(
            "La sesión administrativa expiró. " +
            "Vuelve a iniciar sesión."
        );

        StartCoroutine(
            CloseAdminAfterExpiredSession()
        );
    }

    IEnumerator CloseAdminAfterExpiredSession()
    {
        yield return new WaitForSeconds(2.0f);

        SetSaveStatus("");

        if (adminAuth != null)
        {
            adminAuth.CloseAll();
        }
    }

    IEnumerator LoadWebConfig()
    {
        using (UnityWebRequest request =
            UnityWebRequest.Get(ApiRoutes.ExhibitionConfig))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Error cargando configuración Web: " +
                    request.error
                );

                yield break;
            }

            ConfigData data =
                JsonUtility.FromJson<ConfigData>(
                    request.downloadHandler.text
                );

            if (data == null || data.slots == null)
            {
                Debug.LogWarning(
                    "Configuración Web inválida."
                );

                yield break;
            }

            // Asegura que una recarga refleje exactamente
            // la configuración almacenada en PostgreSQL.
            gallery.ClearAll();

            for (
                int i = 0;
                i < data.slots.Length &&
                i < gallery.slots.Length;
                i++
            )
            {
                string portalCode = data.slots[i];

                if (string.IsNullOrEmpty(portalCode))
                    continue;

                LoadedSpecimen loaded = null;

                yield return apiClient.LoadCompleteSpecimen(
                    portalCode,
                    specimen =>
                    {
                        loaded = specimen;
                    }
                );

                if (loaded != null)
                {
                    gallery.AssignSpecimen(i, loaded);
                }
            }

            Debug.Log(
                "Configuración de exhibición cargada desde servidor."
            );
        }
    }


}