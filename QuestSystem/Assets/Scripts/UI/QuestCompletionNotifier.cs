using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class QuestCompletionNotifier : MonoBehaviour
{
    [SerializeField] private string titleDescription;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private float showTime = 3f;

    private Queue<Quest> reservedQuests = new Queue<Quest>();
    private StringBuilder stringBuilder = new StringBuilder();

    private void Start()
    {
        var questSystem = QuestSystem.Instance;
        questSystem.onQuestCompleted += Notify;
        questSystem.onAchievementCompleted += Notify;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem is not null)
        {
            questSystem.onQuestCompleted -= Notify;
            questSystem.onAchievementCompleted -= Notify; 
        }
    }

    private void Notify(Quest quest)
    {
        reservedQuests.Enqueue(quest);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowNotice());
        }
    }

    private IEnumerator ShowNotice()
    {
        var waitSeconds = new WaitForSeconds(showTime);

        Quest quest;
        while (reservedQuests.TryDequeue(out quest))
        {
            titleText.text = titleDescription.Replace("%{dn}", quest.DisplayName);
            foreach (var reward in quest.Rewards)
            {
                stringBuilder.Append(reward.Description);
                stringBuilder.Append(" ");
                stringBuilder.Append(reward.Quantity);
                stringBuilder.Append(" ");
            }
            rewardText.text = stringBuilder.ToString();
            stringBuilder.Clear();

            yield return waitSeconds;
        }

        gameObject.SetActive(false);
    }
}
