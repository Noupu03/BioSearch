using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BodyPartsData", menuName = "Body Parts/BodyPartsData", order = 0)]
public class BodyPartsData : ScriptableObject
{
    [Header("BodyParts 리스트")]
    [SerializeField]
    public List<BodyPartDataObject> parts = new List<BodyPartDataObject>();

    // 이름으로 BodyPart 가져오기 (대소문자/공백 무시)
    public BodyPartDataObject GetPart(string name)
    {
        string key = name.Replace(" ", "").ToLower();
        foreach (var part in parts)
        {
            if (part.partName.Replace(" ", "").ToLower() == key)
                return part;
        }
        return null;
    }

    public List<BodyPartDataObject> GetAllParts() => parts;

    public void InitializeAllParts()
    {
        foreach (var part in parts)
            part.Initialize();
    }

    public void RandomlySetErrors(float chance)
    {
        foreach (var part in parts)
            part.isError = (Random.value < chance);
    }
}
