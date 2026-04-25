#if UNITY_TOOLBAR_EXTENDER || UNITY_6000_3_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ASze.CustomPlayButton
{
    [InitializeOnLoad]
    public static class CustomPlayButtonCore
    {
        public const string ICONS_PATH = "Packages/com.antonysze.custom-play-button/Editor/Icons/";

        static SceneAsset s_SelectedScene;
        static SceneAsset s_LastScene;

        public static event Action SelectedSceneChanged;

        public static SceneAsset SelectedScene
        {
            get => s_SelectedScene;
            set
            {
                s_SelectedScene = value;
                SelectedSceneChanged?.Invoke();

                if (value != null)
                {
                    var path = AssetDatabase.GetAssetPath(value);
                    EditorPrefs.SetString(GetEditorPrefKey(), path);
                }
                else
                {
                    EditorPrefs.DeleteKey(GetEditorPrefKey());
                }
            }
        }

        static CustomPlayButtonCore()
        {
            var savedScenePath = EditorPrefs.GetString(GetEditorPrefKey(), "");
            s_SelectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(savedScenePath);
            if (s_SelectedScene == null && EditorBuildSettings.scenes.Length > 0)
            {
                var scenePath = EditorBuildSettings.scenes[0].path;
                SelectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }

            EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
            EditorApplication.update += OnUpdate;
        }

        public static void PlayCustomScene()
        {
            if (s_SelectedScene != null)
            {
                StartScene(s_SelectedScene);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Cannot play custom scene",
                    "No scene is selected to play. Please select a scene from the dropdown list.",
                    "Ok");
            }
        }

        public static void PlayGameScene()
        {
            if (EditorBuildSettings.scenes.Length > 0)
            {
                var scenePath = EditorBuildSettings.scenes[0].path;
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                StartScene(scene);
            }
            else
            {
                if (!EditorUtility.DisplayDialog(
                    "Cannot play the game",
                    "Please add the first scene in build setting in order to play the game.",
                    "Ok", "Open build setting"))
                {
                    EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                }
            }
        }

        public static string GetEditorPrefKey()
        {
            var projectPrefix = PlayerSettings.companyName + "." + PlayerSettings.productName;
            return projectPrefix + "_CustomPlayButton_SelectedScenePath";
        }

        public static GUIContent CreateIconContent(string localTex, string builtInTex, string tooltip)
        {
            var tex = LoadTexture(localTex);
            if (tex != null) return new GUIContent(tex, tooltip);
            return EditorGUIUtility.IconContent(builtInTex, tooltip);
        }

        public static Texture2D LoadTexture(string path)
        {
            return (Texture2D)EditorGUIUtility.Load(ICONS_PATH + path);
        }

        static void StartScene(SceneAsset scene)
        {
            if (EditorApplication.isPlaying)
            {
                s_LastScene = scene;
                EditorApplication.isPlaying = false;
            }
            else
            {
                ChangeScene(scene);
            }
        }

        static void OnUpdate()
        {
            if (s_LastScene == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ChangeScene(s_LastScene);
            s_LastScene = null;
        }

        static void ChangeScene(SceneAsset scene)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = scene;
                EditorApplication.isPlaying = true;
            }
        }

        static void HandleOnPlayModeChanged(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.ExitingPlayMode)
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}
#endif
