using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PersonHandPositionBatchEditor : EditorWindow
{
    private enum ApplyResult
    {
        Updated,
        AlreadyMatching,
        Skipped
    }

    private const string DefaultBasePrefabPath = "Assets/Prefab/Persons/PersonTemplete.prefab";

    private Vector3 handPosition = new Vector3(0f, 1.25f, 0f);
    private string foldersText = "Assets/Prefab/Persons\nAssets/Prefab/PersonTemplate";
    private bool includeInactive = true;

    [MenuItem("Tools/Person/Batch Hand Position")]
    public static void ShowWindow()
    {
        GetWindow<PersonHandPositionBatchEditor>("Batch Hand Position");
    }

    private void OnEnable()
    {
        TryPullFromPrefab(DefaultBasePrefabPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Person Hand Position", EditorStyles.boldLabel);
        handPosition = EditorGUILayout.Vector3Field("Hand Position", handPosition);
        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Prefab Folders");
        foldersText = EditorGUILayout.TextArea(foldersText, GUILayout.MinHeight(48f));

        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pull From Base Prefab", GUILayout.Height(28f)))
            TryPullFromPrefab(DefaultBasePrefabPath, true);

        if (GUILayout.Button("Apply To All Person Prefabs", GUILayout.Height(28f)))
            ApplyToAllPersonPrefabs();
        EditorGUILayout.EndHorizontal();
    }

    private void TryPullFromPrefab(string prefabPath, bool showDialog = false)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Batch Hand Position", $"Could not load {prefabPath}.", "OK");
            return;
        }

        PersonVisual visual = prefab.GetComponentInChildren<PersonVisual>(includeInactive);
        if (visual == null)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Batch Hand Position", "Base prefab has no PersonVisual.", "OK");
            return;
        }

        SerializedObject serializedVisual = new SerializedObject(visual);
        SerializedProperty handPositionProperty = serializedVisual.FindProperty("handPosition");
        if (handPositionProperty != null)
            handPosition = handPositionProperty.vector3Value;
    }

    private void ApplyToAllPersonPrefabs()
    {
        string[] folders = GetValidFolders();
        if (folders.Length == 0)
        {
            EditorUtility.DisplayDialog("Batch Hand Position", "No valid prefab folders found.", "OK");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", folders);
        int updatedCount = 0;
        int alreadyMatchingCount = 0;
        int skippedCount = 0;

        try
        {
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar(
                    "Batch Hand Position",
                    prefabPath,
                    prefabGuids.Length == 0 ? 1f : (float)i / prefabGuids.Length);

                ApplyResult result = ApplyToPrefab(prefabPath);
                switch (result)
                {
                    case ApplyResult.Updated:
                        updatedCount++;
                        break;
                    case ApplyResult.AlreadyMatching:
                        alreadyMatchingCount++;
                        break;
                    default:
                        skippedCount++;
                        break;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog(
            "Batch Hand Position",
            $"Updated {updatedCount} prefab(s).\nAlready matching {alreadyMatchingCount} prefab(s).\nSkipped {skippedCount} prefab(s) without PersonVisual.",
            "OK");
    }

    private ApplyResult ApplyToPrefab(string prefabPath)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            PersonVisual visual = prefabRoot.GetComponentInChildren<PersonVisual>(includeInactive);
            if (visual == null) return ApplyResult.Skipped;

            SerializedObject serializedVisual = new SerializedObject(visual);
            SerializedProperty handPositionProperty = serializedVisual.FindProperty("handPosition");
            if (handPositionProperty == null) return ApplyResult.Skipped;

            if (handPositionProperty.vector3Value == handPosition) return ApplyResult.AlreadyMatching;

            handPositionProperty.vector3Value = handPosition;
            serializedVisual.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            return ApplyResult.Updated;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private string[] GetValidFolders()
    {
        string[] lines = foldersText.Split('\n');
        List<string> folders = new List<string>();

        foreach (string line in lines)
        {
            string folder = line.Trim();
            if (string.IsNullOrEmpty(folder)) continue;
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            if (folders.Contains(folder)) continue;

            folders.Add(folder);
        }

        return folders.ToArray();
    }
}
