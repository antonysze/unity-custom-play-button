#if UNITY_TOOLBAR_EXTENDER && UNITY_6000_0_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditorInternal; // Required for ReorderableList
using System.Collections.Generic;
using ASze.CustomPlayButton;
using System.Linq;

public class SceneListSettingsProvider : SettingsProvider
{
    private const string BOOKMARKS_SETTING_PATH = "Custom Play Button/Bookmarks";
    private ReorderableList reorderableList;
    private float columnWidth;

    // Constructor required by SettingsProvider
    public SceneListSettingsProvider(string path, SettingsScope scopes = SettingsScope.Project)
        : base(path, scopes)
    {
    }
    
    // This method is called when the provider is activated.
    // Use it to set up your ReorderableList.
    public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
    {
        // Initialize the ReorderableList
        reorderableList = new ReorderableList(SceneBookmark.Bookmarks, typeof(string), true, true, true, true);

        // Set up the header drawing
        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Scenes in List");
        };

        // Set up how each element is drawn
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            // Ensure the list has enough elements before trying to access
            if (index >= SceneBookmark.Bookmarks.Count)
            {
                // This can happen if elements are removed while drawing
                return;
            }

            SceneAsset sceneAsset = SceneBookmark.Bookmarks[index];

            EditorGUI.BeginChangeCheck();
            sceneAsset = (SceneAsset)EditorGUI.ObjectField(rect, sceneAsset, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (SceneBookmark.Bookmarks.Contains(sceneAsset))
                {
                    // If the scene is already in the list, do not add it again
                    return;
                }
                SceneBookmark.Bookmarks[index] = sceneAsset;
                SceneBookmark.SaveBookMarks();
            }
        };

        // Set up adding new elements
        reorderableList.onAddCallback = (ReorderableList list) =>
        {
            SceneBookmark.AddBookmark(null); // Add a new empty bookmark
            // Save done in the function above
        };

        // Set up removing elements
        reorderableList.onRemoveCallback = (ReorderableList list) =>
        {
            SceneBookmark.RemoveLastBookmark();
                // Save done in the function above
        };

        // Set up reordering callback
        reorderableList.onReorderCallback = (ReorderableList list) =>
        {
            SceneBookmark.SaveBookMarks();
        };

        // Set up height for each element
        reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        columnWidth = EditorSelectScenePopup.GetColumnWidth();
    }

    // This method is called to draw the UI for your settings.
    public override void OnGUI(string searchContext)
    {
        // Draw the ReorderableList
        reorderableList.DoLayoutList();

        // Add a horizontal line for separation
        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        if (GUILayout.Button("Clear All Scenes"))
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to clear the entire scene list?", "Clear", "Cancel"))
            {
                SceneBookmark.RemoveAll();
                // If you clear the list, the ReorderableList needs to know to update
                // its internal state. Re-initializing it effectively refreshes it.
                // Or you might need reorderableList.displayAdd, displayRemove etc. to be managed.
                // For a full clear, re-creating is simple.
                reorderableList = new ReorderableList(SceneBookmark.Bookmarks.ToList(), typeof(string), true, true, true, true);
            }
        }

        EditorGUI.BeginChangeCheck();
        columnWidth = EditorGUILayout.FloatField("Popup Column Width", columnWidth);
        if (EditorGUI.EndChangeCheck())
        {
            EditorSelectScenePopup.SaveColumnWidth(columnWidth);
        }

        // Inform the user about persistence
        EditorGUILayout.HelpBox("Scene list changes are automatically saved to EditorPrefs.", MessageType.Info);
    }


    // This static method registers the SettingsProvider with Unity.
    // The path here should match the path in the constructor.
    [SettingsProvider]
    public static SettingsProvider CreateSceneListSettingsProvider()
    {
        var provider = new SceneListSettingsProvider("Project/My Company/Scene List", SettingsScope.Project);

        // You can add keywords to make your settings searchable
        provider.keywords = new HashSet<string>(new[] { "Scene", "List", "Build", "Order", "Paths" });
        return provider;
    }

#region Editor Menu
    [SettingsProvider]
    public static SettingsProvider CustomSettings_Bookmarks()
    {
        var provider = new SceneListSettingsProvider(BOOKMARKS_SETTING_PATH, SettingsScope.Project);
        return provider;
    }
    
    public static void OpenBookmarkSettings()
    {
        SettingsService.OpenProjectSettings(BOOKMARKS_SETTING_PATH);
    }
#endregion
}
#endif
