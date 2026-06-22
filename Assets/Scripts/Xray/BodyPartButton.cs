using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BodyPartButton : MonoBehaviour
{
    [Header("부위 설정")]
    [SerializeField] private string partName;
    // e.g. {"Body","Chest"} — 루트 폴더에서의 경로
    [SerializeField] private string[] folderPath;

    [Header("스프라이트")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite abnormalSprite;

    [Header("색상")]
    [SerializeField] private Color selectedColor = Color.green;

    private Button button;
    private Color restoreColor; // 클릭 직전 색상 (FileWindow가 red 등으로 바꿔둘 수 있음)

    public string   PartName       => partName;
    public string[] FolderPath     => folderPath;
    public Sprite   NormalSprite   => normalSprite;
    public Sprite   AbnormalSprite => abnormalSprite;

    void Start()
    {
        button = GetComponent<Button>();

        var panel = GetComponentInParent<XRayPanel>();
        if (panel != null)
            panel.Register(this);
        else
            Debug.LogWarning($"[BodyPartButton] {gameObject.name}: 부모에 XRayPanel이 없습니다.");

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // FileWindow가 변경했을 수 있는 현재 색상을 복원 기준으로 저장
        restoreColor = button.colors.normalColor;
        GetComponentInParent<XRayPanel>()?.ShowPart(this);
    }

    public void SetHighlight(bool selected)
    {
        ColorBlock cb       = button.colors;
        Color      c        = selected ? selectedColor : restoreColor;
        cb.normalColor      = c;
        cb.highlightedColor = c;
        cb.pressedColor     = c;
        cb.selectedColor    = c;
        button.colors       = cb;
    }
}
