#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NotYetSafe.MenuTheme.Editor
{
    public static class EarthquakeMenuSetup
    {
        private const string MenuPath = "Assets/_Project/MenuTheme/MenuScene_Deprem.unity";
        private const string GamePath = "Assets/_Project/MenuTheme/MainScene_Home.unity";

        [MenuItem("Tools/Not Yet Safe/Open Earthquake Menu")]
        public static void OpenMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before opening the earthquake menu.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath) == null ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(GamePath) == null)
            {
                Debug.LogError("The menu or MainScene_Home could not be found. Copy the complete MenuTheme folder into Assets/_Project.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(MenuPath, true));
            bool hasGame = false;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == MenuPath)
                    continue;
                if (scene.path == GamePath)
                {
                    scenes.Add(new EditorBuildSettingsScene(GamePath, true));
                    hasGame = true;
                }
                else
                {
                    scenes.Add(scene);
                }
            }
            if (!hasGame)
                scenes.Add(new EditorBuildSettingsScene(GamePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorSceneManager.OpenScene(MenuPath);
            ValidateMenu();
            ValidateVisualAssets();
        }

        [MenuItem("Tools/Not Yet Safe/Open Furnished Home")]
        public static void OpenHome()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before opening the home scene.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(GamePath);
        }

        private static void ValidateVisualAssets()
        {
            foreach (string path in new[] {
                "Assets/_Project/MenuTheme/Home/FurnishedHome.prefab",
                "Assets/_Project/MenuTheme/Home/CartoonRadio.prefab" })
            {
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    Debug.LogError("Missing visual prefab: " + path);
                    continue;
                }
                foreach (MeshFilter filter in asset.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null)
                        Debug.LogError("Missing primitive mesh on " + filter.name + " in " + path, asset);
                }
                foreach (MeshRenderer renderer in asset.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer.sharedMaterial == null || renderer.sharedMaterial.shader == null)
                        Debug.LogError("Missing material on " + renderer.name + " in " + path, asset);
                }
            }
        }

        [MenuItem("Tools/Not Yet Safe/Validate Open Menu")]
        public static void ValidateMenu()
        {
            MenuUIManager manager = Object.FindFirstObjectByType<MenuUIManager>();
            SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (manager == null || settings == null || eventSystem == null)
            {
                Debug.LogError("Open MenuScene_Deprem first. MenuUIManager, SettingsManager and EventSystem are required.");
                return;
            }
            SerializedObject menu = new SerializedObject(manager);
            foreach (string field in new[] { "mainMenuPanel", "settingsPanel", "playButton",
                "settingsButton", "exitButton", "settingsBackButton" })
            {
                if (menu.FindProperty(field).objectReferenceValue == null)
                {
                    Debug.LogError("Missing menu connection: " + field, manager);
                    return;
                }
            }
            SerializedObject options = new SerializedObject(settings);
            if (options.FindProperty("masterVolumeSlider").objectReferenceValue == null ||
                options.FindProperty("fullscreenToggle").objectReferenceValue == null)
            {
                Debug.LogError("The volume slider or fullscreen toggle is not connected.", settings);
                return;
            }
            if (menu.FindProperty("targetSceneName").stringValue != "MainScene_Home")
            {
                Debug.LogWarning("The play target differs from MainScene_Home; check Target Scene Name.", manager);
                return;
            }
            if (eventSystem.GetComponent<BaseInputModule>() == null)
            {
                Debug.LogError("EventSystem requires an input module.", eventSystem);
                return;
            }
            Debug.Log("Earthquake menu connections are ready. Press Play to check Start Game, Settings and Back. In Build Profiles, keep this menu first if Override Global Scene List is enabled.", manager);
        }
    }
}
#endif
