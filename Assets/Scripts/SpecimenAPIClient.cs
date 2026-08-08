using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpecimenAPIClient : MonoBehaviour
{

    #region Configuration

    [SerializeField]
    private bool verboseLogs = true;

    [SerializeField]
    private int timeoutSeconds = 15;

    #endregion

   #region HTTP Core

       string BuildURL(string endpoint)
    {
        return APIConfig.Build(endpoint);
    }

    IEnumerator SendGET(
        string endpoint,
        System.Action<string> onSuccess,
        System.Action<string> onError = null)
    {
        string url = BuildURL(endpoint);

        LogRequest("GET", url);

        UnityWebRequest request = UnityWebRequest.Get(url);

        request.timeout = timeoutSeconds;

        yield return request.SendWebRequest();

        LogResponse(request);

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            string error = request.error;

            Debug.LogError(
                "\n========== API ERROR ==========\n" +
                error +
                "\nURL:\n" +
                url +
                "\n==============================="
            );

            onError?.Invoke(error);
        }
    }

    void LogRequest(string method, string url)
    {
        if (!verboseLogs)
            return;

        Debug.Log(
            "\n========== API ==========\n" +
            method +
            "\n\n" +
            url +
            "\n========================="
        );
    }

    void LogResponse(UnityWebRequest request)
    {
        if (!verboseLogs)
            return;

        Debug.Log(
            "\n========== RESPONSE ==========\n" +
            "Code: " + request.responseCode +
            "\nResult: " + request.result +
            "\nError: " + request.error +
            "\n=============================="
        );
    }

    #endregion 

    #region Image Cache

    Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

    bool HasImage(string url)
    {
        return imageCache.ContainsKey(url);
    }

    Texture2D GetCachedImage(string url)
    {
        return imageCache[url];
    }

    void SaveImage(string url, Texture2D image)
    {
        if (image == null)
            return;

        imageCache[url] = image;
    }

    #endregion

    #region Specimen Cache

    Dictionary<string, SpecimenData> specimenCache = new Dictionary<string, SpecimenData>();

    bool HasSpecimen(string id)
    {
        return specimenCache.ContainsKey(id);
    }

    SpecimenData GetCachedSpecimen(string id)
    {
        return specimenCache[id];
    }

    void SaveSpecimen(SpecimenData specimen)
    {
        if (specimen == null)
            return;

        specimenCache[specimen.id] = specimen;
    }

    #endregion

    #region LoadedSpecimen Cache

    Dictionary<string, LoadedSpecimen> loadedCache = new Dictionary<string, LoadedSpecimen>();

    bool HasLoadedSpecimen(string id)
    {
        return loadedCache.ContainsKey(id);
    }

    LoadedSpecimen GetLoadedSpecimen(string id)
    {
        return loadedCache[id];
    }

    void SaveLoadedSpecimen(LoadedSpecimen specimen)
    {
        if (specimen == null)
            return;

        loadedCache[specimen.data.id] = specimen;
    }

    #endregion

    #region Image Loading

    public IEnumerator LoadImagesStreaming(
        string[] urls,
        System.Action<Texture2D> onFirstLoaded,
        System.Action<Texture2D[]> onAllLoaded)
    {
        List<Texture2D> textures = new List<Texture2D>();

        for (int i = 0; i < urls.Length; i++)
        {
            string url = urls[i];

            if (HasImage(url))
            {
                Texture2D cached = GetCachedImage(url);
                textures.Add(cached);

                if (i == 0) onFirstLoaded?.Invoke(cached);
                continue;
            }

            UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                textures.Add(tex);
                SaveImage(url, tex);

                if (i == 0) onFirstLoaded?.Invoke(tex);
            }
        }

        onAllLoaded?.Invoke(textures.ToArray());
    }

    Texture2D[] GetCachedImages(string[] urls)
    {
        Texture2D[] textures = new Texture2D[urls.Length];

        for (int i = 0; i < urls.Length; i++)
        {
            if (!HasImage(urls[i]))
                return null;

            textures[i] = GetCachedImage(urls[i]);
        }

        return textures;
    }

    #endregion

    #region Specimen Loading

    public IEnumerator GetSpecimen(
        string id,
        System.Action<SpecimenData> callback)
    {
        if (HasSpecimen(id))
        {
            callback?.Invoke(GetCachedSpecimen(id));
            yield break;
        }

        yield return SendGET(
            "specimen/" + id,

            (json) =>
            {
                SpecimenData data =
                    JsonUtility.FromJson<SpecimenData>(json);

                SaveSpecimen(data);    

                callback?.Invoke(data);
            }
        );
    }

    public IEnumerator GetAllSpecimens(System.Action<SpecimenListItem[]> callback)
    {
        yield return SendGET(
            "specimens",

            (json) =>
            {
                json = "{ \"items\": " + json + "}";

                SpecimenListWrapper wrapper =
                    JsonUtility.FromJson<SpecimenListWrapper>(json);

                callback?.Invoke(wrapper.items);
            }
        );
    }

    IEnumerator PrepareSpecimen(
        SpecimenData specimen,
        System.Action<LoadedSpecimen> callback)
    {
        Texture2D[] cachedImages =
            GetCachedImages(specimen.images);

        if (cachedImages != null)
        {
            LoadedSpecimen loaded = new LoadedSpecimen();

            loaded.data = specimen;

            loaded.images = cachedImages;

            loaded.preview = cachedImages.Length > 0
                ? cachedImages[0]
                : null;

            loaded.isCached = true;

            loaded.imagesReady = true;

            Debug.Log(
                $"Prepared specimen (CACHE): {loaded.data.id}"
            );

            SaveLoadedSpecimen(loaded);

            callback?.Invoke(loaded);

            yield break;
        }

        yield return LoadImagesStreaming(

            specimen.images,

            null,

            (textures) =>
            {
                LoadedSpecimen loaded =
                    new LoadedSpecimen();

                loaded.data = specimen;

                loaded.images = textures;

                loaded.preview =
                    textures.Length > 0
                        ? textures[0]
                        : null;

                loaded.isCached = false;

                loaded.imagesReady = true;

                Debug.Log(
                    $"Prepared specimen: {loaded.data.id}"
                );

                SaveLoadedSpecimen(loaded);

                callback?.Invoke(loaded);
            }

        );
    }

    public IEnumerator LoadCompleteSpecimen(
        string id,
        System.Action<LoadedSpecimen> callback)
    {
        if (HasLoadedSpecimen(id))
        {
            Debug.Log(
                $"LoadedSpecimen CACHE HIT: {id}"
            );


            callback?.Invoke(
                GetLoadedSpecimen(id)
            );

            yield break;
        }


        SpecimenData specimen = null;

        yield return GetSpecimen(
            id,
            data =>
            {
                specimen = data;
            }
        );

        if (specimen == null)
            yield break;

        yield return PrepareSpecimen(
            specimen,
            callback
        );
    }

    #endregion

    #region Viewer Helpers

    public void ApplyPreview(
        LoadedSpecimen specimen,
        Renderer renderer,
        ref int materialIndex)
    {
        if (specimen == null)
            return;

        if (specimen.preview == null)
            return;

        if (renderer == null)
            return;

        Material[] mats = renderer.materials;

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

        mats[materialIndex].mainTexture =
            specimen.preview;

        renderer.materials = mats;
    }

    #endregion
    
}