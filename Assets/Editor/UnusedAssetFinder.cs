using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class UnusedAssetFinder : EditorWindow
{
    private Vector2 scroll;
    private List<string> unusedAssets = new List<string>();

    [MenuItem("Tools/Find Unused Assets")]
    private static void ShowWindow()
    {
        GetWindow<UnusedAssetFinder>("Unused Assets");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Project"))
        {
            Scan();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Unused assets:", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var path in unusedAssets)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Ping", GUILayout.Width(40)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }

            EditorGUILayout.LabelField(path);

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog(
                    "Delete asset?",
                    $"Are you sure you want to delete:\n{path}\n(This cannot be undone.)",
                    "Yes, delete",
                    "Cancel"))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void Scan()
    {
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var usedAssets = new HashSet<string>();
        foreach (var scenePath in scenePaths)
        {
            var deps = AssetDatabase.GetDependencies(scenePath, true);
            foreach (var dep in deps)
            {
                usedAssets.Add(dep);
            }
        }

        bool Keep(string path)
        {
            if (Directory.Exists(path)) return true;

            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".cs" || ext == ".asmdef" || ext == ".asmref") return true;

            if (path.Contains("/Resources/")) return true;

            return false;
        }

        var allPaths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/")).ToArray();

        unusedAssets = allPaths.Where(p => !usedAssets.Contains(p)).Where(p => !Keep(p)).OrderBy(p => p).ToList();

        Debug.Log($"Found {unusedAssets.Count} candidate unused assets.\n" +
                  "Review them in the window before deleting. Use version control!");
    }
}
