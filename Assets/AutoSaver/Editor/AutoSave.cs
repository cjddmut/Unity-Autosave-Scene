using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityMadeAwesome.UnityAutoSaver
{
    [InitializeOnLoad]
    public class AutoSave : ScriptableObject
    {
        private static double _lastSaveTime;

        private static string _currentScene;

        private static string _autoSaveFolder;
        private static string _autoSaveFile;
        private static string _originalSceneName;
        private static int _indexInSceneFile;

        private const string SAVE_TIME_KEY = Data.PACKAGE_NAME + " LST";

        static AutoSave()
        {
            if (Data.autoSaveEnabled)
            {
                Initialize();
            }

            Data.onAutoSaveEnabled += Initialize;
            Data.onAutoSaveDisabled += Uninitialize;
        }

        private static void Initialize()
        {
            EditorApplication.update += Update;

            if (EditorPrefs.HasKey(SAVE_TIME_KEY))
            {
                _lastSaveTime = EditorPrefs.GetFloat(SAVE_TIME_KEY);
            }

            if (_lastSaveTime > EditorApplication.timeSinceStartup)
            {
                _lastSaveTime = EditorApplication.timeSinceStartup;
                EditorPrefs.SetFloat(SAVE_TIME_KEY, (float)_lastSaveTime);
            }

            NewScene(EditorApplication.currentScene);
        }

        private static void Uninitialize()
        {
            EditorApplication.update -= Update;
        }

        private static void NewScene(string newScene)
        {
            _currentScene = newScene;

            if (_currentScene != "")
            {
                ScriptableObject autoSaveObj = null;
                string autosaveFolder;

                try
                {
                    // Is this really how I have to do this to get the path to the autosave folder? This feel less ideal.
                    autoSaveObj = CreateInstance<AutoSave>();
                    autosaveFolder = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(autoSaveObj));
                }
                finally
                {
                    if (autoSaveObj != null)
                    {
                        DestroyImmediate(autoSaveObj);
                    }
                }

                autosaveFolder = autosaveFolder.Replace("Editor/AutoSave.cs", "AutoSaves/");

                string[] pathSplit = newScene.Split('/');
                string sceneName = pathSplit[pathSplit.Length - 1];
                pathSplit[pathSplit.Length - 1] = "";

                _autoSaveFolder = autosaveFolder + string.Join("/", pathSplit);

                _indexInSceneFile = sceneName.IndexOf(".unity");
                _originalSceneName = sceneName;

                // I want to try saving before bumping files down so we'll save as _0 and just bump to _1.
                sceneName = sceneName.Insert(_indexInSceneFile, "_0");

                _autoSaveFile = _autoSaveFolder + sceneName;
            }
        }

        private static void Update()
        {
            // We reset our last time saved if the scene changes.
            if (EditorApplication.currentScene != _currentScene)
            {
                _lastSaveTime = EditorApplication.timeSinceStartup;
                NewScene(EditorApplication.currentScene);
            }

            if (_currentScene == "")
            {
                return;
            }

            if (EditorApplication.timeSinceStartup > (_lastSaveTime + Data.autoSaveFrequency * 60) && !EditorApplication.isPlaying)
            {
                _lastSaveTime = EditorApplication.timeSinceStartup;
                EditorPrefs.SetFloat(SAVE_TIME_KEY, (float)_lastSaveTime);

                DirectoryInfo dir = new DirectoryInfo(_autoSaveFolder);

                if (!dir.Exists)
                {
                    // TODO: Unity will complain once on creation about the folder, be nice to remove that.
                    dir.Create();
                }

                bool success = EditorApplication.SaveScene(_autoSaveFile, true);

                if (!success)
                {
                    Debug.LogWarning(Data.PACKAGE_NAME + " - Scene auto save failed.");
                }
                else
                {
                    string oldSceneName = _autoSaveFolder + _originalSceneName.Insert(_indexInSceneFile, "_" + Data.savesToKeep);
                    string oldMetaFile = oldSceneName + ".meta";

                    if (File.Exists(oldSceneName))
                    {
                        File.Delete(oldSceneName);
                    }

                    if (File.Exists((oldMetaFile)))
                    {
                        File.Delete(oldMetaFile);
                    }

                    // Go through and bump down the current files by an increment.
                    for (int i = Data.savesToKeep - 1; i >= 0; i--)
                    {
                        oldSceneName = _autoSaveFolder + _originalSceneName.Insert(_indexInSceneFile, "_" + i);

                        if (!File.Exists(oldSceneName))
                        {
                            continue;
                        }

                        string newSceneName = _autoSaveFolder + _originalSceneName.Insert(_indexInSceneFile, "_" + (i + 1));
                        File.Copy(oldSceneName, newSceneName);
                        File.Delete(oldSceneName);

                        // Move the meta file as well.
                        oldMetaFile = oldSceneName + ".meta";

                        if (File.Exists(oldMetaFile))
                        {
                            string newMetaFile = _autoSaveFolder + _originalSceneName.Insert(_indexInSceneFile, "_" + (i + 1)) + ".meta";
                            File.Copy(oldMetaFile, newMetaFile);
                            File.Delete(oldMetaFile);
                        }
                    }
                }
            }
        }
    }
}