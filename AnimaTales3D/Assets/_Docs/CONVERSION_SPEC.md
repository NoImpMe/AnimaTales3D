# CONVERSION_SPEC.md — 2D→3D 전환 사양

**상태: 초안.** 3번(좌표계)과 4번(카메라)은 아직 방식이 확정되지 않았다. 실제 [전환] 작업을 시작하기 전에 [설계] 명령으로 방안을 정하고 이 문서를 갱신할 것. (근거 자료: `LEGACY_2D_SCRIPT_SUMMARY.md`)

## 1. 전환 범위
좌표계·이동·카메라까지 포함해 재설계한다. 시각적 표현(모델/조명)만 바꾸는 것이 아니라, 오버월드 배치 방식과 전투 카메라 연출도 3D 공간에 맞게 다시 설계 대상이다.

## 2. 보존해야 할 기존 로직 (변경 금지, 회귀 테스트 대상)
- 턴 순서: Speed 내림차순 정렬 큐 (`TurnManager.UpdateTurnList`)
- 전투 상태 머신: `BattleState` enum(start/playerTurn/enemyTurn/win/defeat)과 전이 조건
- 버프/디버프: `Buff`/`BuffManager`의 추가·갱신·틱·제거 로직, `TickOne`이 "유닛 행동 종료 시점"에 도는 타이밍
- 데미지 계산 공식: `damage * (1000/(1000+Defense)) * Random(0.95~1.11)`, 실드 우선 소모
- 적 AI 가중 랜덤 의사결정: `EnemyActions.DecideAction`, 타입별 예외(`Irascor` 등)
- 오버월드 그래프 로직: `RegionController.neighbors` 기반 방문/활성화 상태 전이
- 메타 시스템 수치·확률·저장 로직: 골드/상점/조합/여관/인벤토리 (`ShopEffectHandler`, `MixManager`, `InnManager`, `AbilityManager` 등)
- 데이터 모델: `AnimaDataSO`, `SkillData`, `PlayerInfo`, BGDatabase 연동

## 3. 좌표계 설계 — ⚠️ 미확정
기존 오버월드에는 axial/offset/cube 같은 알고리즘적 hex 좌표계가 **존재하지 않았다**. `RegionController`는 좌표 필드 없이 `Transform.position`(에디터 수동 배치) + `neighbors` 리스트로만 동작했다 (`LEGACY_2D_SCRIPT_SUMMARY.md` 2절).

선택지:
- (A) 이 기회에 axial 좌표계를 신규 도입하고, 3D 월드 좌표로 변환하는 공식을 새로 정의한다. `neighbors` 그래프는 좌표 기반 인접 판정으로 대체하거나 검증용으로 병행한다.
- (B) 좌표계는 도입하지 않고, `neighbors` 그래프 구조만 유지한 채 3D 배치도 기존처럼 수동(Transform) 배치를 유지한다.

→ 결정 후 이 절을 확정 사양으로 교체할 것.

## 4. 카메라 설계 — ⚠️ 미확정
기존: `orthographic` 고정, z축 -10 고정(`CameraManager.ZoomSingleOpp/ZoomMultiOpp`), `CameraController`는 `SpriteRenderer.bounds` 기반으로 이동 범위를 클램프.

3D 전환 시 선택지:
- 고정 각도 아이소메트릭(또는 쿼터뷰) 카메라 유지
- 자유 회전·줌이 가능한 3D 카메라로 전환

→ 결정 후 이 절을 확정 사양으로 교체할 것.

## 5. UI 배치 방식 변경 (확정 — 유지 불가능한 부분)
기존 HP바/파서바 등은 "몇 번째 유닛인가"라는 인덱스 공식(`(i * 380f) - 380f` 등)으로 화면 좌표를 계산한다. 3D 카메라가 움직이면 유닛과 UI가 어긋나므로, `Camera.WorldToScreenPoint` 기반으로 유닛을 실시간 추적하는 방식으로 교체한다.

## 6. 전환 작업 시 리스크 (지침 "절대 하지 않는다" 참고)
- `GameObject.Find` 문자열 참조가 광범위함(`BattleManager.cs` 47곳 등) — 오브젝트 이름은 검증 없이 바꾸지 않는다.
- 일부 스크립트가 CP949 인코딩 — 수정 전 원본 인코딩을 확인한다.
- `BattleScript/Elite/`, `StageScript`의 `SpawnStage`/`SpawnLine`/`StageNode`는 전체 주석 처리된 죽은 코드다. 현재 라이브 코드와 혼동하지 않는다.
