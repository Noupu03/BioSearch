using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ExeDataSO", menuName = "OS/Exe Data SO", order = 2)]
public class ExeDataSO : ScriptableObject
{
    public List<ExeData> exeDatas = new List<ExeData>();
}
