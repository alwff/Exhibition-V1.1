using System;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
    public string filePath = "C:/Specimens/ExhibitionConfig.json";
    public GalleryManager gallery;

    public SpecimenAPIClient apiClient;

    [Header("Web API")]
    public string exhibitionConfigUrl = "/api/exhibition/config";

    private string configID = Guid.NewGuid().ToString();

    IEnumerator Start()
    {
        yield return null;

        yield return Load();
    }

    public void Save()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR

        StartCoroutine(SaveWebConfig());
        return;

    #else

        if (gallery == null)
        {
            Debug.LogError("Gallery no asignado en ConfigManager");
            return;
        }

        ConfigData data = new ConfigData();
        data.configID = configID;

        data.version = 1;
        data.exhibitionName = "Colecciones UVG";
        data.modifiedBy = "admin";

        data.lastModified =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string[] raw = gallery.GetCurrentAssignments();
        string[] clean = new string[raw.Length];

        for (int i = 0; i < raw.Length; i++)
        {
            clean[i] =
                string.IsNullOrEmpty(raw[i]) ? "" : raw[i];
        }

        data.slots = clean;

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(filePath, json);

        Debug.Log(
            $"ExhibitionConfig guardada ({data.exhibitionName})"
        );

    #endif
    }
    public IEnumerator Load()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR

        yield return LoadWebConfig();
        yield break;

    #else

        if (!File.Exists(filePath))
        {
            Debug.LogWarning(
                "No existe ExhibitionConfig.json aún"
            );

            yield break;
        }

        string json = File.ReadAllText(filePath);

        ConfigData data =
            JsonUtility.FromJson<ConfigData>(json);

        if (data == null || data.slots == null)
        {
            Debug.LogWarning("Config inválida");
            yield break;
        }

        if (!string.IsNullOrEmpty(data.configID))
        {
            configID = data.configID;
        }

        Debug.Log(
            $"Config v{data.version} | " +
            $"{data.exhibitionName} | " +
            $"{data.lastModified}"
        );

        for (
            int i = 0;
            i < data.slots.Length && i < gallery.slots.Length;
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
            $"ExhibitionConfig cargada ({data.exhibitionName})"
        );

    #endif
    }
    IEnumerator SaveWebConfig()
    {
        if (gallery == null)
        {
            Debug.LogError("Gallery no asignado en ConfigManager");
            yield break;
        }

        string[] raw = gallery.GetCurrentAssignments();
        string[] clean = new string[raw.Length];

        for (int i = 0; i < raw.Length; i++)
        {
            clean[i] =
                string.IsNullOrEmpty(raw[i]) ? "" : raw[i];
        }

        ConfigData data = new ConfigData();
        data.slots = clean;

        string json = JsonUtility.ToJson(data);

        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request =
            new UnityWebRequest(
                exhibitionConfigUrl,
                UnityWebRequest.kHttpVerbPUT
            ))
        {
            request.uploadHandler =
                new UploadHandlerRaw(bodyRaw);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Error guardando configuración Web: " +
                    request.error +
                    "\n" +
                    request.downloadHandler.text
                );

                yield break;
            }

            Debug.Log(
                "Configuración de exhibición guardada en servidor."
            );
        }
    }

    IEnumerator LoadWebConfig()
    {
        using (UnityWebRequest request =
            UnityWebRequest.Get(exhibitionConfigUrl))
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