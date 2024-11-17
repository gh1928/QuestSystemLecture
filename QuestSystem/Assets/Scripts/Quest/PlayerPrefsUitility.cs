using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsUitility : MonoBehaviour
{
    [ContextMenu("Delete Save Data")]
    private void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
    }
}
