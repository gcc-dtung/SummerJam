using System;

public enum TutorialRunState
{
    Idle = 0,
    Running = 1,
    Completed = 2
}

public sealed class TutorialStateMachine
{
    public event Action<int> StepEntered;
    public event Action<int> StepExited;
    public event Action Completed;

    public TutorialRunState State { get; private set; } = TutorialRunState.Idle;
    public int CurrentStepIndex { get; private set; } = -1;
    public int StepCount { get; private set; }
    public bool IsRunning => State == TutorialRunState.Running;

    public void Start(int stepCount, int startStepIndex = 0)
    {
        if (stepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepCount), "A tutorial must contain at least one step.");

        if (startStepIndex < 0 || startStepIndex >= stepCount)
            throw new ArgumentOutOfRangeException(nameof(startStepIndex));

        Stop();

        StepCount = stepCount;
        CurrentStepIndex = startStepIndex;
        State = TutorialRunState.Running;
        StepEntered?.Invoke(CurrentStepIndex);
    }

    public bool TryAdvance()
    {
        if (!IsRunning)
            return false;

        int exitedStepIndex = CurrentStepIndex;
        StepExited?.Invoke(exitedStepIndex);

        int nextStepIndex = exitedStepIndex + 1;
        if (nextStepIndex >= StepCount)
        {
            State = TutorialRunState.Completed;
            CurrentStepIndex = -1;
            Completed?.Invoke();
            return true;
        }

        CurrentStepIndex = nextStepIndex;
        StepEntered?.Invoke(CurrentStepIndex);
        return true;
    }

    public void Stop()
    {
        if (IsRunning)
            StepExited?.Invoke(CurrentStepIndex);

        State = TutorialRunState.Idle;
        CurrentStepIndex = -1;
        StepCount = 0;
    }
}
