using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestTargetMarker : MonoBehaviour
{
    [SerializeField] private TaskTarget target;
    [SerializeField] MarkerMaterialData[] markerMaterialDatas;

    private Dictionary<Quest, Task> targetTaskByQuest = new Dictionary<Quest, Task>();
    private Transform cameraTransform;
    private Renderer markerRenderer;

    private int currentRunningTargetCount;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        markerRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        gameObject.SetActive(false);

        QuestSystem.Instance.onQuestRegistered += TryAddTargetQuest;
        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            TryAddTargetQuest(quest);
        }
    }

    private void Update()
    {
        var rotation = Quaternion.LookRotation((cameraTransform.position - transform.position).normalized);
        transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y + 180f, 0);
    }

    private void OnDestroy()
    {
        QuestSystem.Instance.onQuestRegistered -= TryAddTargetQuest;
        foreach((Quest quest, Task task) in targetTaskByQuest)
        {
            quest.onNewTaskGroup -= UpdateTargetTask;
            quest.onCompleted -= RemoveTargetQuest;
            task.onStateChanged -= UpdateRunningTargetTaskCount;
        }
    }

    private void TryAddTargetQuest(Quest quest)
    {
        if(target is not null && quest.ContainsTarget(target))
        {
            quest.onNewTaskGroup += UpdateTargetTask;
            quest.onCompleted += RemoveTargetQuest;

            UpdateTargetTask(quest, quest.CurrentTaskGroup);
        }
    }

    private void UpdateTargetTask(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup = null)
    {
        targetTaskByQuest.Remove(quest);

        var task = currentTaskGroup.FindTaskByTarget(target);
        if(task is not null)
        {
            targetTaskByQuest[quest] = task;
            task.onStateChanged += UpdateRunningTargetTaskCount;

            UpdateRunningTargetTaskCount(task, task.State);
        }
    }

    private void RemoveTargetQuest(Quest quest) => targetTaskByQuest.Remove(quest);

    private void UpdateRunningTargetTaskCount(Task task, TaskState currentState, TaskState prevState = TaskState.InActive)
    {
        if(currentState == TaskState.Running)
        {
            markerRenderer.material = markerMaterialDatas.First(x => x.category == task.Category).markerMaterial;
            currentRunningTargetCount++;
        }
        else
        {
            currentRunningTargetCount--;
        }

        gameObject.SetActive(currentRunningTargetCount != 0);
    }

    [System.Serializable]
    private struct MarkerMaterialData
    {
        public Category category;
        public Material markerMaterial;
    }
}
