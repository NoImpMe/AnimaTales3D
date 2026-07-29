using UnityEngine;

/// <summary>
/// 헥사곤 타일 하나를 나타내는 컴포넌트.
/// 그레이박스 단계에서는 Cube/Cylinder 프리팹에 부착해서 사용한다.
/// 나중에 3D 에셋으로 교체할 때도 이 컴포넌트 구조는 그대로 유지하면 된다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HexTile : MonoBehaviour
{
    [Header("데이터")]
    public HexCoord coord;
    public HexTileType tileType;
    public ZoneTheme theme;

    [Header("상태")]
    public bool isRevealed = false;   // 플레이어에게 보이는 상태인가
    public bool isCleared = false;    // 전투를 클리어했거나 이미 지나간 타일인가

    [Header("그레이박스 표시용 (나중에 3D 에셋으로 교체)")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color startColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color battleColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color wallColor = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color emptyColor = new Color(0.8f, 0.8f, 0.6f);
    [SerializeField] private Color bossColor = new Color(0.6f, 0.1f, 0.6f);
    [SerializeField] private Color hiddenColor = new Color(0.05f, 0.05f, 0.05f);
    [SerializeField, Range(0f, 1f)] private float themeTintStrength = 0.45f; // 테마색을 얼마나 섞을지

    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    /// <summary>
    /// 타일 데이터를 초기화하고 그레이박스 색상을 타입에 맞게 세팅한다.
    /// DungeonGridManager가 타일을 생성한 직후 호출.
    /// </summary>
    public void Initialize(HexCoord hexCoord, HexTileType type, ZoneTheme zoneTheme)
    {
        coord = hexCoord;
        tileType = type;
        theme = zoneTheme;
        isRevealed = false;
        isCleared = false;
        RefreshVisual();
    }

    /// <summary>
    /// 타일을 플레이어에게 노출시킨다.
    /// 기존 2D 로직의 "인접 타일이 플레이어에게 보여진다" 부분과 동일한 역할.
    /// </summary>
    public void Reveal()
    {
        isRevealed = true;
        RefreshVisual();
    }

    public void MarkCleared()
    {
        isCleared = true;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (meshRenderer == null) return;

        if (!isRevealed)
        {
            meshRenderer.material.color = hiddenColor;
            return;
        }

        Color baseColor = tileType switch
        {
            HexTileType.Start => startColor,
            HexTileType.Battle => battleColor,
            HexTileType.Wall => wallColor,
            HexTileType.Boss => bossColor,
            _ => emptyColor,
        };

        // 타일 역할(색)과 구역 테마 색을 섞어서, "역할"과 "어느 구역인지"를 동시에 구분할 수 있게 한다.
        // Wall은 테마색을 섞지 않아 항상 어둡게 유지 (이동 불가라는 인지가 우선이므로).
        Color c = tileType == HexTileType.Wall
            ? baseColor
            : Color.Lerp(baseColor, ZoneThemeUtility.GetColor(theme), themeTintStrength);

        // 이미 클리어한 타일은 살짝 어둡게 표시해서 구분
        if (isCleared) c *= 0.6f;

        meshRenderer.material.color = c;
    }

    /// <summary>
    /// 마우스 클릭(그레이박스 단계 임시 인터랙션).
    /// 실제로는 DungeonGridManager가 Raycast로 이 타일을 감지해서 처리한다.
    /// </summary>
    private void OnMouseDown()
    {
        if (!isRevealed) return;
        if (tileType == HexTileType.Wall) return;

        DungeonGridManager.Instance?.OnTileClicked(this);
    }
}
