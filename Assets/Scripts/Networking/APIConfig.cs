using UnityEngine;

public static class APIConfig
{
    public static APIEnvironment Environment = APIEnvironment.Ngrok;

    private const string LocalURL =
        "http://192.168.1.22:8000/api";

    private const string NgrokURL =
        "https://relish-angriness-submarine.ngrok-free.dev/api";
    public static string BaseURL
    {
        get
        {
            switch (Environment)
            {
                case APIEnvironment.Ngrok:
                    return NgrokURL;

                default:
                    return LocalURL;
            }
        }
    }

    public static string Build(string endpoint)
    {
        endpoint = endpoint.TrimStart('/');

        return BaseURL.TrimEnd('/') + "/" + endpoint;
    }
}
