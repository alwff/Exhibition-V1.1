using UnityEngine;

public class GameplayCursorManager : MonoBehaviour
{
    public GameObject resumeCursorMessage;

    private bool waitingForLock = false;
    private bool mouseReleased = true;

    void Start()
    {
        if (resumeCursorMessage != null)
            resumeCursorMessage.SetActive(false);
    }

    public void EnterUI()
    {
        waitingForLock = false;

        if (resumeCursorMessage != null)
            resumeCursorMessage.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitUI()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        // Esperar una nueva interacción del usuario.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        waitingForLock = true;
        mouseReleased = !Input.GetMouseButton(0);

        if (resumeCursorMessage != null)
            resumeCursorMessage.SetActive(true);

#else

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        waitingForLock = false;

#endif
    }

    void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        if (!waitingForLock)
            return;

        // Evita reutilizar el clic que cerró una interfaz.
        if (!mouseReleased)
        {
            if (!Input.GetMouseButton(0))
                mouseReleased = true;

            return;
        }

        // Solicitar Pointer Lock con un nuevo clic.
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // No ocultar el aviso hasta confirmar el bloqueo.
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            waitingForLock = false;

            if (resumeCursorMessage != null)
                resumeCursorMessage.SetActive(false);
        }

#endif
    }
}