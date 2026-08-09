using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialRuntimeGrid
{
    Board = 0,
    WaitLine = 1
}

public enum TutorialRuntimeTargetObject
{
    Cell = 0,
    Person = 1
}

[Serializable]
public sealed class TutorialRuntimeTargetBinding
{
    [SerializeField] private TutorialTargetId targetId;
    [SerializeField] private TutorialRuntimeGrid grid;
    [SerializeField] private TutorialRuntimeTargetObject targetObject;
    [SerializeField] private Vector2Int coordinate;

    public TutorialTargetId TargetId => targetId;
    public TutorialRuntimeGrid Grid => grid;
    public TutorialRuntimeTargetObject TargetObject => targetObject;
    public Vector2Int Coordinate => coordinate;
}

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Tutorial Sequence")]
public sealed class TutorialSequence : ScriptableObject
{
    [Header("Eligibility")]
    [SerializeField, Min(1)] private int levelId = 1;
    [SerializeField, Min(1)] private int version = 1;

    [Header("Steps")]
    [Tooltip("Step used after the Play button finishes loading the gameplay scene.")]
    [SerializeField, Min(0)] private int gameplayStartStepIndex = 1;
    [SerializeField] private List<TutorialStepData> steps = new List<TutorialStepData>();

    [Header("Runtime Targets")]
    [Tooltip("Targets spawned by GridManager and therefore unavailable for manual Inspector wiring.")]
    [SerializeField] private List<TutorialRuntimeTargetBinding> runtimeTargets =
        new List<TutorialRuntimeTargetBinding>();

    public int LevelId => levelId;
    public int Version => version;
    public int GameplayStartStepIndex => gameplayStartStepIndex;
    public int StepCount => steps.Count;
    public IReadOnlyList<TutorialStepData> Steps => steps;
    public IReadOnlyList<TutorialRuntimeTargetBinding> RuntimeTargets => runtimeTargets;

    public bool TryGetStep(int index, out TutorialStepData step)
    {
        if (index < 0 || index >= steps.Count)
        {
            step = null;
            return false;
        }

        step = steps[index];
        return step != null;
    }

    private void OnValidate()
    {
        levelId = Mathf.Max(1, levelId);
        version = Mathf.Max(1, version);
        gameplayStartStepIndex = Mathf.Max(0, gameplayStartStepIndex);
    }
}
