﻿using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityToolbarExtender;
using System.IO;
using System.Reflection;

#if UNITY_2019_1_OR_NEWER
using VisualElement = UnityEngine.UIElements.VisualElement;
#else
using VisualElement = UnityEngine.Experimental.UIElements.VisualElement;
#endif

namespace ASze.CustomPlayButton
{
    [InitializeOnLoad]
    public static class CustomPlayButton
    {
        const string FOLDER_PATH = "Assets/Editor/CustomPlayButton/";
        const string SETTING_PATH = FOLDER_PATH + "BookmarkSetting.asset";
        const string ICONS_PATH = "Packages/com.antonysze.custom-play-button/Icons/";

        private static SceneBookmark bookmark = null;
        private static SceneAsset selectedScene = null;


        static GUIContent customSceneContent;
        static GUIContent gameSceneContent;

        static Rect buttonRect;
        static VisualElement toolbarElement;
        static SceneAsset lastScene = null;

        public static SceneBookmark Bookmark
        {
            get
            {
                if (bookmark == null)
                {
                    bookmark = ScriptableObject.CreateInstance<SceneBookmark>();
                    if (!Directory.Exists(FOLDER_PATH))
                        Directory.CreateDirectory(FOLDER_PATH);
                    AssetDatabase.CreateAsset(bookmark, SETTING_PATH);
                    AssetDatabase.Refresh();
                }
                return bookmark;
            }
        }

        public static SceneAsset SelectedScene
        {
            get { return selectedScene; }
            set
            {
                selectedScene = value;
                toolbarElement?.MarkDirtyRepaint();

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

        static class ToolbarStyles
        {
            public static readonly GUIStyle commandButtonStyle;

            static ToolbarStyles()
            {
                EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
                EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
                commandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        static CustomPlayButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarLeftGUI);
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            if (bookmark == null)
            {
                bookmark = AssetDatabase.LoadAssetAtPath<SceneBookmark>(SETTING_PATH);
                bookmark.RemoveNullValue();
            }

            var savedScenePath = EditorPrefs.GetString(GetEditorPrefKey(), "");
            SelectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(savedScenePath);
            if (SelectedScene == null && EditorBuildSettings.scenes.Length > 0)
            {
                var scenePath = EditorBuildSettings.scenes[0].path;
                SelectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }

            customSceneContent = CreateIconContent("PlaySceneButton.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Custom Scene");
            gameSceneContent = CreateIconContent("PlayGameButton.png", "d_UnityEditor.GameView@2x", "Play Game Scene");
        }

        static void OnToolbarLeftGUI()
        {
            GUILayout.FlexibleSpace();

            var sceneName = selectedScene != null ? selectedScene.name : "Select Scene...";
            var selected = EditorGUILayout.DropdownButton(new GUIContent(sceneName), FocusType.Passive, GUILayout.Width(128.0f));
            if (Event.current.type == EventType.Repaint)
            {
                buttonRect = GUILayoutUtility.GetLastRect();
            }

            if (selected)
            {
                PopupWindow.Show(buttonRect, new EditorSelectScenePopup());
            }

            if (GUILayout.Button(customSceneContent, ToolbarStyles.commandButtonStyle))
            {
                StartScene(selectedScene);
            }

            if (EditorBuildSettings.scenes.Length > 0)
            {
                if (GUILayout.Button(gameSceneContent, ToolbarStyles.commandButtonStyle))
                {
                    var scenePath = EditorBuildSettings.scenes[0].path;
                    var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                    StartScene(scene);
                }
            }
        }

        static void StartScene(SceneAsset scene)
        {
            if (EditorApplication.isPlaying)
            {
                lastScene = scene;
                EditorApplication.isPlaying = false;
            }
            else
            {
                ChangeScene(scene);
            }
        }

        static void OnUpdate()
        {
            // Get toolbar element for repainting
            if (toolbarElement == null)
            {
                var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                var currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (currentToolbar != null)
                {
                    var guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
                    var viewVisualTree = guiViewType.GetProperty("visualTree",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    toolbarElement = (VisualElement)viewVisualTree.GetValue(currentToolbar, null);
                }
            }

            if (lastScene == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ChangeScene(lastScene);
            lastScene = null;
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

        public static string GetEditorPrefKey()
        {
            var projectPrefix = PlayerSettings.companyName + "." + PlayerSettings.productName;
            return projectPrefix + "_CustomPlayButton_SelectedScenePath";
        }

        public static GUIContent CreateIconContent(string localTex, string builtInTex, string tooltip)
        {
            var tex = LoadTexture(localTex);
            if (tex != null) return new GUIContent(tex, tooltip);
            else return EditorGUIUtility.IconContent(builtInTex, tooltip);
        }

        public static Texture2D LoadTexture(string path)
        {
            return (Texture2D)EditorGUIUtility.Load(ICONS_PATH + path);
        }
    }
}