using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

public static class SceneAuditor
{
    private static StreamWriter writer;

    [MenuItem("Tools/Exhibition/Generate Scene Report")]
    static void Generate()
    {
        string folder = "Assets/Reports";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string file =
            Path.Combine(folder, "SceneAudit.txt");

        writer = new StreamWriter(file, false);

        WriteHeader();

        WriteRootObjects();

        WriteHierarchy();

        WriteComponents();

        writer.Close();

        AssetDatabase.Refresh();

        Debug.Log("Scene Audit generado:\n" + file);
    }

    static void WriteHeader()
    {
        writer.WriteLine("=======================================");
        writer.WriteLine("EXHIBITION SCENE AUDIT");
        writer.WriteLine("=======================================");
        writer.WriteLine();

        writer.WriteLine("Scene:");
        writer.WriteLine(SceneManager.GetActiveScene().name);
        writer.WriteLine();

        writer.WriteLine("Generated:");
        writer.WriteLine(System.DateTime.Now);
        writer.WriteLine();
    }

    static void WriteRootObjects()
    {
        writer.WriteLine("=======================================");
        writer.WriteLine("ROOT OBJECTS");
        writer.WriteLine("=======================================");
        writer.WriteLine();

        GameObject[] roots =
            SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in roots)
            writer.WriteLine(go.name);

        writer.WriteLine();
    }

    static void WriteHierarchy()
    {
        writer.WriteLine("=======================================");
        writer.WriteLine("HIERARCHY");
        writer.WriteLine("=======================================");
        writer.WriteLine();

        GameObject[] roots =
            SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in roots)
            WriteTransform(go.transform, 0);

        writer.WriteLine();
    }

    static void WriteTransform(
        Transform t,
        int depth)
    {
        writer.WriteLine(
            new string(' ', depth * 4) + t.name
        );

        foreach (Transform child in t)
            WriteTransform(child, depth + 1);
    }

    static void WriteComponents()
    {
        writer.WriteLine("=======================================");
        writer.WriteLine("COMPONENTS");
        writer.WriteLine("=======================================");
        writer.WriteLine();

        GameObject[] roots =
            SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in roots)
            WriteComponentsRecursive(go.transform);

        writer.WriteLine();
    }

    static void WriteComponentsRecursive(
        Transform t)
    {
        Component[] comps =
            t.GetComponents<Component>();

        writer.WriteLine(t.name);

        foreach (Component c in comps)
        {
            if (c == null)
                writer.WriteLine("    Missing Script");
            else
                writer.WriteLine("    " + c.GetType().Name);
        }

        writer.WriteLine();

        foreach (Transform child in t)
            WriteComponentsRecursive(child);
    }
}