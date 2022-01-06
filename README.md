# Unity Custom Play Button
Add 2 custom play buttons to the Unity Toolbar to avoid keep switching scenes when testing the game. Directly play the game (the first scene in build setting) or specific scene (chose from dropdown list) by clicking those buttons without changing scene. When stop playing, scene will change back to the scene before play.  Additional dropdown list for quick build or bookmarked scenes switching without searching.

![image](https://user-images.githubusercontent.com/3353695/148316557-47d93af2-fb9d-46b8-97da-9ce409e02317.png)

## Installation
Import this from Unity Package Manager. You can [download and import it from your hard drive](https://docs.unity3d.com/Manual/upm-ui-local.html), or [link to it from github directly](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

## Requirements
Please make sure following package is installed to make this package works:
- [unity-toolbar-extender](https://github.com/marijnz/unity-toolbar-extender) - 1.4.1 or above

You can also install the prerequisite package via popup window after you installed this package:
![image](https://user-images.githubusercontent.com/3353695/148312273-2188311b-fe3e-4a4b-87ea-00ccaead8aef.png)

## How to use
![image](https://user-images.githubusercontent.com/3353695/148320339-2efc85a4-fc4b-44d7-bd84-662ff9e34c52.gif)

### Play Buttons on Unity Toobar
![image](https://user-images.githubusercontent.com/3353695/148315309-e6369f75-5a44-4684-8848-f59341058443.png)
1. **Dropdown button of custom scene** - open scene selection window
2. **Play custom scene button** - play target scene from dropdown button
3. **Play game button** - play the game (the first scene in build setting)

### Scene selection window
![image](https://user-images.githubusercontent.com/3353695/148315308-dc98f926-4faa-4a77-b58a-2409b8bf4a2d.png)
1. Select custom scene (for **play custom scene button**)
2. Bookmark/Unbookmark
3. Open scene in scene view
4. Select bookmark scriptable object
5. Unbookmark

### Scene Bookmark
Please note that bookmark is stored in a bookmark scripable object in your project. Please add to .gitignore if you do not want to share it. This scriptable object will be automatucally created from `Assets/Editor/CustomPlayButton/BookmarkSetting.asset`. You can also edit or reorder the list of bookmarks by modifying the scripable object directly.
