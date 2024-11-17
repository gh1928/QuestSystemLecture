using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TaskDescriptor taskDescriptorPrefab;

    private Dictionary<Task, TaskDescriptor> taskDescriptorsByTask = new Dictionary<Task, TaskDescriptor>();

    private Quest targetQuest;

    private void OnDestroy()
    {
        if(targetQuest is not null)
        {
            targetQuest.onNewTaskGroup -= UpdateTaskDescriptors;
            targetQuest.onCompleted -= DestroySelf;
        }

        foreach(var tuple in taskDescriptorsByTask)
        {
            var task = tuple.Key;
            task.onSuccessChanged -= UpdateText;
        }
    }

    public void Setup(Quest targetQuest, Color titleColor)
    {
        this.targetQuest = targetQuest;

        questTitleText.text = targetQuest.Category is null ?
            targetQuest.DisplayName :
            $"[{targetQuest.Category.DisplayName}] {targetQuest.DisplayName}";

        questTitleText.color = titleColor;

        targetQuest.onNewTaskGroup += UpdateTaskDescriptors;
        targetQuest.onCompleted += DestroySelf;

        var taskGroups = targetQuest.TaskGroups;
        UpdateTaskDescriptors(targetQuest, taskGroups[0]);

        if (taskGroups[0] != targetQuest.CurrentTaskGroup)
        {
            for(int i =  1; i < taskGroups.Count; i++)
            {
                var taskGroup = taskGroups[i];
                UpdateTaskDescriptors(targetQuest, taskGroup, taskGroups[i - 1]);

                if(taskGroup == targetQuest.CurrentTaskGroup)
                {
                    break;
                }
            }
        }
    }

    private void UpdateTaskDescriptors(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup = null)
    {
        foreach (var task in currentTaskGroup.Tasks)
        {
            var taskDescriptor = Instantiate(taskDescriptorPrefab, transform);
            taskDescriptor.UpdateText(task);
            task.onSuccessChanged += UpdateText;

            taskDescriptorsByTask.Add(task, taskDescriptor);
        }

        if (prevTaskGroup is not null)
        {
            foreach (var task in prevTaskGroup.Tasks)
            {
                var taskDescriptor = taskDescriptorsByTask[task];
                taskDescriptor.UpdateTextUsingStrikeThrough(task);
            }
        }
    }

    private void UpdateText(Task task, int currentSucess, int prevSucess)
    {
        taskDescriptorsByTask[task].UpdateText(task);
    }

    private void DestroySelf(Quest quest)
    {
        Destroy(gameObject);
    }
}
