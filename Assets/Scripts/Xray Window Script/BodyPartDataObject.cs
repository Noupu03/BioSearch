using UnityEngine;

[System.Serializable] // 리스트 안에서 인스펙터로 편집 가능
public class BodyPartDataObject
{
    public string partName;
    public Sprite normalSprite;
    public Sprite errorSprite;
    public bool isError;
    public bool isChecked;

    public void Initialize()
    {
        isError = false;
        isChecked = false;
    }
}
