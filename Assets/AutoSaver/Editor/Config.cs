using UnityEditor;

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

        public static float autoSaveFrequency = 5f; // Minutes
        public static int savesToKeep = 5;

        public static System.Action onAutoSaveEnabled;
        public static System.Action onAutoSaveDisabled;

        public static bool autoSaveEnabled
        {
            get
            {
                return _autoSaveEnabled;
            }
            set
            {
                if (_autoSaveEnabled != value)
                {
                    _autoSaveEnabled = value;

                    if (_autoSaveEnabled)
                    {
                        if (onAutoSaveEnabled != null)
                        {
                            onAutoSaveEnabled();
                        }
                    }
                    else
                    {
                        if (onAutoSaveDisabled != null)
                        {
                            onAutoSaveDisabled();
                        }
                    }
                }
            }
        }

        private static bool _autoSaveEnabled;

        static Data()
        {
            // Loaded up, load up da settings.
            LoadData();
        }

        public static void SaveData()
        {
            EditorPrefs.SetBool(PACKAGE_NAME + " - AS", autoSaveEnabled);
            EditorPrefs.SetFloat(PACKAGE_NAME + " - AS Freq", autoSaveFrequency);
            EditorPrefs.SetInt(PACKAGE_NAME + " - AS Saves", savesToKeep);
        }

        public static void LoadData()
        {
            // If the first key is missing then just assume we have no data to load and go with defaults.
            if (!EditorPrefs.HasKey(PACKAGE_NAME + " - AS"))
            {
                autoSaveEnabled = true;
                return;
            }

            autoSaveEnabled = EditorPrefs.GetBool(PACKAGE_NAME + " - AS");
            autoSaveFrequency = EditorPrefs.GetFloat(PACKAGE_NAME + " - AS Freq");
            savesToKeep = EditorPrefs.GetInt(PACKAGE_NAME + " - AS Saves");
        }
    }
}