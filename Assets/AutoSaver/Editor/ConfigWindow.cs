using UnityEditor;

namespace UnityMadeAwesome.UnityAutoSaver
{
    public class ConfigWindow : EditorWindow
    {
        [MenuItem("Window/Unity Made Awesome/Autosaver")]

        static void OpenWindow()
        {
            ConfigWindow window = (ConfigWindow)GetWindow(typeof(ConfigWindow));
            window.title = Data.PACKAGE_NAME + " Config";
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            // Autosave

            Data.autoSaveEnabled = EditorGUILayout.BeginToggleGroup("Auto Save Enabled", Data.autoSaveEnabled);
            Data.autoSaveFrequency = EditorGUILayout.FloatField("Frequency (minutes)", Data.autoSaveFrequency);
            Data.savesToKeep = EditorGUILayout.IntField("Number of Saves", Data.savesToKeep);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

        }

        void OnLostFocus()
        {
            // We lost focus, save data that might have changed.

            // I place the save data here instead of in OnDisable in case the window is left opened and "OnDisable" is never called. (Like
            // if the user does a bunch of work and then quits without playing or compiling scripts). Now we save when the window is
            // no longer being used. The only concern would be if there is a window where scripts can compile since I'm not sure if that
            // would trigger this function. If scripts will only compile based off of an action from the user then we should be fine since
            // that action will cause the window to lose focus.
            Data.SaveData();
        }
    }
}