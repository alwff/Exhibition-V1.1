public static class ApiRoutes
{
#if UNITY_WEBGL && !UNITY_EDITOR

    // WebGL está servido por el mismo Flask.
    private const string BaseUrl = "";

#else

    // Desarrollo desde Unity Editor.
    private const string BaseUrl =
        "http://192.168.1.21:8000";

#endif


    public static string Specimens =>
        BaseUrl + "/api/specimens";


    public static string Specimen(string identifier)
    {
        return BaseUrl
            + "/api/specimen/"
            + identifier;
    }


    public static string ExhibitionConfig =>
        BaseUrl + "/api/exhibition/config";


    public static string ExhibitionAuth =>
        BaseUrl + "/api/exhibition/auth";


    public static string ExhibitionSettings =>
        BaseUrl + "/api/exhibition/settings";
}
