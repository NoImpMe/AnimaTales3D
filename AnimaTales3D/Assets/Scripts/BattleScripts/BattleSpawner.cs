using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 씬 진입 시 대형 규칙(BattleFormation)에 따라 아군/적군을 스폰하는 구조 스켈레톤.
/// 이번 단계는 씬 구조·스폰·카메라 프레이밍·기본 HP UI까지만 다룬다 — 실제 턴 진행(플레이어 입력 →
/// 스킬 선택 → 적 AI 응답 → 승패 판정)은 아직 연결되지 않았다(다음 단계에서 진행 예정).
/// </summary>
public class BattleSpawner : MonoBehaviour
{
    [SerializeField] private BattleSceneConfig config;
    [SerializeField] private TextAsset animaListJson;
    [SerializeField] private GameObject unitVisualPrefab;
    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private Transform hpBarCanvasRoot;
    [SerializeField] private Camera trackingCamera;
    [SerializeField] private string[] allyTemplateNames = { "felix1", "felix1", "felix1" };
    [SerializeField] private string[] enemyTemplateNames = { "irascor1", "irascor1", "irascor1" };

    public IReadOnlyList<AnimaUnit> AllyUnits => allyUnits;
    public IReadOnlyList<AnimaUnit> EnemyUnits => enemyUnits;

    private readonly List<AnimaUnit> allyUnits = new();
    private readonly List<AnimaUnit> enemyUnits = new();

    private void Start()
    {
        List<AnimaTemplate> templates = AnimaDatabase.ParseJsonArray(animaListJson.text);

        SpawnSide(templates, allyTemplateNames, isAlly: true, allyUnits);
        SpawnSide(templates, enemyTemplateNames, isAlly: false, enemyUnits);
    }

    private void SpawnSide(List<AnimaTemplate> templates, string[] names, bool isAlly, List<AnimaUnit> resultList)
    {
        int count = names.Length;
        if (count < BattleFormation.MinUnitsPerSide || count > BattleFormation.MaxUnitsPerSide)
        {
            Debug.LogError($"[BattleSpawner] {(isAlly ? "아군" : "적군")} 인원 수({count})가 {BattleFormation.MinUnitsPerSide}~{BattleFormation.MaxUnitsPerSide} 범위를 벗어났습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            AnimaTemplate template = AnimaDatabase.Find(templates, names[i]);
            if (template == null)
            {
                Debug.LogError($"[BattleSpawner] '{names[i]}' 템플릿을 AnimaList.json에서 찾을 수 없습니다.");
                continue;
            }

            AnimaUnit unit = AnimaUnit.Initialize(template, config.level, isAlly);
            resultList.Add(unit);

            Vector3 localPos = isAlly
                ? BattleFormation.GetAllySlotPosition(i, count, config.sideOffset)
                : BattleFormation.GetEnemySlotPosition(i, count, config.sideOffset);
            Vector3 worldPos = localPos + Vector3.up * config.unitVisualYOffset;

            GameObject visualGO = Instantiate(unitVisualPrefab, worldPos, Quaternion.identity, transform);
            visualGO.name = $"{unit.UnitName}_{(isAlly ? "Ally" : "Enemy")}{i}";
            var visual = visualGO.GetComponent<BattleUnitVisual>();
            visual.Bind(unit);

            GameObject hpBarGO = Instantiate(hpBarPrefab, hpBarCanvasRoot);
            var hpBar = hpBarGO.GetComponent<HpBarWorldFollow>();
            hpBar.Bind(visualGO.transform, unit, config.hpBarYOffset, trackingCamera);
        }
    }
}
