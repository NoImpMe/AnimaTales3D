# CONVERSION_SPEC.md — 2D→3D 전환 사양

**상태**: 진행 중. 오버월드(좌표계·카메라·절차적 생성)는 이미 구현됨 + EditMode 테스트 보강 완료 + 생성 수치 Config 이전 완료. 전투 씬은 순수 로직 레이어(턴/상태/버프/데미지공식/적AI) 포팅 착수 — 스폰/카메라/UI 등 프레젠테이션 레이어는 아직 미착수.
(2026-09-05 기준: LOG.md #2·#3·#7·#13 작업 반영해 이 문서를 갱신함)

## 1. 전환 범위
좌표계·이동·카메라까지 포함해 재설계한다. 오버월드는 원본(고정 스테이지 배치)보다 넓은 범위로 재설계됐다 — 절차적 구역 생성은 의도한 설계로 승인됨(아래 3절 참고). 전투 씬은 아직 손대지 않았다.

## 2. 보존해야 할 기존 로직 (변경 금지, 회귀 테스트 대상)
- 턴 순서: Speed 내림차순 정렬 큐 (`TurnManager.UpdateTurnList`)
- 전투 상태 머신: `BattleState` enum(start/playerTurn/enemyTurn/win/defeat)과 전이 조건
- 버프/디버프: `Buff`/`BuffManager`의 추가·갱신·틱·제거 로직, `TickOne`이 "유닛 행동 종료 시점"에 도는 타이밍
- 데미지 계산 공식: `damage * (1000/(1000+Defense)) * Random(0.95~1.11)`, 실드 우선 소모
- 적 AI 가중 랜덤 의사결정: `EnemyActions.DecideAction`, 타입별 예외(`Irascor` 등)
- 메타 시스템 수치·확률·저장 로직: 골드/상점/조합/여관/인벤토리 (`ShopEffectHandler`, `MixManager`, `InnManager`, `AbilityManager` 등)
- 데이터 모델: `AnimaDataSO`, `SkillData`, `PlayerInfo`, BGDatabase 연동
- (참고) 오버월드 그래프 로직(`RegionController.neighbors`)은 "그대로 포팅"이 아니라 아래 3절의 `HexCoord` 좌표 기반 인접 계산으로 대체됨 — 로직은 다르지만 "인접 타일만 갈 수 있다"는 동작 자체는 유지됨.

## 3. 좌표계 설계 — ✅ 확정 (실제 구현 기준으로 갱신)
Axial(q, r) 좌표계를 새로 도입했다 (`CONVERSION_SPEC.md` 이전 버전의 (A)안 채택). `HexCoord.cs`:
- Flat-top 헥사곤, 6방향 `Directions` 배열 (우/우상/좌상/좌/좌하/우하)
- `ToWorldPosition(size)`: `x = size * 1.5 * q`, `z = size * (√3/2 * q + √3 * r)`, y는 항상 0 고정 (지형 높이는 별도 처리 예정)
- `GetNeighbors()`, `GetRange(center, radius)`, `DistanceTo()` 제공
- 원본 2D의 `RegionController.neighbors` 수동 연결 리스트는 좌표 기반 인접 계산(`GetNeighbors`)으로 대체됨

**오버월드 생성 방식 자체도 원본과 다르게 재설계됐다 (승인된 스코프 확장, 2026-09-02 확인)**: 원본 2D는 디자이너가 손으로 배치한 고정 `Stage{N}` 프리팹을 통째로 로드하는 방식이었으나, 3D판은 `DungeonGridManager`가 "구역(Zone)" 단위로 절차적 생성한다.
- 구역 = 반경 `zoneRadius`의 헥사곤 뭉치, 테마(`ZoneTheme`: 원작 6개 아니마 감정 Amare/Felix/Havet/Irascor/Lacrima/Phobia) 하나로 통일
- 구역마다 보스 타일 하나(`HexTileType.Boss`). 보스를 클리어하면 기존 구역과 좌표적으로 절대 겹치지 않는 새 구역을 인접 방향으로 자동 확장 생성 (`HandleBossCleared` / `TryFindNonOverlappingZonePlacement`)
- 타일 타입: `Start`/`Battle`/`Wall`/`Empty`/`Boss` (`HexTileType`)
- ✅ `wallChance`/`battleChance`/`hexSize`/`zoneRadius`는 `DungeonGenerationConfig`(ScriptableObject, `Assets/Configs/DungeonGenerationConfig.asset`)가 소유한다. `DungeonGridManager`는 `config` 참조 하나만 들고 값을 읽기만 한다 (LOG #3, 2026-09-03)

## 4. 카메라 설계 — ✅ 확정 (실제 구현 기준으로 갱신, 2026-09-05 LOG #7 반영)
`CameraDragController` + Orthographic 카메라로 구현됨 (`StageScene`, Camera 컴포넌트 `orthographic: 1`, `orthographic size: 5`). Player 기준 오프셋 `(0, 5, -10)`, X축 약 25° 하향 피치의 쿼터뷰 각도.
- 우클릭 드래그로 XZ 평면(Y = `planeHeight`, 기본 0)을 팬(pan) 이동 — Raycast와 Plane의 교차점 기반
- **Player가 타일 이동할 때 카메라도 같은 방향·거리만큼, 같은 시간(`PlayerToken.moveDuration`) 동안 함께 이동한다** (`PlayerToken.MoveTo` → `CameraDragController.FollowMove(delta, duration)`). 드래그 팬과는 별도 코루틴이라 공존 가능. 카메라는 Player의 Y(수직 hop 연출)는 따라가지 않고 XZ 평면 이동만 추적
- 이동 가능 범위는 `tilesRoot` 하위 모든 `Renderer`의 Bounds를 스캔해 자동 계산(+`boundsPadding`), 새 구역이 생길 때마다 `DungeonGridManager`가 `CameraDragController.Instance.RecalculateBounds()`를 호출해 갱신. 드래그 팬과 `FollowMove` 둘 다 이 범위로 클램프되므로, 아직 넓지 않은 초기 구역에서는 카메라가 Player를 완벽히 따라가지 못하고 경계에서 멈출 수 있음(의도된 동작 — 드래그 팬과 동일 정책)
- 좌클릭은 `HexTile.OnMouseDown` → `DungeonGridManager.OnTileClicked`로 별도 처리되므로 우클릭 드래그와 충돌 없음
- ⚠️ **전투 씬 카메라는 아직 포팅되지 않음**: 원본 `CameraManager.ZoomSingleOpp/ZoomMultiOpp`(Cinemachine 줌인 연출, z=-10 고정)에 대응하는 3D 버전이 없다. 오버월드 카메라와는 별개 시스템이므로, 전투 씬 작업을 시작할 때 별도로 설계할 것.

## 5. UI 배치 방식 변경 — 아직 미착수
전투 씬 자체가 포팅되지 않아 HP바/파서바 UI도 존재하지 않는다. 원래 계획대로 `Camera.WorldToScreenPoint` 기반 유닛 추적 방식으로 새로 구현할 것 (카메라가 orthographic이든 perspective든 동일하게 동작하므로 4절 결정과 무관하게 진행 가능).

## 6. 전환 작업 시 리스크
- `GameObject.Find` 문자열 참조가 광범위함(`BattleManager.cs` 47곳 등, 아직 미포팅) — 오브젝트 이름은 검증 없이 바꾸지 않는다.
- 일부 원본 2D 스크립트가 CP949 인코딩 — 포팅 시 원본 인코딩을 확인한다. (참고: git 커밋 메시지가 콘솔에서 깨져 보인 건 조회 시 인코딩 문제였고 실제 커밋 데이터는 정상이었음 — UTF-8로 강제 지정해 재확인함, 2026-09-02)
- `BattleScript/Elite/`, `StageScript`의 `SpawnStage`/`SpawnLine`/`StageNode`는 전체 주석 처리된 죽은 코드다. 현재 라이브 코드와 혼동하지 않는다.
- [해결됨, LOG #2] `HexCoord`/`DungeonGridManager`의 순수 로직은 `DungeonZonePlanner`로 분리되어 EditMode 테스트 26개로 커버됨.
- [해결됨, LOG #3] `wallChance`/`battleChance`/`hexSize`/`zoneRadius`는 `DungeonGenerationConfig` ScriptableObject로 이전됨 (테스트 4개 포함, 총 EditMode 테스트 30개).
- **전투 시스템(원본 BattleScript 35개 스크립트) 포팅 진행 중** — `DungeonGridManager.EnterBattle()`은 아직 임시로 `OnBattleWon`을 즉시 호출하는 스텁 상태.
  - [완료, LOG #13] 순수 로직 레이어: `TurnManager<TUnit>`/`BattleState`/`Buff<TUnit>`·`BuffManager<TUnit>`/`BattleMath`(데미지·회복·실드·버프 공식)/`EnemyAI`(가중 랜덤 행동 결정) — 2D 원본과 동일 수식으로 회귀 테스트 25개 통과.
  - [수정 완료, LOG #14] `BattleMath`의 아군/적 비대칭(적 스킬 데미지 weight 미적용, 적 회복량 고정 1.13배) 제거 — 사람 지시("스킬을 원래와 같이 Json으로 관리하고 모든 스킬의 weight 배율은 그 곳에서 관리한다")로 확정. `CalcAllySkillDamage`/`CalcEnemySkillDamage` → `CalcSkillDamage(attackerDamage, defenderDefense, weight, randomRoll)` 하나로, `CalcAllyHealAmount`/`CalcEnemyHealAmount` → `CalcHealAmount(healerDamage, targetMaxStamina, weight, randomRoll)` 하나로 통합. weight는 항상 `SkillData.Weight`(`Resources/Skills/SkillList.json`, 2D 원본과 동일 7개 스킬)에서 온 값. `SkillData`/`SkillDatabase`(JsonUtility 기반, Newtonsoft 미사용) 신규 추가, 회귀 테스트 5개 포함 총 58개 통과.
  - [미착수] 실제 유닛 데이터 모델(2D 원본 AnimaDataSO에 대응, BGDatabase 대신 순수 ScriptableObject/JSON으로 재도입하기로 결정됨) — `IBattleUnit` 인터페이스만 존재, 구현체 없음. `SkillData`/`SkillDatabase`는 이번 작업으로 이미 준비됨.
  - [완료, LOG #15] 나머지 5개 테마(Amare/Felix/Havet/Lacrima/Phobia) 적 AI 상황 인지형 개선 — `EnemySituationalAI.ApplySituationalModifiers`(신규, 원본에 없던 로직): 테마별로 상황에 따라 UseSkill 가중치에 배율(2.5배 부스트/0.4배 억제)을 적용한 뒤 기존 `EnemyAI.DecideAction`에 그대로 넘기는 구조. Amare=팀원 HP 50% 이하면 회복 우선, Felix=아직 버프 안 걸었으면 버프 우선, Havet=상대 HP 30% 이하(처치권)면 스킬 우선, Lacrima=생존 상대 2명 이상이면 광역 스킬 우선, Phobia=타겟 미디버프면 디버프 우선. Irascor/미지정 테마는 배율 1(무변화). 테스트 11개 추가, EditMode 69/69 통과
  - [완료, LOG #17] 턴 순서 시각화: 좌우로 회전하는 회전체 배치 계산 — `TurnOrderCarouselLayout`(신규 순수 클래스, 원본에 없던 로직). 슬롯 index가 현재 턴(currentIndex)과 몇 도 떨어져 있는지(`GetSlotAngleOffset`), 그 각도로 원형 호 위에 배치한 로컬 위치(`GetSlotLocalPosition`, 정면=−Z·오른쪽=+X), 현재 턴 유닛이 항상 정면에 오도록 회전체 전체에 적용할 Y회전각(`GetCarouselYRotation`)을 계산. 실제 회전 애니메이션·아이콘 프리팹 등 MonoBehaviour/UI 연결은 아직 미착수(유닛 데이터 모델 이식 후 진행)
  - [완료, LOG #16] 전투 구도 규칙: 최대 3:3, 최소 1:1, 좌측 아군/우측 적군 배치 — `BattleFormation`(신규 순수 클래스). 2D 원본 `AllyBattleSetting.SpawnAlly`/`EnemyBattleSetting.SpawnEnemy`의 인원수별 가로 간격 공식(3명: i*3.5-3.5, 2명: i*3.5-1.75, 1명: 0)은 그대로 재사용하고, 원본이 위/아래 행으로 나눴던 축만 좌/우 축으로 새로 매핑(사람 지시로 확정된 신규 배치). 아직 MonoBehaviour 스폰 로직에 연결되지는 않음(유닛 데이터 모델 이식 후 진행)
  - [미착수] MonoBehaviour/씬 연결: 스폰(위 구도 규칙 적용), 카메라 연출(Cinemachine 재도입 승인됨), UI(HP바/파서바 등 world-to-screen 방식 + 턴순서 회전체), 별도 BattleScene 전환(구조 결정됨)

## 7. 다음 작업 순서 (제안, 확정 아님)
1. ~~`HexCoord`/`DungeonGridManager`에 EditMode 테스트 보강 + LOG.md에 소급 기록~~ — 완료 (LOG #2)
2. ~~`wallChance`/`battleChance`/`hexSize`/`zoneRadius` → ScriptableObject Config로 이전~~ — 완료 (LOG #3)
3. 전투 씬 포팅 (방안 1: 레이어 분리 우선, LOG #4에서 확정)
   1. ~~순수 로직 레이어(턴/상태/버프/데미지공식/적AI) 포팅 + 회귀 테스트~~ — 완료 (LOG #13)
   2. 유닛 데이터 모델 이식 (`IBattleUnit` 구현체, 순수 ScriptableObject/JSON) — 다음 작업
   3. MonoBehaviour/씬 연결: 스폰, 카메라(Cinemachine), UI(world-to-screen), 별도 BattleScene 전환
