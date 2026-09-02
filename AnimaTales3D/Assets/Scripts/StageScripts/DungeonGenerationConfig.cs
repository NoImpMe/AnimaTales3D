using UnityEngine;

/// <summary>
/// 던전 절차적 생성에 쓰이는 모든 수치를 소유하는 Config.
/// DungeonGridManager는 이 값들을 필드로 직접 들고 있지 않고 이 Config를 참조해서 읽기만 한다.
/// (지침: "모든 수치는 ScriptableObject Config가 소유. MonoBehaviour에 숫자 리터럴 금지")
///
/// 기본값은 기존 DungeonGridManager 인스펙터에 실제로 오버라이드돼 있던 값(1.2 / 3 / 0.25 / 0.45)과
/// 동일하게 맞춰, Config로 옮기는 과정에서 생성 결과가 달라지지 않도록 했다 (2026-09-03 리팩터링).
/// </summary>
[CreateAssetMenu(fileName = "DungeonGenerationConfig", menuName = "AnimaTales3D/Dungeon Generation Config")]
public class DungeonGenerationConfig : ScriptableObject
{
    [Header("타일 크기 / 구역 반경")]
    [Min(0.01f)] public float hexSize = 1.2f;
    [Min(1)] public int zoneRadius = 3;

    [Header("타일 타입 확률 (DungeonZonePlanner.DecideTileType에서 사용)")]
    [Range(0f, 1f)] public float wallChance = 0.25f;
    [Range(0f, 1f)] public float battleChance = 0.45f;
}
