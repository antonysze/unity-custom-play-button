using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ASze.CustomPlayButton
{
    public class SceneBookmark : ScriptableObject
    {
        public List<SceneAsset> bookmarks = new List<SceneAsset>();

        public bool HasBookmark()
        {
            return bookmarks != null && bookmarks.Count > 0;
        }

        public void RemoveNullValue()
        {
            bookmarks.RemoveAll(item => item == null);
        }
    }
}
