using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FileDataSO", menuName = "OS/File Data SO", order = 1)]
public class FileDataSO : ScriptableObject
{
    public List<FileData> fileDatas = new List<FileData>();
}
