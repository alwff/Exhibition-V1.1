using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SUPERCharacter;
using System.Collections;
using UnityEngine.Networking;

[System.Serializable]
public class ExhibitionAuthRequest
{
    public string password;
}

[System.Serializable]
public class ExhibitionAuthResponse
{
    public bool success;
    public string token;
}


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
    public string authUrl = "/api/exhibition/auth";

    [Header("Admin Shortcut")]
    public KeyCode adminKey = KeyCode.M;

    [Header("Player")]
    public SUPERCharacterAIO playerController;
    public Rigidbody playerRigidbody;

    private string input = "";
    private bool isEntering = false;
    private bool showPassword = false;

    public static string AdminToken { get; private set; }

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
        if (Input.GetKeyDown(adminKey))
        {
            // Si ya estamos dentro del menú Admin, M sale directamente al museo
            if (adminCanvas.activeSelf)
            {
                CloseAll();
                return;
            }

            // Si estamos jugando normalmente, M abre autenticación
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
        
        if (loginButton != null)
            loginButton.interactable = true;

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

        if (string.IsNullOrEmpty(input))
        {
            if (errorText != null)
                errorText.text = "Ingresa la clave.";

            return;
        }

        StartCoroutine(
            AuthenticateWithServer(input)
        );
    }

    IEnumerator AuthenticateWithServer(string password)
    {
        // Evita múltiples intentos simultáneos.
        isEntering = false;

        if (loginButton != null)
            loginButton.interactable = false;

        ClearError();

        if (errorText != null)
            errorText.text = "Verificando...";


        ExhibitionAuthRequest authData =
            new ExhibitionAuthRequest();

        authData.password = password;

        string json =
            JsonUtility.ToJson(authData);

        byte[] body =
            System.Text.Encoding.UTF8.GetBytes(json);


        using (
            UnityWebRequest request =
                new UnityWebRequest(
                    authUrl,
                    UnityWebRequest.kHttpVerbPOST
                )
        )
        {
            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );


            yield return request.SendWebRequest();


            // ---------- LOGIN CORRECTO ----------

            if (request.responseCode == 200)
            {
                ExhibitionAuthResponse response =
                    JsonUtility.FromJson
                    <ExhibitionAuthResponse>(
                        request.downloadHandler.text
                    );

                if (
                    response != null &&
                    response.success
                )
                {
                    AdminToken = response.token;
                    
                    passwordCanvas.SetActive(false);
                    adminCanvas.SetActive(true);

                    input = "";

                    ClearError();

                    if (loginButton != null)
                        loginButton.interactable = true;

                    yield break;
                }
            }


            // ---------- CLAVE INCORRECTA ----------

            if (request.responseCode == 401)
            {
                if (errorText != null)
                {
                    errorText.text =
                        "Clave incorrecta. Inténtalo nuevamente.";
                }

                input = "";
                isEntering = true;

                UpdatePasswordDisplay();

                if (loginButton != null)
                    loginButton.interactable = true;

                yield break;
            }


            // ---------- OTRO ERROR ----------

            Debug.LogError(
                "Error autenticando administrador.\n" +
                "HTTP: " + request.responseCode + "\n" +
                "Error: " + request.error + "\n" +
                "Response: " + request.downloadHandler.text
            );

            if (errorText != null)
            {
                errorText.text =
                    "No se pudo conectar con el servidor.";
            }

            isEntering = true;

            if (loginButton != null)
                loginButton.interactable = true;
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