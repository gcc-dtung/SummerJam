using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConditionsSO))]
public class ConditionsSOEditor : Editor
{
    private SerializedProperty typeProp;
    private SerializedProperty targetProp;
    private SerializedProperty dishTagsProp;
    private SerializedProperty personTagsProp;
    private SerializedProperty descriptionProp;

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("<Type>k__BackingField");
        targetProp = serializedObject.FindProperty("<Target>k__BackingField");
        dishTagsProp = serializedObject.FindProperty("<DishTags>k__BackingField");
        personTagsProp = serializedObject.FindProperty("<PersonTags>k__BackingField");
        descriptionProp = serializedObject.FindProperty("<Description>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(typeProp, new GUIContent("Type"));
        EditorGUILayout.PropertyField(targetProp, new GUIContent("Target"));
        string currentTarget = targetProp.enumNames[targetProp.enumValueIndex];
        if (currentTarget == "Dish")
        {
            EditorGUILayout.PropertyField(dishTagsProp, new GUIContent("Dish Tags"));
        }
        else if (currentTarget == "Person")
        {
            EditorGUILayout.PropertyField(personTagsProp, new GUIContent("Person Tags"));
        }
        EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description"));
        serializedObject.ApplyModifiedProperties();
    }
}