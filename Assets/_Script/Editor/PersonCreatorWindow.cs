using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PersonCreatorWindow : EditorWindow
{
    private enum PreviewState
    {
        Normal,
        Happy,
        Angry
    }

    private const string DefaultFolder = "Assets/Data/Person";
    private const string DefaultPrefabFolder = "Assets/Prefab/Persons";
    private const string DefaultBasePrefabPath = "Assets/Prefab/Persons/PersonTemplete.prefab";

    private GameObject basePrefab;
    private PersonSkinSO skinSO;
    private ConditionsSO conditionsSO;
    private string personName = "New Person";
    private string personId = "person_new";
    private Trait trait = Trait.Normal;
    private int baseSkinIndex;
    private PreviewState previewState = PreviewState.Normal;
    private bool showTooltip = true;
    private string tooltipContent = "Condition preview";
    private string dataOutputFolder = DefaultFolder;
    private string prefabOutputFolder = DefaultPrefabFolder;
    private GameObject previewInstance;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Person Creator")]
    public static void ShowWindow()
    {
        GetWindow<PersonCreatorWindow>("Person Creator");
    }

    private void OnEnable()
    {
        if (basePrefab == null)
            basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBasePrefabPath);

        PullVisualDefaultsFromBasePrefab();
    }

    private void OnDisable()
    {
        RemoveScenePreview();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawPersonDataSection();
        EditorGUILayout.Space(8f);
        DrawVisualSection();
        EditorGUILayout.Space(8f);
        DrawScenePreviewSection();
        EditorGUILayout.Space(8f);
        DrawCreateSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawPersonDataSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Person Data", EditorStyles.boldLabel);

        personName = EditorGUILayout.TextField("Name", personName);
        personId = EditorGUILayout.TextField("ID", personId);
        trait = (Trait)EditorGUILayout.EnumPopup("Trait", trait);
        conditionsSO = (ConditionsSO)EditorGUILayout.ObjectField("Conditions", conditionsSO, typeof(ConditionsSO), false);

        EditorGUILayout.EndVertical();
    }

    private void DrawVisualSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        basePrefab = (GameObject)EditorGUILayout.ObjectField("Base Prefab", basePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
            PullVisualDefaultsFromBasePrefab();

        skinSO = (PersonSkinSO)EditorGUILayout.ObjectField("Person Skin SO", skinSO, typeof(PersonSkinSO), false);
        baseSkinIndex = Mathf.Max(0, EditorGUILayout.IntField("Base Skin Index", baseSkinIndex));
        previewState = (PreviewState)EditorGUILayout.EnumPopup("State", previewState);
        showTooltip = EditorGUILayout.Toggle("Show Tooltip", showTooltip);

        if (showTooltip)
        {
            EditorGUILayout.LabelField("Tooltip Fallback Text");
            tooltipContent = EditorGUILayout.TextArea(tooltipContent, GUILayout.MinHeight(40f));
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawScenePreviewSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Scene Preview", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Preview nay spawn base prefab that vao Scene va dung TooltipPopup that cua prefab.", MessageType.Info);

        GUI.enabled = basePrefab != null;
        if (GUILayout.Button(previewInstance == null ? "Spawn Preview In Scene" : "Update Scene Preview", GUILayout.Height(28f)))
        {
            SpawnOrUpdateScenePreview();
        }

        GUI.enabled = previewInstance != null;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(showTooltip ? "Apply With Tooltip" : "Apply Without Tooltip", GUILayout.Height(24f)))
            ApplyPreviewInstanceData();

        if (GUILayout.Button("Remove Preview", GUILayout.Height(24f)))
            RemoveScenePreview();
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;

        if (previewInstance != null)
            EditorGUILayout.ObjectField("Preview Instance", previewInstance, typeof(GameObject), true);

        EditorGUILayout.EndVertical();
    }

    private void DrawCreateSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Create Assets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        dataOutputFolder = EditorGUILayout.TextField("Data Folder", dataOutputFolder);
        if (GUILayout.Button("Select", GUILayout.Width(70f)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Person Data Folder", "Assets", string.Empty);
            TrySetProjectFolder(selected, ref dataOutputFolder);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        prefabOutputFolder = EditorGUILayout.TextField("Prefab Folder", prefabOutputFolder);
        if (GUILayout.Button("Select", GUILayout.Width(70f)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Person Prefab Folder", "Assets", string.Empty);
            TrySetProjectFolder(selected, ref prefabOutputFolder);
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = CanCreateAsset();
        if (GUILayout.Button("Create Person Prefab", GUILayout.Height(32f)))
            CreatePersonAssets();
        GUI.enabled = true;

        if (basePrefab == null)
            EditorGUILayout.HelpBox("Assign a base prefab first. PersonTemplete.prefab is used by default when it exists.", MessageType.Warning);

        EditorGUILayout.EndVertical();
    }

    private Sprite GetFaceSprite()
    {
        if (skinSO == null) return null;

        switch (previewState)
        {
            case PreviewState.Happy:
                return skinSO.GetStateFace(true);
            case PreviewState.Angry:
                return skinSO.GetStateFace(false);
            default:
                return skinSO.GetNormalFace();
        }
    }

    private string GetTooltipPreviewContent()
    {
        if (conditionsSO == null)
            return tooltipContent;

        List<ConditionInfo> conditionInfos = new List<ConditionInfo>();
        conditionsSO.GetConditionInfo(conditionInfos);

        if (conditionInfos.Count == 0)
            return tooltipContent;

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < conditionInfos.Count; i++)
        {
            builder.Append(conditionInfos[i].Description);
            if (i < conditionInfos.Count - 1)
                builder.Append('\n');
        }

        return builder.ToString();
    }

    private void SpawnOrUpdateScenePreview()
    {
        if (basePrefab == null) return;

        if (previewInstance == null)
        {
            previewInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            if (previewInstance == null) return;

            previewInstance.hideFlags = HideFlags.DontSave;
            Undo.RegisterCreatedObjectUndo(previewInstance, "Spawn Person Preview");
        }

        ApplyPreviewInstanceData();

        Selection.activeGameObject = previewInstance;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        SceneView.RepaintAll();
    }

    private void ApplyPreviewInstanceData()
    {
        if (previewInstance == null) return;

        previewInstance.name = $"[Preview] {MakeSafeFileName(personId)}";
        ApplyPersonReferences(previewInstance, CreatePreviewData());
        ApplyVisualReferences(previewInstance);
        ApplyVisualSprites(previewInstance);
        ApplyTooltipPreview(previewInstance);
    }

    private PersonDataSO CreatePreviewData()
    {
        PersonDataSO previewData = CreateInstance<PersonDataSO>();
        previewData.hideFlags = HideFlags.DontSave;

        SerializedObject serializedData = new SerializedObject(previewData);
        serializedData.FindProperty("<Name>k__BackingField").stringValue = personName;
        serializedData.FindProperty("<ID>k__BackingField").stringValue = personId;

        SerializedProperty traitList = serializedData.FindProperty("<Trait>k__BackingField");
        traitList.arraySize = 1;
        traitList.GetArrayElementAtIndex(0).enumValueIndex = (int)trait;

        serializedData.ApplyModifiedPropertiesWithoutUndo();
        return previewData;
    }

    private void ApplyVisualSprites(GameObject target)
    {
        if (skinSO == null) return;

        PersonVisual visual = target.GetComponentInChildren<PersonVisual>(true);
        if (visual == null) return;

        SerializedObject serializedVisual = new SerializedObject(visual);
        SpriteRenderer skinRenderer = serializedVisual.FindProperty("skinRenderer").objectReferenceValue as SpriteRenderer;
        SpriteRenderer faceRenderer = serializedVisual.FindProperty("faceRenderer").objectReferenceValue as SpriteRenderer;
        SpriteRenderer traitRenderer = serializedVisual.FindProperty("traitRenderer").objectReferenceValue as SpriteRenderer;

        if (skinRenderer != null)
            skinRenderer.sprite = skinSO.GetBaseSkin(baseSkinIndex);

        if (faceRenderer != null)
            faceRenderer.sprite = GetFaceSprite();

        if (traitRenderer != null)
        {
            Sprite traitSkin = skinSO.GetTraitSkin(trait);
            traitRenderer.sprite = traitSkin;
            traitRenderer.enabled = traitSkin != null;
        }

        EditorUtility.SetDirty(target);
    }

    private void ApplyTooltipPreview(GameObject target)
    {
        TooltipPopup tooltipPopup = target.GetComponentInChildren<TooltipPopup>(true);
        if (tooltipPopup == null) return;

        if (showTooltip)
        {
            tooltipPopup.HideImmediate();
            tooltipPopup.Show(personName, trait.ToString(), GetTooltipPreviewContent());
        }
        else
        {
            tooltipPopup.HideImmediate();
        }

        EditorUtility.SetDirty(tooltipPopup);
    }

    private void RemoveScenePreview()
    {
        if (previewInstance == null) return;

        DestroyImmediate(previewInstance);
        previewInstance = null;
        SceneView.RepaintAll();
    }

    private bool CanCreateAsset()
    {
        return !string.IsNullOrWhiteSpace(personName)
               && !string.IsNullOrWhiteSpace(personId)
               && !string.IsNullOrWhiteSpace(dataOutputFolder)
               && dataOutputFolder.StartsWith("Assets")
               && !string.IsNullOrWhiteSpace(prefabOutputFolder)
               && prefabOutputFolder.StartsWith("Assets")
               && basePrefab != null;
    }

    private void CreatePersonAssets()
    {
        EnsureFolderExists(dataOutputFolder);
        EnsureFolderExists(prefabOutputFolder);

        PersonDataSO dataAsset = CreatePersonDataAsset();
        GameObject prefabAsset = CreatePersonPrefab(dataAsset);

        Selection.activeObject = prefabAsset != null ? prefabAsset : dataAsset;
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    private PersonDataSO CreatePersonDataAsset()
    {
        PersonDataSO asset = CreateInstance<PersonDataSO>();
        SerializedObject serializedAsset = new SerializedObject(asset);

        serializedAsset.FindProperty("<Name>k__BackingField").stringValue = personName;
        serializedAsset.FindProperty("<ID>k__BackingField").stringValue = personId;

        SerializedProperty traitList = serializedAsset.FindProperty("<Trait>k__BackingField");
        traitList.arraySize = 1;
        traitList.GetArrayElementAtIndex(0).enumValueIndex = (int)trait;

        serializedAsset.ApplyModifiedPropertiesWithoutUndo();

        string fileName = MakeSafeFileName(personId);
        string path = AssetDatabase.GenerateUniqueAssetPath($"{dataOutputFolder}/{fileName}.asset");

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created PersonDataSO: {path}");
        return asset;
    }

    private GameObject CreatePersonPrefab(PersonDataSO dataAsset)
    {
        string basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
        string fileName = MakeSafeFileName(personId);
        string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{prefabOutputFolder}/{fileName}.prefab");

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(basePrefabPath);

        try
        {
            prefabRoot.name = fileName;
            ApplyPersonReferences(prefabRoot, dataAsset);
            ApplyVisualReferences(prefabRoot);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created Person prefab: {prefabPath}");
            return prefabAsset;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private void ApplyPersonReferences(GameObject prefabRoot, PersonDataSO dataAsset)
    {
        Person person = prefabRoot.GetComponentInChildren<Person>(true);
        if (person == null)
        {
            Debug.LogWarning("Base prefab does not contain a Person component.");
            return;
        }

        SerializedObject serializedPerson = new SerializedObject(person);
        serializedPerson.FindProperty("data").objectReferenceValue = dataAsset;
        serializedPerson.FindProperty("conditions").objectReferenceValue = conditionsSO;
        serializedPerson.ApplyModifiedPropertiesWithoutUndo();
    }

    private void ApplyVisualReferences(GameObject prefabRoot)
    {
        PersonVisual visual = prefabRoot.GetComponentInChildren<PersonVisual>(true);
        if (visual == null) return;

        SerializedObject serializedVisual = new SerializedObject(visual);
        serializedVisual.FindProperty("skinSO").objectReferenceValue = skinSO;
        serializedVisual.FindProperty("baseSkinIndex").intValue = baseSkinIndex;
        serializedVisual.ApplyModifiedPropertiesWithoutUndo();
    }

    private void PullVisualDefaultsFromBasePrefab()
    {
        if (basePrefab == null) return;

        PersonVisual visual = basePrefab.GetComponentInChildren<PersonVisual>(true);
        if (visual == null) return;

        SerializedObject serializedVisual = new SerializedObject(visual);

        SerializedProperty skinProperty = serializedVisual.FindProperty("skinSO");
        if (skinSO == null && skinProperty != null)
            skinSO = skinProperty.objectReferenceValue as PersonSkinSO;

        SerializedProperty baseSkinIndexProperty = serializedVisual.FindProperty("baseSkinIndex");
        if (baseSkinIndexProperty != null)
            baseSkinIndex = Mathf.Max(0, baseSkinIndexProperty.intValue);
    }

    private void TrySetProjectFolder(string selected, ref string targetFolder)
    {
        if (string.IsNullOrEmpty(selected)) return;

        string projectPath = Application.dataPath.Replace("\\", "/");
        selected = selected.Replace("\\", "/");

        if (selected.StartsWith(projectPath))
            targetFolder = "Assets" + selected.Substring(projectPath.Length);
        else
            Debug.LogWarning("Please select a folder inside this Unity project's Assets folder.");
    }

    private void EnsureFolderExists(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private string MakeSafeFileName(string value)
    {
        string safeName = value.Trim();
        foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            safeName = safeName.Replace(invalidChar.ToString(), string.Empty);

        return string.IsNullOrEmpty(safeName) ? "PersonData" : safeName;
    }
}
