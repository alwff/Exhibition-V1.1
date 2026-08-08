using System;
using System.IO;
using UnityEngine;
using System.Collections;

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

    private string configID = Guid.NewGuid().ToString();

    IEnumerator Start()
    {
        yield return null;

        yield return Load();
    }

    public void Save()
    {
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
            clean[i] = string.IsNullOrEmpty(raw[i]) ? "" : raw[i];
        }

        data.slots = clean;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log(
            $"ExhibitionConfig guardada ({data.exhibitionName})"
        );
    }

    public IEnumerator Load()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("No existe ExhibitionConfig.json aún");
            yield break;
        }

        string json = File.ReadAllText(filePath);
        ConfigData data =
            JsonUtility.FromJson<ConfigData>(json);

        if (!string.IsNullOrEmpty(data.configID))
        {
            configID = data.configID;
        }

        if (data == null || data.slots == null)
        {
            Debug.LogWarning("Config inválida");
            yield break;
        }

        Debug.Log(
            $"Config v{data.version} | " +
            $"{data.exhibitionName} | " +
            $"{data.lastModified}"
        );

        for (int i = 0; i < data.slots.Length && i < gallery.slots.Length; i++)
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
    }
}