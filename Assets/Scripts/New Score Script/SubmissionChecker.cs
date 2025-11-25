
using UnityEngine;

public class SubmissionChecker : MonoBehaviour
{
    // 파일 체크 통계
    public static string CheckFilesStatus()
    {
        if (FileWindow.Instance == null)
            return "FileWindow 미설정";

        int mustSubmit = 0;       // isImportant = true
        int correctChecked = 0;   // isImportant && isChecked

        foreach (var filedata in FileWindow.Instance.allFiles)
        {
            if (filedata.isImportant)
            {
                mustSubmit++;

                if (filedata.isChecked)
                    correctChecked++;
            }
        }

        return $"제출해야할 파일 개수 : {mustSubmit}, 맞게 제출한 파일 개수 : {correctChecked}";
    }


    // 부품 체크 통계
    public static string CheckPartsStatus()
    {
        if (MachinePartViewer.Instance == null) return "MachinePartViewer 미설정";

        int totalErrorParts = 0;
        int correctChecked = 0;
        var parts = MachinePartViewer.Instance.GetPartDict();
        foreach (var kvp in parts)
        {
            BodyPart part = kvp.Value;
        
        if (part == null) continue;

            if (part.isError)
            {
                totalErrorParts++;
                if (part.isChecked)
                    correctChecked++;
            }
        }

        return $"오류가 생긴 부품 개수 : {totalErrorParts}, 맞게 체크한 부품 개수 : {correctChecked}";
    }

    // 전체 통계 출력
    public static void PrintSubmissionStatus()
    {
        Debug.Log(CheckFilesStatus());
        Debug.Log(CheckPartsStatus());
    }
}
