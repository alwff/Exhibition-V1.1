using UnityEngine;
using System;

[Serializable]
public class SpecimenData
{
    public string id;
    public string name;
    public string description;
    public string collection;
    public string status;

    public string[] images;
}

[Serializable]
public class SpecimenListItem
{
    public string id;
    public string name;
    public string collection;
}

[Serializable]
public class SpecimenListWrapper
{
    public SpecimenListItem[] items;
}

[Serializable]
public class LoadedSpecimen
{
    public SpecimenData data;

    public Texture2D[] images;

    public Texture2D preview;

    public bool isCached;

    public bool imagesReady;
}