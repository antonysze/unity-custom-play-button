#if UNITY_TOOLBAR_EXTENDER
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace ASze.CustomPlayButton
{
    public class EditorSelectScenePopup : PopupWindowContent
    {
        const float COLLUMN_WIDTH = 200.0f;
        const float ICON_SIZE = 20.0f;
        readonly GUILayoutOption[] ICON_LAYOUT = new GUILayoutOption[] {
            GUILayout.Width(ICON_SIZE), GUILayout.Height(ICON_SIZE)
        };


        GUIStyle titleButtonStyle;
        GUIStyle buttonStyle;
        GUIStyle selectedButtonStyle;
        GUIContent bookmarkContent;
        SceneAsset[] buildScenes;
        SceneAsset currentScene;

        Vector2 scrollPosBuild;
        Vector2 scrollPosBookmark;

        public EditorSelectScenePopup() : base()
        {
            InitStyles();

            bookmarkContent = EditorGUIUtility.IconContent("blendKeySelected", "Bookmark ScriptableObject");

            GetBuildScenes();
            currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorSceneManager.GetActiveScene().path);
        }

        void InitStyles()
        {
            var blankTex = MakeTex(new Color(0f, 0f, 0f, 0f));
            var selectedTex = MakeTex(new Color(0f, 0f, 0f, 0.3f));

            var hoverState = new GUIStyleState()
            {
                background = selectedTex,
                textColor = GUI.skin.button.onHover.textColor,
            };
            buttonStyle = new GUIStyle(GUI.skin.label)
            {
                onHover = hoverState,
                hover = hoverState,
            };
            buttonStyle.normal.background = blankTex;

            selectedButtonStyle = new GUIStyle(buttonStyle);
            selectedButtonStyle.normal.background = selectedTex;

            titleButtonStyle = new GUIStyle(EditorStyles.boldLabel);
            titleButtonStyle.onHover = buttonStyle.onHover;
            titleButtonStyle.hover = buttonStyle.hover;
            titleButtonStyle.normal.background = blankTex;
        }

        public static Texture2D MakeTex(Color col)
        {
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.SetPixel(0, 0, col);
            texture.Apply();
            return texture;
        }

        public override Vector2 GetWindowSize()
        {
            var width = COLLUMN_WIDTH * (SceneBookmark.HasBookmark() ? 2 : 1);
            var maxRow = Mathf.Max(buildScenes.Length, SceneBookmark.Bookmarks.Count, 1);
            var height = Mathf.Min(22 * maxRow + 26, Screen.currentResolution.height * 0.5f);
            return new Vector2(width, height);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            DrawBuildScenes();
            DrawBookmarkScenes();
            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.MouseMove && EditorWindow.mouseOverWindow == editorWindow)
                editorWindow?.Repaint();
        }

        void DrawBuildScenes()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Scenes in Build", EditorStyles.boldLabel, GUILayout.Height(20.0f));
            if (!SceneBookmark.HasBookmark())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("blendKeySelected"), titleButtonStyle, ICON_LAYOUT))
                {
                    SceneBookmark.OpenBookmarkSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (buildScenes.Length > 0)
            {
                scrollPosBuild = EditorGUILayout.BeginScrollView(scrollPosBuild);
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    DrawSelection(buildScenes[i], i, true);
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No scene in build setting");
            }
            EditorGUILayout.EndVertical();
        }

        void DrawBookmarkScenes()
        {
            if (!SceneBookmark.HasBookmark()) return;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(COLLUMN_WIDTH));

            var content = new GUIContent(bookmarkContent);
            content.text = " Bookmarks";
            if (GUILayout.Button(content, titleButtonStyle, GUILayout.Height(20.0f)))
            {
                SceneBookmark.OpenBookmarkSettings();
            }


            scrollPosBookmark = EditorGUILayout.BeginScrollView(scrollPosBookmark);
            var bookmarks = new List<SceneAsset>(SceneBookmark.Bookmarks);
            for (int i = 0; i < bookmarks.Count; i++)
            {
                DrawSelection(bookmarks[i], i);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawSelection(SceneAsset scene, int index = -1, bool bookmarkButton = false)
        {
            GUILayout.BeginHorizontal();
            var style = CustomPlayButton.SelectedScene == scene ? selectedButtonStyle : buttonStyle;
            string sceneName = scene != null ? scene.name : "<NOT FOUND>";
            if (GUILayout.Button(index >= 0 ? $"{index}\t{sceneName}" : sceneName, style))
            {
                SelectScene(scene);
            }

            if (scene != null)
            {
                 style = currentScene == scene ? selectedButtonStyle : buttonStyle;
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_BuildSettings.SelectedIcon", "Open Scene"), style, ICON_LAYOUT))
                {
                    OpenScene(scene);
                }
            }
            else
            {
                GUILayout.Space(ICON_SIZE);
            }
           

            if (bookmarkButton)
            {
                if (scene != null)
                {
                    bool inBookmark = SceneBookmark.Bookmarks.Contains(scene);
                    GUIContent content;
                    if (inBookmark)
                        content = EditorGUIUtility.IconContent("blendKeySelected", "Unbookmark");
                    else
                        content = EditorGUIUtility.IconContent("blendKeyOverlay", "Bookmark");
                    if (GUILayout.Button(content, buttonStyle, ICON_LAYOUT))
                    {
                        if (inBookmark)
                        {
                            SceneBookmark.RemoveBookmark(scene);
                        }
                        else
                        {
                            SceneBookmark.AddBookmark(scene);
                        }
                    }
                }
                else
                {
                    GUILayout.Space(ICON_SIZE);
                }
            }
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_P4_DeletedLocal", "Unbookmark"), buttonStyle, ICON_LAYOUT))
                {
                    SceneBookmark.RemoveBookmarkAt(index);
                }
            }

            GUILayout.EndHorizontal();
        }

        void SelectScene(SceneAsset scene)
        {
            if (scene == null) return;
            CustomPlayButton.SelectedScene = scene;
            editorWindow.Close();
        }

        void OpenScene(SceneAsset scene)
        {
            if (scene == null) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var scenePath = AssetDatabase.GetAssetPath(scene);
                EditorSceneManager.OpenScene(scenePath);
                currentScene = scene;
                // Recreate textures which are destoryed by OpenScene
                InitStyles();
            }
        }

        void GetBuildScenes()
        {
            List<SceneAsset> buildSceneList = new List<SceneAsset>();
            var settingScenes = EditorBuildSettings.scenes;
            foreach (var settingScene in settingScenes)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(settingScene.guid.ToString());
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (scene != null) buildSceneList.Add(scene);
            }
            buildScenes = buildSceneList.ToArray();
        }
    }
}
#endif
