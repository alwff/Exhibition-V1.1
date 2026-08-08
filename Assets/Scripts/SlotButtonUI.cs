using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotButtonUI : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Button button;

    private int slotIndex;

    public void Setup(int index, System.Action<int> onClick)
    {
        slotIndex = index;
        label.text = "Slot " + (index + 1);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(slotIndex));
    }
}