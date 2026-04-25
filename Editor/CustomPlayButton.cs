// Unity 6000.3 is the first version that supports the MainToolbar API
// CustomPlayButtonMainToolbar.cs is used for Unity 6000.3 and above
#if !UNITY_6000_3_OR_NEWER

using UnityEngine;
using UnityEditor;
using System.Reflection;

#if UNITY_TOOLBAR_EXTENDER
using UnityToolbarExtender;
#else
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

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
#if UNITY_TOOLBAR_EXTENDER
        static GUIContent customSceneContent;
        static GUIContent gameSceneContent;

        static Rect buttonRect;
        static VisualElement toolbarElement;

        static class ToolbarStyles
        {
            public static readonly GUIStyle commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold,
            };
        }

        static CustomPlayButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarLeftGUI);
            EditorApplication.update += FindToolbarElement;
            CustomPlayButtonCore.SelectedSceneChanged += () => toolbarElement?.MarkDirtyRepaint();

            customSceneContent = CustomPlayButtonCore.CreateIconContent(
                "PlaySceneButton.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Custom Scene");
            gameSceneContent = CustomPlayButtonCore.CreateIconContent(
                "PlayGameButton.png", "d_UnityEditor.GameView@2x", "Play Game Scene");
        }

        static void OnToolbarLeftGUI()
        {
            GUILayout.FlexibleSpace();

            var scene = CustomPlayButtonCore.SelectedScene;
            var sceneName = scene != null ? scene.name : "Select Scene...";
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
                CustomPlayButtonCore.PlayCustomScene();
            }

            if (GUILayout.Button(gameSceneContent, ToolbarStyles.commandButtonStyle))
            {
                CustomPlayButtonCore.PlayGameScene();
            }
        }

        static void FindToolbarElement()
        {
            if (toolbarElement != null) return;

            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            var currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            if (currentToolbar == null) return;

            var guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
#if UNITY_2020_1_OR_NEWER
            var iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
            var guiBackend = guiViewType.GetProperty("windowBackend",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var viewVisualTree = iWindowBackendType.GetProperty("visualTree",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var windowBackend = guiBackend.GetValue(currentToolbar);
            toolbarElement = (VisualElement)viewVisualTree.GetValue(windowBackend, null);
#else
            var viewVisualTree = guiViewType.GetProperty("visualTree",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            toolbarElement = (VisualElement)viewVisualTree.GetValue(currentToolbar, null);
#endif
        }
#else
        static AddRequest request;

        static CustomPlayButton()
        {
            if (!EditorUtility.DisplayDialog(
                "Cannot activate Custom Play Button",
                "Prerequisite package is needed for \"unity-custom-play-button\".\nPlease install package \"unity-toolbar-extender\"(https://github.com/marijnz/unity-toolbar-extender.git).",
                "Ok", "Install package"))
            {
                request = Client.Add("https://github.com/marijnz/unity-toolbar-extender.git");
                EditorApplication.update += Progress;
            }
        }

        static void Progress()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure)
                    Debug.Log(request.Error.message);

                EditorApplication.update -= Progress;
            }
        }
#endif
    }
}

#endif