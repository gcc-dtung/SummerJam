using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TutorialDirector : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TutorialSequence tutorialSequence;
    [Tooltip("Scene roots containing tutorial targets, such as the main menu and gameplay canvases.")]
    [SerializeField] private List<GameObject> targetRoots = new List<GameObject>();

    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SaveLoadManager saveLoadManager;

    private readonly TutorialStateMachine stateMachine = new TutorialStateMachine();
    private readonly TutorialTargetRegistry targetRegistry = new TutorialTargetRegistry();
    private readonly List<TutorialTargetAnchor> runtimeTargets = new List<TutorialTargetAnchor>();
    private float stepEnteredAt;
    private bool awaitingGameplayLoad;
    private int resumeStepAfterGameplayLoad = -1;

    public event Action<LevelConfig, GameObject> OnTutorialReady;
    public event Action<int, TutorialStepData> OnStepEntered;
    public event Action<int, TutorialStepData> OnStepExited;
    public event Action OnTutorialCompleted;

    public bool IsTutorialPending { get; private set; }
    public bool IsRunning => stateMachine.IsRunning;
    public int CurrentStepIndex => stateMachine.CurrentStepIndex;
    public LevelConfig ActiveLevel { get; private set; }
    public GameObject ActiveLayout { get; private set; }
    public TutorialSequence Sequence => tutorialSequence;
    public TutorialTargetRegistry TargetRegistry => targetRegistry;
    public TutorialStepData CurrentStep => GetStep(CurrentStepIndex);

    private void Awake()
    {
        ResolveDependencies();
        stateMachine.StepEntered += HandleStepEntered;
        stateMachine.StepExited += HandleStepExited;
        stateMachine.Completed += HandleTutorialCompleted;
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLevelReady += HandleLevelReady;

        EventBus.AddListener<Person>(GameEventType.Press, HandlePersonPressed);
        EventBus.AddListener<Person>(GameEventType.StartDragPerson, HandlePersonDragStarted);
        EventBus.AddListener<Person>(GameEventType.DroppedPerson, HandlePersonDropped);
        EventBus.AddListener<Booster>(GameEventType.BoosterUsed, HandleBoosterUsed);
    }

    private void Start()
    {
        TryStartMainMenuTutorial();
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLevelReady -= HandleLevelReady;

        EventBus.RemoveListener<Person>(GameEventType.Press, HandlePersonPressed);
        EventBus.RemoveListener<Person>(GameEventType.StartDragPerson, HandlePersonDragStarted);
        EventBus.RemoveListener<Person>(GameEventType.DroppedPerson, HandlePersonDropped);
        EventBus.RemoveListener<Booster>(GameEventType.BoosterUsed, HandleBoosterUsed);

        stateMachine.Stop();
        UnbindTargetEvents();
        awaitingGameplayLoad = false;
        resumeStepAfterGameplayLoad = -1;
    }

    private void OnDestroy()
    {
        stateMachine.StepEntered -= HandleStepEntered;
        stateMachine.StepExited -= HandleStepExited;
        stateMachine.Completed -= HandleTutorialCompleted;
    }

    private void Reset()
    {
        gameManager = GetComponent<GameManager>();
        saveLoadManager = GetComponent<SaveLoadManager>();
    }

    public bool BeginTutorial(int startStepIndex = 0)
    {
        if (!IsTutorialPending || stateMachine.IsRunning)
            return false;

        if (tutorialSequence == null || tutorialSequence.StepCount <= 0)
            return false;

        if (startStepIndex < 0 || startStepIndex >= tutorialSequence.StepCount)
            return false;

        stateMachine.Start(tutorialSequence.StepCount, startStepIndex);
        return true;
    }

    public bool TryAdvanceStep()
    {
        return stateMachine.TryAdvance();
    }

    public void StopTutorial()
    {
        stateMachine.Stop();
    }

    public bool CanSelectWorldTarget(GameObject source)
    {
        if (!stateMachine.IsRunning)
            return true;

        TutorialStepData step = CurrentStep;
        if (step == null)
            return false;

        switch (step.RequiredAction)
        {
            case TutorialAction.PressPerson:
            case TutorialAction.StartDragPerson:
            case TutorialAction.PlacePersonOnTarget:
                return MatchesTarget(step.SourceTarget, source);
            default:
                return false;
        }
    }

    public bool CanPerformWorldAction(TutorialAction attemptedAction, GameObject source)
    {
        if (!stateMachine.IsRunning)
            return true;

        TutorialStepData step = CurrentStep;
        if (step == null || !HasMetMinimumDisplayTime(step))
            return false;

        bool isRetryingPlacement = step.RequiredAction == TutorialAction.PlacePersonOnTarget &&
                                   attemptedAction == TutorialAction.StartDragPerson;
        if (step.RequiredAction != attemptedAction && !isRetryingPlacement)
            return false;

        return MatchesTarget(step.SourceTarget, source);
    }

    public bool CanDropAtRequiredTarget(GameObject source, Vector2 worldPoint)
    {
        if (!stateMachine.IsRunning)
            return true;

        TutorialStepData step = CurrentStep;
        if (step == null || step.RequiredAction != TutorialAction.PlacePersonOnTarget)
            return false;

        if (!HasMetMinimumDisplayTime(step) || !MatchesTarget(step.SourceTarget, source))
            return false;

        IReadOnlyList<TutorialTargetAnchor> destinations = targetRegistry.GetAll(step.DestinationTarget);
        for (int i = 0; i < destinations.Count; i++)
        {
            TutorialTargetAnchor destination = destinations[i];
            if (destination != null && destination.gameObject.activeInHierarchy &&
                IsPointInsideDestination(destination, worldPoint))
                return true;
        }

        return false;
    }

    private static bool IsPointInsideDestination(
        TutorialTargetAnchor destination,
        Vector2 worldPoint)
    {
        if (destination.TryGetComponent(out Cell destinationCell) && GridManager.Instance != null)
        {
            Grid<Cell> board = GridManager.Instance.Board;
            if (board != null && board.GetValueFromWorldPosition(worldPoint) == destinationCell)
                return true;

            Grid<Cell> waitLine = GridManager.Instance.WaitLine;
            return waitLine != null &&
                   waitLine.GetValueFromWorldPosition(worldPoint) == destinationCell;
        }

        return destination.ContainsWorldPoint(worldPoint);
    }

    public bool TryHandleAction(
        TutorialAction action,
        GameObject source = null,
        GameObject destination = null)
    {
        TutorialStepData step = CurrentStep;
        if (!stateMachine.IsRunning || step == null || step.RequiredAction != action)
            return false;

        if (!HasMetMinimumDisplayTime(step))
            return false;

        if (RequiresSourceTarget(action) && !MatchesTarget(step.SourceTarget, source))
            return false;

        if (action == TutorialAction.PlacePersonOnTarget &&
            !MatchesTarget(step.DestinationTarget, destination))
            return false;

        return stateMachine.TryAdvance();
    }

    private void Update()
    {
        TutorialStepData step = CurrentStep;
        if (!stateMachine.IsRunning || step == null || step.RequiredAction != TutorialAction.Wait)
            return;

        if (HasMetMinimumDisplayTime(step))
            stateMachine.TryAdvance();
    }

    private void ResolveDependencies()
    {
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();

        if (saveLoadManager == null)
            saveLoadManager = GetComponent<SaveLoadManager>();
    }

    private void HandleLevelReady(LevelConfig levelConfig, GameObject layout)
    {
        bool shouldResumeAfterPlay = awaitingGameplayLoad;
        int requestedStartStep = resumeStepAfterGameplayLoad;

        stateMachine.Stop();
        ActiveLevel = levelConfig;
        ActiveLayout = layout;
        UnbindTargetEvents();
        BindRuntimeTargets();
        targetRegistry.Rebuild(targetRoots, layout);
        RegisterRuntimeTargets();
        BindTargetEvents();
        IsTutorialPending = ShouldStartTutorial(levelConfig);
        awaitingGameplayLoad = false;
        resumeStepAfterGameplayLoad = -1;

        if (IsTutorialPending)
        {
            OnTutorialReady?.Invoke(levelConfig, layout);

            int gameplayStartStep = tutorialSequence != null
                ? tutorialSequence.GameplayStartStepIndex
                : 0;
            int startStep = shouldResumeAfterPlay
                ? Mathf.Max(requestedStartStep, gameplayStartStep)
                : gameplayStartStep;

            if (startStep >= 0 && startStep < tutorialSequence.StepCount)
                BeginTutorial(startStep);
        }
    }

    private void TryStartMainMenuTutorial()
    {
        if (stateMachine.IsRunning || tutorialSequence == null || LevelManager.Instance == null)
            return;

        IReadOnlyList<LevelConfig> levels = LevelManager.Instance.LevelConfigs;
        int levelIndex = LevelManager.Instance.CurrentLevelIndex;
        if (levels == null || levelIndex < 0 || levelIndex >= levels.Count)
            return;

        LevelConfig levelConfig = levels[levelIndex];
        ActiveLevel = levelConfig;
        ActiveLayout = null;

        UnbindTargetEvents();
        targetRegistry.Rebuild(targetRoots, null);
        BindTargetEvents();
        IsTutorialPending = ShouldStartTutorial(levelConfig);

        if (!IsTutorialPending)
            return;

        OnTutorialReady?.Invoke(levelConfig, null);

        TutorialStepData firstStep = GetStep(0);
        if (firstStep != null && firstStep.SourceTarget == TutorialTargetId.PlayButton)
            BeginTutorial(0);
    }

    private void BindRuntimeTargets()
    {
        runtimeTargets.Clear();

        if (tutorialSequence == null || GridManager.Instance == null)
            return;

        IReadOnlyList<TutorialRuntimeTargetBinding> bindings = tutorialSequence.RuntimeTargets;
        for (int i = 0; i < bindings.Count; i++)
        {
            TutorialRuntimeTargetBinding binding = bindings[i];
            if (binding == null || binding.TargetId == TutorialTargetId.None)
                continue;

            Grid<Cell> grid = binding.Grid == TutorialRuntimeGrid.Board
                ? GridManager.Instance.Board
                : GridManager.Instance.WaitLine;
            Vector2Int coordinate = binding.Coordinate;
            if (grid == null || !grid.IsOnRange(coordinate.x, coordinate.y))
            {
                Debug.LogWarning(
                    $"[Tutorial] Target {binding.TargetId} is outside {binding.Grid} at {coordinate}.");
                continue;
            }

            Cell cell = grid.GetValue(coordinate.x, coordinate.y);
            GameObject target = ResolveRuntimeTarget(binding, cell);
            if (target == null)
            {
                Debug.LogWarning(
                    $"[Tutorial] Target {binding.TargetId} could not resolve {binding.TargetObject} " +
                    $"from {binding.Grid} at {coordinate}.");
                continue;
            }

            TutorialTargetAnchor anchor = target.GetComponent<TutorialTargetAnchor>();
            if (anchor == null)
                anchor = target.AddComponent<TutorialTargetAnchor>();

            anchor.Configure(binding.TargetId);
            runtimeTargets.Add(anchor);
        }
    }

    private void RegisterRuntimeTargets()
    {
        for (int i = 0; i < runtimeTargets.Count; i++)
            targetRegistry.Register(runtimeTargets[i]);
    }

    private static GameObject ResolveRuntimeTarget(
        TutorialRuntimeTargetBinding binding,
        Cell cell)
    {
        if (cell == null)
            return null;

        if (binding.TargetObject == TutorialRuntimeTargetObject.Person)
            return cell.CurrentPerson != null ? cell.CurrentPerson.gameObject : null;

        return cell.gameObject;
    }

    private bool ShouldStartTutorial(LevelConfig levelConfig)
    {
        if (tutorialSequence == null || tutorialSequence.StepCount <= 0)
            return false;

        if (levelConfig == null || levelConfig.ID != tutorialSequence.LevelId)
            return false;

        GameData gameData = saveLoadManager != null ? saveLoadManager.GameData : null;
        return gameData != null && gameData.tutorialVersionCompleted < tutorialSequence.Version;
    }

    private void HandleStepEntered(int stepIndex)
    {
        TutorialStepData step = GetStep(stepIndex);
        if (step != null)
        {
            stepEnteredAt = Time.unscaledTime;
            OnStepEntered?.Invoke(stepIndex, step);
        }
    }

    private void HandleStepExited(int stepIndex)
    {
        TutorialStepData step = GetStep(stepIndex);
        if (step != null)
            OnStepExited?.Invoke(stepIndex, step);
    }

    private void HandleTutorialCompleted()
    {
        IsTutorialPending = false;

        GameData gameData = saveLoadManager != null ? saveLoadManager.GameData : null;
        if (gameData != null && tutorialSequence != null)
        {
            gameData.tutorialVersionCompleted = Math.Max(
                gameData.tutorialVersionCompleted,
                tutorialSequence.Version);
            saveLoadManager.SaveGame();
        }

        OnTutorialCompleted?.Invoke();
    }

    private TutorialStepData GetStep(int stepIndex)
    {
        if (tutorialSequence == null)
            return null;

        return tutorialSequence.TryGetStep(stepIndex, out TutorialStepData step) ? step : null;
    }

    private void BindTargetEvents()
    {
        IReadOnlyList<TutorialTargetAnchor> targets = targetRegistry.AllTargets;
        for (int i = 0; i < targets.Count; i++)
        {
            TutorialTargetAnchor target = targets[i];
            if (target != null)
                target.Clicked += HandleTargetClicked;
        }
    }

    private void UnbindTargetEvents()
    {
        IReadOnlyList<TutorialTargetAnchor> targets = targetRegistry.AllTargets;
        for (int i = 0; i < targets.Count; i++)
        {
            TutorialTargetAnchor target = targets[i];
            if (target != null)
                target.Clicked -= HandleTargetClicked;
        }
    }

    private void HandleTargetClicked(TutorialTargetAnchor target)
    {
        if (target == null || CurrentStep == null || CurrentStep.RequiredAction != TutorialAction.TapTarget)
            return;

        if (CurrentStep.SourceTarget == TutorialTargetId.PlayButton &&
            MatchesTarget(CurrentStep.SourceTarget, target.gameObject) &&
            HasMetMinimumDisplayTime(CurrentStep))
        {
            awaitingGameplayLoad = true;
            resumeStepAfterGameplayLoad = CurrentStepIndex + 1;
            stateMachine.Stop();
            return;
        }

        TryHandleAction(TutorialAction.TapTarget, target.gameObject);
    }

    private void HandlePersonPressed(Person person)
    {
        if (person != null)
            TryHandleAction(TutorialAction.PressPerson, person.gameObject);
    }

    private void HandlePersonDragStarted(Person person)
    {
        if (person != null)
            TryHandleAction(TutorialAction.StartDragPerson, person.gameObject);
    }

    private void HandlePersonDropped(Person person)
    {
        if (person == null)
            return;

        Transform parent = person.transform.parent;
        TryHandleAction(
            TutorialAction.PlacePersonOnTarget,
            person.gameObject,
            parent != null ? parent.gameObject : null);
    }

    private void HandleBoosterUsed(Booster booster)
    {
        TutorialStepData step = CurrentStep;
        if (step == null || step.RequiredAction != TutorialAction.UseBooster)
            return;

        if (!MatchesBooster(step.SourceTarget, booster) || !HasMetMinimumDisplayTime(step))
            return;

        stateMachine.TryAdvance();
    }

    private bool MatchesTarget(TutorialTargetId targetId, GameObject candidate)
    {
        if (targetId == TutorialTargetId.None)
            return true;

        if (candidate == null)
            return false;

        Transform candidateTransform = candidate.transform;
        IReadOnlyList<TutorialTargetAnchor> matches = targetRegistry.GetAll(targetId);
        for (int i = 0; i < matches.Count; i++)
        {
            TutorialTargetAnchor anchor = matches[i];
            if (anchor == null)
                continue;

            Transform anchorTransform = anchor.transform;
            if (candidateTransform == anchorTransform ||
                candidateTransform.IsChildOf(anchorTransform) ||
                anchorTransform.IsChildOf(candidateTransform))
                return true;
        }

        return false;
    }

    private bool HasMetMinimumDisplayTime(TutorialStepData step)
    {
        return Time.unscaledTime - stepEnteredAt >= step.MinimumDisplayTime;
    }

    private static bool RequiresSourceTarget(TutorialAction action)
    {
        return action == TutorialAction.TapTarget ||
               action == TutorialAction.PressPerson ||
               action == TutorialAction.StartDragPerson ||
               action == TutorialAction.PlacePersonOnTarget;
    }

    private static bool MatchesBooster(TutorialTargetId targetId, Booster booster)
    {
        if (targetId == TutorialTargetId.None || targetId == TutorialTargetId.BoosterArea)
            return true;

        return (targetId == TutorialTargetId.RemoveBooster && booster == Booster.Remove) ||
               (targetId == TutorialTargetId.MoreMoveBooster && booster == Booster.Move) ||
               (targetId == TutorialTargetId.UndoBooster && booster == Booster.Undo);
    }
}
