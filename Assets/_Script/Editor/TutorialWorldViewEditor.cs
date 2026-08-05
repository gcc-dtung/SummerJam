using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialWorldView))]
public sealed class TutorialWorldViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TutorialWorldView worldView = (TutorialWorldView)target;

        EditorGUILayout.HelpBox(
            "Play Mode authoring: run a World presentation step, select TutorialLayer, " +
            "then move and scale the yellow spotlight handles in Scene View. " +
            "Changes are saved to the TutorialSequence asset.",
            MessageType.Info);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "World targets are created at runtime. Enter Play Mode to edit their spotlight.",
                MessageType.None);
            return;
        }

        TutorialStepData step = worldView.CurrentStep;
        if (step == null || step.PresentationSpace != TutorialPresentationSpace.World)
        {
            EditorGUILayout.HelpBox(
                "The current tutorial step is not using World presentation.",
                MessageType.Warning);
            return;
        }

        TutorialSequence sequence = worldView.Director != null
            ? worldView.Director.Sequence
            : null;
        if (sequence != null && GUILayout.Button("Select Tutorial Sequence"))
            Selection.activeObject = sequence;
    }

    private void OnSceneGUI()
    {
        TutorialWorldView worldView = (TutorialWorldView)target;
        TutorialDirector director = worldView.Director;
        TutorialStepData step = worldView.CurrentStep;
        TutorialSequence sequence = director != null ? director.Sequence : null;

        if (!Application.isPlaying || sequence == null || step == null ||
            step.PresentationSpace != TutorialPresentationSpace.World ||
            !worldView.TryGetCurrentTargetBounds(out Bounds targetBounds))
            return;

        int stepIndex = director.CurrentStepIndex;
        if (stepIndex < 0 || stepIndex >= sequence.StepCount)
            return;

        SerializedObject sequenceObject = new SerializedObject(sequence);
        SerializedProperty stepsProperty = sequenceObject.FindProperty("steps");
        if (stepsProperty == null || stepIndex >= stepsProperty.arraySize)
            return;

        SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(stepIndex);
        SerializedProperty offsetProperty =
            stepProperty.FindPropertyRelative("worldHighlightOffset");
        SerializedProperty sizeProperty =
            stepProperty.FindPropertyRelative("worldHighlightSizeDelta");
        if (offsetProperty == null || sizeProperty == null)
            return;

        sequenceObject.Update();

        Vector2 offset = offsetProperty.vector2Value;
        Vector2 sizeDelta = sizeProperty.vector2Value;
        Vector3 center = targetBounds.center + new Vector3(offset.x, offset.y, 0f);
        Vector3 size = new Vector3(
            Mathf.Max(0.01f, targetBounds.size.x + sizeDelta.x),
            Mathf.Max(0.01f, targetBounds.size.y + sizeDelta.y),
            0.01f);

        Handles.color = new Color(1f, 0.72f, 0.1f, 1f);
        Handles.DrawWireCube(center, size);
        Handles.Label(
            center + (Vector3.up * ((size.y * 0.5f) + 0.2f)),
            $"Step {stepIndex}: {step.Id}\nMove = offset, Scale = size");

        EditorGUI.BeginChangeCheck();
        Vector3 editedCenter = Handles.PositionHandle(center, Quaternion.identity);
        float handleSize = HandleUtility.GetHandleSize(center) * 0.8f;
        Vector3 editedSize = Handles.ScaleHandle(
            size,
            editedCenter,
            Quaternion.identity,
            handleSize);

        if (!EditorGUI.EndChangeCheck())
            return;

        editedCenter.z = targetBounds.center.z;
        editedSize.x = Mathf.Max(0.01f, editedSize.x);
        editedSize.y = Mathf.Max(0.01f, editedSize.y);

        Undo.RecordObject(sequence, "Edit Tutorial World Spotlight");
        offsetProperty.vector2Value = new Vector2(
            editedCenter.x - targetBounds.center.x,
            editedCenter.y - targetBounds.center.y);
        sizeProperty.vector2Value = new Vector2(
            editedSize.x - targetBounds.size.x,
            editedSize.y - targetBounds.size.y);
        sequenceObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(sequence);
        SceneView.RepaintAll();
    }
}
