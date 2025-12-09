using UnityEngine;

[System.Serializable]
public class FaxData
{
    public GameDateTime deliveryTime;  // 언제 배송될지
    public string faxText;             // 팩스 내용
}
