using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] private List<Quest> quests = new List<Quest>();
    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string codeName) => quests.FirstOrDefault(x => x.CodeName == codeName);

#if UNITY_EDITOR
    [ContextMenu("Find Quests")]
    private void FindQuests()
    {
        FindQuestsBy<Quest>();
    }

    [ContextMenu("Find Achievements")]
    private void FindAchievements()
    {
        FindQuestsBy<Achievement>();
    }

    private void FindQuestsBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            
            if(quest.GetType() == typeof(T))
            {
                quests.Add(quest);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
