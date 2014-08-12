using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.UnityAutoSaver
{
    [InitializeOnLoad]
    public class Data
    {
        //
        // Unconfigurable Data
        //

        public const string PACKAGE_NAME = "UMA.UAS";

        //
        // Configurable data.
        //

        //
        // Auto Save
        //

        public static bool autoSaveEnabled = true;
        public static float autoSaveFrequency = 5f; // Minutes
        public static int savesToKeep = 5;

        static Data()
        {
            // Loaded up, load up da settings.
            Data.LoadData();
        }

        public static void SaveData()
        {
            // TODO: I bet there's a way to do this easier with SerializeObject, explore later. If not, consider a cool solution
            //       using reflection later. This is currently a little unwieldy.
            EditorPrefs.SetBool(PACKAGE_NAME + " - AS", autoSaveEnabled);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - AS Freq", autoSaveFrequency);
            EditorPrefs.SetInt(PACKAGE_NAME + " - AS Saves", savesToKeep);
        }

        public static void LoadData()
        {
            // If the first key is missing then just assume we have no data to load and go with defaults.
            if (!EditorPrefs.HasKey(PACKAGE_NAME + " - AS"))
            {
                return;
            }

            autoSaveEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - AS");
            autoSaveFrequency = EditorPrefs.GetFloat(PACKAGE_NAME + " - AS Freq");
            savesToKeep = EditorPrefs.GetInt(PACKAGE_NAME + " - AS Saves");
        }
    }
}