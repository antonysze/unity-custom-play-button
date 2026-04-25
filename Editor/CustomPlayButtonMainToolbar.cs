#if UNITY_6000_3_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace ASze.CustomPlayButton
{
    [InitializeOnLoad]
    public static class CustomPlayButtonMainToolbar
    {
        const string k_DropdownId   = "CustomPlayButton/SceneSelect";
        const string k_PlayCustomId = "CustomPlayButton/PlayCustom";
        const string k_PlayGameId   = "CustomPlayButton/PlayGame";

        static CustomPlayButtonMainToolbar()
        {
            CustomPlayButtonCore.SelectedSceneChanged += () => MainToolbar.Refresh(k_DropdownId);
        }

        [MainToolbarElement(k_DropdownId, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -3)]
        static MainToolbarElement CreateSceneDropdown()
        {
            var scene = CustomPlayButtonCore.SelectedScene;
            var sceneName = scene != null ? scene.name : "Select Scene...";
            return new MainToolbarDropdown(
                new MainToolbarContent(sceneName, "Select scene to play"),
                rect => PopupWindow.Show(rect, new EditorSelectScenePopup()));
        }

        [MainToolbarElement(k_PlayCustomId, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -2)]
        static MainToolbarElement CreatePlayCustomButton()
            => MakeButton("PlaySceneButton.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Selected Scene", CustomPlayButtonCore.PlayCustomScene);

        [MainToolbarElement(k_PlayGameId, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
        static MainToolbarElement CreatePlayGameButton()
            => MakeButton("PlayGameButton.png", "d_UnityEditor.GameView@2x", "Play Main Scene", CustomPlayButtonCore.PlayGameScene);

        static MainToolbarButton MakeButton(string localTex, string builtInTex, string tooltip, Action onClick)
        {
            var gui = CustomPlayButtonCore.CreateIconContent(localTex, builtInTex, tooltip);
            return new MainToolbarButton(new MainToolbarContent(gui.image as Texture2D, tooltip), onClick);
        }
    }
}
#endif
