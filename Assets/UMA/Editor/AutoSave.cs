﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace UnityMadeAwesome.UnityAutoSaver
{
[InitializeOnLoad]
public class AutoSave : ScriptableObject
{

    private const int TIME_BEFORE_SAVE = 30; // Seconds

    private static double timeTilSave = 0;
    private static bool canSave = false;

    static AutoSave()
    {
        EditorApplication.update += Update;

        // So we don't save too often, we'll make sure TIME_BEFORE_SAVE time has passed before the first
        // auto save.
        timeTilSave = EditorApplication.timeSinceStartup + TIME_BEFORE_SAVE;

        canSave = false;
    }

    static void Update()
    {
        // Are we enabled?
        if (!Data.autoSaveEnabled || EditorApplication.currentScene == "")
        {
            return;
        }

        if (!canSave)
        {
            if (EditorApplication.timeSinceStartup > timeTilSave)
            {
                // Open it up! And set next auto save to 5 minutes.
                timeTilSave = EditorApplication.timeSinceStartup + Data.autoSaveFrequency * 60;
                canSave = true;
            }

            // Not allowed to save yet.
            return;
        }

        // Should we save?
        if (EditorApplication.isPlayingOrWillChangePlaymode || 
            EditorApplication.isCompiling || 
            EditorApplication.timeSinceStartup > timeTilSave)
        {
            // SSSSSSSSAAAAAAAAAAAVVVVVVVVVVEEEEEEEEEEEE!
            canSave = false;
            timeTilSave = EditorApplication.timeSinceStartup + Data.autoSaveFrequency * 60;

            ScriptableObject autoSaveObj = null;
            string autosaveFolder;

            try
            {
                // Is this really how I have to do this to get the path to the autosave folder? This feel less ideal.
                autoSaveObj = ScriptableObject.CreateInstance<AutoSave>();
                autosaveFolder = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(autoSaveObj));
            }
            finally
            {
                if (autoSaveObj != null)
                {
                    ScriptableObject.DestroyImmediate(autoSaveObj);
                }
            }

            // May not work on mac?
            autosaveFolder = autosaveFolder.Replace("Editor/AutoSave.cs", "AutoSaves/");

            string[] pathSplit = EditorApplication.currentScene.Split('/');
            string sceneName = pathSplit[pathSplit.Length - 1];
            pathSplit[pathSplit.Length - 1] = "";

            string dirPath = autosaveFolder + string.Join("/", pathSplit);

            DirectoryInfo dir = new DirectoryInfo(dirPath);

            if (!dir.Exists)
            {
                // TODO: Unity will complain once on creation about the folder, be nice to remove that.
                dir.Create();
            }

            int index = sceneName.IndexOf(".unity");
            string origSceneName = sceneName;

            // I want to try saving before bumping files down so we'll save as _0 and just bump to _1.
            sceneName = sceneName.Insert(index, "_0");

            string filePath = dirPath + sceneName;

            bool success = EditorApplication.SaveScene(filePath, true);

            if (!success)
            {
                Debug.LogWarning(Data.PACKAGE_NAME + " - Scene auto save failed.");
            }
            else
            {
                string oldSceneName = dirPath + origSceneName.Insert(index, "_" + Data.savesToKeep);

                if (File.Exists(oldSceneName))
                {
                    File.Delete(oldSceneName);
                }

                // Go through and bump down the current files by an increment.
                for (int i = Data.savesToKeep - 1; i >= 0; i--)
                {
                    oldSceneName = dirPath + origSceneName.Insert(index, "_" + i);

                    if (!File.Exists(oldSceneName))
                    {
                        continue;
                    }
                    string newSceneName = dirPath + origSceneName.Insert(index, "_" + (i + 1));
                    File.Copy(oldSceneName, newSceneName);
                    File.Delete(oldSceneName);
                }
            }
        }
    }
}
}