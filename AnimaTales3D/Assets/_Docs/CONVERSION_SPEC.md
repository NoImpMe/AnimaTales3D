# CONVERSION_SPEC.md — 2D→3D 전환 사양

**상태**: 진행 중. 오버월드(좌표계·카메라·절차적 생성)는 이미 구현됨. 전투 씬은 아직 미착수.
(2026-09-02 기준: 실제 `AnimaTales3D` 프로젝트의 `HexCoord.cs`/`DungeonGridManager.cs`/`CameraDragController.cs` 코드를 확인하고 이 문서를 갱신함)

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
- 구역마다 `타일을 하나(`HexTileType.Boss`). 보스를 클리어하면 기존 구역과 좌표적으로 절대 겹치지 않는 새 구역을 인접 방향으로 자동 확장 생성 (`HandleBossCleared` / `TryFindNonOverlappingZonePlacement`)
- 타일 타입: `Start`/`Battle`/`Wall`/`Empty`/`Boss` (`HexTileType`)
- ⚠️ `wallChance`/`battleChance`/`hexSize`/`zoneRadius`가 `DungeonGridManager`의 `[SerializeField]`로 직접 노출돼 있음 — 지침 "모든 수치는 ScriptableObject Config가 소유, MonoBehaviour에 숫자 리터럴 금지" 위반 상태. ScriptableObject Config로 옮기는 작업이 필요함 (6절 리스크 참고)

## 4. 카메라 설계 — ✅ 확정 (실제 구현 기준으로 갱신)
`CameraDragController` + Orthographic 카메라로 구현됨 (`StageScene`, Camera 컴포넌트 `orthographic: 1`, `orthographic size: 5`).
- 우클릭 드래그로 XZ 평면(Y = `planeHeight`, 기본 0)을 팬(pan) 이동 — Raycast와 Plane의 교차점 기반
- 이동 가능 범위는 `tilesRoot` 하위 모든 `Renderer`의 Bounds를 스캔해 자동 계산(+`boundsPadding`), 새 구역이 생길 때마다 `DungeonGridManager`가 `CameraDragController.Instance.RecalculateBounds()`를 호출해 갱신
- 좌클릭은 `HexTile.OnMouseDown` → `DungeonGridManager.OnTileClicked`로 별도 처리되므로 우클릭 드래그와 충돌 없음
- ⚠️ **전투 씬 카메라는 아직 포팅되지 않음**: 원본 `CameraManager.ZoomSingleOpp/ZoomMultiOpp`(Cinemachine 줌인 연출, z=-10 고정)에 대응하는 3D 버전이 없다. 오버월드 카메라와는 별개 시스템이므로, 전투 씬 작업을 시작할 때 별도로 설계할 것.

## 5. UI 배치 방식 변경 — 아직 미착수
전투 씬 자체가 포팅되지 않아 HP바/파서바 UI도 존재하지 않는다. 원래 계획대로 `Camera.WorldToScreenPoint` 기반 유닛 추적 방식으로 새로 구현할 것 (카메라가 orthographic이든 perspective든 동일하게 동작하므로 4절 결정과 무관하게 진행 가능).

## 6. 전환 작업 시 리스크
- `GameObject.Find` 문자열 참조가 광범위함(`BattleManager.cs` 47곳 등, 아직 미포팅) — 오브젝트 이름은 검증 없이 바꾸지 않는다.
- 일부 원본 2D 스크립트가 CP949 인코딩 — 포팅 시 원본 인코딩을 확인한다. (참고: git 커밋 메시지가 콘솔에서 깨져 보인 건 조회 시 인코딩 문제였고 실제 커밋 데이터는 정상이었음 — UTF-8로 강제 지정해 재확인함, 2026-09-02)
- `BattleScript/Elite/`, `StageScript`의 `SpawnStage`/`SpawnLine`/`StageNode`는 전체 주석 처리된 죽은 코드다. 현재 라이브 코드와 혼동하지 않는다.
- **[신규] `HexCoord`/`DungeonGridManager`에 EditMode 테스트가 하나도 없음** — 지침의 "순수 로직 + EditMode 테스트" 규칙이 지켜지지 않은 상태로 이미 구현·커밋됨. 다음 작업으로 보강 필요.
- **[신규] `wallChance`/`battleChance`/`hexSize`/`zoneRadius`가 ScriptableObject Config가 아니라 MonoBehaviour 필드에 하드코딩돼 있음** — 지침 위반. Config로 이전 필요.
- **[신규] 전투 시스템(원본 BattleScript 35개 스크립트)이 하나도 포팅되지 않음** — `DungeonGridManager.EnterBattle()`은 임시로 `OnBattleWon`을 즉시 호출하는 스텁 상태.

## 7. 다음 작업 순서 (제안, 확정 아님)
1. `HexCoord`/`DungeonGridManager`에 EditMode 테스트 보강 + LOG.md에 소급 기록 (안전망 확보)
2. `wallChance`/`battleChance`/`hexSize`/`zoneRadius` → ScriptableObject Config로 이전
3. 전투 씬 포팅 착수 (BattleManager 등, 카메라·UI 설계 포함)
