using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneCursorController
{
    // If we add any new scenes that have UI - add them here!
    static readonly HashSet<string> uiScenesNeedingCursor = new HashSet<string>
    {
        "IntroScene",
        "InterLevelScene",
        "WinScene",
        "LoseScene"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    static void Apply(Scene scene)
    {
        if (!scene.IsValid())
            return;

        if (uiScenesNeedingCursor.Contains(scene.name))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
