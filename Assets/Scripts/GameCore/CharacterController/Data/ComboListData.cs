using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboListData : ScriptableObject
{
    public List<DamageData> attackData;

    public ComboListData()
    {
        attackData = new List<DamageData>();
    }

}
