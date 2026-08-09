using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SUPERCharacter;

public class AdminAuth : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject passwordCanvas;
    public GameObject adminCanvas;

    [Header("Password UI")]
    public TextMeshProUGUI passwordText;
    public TextMeshProUGUI errorText;
    public Button loginButton;
    public Button eyeButton;

    [Header("Authentication")]
    public string correctPassword = "1234";

    [Header("Player")]
    public SUPERCharacterAIO playerController;
    public Rigidbody playerRigidbody;

    private string input = "";
    private bool isEntering = false;
    private bool showPassword = false;

    void Start()
    {
        passwordCanvas.SetActive(false);
        adminCanvas.SetActive(false);

        if (loginButton != null)
            loginButton.onClick.AddListener(TryLogin);

        if (eyeButton != null)
            eyeButton.onClick.AddListener(TogglePasswordVisibility);

        ClearError();
        UpdatePasswordDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Si ya estamos dentro del menú Admin, F1 sale directamente al museo
            if (adminCanvas.activeSelf)
            {
                CloseAll();
                return;
            }

            // Si estamos jugando normalmente, F1 abre autenticación
            if (!passwordCanvas.activeSelf)
            {
                OpenAuth();
                return;
            }
        }

        if (!isEntering)
            return;

        // Cerrar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAll();
            return;
        }

        // Leer números
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c))
            {
                input += c;
                ClearError();
            }
        }

        // Borrar último carácter
        if (Input.GetKeyDown(KeyCode.Backspace) && input.Length > 0)
        {
            input = input.Substring(0, input.Length - 1);
            ClearError();
        }

        // Confirmar también con Enter
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TryLogin();
        }

        UpdatePasswordDisplay();
    }

    void OpenAuth()
    {
        passwordCanvas.SetActive(true);
        adminCanvas.SetActive(false);

        isEntering = true;
        input = "";
        showPassword = false;

        ClearError();
        UpdatePasswordDisplay();

        // Bloquear jugador
        if (playerController != null)
            playerController.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void TryLogin()
    {
        if (!isEntering)
            return;

        if (input == correctPassword)
        {
            passwordCanvas.SetActive(false);
            adminCanvas.SetActive(true);

            isEntering = false;
            input = "";

            ClearError();
        }
        else
        {
            if (errorText != null)
                errorText.text = "Clave incorrecta. Inténtalo nuevamente.";

            input = "";
            UpdatePasswordDisplay();
        }
    }

    public void TogglePasswordVisibility()
    {
        showPassword = !showPassword;
        UpdatePasswordDisplay();
    }

    void UpdatePasswordDisplay()
    {
        if (passwordText == null)
            return;

        if (showPassword)
            passwordText.text = input;
        else
            passwordText.text = new string('*', input.Length);
    }

    void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
    }

    void CloseAll()
    {
        passwordCanvas.SetActive(false);
        adminCanvas.SetActive(false);

        isEntering = false;
        input = "";
        showPassword = false;

        ClearError();
        UpdatePasswordDisplay();

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}