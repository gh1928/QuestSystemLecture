using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystemTest : MonoBehaviour
{
    [SerializeField] private Quest quest;
    [SerializeField] private Category category;
    [SerializeField] private TaskTarget target;

    void Start()
    {
        var questSystem = QuestSystem.Instance;

        questSystem.onQuestRegistered += (quest) =>
        {
            print($"Quest registered: {quest.CodeName}");
            print($"Acitve Quests Count:{questSystem.CompletedQuests.Count}");
        };

        questSystem.onQuestCompleted += (quest) =>
        {
            print($"Quest completed: {quest.CodeName}");
            print($"Acitve Quests Count:{questSystem.CompletedQuests.Count}");
        };

        var newQuest = questSystem.Register(quest);
        newQuest.onTaskSuccessChange += (quest, task, currentSuccess, prevSuccess) =>
        {
            print($"Task {task.CodeName} success changed from {prevSuccess} to {currentSuccess}");
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            QuestSystem.Instance.ReceiveReport(category, target, 1);
        }
    }
}


