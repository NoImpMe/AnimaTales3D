# LOG.md — 작업 로그

각 항목은 지침의 아래 템플릿을 그대로 따른다. 번호는 1부터 누적 증가. 명령 1개 = 커밋 1개 = LOG 항목 1개.

```
## [동사] 한 줄 목표 — YYYY-MM-DD HH:MM
### 프롬프트
### 조작 내역
### 검증
### 실패와 수정
```

---

## [수정] CONVERSION_SPEC.md를 실제 구현 기준으로 갱신 — 2026-09-02 23:24
### 프롬프트
"2D 스크립트 요약본을 보고 3D로 변경하는 작업을 하나씩 실시해줘" → 실제 프로젝트 상태를 먼저 조사해보니 CONVERSION_SPEC.md의 좌표계·카메라 "미확정" 항목이 이미 코드로 구현돼 있었음. 사용자 확인 후 CONVERSION_SPEC.md부터 실제 코드 기준으로 갱신하기로 결정.
### 조작 내역
- `execute_code`로 `AnimaTales3D` 프로젝트(`Assets` 경로: `C:/Users/Minwoo/Desktop/Anima3D/AnimaTales3D/Assets`) 구조 조사: `Scripts/StageScripts/`에 `HexCoord.cs`, `DungeonGridManager.cs`, `HexTile.cs`, `HexTileType.cs`, `Zone.cs`, `ZoneTheme.cs`, `ZoneThemeUtility.cs`, `PlayerToken.cs`, `Scripts/CameraDragController.cs` 이미 존재·커밋됨을 확인
- `StageScene.unity`의 Camera 컴포넌트 확인: `orthographic: 1`, `orthographic size: 5`
- `git log --oneline`으로 기존 커밋 이력 확인 (Initial commit → AnimaTales 3D 개발 작업 Init Commit → StageScene 자동 생성 로직 수정 → Docs 추가)
- 체크포인트 커밋 실행 (`_Docs/*.meta` 5개 파일)
- `Assets/_Docs/CONVERSION_SPEC.md`를 실제 코드 기준으로 재작성 (3절 좌표계, 4절 카메라를 "미확정"에서 "확정"으로, 5·6·7절 추가)
### 검증
- `git status --short`로 `CONVERSION_SPEC.md`만 변경됐음을 확인 (코드 변경 없음, 컴파일/EditMode 테스트 대상 아님)
- 파일 재읽기로 한글 인코딩(UTF-8) 정상 기록 확인
### 실패와 수정
- git 커밋 메시지가 콘솔에서 깨져 보였던 것은 `Process` 표준출력 인코딩을 UTF-8로 지정하지 않아서였음(실제 커밋 데이터 손상 아님) → 이후 모든 git 명령은 `StandardOutputEncoding = UTF8`로 실행

## [수정] DungeonGridManager 좌표/배치 로직을 순수 클래스로 분리 + EditMode 테스트 26개 추가 — 2026-09-02 23:50
### 프롬프트
"하나씩 항목으로 넘어가자" → CONVERSION_SPEC.md 7절 로드맵 1번: HexCoord/DungeonGridManager에 EditMode 테스트가 없던 것을 보강.
### 조작 내역
- `Assets/Scripts/StageScripts/DungeonZonePlanner.cs` 신규 생성: `DungeonGridManager`에 있던 순수 계산 로직(`PickBossDirection`, `DecideTileType`, `ZoneRangeOverlaps`, `FindDirectionIndex`, `ShuffleInPlace`, `TryFindNonOverlappingZonePlacement`)을 MonoBehaviour 밖으로 그대로 이동. 알고리즘은 원본과 동일, `Dictionary<HexCoord,HexTile>` 대신 `ICollection<HexCoord>`(점유 좌표)를 받도록만 시그니처 조정
- `Assets/Scripts/StageScripts/DungeonGridManager.cs` 수정: 위 6개 메서드 제거, `DungeonZonePlanner planner` 필드 추가(Awake에서 초기화), `GenerateZoneAt`/`HandleBossCleared`가 `planner`를 호출하도록 변경. `[SerializeField]` 필드/이름은 전혀 건드리지 않아 씬 직렬화(스크립트 GUID `f35995edb1cee4047a7571fbd5cff04d`, 인스펙터 값 hexSize=1.2/zoneRadius=3/wallChance=0.25/battleChance=0.45) 그대로 유지됨을 확인
- `Assets/Scripts/AnimaTales3D.Runtime.asmdef` 신규 생성: `Assets/Scripts/` 하위(StageScripts 포함)를 별도 어셈블리로 분리 — 기존엔 암묵적 Assembly-CSharp라 테스트 어셈블리가 참조 불가능했음(컴파일 순서상 asmdef가 Assembly-CSharp보다 먼저 컴파일되어 참조 불가)
- `Assets/Tests/EditMode/AnimaTales3D.EditModeTests.asmdef` 신규 생성 (references: UnityEngine.TestRunner, UnityEditor.TestRunner, AnimaTales3D.Runtime / precompiledReferences: nunit.framework.dll)
- `Assets/Tests/EditMode/HexCoordTests.cs` 신규 생성: Directions/GetNeighbors/GetRange(육각형 디스크 타일 수 공식 3r²+3r+1 검증)/DistanceTo/ToWorldPosition/연산자/동등성 테스트 15개
- `Assets/Tests/EditMode/DungeonZonePlannerTests.cs` 신규 생성: PickBossDirection(진입 방향 회피, 시드 50개 반복 검증)/DecideTileType(Boss·Empty·Start 우선순위)/ZoneRangeOverlaps/FindDirectionIndex/TryFindNonOverlappingZonePlacement(성공 케이스 + 완전 포위 시 실패 케이스) 테스트 11개
- `refresh_unity(compile=request, mode=force)` → 1차 시도에서 CS0246 에러 10건 (테스트 어셈블리가 Assembly-CSharp을 참조할 수 없음) → `AnimaTales3D.Runtime.asmdef` 추가로 해결 → 재컴파일
- `manage_scene(action=save)`로 StageScene 명시적 저장
### 검증
- `read_console(types=[error,warning])` → 0건 (에러/경고 없음)
- `run_tests(mode=EditMode, assembly_names=[AnimaTales3D.EditModeTests])` → 26/26 통과 (0 실패, 0.49초)
- `git diff --numstat` → `DungeonGridManager.cs`만 실질 변경(15 추가/150 삭제), `.meta`/씬/ProjectSettings 파일들은 줄바꿈(CRLF) 표시만 다르고 실제 diff 0줄임을 확인 후 커밋 대상에서 제외
- 씬 파일의 `m_Script` guid가 리팩터링 전후 동일함을 직접 대조 확인 (delete→create 과정에서 `.meta`가 보존됨)
### 실패와 수정
- 최초 테스트 어셈블리가 `Assembly-CSharp`을 `references`에 문자열로 넣었으나 CS0246 에러 10건 발생 → Unity의 암묵적 Assembly-CSharp은 커스텀 asmdef보다 나중에 컴파일되므로 asmdef에서 참조 불가 → 게임 스크립트 쪽에 `AnimaTales3D.Runtime.asmdef`를 새로 만들어 명시적 어셈블리로 분리하고, 테스트 어셈블리가 그걸 참조하도록 수정해 해결
- `manage_script`의 `create` 액션은 기존 파일이 있으면 실패("Use 'update' action")하지만 `update` 액션은 실제로 노출돼 있지 않음(enum: create/read/delete) → `delete` 후 `create`로 재생성. `.meta`(스크립트 GUID)는 delete 과정에서 지워지지 않고 그대로 남아있어 씬 참조 안전함을 확인함 — 이 패턴을 FAIL.md에 남겨둠
