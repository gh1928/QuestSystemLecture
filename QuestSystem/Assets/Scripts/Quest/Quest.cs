using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;


using Debug = UnityEngine.Debug;

public enum QuestState
{
    InActive,
    Running,
    Complete,
    Cancel,
    WaitingForCompletion,
}

[CreateAssetMenu(menuName = "Quest/Quest", fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region Events
    public delegate void TaskSuccessChangeHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CanceldHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    #endregion

    [SerializeField] private Category category;
    [SerializeField] private Sprite icon;

    [Header("Text")]
    [SerializeField] private string codeName;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;

    [Header("Task")]
    [SerializeField] private TaskGroup[] taskGroups;

    [Header("Reward")]
    [SerializeField] private Reward[] rewards;

    [Header("Option")]
    [SerializeField] private bool useAutoComplete;
    [SerializeField] private bool isCancelable;
    [SerializeField] private bool isSavable;

    [Header("Condition")]
    [SerializeField] private Condition[] acceptionConditions;
    [SerializeField] private Condition[] cancelConditions;


    private int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;

    public bool IsRegistered => State != QuestState.InActive;
    public bool IsCompletable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;

    public virtual bool IsCancelable => isCancelable && cancelConditions.All(x => x.IsPass(this));
    public bool IsAcceptable => acceptionConditions.All(x => x.IsPass(this));
    public virtual bool IsSavable => isSavable;

    public event TaskSuccessChangeHandler onTaskSuccessChange;
    public event CompletedHandler onCompleted;
    public event CanceldHandler onCanceld;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, "This quest already registered");

        foreach (var taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (var task in taskGroup.Tasks)
            {
                task.onSuccessChanged += OnSucessChanged;
            }
        }

        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(IsRegistered, "This quest already registered");
        Debug.Assert(!IsCancel, "This quest has been canceled");

        if (IsComplete) return;

        CurrentTaskGroup.ReceiveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;

                if (useAutoComplete)
                {
                    Complete();
                }
            }
            else
            {
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];
                prevTaskGroup.End();
                CurrentTaskGroup.Start();
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        else
            State = QuestState.Running;
    }

    public void Complete()
    {
        CheckIsRunninig();

        foreach (var taskGroup in taskGroups)
        {
            taskGroup.Complete();
        }

        State = QuestState.Complete;

        foreach (var reward in rewards)
        {
            reward.Give(this);
        }

        onCompleted?.Invoke(this);

        onTaskSuccessChange = null;
        onCompleted = null;
        onCanceld = null;
        onNewTaskGroup = null;
    }

    public virtual void Cancel()
    {
        CheckIsRunninig();
        Debug.Assert(IsCancelable, "This quest can't be canceled");

        State = QuestState.Cancel;
        onCanceld?.Invoke(this);
    }

    public Quest Clone()
    {
        var clone = Instantiate(this);
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();

        return clone;
    }

    public QuestSaveData ToSaveData()
    {
        return new QuestSaveData
        {
            codeName = codeName,
            state = State,
            taskGroupIndex = currentTaskGroupIndex,
            taskSuccessCounts = CurrentTaskGroup.Tasks.Select(x => x.CurrentSuccess).ToArray(),
        };
    }

    public void LoadFrom(QuestSaveData saveData)
    {
        State = saveData.state;
        currentTaskGroupIndex = saveData.taskGroupIndex;

        for (int i = 0; i < currentTaskGroupIndex; i++)
        {
            var taskGroup = taskGroups[i];
            taskGroup.Start();
            taskGroup.Complete();
        }

        for (int i = 0; i < saveData.taskSuccessCounts.Length; i++)
        {
            CurrentTaskGroup.Start();
            CurrentTaskGroup.Tasks[i].CurrentSuccess = saveData.taskSuccessCounts[i];
        }
    }

    private void OnSucessChanged(Task task, int currentSuccess, int prevSuccess)
    => onTaskSuccessChange?.Invoke(this, task, currentSuccess, prevSuccess);


    [Conditional("UNITY_EDITOR")]
    private void CheckIsRunninig()
    {
        Debug.Assert(IsRegistered, "This quest already registered");
        Debug.Assert(!IsCancel, "This quest has been canceled");
        Debug.Assert(!IsComplete, "This quest has already been completed");
    }
}
