# AnimaTales2D 전체 스크립트 요약

**게임 장르**: 헥사타일 스테이지 기반 2D 턴제 전투 로그라이트 게임
**대상**: `Assets/Script` 하위 121개 스크립트 전수 분석 (2026-09 기준, 팀원별 폴더 → 기능별 폴더 재구성 및 리팩토링 완료 시점)
**목적**: 3D 전환을 검토할 때 참고할 기술 레퍼런스

---

## 1. 핵심 클래스/스크립트 목록과 역할

### AbilityScript — 전투 시작 전 능력(심볼) 선택
| 파일 | 역할 |
|---|---|
| AbilityCreator.cs | 전투 시작 시 희귀도 가중 랜덤으로 능력 3개 제시, 리롤·선택 UI 처리 |
| AbilityData.cs | 능력 데이터(id/name/description/icon/value) POCO |
| AbilityHolder.cs | 능력 카드 UI에 `AbilitySO`를 붙여두는 컴포넌트 |
| AbilityManager.cs | 선택된 능력을 누적해 골드/드롭률/실드/스탯 등 런 전체 보정치로 관리 |
| AbilityReroll.cs | 슬롯별 리롤 횟수/버튼 상태 |
| AbilitySO.cs | `AbilityData`를 감싸는 ScriptableObject |
| AbilityVibrator.cs | 미선택 카드의 진동(스케일 펄스) 연출 |

### BattleScript — 전투 핵심 로직 (35개, Elite 서브폴더 포함)
| 파일 | 역할 |
|---|---|
| BattleManager.cs | **전투 총괄 오케스트레이터** (1,650줄 이상): 스폰, UI 배선, 라운드/턴 상태머신, 입력 처리 |
| TurnManager.cs | 턴 큐 생성 및 Speed 기준 정렬 |
| BattleState.cs | 전투 5단계 상태 enum |
| Buff.cs / BuffManager.cs | 버프/디버프 인스턴스와 딕셔너리 기반 관리(추가/갱신/틱/제거) |
| StatusSync.cs | 상태창 UI를 갱신(이름/레벨/HP/버프 목록) |
| AnimaActions.cs | 아군 데미지/힐/실드/버프 연산 |
| EnemyActions.cs | 적 데미지/힐 연산 + 가중 랜덤 AI(`DecideAction`) |
| AllyBattleSetting.cs / EnemyBattleSetting.cs | 아군/적 스프라이트·HP바·정보패널을 하드코딩 좌표에 스폰, 각각 `IAllyBattleSetting`/`IEnemyBattleSetting` 구현 |
| IAllyBattleSetting.cs / IEnemyBattleSetting.cs / IBattleManager.cs | 스폰·오케스트레이션 코드가 참조하는 3개 인터페이스 |
| SingleAttack.cs | 단일 대상 공격/스킬 처리 + 승패 판정(`DefeatEnemy`/`DefeatAlly`) + 버프 틱 |
| MultipleAttack.cs | 광역 스킬 처리 (SingleAttack과 동일 구조) |
| AnimaDataSO.cs | 아니마 1체의 런타임 스탯 블록(ScriptableObject), BGDatabase에서 파생 |
| SkillData.cs | 스킬 1개 정의(name/Type/Weight/Affect/Turn), SkillList.json에서 역직렬화 |
| CameraManager.cs | Cinemachine 줌인/VFX 재생 (공격 연출) |
| HealthBar.cs/Controller, ShieldBar.cs/Controller, ParserBar.cs/Controller | HP/실드/누적 데미지 바의 모델+트윈 비주얼 |
| BattleLogManager.cs | 전투 로그 텍스트 누적 |
| ParserData.cs | 데미지 미터·전투로그 패널 토글 |
| AnimaActionUIController.cs | 플레이어 액션/스킬 패널 버튼·이미지 데이터 홀더 |
| AnimatorController.cs | 애니메이터 상태 종료 대기 헬퍼 |
| DBUpdater.cs | BGDatabase 세이브/로드 래퍼 |
| SceneManagerInBattle.cs | 전투 종료 후 씬 전환 |
| StatusButton.cs | 상태창 패널 토글 |
| `BattleScript/Elite/` (4파일) | **전체가 주석 처리된 미사용 코드** — Boss/Elite 전용 변형으로 추정되나 현재 비활성 |

### StageScript — 오버월드 스테이지 이동 (11개)
| 파일 | 역할 |
|---|---|
| RegionManager.cs | 오버월드 총괄: 맵 인스턴스화, 타일 클릭→씬 전환(전투/마을/보스), BGM, 다음 타일 활성화 |
| RegionController.cs | 개별 타일 데이터: 방문 여부, `neighbors` 리스트, type, 클릭 이벤트 |
| CameraController.cs | 우클릭 드래그로 오버월드 카메라 이동, 맵 경계 클램프 |
| StageController.cs | 필드(구역) 활성화 순서 제어, 타일 시각 상태(콜라이더/알파) 전환 |
| TilesLine.cs | Tilemap 외곽선을 따라 LineRenderer 생성 후 깜빡임 |
| LineGenerate.cs | 두 지점 간 애니메이션 경로선 이펙트 |
| IsVisitedField.cs | 필드 단위 방문/선택 상태 + 인접 필드 목록 |
| DontDesManager.cs | 씬 전환 간 유지되는 싱글톤, 튜토리얼 플래그, 오브젝트 파괴/유지 관리 |
| SpawnStage.cs / SpawnLine.cs / StageNode.cs | **전체 주석 처리, 비활성** — 절차적 분기형 스테이지 그래프 생성기(구 시스템으로 추정) |

### 기타 폴더 (68개)
| 폴더 | 요약 | 대표 파일 |
|---|---|---|
| AnimaScript (13) | 아니마 도감·인벤토리 데이터, 파티 편성용 드래그앤드롭 슬롯 UI | AnimaInventoryManager.cs(싱글톤 인벤토리 매니저), AnimaSlotUI.cs |
| CorridorScript (10) | "회랑" 씬에서 도감/레시피 열람, 진행 배지 확인 | CorridorManager.cs, RecipeUI.cs |
| GlobalScript (4) | 오디오, 화면 페이드, 플레이어 전역 상태 | AudioManager.cs, PlayerInfo.cs(ScriptableObject) |
| GoldScript (1) | 골드 재화 관리/표시 | GoldManager.cs |
| InnScript (3) | 여관에서 골드 소모해 아니마 회복/부활 | InnManager.cs |
| MixScript (3) | 두 아니마 합성(조합)으로 새 아니마 생성 | MixManager.cs |
| OptionScript (4) | 해상도/프레임레이트/볼륨 설정 및 PlayerPrefs 영구 저장 | DisplayController.cs, PreferenceData.cs |
| ShopScript (11) | 마을 상점 아이템 구성/구매/재고 | ShopManager.cs, ShopEffectHandler.cs |
| TitleScript (3) | 타이틀 메뉴 동작, 로고/버튼 연출 | TitleManager.cs |
| TutorialScript (9) | 전투/조합/스테이지/마을 씬별 대사 기반 튜토리얼 | DialogueSystem.cs(싱글톤 타이핑 대사 시스템) |
| UIScript (2) | 필드 UI 토글, 버튼 포커스 강조 | FieldUIManager.cs |
| VillageScript (5) | 마을 건물 상호작용, 마을별 지속 데이터(재고/가격) | VillageController.cs, VillageDataManager.cs |

(각 폴더 전체 파일별 한 줄 역할은 필요시 요청해주시면 표로 더 세분화해드릴게요.)

---

## 2. 헥사타일 좌표계 규칙 — ⚠️ 예상과 다른 중요한 발견

**결론부터: 이 프로젝트에는 axial/offset/cube 같은 알고리즘적 hex 좌표계가 존재하지 않습니다.** "헥사타일 스테이지"라는 이름과 달리, 실제 구현은 **좌표 계산이 아니라 아티스트가 손으로 배치한 타일맵 프리팹 + 인스펙터에서 수동으로 연결한 인접 리스트** 방식입니다.

**근거**:
- `RegionController.cs`에는 좌표 필드가 아예 없습니다. 위치는 순수히 Unity `Transform.position`(에디터에서 수동 배치)에 의존합니다.
```csharp
// RegionController.cs
public List<RegionController> neighbors;   // 좌표가 아니라 "직접 연결"만 존재
public string type;
```
- `RegionManager.StageInit()`은 좌표를 계산하지 않고, 디자이너가 미리 만들어둔 완성된 맵 프리팹을 통째로 불러옵니다.
```csharp
// RegionManager.cs
tileMap = Instantiate(Resources.Load<GameObject>($"Minwoo/TileMap/Stage{stageNum}"), new Vector3(0,0,0), ...);
```
- 타일 클릭 시 인접 타일을 활성화하는 로직만 존재하며, 6방향 hex 인접 판정 같은 기하 연산은 없습니다.
```csharp
// RegionManager.cs
public void SetNextTile(RegionController target)
{
    foreach (var nb in target.neighbors)
        nb.gameObject.SetActive(true);
}
```
- 유일하게 "그리드에 가까운" 코드는 비활성 상태인 `SpawnStage.cs`(전체 주석 처리)에 있는데, 이마저도 hex 공식이 아니라 UI `RectTransform`을 단순 균등 분할하는 절차적 분기 그래프(로그라이크의 "경로 선택" 화면과 유사한 구조)입니다.
```csharp
// (비활성) SpawnStage.cs
x_section = (rect.width / 2f) / sectionCount;
y_section = (rect.height / 2f) / sectionPerStage;
stage.GetComponent<RectTransform>().anchoredPosition = new Vector2(x_section * i + randX, y_section * j + randY);
```

**3D 전환 시 의미**: "좌표 변환 공식을 그대로 이식"할 대상이 없습니다. 이식 가능한 것은 **그래프 구조(`neighbors`, 방문 상태, 타입)** 뿐이고, 실제 타일의 기하학적 배치·좌표계는 3D 환경에 맞게 **새로 설계**해야 합니다. (원한다면 이 시점에 axial 좌표계를 새로 도입하는 게 좋은 기회일 수 있습니다.)

---

## 3. 턴제 전투 흐름

### 턴 순서 결정
고정 교대(아군→적군→아군...)가 아니라 **Speed 스탯 기준 정렬 큐**입니다. 매 라운드마다 `BattleManager.BattleStart()`가 생존한 적+아군 전체를 모아 새 `TurnManager`를 만들고 정렬합니다.
```csharp
// TurnManager.cs
public List<AnimaDataSO> UpdateTurnList()
{
    turnList.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    return turnList;
}
```
`SetState(turnList)`가 `turnList[0]`을 꺼내(`RemoveAt(0)`) 그 유닛이 아군인지 적인지에 따라 플레이어 입력 대기 또는 적 AI 실행으로 분기합니다. 버프로 Speed가 바뀌면 `TurnManager.CheckChanged()`로 재정렬이 가능하지만, 기본적으로 "한 라운드 = 한 번의 Speed 정렬 큐 순회"입니다.

### 행동력/쿨다운
**AP/스태미나 소모형 시스템이나 스킬 쿨다운은 없습니다.** 각 유닛은 턴마다 정확히 한 번(공격 또는 보유한 스킬 중 하나) 행동하고 큐에서 제거됩니다. `AnimaDataSO`에 `MaxSkill_pp`/`Skill_pp` 필드가 있지만 실제로는 어디서도 읽거나 쓰이지 않는 **죽은 필드**입니다. 스킬은 "몇 번 쓸 수 있는가"가 아니라 "그 아니마가 보유한 스킬 슬롯(최대 2개)"으로만 제한됩니다. `SkillData.Turn`이 유일하게 "지속시간" 개념을 갖는데, 이는 스킬 재사용 쿨다운이 아니라 **버프 지속 턴 수**입니다.

### 전투 상태 머신
```csharp
// BattleState.cs
public enum BattleState { start, playerTurn, enemyTurn, win, defeat }
```
- `win`은 `SingleAttack.DefeatEnemy()`에서 적이 전멸(또는 보스 처치) 시 설정
- `defeat`는 `SingleAttack.DefeatAlly()`에서 아군이 전멸 시 설정
- 명시적 상태 전이 콜백 테이블 없이 `BattleManager`/`SingleAttack`/`MultipleAttack` 여러 곳에 필드 대입이 흩어져 있는 구조입니다.

### 버프/디버프 처리
`Buff`(POCO: type 목록, weight, remainingTurns, target, distinct)를 `BuffManager`가 `Dictionary<Buff,int>`로 관리합니다.
- `AddOrRenuwBuff` — 동일 타입 버프가 있으면 지속시간 갱신, 없으면 추가
- `TickOne(target)` — 해당 유닛 턴이 끝날 때마다(라운드 종료가 아니라 **그 유닛이 행동을 마칠 때마다**) 지속시간 1 감소, 만료된 버프 제거
- 적용/제거는 modifier 스택이 아니라 **직접 스탯 재계산**: 버프 걸릴 때 원래 값을 `tmpAbility` 딕셔너리에 캐싱해두고, 만료 시 `CalcStat(level, weight, defAP)`로 기본 스탯을 처음부터 다시 계산

### 공격/스킬 공통 처리 흐름
`BattleManager`(버튼/AI 이벤트) → `SingleAttack`/`MultipleAttack`의 코루틴 →
1. `PrepareAttack`: 턴 큐에서 제거, UI 비활성화
2. `CameraManager.ZoomSingleOpp/ZoomMultiOpp`: Cinemachine 줌 + VFX 연출
3. `AnimaActions/EnemyActions`의 `Attack/Skill`: 데미지 계산(`damage * (1000/(1000+Defense)) * Random(0.95~1.11)`), 실드 우선 소모 후 실제 스탯 반영
4. 로그·데미지 미터 갱신
5. 사망 체크 → `DefeatEnemy/DefeatAlly` (승패 판정 포함)
6. `BuffUpdate` — 그 유닛의 버프 틱
7. 큐가 비었으면 `BattleStart()`(새 라운드), 아니면 `SetState(turnList)`(다음 유닛)

적 AI(`EnemyActions.DecideAction`)는 `actionWeights`(기본 공격:스킬 = 1:1) 기반 가중 랜덤이며, `type == "Irascor"` 같은 특정 타입은 강제로 공격만 하도록 예외 처리되어 있습니다.

---

## 4. 3D 전환 시 유지 vs 자유롭게 바꿔도 되는 부분

### ✅ 그대로 유지 가능 (차원 무관 로직/데이터)
- **턴 순서/전투 상태머신**: `TurnManager`의 Speed 정렬, `BattleState` enum과 전이 조건
- **버프/디버프 시스템**: `Buff`, `BuffManager`의 추가/갱신/틱/제거 로직
- **데미지·스탯 계산 공식**: `AnimaActions`/`EnemyActions`의 `CalcAttackDamage`/`CalcSkillDamage` 등
- **적 AI 의사결정**: `EnemyActions.DecideAction`의 가중 랜덤 로직
- **데이터 모델**: `AnimaDataSO`, `SkillData`, `PlayerInfo`(ScriptableObject), BGDatabase 연동 (`DBUpdater`, `Anima.cs`, `RecipeEntry.cs`)
- **오버월드 그래프 로직**: `RegionController.neighbors` 기반 방문/활성화 상태 전이 (좌표계 자체는 새로 설계해야 하지만, "그래프를 따라 다음 노드를 연다"는 로직 자체는 그대로 재사용 가능)
- **메타 시스템**: 골드/상점/조합/여관/인벤토리의 로직 부분(수치 계산, 확률, 데이터 저장) — `ShopEffectHandler`, `MixManager`, `InnManager`, `AbilityManager` 등
- **인터페이스 계약**: `IBattleManager`/`IAllyBattleSetting`/`IEnemyBattleSetting` (단, 아래 주의사항 참고)

### ⚠️ 반드시 다시 설계해야 하는 부분 (2D 프레젠테이션에 강결합)
- **좌표→월드 변환 공식 자체가 없음** — 위 2번 항목대로, 아트에 좌표가 박혀 있어 3D 그리드는 이식이 아니라 **신규 설계** 필요
- **`Tilemap`/`TilemapCollider2D`/`Physics2D.Raycast`**: `RegionManager`, `StageController`, `TilesLine`에 광범위하게 결합된 2D 전용 API
- **orthographic 카메라 가정**: `CameraController`의 `SpriteRenderer.bounds`/`orthographicSize` 기반 클램프
- **고정 z축 카메라 프레이밍**: `CameraManager.ZoomSingleOpp/ZoomMultiOpp`가 `cameraposz = -10f` 고정값으로 Cinemachine 카메라를 유닛과 같은 평면에 놓는 구조 — 임의 각도의 3D 카메라 연출로 바꾸려면 재설계 필요
- **유닛 스폰 좌표 하드코딩**: `AllyBattleSetting`/`EnemyBattleSetting`이 `Vector3(x, y, 0f)`(z=0 고정) + `Rotate(0,180f,0)`로 좌우 반전하는 2D 관례를 사용
- **HP바/파서바 등 UI 배치**: 유닛의 실제 3D 위치가 아니라 "몇 번째 유닛인가"라는 인덱스 공식(`(i * 380f) - 380f` 등)으로 화면 좌표를 계산 — 3D 카메라가 움직이면 유닛과 UI가 어긋남. world-to-screen 방식(예: `Camera.WorldToScreenPoint` + 유닛 추적)으로 바꿔야 함
- **스프라이트 전용 연출**: `OutlineHighlight.cs`(2D 아웃라인 머티리얼), `SkyboxRotator.cs`(스카이박스 회전 연출) 등은 3D 환경에서 다른 방식으로 대체 필요

---

## 5. 스크립트 간 의존관계 / 주의할 점

- **`GameObject.Find` 문자열 참조가 매우 많음**: `BattleManager.cs` 한 파일에서만 47곳, `EnemyBattleSetting.cs` 12곳 등. `$"Ally{i}"`, `$"EnemyAnimaHP{i}"`, `"Game Manager"`, `"BattleManager"` 같은 이름으로 씬 하이어라키를 찾습니다. **오브젝트 이름을 바꾸면 조용히(컴파일 에러 없이) 런타임에서 깨지는** 가장 취약한 지점이므로, 3D 전환 작업 중 오브젝트 이름은 절대 바꾸지 않거나, 이 기회에 참조 방식을 `[SerializeField]` 직접 연결이나 싱글톤 프로퍼티로 교체하는 걸 권장합니다.
- **인터페이스가 부분적으로만 깨끗함**: `IAllyBattleSetting`/`IEnemyBattleSetting`이 `BattleManager BattleManager { get; }`처럼 구현체 타입을 그대로 반환하는 멤버가 있어, 완전한 의존성 역전(DI/모킹)은 인터페이스 자체도 손봐야 가능합니다.
- **`BattleManager`가 God Object**: 1,650줄 이상으로 스폰/UI 배선/입력 처리(`Update()`에서 `Input.GetKeyUp` 직접 폴링)/상태 전이를 전부 담당합니다. 턴 로직을 건드리는 3D 전환 작업은 필연적으로 같은 파일 안의 카메라/UI 코드도 함께 건드리게 됩니다.
- **`BattleScript/Elite/`는 죽은 코드**: 4개 파일 전체가 주석 처리되어 있고, 여기도 동일한 `GameObject.Find` 패턴이 남아있어 향후 되살릴 경우 같은 취약점을 물려받습니다.
- **한글 인코딩(CP949) 주의**: 프로젝트 내 일부 파일이 UTF-8이 아닌 CP949로 저장되어 있습니다. 앞으로 스크립트를 수정할 때(특히 Claude나 다른 도구로 자동 편집 시) 원본 인코딩을 확인하지 않고 UTF-8로 재저장하면 한글이 깨질 수 있습니다.
- **`SpawnStage.cs`/`SpawnLine.cs`/`StageNode.cs`(StageScript)와 `BattleScript/Elite/`는 모두 비활성 죽은 코드**입니다. 3D 전환 계획을 세울 때 "현재 라이브 코드"와 "주석 처리된 구버전 시스템"을 혼동하지 않도록 주의하세요.
