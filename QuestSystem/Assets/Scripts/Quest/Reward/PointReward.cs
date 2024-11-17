using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PointReward_", menuName = "Quest/Reward/PointReward")]
public class PointReward : Reward
{
    public override void Give(Quest quest)
    {
        GameSystem.Instance.AddScore(Quantity);
        PlayerPrefs.SetInt("bonusScore", Quantity);
        PlayerPrefs.Save();
    }
}
