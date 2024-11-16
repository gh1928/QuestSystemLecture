using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TaskGroupState
{
    InActive,
    Running,
    Complete,    
}

[System.Serializable]
public class TaskGroup
{
    [SerializeField] private Task[] tasks;
    public IReadOnlyList<Task> Tasks => tasks;
    public Quest Owner { get; private set; }
    public bool IsAllTaskComplete => tasks.All(t => t.IsComplete);
    public bool IsComplete => State == TaskGroupState.Complete;
    public TaskGroupState State { get; private set; }

    public TaskGroup(TaskGroup copyTarget)
    {
        tasks = copyTarget.Tasks.Select(x => Object.Instantiate(x)).ToArray();
    }

    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var task in tasks)
        {
            task.Setup(owner);
        }
    }

    public void Start()
    {
        State = TaskGroupState.Running;
        foreach (var task in tasks)
        {
            task.Start();
        }
    }

    public void End()
    {
        State = TaskGroupState.Complete;
        foreach (var task in tasks)
        {
            task.End();
        }
    }

    public void ReceiveReport(string category, object target, int sucessCount)
    {
        foreach (var task in tasks)
        {
            if (task.IsTarget(category, target))
            {
                task.ReceiveReport(sucessCount);
            }
        }
    }

    public void Complete()
    {
        if (IsComplete) return;

        State = TaskGroupState.Complete;

        foreach (var task in tasks)
        {
            if(!task.IsComplete)
            {
                task.Complete();
            }
        }
    }
}
