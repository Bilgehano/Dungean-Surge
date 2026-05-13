using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SceneBootstrapper
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string SessionKey = "MainMenuOpenedOnStartup";

    static SceneBootstrapper()
    {
        // Set the scene that will be loaded when pressing 'Play' in the Editor,
        // regardless of which scene is currently open.
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (sceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
        }

        // On project startup, if we haven't already opened MainMenu in this session, open it.
        if (!SessionState.GetBool(SessionKey, false))
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && 
                    EditorSceneManager.GetActiveScene().path != MainMenuPath)
                {
                    EditorSceneManager.OpenScene(MainMenuPath);
                    SessionState.SetBool(SessionKey, true);
                }
            };
        }
    }
}
