# NETBREAK 인수인계

기준일: 2026-09-15. 현재 마일스톤: **STEP 10D Dynamic Hotbar 구현·검증 완료**. 목표 설계는 `Docs/NETBREAK_DESIGN.md`, 장기 개발 순서는 `Docs/NETBREAK_ROADMAP.md`, 작업 규칙은 `AGENTS.md`를 읽는다. 구현의 기준은 Git이며 현재 개발 상태의 기준은 이 문서다.

## Git·환경
- 저장소: `C:\game_dev\unity\NetBreak`, 브랜치 `vertical-slice`.
- origin: `https://github.com/jeonjiwon1/NetBreak.git`.
- STEP 10D 작업 전 HEAD: `6e744dada4dc8a21e620fc6075a63e09857e0fab`.
- 기존 사용자 변경: `Assets/UI/Fonts/NanumGothic-Bold SDF.asset`, `NanumGothic-ExtraBold SDF.asset`, `NanumGothic-Regular SDF.asset`. 보존하며 이번 문서 커밋에 자동 포함하지 않는다.
- Unity `6000.3.11f1`, URP `17.3.0`, Input System `1.19.0`, Test Framework `1.6.0`. `activeInputHandler: 1`.
- 추적 파일 242개. 주요 구조: Assets/Scripts/{Core,Fish,Gear,UI}, Assets/Editor, Assets/Scenes, Assets/Prefabs/{Fish,Gear}, Assets/UI/Fonts, Packages, ProjectSettings, Docs. Library/Logs/UserSettings는 로컬 Unity 산출물이다.

## 코드에서 확인한 기존 시스템과 연결
실행 검증 완료를 뜻하지 않는 정적 조사 결과다.

| 영역 | 현재 구현 |
|---|---|
| Run | `RunManager`: Gold, 포획 수/가치, EXP/Level. `RegisterFishCaptured` → `CheckLevelUp` → 증강 `ShowChoices`; 선택 종료 → `ResolveLevelUp`로 누적 레벨업 재확인 |
| 흐름 | `PrototypeGameFlowManager`: 준비/시작, 보스 진입/결과. `StartFishing` → Spawner. `CompleteBossEncounter(bool)`가 성공 여부와 정지 처리. 재시작은 현재 Scene buildIndex 재로드 |
| 물고기 | FishData ScriptableObject, FishController Resistance/포획 이벤트, FishMovement 경로·미끼·그물 영향, FishRoute 제작 경로/프리뷰. 레거시 이동 fallback도 남음 |
| 스폰 | `FishSpawner`: 풀링 및 Coast 초반→첫 대어군→성장→특수→미니보스→러시→최종 시퀀스, 보스/미니보스 테스트 모드. 첫 대어군 구간에서 전직 요청 |
| 특수/보스 | 복어의 그물 방해, SquidController 먹물, MiniBossController 돌진/보상, BossBehaviorController 행동. BossEncounterController가 포획/도착 이벤트를 받아 3회 회유·회복·지원 어군·결과 연결 |
| 도구 | LandingNetController, BaitController, CastNetController, NetPlacementController/NetController, FishingRodPlacementController/FishingRodController. Ctrl+LMB 재배치는 GearRepositionController |
| 증강/직업 | PrototypeAugmentManager가 코드 내 후보 풀·3택1·Gold 리롤·카테고리 가중치·Unique HashSet을 관리. Unique: CastNetFullHaul/FishingRodExtraHook/LandingNetChainCapture. PrototypeJobManager가 4종 전직 효과를 컨트롤러에 직접 적용 |
| UI | PrototypeHUDCanvas는 HUD/시작/공지, PrototypeSelectionCanvas는 증강/직업 선택, PrototypeHUD는 월드 피드백/결과/재시작. BossHUD/MiniBossHUD는 전용 TMP UI |

## 설계 충돌·미구현·주의점
1. `BaitController.HandleInput` Q, `NetPlacementController.HandleModeInput` W, `CastNetController.HandleCastNetInput` E 누름/뗌, `FishingRodPlacementController.HandleModeInput` R로 분산 입력된다. 단순 키 치환만으로 범용 슬롯 구조가 되지 않는다. 배치/재배치 static 상태와 취소/선택 중 입력 차단도 함께 추적해야 한다.
2. Run 도구 소유/슬롯, 도구 획득, Meta 도구 해금 및 다중 지역 진행 모델을 현재 Scripts에서 찾지 못했다. 컨트롤러가 Scene에 존재하는 것과 도구를 소유하는 것을 구분해야 한다.
3. 증강은 첫 레벨업부터 즉시 표시되고 후보 풀에는 소유 도구 필터가 없다. 직업 가중치도 소유 여부와 별개다. General enum은 있지만 현재 기본 풀에 General 후보는 없다. 향후 필터 적용 시 3개 미만 후보/리롤/Unique 고갈 처리도 유지해야 한다.
4. 증강/전직/결과가 각자 Time.timeScale을 변경한다. 도구 획득 선택 추가 시 중첩 선택, 클릭 잔류, 결과 도달과 선택 종료 순서를 확인해야 한다. 현재 도구 코드에서 일반적인 UI 포인터 차단을 찾지 못해 Hotbar 클릭과 월드 사용의 중복 실행을 후속 검증해야 한다.
5. 업그레이드/직업 효과가 개별 도구 및 배치 컨트롤러에 저장되고 Run 재시작은 Scene 재로드다. 향후 지역 이동에서 빌드 보존과 월드 초기화를 구분할 설계가 필요하다. 이번에는 Area 리팩터링하지 않는다.
6. `PrototypeHUDCanvas.UpdateCastNetInfo`는 `[E]`를 고정 출력한다. 동적 Hotbar는 아직 없다. 기존 정보를 대체한 뒤 중복 표시를 제거한다.
7. 기존 README/GDD/PrototypeDesign/VerticalSliceDesign에는 자유 이동 중심 설명, 마감 조업 타이머, 어획률 성공선, 보스/낚싯대 미구현 계획 등이 남아 최신 코드/설계와 다르다. 역사적 문서로 보존했고 새 설계 문서를 우선하도록 명시했다.
8. UTF-8 엄격 디코딩에서 Scripts 26개 중 19개가 실패했다. 예를 들어 PrototypeGameFlowManager는 CP949로 읽으면 `연안 조업 성공!` 등 정상 한국어가 나온다. UTF-8 화면의 깨짐만으로 원본 문자열 손상을 단정하지 않는다. 원본 인코딩을 확인하고 관련 수정 때만 안전하게 UTF-8로 저장한다. 이번에는 C# 인코딩을 변경하지 않았다.
9. FishRoute에는 OnGUI 경로 라벨 코드가 남지만 Main의 조사된 4개 경로는 showRouteLabels=0이다. 주요 UI 이관 완료와 OnGUI 완전 제거는 다른 상태다.

## Unity Scene/Inspector 확인 사항
- 실제 게임 Scene은 `Assets/Scenes/Main.unity`. `Assets/_Recovery/0.unity`와 URP 템플릿도 있으나 게임 진입 Scene으로 가정하지 않는다.
- **Main의 FishSpawner.bossTestMode=1, miniBossTestMode=0**. 현재 저장된 Scene은 정상 전체 Run 대신 보스 테스트 경로로 시작한다. 전체 Run 검증 전에 Inspector에서 모드를 확인해야 한다. 이번에는 변경하지 않았다.
- BossEncounterController의 maxPasses=3, 회유 경로 3개 참조가 지정되어 있다. Augment/Job Manager의 도구 참조와 HUD/선택/결과/보스 패널 참조도 YAML에서 확인했다. Editor에서 Missing 참조나 클릭 동작까지 검증한 것은 아니다.
- 코드 기본값과 Inspector 값이 다르다. 예: 뜰채 capturePower/radius/cooldown은 Main에서 3/1.1/0.45다. 이번에 수치 변경은 없다.
- Main의 TMP fontAsset 참조는 NanumGothic-Bold SDF GUID를 사용하며 해당 에셋 atlas population mode는 Dynamic이다. 폰트 3개의 기존 변경은 보존했다.
- **ProjectSettings/EditorBuildSettings.asset는 존재하지 않는 Assets/Scenes/SampleScene.unity를 등록**한다. Main의 빌드 포함과 결과 화면 재시작(buildIndex 사용)은 Unity Build Profiles/Editor에서 별도로 확인해야 한다. 이번 문서 작업에서는 수정하지 않았다.

## 초기 온보딩 변경·검증
- 새 파일: `AGENTS.md`, `Docs/NETBREAK_DESIGN.md`, `NETBREAK_STATE.md`만 작성. 기존 Docs 대문자 경로를 사용한다.
- Git 상태/브랜치/remote/HEAD, 파일 구조, 주요 제어 흐름, 입력 API, 증강 풀/직업/보스 결과, Scene 직렬화 설정, UTF-8 유효성 및 CP949 예시를 정적으로 조사했다.
- 프로젝트에 추적된 테스트/asmdef 파일은 검색되지 않았다. Test Framework 패키지 설치는 테스트 실행의 증거가 아니다.
- 이번 작업은 문서 전용이며 Unity 프로세스는 조사 시 실행 중이지 않았다. Editor 실행, 컴파일, Console, Play Mode, 빌드/재시작 검증은 수행하지 않았다. 게임 동작 검증 성공으로 간주하지 않는다.
- Gameplay, Scene, Prefab, 설정, 밸런스 및 기존 폰트 변경을 수정하지 않았다. commit/push하지 않았다.

## 최근 변경 — 상용 1.0 로드맵
- 사용자가 초기 온보딩 문서를 검토했고, STEP 10A 전에 장기 로드맵 추가를 요청했다.
- 새 파일 `Docs/NETBREAK_ROADMAP.md`: Area 1 Slice → 외부 플레이테스트 → Area 2 제작 파이프라인 검증 → 데모 품질 → 본 제작 → 전체 Meta 진행 → Alpha → Beta/RC → 상용 1.0 → 출시 후 대응의 10단계를 정리했다.
- 외부 검증 전 대규모 콘텐츠 제작 금지, Area 2에서 필요성이 확인된 데이터화, Run/Meta 구분 및 미래 시스템의 성급한 밸런싱 방지를 명시했다. Area 2~6 세부 설계, 데모 분량과 출시일은 확정하지 않았다.
- 이번 수정 파일은 로드맵과 `NETBREAK_STATE.md`뿐이다. 기존 온보딩 문서와 폰트 변경을 보존한다. 문서 검토 및 Git status/diff 확인 대상이며 Unity 컴파일/실행 검증을 의미하지 않는다.
- STEP 10A, 게임 코드/Scene/Prefab/설정 변경, commit/push는 이번 요청 범위에서 제외했다.

## 최근 변경 — STEP 10A Dynamic Tool Slot Foundation
- `RunManager`가 Run 단위 4칸 `RunToolLoadout`과 프레임 단위 `ToolSlotInput`을 소유한다. LMB 뜰채는 슬롯 밖에 고정하고, STEP 10A 임시 로드아웃은 Q=미끼, W=그물, E=투망, R=낚싯대다.
- 네 액티브 도구 Controller의 Q/W/E/R 직접 참조를 제거했다. 공통 입력이 슬롯 누름/뗌/취소를 도구에 전달하며 빈 슬롯, 슬롯 교환, 선택창 차단, 설치·재배치 충돌을 처리한다.
- Scene/Prefab/Inspector 참조와 밸런스 값은 변경하지 않았다. Tool Acquisition, Run Ownership 기반 증강 필터, Hotbar는 STEP 10B~10D 범위다.
- Unity 6000.3.11f1에서 스크립트 컴파일과 Console compiler error 0개를 확인했다. Main Play Mode 입력 회귀 32개가 통과해 LMB, Q/W/E/R, ESC/RMB 취소, 실제 설치 비용/피해, Ctrl+LMB 재배치, 슬롯 교환·빈 슬롯을 확인했다.

## 최근 변경 — STEP 10B Tool Acquisition
- 새 Run은 뜰채만 소유하고 Q/W/E/R 슬롯은 비어 있다. `RunToolLoadout`이 Run 소유권을 관리하며 미소유 도구의 슬롯 배치를 차단한다.
- `ToolAcquisitionManager`가 현재 구현된 미끼·그물·투망·낚싯대 중 미소유 도구를 최대 3개 제시하고, 선택한 도구를 첫 빈 슬롯에 배치한다. 사용 가능 풀은 이후 Meta 해금 결과로 교체할 수 있게 Run 소유 데이터와 분리했다.
- 기존 `PrototypeSelectionCanvas`의 3개 선택 버튼을 재사용하며 도구 획득 제목·이름·설명을 한국어로 표시한다. Area 진행 타이밍, 소유 도구 증강 필터, Hotbar는 구현하지 않았다.
- Unity 컴파일과 Console Error 0개를 확인했다. STEP 10A 회귀 33개와 STEP 10B 획득 16개가 통과했고, Computer Use로 획득 UI의 한국어 제목·3개 선택지 표시를 확인했다.
- 알려진 문제: 현재 획득 후보 순서는 결정론적 고정 순서이며 Area 1 실제 획득 시점과 연결되지 않았다. 네 도구를 모두 소유한 뒤에는 추가 획득 요청을 받지 않는다.

## 최근 변경 — STEP 10C 초기 도구 진행과 소유 도구 증강
- Coast 초반 조업 시작 시 첫 도구 획득을 요청하고, 초반 조업 종료 뒤 두 번째 도구 획득을 요청한다. 선택이 끝날 때까지 진행을 대기하며 기존 획득 시스템과 선택 Canvas를 그대로 사용한다.
- 액티브 도구 2개를 소유하기 전에는 레벨업 증강을 보류한다. 두 번째 도구 획득 뒤 보류 중인 증강을 열고, 이후에는 기존 레벨업 흐름을 유지한다.
- 증강 후보는 General, 항상 소유하는 뜰채, 현재 Run에서 소유한 액티브 도구 범주만 허용한다. 미소유 도구의 일반·Unique 증강은 제외한다.
- 변경 파일: `RunToolLoadout.cs`, `RunManager.cs`, `ToolAcquisitionManager.cs`, `PrototypeAugmentManager.cs`, `FishSpawner.cs`, `NETBREAK_STATE.md`.
- STEP 10C 수동 검증 완료: 사용자가 새 Run의 첫 획득에서 미끼 선택, Q 슬롯 배치, 선택창 종료 후 실제 Q 미끼 사용을 확인했다. 확장된 결정론적 획득 검증은 새 Run 뜰채 단독 소유와 빈 Q/W/E/R, 첫·두 번째 도구의 Q/W 배치, 소유 도구 후보 제외, 2개 전 증강 보류, 2개 후 증강 허용, 미소유 도구 증강 제외를 확인했다. 기존 전직 및 Gameplay 코드는 변경하지 않았다.
- 알려진 임시 페이싱: 현재 첫 도구 획득이 Coast 시작 직후 발생한다. 기능 검증용 임시 배치이며, Area 1 전체 페이싱 정리 단계에서 뜰채만 사용하는 짧은 도입 구간 이후로 이동한다.
- 알려진 임시 페이싱: 현재 두 번째 도구 획득 직후 보류된 첫 증강 선택창이 바로 이어서 표시된다. 기능 검증용 동작이며, Area 1 전체 페이싱 정리 단계에서 두 번째 도구를 실제로 사용해볼 수 있는 짧은 조업 구간을 둔 뒤 첫 증강이 나오도록 조정한다.

## 최근 변경 — STEP 10D Dynamic Hotbar
- 기존 `GameCanvas`/`PrototypeHUDCanvas` 안에 Bottom Center 기준의 독립 가로형 Hotbar를 구성했다. `[LMB] 뜰채 / 고정 도구`와 Q/W/E/R 네 슬롯을 표시하며, 빈 액티브 슬롯은 `비어 있음`으로 보인다.
- Q/W/E/R 표시는 매 프레임 `RunManager.ToolSlots`의 실제 `RunToolLoadout`을 읽는다. HUD에 소유권이나 슬롯 배치를 복제하지 않으며, 도구 획득 직후 실제 배치된 Q/W 슬롯이 즉시 갱신된다. 기존 좌측 상단 HUD에는 Gold·포획·어획률·Level·EXP·구간 등 Run 상태만 남고, 고정 `[E]` 투망 안내는 새 Hotbar의 동적 투망 상태 표시로 대체했다.
- Hotbar의 배경과 TMP 텍스트는 Raycast를 받지 않아 월드 입력과 선택 UI를 가로막지 않는다. Scene YAML과 NanumGothic 에셋, Gameplay 도구 입력 코드는 변경하지 않았다.
- `ToolAcquisitionValidation`의 Q 오탐은 선택창 종료 직후 가상 키 press/release 상태를 Gameplay 동작으로 판정하던 검사를 제거하고, 실제 슬롯 배치와 `ToolSlotInput`의 Q 바인딩을 검증하도록 수정했다. 검증 중 Coast 진행 코루틴이 두 번째 획득과 충돌하지 않도록 검증 내부에서만 조업 상태를 분리했다.
- Unity 6000.3.11f1 컴파일 및 최종 Console Error 0 / Warning 0을 확인했다. STEP 10A 입력 회귀 33개가 통과했고, 확장된 STEP 10B~10D 획득·소유·증강·Hotbar 검증 25개가 통과했다.
- Computer Use Play Mode 확인: 하단 중앙 5슬롯, LMB 고정 뜰채, 초기 Q/W/E/R 빈 상태, 좌측 상단 Run HUD 유지와 기존 Hotbar 텍스트 제거, 한국어 선택 UI와 Hotbar의 비중첩을 확인했다. 첫 도구 Q 및 두 번째 도구 W 갱신은 결정론적 검증으로 확인했고, 첫 미끼 Q 표시와 실제 Q 사용은 사용자 수동 확인 결과를 반영했다.
- 알려진 임시 페이싱은 유지한다: 첫 Tool Acquisition은 Coast 시작 직후 발생하고, 두 번째 Tool Acquisition 직후 첫 Augment가 바로 표시된다.

## 다음 정확한 단계
STEP 10E Area 1 progression/pacing cleanup을 진행한다.
