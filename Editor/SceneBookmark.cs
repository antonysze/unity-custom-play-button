#if UNITY_TOOLBAR_EXTENDER
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using System;

namespace ASze.CustomPlayButton
{
    public static class SceneBookmark
    {
        [Serializable]
        private class BookmarksData
        {
            public string[] bookmarkGUIDs;

            public BookmarksData(GUID[] gUIDs)
            {
                bookmarkGUIDs = gUIDs.Select(g => g.ToString()).ToArray(); ;
            }
        }

        private const string EDITOR_PREF_KEY = "ASze.CustomPlayButton.SceneBookmark";

        private static List<SceneAsset> _bookmarks;

        public static List<SceneAsset> Bookmarks
        {
            get
            {
                if (_bookmarks == null)
                {
                    LoadBookmarks();
                }
                return _bookmarks;
            }
        }

        public static void AddBookmark(SceneAsset sceneAsset)
        {
            if (_bookmarks == null)
            {
                LoadBookmarks();
            }

            if (sceneAsset == null || !_bookmarks.Contains(sceneAsset))
            {
                _bookmarks.Add(sceneAsset);
                SaveBookMarks();
            }
        }

        public static void RemoveBookmark(SceneAsset sceneAsset)
        {
            if (_bookmarks == null)
            {
                LoadBookmarks();
            }

            if (sceneAsset != null && _bookmarks.Contains(sceneAsset))
            {
                _bookmarks.Remove(sceneAsset);
                SaveBookMarks();
            }
        }

        public static void RemoveBookmarkAt(int index)
        {
            if (_bookmarks == null)
            {
                LoadBookmarks();
            }

            if (index >= 0 && index < _bookmarks.Count)
            {
                _bookmarks.RemoveAt(index);
                SaveBookMarks();
            }
        }

        public static void RemoveLastBookmark()
        {
            if (_bookmarks == null)
            {
                LoadBookmarks();
            }

            RemoveBookmarkAt(_bookmarks.Count - 1);
        }

        public static void RemoveAll()
        {
            _bookmarks?.Clear();
            SaveBookMarks();
        }

        public static bool HasBookmark()
        {
            return _bookmarks != null && _bookmarks.Count > 0;
        }

        public static void SaveBookMarks()
        {
            if (_bookmarks != null)
            {
                NativeArray<int> instanceIDs = new NativeArray<int>(_bookmarks.Select(b => b?.GetInstanceID() ?? 0).ToArray(), Allocator.TempJob);
                NativeArray<GUID> guids = new NativeArray<GUID>(instanceIDs.Length, Allocator.TempJob);

                AssetDatabase.InstanceIDsToGUIDs(instanceIDs, guids);

                if (guids != null && guids.Length > 0)
                {
                    string json = JsonUtility.ToJson(new BookmarksData(guids.ToArray()));
                    EditorPrefs.SetString(EDITOR_PREF_KEY, json);
                }
                else
                {
                    EditorPrefs.DeleteKey(EDITOR_PREF_KEY);
                }

                instanceIDs.Dispose();
                guids.Dispose();
            }
        }

        private static void LoadBookmarks()
        {
            _bookmarks = new List<SceneAsset>();

            string json = EditorPrefs.GetString(EDITOR_PREF_KEY, null);
            if (!string.IsNullOrEmpty(json))
            {
                BookmarksData data = JsonUtility.FromJson<BookmarksData>(json);
                GUID[] guids = data?.bookmarkGUIDs.Select(g => new GUID(g)).ToArray();
                if (guids != null && guids.Length > 0)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                        _bookmarks.Add(sceneAsset);
                    }
                }
            }
        }
    }
}
#endif
