using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 헥사곤 던전을 "구역(Zone) 단위"로 절차적 생성/관리하는 싱글턴.
///
/// 구조:
/// - 하나의 구역(Zone)은 zoneRadius 범위의 헥사곤 타일 뭉치이며, 테마(ZoneTheme) 하나로 통일된다.
/// - 구역 안에는 보스 타일(Boss)이 정확히 하나 존재한다.
/// - 보스를 클리어하면, 그 보스 타일에서 임의의 방향으로 새로운 구역이 생성된다.
///   이때 두 구역의 타일은 좌표 수학적으로 절대 겹치지 않는다 (아래 HandleBossCleared 참고).
/// - 새 구역의 테마는 이전 구역과 반드시 다르게 뽑힌다.
/// </summary>
public class DungeonGridManager : MonoBehaviour
{
    public static DungeonGridManager Instance { get; private set; }

    [Header("프리팹")]
    [SerializeField] private HexTile hexTilePrefab;
    [SerializeField] private PlayerToken playerToken;

    [Header("생성 설정")]
    [SerializeField] private float hexSize = 1.2f;
    [SerializeField] private int zoneRadius = 3;
    [SerializeField, Range(0f, 1f)] private float wallChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float battleChance = 0.5f;

    // 전역 타일 저장소 (모든 구역의 타일이 같은 좌표 공간에 함께 들어간다)
    private readonly Dictionary<HexCoord, HexTile> tiles = new Dictionary<HexCoord, HexTile>();
    private readonly List<Zone> zones = new List<Zone>();

    private HexCoord currentPlayerCoord;
    private int zoneCounter = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ZoneTheme firstTheme = ZoneThemeUtility.GetRandomTheme();
        GenerateZoneAt(new HexCoord(0, 0), zoneRadius, firstTheme, entryCoord: null);
    }

    // ------------------------------------------------------------------
    // 구역 생성
    // ------------------------------------------------------------------

    /// <summary>
    /// center를 중심으로 하나의 테마 구역을 생성한다.
    /// entryCoord가 null이면 "첫 구역"으로 취급해 center 타일이 Start가 된다.
    /// entryCoord가 있으면 그 좌표가 이 구역의 진입 지점(Empty)이 된다.
    /// </summary>
    private void GenerateZoneAt(HexCoord center, int radius, ZoneTheme theme, HexCoord? entryCoord)
    {
        var zone = new Zone
        {
            zoneIndex = zoneCounter++,
            theme = theme,
            center = center,
            radius = radius,
            entryCoord = entryCoord,
        };

        // 보스 위치를 먼저 정한다 (진입 방향과 겹치지 않는 방향으로).
        HexCoord bossDirection = PickBossDirection(entryCoord, center);
        zone.bossCoord = center + bossDirection * radius;

        // 이 구역이 차지할 모든 타일 좌표를 계산 후, 각 좌표에 타입을 부여하며 생성.
        var coordsInZone = HexCoord.GetRange(center, radius);
        foreach (var coord in coordsInZone)
        {
            HexTileType type = DecideTileType(coord, center, zone, entryCoord);
            SpawnTile(coord, type, theme, zone);
        }

        zones.Add(zone);

        // 진입 지점(또는 첫 구역이면 center) 기준으로 시야를 노출.
        HexCoord revealOrigin = entryCoord ?? center;
        RevealTile(revealOrigin);
        RevealNeighbors(revealOrigin);

        // 첫 구역 생성 시에는 플레이어 토큰을 시작 위치로 즉시 배치.
        if (entryCoord == null)
        {
            currentPlayerCoord = revealOrigin;
            if (playerToken != null && tiles.TryGetValue(revealOrigin, out var startTile))
            {
                playerToken.WarpTo(startTile.transform.position);
            }
        }

        Debug.Log($"[DungeonGridManager] 구역 #{zone.zoneIndex} 생성 완료 - 테마: {theme}, 중심: {center}, 보스: {zone.bossCoord}");
    }

    /// <summary>
    /// 보스를 배치할 방향을 고른다. 진입 방향(들어온 쪽)은 제외해서
    /// "들어온 쪽으로 다시 보스가 붙는" 어색한 상황을 피한다.
    /// (첫 구역처럼 entryCoord가 없으면 완전 랜덤)
    /// </summary>
    private HexCoord PickBossDirection(HexCoord? entryCoord, HexCoord center)
    {
        var directions = HexCoord.Directions;

        if (entryCoord == null)
        {
            return directions[Random.Range(0, directions.Length)];
        }

        // entryCoord는 항상 "center + 어떤방향*radius" 형태로 생성되므로 나눗셈으로 정확히 역산 가능.
        HexCoord diff = entryCoord.Value - center;
        HexCoord towardEntry = new HexCoord(
            zoneRadius != 0 ? diff.q / zoneRadius : diff.q,
            zoneRadius != 0 ? diff.r / zoneRadius : diff.r
        );

        HexCoord picked;
        int guard = 0; // 무한루프 방지
        do
        {
            picked = directions[Random.Range(0, directions.Length)];
            guard++;
        }
        while (picked == towardEntry && guard < 20);

        return picked;
    }

    private HexTileType DecideTileType(HexCoord coord, HexCoord center, Zone zone, HexCoord? entryCoord)
    {
        if (coord == zone.bossCoord) return HexTileType.Boss;
        if (entryCoord.HasValue && coord == entryCoord.Value) return HexTileType.Empty; // 진입로는 항상 통행 가능
        if (!entryCoord.HasValue && coord == center) return HexTileType.Start; // 첫 구역의 시작점

        float roll = Random.value;
        if (roll < wallChance) return HexTileType.Wall;
        if (roll < wallChance + battleChance) return HexTileType.Battle;
        return HexTileType.Empty;
    }

    private void SpawnTile(HexCoord coord, HexTileType type, ZoneTheme theme, Zone zone)
    {
        Vector3 worldPos = coord.ToWorldPosition(hexSize);
        var tile = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, transform);
        tile.name = $"HexTile_{coord.q}_{coord.r}_{theme}";
        tile.Initialize(coord, type, theme);
        tiles[coord] = tile;
        zone.tileCoords.Add(coord);
    }

    // ------------------------------------------------------------------
    // 노출 / 이동
    // ------------------------------------------------------------------

    public void RevealNeighbors(HexCoord center)
    {
        foreach (var neighborCoord in center.GetNeighbors())
        {
            RevealTile(neighborCoord);
        }
    }

    private void RevealTile(HexCoord coord)
    {
        if (tiles.TryGetValue(coord, out var tile))
        {
            tile.Reveal();
        }
    }

    public void OnTileClicked(HexTile clickedTile)
    {
        if (clickedTile.coord.DistanceTo(currentPlayerCoord) != 1)
        {
            Debug.Log("[DungeonGridManager] 인접하지 않은 타일입니다. 이동 불가.");
            return;
        }

        currentPlayerCoord = clickedTile.coord;
        playerToken?.MoveTo(clickedTile.transform.position);

        switch (clickedTile.tileType)
        {
            case HexTileType.Battle:
            case HexTileType.Boss:
                EnterBattle(clickedTile);
                break;

            case HexTileType.Empty:
            case HexTileType.Start:
                clickedTile.MarkCleared();
                RevealNeighbors(clickedTile.coord);
                break;
        }
    }

    // ------------------------------------------------------------------
    // 전투 진입 / 결과 처리
    // ------------------------------------------------------------------

    private void EnterBattle(HexTile battleTile)
    {
        Debug.Log($"[DungeonGridManager] 전투 진입: {battleTile.coord} (타입: {battleTile.tileType})");

        // TODO: 여기서 실제 BattleManager.Instance.StartBattle(...) 호출로 교체
        // 지금은 그레이박스 테스트용으로 즉시 승리 처리
        OnBattleWon(battleTile);
    }

    /// <summary>
    /// 전투 승리 콜백. 실제 전투 시스템 완성 후 이 함수를 승리 이벤트에 연결하면 된다.
    /// 보스 타일이었다면 새로운 구역을 확장 생성한다.
    /// </summary>
    public void OnBattleWon(HexTile clearedTile)
    {
        clearedTile.MarkCleared();

        if (clearedTile.tileType == HexTileType.Boss)
        {
            HandleBossCleared(clearedTile);
        }
        else
        {
            RevealNeighbors(clearedTile.coord);
        }
    }

    /// <summary>
    /// 보스 클리어 시 호출.
    /// 보스 타일에서 임의의 방향으로 radius+1칸 떨어진 지점을 다음 구역의 중심으로 삼는다.
    /// 이렇게 하면 보스 타일의 바로 옆 칸(같은 방향으로 1칸)이 자동으로 다음 구역의 경계 타일이 되어,
    /// 두 구역이 겹치지 않으면서도 자연스럽게 이어진다. (수학적 증명은 README 참고)
    /// </summary>
    private void HandleBossCleared(HexTile bossTile)
    {
        Zone currentZone = FindZoneContaining(bossTile.coord);
        if (currentZone == null)
        {
            Debug.LogWarning("[DungeonGridManager] 보스 타일이 속한 구역을 찾지 못했습니다.");
            return;
        }

        currentZone.bossCleared = true;

        var directions = HexCoord.Directions;
        HexCoord expandDirection = directions[Random.Range(0, directions.Length)];

        HexCoord entryCoord = bossTile.coord + expandDirection;
        HexCoord newCenter = bossTile.coord + expandDirection * (currentZone.radius + 1);

        ZoneTheme nextTheme = ZoneThemeUtility.GetRandomTheme(exclude: currentZone.theme);

        Debug.Log($"[DungeonGridManager] 보스 클리어! 구역 #{currentZone.zoneIndex}({currentZone.theme}) -> 다음 구역({nextTheme}) 생성");

        GenerateZoneAt(newCenter, currentZone.radius, nextTheme, entryCoord);
    }

    private Zone FindZoneContaining(HexCoord coord)
    {
        foreach (var zone in zones)
        {
            if (zone.Contains(coord)) return zone;
        }
        return null;
    }
}
