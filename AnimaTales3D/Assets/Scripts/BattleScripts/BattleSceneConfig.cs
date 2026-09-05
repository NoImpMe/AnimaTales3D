using UnityEngine;

/// <summary>
/// 전투 씬(BattleScene)의 스폰·비주얼·카메라 수치를 소유하는 Config. MonoBehaviour에 숫자 리터럴을
/// 두지 않기 위해 이 ScriptableObject 하나로 모은다(DungeonGenerationConfig와 동일한 패턴).
/// </summary>
[CreateAssetMenu(fileName = "BattleSceneConfig", menuName = "AnimaTales/Battle Scene Config")]
public class BattleSceneConfig : ScriptableObject
{
    [Header("스폰")]
    public int level = 5;
    public float sideOffset = 4f;

    [Header("유닛 비주얼")]
    [Tooltip("스프라이트 피벗(중앙)이 지면 위로 뜨는 높이 — 캐릭터 발이 지면에 닿도록 보정")]
    public float unitVisualYOffset = 1.1f;

    [Header("HP 바")]
    [Tooltip("유닛 위 어느 월드 높이에 HP 바를 띄울지")]
    public float hpBarYOffset = 2.4f;

    [Header("카메라(Cinemachine)")]
    public float cameraPitchDegrees = 25f;
    public float cameraDistance = 12f;
    public float cameraHeight = 6f;
}
