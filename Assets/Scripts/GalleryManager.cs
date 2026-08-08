using UnityEngine;

public class GalleryManager : MonoBehaviour
{
    public PaintingSlot[] slots;

    public void AssignSpecimen(int slotIndex, LoadedSpecimen specimen)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        slots[slotIndex].SetSpecimen(specimen);
    }

    public string[] GetCurrentAssignments()
    {
        string[] data = new string[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            data[i] = slots[i].specimenID;
        }

        return data;
    }
}