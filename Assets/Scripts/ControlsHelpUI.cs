using UnityEngine;

public class ControlsHelpUI : MonoBehaviour
{
    public GameObject controlsPanel;

    private bool isOpen = false;

    void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleControls();
        }
    }

    public void ToggleControls()
    {
        isOpen = !isOpen;

        if (controlsPanel != null)
            controlsPanel.SetActive(isOpen);
    }
}