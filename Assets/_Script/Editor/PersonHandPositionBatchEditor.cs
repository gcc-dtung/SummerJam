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

    private struct RendererSorting
    {
        public bool HasValue;
        public int SortingLayerID;
        public int SortingOrder;

        public void PullFrom(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                HasValue = false;
                return;
            }

            HasValue = true;
            SortingLayerID = renderer.sortingLayerID;
            SortingOrder = renderer.sortingOrder;
        }

        public bool ApplyTo(SpriteRenderer renderer)
        {
            if (!HasValue || renderer == null) return false;

            bool changed = false;
            if (renderer.sortingLayerID != SortingLayerID)
            {
                renderer.sortingLayerID = SortingLayerID;
                changed = true;
            }

            if (renderer.sortingOrder != SortingOrder)
            {
                renderer.sortingOrder = SortingOrder;
                changed = true;
            }

            return changed;
        }
    }

    private const string DefaultBasePrefabPath = "Assets/Prefab/Persons/PersonTemplete.prefab";
    private const string HandOnPersonObjectName = "HandOnPerson";
    private const string SkinRendererPropertyName = "skinRenderer";
    private const string FaceRendererPropertyName = "faceRenderer";
    private const string TraitRendererPropertyName = "traitRenderer";

    private Vector3 handPosition = new Vector3(0f, 1.25f, 0f);
    private string foldersText = "Assets/Prefab/Persons\nAssets/Prefab/PersonTemplate";
    private bool includeInactive = true;
    private bool syncHandPosition = true;
    private bool syncHandOnPerson = true;
    private bool syncSkinFaceTraitSorting = true;
    private RendererSorting skinSorting;
    private RendererSorting faceSorting;
    private RendererSorting traitSorting;

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
        syncHandPosition = EditorGUILayout.Toggle("Sync Hand Position", syncHandPosition);
        handPosition = EditorGUILayout.Vector3Field("Hand Position", handPosition);
        syncHandOnPerson = EditorGUILayout.Toggle("Sync HandOnPerson Renderer", syncHandOnPerson);
        syncSkinFaceTraitSorting = EditorGUILayout.Toggle("Sync Skin/Face/Trait Sorting", syncSkinFaceTraitSorting);
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

        PullSortingDefaults(serializedVisual);
    }

    private void ApplyToAllPersonPrefabs()
    {
        if (!syncHandPosition && !syncHandOnPerson && !syncSkinFaceTraitSorting)
        {
            EditorUtility.DisplayDialog("Batch Hand Position", "Enable at least one sync option.", "OK");
            return;
        }

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
            bool changed = false;
            bool missingSelectedData = false;

            if (syncHandPosition)
            {
                SerializedProperty handPositionProperty = serializedVisual.FindProperty("handPosition");
                if (handPositionProperty == null)
                {
                    missingSelectedData = true;
                }
                else if (handPositionProperty.vector3Value != handPosition)
                {
                    handPositionProperty.vector3Value = handPosition;
                    changed = true;
                }
            }

            if (syncHandOnPerson)
            {
                SerializedProperty handOnPersonProperty = serializedVisual.FindProperty("handOnPerson");
                SpriteRenderer handOnPersonRenderer = FindHandOnPersonRenderer(prefabRoot);

                if (handOnPersonProperty == null || handOnPersonRenderer == null)
                {
                    missingSelectedData = true;
                }
                else if (handOnPersonProperty.objectReferenceValue != handOnPersonRenderer)
                {
                    handOnPersonProperty.objectReferenceValue = handOnPersonRenderer;
                    changed = true;
                }
            }

            if (syncSkinFaceTraitSorting)
                changed |= ApplySortingDefaults(serializedVisual);

            if (!changed) return missingSelectedData ? ApplyResult.Skipped : ApplyResult.AlreadyMatching;

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

    private SpriteRenderer FindHandOnPersonRenderer(GameObject prefabRoot)
    {
        SpriteRenderer[] renderers = prefabRoot.GetComponentsInChildren<SpriteRenderer>(includeInactive);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.name == HandOnPersonObjectName)
                return renderer;
        }

        return null;
    }

    private void PullSortingDefaults(SerializedObject serializedVisual)
    {
        skinSorting.PullFrom(GetRenderer(serializedVisual, SkinRendererPropertyName));
        faceSorting.PullFrom(GetRenderer(serializedVisual, FaceRendererPropertyName));
        traitSorting.PullFrom(GetRenderer(serializedVisual, TraitRendererPropertyName));
    }

    private bool ApplySortingDefaults(SerializedObject serializedVisual)
    {
        bool changed = false;

        changed |= skinSorting.ApplyTo(GetRenderer(serializedVisual, SkinRendererPropertyName));
        changed |= faceSorting.ApplyTo(GetRenderer(serializedVisual, FaceRendererPropertyName));
        changed |= traitSorting.ApplyTo(GetRenderer(serializedVisual, TraitRendererPropertyName));

        return changed;
    }

    private SpriteRenderer GetRenderer(SerializedObject serializedVisual, string propertyName)
    {
        SerializedProperty property = serializedVisual.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as SpriteRenderer : null;
    }
}
