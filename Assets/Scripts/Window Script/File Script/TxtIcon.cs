using UnityEngine;

/// <summary>
/// TXT ���� ������ (�ؽ�Ʈ ���� ���� �� ó��)
/// </summary>
public class TxtIcon : FileIcon
{
    protected override void OnDoubleClick()
    {
        LogWindowManager.Instance.Log($"TXT ���� ����: {file.name}.{file.extension}");
        // TODO: �ؽ�Ʈ ��� ���� ��� ����
    }
}
