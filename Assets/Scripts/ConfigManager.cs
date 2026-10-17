using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.Runtime.InteropServices;
using SUPERCharacter;

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

    [Header("Inicio del recorrido")]
    public MovementTutorialUI movementTutorial;

    public SUPERCharacterAIO playerController;

    public static bool IsInitialLoading { get; private set; }

    private bool playerWasEnabled;

    private bool contentReady = false;
    private bool explorationStarted = false;

    [Header("Save Feedback")]
    public TextMeshProUGUI saveStatusText;

    public float retryDelay = 1.0f;

    [Header("Admin Session")]
    public AdminAuth adminAuth;

    private string configID = Guid.NewGuid().ToString();

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ExhibitionLoadProgress(
            int completed,
            int total
        );

        [DllImport("__Internal")]
        private static extern void ExhibitionLoadReady();

        [DllImport("__Internal")]
        private static extern void ExhibitionLoadFailed();  
    #endif

        private void Awake()
        {
            IsInitialLoading = true;
            InputBlocker.blockInput = true;

            if (playerController != null)
            {
                playerWasEnabled = playerController.enabled;
                playerController.enabled = false;
            }
        }

        public void BeginExploration()
        {
            if (!contentReady || explorationStarted)
                return;

            explorationStarted = true;

            IsInitialLoading = false;
            InputBlocker.blockInput = false;

            if (playerController != null)
                playerController.enabled = playerWasEnabled;

            if (movementTutorial != null)
                movementTutorial.ShowTutorial();
        }

        private void ReportLoadingFailed()
        {
        #if UNITY_WEBGL && !UNITY_EDITOR
            ExhibitionLoadFailed();
        #endif
        }

        private void ReportLoadingProgress(int completed, int total)
        {
        #if UNITY_WEBGL && !UNITY_EDITOR
                ExhibitionLoadProgress(completed, total);
        #else
                Debug.Log(
                    $"Carga de exhibición: {completed}/{total} especímenes."
                );
        #endif
        }

        private void ReportLoadingReady()
        {
                contentReady = true;

            #if UNITY_WEBGL && !UNITY_EDITOR
                ExhibitionLoadReady();
            #else
                Debug.Log("Exhibición preparada para explorar.");
                BeginExploration();
            #endif
        }

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
        if (gallery == null || apiClient == null)
        {
            Debug.LogError(
                "Falta asignar Gallery o API Client en ConfigManager."
            );

            ReportLoadingFailed();

            yield break;
        }

        using (UnityWebRequest request =
            UnityWebRequest.Get(ApiRoutes.ExhibitionConfig))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Error cargando configuración Web: " +
                    request.error
                );

                ReportLoadingFailed();

                yield break;
            }

            ConfigData data =
                JsonUtility.FromJson<ConfigData>(
                    request.downloadHandler.text
                );

            if (data == null || data.slots == null)
            {
                Debug.LogError("Configuración Web inválida.");

                ReportLoadingFailed();
                yield break;
            }

            int slotCount = Mathf.Min(
                data.slots.Length,
                gallery.slots.Length
            );

            int total = 0;

            for (int i = 0; i < slotCount; i++)
            {
                if (!string.IsNullOrEmpty(data.slots[i]))
                    total++;
            }

            gallery.ClearAll();

            int completed = 0;

            ReportLoadingProgress(completed, total);

            for (int i = 0; i < slotCount; i++)
            {
                string portalCode = data.slots[i];

                if (string.IsNullOrEmpty(portalCode))
                    continue;

                if (gallery.slots[i] == null)
                {
                    Debug.LogError(
                        $"No está asignado el cuadro {i + 1} en Gallery."
                    );

                    ReportLoadingFailed();

                    yield break;
                }

                LoadedSpecimen loaded = null;

                yield return apiClient.LoadCompleteSpecimen(
                    portalCode,
                    specimen =>
                    {
                        loaded = specimen;
                    }
                );

                if (
                    loaded == null ||
                    !loaded.imagesReady ||
                    loaded.images == null ||
                    loaded.images.Length == 0
                )
                {
                    Debug.LogError(
                        $"No se completó la carga de {portalCode}. " +
                        "La exhibición todavía no está preparada."
                    );

                    ReportLoadingFailed();

                    yield break;
                }

                gallery.AssignSpecimen(i, loaded);

                completed++;

                ReportLoadingProgress(completed, total);
            }

            Debug.Log(
                "Configuración e imágenes de la exhibición " +
                "cargadas desde el servidor."
            );

            ReportLoadingReady();
        }
    }


}