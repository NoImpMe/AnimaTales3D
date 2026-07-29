using System.Collections.Generic;

/// <summary>
/// 하나의 테마 구역(Zone)이 가진 데이터.
/// MonoBehaviour가 아닌 순수 데이터 클래스 — DungeonGridManager가 리스트로 들고 관리한다.
/// </summary>
public class Zone
{
    public int zoneIndex;
    public ZoneTheme theme;
    public HexCoord center;
    public int radius;

    public HexCoord bossCoord;
    public HexCoord? entryCoord; // 이전 구역에서 넘어오는 진입 타일 (첫 구역은 null)

    public bool bossCleared;

    public readonly HashSet<HexCoord> tileCoords = new HashSet<HexCoord>();

    public bool Contains(HexCoord coord) => tileCoords.Contains(coord);
}
