using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestTrackerView : MonoBehaviour
{
    [SerializeField] private QuestTracker questTrackerPrefab;
    [SerializeField] private CategoryColor[] categoryColors;

    private void Start()
    {
        QuestSystem.Instance.onQuestRegistered += CreateQuestTracker;

        foreach(var quest in QuestSystem.Instance.ActiveQuests)
        {
            CreateQuestTracker(quest);
        }
    }

    private void OnDestroy()
    {
        if(QuestSystem.Instance is not null)
            QuestSystem.Instance.onQuestRegistered -= CreateQuestTracker;
    }

    private void CreateQuestTracker(Quest quest)
    {
        var categoryColor = categoryColors.FirstOrDefault(x => x.category == quest.Category);
        var color = categoryColor.category is null ? Color.white : categoryColor.color;
        Instantiate(questTrackerPrefab, transform).Setup(quest, color);
    }

    [System.Serializable]
    private struct CategoryColor
    {
        public Category category;
        public Color color;
    }
}


