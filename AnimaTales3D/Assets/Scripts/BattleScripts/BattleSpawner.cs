using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 씬 진입 시 대형 규칙(BattleFormation)에 따라 아군/적군을 스폰한다.
/// 스폰이 끝나면 <see cref="BattleController.BeginBattle"/>을 호출해 실제 턴 진행을 시작시킨다 —
/// BattleController.Awake()가 먼저 Instance를 세팅해둔다는 보장(모든 Awake가 모든 Start보다 먼저 끝남)에
/// 기대어, Start() 간의 실행 순서(보장되지 않음)에 의존하지 않고 명시적으로 순서를 맞춘다.
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

        BattleController.Instance?.BeginBattle();
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
