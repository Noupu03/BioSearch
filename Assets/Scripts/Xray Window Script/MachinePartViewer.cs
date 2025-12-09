using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MachinePartViewer : MonoBehaviour
{
    public static MachinePartViewer Instance;

    [Header("좌측 카메라창")]
    [SerializeField] private Image cameraPanel;

    [Header("BodyPartsData 참조")]
    [SerializeField] private BodyPartsData bodyPartsData;

    [Header("각 BodyPart와 매핑될 버튼들")]
    [SerializeField] private Button br1Btn;
    [SerializeField] private Button br2Btn;
    [SerializeField] private Button br3Btn;
    [SerializeField] private Button br4Btn;
    [SerializeField] private Button br5Btn;
    [SerializeField] private Button br6Btn;
    //[SerializeField] private Button br7Btn;
    [SerializeField] private Button br8Btn;
    

    private Dictionary<string, Button> buttonDict;

    private void Awake()
    {
        Instance = this;
        BuildButtonDict();
        RegisterButtonEvents();
    }

    private void BuildButtonDict()
    {
        buttonDict = new Dictionary<string, Button>
        {
            { "br1", br1Btn },
            { "br2", br2Btn },
            { "br3", br3Btn },
            { "br4", br4Btn },
            { "br5", br5Btn },
            { "br6", br6Btn },
            //{ "br7", br7Btn },
            { "br8", br8Btn },
        };
    }

    private void RegisterButtonEvents()
    {
        foreach (var kvp in buttonDict)
        {
            string partName = kvp.Key;
            Button btn = kvp.Value;

            if (btn != null)
                btn.onClick.AddListener(() => OnPartClicked(partName));
        }
    }

    private void OnPartClicked(string partName)
    {
        BodyPartDataObject part = bodyPartsData.GetPart(partName);
        if (part == null) return;

        part.isChecked = !part.isChecked;

        UpdateButtonVisual(partName, part.isChecked);

        cameraPanel.sprite =
            part.isError ? part.errorSprite : part.normalSprite;
    }

    private void UpdateButtonVisual(string name, bool isChecked)
    {
        if (!buttonDict.TryGetValue(name, out Button btn))
            return;

        ColorBlock c = btn.colors;
        if (isChecked)
        {
            c.normalColor = Color.green;
            c.highlightedColor = Color.green;
        }
        else
        {
            c.normalColor = Color.white;
            c.highlightedColor = Color.white;
        }
        btn.colors = c;
    }
}
