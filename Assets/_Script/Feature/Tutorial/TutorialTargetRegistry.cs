using System.Collections.Generic;
using UnityEngine;

public sealed class TutorialTargetRegistry
{
    private static readonly TutorialTargetAnchor[] EmptyTargets = new TutorialTargetAnchor[0];
    private readonly Dictionary<TutorialTargetId, List<TutorialTargetAnchor>> targets =
        new Dictionary<TutorialTargetId, List<TutorialTargetAnchor>>();
    private readonly List<TutorialTargetAnchor> allTargets = new List<TutorialTargetAnchor>();

    public int Count { get; private set; }
    public IReadOnlyList<TutorialTargetAnchor> AllTargets => allTargets;

    public void Rebuild(IReadOnlyList<GameObject> sceneRoots, GameObject runtimeLayout)
    {
        Clear();

        if (sceneRoots != null)
        {
            for (int i = 0; i < sceneRoots.Count; i++)
                RegisterRoot(sceneRoots[i]);
        }

        RegisterRoot(runtimeLayout);
    }

    public bool TryGetFirst(TutorialTargetId targetId, out TutorialTargetAnchor target)
    {
        if (targets.TryGetValue(targetId, out List<TutorialTargetAnchor> matches))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                TutorialTargetAnchor candidate = matches[i];
                if (candidate != null && candidate.gameObject.activeInHierarchy)
                {
                    target = candidate;
                    return true;
                }
            }
        }

        target = null;
        return false;
    }

    public IReadOnlyList<TutorialTargetAnchor> GetAll(TutorialTargetId targetId)
    {
        return targets.TryGetValue(targetId, out List<TutorialTargetAnchor> matches)
            ? matches
            : EmptyTargets;
    }

    public void Clear()
    {
        targets.Clear();
        allTargets.Clear();
        Count = 0;
    }

    private void RegisterRoot(GameObject root)
    {
        if (root == null)
            return;

        TutorialTargetAnchor[] anchors = root.GetComponentsInChildren<TutorialTargetAnchor>(true);
        for (int i = 0; i < anchors.Length; i++)
            Register(anchors[i]);
    }

    public void Register(TutorialTargetAnchor anchor)
    {
        if (anchor == null || anchor.TargetId == TutorialTargetId.None)
            return;

        if (!targets.TryGetValue(anchor.TargetId, out List<TutorialTargetAnchor> matches))
        {
            matches = new List<TutorialTargetAnchor>();
            targets.Add(anchor.TargetId, matches);
        }

        if (matches.Contains(anchor))
            return;

        matches.Add(anchor);
        allTargets.Add(anchor);
        Count++;
    }
}
