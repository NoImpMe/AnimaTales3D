using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 행동 선택 단계 — 원본에는 없던 3D UI 전용 상태(원본은 EventSystem 기반 버튼 셀렉트 방식).
/// </summary>
public enum BattlePhase
{
    AwaitingAction,
    AwaitingTarget,
    Resolving,
    Finished,
}

/// <summary>
/// 실제 턴 진행 연결 — 순수 로직 레이어(TurnManager/BuffManager/BattleMath/EnemyAI)를 실제 스폰된
/// AnimaUnit에 연결해 턴제 전투를 돌린다. 2D 원본(BattleManager.cs, 1650줄+)은 EventSystem 기반
/// 버튼 셀렉트·DamageNumberPro·TMPro가 깊이 얽힌 God Object라 그대로 포팅하지 않고(LOG #4에서
/// "레이어 분리 우선" 확정), 이미 이식된 순수 로직만 재사용해 3D UI(클릭 타게팅 + 버튼 2개)로 새로
/// 연결했다. 보존 대상은 CONVERSION_SPEC.md 2절에 명시된 것만: Speed 내림차순 턴 큐, 실드 우선 소모,
/// 데미지 공식, 적 AI 가중 랜덤, 버프 TickOne이 "유닛 행동 종료 시점"에 도는 타이밍.
///
/// 스킬 타입 중 Single*(SingleAttack/SingleHeal/SingleShield/SingleBuff/SingleDebuff)만 지원한다 —
/// 현재 데모 로스터(felix1=SingleBuff, irascor1=스킬 없음)가 Multi* 타입을 전혀 안 쓰기도 하고,
/// Multi*(전체 대상) 지원은 별도 다음 작업으로 미룬다.
/// </summary>
public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleSpawner spawner;
    [SerializeField] private TextAsset skillListJson;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Text turnStatusText;
    [SerializeField] private Text resultText;

    public static BattleController Instance { get; private set; }

    public AnimaUnit CurrentActor => roundQueue.Count > 0 ? roundQueue[0] : null;
    public BattlePhase Phase => phase;
    public BattleState State => state;

    private TurnManager<AnimaUnit> turnManager;
    private readonly List<AnimaUnit> roundQueue = new();
    private List<SkillData> skillList;
    private BuffManager<AnimaUnit> buffManager;
    private BattleState state;
    private BattlePhase phase;
    private BattleActionType pendingActionType;
    private SkillData pendingSkill;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        skillButton.onClick.AddListener(OnSkillButtonClicked);
        resultText.gameObject.SetActive(false);
        state = BattleState.Start;
    }

    /// <summary>BattleSpawner가 스폰을 끝낸 뒤 호출한다 — Start() 실행 순서에 기대지 않기 위해
    /// (Awake는 모든 컴포넌트가 먼저 끝난다는 보장을 이용해 Instance를 여기서 세팅) 명시적으로 연결한다.</summary>
    public void BeginBattle()
    {
        skillList = SkillDatabase.ParseJsonArray(skillListJson.text);
        buffManager = new BuffManager<AnimaUnit>();
        turnManager = new TurnManager<AnimaUnit>();
        StartNewRound();
    }

    private void StartNewRound()
    {
        turnManager.ResetTurnList();
        foreach (var unit in AllLivingUnits()) turnManager.InsertUnit(unit);
        roundQueue.Clear();
        roundQueue.AddRange(turnManager.UpdateTurnList());
        AdvanceToNextActor();
    }

    private IEnumerable<AnimaUnit> AllLivingUnits()
    {
        foreach (var u in spawner.AllyUnits) if (!u.Animadie) yield return u;
        foreach (var u in spawner.EnemyUnits) if (!u.Animadie) yield return u;
    }

    private void AdvanceToNextActor()
    {
        if (CheckBattleEnd()) return;

        if (roundQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        AnimaUnit actor = CurrentActor;
        if (actor.Animadie)
        {
            roundQueue.RemoveAt(0);
            AdvanceToNextActor();
            return;
        }

        if (actor.IsAlly)
        {
            state = BattleState.PlayerTurn;
            phase = BattlePhase.AwaitingAction;
            SetActionButtonsInteractable(true, actor.SkillNames.Count > 0);
            UpdateTurnStatusText(actor);
        }
        else
        {
            state = BattleState.EnemyTurn;
            phase = BattlePhase.Resolving;
            SetActionButtonsInteractable(false, false);
            UpdateTurnStatusText(actor);
            StartCoroutine(ResolveEnemyTurnNextFrame());
        }
    }

    private IEnumerator ResolveEnemyTurnNextFrame()
    {
        // 한 프레임 늦춰 상태 텍스트가 실제로 화면에 반영된 뒤 처리되도록 함(연출 자리 확보 — 추후 애니메이션으로 대체 예정).
        yield return null;
        ResolveEnemyTurn();
    }

    private void ResolveEnemyTurn()
    {
        AnimaUnit actor = CurrentActor;
        var baseWeights = new List<WeightedAction> { new(BattleActionType.Attack, 1f), new(BattleActionType.UseSkill, 1f) };
        BattleSituation situation = BuildSituation(actor);
        List<WeightedAction> weights = EnemySituationalAI.ApplySituationalModifiers(actor.Type, baseWeights, situation);
        float totalWeight = weights.Sum(w => w.Weight);
        float roll = Random.Range(0f, totalWeight);
        BattleActionType decided = EnemyAI.DecideAction(weights, actor.Type, roll);

        if (decided == BattleActionType.UseSkill && actor.SkillNames.Count > 0)
        {
            SkillData skill = skillList.FirstOrDefault(s => s.name == actor.SkillNames[0]);
            if (skill != null)
            {
                AnimaUnit target = PickSkillTarget(actor, skill);
                if (target != null) ApplySkill(actor, target, skill);
            }
        }
        else
        {
            AnimaUnit target = PickRandomLivingOpponent(actor);
            if (target != null) ApplyAttack(actor, target);
        }

        FinishActorTurn();
    }

    private BattleSituation BuildSituation(AnimaUnit actor)
    {
        var ownSide = actor.IsAlly ? spawner.AllyUnits : spawner.EnemyUnits;
        var otherSide = actor.IsAlly ? spawner.EnemyUnits : spawner.AllyUnits;
        var livingOwn = ownSide.Where(u => !u.Animadie).ToList();
        var livingOther = otherSide.Where(u => !u.Animadie).ToList();

        float allyLowestHpRatio = livingOwn.Count > 0 ? livingOwn.Min(u => u.Stamina / u.Maxstamina) : 1f;
        float targetLowestHpRatio = livingOther.Count > 0 ? livingOther.Min(u => u.Stamina / u.Maxstamina) : 1f;

        // selfTeamBuffed/targetDebuffed는 이번 단계에서 버프 상태 추적을 안전한 기본값(false)으로 단순화했다
        // (상황 인지형 AI의 배율 결정에만 영향, 핵심 전투 판정과는 무관) — TODO: 다음 단계에서 buffManager 조회로 대체.
        return new BattleSituation(
            allyLowestHpRatio: allyLowestHpRatio,
            selfTeamBuffed: false,
            targetLowestHpRatio: targetLowestHpRatio,
            aliveTargetCount: livingOther.Count,
            targetDebuffed: false);
    }

    private AnimaUnit PickSkillTarget(AnimaUnit actor, SkillData skill)
    {
        bool targetsSameSide = skill.Type is "SingleHeal" or "SingleShield" or "SingleBuff";
        var pool = targetsSameSide
            ? (actor.IsAlly ? spawner.AllyUnits : spawner.EnemyUnits)
            : (actor.IsAlly ? spawner.EnemyUnits : spawner.AllyUnits);
        var living = pool.Where(u => !u.Animadie).ToList();
        return living.Count > 0 ? living[Random.Range(0, living.Count)] : null;
    }

    private AnimaUnit PickRandomLivingOpponent(AnimaUnit actor)
    {
        var pool = actor.IsAlly ? spawner.EnemyUnits : spawner.AllyUnits;
        var living = pool.Where(u => !u.Animadie).ToList();
        return living.Count > 0 ? living[Random.Range(0, living.Count)] : null;
    }

    public void OnAttackButtonClicked()
    {
        if (phase != BattlePhase.AwaitingAction) return;
        pendingActionType = BattleActionType.Attack;
        pendingSkill = null;
        phase = BattlePhase.AwaitingTarget;
        SetActionButtonsInteractable(false, false);
        UpdateTurnStatusText(CurrentActor, "공격할 대상을 선택하세요");
    }

    public void OnSkillButtonClicked()
    {
        if (phase != BattlePhase.AwaitingAction) return;
        AnimaUnit actor = CurrentActor;
        if (actor.SkillNames.Count == 0) return;

        SkillData skill = skillList.FirstOrDefault(s => s.name == actor.SkillNames[0]);
        if (skill == null)
        {
            Debug.LogError($"[BattleController] 스킬 '{actor.SkillNames[0]}'을 SkillList.json에서 찾을 수 없습니다.");
            return;
        }

        pendingActionType = BattleActionType.UseSkill;
        pendingSkill = skill;
        phase = BattlePhase.AwaitingTarget;
        SetActionButtonsInteractable(false, false);
        UpdateTurnStatusText(actor, "스킬 대상을 선택하세요");
    }

    /// <summary>유닛 비주얼 클릭 시 호출됨(BattleUnitVisual.OnMouseDown). 테스트 시 eval로 직접 호출해도 된다.</summary>
    public void OnUnitClicked(AnimaUnit clicked)
    {
        if (phase != BattlePhase.AwaitingTarget || clicked.Animadie) return;

        AnimaUnit actor = CurrentActor;
        bool needsOppositeSideTarget = pendingActionType == BattleActionType.Attack
            || (pendingSkill != null && pendingSkill.Type is "SingleAttack" or "SingleDebuff");
        bool clickedIsOppositeSide = clicked.IsAlly != actor.IsAlly;
        if (needsOppositeSideTarget != clickedIsOppositeSide) return;

        if (pendingActionType == BattleActionType.Attack)
        {
            ApplyAttack(actor, clicked);
        }
        else
        {
            ApplySkill(actor, clicked, pendingSkill);
        }

        FinishActorTurn();
    }

    private void ApplyAttack(AnimaUnit attacker, AnimaUnit target)
    {
        float roll = Random.Range(0.95f, 1.11f);
        float damage = BattleMath.CalcAttackDamage(attacker.Damage, target.Defense, roll);
        DealDamage(target, damage);
    }

    private void ApplySkill(AnimaUnit actor, AnimaUnit target, SkillData skill)
    {
        float roll = Random.Range(0.95f, 1.11f);
        switch (skill.Type)
        {
            case "SingleAttack":
                DealDamage(target, BattleMath.CalcSkillDamage(actor.Damage, target.Defense, skill.Weight, roll));
                break;
            case "SingleHeal":
                Heal(target, BattleMath.CalcHealAmount(actor.Damage, target.Maxstamina, skill.Weight, roll));
                break;
            case "SingleShield":
                target.Shield += BattleMath.CalcShieldAmount(actor.Damage, skill.Weight, roll);
                break;
            case "SingleBuff":
                ApplyBuff(actor, target, skill);
                break;
            case "SingleDebuff":
                ApplyDebuff(actor, target, skill);
                break;
            default:
                Debug.LogWarning($"[BattleController] 스킬 타입 '{skill.Type}'은 아직 지원하지 않습니다(다인원 대상 스킬은 다음 단계에서 추가 예정).");
                break;
        }
    }

    private void ApplyBuff(AnimaUnit caster, AnimaUnit target, SkillData skill)
    {
        buffManager.AddOrRenewBuff(new Buff<AnimaUnit>(skill.Affect, skill.Weight, skill.Turn, target, distinct: 0));
        float ratio = BattleMath.CalcBuffRatio(caster.Damage, skill.Weight);
        foreach (string stat in skill.Affect)
        {
            if (target.TmpAbility.ContainsKey(stat)) continue;
            switch (stat)
            {
                case "strengthup":
                    target.TmpAbility[stat] = target.Damage;
                    target.Damage *= ratio;
                    break;
                case "speedup":
                    target.TmpAbility[stat] = target.Speed;
                    target.SetSpeed(target.Speed * ratio);
                    break;
                case "defenseup":
                    target.TmpAbility[stat] = target.Defense;
                    target.Defense *= ratio;
                    break;
            }
        }
    }

    private void ApplyDebuff(AnimaUnit debuffer, AnimaUnit target, SkillData skill)
    {
        buffManager.AddOrRenewBuff(new Buff<AnimaUnit>(skill.Affect, skill.Weight, skill.Turn, target, distinct: 1));
        foreach (string stat in skill.Affect)
        {
            if (target.TmpAbility.ContainsKey(stat)) continue;
            float ratio;
            switch (stat)
            {
                case "strengthdown":
                    target.TmpAbility[stat] = target.Damage;
                    ratio = BattleMath.CalcDebuffRatio(debuffer.Damage, skill.Weight);
                    target.Damage = Mathf.Max(0f, ratio);
                    break;
                case "speeddown":
                    target.TmpAbility[stat] = target.Speed;
                    ratio = BattleMath.CalcDebuffRatio(debuffer.Damage, skill.Weight);
                    target.SetSpeed(Mathf.Max(0f, ratio));
                    break;
                case "defensedown":
                    target.TmpAbility[stat] = target.Defense;
                    ratio = BattleMath.CalcDebuffRatio(debuffer.Damage, skill.Weight);
                    target.Defense = Mathf.Max(0f, ratio);
                    break;
            }
        }
    }

    private void DealDamage(AnimaUnit target, float damage)
    {
        if (target.Shield > 0f)
        {
            float absorbed = Mathf.Min(target.Shield, damage);
            target.Shield -= absorbed;
            damage -= absorbed;
        }
        target.Stamina -= damage;
        if (target.Stamina <= 0f)
        {
            target.Stamina = 0f;
            target.Animadie = true;
        }
    }

    private void Heal(AnimaUnit target, float amount)
    {
        target.Stamina = Mathf.Min(target.Stamina + amount, target.Maxstamina);
    }

    private void FinishActorTurn()
    {
        phase = BattlePhase.Resolving;
        AnimaUnit actor = CurrentActor;
        List<string> expired = buffManager.TickOne(actor);
        RestoreExpiredBuffs(actor, expired);

        if (roundQueue.Count > 0) roundQueue.RemoveAt(0);
        AdvanceToNextActor();
    }

    private static void RestoreExpiredBuffs(AnimaUnit unit, List<string> expiredTypes)
    {
        foreach (string stat in expiredTypes)
        {
            if (!unit.TmpAbility.TryGetValue(stat, out float original)) continue;
            switch (stat)
            {
                case "strengthup":
                case "strengthdown":
                    unit.Damage = original;
                    break;
                case "speedup":
                case "speeddown":
                    unit.SetSpeed(original);
                    break;
                case "defenseup":
                case "defensedown":
                    unit.Defense = original;
                    break;
            }
            unit.TmpAbility.Remove(stat);
        }
    }

    private bool CheckBattleEnd()
    {
        bool allyWiped = spawner.AllyUnits.Count > 0 && spawner.AllyUnits.All(u => u.Animadie);
        bool enemyWiped = spawner.EnemyUnits.Count > 0 && spawner.EnemyUnits.All(u => u.Animadie);
        if (!allyWiped && !enemyWiped) return false;

        phase = BattlePhase.Finished;
        state = enemyWiped ? BattleState.Win : BattleState.Defeat;
        SetActionButtonsInteractable(false, false);
        resultText.gameObject.SetActive(true);
        resultText.text = enemyWiped ? "승리!" : "패배...";
        turnStatusText.text = "";
        return true;
    }

    private void SetActionButtonsInteractable(bool attack, bool skill)
    {
        attackButton.interactable = attack;
        skillButton.interactable = skill;
    }

    private void UpdateTurnStatusText(AnimaUnit actor, string suffix = null)
    {
        string side = actor.IsAlly ? "아군" : "적";
        turnStatusText.text = suffix != null
            ? $"{actor.UnitName}({side})의 턴 — {suffix}"
            : $"{actor.UnitName}({side})의 턴";
    }
}
