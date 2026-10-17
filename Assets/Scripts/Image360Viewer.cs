using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Image360Viewer : MonoBehaviour
{
    public GameObject panel;
    public RawImage display;

    public RectTransform imageViewport;

    public GameObject hintContainer;
    public TextMeshProUGUI hintText;
    public GameObject loading360Text;

    public Rigidbody playerRigidbody;

    [Header("Cursor Management")]
    public GameplayCursorManager cursorManager;

    private Texture2D[] frames;
    private int index = 0;

    private bool isDragging = false;

    public float hintDuration = 5f;

    // ZOOM
    float zoom = 1f;
    float zoomVelocity = 0f;

    float minZoom = 1f;
    float maxZoom = 2.5f;

    float zoomSmooth = 8f;
    float zoomDamping = 5f;

    private void SetImages(Texture2D[] imgs)
    {
        frames = imgs;
        index = 0;

        if (frames != null && frames.Length > 0)
        {
            display.texture = frames[0];
        }
    }

    public void ShowLoading()
    {
        if (loading360Text != null)
            loading360Text.SetActive(true);
    }

    public void HideLoading()
    {
        if (loading360Text != null)
            loading360Text.SetActive(false);
    }

    void Start()
    {
        HideLoading();
    }

    private void Open()
    {
        panel.SetActive(true);

        // Reiniciar estado del visor
        zoom = 1f;
        zoomVelocity = 0f;
        index = 0;
        isDragging = false;

        display.rectTransform.localScale = Vector3.one;

        if (frames != null && frames.Length > 0)
        {
            display.texture = frames[0];
        }

        panel.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleIn());

        InputBlocker.blockInput = true;

        // Liberar cursor para utilizar el visor.
        if (cursorManager != null)
            cursorManager.EnterUI();

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.Sleep();
        }

        if (hintContainer != null)
        {
            hintContainer.SetActive(true);

            if (hintText != null)
            {
                hintText.text = "Mantén click izquierdo para rotar\nScroll para zoom\nPresiona X o utiliza el botón para cerrar";
            }

            CancelInvoke(nameof(HideHint));
            Invoke(nameof(HideHint), hintDuration);
        }
    }

    public void Close()
    {
        isDragging = false;

        CancelInvoke(nameof(HideHint));

        panel.SetActive(false);

        InputBlocker.blockInput = false;

        if (cursorManager != null)
            cursorManager.ExitUI();
    }

    void Update()
    {
        if (!panel.activeSelf || frames == null || frames.Length == 0) return;

        // ROTACIÓN
        if (Input.GetMouseButtonDown(0))
        {
            bool mouseInsideViewport =
                imageViewport != null &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    imageViewport,
                    Input.mousePosition,
                    null
                );

            if (mouseInsideViewport)
            {
                isDragging = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            float delta = Input.GetAxis("Mouse X");

            index += (int)(delta * 10);

            if (index >= frames.Length)
                index = 0;

            if (index < 0)
                index = frames.Length - 1;

            display.texture = frames[index];
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ZOOM INPUT
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            zoomVelocity += scroll * 5f;
        }

        // INERCIA
        zoom += zoomVelocity * Time.deltaTime;
        zoomVelocity = Mathf.Lerp(zoomVelocity, 0, Time.deltaTime * zoomDamping);

        // LÍMITES
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        // SUAVIZADO
        Vector3 targetScale = Vector3.one * zoom;
        display.rectTransform.localScale = Vector3.Lerp(
            display.rectTransform.localScale,
            targetScale,
            Time.deltaTime * zoomSmooth
        );

        // CERRAR VISOR CON X
        if (Input.GetKeyDown(KeyCode.X))
        {
            Close();
        }
    }

    IEnumerator ScaleIn()
    {
        float duration = 0.25f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(0, 1, t / duration);
            panel.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        panel.transform.localScale = Vector3.one;
    }

    IEnumerator FadeText(TextMeshProUGUI text, float duration)
    {
        if (text == null) yield break;

        Color c = text.color;
        c.a = 0;
        text.color = c;

        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / duration);
            text.color = c;
            yield return null;
        }

        c.a = 1;
        text.color = c;
    }

    void HideHint()
    {
        if (hintContainer != null)
        {
            hintContainer.SetActive(false);
        }
    }

    public void Show(LoadedSpecimen specimen)
    {
        if (specimen == null)
            return;

        SetImages(specimen.images);

        // Activa ViewerPanel
        Open();

        // Carga la información
        if (infoPanel != null)
            infoPanel.Show(specimen);
    }

    public SpecimenViewerUI infoPanel;
}

