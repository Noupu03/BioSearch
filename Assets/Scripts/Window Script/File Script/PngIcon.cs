using UnityEngine;

/// <summary>
/// PNG ���� ������ (�̹��� ���� ���� �� ó��)
/// </summary>
public class PngIcon : FileIcon
{
    protected override void OnDoubleClick()
    {
        LogWindowManager.Instance.Log($"PNG ���� ����: {file.name}.{file.extension}");
        // TODO: �̹��� ��� ���� ��� ����
    }
}
