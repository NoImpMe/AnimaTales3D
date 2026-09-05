using UnityEngine;

/// <summary>
/// 헥사곤 타일 하나를 나타내는 컴포넌트.
/// 그레이박스 단계에서는 Cube 기반 프리팹에 부착해서 사용한다. 마을/전투/보스 타입은 기본 형태 위에
/// 도형 장식(Decoration)을 얹은 별도 프리팹(DungeonGridManager가 타입별로 골라 생성)을 쓴다.
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
    [SerializeField] private Color startColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color battleColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color wallColor = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color emptyColor = new Color(0.8f, 0.8f, 0.6f);
    [SerializeField] private Color bossColor = new Color(0.6f, 0.1f, 0.6f);
    [SerializeField, Range(0f, 1f)] private float themeTintStrength = 0.45f; // 테마색을 얼마나 섞을지

    [Header("타입별 장식 (없으면 무시)")]
    [Tooltip("타입별로 다른 모양의 장식을 얹은 프리팹(마을/전투/보스)에서만 연결. 인스턴스마다 살짝 다르게 보이도록 회전을 무작위로 준다.")]
    [SerializeField] private Transform decoration;
    [SerializeField] private float decorationRotationJitterDegrees = 360f;

    [Header("전투 타일 실사 이미지 (없으면 무시)")]
    [Tooltip("설정하면 코드 색상 대신 Resources/Tile/<테마>Battle.png(Sprite)를 로드해 이 SpriteRenderer에 입힌다. Sprite-Lit-Default 셰이더를 써야 Light2D의 영향을 받는다.")]
    [SerializeField] private SpriteRenderer themeArtRenderer;

    [Header("2.5D 연출 (Light2D/Particle, 없으면 무시)")]
    [Tooltip("Light2D·ParticleSystem을 담은 자식. 미공개 상태에서는 빛/파티클도 실루엣처럼 새어 보이면 안 되므로 공개 여부에 맞춰 통째로 활성/비활성한다.")]
    [SerializeField] private GameObject effects;

    // MeshRenderer(그레이박스 프리미티브)와 SpriteRenderer(실사 이미지)를 모두 아우르는 공통 타입으로 잡아야
    // 실사 이미지 타일도 공개/비공개·색상 처리 루프에 함께 걸린다.
    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        if (decoration != null)
        {
            float randomYaw = Random.Range(0f, decorationRotationJitterDegrees);
            decoration.Rotate(Vector3.up, randomYaw, Space.Self);
        }
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

        if (themeArtRenderer != null)
        {
            var sprite = Resources.Load<Sprite>($"Tile/{theme}Battle");
            if (sprite != null) themeArtRenderer.sprite = sprite;
        }

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
        if (renderers == null || renderers.Length == 0) return;

        // 인접하지 않아 아직 탐험하지 않은 타일은 실루엣조차 보이면 안 되므로 렌더러 자체를 끈다
        // (색만 어둡게 칠하는 방식은 형태가 비쳐 보여서 안 됨).
        foreach (var renderer in renderers)
        {
            renderer.enabled = isRevealed;
        }

        // Light2D는 Renderer가 아니라서 위 루프에 안 걸린다 — 따로 꺼줘야 빛이 새지 않는다.
        if (effects != null) effects.SetActive(isRevealed);

        if (!isRevealed) return;

        // 실사 이미지(themeArtRenderer)를 쓰는 타일은 이미지 자체가 테마를 표현하므로 흰색(원본 그대로) 유지.
        Color baseColor = themeArtRenderer != null
            ? Color.white
            : tileType switch
            {
                HexTileType.Start => startColor,
                HexTileType.Battle => battleColor,
                HexTileType.Wall => wallColor,
                HexTileType.Boss => bossColor,
                _ => emptyColor,
            };

        // 타일 역할(색)과 구역 테마 색을 섞어서, "역할"과 "어느 구역인지"를 동시에 구분할 수 있게 한다.
        // Wall과 실사 이미지 타일은 테마색을 섞지 않는다 (Wall은 항상 어둡게 유지, 이미지는 원본 유지가 우선).
        Color c = (tileType == HexTileType.Wall || themeArtRenderer != null)
            ? baseColor
            : Color.Lerp(baseColor, ZoneThemeUtility.GetColor(theme), themeTintStrength);

        // 이미 클리어한 타일은 살짝 어둡게 표시해서 구분 (알파는 그대로 둬야 실사 이미지의 투명 배경이 안 깨짐)
        if (isCleared) c = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, c.a);

        foreach (var renderer in renderers)
        {
            // SpriteRenderer는 material.color가 아니라 전용 color 프로퍼티를 써야 한다.
            // material.color(내부적으로 히든 "_Color" 프로퍼티)를 강제로 건드리면 Sprite-Lit-Default
            // 셰이더의 알파 블렌딩이 깨져 투명 배경이 흰 사각형으로 보이는 문제가 있었다.
            if (renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.color = c;
            else
                renderer.material.color = c;
        }
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
