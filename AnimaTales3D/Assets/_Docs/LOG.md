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

## [수정] wallChance/battleChance/hexSize/zoneRadius를 ScriptableObject Config로 이전 — 2026-09-03 00:20
### 프롬프트
"이어서 진행하도록" → CONVERSION_SPEC.md 7절 로드맵 2번: DungeonGridManager의 [SerializeField] 숫자 필드(hexSize/zoneRadius/wallChance/battleChance)가 ScriptableObject Config가 아니라 MonoBehaviour에 직접 있던 지침 위반 상태를 해소.
### 조작 내역
- `Assets/Scripts/StageScripts/DungeonGenerationConfig.cs` 신규 생성: `ScriptableObject` 서브클래스, `hexSize`/`zoneRadius`/`wallChance`/`battleChance` 4개 필드 소유. 기본값은 리팩터링 전 씬 인스펙터의 실제 오버라이드 값(1.2/3/0.25/0.45)과 동일하게 설정해 생성 결과가 달라지지 않도록 함
- `Assets/Scripts/StageScripts/DungeonGridManager.cs` 수정: 4개 `[SerializeField]` 숫자 필드 제거, `[SerializeField] private DungeonGenerationConfig config` 하나로 대체. `Awake`/`Start`/`GenerateZoneAt`/`SpawnTile`에서 `hexSize`/`zoneRadius`/`wallChance`/`battleChance` 참조를 전부 `config.*`로 교체. `config == null`일 때 `Debug.LogError` 후 조기 반환하는 방어 코드 추가(신규 동작이지만 안전을 위한 최소 추가)
- `Assets/Tests/EditMode/DungeonGenerationConfigTests.cs` 신규 생성: Config 기본값이 이전 씬 오버라이드 값과 일치하는지, hexSize>0/zoneRadius>=1/wallChance·battleChance가 [0,1] 범위이고 합이 1을 넘지 않는지 검증하는 테스트 4개
- `manage_scriptable_object(create)`로 `Assets/Configs/DungeonGenerationConfig.asset` 에셋 인스턴스 생성 후 값이 1.2/3/0.25/0.45인지 `execute_code`로 직접 로드해 재확인
- `manage_components(set_property)`로 씬의 `DungeonGridManager` 컴포넌트(instanceID 91726)의 `config` 필드에 위 에셋을 연결, `SerializedObject`로 참조가 정상 연결됐는지 재확인
- `refresh_unity(compile=request, mode=force)` 후 컴파일 완료 대기
- `manage_scene(action=save)`로 StageScene 명시적 저장
### 검증
- `read_console(types=[error,warning])` → 스크립트 수정 직후 0건, 씬 저장 후 재확인 0건
- `run_tests(mode=EditMode, assembly_names=[AnimaTales3D.EditModeTests])` → 30/30 통과 (기존 26개 + 신규 Config 테스트 4개, 0.47초)
- `git diff -- Assets/Scenes/StageScene.unity` → 씬 diff가 정확히 `hexSize/zoneRadius/wallChance/battleChance` 4줄 제거 + `config` 참조 1줄 추가뿐임을 확인 (hexTilePrefab/playerToken 등 다른 참조는 그대로)
- `git diff --numstat` → `DungeonGridManager.cs`(14+/9-), `StageScene.unity`(1+/4-)만 실질 변경. 체크포인트 커밋에서 `Settings/`·`ProjectSettings/`의 줄바꿈(CRLF) 표시 노이즈가 이미 정리돼 이번엔 해당 파일들이 아예 status에 나타나지 않음
### 실패와 수정
- 없음. (참고: 검증용 `execute_code`에서 `EditorUtility.InstanceIDToObject`가 이 Unity 버전에서 obsolete로 컴파일 에러 처리되어 `EditorUtility.EntityIdToObject`로 교체함 — 게임 코드가 아닌 조사용 스크립트 한정 이슈라 FAIL.md에는 기록하지 않음)

## [설계] 전투 씬 포팅 방안 — 2026-09-03 01:00
### 프롬프트
"전투씬 포팅계획 논의하자" → CONVERSION_SPEC.md 7절 로드맵 3번(전투 씬 포팅) 착수 전, 작업 순서/구조에 대한 방안을 논의.
### 조작 내역 (제시한 방안 요약)
- 조사: `manage_packages(list_packages)`로 Cinemachine/BGDatabase/DOTween 등이 AnimaTales3D 프로젝트에 설치돼 있지 않음을 확인(48개 패키지 중 대부분 Unity 기본 모듈). `activeInputHandler: 2`(Both)로 레거시 Input과 신규 Input System이 둘 다 동작 가능함도 확인. Assets 전체에 Battle 관련 스크립트/씬이 아직 하나도 없음(StageScene/TitleScene만 존재)을 확인
- 방안 1(레이어 분리 우선, 추천): TurnManager/BattleState/Buff·BuffManager/데미지 계산/EnemyActions AI 등 순수 로직을 MonoBehaviour 밖 C#으로 먼저 포팅 + 2D 원본과 동일 시드에서 동일 결과가 나오는지 비교하는 회귀 테스트를 붙인 뒤, 스폰/카메라/UI 같은 프레젠테이션 레이어를 이후 커밋들로 순차 구현. HexCoord/DungeonZonePlanner 때 이미 쓴 패턴과 일치, 지침의 [전환] 회귀 검증 요구와도 부합
- 방안 2(수직 슬라이스 우선): 아군 1 vs 적 1, 기본 공격만 되는 최소 전투 루프를 로직+스폰+카메라+UI까지 세로로 관통해 빠르게 프로토타입 확인 후 스킬/버프/AI/다중 유닛을 점증 추가. 가시적 피드백은 빠르나 초기 코드가 지침의 "순수 로직 먼저" 원칙과 부분적으로 충돌할 여지 있어, 데미지 계산 등 핵심 계산만이라도 pure 클래스로 먼저 뽑고 시작하는 완화책 필요
- 방안 3(God Object 그대로 이식 후 리팩터): `BattleManager`(1,650줄+)를 구조 그대로 우선 이식해 빠르게 동작시키고 이후 별도 [수정] 작업으로 점진적 분리. 이식 속도는 가장 빠르지만, 기존 로드맵 1·2번에서 이미 겪은 "지침 위반 상태로 먼저 구현·커밋 후 나중에 보강" 패턴이 반복될 위험
- 별도 결정 필요 항목(패키지 추가는 사람만 가능하므로 승인 필요): (a) 카메라 연출 - Cinemachine 재도입 여부(승인 필요) vs 직접 코루틴/Lerp 구현(패키지 불필요, 기본값 제안) (b) 데이터 모델 - BGDatabase 재도입 여부(승인 필요) vs 순수 ScriptableObject/JSON으로 대체(기본값 제안) (c) 씬 구조 - 별도 BattleScene 전환 vs 같은 씬 내 전환 (d) `GameObject.Find` 47곳+ - 포팅 시 `[SerializeField]`/싱글톤 참조로 교체할지 여부
### 검증
해당 없음 ([설계]는 실행하지 않음)
### 실패와 수정
없음
