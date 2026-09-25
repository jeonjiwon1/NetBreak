# NETBREAK 인수인계

## VS-2C-3 — 복어 그물 중단 순간 연출 (2026-09-25, Unity 수동 검증 완료)

- 활성 그물·복어 접촉 성공 시 `NetController`가 기존 `DisableTemporarily`를 먼저 적용하고 전용 이벤트, 특수 애니메이션, 접촉점 Impact VFX와 SFX를 즉시 시작한다. 비활성 그물의 `OnTriggerStay2D`는 반환하므로 동일 중단 중 연출을 반복하지 않는다. 기존 3초 중단, 감속·포획 피해·Resistance·Collider·타겟 판정은 변경하지 않았다.
- 승인된 수영 시트를 기준으로 `Pufferfish_Disrupt.png` 48×48 셀×4방향축×4프레임(192×192)을 만들었다. 12 FPS, 약 0.33초의 순간 반응 후 현재 이동 방향 Swim으로 복귀한다. 지속 팽창 상태나 새 공격 판정은 없다.
- `Pufferfish_NetImpact.png`는 24×24 셀×4프레임, 0.28초다. 공용 `CombatVfxPool`의 상한 48과 RepeatedHit 우선순위를 사용한다. `Pufferfish_NetDisrupt.wav`는 내부 합성 44.1 kHz/16-bit/mono/0.22초 one-shot이고 Profile 볼륨 0.34와 전역 0.1초 동일음 중복 제한을 쓴다. 공용 AudioSource의 Pause/Run 정리를 공유한다.
- `PufferfishDisruptionProfile`과 `NETBREAK/Art/Setup Pufferfish Disruption Presentation`, `Validate Pufferfish Disruption Presentation` 메뉴가 16+4 Sprite 분할·Import·WAV·FishData·Resources 참조를 확인한다. Scene/Prefab/ProjectSettings/URP는 수정하지 않았다.
- 신규 `PufferfishDisruptionTests` 7개는 성공 순서·단발 연출·무효 대상·애니메이션·VFX 풀·오디오 중복·에셋 누락·풀 포화·풀 재사용·scaled time을 검사한다. 별도 프로젝트 복사본의 Unity 6000.3.11f1에서 Runtime/Editor/Test assembly 컴파일 및 **전체 EditMode 200/200 통과, 실패 0·skip 0**을 확인했다. 최초 배치 실행은 Package Manager IPC 문제로 종료됐고, 첫 실제 테스트의 1개 실패는 경계 시점 테스트 입력을 수정해 전체 재실행으로 해결했다. 같은 복사본에서 Setup/Validate 메뉴를 두 번 실행했고 16+4 Sprite·WAV·FishData 검증 로그와 Profile/Import 메타데이터 해시 불변을 확인했다. PNG·메타데이터·Profile·WAV 정적 검사도 통과했다.
- 사용자 원본 Unity Editor에서 compile 정상, Console Error 0, **전체 EditMode 200/200 통과**를 확인했다. Play Mode에서 실제 활성 그물·복어 접촉 시 즉시 중단과 약 3초 지속, Puffer Disrupt 애니메이션과 현재 방향 Swim 복귀, 접촉점 Impact 및 SFX를 확인했다. 지속 접촉의 연출 반복과 동일음의 과도한 중첩은 없었고 일반 어종에는 복어 연출이 발생하지 않았다. Pause, Fish Pool 재사용, Run 재시작의 잔상 없이 기존 Squid Ink Presentation·Fish animation·Resistance·포획·UI·VFX도 정상이다. **Puffer Disrupt Sprite: Manual Visual Validation Passed / Prototype Approval Approved / Final Production Art Approval Pending. Puffer Net Impact: Manual Visual Validation Passed / Prototype Approval Approved / Final Production VFX Approval Pending. Puffer Net Disrupt SFX: Manual Audio Validation Passed / Prototype Approval Approved / Final Production Audio Approval Pending.** 기존 Net 중단 시간·판정·감속·포획·Resistance 수치는 변경하지 않았으며 지속 팽창 gameplay는 없다. 현재 12 FPS·약 0.33초·Impact 0.28초·SFX 볼륨/중복 제한은 최종 출시 확정값이 아니다. 이번 문서 갱신에서 Computer Use와 Git add/commit/push는 수행하지 않는다.

## VS-2C-2 — 오징어 먹물 Projectile·Impact (2026-09-24, Unity 수동 검증 완료)

- 시작 HEAD `3af01e6`, `vertical-slice`, 작업 트리 깨끗함. 최신 사용자 지시에 따라 Computer Use와 Git add/commit/push를 수행하지 않는다.
- 기존 임시 대상 효과는 `ItemEffectManager.ShowSquidInkAttack`의 RepeatedHit `LineRenderer` 번개형 연결선과 Release 즉시 표시되는 원형 마커였다. `SquidController.ReleaseInk`가 `DisableTemporarily`로 실제 방해를 먼저 적용한 뒤 Release 콜백에서 이 효과를 호출했다. 이는 판정과 분리된 Presentation이었다. 이제 연결선·즉시 마커를 생성하지 않고 Projectile 이동 완료 시 Impact를 한 번 표시한다. 낚싯대·그물의 지속 방해 시각 상태는 유지한다.
- 실제 성공 대상인 그물·낚싯대의 위치를 `ReleaseInk`에서 중복 제거 후 캡처해 Presentation에 전달한다. 별도 대상 검색, 충돌 판정이나 피해는 없다. 0.22초 동안 오징어 중심에서 당시 대상 위치로 직선 이동한다. 어구가 제거되거나 재사용되어도 stale 참조가 없다. 0.3초 Impact 뒤 Pool에 반환한다. 시작 위치는 4축별 입 소켓이 없어 추정 오프셋보다 중심을 택했다.
- `Squid_InkProjectile.png`는 16×16×4프레임(시트 64×16), `Squid_InkImpact.png`는 24×24×4프레임(시트 96×24)의 내부 생성 RGBA Prototype이다. 기존 Ink Puff 팔레트를 사용한다. Sprite 분할 `.meta`와 `SquidInkPresentation.asset` 참조를 작성하고 기존 Setup/Validate 메뉴를 확장했다. Scene/Prefab/ProjectSettings는 변경하지 않았다.
- `CombatVfxPool`의 기존 상한 48·RepeatedHit 우선순위·scaled `Tick`을 재사용한다. 이동·Impact 대기값은 풀 반환/Run 정리에서 지운다. 풀 포화로 VFX가 생략되거나 재활용되어도 즉시 적용된 방해 판정은 유지된다. 기존 Ink Attack 4방향 애니메이션, Release Puff와 `Squid_InkRelease.wav` 재생 시점·중복 보호는 유지하며 새 소리는 없다.
- `SquidInkPresentationTests`, `CombatVfxTests`를 Release의 단발 발사, 성공 대상 위치, 이동·도착 후 단발 Impact, 임시 선 중복 제거, Pause/풀 반환/Run 정리, cap·우선순위와 기존 상태 표시 기준으로 확장했다. PNG·메타·Profile 참조 정적 검사는 완료했다. 최초 구현 시 Unity 배치 시도는 Package Manager IPC/Licensing Client 연결 문제로 끝나지 못했다. 이후 사용자 Editor 전체 EditMode에서 192/193 통과, `FullVisualPoolDoesNotDelaySquidInterference` 1개 실패를 확인했다.
- 회귀 원인은 게임 방해/풀 우선순위가 아니라 신규 테스트 Fixture의 낚싯대 Collider가 EditMode 물리 쿼리에 등록되지 않은 것이었다. 실패 재현에서 `specialDisabledUntil=0`, 범위 hit 0, Collider bounds 크기 0을 확인했다. 기존 성공 Fixture와 동일한 자식 Collider를 추가하고 실제 `OverlapCircleAll` 대상 포함을 선행 검증하도록 수정했다. 방해 상태·지속 표시·작동 중단·풀 상한 assertion은 유지·강화했다. 별도 프로젝트 복사본의 Unity EditMode 재실행 결과 실패 테스트 **1/1**, `CombatVfxTests` **21/21**, `SquidInkPresentationTests` **9/9**, 전체 **193/193**, 실패 0·skip 0이다. Runtime/Editor/Test assembly가 컴파일되어 테스트 실행까지 완료했고 테스트 로그에 CS 오류는 없었다.
- `Tools/validate_squid_ink_projectile.py` 정적 검사 2/2 통과. 이후 사용자가 원래 Unity Editor에서 compile 정상, Console Error 0, 전체 EditMode **193/193 통과**와 Play Mode 수동 검증을 확인했다. 두 새 VFX의 실제 표시와 Profile 연결은 Play Mode에서 확인됐으며 Setup/Validate 메뉴 실행 여부는 별도로 전달받지 않았다.
- Play Mode에서 먹물 gameplay 판정·Ink Attack animation·Release VFX/SFX가 정상이고, **Release → 실제 선택된 대상 어구로 이동하는 Projectile → 도착 시 Impact → 기존 지속 방해 상태 표시**가 이어졌다. 이동 속도·가독성, 다중 오징어 대상 연결, 대상 비활성·누락 시 오류·잔상 없음, Pause와 Pool/Run 재시작 초기화도 확인했다. 기존 임시 연결선·즉시 target marker는 중복 표시되지 않았으며 기존 Fish animation·Resistance·Collider·포획·UI·VFX도 정상이다. Gameplay 판정·타겟 선정·방해 지속시간은 변경하지 않았다.
- **Ink Projectile 및 Ink Impact 각각 Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.** 현재 이동시간·크기·Pool priority는 최종 출시 확정값이 아니다. 기존 immutable package 경고의 `Packages/com.unity.2d.animation/package.json`은 Git 변경사항이 아니며 관련 파일을 수정하지 않았다.

## VS-2C-1 — 오징어 먹물 공격 Presentation (2026-09-24, Unity 수동 검증 완료)

- 시작 HEAD `a042316`, `vertical-slice`, 기존 작업 트리 깨끗함. 이번 요청에 따라 Git add/commit/push를 수행하지 않는다.
- `SquidController.ReleaseInk`의 기존 OverlapCircleAll, 그물/낚싯대 중복 제거, DisableTemporarily, InkRange·InkInterval·FirstInkDelay·InkDisableDuration을 보존한다. 영향을 받은 대상이 있을 때 방해 적용 직후 Presentation을 한 번 시작한다. Animation Event는 게임플레이 판정을 만들지 않는다.
- 기존 승인된 `Squid_Swim.png`는 변경하지 않았다. 별도 `Squid_InkAttack.png`는 64×64 셀, E/N/NE/NW 4축×4프레임, 8 FPS의 내부 생성 Prototype이다. `FishVisualController`는 시작 방향을 잠그고 frame 2에서 VFX/SFX를 호출한 뒤 현재 이동 방향의 Swim으로 복귀한다. Pool 재초기화/비활성화에서 special state를 지운다.
- `Squid_InkPuff.png`는 32px×4프레임의 짙은 검보라 구름이다. `CombatVfxPool`에 SpriteRenderer transient를 추가해 기존 48개 공용 상한과 RepeatedHit 우선순위를 사용한다. 낚싯대 대상의 기존 궤적/타격 표시를 함께 Release 프레임에 보여준다. 결과 상태 표시 UI는 유지한다.
- `Squid_InkRelease.wav`는 프로젝트 내부 생성 44.1 kHz/16-bit/mono/0.28초 one-shot Prototype이다. `ItemEffectManager`의 재사용 AudioSource에서 Profile 볼륨(기본 0.38)으로 재생하고 같은 소리의 전역 0.08초 cooldown만 적용한다. Pause 시 AudioSource를 Pause/UnPause하고 Run 상태 정리 시 Stop한다.
- `SquidInkArtSetup.Setup`/`Validate` 메뉴로 새 두 시트를 분할하고 `Assets/Resources/SquidInkPresentation.asset`에 16+4 Sprite와 AudioClip을 연결했다. 메뉴 성공 로그를 확인했다. Scene/Prefab YAML은 수정하지 않았다. 프로필 누락 시 기존 방해 Gameplay는 유지하고 특수 애니메이션/먹물 구름/소리는 생략된다.
- 신규 `SquidInkPresentationTests`는 성공 트리거, 즉시 Gameplay 분리, 방향 잠금, 4프레임 진행/Swim 복귀, VFX 시점·반환, Pause, Pool 재사용, 누락 Profile, 오디오 중복 방지 등을 대상으로 작성했다. 기존 Fish directional 및 먹물 수치 검사는 유지했다.
- 자동 검증 상태: 생성 PNG는 256×256/128×32 RGBA, alpha 0/255만 사용하고 WAV는 44.1 kHz/16-bit/mono/0.28초임을 확인했다. 정상 패키지 상태의 Unity 배치 실행에서 Runtime/Editor 스크립트 빌드 성공, `Setup Squid Ink Presentation`의 16+4 분할·Profile/Audio 링크 및 Validate 성공 로그를 확인했다. 첫 전체 EditMode 실행의 테스트 fixture Collider 설정과 기존 VFX 개수 기대값을 수정하고 자산 연결 검사를 추가한 뒤 **전체 EditMode 190/190 통과, 실패 0, skip 0**. 초기 Unity 배치 시도는 Package Manager IPC, `-noUpm` 시도는 Licensing Client/패키지 참조 문제로 실패했고 정상 재실행으로 해결했다.
- 사용자 Unity 수동 검증 완료: compile 정상, Console Error 0, 전체 EditMode 190/190 통과. 먹물 방해 판정 즉시 적용, E/N/NE/NW 및 반대 방향의 공격 애니메이션, 현재 방향 Swim 복귀, VFX 발동·Pool 반환, SFX 1회·다중 오징어 중복 보호, 영향 대상 없음의 무연출, Pool/비활성화·Pause·Run 재시작 초기화를 확인했다. 기존 Fish Swim·Resistance·포획·UI·VFX도 정상이다. **Squid Ink Animation/VFX/SFX Prototype Approval: Approved. Final Production Art/VFX/Audio Approval: Pending.** 현재 볼륨·cooldown·loudness·Mixer 정책은 최종 확정값이 아니다.
- `git diff --check` 종료 코드 0. 새 미추적 텍스트·메타데이터의 trailing whitespace도 별도로 확인했다. 기존 수영 시트/프로필, FishData 수치, Scene/Prefab, ProjectSettings에는 diff가 없다.

기준일: 2026-09-24. 현재 마일스톤: **G6-C2와 현재 버전 최소 안정화 수동 검증 완료. UX-F1 및 UX-F2-A/B 임시 전투 피드백 구현·자동·수동 검증 완료. VS-2 아트 방향 1차 확정. VS-2B-1~3의 12프레임 프로토타입은 당시 Unity 수동 검증과 승인을 완료했고, VS-2B-4의 16프레임 방향 규칙은 Unity 수동 검증 및 전체 EditMode 183/183 통과로 현재 프로토타입 승인**. 목표 설계는 `Docs/NETBREAK_DESIGN.md`, 성장 설계는 `Docs/NETBREAK_GROWTH_SYSTEM.md`, 아트 기준은 `Docs/NETBREAK_ART_GUIDE.md`, 장기 순서는 `Docs/NETBREAK_ROADMAP.md`, 작업 규칙은 `AGENTS.md`를 읽는다. 구현의 기준은 Git이며 현재 개발 상태의 기준은 이 문서다.

## Git·환경
- 저장소: `C:\game_dev\unity\NetBreak`, 브랜치 `vertical-slice`.
- origin: `https://github.com/jeonjiwon1/NetBreak.git`.
- 이번 문서 작업 시작 시 확인한 HEAD: `3bc311c` (`Add item and combined synergy VFX`), `origin/vertical-slice`와 동일했고 작업 트리는 깨끗했다. UX-F2-B 코드·테스트·문서와 당시 작업 트리에 있던 URP 및 ProjectSettings 관련 변경이 같은 커밋으로 push되어 있다. 이 Git 사실은 해당 설정을 Unity에서 별도로 재검증했다는 뜻이 아니다.
- Windows Build Profile과 Main Scene 등록은 앞선 `99e2031`, UX-F1은 `5b595e0`, UX-F2-A는 `c1d75fe`, UX-F2-B는 `3bc311c`에 포함되어 있다. `3bc311c`에는 `Assets/DefaultVolumeProfile.asset`, `Assets/Settings/UniversalRP.asset`, `Assets/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/TimeManager.asset`, `ProjectSettings/UnityConnectSettings.asset` 변경도 함께 포함되어 있다. 이번 문서 작업에서 이를 되돌리거나 수정하지 않았다.
- Unity `6000.3.11f1`, URP `17.3.0`, Input System `1.19.0`, Test Framework `1.6.0`. `activeInputHandler: 1`.
- 추적 파일 242개. 주요 구조: Assets/Scripts/{Core,Fish,Gear,UI}, Assets/Editor, Assets/Scenes, Assets/Prefabs/{Fish,Gear}, Assets/UI/Fonts, Packages, ProjectSettings, Docs. Library/Logs/UserSettings는 로컬 Unity 산출물이다.

## 초기 온보딩 당시 코드에서 확인한 시스템과 연결 — 역사적 스냅샷
아래 표는 성장 시스템 전환 전의 정적 조사 기록이며 현재 구현 설명이 아니다. 실행 검증 완료를 뜻하지 않는다.

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

## 초기 온보딩 당시 설계 충돌·미구현·주의점 — 역사적 스냅샷
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
- 현재 Main의 `FishSpawner.bossTestMode=0`, `miniBossTestMode=0`이며 정상 Area 1 시퀀스 설정이다. 이는 직렬화 상태 확인이며 현재 G6-C2 전체 Run 성공 검증을 뜻하지 않는다.
- BossEncounterController의 maxPasses=3, 회유 경로 3개 참조가 지정되어 있다. Augment/Job Manager의 도구 참조와 HUD/선택/결과/보스 패널 참조도 YAML에서 확인했다. Editor에서 Missing 참조나 클릭 동작까지 검증한 것은 아니다.
- 코드 기본값과 Inspector 값이 다르다. 예: 뜰채 capturePower/radius/cooldown은 Main에서 3/1.1/0.45다. 이번에 수치 변경은 없다.
- Main의 TMP fontAsset 참조는 NanumGothic-Bold SDF GUID를 사용하며 해당 에셋 atlas population mode는 Dynamic이다. 폰트 3개의 기존 변경은 보존했다.
- `99e2031`에서 Windows Build Profile을 추가하고 `ProjectSettings/EditorBuildSettings.asset`의 등록 Scene을 `Assets/Scenes/Main.unity`로 수정했다. 사용자는 이후 Windows 빌드를 생성하고 EXE 실행을 확인했다. 결과 화면 재시작과 새 Run 초기화는 별도 통합 검증 대상으로 남아 있다.

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
- 해결됨(STEP 10E): 첫 도구 획득 전 뜰채만 사용하는 15초 도입 구간을 추가했다.
- 해결됨(STEP 10E): 두 번째 도구 획득 후 15초 조업 구간을 거친 뒤 첫 증강을 허용한다.

## 최근 변경 — STEP 10D Dynamic Hotbar
- 기존 `GameCanvas`/`PrototypeHUDCanvas` 안에 Bottom Center 기준의 독립 가로형 Hotbar를 구성했다. `[LMB] 뜰채 / 고정 도구`와 Q/W/E/R 네 슬롯을 표시하며, 빈 액티브 슬롯은 `비어 있음`으로 보인다.
- Q/W/E/R 표시는 매 프레임 `RunManager.ToolSlots`의 실제 `RunToolLoadout`을 읽는다. HUD에 소유권이나 슬롯 배치를 복제하지 않으며, 도구 획득 직후 실제 배치된 Q/W 슬롯이 즉시 갱신된다. 기존 좌측 상단 HUD에는 Gold·포획·어획률·Level·EXP·구간 등 Run 상태만 남고, 고정 `[E]` 투망 안내는 새 Hotbar의 동적 투망 상태 표시로 대체했다.
- Hotbar의 배경과 TMP 텍스트는 Raycast를 받지 않아 월드 입력과 선택 UI를 가로막지 않는다. Scene YAML과 NanumGothic 에셋, Gameplay 도구 입력 코드는 변경하지 않았다.
- `ToolAcquisitionValidation`의 Q 오탐은 선택창 종료 직후 가상 키 press/release 상태를 Gameplay 동작으로 판정하던 검사를 제거하고, 실제 슬롯 배치와 `ToolSlotInput`의 Q 바인딩을 검증하도록 수정했다. 검증 중 Coast 진행 코루틴이 두 번째 획득과 충돌하지 않도록 검증 내부에서만 조업 상태를 분리했다.
- Unity 6000.3.11f1 컴파일 및 최종 Console Error 0 / Warning 0을 확인했다. STEP 10A 입력 회귀 33개가 통과했고, 확장된 STEP 10B~10D 획득·소유·증강·Hotbar 검증 25개가 통과했다.
- Computer Use Play Mode 확인: 하단 중앙 5슬롯, LMB 고정 뜰채, 초기 Q/W/E/R 빈 상태, 좌측 상단 Run HUD 유지와 기존 Hotbar 텍스트 제거, 한국어 선택 UI와 Hotbar의 비중첩을 확인했다. 첫 도구 Q 및 두 번째 도구 W 갱신은 결정론적 검증으로 확인했고, 첫 미끼 Q 표시와 실제 Q 사용은 사용자 수동 확인 결과를 반영했다.
- STEP 10C에서 남긴 두 임시 페이싱 문제는 STEP 10E에서 해결했다.

## 최근 변경 — STEP 10E Area 1 progression/pacing cleanup
- 실제 진행 순서는 조업 시작 후 15초 Landing Net-only → 첫 Tool Acquisition → 45초 gameplay → 두 번째 Tool Acquisition → 15초 gameplay → 첫 Augment 허용이다.
- `RunManager`에 진행 기반 Augment 잠금을 추가했다. 두 도구 소유 조건과 별도로 위 페이싱이 끝날 때 잠금을 해제하며, 그동안 쌓인 EXP와 보류된 레벨업은 유지된다.
- STEP 10A 검증은 ESC 복원 직후 합성 입력의 물리 위치를 동기화하고 중립 프레임을 둔다. 또한 STEP 10E 진행 잠금을 검증 내부에서만 해제한 뒤 선택창 입력 차단을 확인한다. `GearRepositionController`, `ToolSlotInput`, `RunToolLoadout` 및 배치·입력 Gameplay 코드는 변경하지 않았다.
- Unity 6000.3.11f1 스크립트 컴파일과 Console Error 0개를 확인했다. STEP 10A 입력 회귀 33개와 STEP 10B~10E 통합 검증 32개가 통과했다. Ctrl+LMB Gear Reposition의 그물 선택·재배치는 사용자 수동 검증에서도 정상임을 확인했다.

## 최근 변경 — Pre-full-run cleanup
- STEP 10E 완료 상태는 유지된다.
- Tool Acquisition은 현재 구현된 Active Tool 중 현재 Run에서 미소유인 도구 풀을 임시 목록으로 만들고, 마스터 사용 가능 목록을 변경하지 않은 채 최대 3개를 무작위·중복 없이 제시한다. 남은 유효 도구가 3개 미만이면 그 도구만 후보가 되며, Tool Acquisition 리롤은 의도적으로 추가하지 않았다.
- Coast의 일반 Ambient Spawn Event 간격을 임시 1차 조정했다: Early 2.0~3.0초, Growth 1.7~2.6초, Special 1.5~2.4초, MiniBossSupport 2.0~3.0초, Rush 0.9~1.6초, Final 1.1~1.8초.
- Spawn 수량·어종 구성·어군 크기·Boss/MiniBoss·풀링·보상 및 밸런스 구조는 재설계하지 않았다. 진지한 스폰/밸런스 결정은 측정된 전체 Run 테스트 이후로 미룬다.
- Bottom-center Hotbar에 뜰채·미끼·투망 쿨다운 Fill을 추가했다. 미끼와 투망은 실제 배치된 Q/W/E/R 슬롯을 따라가며 쿨다운 중 남은 초도 표시한다.
- 쿨다운 UI는 각 Controller의 기존 실제 쿨다운 타이머를 읽으며 독립 HUD 타이머를 만들지 않는다. 기존 효과로 실제 쿨다운이 바뀌면 같은 상태가 즉시 반영된다.
- 기존 좌측 `CastNetText`의 고정 키·준비/쿨다운 표시는 비활성화해 동적 Hotbar를 유일한 플레이어 노출 투망 상태 표시로 사용한다. Gold·포획·어획률·Level·EXP·구간 등 Run HUD는 유지한다.
- 이번 정리는 코드 구현만 수행했으며 Unity 컴파일, Console, Play Mode 및 자동 검증을 실행하지 않았다. 수동 Unity 검증이 필요하다.

## 당시 다음 단계 기록 — 역사적
당시 예정은 FULL RUN #2 수동 검증 및 실측이었다. 현재 다음 단계는 문서 끝의 최신 기획 변경을 따른다.

## 최근 변경 — Balance Inspector Pass
- Area 1 / Vertical Slice에서 반복 조정 가능성이 높은 하드코딩 수치만 Inspector에 노출했다. EXP 단계별 요구량과 Lv4+ 성장식, 낚시꾼의 낚싯대 최대 수·설치비 배율, 투망 기본 최대 충전 수, 구간별 Ambient 스폰 간격·특수어 확률을 각 기존 권위 컴포넌트에 두었다.
- 이미 직렬화된 시작 Gold, 낚싯대 설치 경제·전투 수치, 뜰채·미끼·그물·투망 전투 수치, Area 1 구간 시간·경고 시간은 중복 필드를 만들지 않고 그대로 유지했다. FishData 소유 수치도 다른 Manager로 복제하지 않았다.
- Lv2/Lv3 도구 획득, Lv4+ 증강을 비롯한 진행·입력·성공/실패 규칙은 계속 코드로 정의된다. 새 필드 기본값은 기존 하드코딩 값과 같아 현재 밸런스를 의도적으로 보존한다.
- Unity 실행, 컴파일, Console, Play Mode 및 수동 검증은 수행하지 않았다. 사용자가 Unity Inspector 연결·표시와 기존 동작을 확인해야 하며 다음 단계는 계속 측정 기반 Full Run 밸런스 테스트다.

## 최근 변경 — FULL RUN #1 후 1차 밸런스·UX 패치
- FULL RUN #1 실측: 약 10~12분, Level 12, 잔여 Gold 2361, 어획률 100%, 포획 1594마리. 낚싯대+그물 빌드와 낚시꾼 직업을 사용했다. 중반 이후 난도가 낮고 자동화되었으며 미니보스는 약 3초, 빌드 완성 뒤 Gold는 의미를 잃었다. 특수어는 약하고 드물었으나 스폰 템포는 이전 버전보다 크게 개선되었다.
- 설계 결론: Level Up 즉시 Augment 규칙은 유지하고 연속 초기 Augment는 EXP 요구 곡선으로 조정한다. 비전문화 뜰채가 후반에 자연스럽게 덜 쓰이는 것은 허용하며, 빌드 전문화와 선택한 빌드가 성숙할수록 편안해지는 진행을 의도한다.
- FULL RUN #2용 사용자 Inspector 값: Starting Gold 0, 낚싯대 Base Cost 30 / Cost Increase Per Rod 25, 그물 Base Cost 8 / Cost Per Unit Length 6 / Cost Increase Per Net 0.50, Special Fish Warning Time 4.0초, Boss Resistance 700, MiniBoss Resistance 280, Coast 일반 테스트 모드 비활성. 현재 직렬화 값과 기타 사용자 변경을 보존했다.
- 소스 패치: 실제 조업 시작부터 `Time.deltaTime`으로 Run Timer를 누적해 선택 UI의 `timeScale=0` 구간과 결과 화면을 제외하고 결과에 `MM:SS`로 표시한다. 동적 Hotbar의 실제 슬롯에서 낚싯대 개수/상한/다음 비용과 그물 개수/상한을 표시하며, 드래그 중 그물의 실제 계산 비용을 `예상 비용`으로 표시한다.
- EXP 요구량은 최초 20을 유지하고 이후 기존 `이전 요구량 × 1.35`에서 `Round(20 × 1.45^(현재 Level-1))`로 변경했다. 첫 Augment 시점은 유지하면서 이후 요구량이 더 빠르게 증가하는 1차 조정이다.
- Ambient 특수어 이벤트 비율을 성장 12%→20%, 특수 구간 22%→35%, Rush 18%→35%, Final 25%→40%로 높였다. 첫 복어·오징어 소개 시퀀스와 전체 어군 크기/구간 시간은 유지했다.
- 복어가 활성 그물에 처음 닿으면 그물을 3초간 비활성화하고 접촉 중인 물고기를 풀어 후속 어군이 통과할 시간을 만든다. 오징어가 쓰는 기존 그물 임시 비활성화 구조를 재사용했다.
- Codex는 Unity 실행, 컴파일, Console, Play Mode 및 자동 검증을 수행하지 않았다. 한 번의 실측 Run 뒤 적용한 1차 튜닝이므로 수동 검증이 필수이며 다음 단계는 FULL RUN #2다. commit/push하지 않았다.

## 최근 변경 — FULL RUN #2 전 수동 확인 후 보정
- 기존 Run Timer의 동일한 실제 경과 시간을 왼쪽 상태 HUD에 `플레이 시간: MM:SS`로 추가했다. 결과 화면 표시는 유지한다.
- EXP Curve를 OLD `20 × 1.45^(Level-1)`에서 NEW `50 × 1.30^(Level-1)`로 변경했다. Level Up 즉시 Augment 규칙과 Fish EXP, Augment gating/presentation은 유지하며 첫 Augment 직후 연속 Augment 현상을 FULL RUN #2에서 재확인한다.
- Net 예상 설치비는 상시 HUD/Hotbar가 아니라 기존 `NetCostText`를 사용해 배치 드래그 중에만 현재 드래그 끝점 근처에 표시한다. 실제 `CurrentPlacementCost`를 그대로 사용하며 Net Hotbar에는 현재/최대 설치 수만 유지한다.
- Unity 수동 검증이 필요하다. Codex는 Unity 실행, Computer Use, 자동 검증, commit/push를 수행하지 않았다.

## 최근 변경 — Level Reward 기반 초기 성장 전환
- `ExpText`는 다시 EXP 전용으로 복원했다. Live Run Timer는 `PrototypeHUDCanvas.runTimeText` 별도 TMP 참조로 분리했으며 Top Center 배치와 Scene Inspector 연결은 사용자가 수행해야 한다. 결과 화면 타이머와 기존 시간 계산은 유지한다.
- Level Reward는 Lv2=Tool Acquisition #1, Lv3=Tool Acquisition #2, Lv4+=Augment로 변경했다. EXP는 Run 시작부터 항상 획득하며 Level Up마다 해당 성장 보상을 즉시 연다. 선택 완료 뒤 기존 `ResolveLevelUp()`이 overflow를 이어서 처리한다.
- EXP 요구량은 Lv1→2 30, Lv2→3 80, Lv3→4 120, 현재 Level 4 이상은 `Round(120 × 1.30^(Level-3))`이다. Fish EXP는 변경하지 않았다.
- Coast 시간/구간 기반 Tool Acquisition 호출과 진행 기반 Augment 잠금은 제거했다. Augment 가능 여부는 Lv4 이상 및 Active Tool 2개 소유 조건으로 단순화했고 Job 흐름은 유지했다.
- 기존 `NetCostText`의 부모 Canvas local point를 앵커 기준 `anchoredPosition`에 잘못 적용해 화면 밖으로 밀리던 문제를 부모 피벗 기준 `localPosition` 적용으로 수정했다. 배치 중 실제 비용과 드래그 끝점 추적, 종료 시 숨김 동작은 기존 구조를 유지한다.
- Unity 수동 검증이 필요하며 다음 단계는 FULL RUN #2다. Codex는 Unity 실행, Computer Use, 자동 검증, commit/push를 수행하지 않았다.

## 최근 변경 — Vertical Slice 1차 전직 시점 조정
- 첫 Job Selection 요청을 첫 대형 어군 종료 시점에서 MiniBoss 실제 포획 직후로 이동했다. 첫 대형 어군은 초기 빌드의 첫 pressure test, MiniBoss는 중간 시험과 1차 전직 보상 역할을 맡는다.
- MiniBoss 포획이 확인된 경우에만 기존 `PrototypeJobManager` 선택을 요청하고, 선택 완료 뒤 Rush로 진행한다. Rush 이후 구간에서 전직 효과를 충분히 체험하도록 했다.
- Unity 수동 검증이 필요하며 다음 단계는 FULL RUN #2다. Codex는 Unity 실행, validation, commit/push를 수행하지 않았다.

## Full Run #2 및 후속 1차 조정
- 결과: 플레이 시간 10:21, 낚싯대+그물 빌드, 낚시꾼, Final Level 9, Gold 2226, 어획률 99.1%. 초반은 적당했고 중반과 연속 Augment 문제, Special Fish/복어는 개선되었다.
- MiniBoss 구간은 긴장감이 생겼으나 적극 공격 후 Resistance 약 20을 남기고 포획하지 못했다. 당시 구현은 이후 Job/Rush 진행을 허용했다. Rush 이전/초반에 저장 Gold로 낚싯대를 대량 설치해 Rod 12 + Net 3 최종 빌드가 너무 빨리 완성됐고 후반/Boss는 여전히 쉬웠다.
- 설계 결론: 낚시꾼의 높은 Rod 최대 수와 공격력은 유지하고 최종 빌드 완성 속도를 경제 곡선으로 조절한다. Boss/후반 추가 밸런스는 새 Rod 경제와 MiniBoss 관문 규칙을 확인한 뒤 결정한다.
- 낚싯대 가격을 선형에서 `Round(BasePlacementCost × GrowthMultiplier^CurrentActiveRodCount × 기존 Job 비용 배율)` 지수식으로 변경했다. Base Inspector 값 30을 유지하고 Growth Multiplier 기본값은 1.5다. Hotbar는 실제 다음 가격을 그대로 읽는다.
- MiniBoss는 Area 1 중간 관문이다. 본체 포획 성공 시 기존 보상 → Job 선택 → Rush로 진행한다. 본체 도주 또는 Encounter 시간 초과 시 `미니보스 포획 실패`로 Run Failure 처리하며 Job/Rush/Final/Boss를 중단한다. 지원 일반어 Escape는 이 실패 조건과 무관하다.
- 개발 Test Speed를 기존 `PrototypeGameFlowManager`의 단일 상태로 추가했다. Editor/Development Build에서 F1=x1, F2=x2, F3=x3 및 동일 public Button API를 제공한다. 선택 UI는 0으로 pause하고 종료 시 저장된 배속을 복원한다. 기존 Run Timer는 scaled `Time.deltaTime` 동작을 유지한다.
- Unity 수동 검증이 필요하며 다음 단계는 Full Run #3다. Codex는 Unity 실행, validation, commit/push를 수행하지 않았다.

## Full Run #3 후 입력 보조 및 실패 등급 보정
- 확인 결과 MiniBoss 포획 실패는 Run Failure로 정상 종료되었지만, 어획률 97.9%가 반영되어 실패 결과 등급이 S로 표시되었다. 전직 전 반복 LMB 입력의 피로도도 확인했다.
- 다음 Run용 사용자 Inspector 변경인 낚싯대 Base Max 2→3, MiniBoss Resistance 280→220을 확인했으며 해당 직렬화 값은 그대로 보존한다.
- 실제 Run Failure는 어획률 통계 계산과 표시는 유지하되 결과 등급을 항상 F로 표시한다. 성공 Run은 기존 어획률 등급 계산을 유지한다.
- 뜰채는 LMB를 누르고 있는 동안 기존 실제 쿨다운이 준비될 때마다 현재 커서 위치에 다시 사용된다. 기존 단일 클릭, 선택/결과/배치·재배치 포인터 예약 차단과 직업 자동 사용 구조는 유지하며 별도 쿨다운이나 완전 자동화는 추가하지 않았다.
- 입력 피로도를 낮추는 보조 변경이며 Unity 수동 검증이 필요하다. 다음 단계는 Full Run #4다. Codex는 Unity 실행, Computer Use, validation, commit/push를 수행하지 않았다.

## 최근 변경 — G1 Growth Foundation
- `RunManager`가 Run마다 하나의 `RunGrowthState`를 생성해 소유한다. 상태는 기존 `ToolId`를 재사용하며 Core/Partner에 서로 다른 Active Tool만 한 번씩 선택할 수 있고, 두 역할의 Tree 진행과 구매 노드 Rank를 분리해 보관한다.
- 공용 숙련 포인트의 미사용/사용 합계를 분리해 기록한다. 양수 지급, 잔액 확인, 부족 잔액 및 정수 오버플로를 거부하는 지출 API를 추가했다. 새 포인트는 기존 Level Up 흐름에 연결하지 않았으므로 G1 플레이 중에는 지급되지 않는다.
- `SkillTreeDefinition` ScriptableObject C# 타입과 Node/Rank/선행 노드/랭크별 비용/진행 조건/효과 식별 데이터 타입을 추가했다. 정적 Definition과 Run Rank 상태는 분리되며, Player Level·현재 Area·MiniBoss/Boss 완료 조건으로 현재 허용 Rank Cap을 계산할 수 있다. 실제 Definition `.asset`, 샘플 Tree, 노드 효과 적용은 만들지 않았다.
- 노드 Rank 구매 API는 선택 Tool·Core/Partner 역할·Tree ID·선행 Rank·Max Rank·현재 허용 Rank·포인트 잔액을 모두 확인한 뒤 포인트 지출과 Rank 증가를 한 번에 처리한다. 실패 시 포인트와 Rank를 변경하지 않는다.
- 미래 E/R은 `TacticalE`/`SignatureR` 슬롯별 해금 여부와 장착 Ability ID만 기록한다. Ability 정의·효과·쿨다운·입력·UI는 추가하지 않았다.
- 기존 `RunToolLoadout`은 현재 플레이의 Tool 소유/QWER 배치 source of truth로 그대로 유지된다. 새 Growth State는 G2 전까지 미선택·미연결 상태이며 Tool Acquisition, Dynamic Q/W/E/R, Augment, Job, EXP, Landing Net Hold, Pause/Test Speed, FishSpawner/MiniBoss/Boss 코드를 변경하지 않는다.
- 변경 파일: `Assets/Scripts/Core/RunGrowthState.cs`, `Assets/Scripts/Core/SkillTreeDefinition.cs`, `Assets/Scripts/Core/RunManager.cs`, `NETBREAK_STATE.md`. Scene/Prefab/기존 `.asset`/밸런스/Inspector 값은 변경하지 않았고 실제 Skill Tree asset도 생성하지 않았다.
- 정적 점검으로 `ToolId` 정의가 기존 한 곳뿐임, 기존 Level Up 보상 호출이 Lv2/Lv3 `ToolAcquisition` 및 Lv4+ `PrototypeAugmentManager`를 계속 사용함, 새 Growth API가 기존 Gameplay 코드에서 호출되지 않음을 확인했다. Unity 실행, 컴파일, Console, Play Mode, Computer Use는 요청에 따라 수행하지 않았다. commit/push하지 않았다.
- 다음 성장 단계는 G2 Core/Partner Acquisition 연결이다. G2 전까지 Lv2/Lv3 숙련 포인트 지급, Core/Partner 루트 비용 지출, Q/W 배정 전환은 의도적으로 미구현이다.

## 최근 변경 — G2+G3 Core/Partner 및 기능형 Skill Tree
- `RunManager`는 Lv2부터 레벨마다 Inspector의 `masteryPointsPerLevel`(기본 1)을 한 번만 지급한다. Lv2는 Core, Lv3은 Partner 필수 획득을 열고 Lv4+는 포인트만 지급한 뒤 EXP overflow 레벨업을 계속 처리한다. 기존 Lv4+ `PrototypeAugmentManager.ShowChoices()` 호출은 제거했고 legacy `AreAugmentsUnlocked`는 false로 고정해 우발적인 Random Augment 표시도 차단했다.
- `SkillTreeManager`가 최종 Tree 모달, 필수 획득, 후보 생성, 구매 검증, 효과 적용을 조정한다. Core 후보는 Fishing Rod/Net/Cast Net 3개, Partner 후보는 Bait와 Core를 제외한 나머지 도구 2개로 현재 정확히 3개다. 확정 시에만 기본 1P를 지출하고 Core는 Q(index 0), Partner는 W(index 1)에 같은 프레임의 검증된 연산으로 획득시킨다. 중복 Tool, 이미 찬 목표 슬롯, 부족 포인트는 상태를 바꾸지 않는다.
- 필수 획득 중 Tree는 닫기/Escape/Tab을 거부한다. 일반 탐색은 Tab 또는 UI API로 열고 닫을 수 있으며 닫을 때 기존 개발 배속 x1/x2/x3을 복원한다. `ToolSlotInput`이 열린 Tree를 월드 입력 차단 조건에 포함하므로 뜰채 Hold, 도구 입력, 배치/재배치의 클릭 누수를 취소·차단한다.
- 최종 UI용 `SkillTreeCanvas`, 좌우 `SkillTreeBranchView`, 재사용 `SkillTreeNodeView`, 해상도 독립 RectTransform 기반 `SkillTreePanZoom`을 추가했다. 공용 포인트, `?` 루트와 한국어 상태, 후보 3개, 도구별 전체 노드, 제목/설명/랭크/다음 비용/다음 효과/잠금 이유, 구매 가능 색상, 닫기 제한을 표시한다. UI Hierarchy와 직렬화 참조는 사용자가 Editor에서 연결해야 하며 Scene/Prefab YAML은 수정하지 않았다.
- 에셋 없이도 동작하는 내장 VS 정의 7개(Core 3 + Partner 4)를 추가했다. 각 Tool Tree에는 비용 1/2/3의 3랭크 수치 노드와 그 1랭크를 선행 조건으로 요구하는 보조 노드가 있다. 랭크 1은 Lv4, 랭크 2는 Lv6, 랭크 3은 해역 2에서 열리므로 미래 랭크가 보이면서 Area 1에서는 Cap으로 막힌다. 사용자가 만든 동일 Tool/Role `SkillTreeDefinition` 에셋은 내장 정의를 대체한다.
- 실제 효과: Fishing Rod 포획력/범위, Net 최대 길이/설치 상한, Cast Net 포획력/반경, Bait 유인 반경/지속시간. 기존 컨트롤러의 authoritative 증가 API를 재사용하며 효과 참조나 effect ID가 유효하지 않으면 포인트를 쓰기 전에 구매를 거부한다. 성공 구매 후 해당 랭크 효과만 한 번 적용하므로 Tree 재열기만으로 중복 적용하지 않는다.
- Tree로 이전된 legacy Augment 대응 효과는 FishingRodPower/Range, NetLength, CastNetPower/Radius, BaitRadius/Duration이다. LandingNet 3종, CastNetCooldown/FullHaul, FishingRodSpeed/ExtraHook은 소스만 보존된 휴면 상태다. 기존 MiniBoss Job 선택은 G4 전까지 유지하며 새 Core/Partner 상태를 변경하지 않는다.
- 새/변경 C#: `RunManager`, `RunGrowthState`, `RunToolLoadout`, `SkillTreeDefinition`, `ToolSlotInput`, `SkillTreeManager`, `VerticalSliceSkillTreeDefaults`, `SkillTreeCanvas`, `SkillTreeBranchView`, `SkillTreeNodeView`, `SkillTreePanZoom`. 문서: `Docs/NETBREAK_GROWTH_SYSTEM.md`, `NETBREAK_STATE.md`.
- Unity Editor를 실행하지 않았다. 기존 Unity Bee 응답 파일을 임시 출력 대상으로 사용한 Roslyn 소스 컴파일은 C# 오류 없이 통과했고, Editor 밖 compiler host와 Unity source generator 버전 차이 경고만 발생했다. 실제 Unity compile/Console/Hierarchy 참조/Play Mode는 사용자 검증이 필요하다. Scene/Prefab/기존 `.asset`, commit/push는 변경하거나 수행하지 않았다.
- G4 E/R, reroll/meta persistence, Tree art/animation, Area 2 실제 진행·콘텐츠, legacy 코드 삭제는 구현하지 않았다.

## 최근 변경 — G2+G3 Skill Tree UI Editor 생성기
- `Assets/Editor/SkillTreeUIGenerator.cs`에 `NETBREAK/UI/Generate Skill Tree UI` 메뉴를 추가했다. 활성 Scene의 유일한 `GameCanvas`와 `InputSystemUIInputModule` EventSystem을 확인한 뒤 SkillTreeUI root, 입력 차단 TreePanel, Header/포인트/공지/닫기, Core·Partner Viewport/Content/Branch/Node Container, 런타임 Node Template, Acquisition 3후보 Panel, Tree 열기 Button, Pan/Zoom을 한 번에 생성한다.
- 생성기는 현재 `SkillTreeCanvas`, `SkillTreeBranchView`, `SkillTreeNodeView`, `SkillTreePanZoom`의 실제 private serialized field를 `SerializedObject`로 자동 연결하고, Open Button persistent listener도 연결한다. SkillTreeUI root는 활성, TreePanel은 비활성으로 저장한다. `NanumGothic-Bold SDF`가 있으면 자동 사용하며 모든 화면 배치는 Canvas anchor/layout 기반이다.
- 동일 Scene에 완전한 `GameCanvas/SkillTreeUI + SkillTreeCanvas`가 이미 있으면 선택만 하고 종료한다. 이름 또는 컴포넌트만 남은 부분 구조, 중복 GameCanvas/SkillTreeCanvas, Input System EventSystem 부재는 사용자 UI를 덮어쓰지 않고 명시적 경고로 중단한다. 자동 실행하지 않으며 생성은 단일 Undo 그룹이고 성공 시 Scene을 dirty 처리한다.
- Unity/Computer Use는 실행하지 않았다. Editor 어셈블리 Roslyn 소스 컴파일은 오류 없이 통과했으며 실제 메뉴 실행, 생성 결과 Scene 저장, Play Mode UI/입력/해상도 검증은 사용자가 수행해야 한다. Scene/Prefab YAML, gameplay/balance, commit/push는 변경하거나 수행하지 않았다.

## 최근 변경 — G2+G3 Skill Tree UI/UX 폴리시
- `RunGrowthState.AvailableMasteryPointsChanged`와 `SkillTreeManager.OpenStateChanged` 이벤트를 추가했다. 포인트 지급·Core/Partner 획득 비용·노드 구매와 Tree 열기/닫기에만 발행하므로 별도 매 프레임 폴링 없이 UI가 즉시 반응하며 성장 수치와 규칙은 변경하지 않았다.
- `MasteryPointReminder`는 미사용 숙련 포인트가 1 이상이고 Tree가 닫혔을 때만 하단 중앙 Hotbar 위에 `숙련 포인트 N개 사용 가능! / Tab — 스킬 트리 열기`를 지속 표시한다. Tree가 열리거나 잔액이 0이면 숨고, CanvasGroup과 비 Raycast UI로 월드·도구·Hotbar 입력을 차단하지 않는다.
- 플레이어 노출 `Core/Partner`를 `주력/보조`로 변경했다. Branch 제목은 `주력 스킬 트리`/`보조 스킬 트리`, 필수 획득 제목과 안내는 `주력 도구`/`보조 도구`를 사용한다.
- Node Card는 설명에 전체 랭크 표를 반복하지 않고 노드 요약과 다음 효과만 표시한다. 현재 랭크, 다음 비용, 잠금 이유는 독립 영역으로 유지했다. 생성기 표준 Template의 높이·패딩·행간·각 TMP 영역을 넓혀 중첩을 줄였다.
- 기존 Scene용 `NETBREAK/UI/Apply Skill Tree UI Polish` 메뉴를 추가했다. 기존 SkillTreeUI를 삭제·재생성하지 않고 Reminder만 없을 때 추가하며, 알려진 Branch/Acquisition TMP와 Scene 내부 Node Template만 Undo 가능한 방식으로 갱신한다. 부분 Reminder, 누락 참조, Project Asset Template 등 모호한 구조는 경고 후 중단한다. 최초 `Generate Skill Tree UI`도 새 한국어 표기와 Reminder를 포함한다.
- Runtime과 Editor 어셈블리 Roslyn 소스 컴파일은 오류 없이 통과했다. Unity/Computer Use는 실행하지 않았으므로 기존 Main Scene에 폴리시 메뉴 적용, 저장, Play Mode 포인트 이벤트/비차단/가독성/해상도 검증이 필요하다. Scene/Prefab YAML, gameplay balance/progression, commit/push는 변경하거나 수행하지 않았다.

## 최근 변경 — G4-A Milestone Mastery 및 Legacy Job 효과 이전
- `RunManager`의 기존 Growth Rewards Inspector 소유자에 `miniBossMasteryReward` 기본 1과 `bossMasteryReward` 기본 2를 추가했다. `RegisterFishCaptured`가 실제 `FishSpecialType.MiniBoss/Boss` 포획 성공을 등록할 때 지급하므로 도주·실패·타임아웃에는 지급하지 않으며, Boss +2는 기존 결과 종료보다 먼저 `RunGrowthState`에 기록된다. 레벨당 `masteryPointsPerLevel` 기본 1과 EXP 요구량/레벨업 순서는 변경하지 않았다.
- `RunGrowthState`는 현재 해역(기본 1)과 해역별 MiniBoss/Boss 보상 완료 집합을 Run 범위로 보관한다. 동일 해역·동일 관문 보상은 첫 성공만 포인트를 더하며 반복 이벤트는 거부한다. 새 `RunManager`가 새 상태를 생성하므로 새 Run에서는 포인트와 지급 기록이 초기화된다. 미래 해역 전환용 `TrySetCurrentArea` 경계만 추가했으며 해역 2~6 콘텐츠는 만들지 않았다.
- 기존 Job gameplay 효과를 감사했다. 낚시꾼은 Main Scene 직렬화 기준 낚싯대 상한 3→12와 설치 비용 ×0.9, 그물잡이는 상한 10·비용 ×0.7·포획력 ×3, 투망꾼은 최대 충전 1→3, 뜰채잡이는 반경 ×2·대상 최소 8·자동 사용이었다. Job의 Random Augment 카테고리 가중치는 G3 이후 레벨업 Augment가 중단되어 휴면이다.
- 낚싯대에는 MiniBoss 이후 구매하는 `다중 낚싯대 운용` 3랭크(+3/+3/+3)와 `효율적인 낚싯대 설치` 2랭크(누적 비용 ×0.9)를 추가했다. 그물은 기존 `여분의 그물`에 +2/+4 후속 랭크를 더해 누적 상한 10을 보존하고, `효율적인 그물 설치` 2랭크(누적 ×0.7), `강화 그물코` 2랭크(누적 포획력 ×3)를 추가했다. 투망은 `여분의 투망` 2랭크(+1/+1 충전)를 추가했다. 모든 고급 랭크는 MiniBoss 포획과 선행 노드를 요구하며 자동 지급되지 않는다. 비용·효과·조건은 내장 기본 Definition 값이며 같은 Tool/Role의 `SkillTreeDefinition` 에셋으로 대체·조정할 수 있다.
- G4-B 전환 안전을 위해 기존 Job UI, 선택 상태, `HasAdvanced` 진행 대기와 호출 API는 유지하지만 Tool controller의 `Enable*Job` 메서드는 gameplay modifier를 적용하지 않는다. 따라서 현재 중간 빌드에서는 어떤 Job을 선택해도 즉시 효과가 없고, 옮겨진 효과는 해당 도구 Tree에서 포인트로 구매해야 한다. Landing Net은 Core/Partner Tree 대상이 아니므로 뜰채잡이 효과를 Tree로 옮기지 않고 자동 적용만 중단했다. 이는 최종 의도 경험이 아니라 G4-B에서 Job 선택을 E 선택으로 교체하기 전 임시 동작이다.
- Area 1 MiniBoss는 장기적으로 +1과 함께 E 3택1, Area 1 Boss는 +2와 함께 Core R을 해금한다. Area 2~3 MiniBoss/Boss는 숙련 보상과 아이템 신규 획득(기본 3택1), Area 4~6은 숙련 보상과 보유 아이템 업그레이드(4슬롯 완성 뒤 기본 2택1) 방향이다. Area 1 정상 플레이에는 Item이 없다. G4-A 뒤에도 E/R 해금·gameplay·UI와 Item 시스템은 미구현이다.
- 기존 Unity Bee 응답 파일을 사용한 Editor 외부 Roslyn 소스 컴파일은 오류 없이 통과했다. Unity 실행, Unity Compile/Console, Play Mode, Computer Use, Scene/Prefab 편집, commit/push는 수행하지 않았으며 실제 Unity 검증은 사용자에게 남아 있다.

## 최근 변경 — G4-B 기능형 E 전술 스킬
- Area 1 MiniBoss 포획 성공 뒤 기존 `PrototypeJobManager.RequestJobSelection()`/`HasAdvanced` 대기를 제거하고, `TacticalSkillManager`의 필수 E 3택 완료를 Rush 진입 조건으로 연결했다. G4-A의 MiniBoss 숙련 +1 지급과 중복 방지 기록은 그대로 사용하며 E 선택은 현재 숙련 포인트 잔액과 무관한 무료 원자적 해금·장착이다. 실패·도주·타임아웃에는 숙련/E를 지급하지 않는다.
- 후보는 선택된 주력 도구 E, 선택된 보조 도구 E, 범용 `집중 조업`으로 정확히 3개다. 낚싯대=`급속 릴링`, 그물=`긴급 봉쇄`, 투망=`비상 투망`, 미끼=`과잉 집어` 매핑을 사용한다. 기존 3선택 Canvas와 버튼을 재사용하고 리롤을 숨기며, 필수 선택은 Escape/Tab/닫기/배경 우회를 허용하지 않는다. 다른 필수 Tree/선택 모달이 열려 있으면 E 선택을 대기시키고 마지막 선택이 끝날 때까지 gameplay 배속을 복원하지 않는다.
- `급속 릴링`은 6초 동안 낚싯대의 유효 공격 속도를 ×2로 만든다(쿨다운 24초). 기존 Rod와 지속 중 배치한 Rod에 동일하게 적용하고 기본 `attackInterval`은 변경하지 않는다. `긴급 봉쇄`는 설치 그물을 재가동하고 6초 동안 포획 피해와 감속 강도를 ×1.75로 만든다(쿨다운 28초). 복어가 계속 겹치면 기존 방해가 다시 적용되며 영구 포획력/감속 수치는 변경하지 않는다.
- `비상 투망`은 E를 누르는 동안 조준하고 E를 떼면 현재 위치에 기존 투망 범위/포획력 판정을 한 번 실행하며, 일반 충전을 소비하거나 환급하지 않는다(확정 시 쿨다운 18초). `과잉 집어`는 7초 동안 미끼의 유효 유인 반경을 ×1.75로 만든다(쿨다운 22초). `집중 조업`은 E→LMB로 반경 2.5, 지속 6초의 구역을 만들고 구역 안에서 기존 `FishController.TakeCaptureDamage` 경로로 들어오는 Resistance 피해를 ×1.5 적용한다(쿨다운 26초). 별도 HP/재귀 피해 경로는 만들지 않았다.
- E는 전술 스킬 전용이며 Q/W만 Tool 입력으로 샘플링한다. R은 잠긴 별도 슬롯으로 남겨 G4-C로 연기했다. 비상 투망과 집중 조업은 RMB/Escape로 취소하며 쿨다운·충전·공격을 소비하지 않고, 비상 투망은 취소 뒤 E를 떼어도 발사하지 않는다. 지정 중 LMB 뜰채, Q/W 도구, 설치/재배치 입력을 예약·차단한다. 쿨다운/지속시간은 `Time.deltaTime` 기반이라 Pause 중 진행하지 않으며 선택 완료 후 기존 x1/x2/x3 개발 배속을 복원한다.
- Hotbar는 `[LMB][Q][W][E][R]`을 유지하되 E에 잠김/스킬명/준비/남은 쿨다운/대상 지정 상태를 표시하고 R은 `보스 보상` 잠금으로 분리했다. 구 Job HUD 텍스트는 숨기고, legacy Job Manager/UI/효과 API는 다른 참조를 위해 삭제하지 않았지만 정상 MiniBoss 흐름에서는 요청되지 않는다.
- 반복 튜닝 값은 `TacticalSkillManager` Inspector에 모았다. 기존 Scene을 직접 수정하지 않았으며 `NETBREAK/Growth/Setup Tactical Skill Manager` 메뉴가 유일한 RunManager에 컴포넌트를 추가하고 기존 도구 참조만 안전하게 연결한다. 이미 올바른 컴포넌트가 있으면 재사용하고, 중복/외부 컴포넌트가 있으면 중단하며 Undo와 Scene dirty를 지원한다. 메뉴를 실행하지 않아도 RunManager가 런타임 fallback 컴포넌트를 추가하지만 Inspector 튜닝값을 저장하려면 메뉴 실행 후 Scene 저장이 필요하다.
- Scene/Prefab/YAML, 기존 Tool/Fish/EXP/Mastery/Gold/Tree 수치와 Boss 결과 흐름은 수정하지 않았다. 기존 사용자 변경인 `Assets/Scenes/Main.unity`는 보존했다. Unity/Computer Use/Play Mode/Console/commit/push는 수행하지 않았다. 기존 Bee 응답 파일을 복사한 임시 Roslyn runtime/editor 소스 컴파일은 오류 없이 통과했으며 실제 Unity compile, 메뉴 실행·참조 확인, 전체 MiniBoss→E→Rush/Final/Boss 흐름과 다섯 E 효과·취소·쿨다운·배속·새 Run 초기화는 사용자 수동 검증이 필요하다. R 및 Item은 각각 G4-C/후속 단계로 연기한다.

## 최근 변경 — G4-C1 Core Signature R

- Area 1 Boss 포획 등록 시 기존 해역별 1회 숙련 +2 지급을 먼저 유지하고, 지급 성공 뒤 선택된 Core에 따라 R을 자동 해금·장착한다. 낚싯대=`어장 대횡단`, 그물=`교차 봉쇄`, 투망=`천망`이며 첫 Boss에서 선택 UI는 없다. 한국어 해금 공지를 2초 표시하고 기존 결과 화면으로 이어진다. R 해금/장착 ID는 `RunGrowthState.SignatureR`에 저장되어 새 Run에서 초기화된다.
- `SignatureSkillManager`가 공통 90초 쿨다운, 실제 Camera pixel viewport 안의 HUD 제외 조업 영역(기본 normalized x=0/y=0.12/w=1/h=0.72), 세 R의 모든 튜닝값과 런타임 placeholder visual을 단일 Inspector에서 소유한다. E와 R 쿨다운/대상 지정/지속 상태는 독립이다. 쿨다운·지속시간·연속 시전 간격은 scaled gameplay time을 사용하므로 Pause에서 멈춘다.
- `어장 대횡단`은 기본 9개 Lane의 부표가 우→좌로 2.5초 동안 동시에 1회 횡단한다. 한 횡단당 각 물고기는 HashSet으로 1회만 피해를 받으며 새 횡단에서 다시 대상이 된다. 기본 피해 Normal 25/Pufferfish 18/Squid 24/MiniBoss 55/Boss 70이다. 실제 Area 1 최대 Resistance 일반 25(참치; 정어리 5, 고등어 10), 복어 18, 오징어 24를 기준으로 일반·일반 특수어를 1회 포획하고 MiniBoss 220/Boss 700은 즉시 포획하지 않게 정했다. 설치 낚싯대가 없어도 동작한다.
- `교차 봉쇄`는 실제 판정 폭 0.55의 X를 기본 6초 유지한다. 접촉 이동 배율 Normal/Pufferfish/Squid 0, MiniBoss 0.5, Boss 0.65와 최대 Resistance 초당 피해율 3%/3%/3%/1.5%/0.75%를 사용한다. 기존 Net·특수 이동 배율과 곱해 적용하고 접촉 종료, 효과 만료, 비활성화 및 Run 종료에 임시 배율을 제거한다. 두 X 팔이 겹쳐도 프레임당 물고기 1회만 피해를 적용한다.
- `천망`은 R Hold 조준/Release 시전, RMB/Escape 취소다. 기본 지름은 조업 영역 높이 100%, 기존 투망 `capturePower`에 ×1 피해, 1회 시전이며 일반 투망 충전을 소비하지 않는다. 연속 시전 준비값은 1회/0.6초 간격이고 각 후속 시전 시 현재 커서를 다시 읽는다. 한 시전의 Collider 중복은 HashSet으로 제거한다.
- Hotbar R은 잠금/스킬명/조준/준비/남은 쿨다운을 표시한다. R 조준 중 월드 포인터와 Q/W/LMB 충돌을 막고, Skill Tree·선택 모달·Boss 보상 공지·결과/실패 및 기존 차단 상태에서는 활성화하지 않는다.
- 기존 `NETBREAK/Growth/Setup Tactical Skill Manager`가 같은 RunManager에 `SignatureSkillManager`를 안전하게 추가하고 기존 Cast Net 참조만 연결하도록 확장됐다. 중복/외부 R 매니저가 있으면 변경 전 중단한다. 메뉴 미실행 시 RunManager 런타임 fallback이 컴포넌트를 추가한다. 개발 검증은 Editor/Development Build Play Mode에서 Core 선택 뒤 컴포넌트 Context Menu `Development/Unlock Core Signature R`을 사용하며 출시 빌드에서는 변경하지 않는다.
- Scene/Prefab/YAML, 기존 FishData/도구/EXP/Gold/Tree 수치, E 동작, Boss 다중 회유는 수정하지 않았다. Unity/Computer Use/Play Mode/Console/commit/push는 수행하지 않았다. 기존 Bee 응답 파일을 복사한 임시 Roslyn runtime/editor 소스 컴파일은 오류 없이 통과했다. 실제 Unity compile/Console, Setup 메뉴 저장, 세 Core별 개발 해금·입력·시각/판정·취소·Pause/x1/x2/x3·Boss 공지→결과·새 Run 초기화는 사용자 검증이 필요하다. E/R 강화 노드와 구매는 G4-C2로 연기한다.

## 최근 수정 — Fish Pool 고갈 방지

- G4-C1 수동 검증 중 반복된 `FishSpawner: 비활성 Fish가 부족합니다.`는 Editor.log 스택상 Rush/Final Ambient와 최종 대어군의 `SpawnSchoolMembers → SpawnFish`에서 고정 200개 풀이 모두 활성일 때 발생했다. 기존 처리는 재시도나 지연 없이 해당 개체를 즉시 누락했고, 같은 어군의 남은 개체마다 경고를 반복했다.
- 초기 풀 200은 유지하고, 부족할 때 20개 단위로 최대 320까지 확장하는 bounded pool을 `FishSpawner`에 추가했다. 무제한 Instantiate는 하지 않으며 상한 도달 뒤에는 기존처럼 스폰을 누락하되 경고는 프레임당 한 번으로 합쳐 현재/최대 크기를 표시한다. 세 값은 기존 FishSpawner Inspector의 Pool 구역에서 조절한다.
- Fish는 포획(`FishController.Capture`) 또는 경로 Destination 도달(`FishMovement.ReachDestination`) 시 `SetActive(false)`되고 같은 풀 항목으로 재사용된다. G4-C1의 세 R도 기존 `TakeCaptureDamage → Capture` 경로를 사용하므로 별도 반환 누수는 확인되지 않았다.
- 물고기 수, 구간별 스폰 요청량/간격, 경로, 난이도는 변경하지 않았다. Scene/Prefab YAML, Unity 실행, commit/push는 수행하지 않았다. Roslyn 소스 컴파일과 diff 정적 검증 뒤 Unity에서 Rush/Final 동시 활성 수와 경고 재발 여부를 확인해야 한다.

## 최근 변경 — G4-C2 E/R 강화와 Compact Skill Tree Graph

- `RunGrowthState`의 기존 E/R 상태에 안정적인 Tree/Node Rank 저장과 원자적 구매를 확장했다. Q/W/E/R은 기존 공용 숙련 포인트 잔액을 사용하며, 장착 Ability ID·해금·Rank Cap·선행 조건·잔액을 모두 통과한 경우에만 한 번 차감하고 한 랭크를 기록한다. UI에는 짧은 실시간 구매 간격 잠금도 있어 더블클릭이 다음 랭크까지 연속 구매하는 것을 막는다. 새 Run은 새 `RunGrowthState`와 함께 E/R 해금·랭크를 초기화한다.
- E는 선택된 스킬에만 독립 단일 랭크 `효과` 1P와 `재사용` 2P를 제공한다. Main Scene 직렬화 기본값 기준 유효값은 급속 릴링 ×2→×2.5/24→20초, 긴급 봉쇄 ×1.75→×2/28→24초, 비상 투망 반경 ×1→×1.2/18→15초, 과잉 집어 ×1.75→×2.1/22→18초, 집중 조업 피해 ×1.5→×1.8/26→22초다. 비상 투망은 실제 `CastNetController` 판정 반경을 확장하되 일반 충전을 소비하지 않고 기존 Hold/Release/RMB·Escape 취소를 유지한다.
- R은 Core에 해당하는 정의만 사용한다. 어장 대횡단은 `횡단` 2P/3P로 1→2→3회, 교차 봉쇄는 독립 `지속`과 `저항 피해`가 각각 2P/3P로 기본 지속 ×1→×1.333→×1.667 및 기본 DPS ×1→×1.33→×1.67, 천망은 `연속 투망` 2P/3P로 1→2→3회다. 0.6초 간격, 각 후속 시전의 현재 커서, 횡단별 적중 dedup, 범주별 피해·이동 회복은 기존 경로를 유지한다. 개발용 `Development/Unlock Core Signature R`과 실제 Boss +2/해금 흐름도 유지했다.
- E/R Manager의 Inspector 값이 authoritative baseline이며 Definition은 상대 조정값과 비용만 가진다. 구매는 직렬화 필드를 변경하지 않는다. E/R 모두 발동 시 유효 횟수·배율·지속시간을 스냅샷하므로 진행 중 쿨다운 및 활성 임시 효과를 Tree 구매가 소급 변경하지 않는다.
- `SkillTreeUI`는 Q/W/E/R 4영역의 112px Compact Node, Rank Badge/잠금 상태, 실제 prerequisite 선, Pan/Zoom, 공용 Hover Tooltip을 지원한다. Tooltip은 전체 한국어 이름·설명·현재/다음 실제 효과·랭크·비용·선행 조건·잠금/포인트 부족/최대 랭크를 표시하고 Raycast를 차단하지 않으며 화면 경계 안에 고정한다. 노드/선은 Definition 변경 때만 재구성하고 매 프레임은 상태만 갱신한다.
- 명시적 Editor 메뉴 `NETBREAK/UI/Migrate Skill Tree UI To Compact Graph`가 기존 UI를 보존하며 Q/W를 상단, E/R을 하단으로 재배치하고 E/R Branch·공용 Tooltip을 추가한다. 부분 구조·중복·외부 Template이면 중단하고 Undo·재실행을 지원한다. 별도 Inspector 할당은 없으며 실행 후 Scene 저장은 사용자가 한다.
- Scene/Prefab/YAML, FishSpawner, 물고기/도구 기본 밸런스, EXP/Gold/보상 수치는 수정하지 않았다. Unity/Computer Use/Play Mode/Console/메뉴 실행/Scene 저장/commit/push는 수행하지 않았다. 기존 Bee 응답 파일을 사용한 외부 Roslyn runtime/editor 컴파일은 오류 없이 통과했으며 source generator 버전 경고만 있었다. 실제 Unity Compile/Console, 마이그레이션 시각 확인, Q/W 획득 회귀, 각 E/R 구매·효과·Pause/취소/새 Run 초기화는 사용자 검증이 필요하다. FishSpawner bounded pool의 Rush/Final 런타임 회귀 확인도 여전히 필요하다.

### G4-C2 첫 UI 검증 보정

- 첫 Unity UI 검증에서 후순위로 추가된 E/R Branch가 기존 Core/Partner `AcquisitionPanel`보다 위에 렌더링되고, 비활성 공용 Tooltip을 첫 Hover로 활성화할 때 `Awake → HideAll`이 `Show`가 설정한 anchor를 지워 `PositionPanel` NullReferenceException이 발생하는 것을 확인했다. Tooltip 초기화를 활성 상태와 분리하고 null 수명 방어를 추가했으며, 제목 21/body 17/정보 15 크기와 430px 가변 높이·화면 경계 보정을 적용했다.
- 기존 `NETBREAK/UI/Migrate Skill Tree UI To Compact Graph` 메뉴는 이미 이관된 Scene을 재사용해 전면 `AcquisitionModalBlocker`, Branch CanvasGroup 입력 차단, Acquisition 최상위 sibling, Tooltip 가독성 설정과 작은 112×88 잠금 Root를 보정한다. 외부 Tool/E/Job 선택 모달 동안 `SkillTreeUI` 전체를 뒤로 보내 기존 전체 화면 raycast blocker가 항상 우선한다. 성장 데이터·구매·효과·비용은 변경하지 않았으며 Unity 실행과 Scene 저장은 사용자 검증으로 남아 있다.

## 최근 변경 — G5-A Passive Item Foundation

- `RunGrowthState.ItemInventory`가 한 Run의 유일한 아이템 소유/레벨 상태다. `RunItemInventory`는 4칸 상한, 안정 ID 기반 중복 방지, 소유 조회, 남은 슬롯 수와 원자적 Lv1 획득 API를 제공한다. 새 Scene Run은 새 `RunGrowthState`를 만들므로 소유 상태가 빈 4칸으로 초기화되고, 같은 Run의 미래 Area 이동에서는 이 상태를 그대로 유지할 수 있다. Definition은 불변 카탈로그, Run 소유는 별도 객체이며 UI에는 소유 상태를 저장하지 않는다. G5-A에는 효과 런타임 상태가 없고 G5-B에서 소유 데이터와 분리해 추가한다.
- 최초 카탈로그는 전기 `storm_orb` 폭풍 구슬·`capacitor_coil` 축전 코일, 검 `spectral_scabbard` 유령 검집·`autonomous_sword_array` 자동 검진, 얼음 `frost_sigil` 서리 인장·`frost_crystal` 빙결 결정 6종이다. 한국어 계획 효과 설명을 포함하지만 실제 패시브 효과는 구현하지 않았다.
- `ItemRewardManager`는 Inspector의 단일 `acquisitionCandidateCount`(기본 3)를 사용해 전체 카탈로그에서 미보유 후보를 뽑고 중복 없이 섞는다. 후보 부족 시 실제 후보만 표시한다. 선택은 무료이며 더블클릭/반복 callback guard 뒤 정확히 하나만 획득한다. 4칸이 찬 개발 요청은 모달을 열지 않고 `아이템 업그레이드 보상은 G6`라는 한국어 진단으로 거절하며 Acquisition/Upgrade 요청 타입 경계를 분리했다.
- 필수 Item 모달은 열릴 때 기존 `Time.timeScale=0` 경계를 사용하고 `ToolSlotInput`, Skill Tree, Tactical E와 Skill Tree Canvas 정렬의 기존 모달 검사에 통합했다. 따라서 뜰채, Q/W, E/R, 재배치와 다른 모달 전환이 막힌다. Escape/Tab/배경에는 닫기 경로가 없고 선택 완료 뒤 기존 `ResumeGameplayTimeScale`로 F1/F2/F3 배속을 복원한다. 정상 Area 1 MiniBoss/Boss 보상에는 연결하지 않았다.
- `ItemHUD`는 빈 칸이 구별되는 4슬롯, 짧은 한국어 이름, 전기/검/얼음, Lv1을 표시한다. Hover Tooltip은 전체 이름·속성·레벨·계획 효과와 `G5-B 예정` 개발 상태를 보여준다. `ItemRewardCanvas`는 후보별 이름·속성·계획 효과·획득 가능 여부를 표시한다. `NETBREAK/UI/Setup Item System UI`가 유일한 `GameCanvas`와 `RunManager`를 검사해 `ItemSystemUI`와 `ItemRewardManager`를 Undo 지원으로 만들며, 기존/부분/중복 구성을 안전하게 감지한다. 별도 Inspector 할당은 없고 실행 뒤 Scene 저장은 사용자가 한다.
- 개발 검증은 Editor/Development Build Play Mode의 진행 중인 Run에서 RunManager의 `ItemRewardManager` Context Menu `Development/Open Item Acquisition Reward`를 호출한다. 실제 3택 모달을 거치며 다른 필수 선택 중에는 거절되고, 네 번까지 서로 다른 아이템을 얻고 다섯 번째는 G6 연기 진단으로 안전하게 거절된다. 자동 지급이나 출시 UI는 없다.
- 전투 이벤트 정적 감사: 뜰채 `LandingNetController.CaptureFish`와 연쇄 포획, 낚싯대 `FishingRodController.Attack`, 그물 `NetController.OnTriggerStay2D` DoT, 일반 투망 `CastNetController.PerformCast`, E 비상 투망의 `ApplyTacticalCast` 경로, R 어장 대횡단·교차 봉쇄의 직접 호출 및 천망의 `ApplySignatureCast`가 모두 `FishController.TakeCaptureDamage`로 끝난다. 그러나 연속 피해 dedup·Tool/Ability/Item 원천·범위 공격 단위가 현재 호출마다 달라 G5-A에서 이벤트/피해 시그니처를 추측해 넣지 않았다. G5-B에서 양의 실제 피해만 카운트하고 Tool 원천과 Item 원천을 구분하며, 같은 연속 원천/대상 0.5초 제한·한 범위 공격 대상 dedup·비재귀 Item 포획을 이 실제 진입점에 연결한다. `FishController.Initialize`는 포획 구독을, `FishMovement.Initialize*`/`OnDisable`은 이동 modifier를 초기화하므로 미래 물고기별 Item 카운터·상태도 같은 Pool 초기화 경계에서 지워야 한다.
- 전기/검/얼음 합산 레벨과 단일 속성/조합 시너지 기반을 설계했다. 당시 3단계 초안은 아래 최신 G6-B1 재설계에서 **2/4/6/8/10 독립 패시브**로 교체되었다. 서로 다른 두 속성 Lv2 이상·Run당 최대 하나·명시 조합만 허용하는 조합 시너지와 비재귀 규칙은 유지한다.
- Scene/Prefab YAML, Area 1 보상, 기존 Tool/E/R/Tree/EXP/숙련/Gold/FishSpawner/포획 보상/배속은 변경하지 않았다. Unity/Computer Use/Play Mode/Console/메뉴 실행/Scene 저장/commit/push는 수행하지 않았다. Unity 6 참조 어셈블리를 사용한 외부 runtime/editor C# 컴파일은 오류 없이 통과했다(기존 직렬화 필드 경고만 발생). 실제 Unity Compile/Console, UI 배치, 모달 차단, 배속 복원, 새 Run 초기화는 사용자 검증이 필요하다.

## 최근 변경 — G5-B 6종 패시브 아이템 효과와 전투 이벤트 통합

- `FishController.TakeCaptureDamage`에 호환 오버로드를 유지한 채 `CombatDamageContext/Result`를 연결했다. 실제 적용된 양의 Resistance 피해, Tool/Item 원천, 공격 ID, 연속 소스 instance ID, 저장된 적중 위치, 해당 피해의 포획 여부와 Pool lifecycle version을 `ItemEffectManager`에 전달한다. Resistance 차감과 기존 `Capture → RunManager.RegisterFishCaptured` 보상은 한 번만 실행되고 호출한 Tool의 범위 판정·환급·연쇄 처리가 끝난 뒤 같은 프레임 `LateUpdate`에서 Item 반응을 처리한다. 따라서 포획·비활성화 뒤에도 저장된 위치로 축전 코일을 처리하면서 기존 Tool 후처리를 먼저 보존하고, 마지막에 물고기별 상태를 정리한다. Item 원천은 같은 피해 파이프라인과 포획 보상을 사용하지만 Manager가 Tool 원천만 조건으로 받아 재귀 체인을 막는다. 집중 조업의 피해 배율은 기존 의미대로 Tool 원천에만 적용한다.
- 뜰채와 연쇄 포획, 낚싯대, 설치 그물 DoT, 일반 투망, E 비상 투망, R 어장 대횡단·교차 봉쇄·천망의 실제 호출부에 공격 메타데이터를 명시했다. 뜰채/투망 범위 공격은 한 공격 안에서 `HashSet<FishController>`로 중복 Collider를 제거한다. 그물과 교차 봉쇄는 실제 피해 주기를 그대로 유지하면서 동일 물고기·공격 ID·컴포넌트 인스턴스마다 기본 0.5초에 한 번만 아이템 적중으로 센다. 서로 다른 Net 인스턴스와 별도 공격은 독립적으로 센다.
- `RunGrowthState.ItemInventory`가 계속 유일한 소유권 원천이다. RunManager의 단일 `ItemEffectManager`가 Inventory Changed를 한 번만 구독하여 진행 중 획득도 즉시 활성화한다. 미보유 아이템은 타이머·카운터·공격을 만들지 않는다. 새 Run/Run 종료에서는 타이머, 전역 및 물고기별 카운터, DoT 기록, 둔화, 임시 visual을 지운다. 포획·Destination 도달·기타 비활성화·Pool 재초기화에서는 해당 물고기 참조와 DoT 기록을 지워 재사용 누수를 막는다.
- 폭풍 구슬 기본값은 8초/Resistance 12/visual 0.35초다. 조업 viewport 안에서 커서에 가장 가까운 활성 물고기를 고르며 Mouse가 없으면 viewport 중앙을 쓴다. 축전 코일은 Tool 적중 5회, 반경 3, 원래 적중 대상을 제외한 추가 대상 최대 2, 각 Resistance 8, visual 0.4초다. 마지막 Tool 적중이 원래 물고기를 포획해도 저장 위치를 사용한다.
- 유령 검집은 같은 물고기 Tool 적중 4회마다 Resistance 16, visual 0.35초이며 포획된 원래 대상은 다시 공격하지 않는다. 자동 검진은 10초마다 현재 Resistance가 가장 높은 적격 물고기에게 Resistance 18, visual 0.45초다. Resistance/거리 동률은 instance ID로 결정한다.
- 서리 인장은 같은 물고기 Tool 적중 3회마다 2초간 40% 둔화한다. 빙결 결정은 12초마다 커서 반경 3의 가까운 대상 최대 2마리에 Resistance 5와 3초간 30% 둔화를 주며 영역 visual은 0.5초다. 두 둔화는 `FishMovement`의 Item별 시간제 modifier로 같은 효과 재적용 시 지속시간을 갱신하고 서로 다른 Item 둔화 중 가장 강한 값만 사용한다. 기존 Net·특수어·R 이동 배율과 별도로 곱하므로 Net 정지를 풀거나 만료 시 다른 감속을 지우지 않는다. None/Pufferfish/Squid/MiniBoss/Boss에 같은 bounded 비율을 적용하며 보스도 영구 정지시키지 않는다.
- 모든 주기 효과는 scaled `Time.deltaTime`, visual 수명은 `WaitForSeconds`를 사용한다. 대상이 없으면 발동을 적립하지 않고 전체 정상 주기를 다시 기다린다. F1/F2/F3 배속을 따르고 `timeScale=0`에서 정지한다. 런타임 placeholder는 전기 LineRenderer, 검 모양 LineRenderer, 얼음 원형 LineRenderer이며 gameplay와 분리되어 생성 실패가 피해를 막지 않는다. Manager가 수명을 추적하고 Run 종료/비활성화 때 전부 정리한다.
- Item tooltip/reward 문구를 실제 한국어 효과와 피해량으로 갱신하고 G5-B 예정 문구를 제거했다. Hover 중 주기 남은 시간, 축전 코일 전역 적중 수, 대상별 발동 기준, 현재 둔화 수를 갱신한다. 기존 4슬롯, 필수 reward modal의 sorting/raycast/pause, Q/W/E/R Tree UI는 변경하지 않았다.
- 모든 G5-B 밸런스의 유일한 Inspector 소유자는 RunManager의 `ItemEffectManager`다. 공통 `gameplayViewport=(0, 0.12, 1, 0.72)`와 `continuousHitCountInterval=0.5` 및 각 Item Header의 주기/피해/반경/대상 수/둔화/visual 값을 조절할 수 있다. `NETBREAK/UI/Setup Item System UI`를 다시 실행하면 기존 Item UI와 ItemRewardManager를 보존하고 누락된 ItemEffectManager만 Undo 지원으로 추가한다. 별도 참조 할당은 없다. 실행하지 않아도 RunManager가 런타임 누락 컴포넌트를 한 개만 보완하지만 Inspector 튜닝과 Scene 저장을 위해 메뉴 실행이 권장된다.
- 정상 Area 1 아이템 0개와 개발용 `ItemRewardManager > Development/Open Item Acquisition Reward`, 4슬롯/중복 방지, MiniBoss/Boss 숙련, E/R, 기존 피해·보상·경제·Spawner 수치는 유지했다. Scene/Prefab YAML, Unity/Computer Use/Play Mode/Console/메뉴 실행/Scene 저장/commit/push는 수행하지 않았다. Unity 6000.3.11f1 참조와 기존 Bee response를 사용한 외부 runtime/editor Roslyn 컴파일은 오류 없이 통과했다. 실제 Unity Compile/Console과 아래 6종 개별/복수 아이템, Pause/배속, capture/escape/pool 회귀는 사용자 검증이 필요하다.
- G5-B 완료 시점에는 아이템 레벨 업그레이드와 속성 레벨 합산도 미구현이었다. 아래 G6-A1에서 두 기반을 추가했고, 최신 단일 속성 단계는 2/4/6/8/10이다.

## 최근 변경 — G6-A1 아이템 레벨·실제 스케일링·속성 합산 기반

- `RunGrowthState.ItemInventory`가 기존과 동일하게 Run별 아이템 소유권과 현재 Level의 유일한 source of truth다. 획득은 항상 Lv1이고 `TryUpgrade`는 안정 ID, 실제 소유, 유효한 현재 레벨, 유한 최대 레벨, `int` overflow를 검사한 뒤 선택 아이템 하나만 정확히 Lv+1 한다. 실패는 `ItemUpgradeResult`로 이유를 반환하고 Inventory·레벨·런타임 효과를 변경하지 않는다. `ItemEffectManager.CanUpgradeItem/TryUpgradeItem`이 Inspector 설정을 사용하는 G6-A2용 authoritative 진입점이다.
- 모든 아이템의 기본 유한 최대 레벨은 5다. RunManager의 기존 단일 `ItemEffectManager`에서 각 아이템 Header마다 `Maximum Level`, `Unlimited Maximum Level`을 설정한다. 무제한은 유한 최대값을 무시하지만 `int.MaxValue` 증가는 차단한다. 0/음수 최대값은 안전하게 1로 정규화하며 무제한을 암묵적으로 켜지 않는다. 최대값을 현재 레벨보다 낮춰도 소유/현재 레벨은 유지하고 추가 업그레이드만 막는다.
- 기존 Main Scene 직렬화 기준값을 변경하지 않았다. 폭풍 구슬 12·축전 코일 8·자동 검진 18은 레벨당 Lv1 기준 피해 +20%, 유령 검집 16은 +25%를 가산한다. 서리 인장 2초와 빙결 결정 3초는 레벨당 둔화 지속시간 +0.4초를 가산한다. 빙결 결정 피해 5, 둔화율, 주기, 필요 적중, 반경, 대상 수는 그대로다. 각 증가량은 같은 아이템 Header의 `Damage Bonus Per Level` 또는 `Duration Per Level`에서 조절한다. 기준 직렬화 필드를 덮어쓰지 않고 발동 순간 계산하므로 완료된 피해와 이미 활성인 둔화에는 소급하지 않으며 다음 발동부터 새 레벨을 사용한다.
- `GetElementLevel(Electric/Sword/Ice)`는 실제 소유 아이템 Level을 Inventory에서 매번 합산하고 별도 mutable 카운터를 만들지 않는다. 획득·업그레이드를 즉시 반영하고 새 Scene Run의 새 `RunGrowthState`에서는 소유/레벨/속성 합계가 0으로 초기화된다. 기존 `ItemEffectManager`의 Run 종료/비활성화 정리는 타이머, 적중 카운터, DoT 기록, 둔화, 임시 visual을 지운다.
- 개발 검증은 Editor/Development Build의 진행 중인 Run에서 실제 아이템 획득 후 `ItemEffectManager > Development/Upgrade First Eligible Owned Item`을 실행한다. 실제 `CanUpgradeItem/TryUpgradeItem`을 사용하고 최대 레벨을 존중하며 정상 플레이 UI나 자동 지급은 추가하지 않았다. Item HUD tooltip 상태에는 현재 유효 피해 또는 둔화 지속시간이 표시된다.
- `Assets/Editor/Tests/ItemProgressionTests.cs`에 Lv1 획득, 정확한 +1, 미소유 실패, 기본/사용자 최대값, 무제한, 잘못된 최대값, 피해/둔화 가산식, 속성 합산, 새 Run 초기화, 실패 불변, 6종 기본 최대값, Manager가 업그레이드 레벨을 실제 유효값에 사용하는 경우를 다루는 Edit Mode 테스트를 추가했다.
- Unity와 Test Runner는 요청에 따라 실행하지 않았다. Unity 6000.3.11f1의 기존 Bee 응답과 Roslyn으로 새 runtime/test 소스를 함께 컴파일한 source-level 검사는 오류 없이 통과했다. 이는 Unity Compile, Console 확인, Edit Mode 테스트 실행, Play Mode 수동 검증을 뜻하지 않는다. Scene/Prefab YAML, 기존 Tool/E/R/Tree/Fish/EXP/Gold/Mastery/FishSpawner 밸런스, 정상 Area 1 아이템 0개 정책, commit/push는 변경하거나 수행하지 않았다.
- 후속 범위였던 G6-A2는 아래와 같이 소스 구현했다. G6-B 단일 속성 시너지는 최신 2/4/6/8/10 설계를 따르며, G6-C의 서로 다른 두 속성 Lv2 이상·명시 조합·Run당 최대 하나 규칙은 유지한다.

## 최근 변경 — G6-A2 필수 2택1 아이템 업그레이드 보상

- `ItemRewardManager.RequestUpgrade`를 미래 해역 관문용 명시적 보상 진입점으로 추가했다. 기존 획득과 업그레이드 요청 타입을 분리했고, 4칸이 찬 일반 획득 요청은 다섯 번째 아이템을 만들거나 조용히 업그레이드로 전환하지 않는다. 정상 해역 1 MiniBoss/Boss에는 아이템 보상을 연결하지 않았고 숙련 +1/+2 및 E/R 흐름도 변경하지 않았다. 실제 해역 2~6 관문 연결은 G8 범위다.
- `Upgrade Candidate Count` 기본값 2를 `ItemRewardManager`의 유일한 후보 수 설정으로 추가했다. 후보 생성은 실제 `RunItemInventory.OwnedItems`만 순회하고 `ItemEffectManager.CanUpgradeItem(itemId, inventory, ...)`로 G6-A1 최대 레벨·무제한·overflow·ID·소유 검사를 재사용한다. 적격 풀 Fisher-Yates shuffle 후 필요한 수만 취해 중복·희소 풀 무한 반복이 없다. 2개 이상은 기본 2택, 1개는 가운데 1택, 0개는 모달을 열지 않고 `NoEligibleCandidates`와 한국어 경고를 반환한다. Gold·숙련 대체 보상이나 자동 강화는 없다.
- `ItemUpgradeRewardState`가 한 보상에 제시된 정확한 후보 ID와 pending/consumed/confirmation 상태를 소유한다. 클릭 시 pending 확인 → 제시 후보 확인 → 현재 소유/자격 재검증 → G6-A1 `TryUpgradeItem` → 성공 후 소비 순서다. 실패는 아이템 레벨과 pending 보상을 유지하고, 더블 클릭·반복 확인·비제시 아이템·한 보상으로 두 번 강화는 차단한다. 완료 뒤 새 `RequestUpgrade`는 새 보상을 정상 생성하며 Run당 1회 제한은 없다. Inventory 참조가 새 Run 상태로 바뀌거나 Manager가 비활성화되면 pending 상태를 초기화한다.
- 기존 `ItemRewardCanvas`와 카드 3개를 획득/업그레이드가 공유한다. 업그레이드 카드는 실제 한국어 이름·속성·`Lv.N → Lv.N+1`·현재/다음 실제 주 효과값·`속성 레벨 +1`을 표시한다. 실제 값은 Main Scene의 기존 ItemEffectManager 기준값과 G6-A1 레벨 스케일링 API에서 계산하며 예시 숫자를 하드코딩하지 않는다. 2/1개에 맞춰 카드를 가운데 재배치하고 빈 카드는 숨긴다.
- `ItemHUD`는 Inventory Changed를 구독해 성공 프레임에 슬롯 Lv와 hover tooltip을 갱신한다. `NETBREAK/UI/Setup Item System UI`를 다시 실행하면 기존 `ItemSystemUI`, 획득 UI, Skill Tree/E/R UI를 보존하면서 우상단에 `속성 Lv 전기/검/얼음` TMP 한 줄을 Undo 가능한 방식으로 추가·연결한다. 합계는 `RunItemInventory.GetElementLevel` 조회값이며 별도 mutable 카운터나 활성 시너지 표시는 없다. 별도 Inspector 할당은 필요 없다.
- 필수 모달은 기존 전체 화면 raycast와 `ToolSlotInput.IsSelectionOrEndBlocked`를 사용해 뜰채·Q/W·E/R·재배치를 막고 Skill Tree보다 위로 정렬한다. Escape/Tab/닫기 경로는 없으며 다른 Core/Partner/E/결과 모달·타기팅이 활성 또는 pending이면 새 Item 모달을 거절한다. 열려 있는 동안 `timeScale=0`을 유지하고 성공 후 기존 `ResumeGameplayTimeScale`로 개발 배속을 복원한다. 실패 선택은 모달을 닫거나 보상을 소비하지 않는다.
- 개발 메뉴는 진행 중인 Run의 `ItemRewardManager > Development/Open Item Upgrade Reward`다. Editor/Development Build에서만 실제 업그레이드 보상 파이프라인을 호출하며, 4개 보유 상태·1개 후보·전부 최대 레벨·다른 필수 모달 충돌을 같은 코드로 처리한다. G6-A1 직접 업그레이드 메뉴는 Item reward가 pending인 동안 실행을 거절한다.
- `Assets/Editor/Tests/ItemProgressionTests.cs`에 G6-A2 집중 Edit Mode 테스트 14개를 추가했다: 4개 보유→서로 다른 2후보, 최대 레벨 제외, 기본 최대 초과 무제한, 1후보, 0후보/모달 상태 없음, 미보유 제외, invalid ID 제외, 한 보상 정확히 1회, 반복 확인 차단, 비제시 선택 차단, 실패 상태 보존, 레벨·속성 합계 갱신, 새 Run reset, Area 1 관문 무업그레이드.
- 요청에 따라 Unity/Computer Use/Test Runner/Play Mode/Console/Editor 메뉴/Scene 저장은 실행하지 않았다. Unity 6000.3.11f1의 기존 Bee response를 사용해 runtime과 Editor/test 어셈블리를 외부 Roslyn으로 컴파일했고 C# 오류 없이 종료 코드 0을 확인했다. Unity Source Generator 버전 불일치 `CS8032` 경고 3종은 외부 Roslyn 실행에서만 발생했으며 이는 Unity Compile 또는 테스트 통과를 의미하지 않는다. `git diff --check`는 통과했다. Scene/Prefab YAML, 밸런스, commit/push는 변경·실행하지 않았다.
- 사용자 Unity 검증 순서: ① Edit Mode에서 `NETBREAK/UI/Setup Item System UI` 실행 후 Main Scene 저장 ② Console compile error 없음 확인 ③ Test Runner/Edit Mode에서 `ItemProgressionTests` 전체 실행 ④ Play Mode 정상 Run 시작 후 개발용 획득 메뉴로 아이템 4개 확보 ⑤ F2/F3 상태에서 `Development/Open Item Upgrade Reward` 실행, 2개 카드/실제 수치/입력 차단/Tree 차단/배속 복원 확인 ⑥ 한 후보만 남긴 상태와 전부 최대 레벨 상태의 1택/무모달 경고 확인 ⑦ 업그레이드 직후 HUD Lv·tooltip 실제값·속성 합계 +1 및 다음 발동 스케일 확인 ⑧ 새 Run에서 pending/소유/레벨/속성 합계 초기화와 정상 Area 1 무아이템 보상 확인.
- G6-B1 전기 시너지는 아래와 같이 소스 구현했다. 당시 미구현이던 G6-B2 검/얼음 단일 속성 시너지는 아래 최신 G6-B2 항목에서 구현했고, G6-C 조합 시너지는 계속 미구현이다. 해역 2~6 gameplay, 관문 reward routing, 상인/보상 대체, 최종 UI art도 후속 범위다.

## 최근 변경 — G6-B1 REWORK 독립 전기 5단계 시너지·속성 hover 툴팁

- 이전 G6-B1의 폭풍 구슬/축전 코일 원본 활성화 종속 경로를 제거했다. 두 Item의 기존 자동 공격·피해·레벨 스케일·축전 타이머/적중 기준은 그대로고 더 이상 시너지 발동이나 감전을 직접 호출하지 않는다. 새 시너지는 `RunItemInventory.GetElementLevel(Electric)`만 평가하므로 폭풍 구슬 단독 Lv6, 축전 코일 단독 Lv6, 미래 Electric Item 합산도 Item ID 분기 없이 같은 2/4/6 단계가 열린다.
- `SingleElementSynergyThresholds` 기본값을 누적 **2/4/6/8/10**으로 확장했다. Lv2·6·10은 주요 효과, Lv4·8은 수치 강화다. `ItemEffectManager > Single-Element Synergy / 단일 속성 시너지`가 전기/검/얼음 공용 문턱의 유일한 Inspector 소유자이며 별도 mutable 속성 카운터는 없다.
- 전기 Lv2 `연쇄 방전`: 양의 Resistance 피해가 실제 적용된 Tool 원천 적중에서 자체 쿨다운 기본 7초마다 발동한다. 적중 위치 반경 3의 적격 물고기 최대 2마리를 거리→instance ID 순으로 골라 각각 Resistance 10 피해를 주며 visual은 0.45초다. 비활성·포획·도주·Pool 및 트리거 Tool 피해로 이미 포획된 물고기는 제외한다.
- Lv4 `전도 확장`은 같은 연쇄 방전 대상만 기본 +1해 총 3마리로 만든다. Lv6 `감전 방전`은 연쇄 방전으로 피해를 받고 생존한 대상에게 일반/특수 1초, MiniBoss/Boss 기본 0.5초 감전을 적용한다. 감전 종료 뒤 기본 3초 lockout이며 `electric_synergy.stun` 키만 제거해 Net/Net R/Item 둔화/특수어 modifier를 보존한다.
- Lv8 `과충전`은 전기 시너지 피해에만 기본 ×1.25를 정확히 한 번 적용해 연쇄 10→12.5, 천둥 폭풍 18→22.5가 된다. 여섯 Item 기준 피해·Item Level 스케일과 Tool 피해는 변경하지 않는다.
- Lv10 `천둥 폭풍`은 자체 쿨다운 기본 15초 뒤 다음 유효 Tool 적중에서 gameplay viewport 안의 적격 물고기 최대 5마리에게 각각 기본 Resistance 18 피해를 주며 visual은 0.65초다. 연쇄와 동시에 준비됐으면 천둥 폭풍만 발동하고 연쇄 쿨다운도 다시 시작한다. 두 쿨다운은 scaled `Time.time`으로 Pause에서 정지하며 pending backlog가 없다.
- 기존 `CombatDamageContext`와 연속 피해의 물고기·source instance·attack ID별 기본 0.5초 dedupe를 공용 진입점에서 한 번 사용한다. 같은 Tool 적중은 기존 Item 카운터와 독립 전기 시너지에 각각 기여한다. Item/ItemSynergy 원천 및 0 피해는 시너지를 발동하지 않으며 추가 피해는 `CombatDamageOrigin.ItemSynergy`라 Item 카운터·다른 시너지·자기 자신을 재귀 발동하지 않는다.
- 전기 시너지 Resistance 피해는 `FishController.TakeCaptureDamage`를 재사용하고 MiniBoss/Boss에도 100% 적용한다. 보스 피해 multiplier는 없다. `ItemEffectManager > Synergy Crowd Control / 시너지 군중제어`의 기본 MiniBoss ×0.5와 Boss ×0.5는 시너지 추가 CC에만 독립 적용된다.
- `ElectricSynergySettings`가 RunManager의 `ItemEffectManager > Electric Synergy / 전기 시너지` 아래에서 쿨다운·반경·대상·피해·visual·Lv4 추가 대상·Lv6 감전/lockout·Lv8 배율·Lv10 수치를 한 번만 소유한다. 새 Run/Inventory 재바인딩/Manager 비활성화는 두 쿨다운, 감전 lockout, pending 피해, visual을 초기화하고 포획·도주·Pool 반환은 물고기별 감전 상태를 정리한다.
- 기존 속성 HUD의 0레벨 숨김·속성별 한 줄·좌측 보정은 유지했다. `ItemElementLevels`의 각 TMP link hover는 공유 `ElementSynergyTooltip`을 열어 실제 공용 문턱 순서와 현재 Manager 수치를 표시한다. G6-B1 시점에는 전기만 `활성/미해금`, 검/얼음은 `구현 예정`으로 표시했으며 아래 G6-B2에서 검/얼음도 실제 상태로 전환했다. 체크/원형/다이아 marker와 명시적 한국어 상태를 함께 사용하며 차단 모달 중 숨고 Inventory `Changed`에서 갱신된다.
- 안전 이관 메뉴는 `NETBREAK/UI/Setup Item System UI`다. 기존 `ItemSystemUI`, 4슬롯, Item tooltip, 획득/업그레이드 모달과 참조를 보존하면서 기존 `ItemElementLevels`에 hover 컴포넌트를 추가하고 화면 안쪽 680×650 고정 패널 하나를 생성·재사용·연결한다. 패널과 텍스트는 raycast를 막지 않고 Auto Size 없이 16.5pt wrap을 사용한다. 중복/외부/부분 구성은 변경 전 중단하며 Undo·재실행을 지원한다. 메뉴 실행과 Scene 저장은 사용자 작업이다.
- 개발용 Lv10 검증은 실제 획득 메뉴로 두 Electric Item을 소유하고 `ItemEffectManager > Development/Upgrade Owned Electric Items To Level 5`를 실행한다. 두 아이템 Lv5 합계로 Lv10이 된다. 단일 Item Lv6 이상은 해당 Item의 `Unlimited Maximum Level`을 개발 중에 켠 뒤 `Development/Upgrade First Eligible Owned Item`을 반복해 실제 검증 API로 확인한다. 정상 Area 1 아이템 0개 및 경제는 유지한다.
- `ItemProgressionTests`를 새 설계로 갱신했다. 0/1/2/4/6/8/10 누적 tier, 사용자 문턱, 단일 Storm/Coil Lv6, Lv4 대상 수, bounded distinct 대상, Lv6/보스 감전, lockout·modifier 합성·Pool reset, Lv8 시너지 피해 1회/Item 무변경, 보스 피해 100%, Tool/Item/ItemSynergy 원천, 연속 source별 0.5초, Lv10 우선순위·두 쿨다운·reset, HUD zero 숨김, 툴팁 5단계 순서/실제 값/검·얼음 pending 상태를 다룬다.
- G6-B1 시점에는 승인된 검/얼음 5단계를 문서·툴팁 설명으로만 준비했고 gameplay는 구현하지 않았다. 이 상태는 아래 최신 G6-B2에서 실제 구현으로 교체했다. G6-C combined synergy, 최종 VFX/SFX, 정상 Area 1 Item 보상은 계속 미구현이다.
- Unity/Computer Use/Test Runner/Play Mode/Console/Editor 메뉴/Scene 저장은 요청대로 실행하지 않았다. Unity 6000.3.11f1 기존 Bee response와 Roslyn으로 runtime 및 Editor/test 어셈블리 source-level 컴파일을 수행했고 오류 없이 통과했다. 이는 Unity 자동 테스트 통과나 수동 Play 검증을 의미하지 않는다. commit/push도 수행하지 않았다.

## 최근 변경 — G6-B2 독립 검/얼음 5단계 시너지

- 기존 `RunItemInventory.GetElementLevel`과 공용 `SingleElementSynergyThresholds` 2/4/6/8/10을 그대로 사용한다. 특정 Item ID는 활성 조건에 사용하지 않으므로 유령 검집/자동 검진 중 하나만 Lv6이어도 검 2/4/6이, 서리 인장/빙결 결정 중 하나만 Lv6이어도 얼음 2/4/6이 열린다. 같은 Tool 이벤트는 해금된 전기·검·얼음에 독립적으로 기여한다.
- 검 Lv2 `영혼 참격`은 기존 source·attack ID·물고기별 0.5초 dedupe를 통과한 양의 Tool 원천 적중을 전역으로 기본 6회 센다. 트리거 대상이 살아 있으면 우선 공격하고 아니면 적중 위치 반경 3의 대상을 거리→instance ID로 결정해 Resistance 14를 준다. 대상이 없어도 완료 activation을 소비해 무제한 pending을 만들지 않으며 visual은 0.45초다.
- 검 Lv4 `예리한 영혼`은 필요 적중 기본 -1로 5회가 되며 최소 1회다. 기존 부분 진행은 보존하고 과거 적중을 소급 발동시키지 않는다. Lv6 `쌍검 소환`은 같은 activation에서 원본과 다른 대상에게 Resistance 10의 검을 한 번 추가하며 대상이 없으면 생략한다.
- 검 Lv8 `검기 증폭`은 검 시너지 피해만 ×1.2해 원본 16.8, 추가 검 12, 검의 비 24가 된다. Lv10 `검의 비`는 실제 원본 검 공격 성공 3회마다 viewport 안의 서로 다른 적격 대상 최대 4마리에게 기본 Resistance 20을 준다. 대상 없는 activation·추가 검·검의 비·Item/다른 시너지는 이 카운터를 올리지 않으며 Lv10 해금 전 activation은 소급하지 않는다. visual은 기본 0.65초다.
- 얼음은 별도 전역 마지막 피해 추론을 만들지 않았다. `FishController.TakeCaptureDamage`가 포획 전에 저장한 hit position, target lifecycle, `CombatDamageContext.Origin`, 실제 `CapturedByHit` 결과를 기존 queue로 전달한다. `IceSynergyRuntimeState`가 같은 instance/lifecycle의 중복 포획 결과를 거부한다. 따라서 Tool 원천 실제 포획만 발동하고 Item/ItemSynergy 포획·비포획 적중·도주는 발동하지 않으며 기존 `Capture()`의 Gold/EXP/MP/Mastery/Item 보상 경로를 재호출하지 않는다.
- 얼음 Lv2 `냉기 파동`은 Tool 포획 위치 반경 2의 **다른** 적격 물고기 최대 2마리를 2초간 25% 둔화하며 visual은 0.5초다. Lv4 `냉기 확산`은 대상 +1로 최대 3마리다. Lv6 `순간 빙결`은 실제 파동 대상에게 1초 빙결을 추가하고 종료 뒤 4초 재빙결 lockout을 둔다.
- 얼음 Lv8 `오래가는 한기`는 얼음 시너지 둔화 지속시간만 +0.5초로 만들어 냉기 파동 2.5초, 서리 폭발 3.5초가 되며 빙결 1초는 유지한다. Lv10 `서리 폭발`은 Lv10 해금 뒤 Tool 포획 3회마다 그 포획의 일반 냉기 파동을 **대체**한다. 반경 3, 최대 4대상, Resistance 12, 3초간 30% 둔화, visual 0.7초이며 누적 Lv6 빙결을 한 번 적용한다.
- 서리 폭발 피해로 포획된 물고기는 기존 capture/reward를 정확히 한 번 받지만 결과 origin이 `ItemSynergy`라 냉기 파동/서리 폭발 카운터, Item 카운터, 전기/검 시너지를 재귀 발동하지 않는다. 모든 검/얼음 시너지 Resistance 피해는 MiniBoss/Boss에도 100%이며 보스 피해 multiplier를 추가하지 않았다.
- 공용 `Synergy Crowd Control`은 전기 감전과 마찬가지로 얼음 추가 둔화 강도 및 빙결 지속시간에만 한 번 적용한다. 기본 MiniBoss ×0.5, Boss ×0.5는 독립 조절된다. `ice_synergy.slow`, `ice_synergy.freeze`, `electric_synergy.stun`, 두 Ice Item modifier는 별도 키라 서로 제거하지 않고 기존 Net/Net R/특수어 배율과 합성된다. 포획·도주·Pool 재초기화·새 Run에서 Ice lockout/modifier와 검/얼음 카운터를 정리한다.
- Inspector 위치는 RunManager의 단일 `ItemEffectManager`다. `Sword Synergy / 검 시너지`에는 6회, 14, 반경3, visual0.45, Lv4 -1, 추가 검10, Lv8 ×1.2, 검의 비 3회/4대상/20/visual0.65가 있다. `Ice Synergy / 얼음 시너지`에는 냉기 파동 반경2/2대상/25%/2초/visual0.5, Lv4 +1, 빙결1초/lockout4초, Lv8 +0.5초, 서리 폭발 3포획/반경3/4대상/12/30%/3초/visual0.7이 있다. 기존 공용 문턱·CC 및 Electric/6종 Item 직렬화 값은 변경하지 않았다.
- 공유 Element hover 툴팁은 Sword/Ice 다섯 단계를 더 이상 `구현 예정`으로 표시하지 않는다. 현재 속성 레벨과 공용 Inspector 문턱에 따라 `활성/미해금`을 표시하고 Sword/Ice의 실제 설정값을 설명에 반영한다. 기존 HUD/공유 패널을 그대로 사용해 새 Scene UI나 참조가 없으므로 G6-B2만을 위해 `NETBREAK/UI/Setup Item System UI`를 다시 실행할 필요가 없다.
- 개발 검증은 진행 중인 Editor/Development Build Run에서 `ItemRewardManager > Development/Open Item Acquisition Reward`로 실제 검 또는 얼음 Item을 소유한 뒤 `ItemEffectManager > Development/Upgrade First Eligible Owned Item`을 반복한다. 기본 최대 레벨에서는 같은 속성 두 Item을 각각 Lv5로 올려 합계 Lv10을 만든다. 정상 Area 1 Item 0개와 보상·경제는 변경하지 않았다.
- `ItemProgressionTests`에 검/얼음 0/1/2/4/6/8/10 누적, 단일 Item Lv6, 검 5-hit/부분 진행/피해 배율/서로 다른 추가 대상/검의 비 원본 카운터, 실제 FishController 포획 origin·position snapshot, Ice origin·중복 방지/대상 수/둔화·빙결/독립 보스 CC/3포획 대체/target bounded/lockout·reset/modifier 합성, 검·얼음 tooltip 활성 상태와 실제 설정값 검증을 추가했다. 기존 Electric/G6-A/Area 1 테스트는 유지했다.
- 요청에 따라 Unity, Computer Use, Test Runner, Play Mode, Console, Editor 메뉴, Scene 저장, commit, push는 실행하지 않았다. Unity 6000.3.11f1 기존 Bee response를 사용한 외부 Roslyn runtime/editor source-level 컴파일은 오류 없이 통과했다. 이는 Unity 자동 테스트 통과 또는 수동 Play 검증을 뜻하지 않는다. G6-C combined synergy와 최종 VFX/SFX는 계속 미구현이다.

## 최근 변경 — G6-C1 성장 관리 아이템 페이지·복합 시너지 기반

- 기존 `SkillTreeManager`의 Tab 입력과 Pause/Resume 경로는 그대로 두고 `SkillTreeCanvas`에 `[스킬 트리] [아이템]` 페이지 상태만 추가했다. `RunGrowthState.LastGrowthManagementPage` 기본값은 스킬 트리이며 같은 Run에서 마지막 페이지를 기억하고 새 Run에서 초기화한다. 필수 Core/Partner 획득 중에는 스킬 트리 페이지를 강제 표시하되 기억값은 덮지 않는다.
- 안전 이관 메뉴 `NETBREAK/UI/Migrate Growth Window To Skill Tree + Item`을 추가했다. 현재 Main Scene의 실제 `SkillTreeUI/TreePanel` 전체 화면 Rect와 Q/W/E/R Branch·Acquisition 참조를 검사한 뒤 공용 Header/닫기/숙련 표시는 창에 남기고 페이지 전용 기존 자식만 같은 full-stretch `SkillTreePage` 아래로 RectTransform 값 그대로 감싼다. 새 `GrowthNavigation`과 `ItemPage`만 만들며 별도 Tab listener, Manager, 영구 HUD를 만들지 않는다. 부분 구조·중복·외부/누락 참조에서는 변경 전 중단하고 Undo·재실행을 지원한다. Main Scene에서 이관 메뉴 실행과 저장을 완료했다.
- `GrowthItemPage`는 `RunGrowthState.ItemInventory`의 실제 4칸을 표시하고 빈 슬롯을 보존한다. Inventory `Changed`, Inventory 재바인딩, 페이지 열기/재진입에만 전체 표시를 갱신한다. 기존 ItemDefinition에는 icon 참조가 없으므로 새 임의 icon을 만들지 않았다. 영구 우상단 Item HUD·기존 Element HUD·Q/W/E/R Hotbar는 변경하지 않는다.
- `ItemEffectManager.BuildItemTooltipText`를 authoritative 아이템 설명 경로로 추가하고 기존 Item HUD와 새 페이지가 공유한다. 현재 Inspector 설정으로 계산한 실제 효과 문장·현재 주 효과값·다음 레벨 개선 또는 최대 레벨·런타임 상태를 표시한다. 새 속성 Hover도 기존 `BuildElementSynergyTooltipText`를 그대로 호출해 전기/검/얼음의 2/4/6/8/10 실제 `활성/미해금` 상태를 표시한다. 두 Tooltip은 페이지 경계 안에 보정되고 raycast를 막지 않으며 페이지 전환·창 닫기·포인터 이탈·차단 모달에서 숨는다.
- `CombinedSynergyCatalog`의 안정 순서는 뇌검 공명(전기+검), 초전도(전기+얼음), 빙검 공명(검+얼음)이다. 자격은 특정 Item ID가 아니라 기존 Inventory의 두 속성 레벨 합으로 평가하며 기본 각 Lv2다. 세 카드에 요구/현재 레벨, 짧은 효과, `미해금` 또는 `해금됨`, `구현 예정`을 함께 표시하고 하나의 상세 영역에서 승인된 G6-C2 임시 수치를 설명한다.
- `RunGrowthState.CombinedSynergy`가 UI 선택 후보와 하나의 Active ID, 결정적 첫 자동 선택, 기존 Active 보존, 원자적 수동 변경 검증, 동일 선택 무동작, 기본 30초 scaled gameplay time 쿨다운을 Run 범위로 소유한다. `RunManager`가 Inventory `Changed`를 구독해 UI를 열지 않아도 첫 자동 선택 API를 평가한다. 같은 Run의 Area 변경에서는 유지되고 새 Run에서 초기화된다. RunManager `Combined Synergy / 복합 시너지`에서 세 조합별 요구 레벨과 `Combined Synergy Switch Cooldown`을 조절한다.
- G6-C1에서는 `CombinedSynergyCatalog.CombatEffectsImplemented=false`로 정상 자동 활성·확인을 모두 차단했다. 따라서 자격을 만족해도 `해금됨 · 구현 예정`만 표시하고 Active 보너스나 `사용 중`을 거짓으로 표시하지 않으며 확인 버튼은 비활성이다. production 상태 API는 테스트에서만 명시적으로 효과 사용 가능 인자를 받아 최종 동작을 검증할 수 있다.
- `ItemProgressionTests`에 성장 페이지 기본/기억/새 Run 초기화, 페이지 변경 시 진행 보존, 실제 4칸/빈 슬롯, 획득·업그레이드 알림과 tooltip preview, 세 조합의 Item ID 비종속 자격, 동시 자격, 카탈로그 tie-break, 첫 활성 무쿨다운, 기존 Active 보존, 성공 변경 쿨다운, 동일 선택 무동작, 잠금/쿨다운 원자적 실패, scaled 시간 정지, Area 유지/새 Run reset, G6-C1 정상 활성 차단을 추가했다. 기존 테스트는 삭제·Skip하지 않았다.
- 첫 이관 저장에서 `OwnedItemSlot_1~4`의 Hover가 `GrowthItemPage.cs` 내부 런타임 MonoScript 참조로 직렬화되어 Missing Script 4개가 발생했다. `GrowthItemSlotHover`를 독립 `.cs/.meta` 자산으로 분리하고 제한적 복구 메뉴로 정확한 네 컴포넌트의 `m_Script`를 정상 GUID에 재바인딩했다. Main Scene 저장, Scene 재로드와 Unity 재시작 뒤 Missing Script 0개 및 경고 미재발을 확인했다. 기존 `owner`, `slotIndex`, 다른 컴포넌트와 Inspector 값은 보존했다.
- 단일 `GameCanvas`의 런타임 sibling 순서에서 Item 보상 모달이 `ItemSystemUI`를 최상위로 올린 뒤 복원하지 않아 영구 HUD가 성장 창보다 앞에 남던 문제를 보정했다. 성장 창이 열릴 때만 `SkillTreeUI`를 HUD보다 앞으로 올리고 닫으면 원래 순서로 복원하며, 외부 보상 모달은 계속 최상위와 입력 차단을 유지한다. 아이템·속성 Tooltip은 표시 중 `TreePanel` 최상위로 올려 본문·HUD보다 앞에 표시하되 기존 경계 보정, raycast 비차단과 숨김 규칙을 유지한다.
- Unity 6000.3.11f1에서 G6-C1 UI 이관과 Main Scene 저장, Scene 재로드·Unity 재시작, Console 컴파일 오류 없음, `ItemProgressionTests` 115/115 통과를 확인했다. 수동 검증으로 기존 Q/W/E/R Tree·노드 Tooltip, 스킬 트리/아이템 페이지, 4칸과 빈 슬롯, 슬롯 Hover, 아이템 획득·강화 실시간 갱신, 단일 속성 Tooltip, 복합 시너지 카드/해금 표시/활성화 차단, 새 Run 초기화, 상시 HUD·성장 창·Tooltip 렌더링 순서, 보상 모달 우선순위와 입력 차단을 확인했다. G6-C1 검증은 완료됐으며 commit/push는 아직 수행하지 않았다.
- 당시 다음 구현 단계는 G6-C2였다. 뇌검 공명·초전도·빙검 공명의 실제 전투 효과, 독립 시너지 이벤트 연결, 효과별 Inspector 튜닝값과 정상 자동 활성·수동 교체는 이 시점에는 아직 미구현이었다. G6-C1의 `CombinedSynergyCatalog.CombatEffectsImplemented=false` 게이트와 비활성 확인 버튼을 G6-C2 검증 전까지 유지했다.

## 최근 변경 — G6-C2 복합 시너지 전투

- 뇌검 공명은 독립 전기 연쇄 방전의 실제 피해 성공 대상에 생명주기별 8초 전도 표식을 부여한다. 같은 대상 재부여는 피해를 중첩하지 않고 만료 시각만 갱신하며, 독립 검 영혼 참격이 유효 적중했을 때 표식을 소비해 원본 대상 저항력 피해 18과 원본을 제외한 주변 최대 2마리 각각 8 피해를 준다. 폭발 내부 쿨다운은 scaled gameplay time 8초다.
- 초전도는 일반 Item 둔화가 아니라 기존 `ice_synergy.slow`가 적용된 대상에 독립 전기 연쇄 방전이 실제 적중했을 때만 발동한다. 원본을 제외한 주변 최대 2마리에게 각각 저항력 피해 8을 주고 기존 얼음 시너지 둔화 종료 시각을 최대 0.5초 연장하며, 내부 쿨다운은 6초다.
- 빙검 공명은 기존 얼음 시너지 둔화 또는 빙결 대상에 독립 검 영혼 참격이 실제 적중했을 때 발동한다. 원본과 중복 Collider를 제외한 주변 최대 2마리에게 각각 저항력 피해 10을 주고 별도 시간제 modifier로 20% 둔화를 1.5초 적용한다. 내부 쿨다운은 6초이며 기존 얼음, Net, R 이동 제어와 독립적으로 합성된다.
- 세 효과는 기존 `CombatDamageOrigin.ItemSynergy`, Resistance 피해·포획 경로와 거리/instance ID 결정적 대상 선정을 재사용한다. 복합 시너지 추가 피해는 Tool 적중 조건을 만족하지 않아 Item이나 단일 시너지 효과를 재귀 발동하지 않는다. Normal/Pufferfish/Squid/MiniBoss/Boss는 추가 Resistance 피해를 동일하게 받고, MiniBoss/Boss 군중제어에만 기존 `SynergyCrowdControlPolicy` 0.5 배율을 한 번 적용한다.
- `ItemEffectManager > Combined Synergy / 복합 시너지`가 세 효과의 피해, 대상 수, 표식·둔화 지속시간, 둔화율과 내부 쿨다운을 단일 Inspector 위치에서 소유한다. 범위는 기존 전기 연쇄 방전과 검 영혼 참격 탐색 반경을 재사용한다. `RunManager`의 각 Lv2 해금 요구와 기본 30초 수동 교체 쿨다운은 그대로 유지했다.
- `CombinedSynergyCatalog.CombatEffectsImplemented=true`로 정상 첫 자동 활성과 아이템 페이지 확인 버튼을 개방했다. 동시에 하나만 활성화하고 추가 조합 해금은 기존 Active를 바꾸지 않는다. 성공한 수동 교체만 scaled gameplay time 30초 쿨다운을 시작하며, UI는 `현재 사용 중`, `해금됨 · 교체 가능`과 남은 시간을 표시한다.
- 전도 표식은 Fish instance ID와 `LifecycleVersion`으로 구분한다. 포획·도주·Pool 반환 시 개체 상태와 빙검 modifier를 정리하고, Run 종료·새 Run에서 모든 표식·내부 쿨다운·임시 modifier를 초기화한다. 기존 Run 상태의 활성 조합과 교체 쿨다운은 Area 상태와 분리되어 있으나, 실제 해역 이동을 포함한 통합 Run 검증은 아직 완료하지 않았다.
- 변경 파일은 `Assets/Scripts/Core/CombinedSynergy.cs`, `ItemEffectManager.cs`, `Assets/Scripts/Fish/FishMovement.cs`, `Assets/Scripts/UI/GrowthItemPage.cs`, `Assets/Editor/Tests/ItemProgressionTests.cs`다. 기존 115개를 삭제·Skip하지 않고 G6-C2 결정론적 테스트 5개를 추가했다.
- Unity 6000.3.11f1에서 Runtime/Editor 컴파일 성공, Console의 새로운 문제 없음, EditMode `ItemProgressionTests` 120/120 통과를 확인했다. Unity 수동 검증으로 복합 시너지 자동 활성·수동 교체 UI, 대표 전투 효과, 기존 단일 속성 시너지와 UI 회귀를 확인했다. 정상 Area 1 아이템 미지급 원칙은 유지했다.
- G6-C2 구현과 지정 검증은 완료됐다. 실제 해역 이동과 향후 전체 Run 통합 검증은 완료로 기록하지 않는다. 당시 다음 작업은 버티컬 슬라이스 잔여 기능과 버그 점검이었으며, 현재 순서는 아래 최신 기획 변경을 따른다. 해당 문서 갱신 뒤 commit/push는 사용자가 수행했다.

## 최근 기획 변경 — 플레이타임·밸런스·개발 순서

- 한 Run의 기존 약 30분 목표를 폐기했다. 30분·45분·60분 같은 고정 목표나 절대 상한을 두지 않으며 1시간 이상도 허용한다. 특정 시간을 채우려고 반복·대기·어군 간격·전투 시간·Boss Resistance를 억지로 늘리지 않는다. 과거 Full Run #1~#3의 10분대 실측은 당시 버전의 역사적 기록으로 그대로 보존한다.
- 한 Run은 조업 시작부터 결과 화면까지다. 정식 게임은 항상 Area 1에서 시작해 해금된 마지막 해역까지 순차 진행하며 해역 이동은 새 Run이 아니다. 개발 단계마다 현재 구현된 해역 수를 기준으로 개별 해역 시간과 전체 Run 시간을 구분해 기록한다. 최우선 평가는 성장 밀도, 전투 변화, 전략적 판단과 지루함 여부다.
- 밸런싱을 분리했다. 지금은 진행 불가, 선택 무력화, 무한 피해 재귀·보상 중복·자원 증식, 무조작 해결, 실제로 확인된 심각한 페이싱 같은 구조적 문제를 처리한다. 아이템별 최종 피해, 도구 DPS, 상점 가격·확률, 전체 경제·난이도·플레이타임의 정밀 조정은 콘텐츠 확장 이후 수행한다. 현재 수치는 기능 검증용 임시값이다.
- G10 Area 1 Balance는 유지하되 Area 1의 최종 수치 확정이 아니라 기본 플레이 가능성, 대표 Core/Partner 빌드 성립, MiniBoss/Boss 진행과 명백한 구조적 문제 확인 단계로 재정의했다.
- 최신 개발 순서는 `현재 버전 최소 안정화 → 최소 온보딩·전투 피드백 → 소규모 외부 플레이테스트 → G7/G8 성장·경제 연결 → Area 2 제작 및 확장성 검증 → 점진적 도구·아이템/Area 3~6 확장 → 충분한 콘텐츠 이후 전체 정밀 밸런스·Meta·Hard Mode·출시 준비`다.
- G9 Legacy 정리는 관련 시스템 대체와 회귀가 확인된 범위만 수행한다. 현재 콘텐츠 제작을 막지 않는 휴면 코드의 대규모 정리는 우선하지 않는다.
- 현재 최소 안정화의 통과 조건은 G6-C2 버전 대표 Area 1 성공 Run, MiniBoss/Boss 주요 실패 경로, 결과 화면 재시작, 새 Run 성장 초기화, 치명적인 입력·모달·참조 오류 확인이다. 여러 Full Run을 반복하는 정밀 밸런스는 현재 통과 조건이 아니다.
- Windows Build Profile과 Main Scene 등록 수정은 커밋 `99e2031`로 `origin/vertical-slice`에 반영되어 있다. 사용자는 Windows 빌드 생성과 EXE 실행을 확인했다. 이 기획 변경 당시에는 빌드 검증 과정의 URP·ProjectSettings 변경이 미커밋 상태였고 문서 작업에서 보존했다. 이후 해당 설정 변경은 UX-F2-B와 함께 `3bc311c`로 push됐지만, 설정 자체의 별도 Unity 재검증 완료로 확대 해석하지 않는다. EXE 실행 확인도 당시 Area 1 성공 Run, 결과 화면 재시작 또는 새 Run 초기화 검증을 뜻하지 않았다.
- 이번 작업은 기획 문서 개정만 수행했다. Unity, 게임 코드, Scene/Prefab, Inspector, Build Profile과 ProjectSettings를 수정하거나 새 테스트를 실행하지 않았다. 문서 commit/push는 사용자가 수행한다.

## 최신 검증 — G6-C2 Area 1 성공 Run과 오징어 낚싯대 방해 조사

- 사용자 수동 Full Run 실측은 10분 36초, Core 낚싯대 / Partner 투망, 최종 Level 8, Gold 2895, 어획률 94.3%다. Boss 포획, 결과 화면, 재시작이 정상이고 Console Error는 0개였다. 이는 현재 Area 1 대표 성공 경로의 실측 기록이며 고정 플레이타임 목표나 최종 밸런스 판정이 아니다.
- 같은 버전에서 재시작과 새 Run 상태 초기화가 정상임을 확인했다. 추가 수동 검증에서 MiniBoss 도주 시 Run Failure·등급 F·재시작, Boss 1~2회 도주 뒤 다음 회유 계속, 3회 도주 시 Run Failure·등급 F·재시작이 모두 정상이고 Console Error는 0개였다. 검증 뒤 Boss/MiniBoss 테스트 모드도 OFF로 복구했다. 따라서 현재 버전 최소 안정화의 대표 성공 경로와 주요 실패·재시작 경로는 확인 완료다.
- 오징어 실제 데이터는 최초 먹물 2초, 이후 간격 5초, 반경 2.5, 방해 지속 2.5초다. `SquidController`는 먹물 시점에 반경 안 Collider의 부모 `FishingRodController`를 중복 제거해 `DisableTemporarily`로 전달한다. MiniBoss 전후, 성장 상태, E 스킬 또는 Tree 상태에 따라 대상을 제외하거나 방해 규칙을 바꾸는 분기는 없다.
- 낚싯대는 먹물 방해 중 `Update` 초입에서 비작동 상태를 확인해 대상을 지우고 공격 타이머 감소와 공격 실행 전에 반환한다. 방해 종료 시 공격 타이머를 0으로 만들어 즉시 공격을 재개한다. E `빠른 릴링`은 공격 간격 배율만, Tree 강화는 위력·범위·설치 수·비용만 변경하므로 먹물 비활성 조건을 우회하지 않는다.
- 여러 오징어의 방해 종료 시각은 `Mathf.Max`로 합쳐져 짧은 후속 방해가 기존 긴 방해를 줄이지 않는다. 재배치 차단과 먹물 차단도 독립 조건으로 합성된다. Fish Pool은 `Initialize(data)` 후 활성화하므로 재사용 오징어의 `OnEnable`에서 현재 데이터의 최초 먹물 지연으로 초기화된다.
- Unity 6000.3.11f1 임시 EditMode 결정론적 테스트 5개가 5/5 통과했다: 반경 안/밖 대상 선택, 방해 중 실제 공격 중지와 만료 후 복구, MiniBoss 완료 상태에서 Tree 강화·E 빠른 릴링과의 조합, 여러 오징어의 종료 시각 합성, 재배치 차단 합성, Pool 재사용 초기화를 검증했다. 제품 코드 결함은 재현되지 않아 C#을 수정하지 않았고 임시 테스트 자산도 제거했다.
- 체감상 방해가 보이지 않을 수 있는 근거는 작은 반경, 최초 발동 전 2초 지연, 발동 전에 오징어가 포획될 가능성, 2.5초의 짧은 지속시간, 별도 먹물 상태 UI/VFX 없이 낚싯대 Sprite가 어두워지고 대상선만 사라지는 현재 피드백이다. 후반 강화 낚싯대의 빠른 포획과 즉시 공격 복구도 관찰을 어렵게 할 수 있다.
- 최소 수동 후속 검증은 MiniBoss 이후 오징어가 낚싯대 2.5 반경 안에 2초 이상 생존하도록 두고, 낚싯대 색상·상태 문구·대상선·실제 Resistance 감소가 2.5초 동안 함께 멈췄다가 복구되는지 한 번 관찰하는 것이다. 이 시각 확인은 아직 완료로 기록하지 않는다.

## 최근 변경 — UX-F1 오징어 먹물 방해 시각화

- `FishingRodController`가 기존 `specialDisabledUntil`을 단일 진실 공급원으로 사용해 실제 먹물 방해 중인 낚싯대만 어둡게 하고 바로 위에 TextMeshPro 문구 `먹물 방해`를 표시한다. 표시용 독립 타이머나 별도 방해 상태는 추가하지 않았으며 만료·연장·공격 중지와 표시가 같은 종료 시각을 따른다.
- 상태 표시는 각 낚싯대 아래에 한 번만 생성해 재사용하고 매 프레임 오브젝트나 문자열을 만들지 않는다. 로드된 `NanumGothic-Bold SDF`를 우선 사용하고 찾지 못할 때 TMP 기본 글꼴로 대체한다. Scene/Prefab/YAML과 기존 Inspector 직렬화 값은 수정하지 않았고 Editor 메뉴도 필요하지 않다.
- 먹물 방해와 재배치 비작동 상태는 독립적으로 합성한다. 재배치 중에는 기존처럼 낚싯대를 어둡게 하지만 `먹물 방해` 문구를 거짓 표시하지 않는다. 렌더러의 원래 색상을 보존하고 외부에서 바뀐 색상도 정상 기준색으로 갱신해 흰색으로 강제 복원하거나 다른 시각 상태를 지우지 않는다.
- 여러 오징어는 기존 `Mathf.Max` 종료 시각을 공유하므로 문구도 가장 늦은 실제 방해 만료까지 하나만 유지된다. 방해를 건 오징어가 먼저 포획되어도 남은 방해 시간은 유지한다. 낚싯대 비활성화, Run 종료, Scene 재시작 시 문구를 숨기고 원래 색상을 복원하며 새 Run의 새 낚싯대는 방해 상태 없이 시작한다.
- 시각 조정값은 기존 `FishingRodController`의 `Ink Interference Visual / 먹물 방해 시각`에만 추가했다. 기본값은 어둡게 할 색상 검정, 혼합 강도 0.65, 상태 글자색 RGBA `(0.75, 0.9, 1, 1)`, 로컬 위치 `(0, 1.6, 0)`, 글자 크기 3.5다. 오징어 범위·주기·지속시간과 낚싯대 위력·공격 간격·범위 등 Gameplay 수치는 변경하지 않았다.
- 변경 파일은 `Assets/Scripts/Gear/FishingRodController.cs`, 신규 `Assets/Editor/Tests/FishingRodInterferenceVisualTests.cs`와 `.meta`, `NETBREAK_STATE.md`다. UX-F1 시작 전 사용자 변경 파일은 그대로 보존했다.
- Unity 6000.3.11f1에서 Runtime 및 Editor/Test 스크립트 컴파일에 성공했다. 신규 EditMode 결정론적 테스트 9/9와 기존 `ItemProgressionTests` 120/120이 각각 통과했으며, 최종 전체 EditMode 회귀도 129/129 통과했다. 신규 테스트는 표시 시작·종료, 공격 중지 일치, 여러 오징어 연장, 비대상 낚싯대, 비활성화 정리, Run 종료·새 Run, E 빠른 릴링, 재배치와 외부 원래 색상 보존을 검증한다. 기존 테스트는 삭제하거나 Skip하지 않았다.
- UX-F1 코드와 자동 테스트는 커밋 `5b595e0`에 포함되어 있고, 사용자는 그 이후 Unity Play Mode에서 오징어 먹물 방해 시각화를 수동 검증 완료했다. UX-F2-A 전체 회귀 중 먼저 생성된 낚싯대가 TMP 기본 글꼴을 정적 캐시에 고정할 수 있던 실행 순서 경계를 추가로 발견해 기본 글꼴은 캐시하지 않고 표시 시 NanumGothic-Bold SDF를 다시 확인하도록 최소 보정했다.

## 최근 변경 — UX-F2-A 전투 VFX

- 기존 G6-B1/B2에는 `ItemEffectManager`가 매 발동마다 LineRenderer 오브젝트를 생성·파괴하는 전기 선, 검격, 냉기 원형 연출을 이미 가지고 있었다. 실제 피해 이벤트 연결과 단일 시너지 지속시간 Inspector 값은 재사용하고, 중복 전역 Manager나 새 공격 이벤트를 만들지 않았다.
- 오징어 먹물이 실제 낚싯대를 방해한 뒤 오징어→낚싯대 검보라색 발사 궤적과 적중 강조를 표시한다. 모든 `DisableTemporarily` 판정을 먼저 끝낸 뒤 VFX를 호출하며 대상이 없으면 연출도 만들지 않는다. UX-F1의 어두움·`먹물 방해` 문구·여러 오징어 종료 시각 연장은 그대로 유지한다.
- 전기 연쇄 방전은 실제 Resistance 피해가 성공한 대상만 궤적과 적중 강조에 포함한다. 순간 감전은 실제 `electric_synergy.stun` 이동 modifier가 유지되는 동안 대상을 따라가는 전기 상태 표시를 사용하고 만료·포획·Pool 반환 시 제거한다. 천둥 폭풍은 실제 피해 성공 대상에만 더 굵은 낙뢰와 적중 강조를 표시한다.
- 검 영혼 참격, 추가 검, 검의 비는 기존 성공한 `DealSynergyDamage` 위치를 사용하면서 색상·방향·다중 검격 형태를 서로 구분했다. 기존 적중 카운터, 추가 대상 선정, 검의 비 카운터와 복합 시너지 연결은 변경하지 않았고 VFX는 추가 공격 이벤트를 발행하지 않는다.
- 얼음 냉기 파동과 서리 폭발은 실제 둔화·빙결 또는 피해가 적용된 대상이 하나 이상 있을 때만 포획 지점과 기존 반경으로 표시한다. 서리 폭발은 톱니형 확산으로 냉기 파동과 구분한다. 순간 빙결은 실제 `ice_synergy.freeze` modifier 동안만 대상을 따라가며 다른 둔화·감전·그물·R 제어를 지우지 않는다.
- 새 `CombatVfxPool`은 기존 `ItemEffectManager`가 소유하는 작은 재사용 풀이다. LineRenderer와 Sprites/Default Material을 재사용하고 scaled `Time.deltaTime`으로 수명을 줄여 Pause 중 정지한다. 활성 표시 기본 상한은 48개이며 상한에서는 가장 오래된 임시 VFX만 재사용해 전투 판정은 제한하지 않고 감전·빙결 지속 표시는 재활용 대상으로 삼지 않는다. Run 종료·Manager 비활성화·Scene 재시작에는 모두 비활성화하고 Pool 개체의 Fish lifecycle 변경에도 상태 표시를 정리한다.
- `ItemEffectManager > Combat VFX / 전투 시각 효과`의 코드 기본값은 활성 표시 상한 48, 선 굵기 배율 1, 크기 배율 1, 먹물 궤적 0.22초, 먹물 적중 0.3초, 공통 적중 강조 0.22초와 효과별 색상이다. 기존 Main 직렬화 값인 연쇄 방전 0.45초, 천둥 폭풍 0.65초, 영혼 참격 0.45초, 검의 비 0.65초, 냉기 파동 0.5초, 서리 폭발 0.7초를 유지했다. Scene/Prefab/YAML과 Gameplay 수치는 수정하지 않았다.
- 변경 파일은 `Assets/Scripts/Core/CombatVfxPool.cs`와 `.meta`, `ItemEffectManager.cs`, `Assets/Scripts/Fish/SquidController.cs`, `Assets/Scripts/Gear/FishingRodController.cs`, 신규 `Assets/Editor/Tests/CombatVfxTests.cs`와 `.meta`, `NETBREAK_STATE.md`다.
- Unity 6000.3.11f1에서 Runtime 및 Editor/Test 컴파일에 성공했다. 신규 UX-F2-A EditMode 테스트 10/10과 기존 테스트 129개를 합친 전체 회귀 139/139가 통과했다. 실제 이벤트·대상 없음·대상 위치 일치·Collider 중복 제거·여러 오징어·상태 만료와 Pool 반환·Pause·Run 종료·풀 상한과 재사용·Resistance 피해·추가 피해 재귀 방지를 검증했으며 삭제·Skip한 기존 테스트는 없다.
- 사용자는 Unity Play Mode에서 UX-F2-A의 임시 단일 시너지 VFX를 수동 검증했다. 이는 현재 임시 피드백의 동작 확인이며 정식 아트·정식 VFX 품질 완료를 뜻하지 않는다. 대규모 어군 성능과 정식 연출 적용 뒤의 화면 밀도·잔상은 버티컬 슬라이스 통합 검증에서 다시 확인한다.

### UX-F2-A 수동 확인 보충

- 사용자는 UX-F2-A Play Mode에서 개별 기능이 대체로 정상이고 명백한 전투 오류가 없음을 확인했다. 여러 표시가 동시에 발생할 때 뚜렷한 가독성 문제는 보고되지 않았지만, 정식 VFX 적용 뒤 효과 구분과 UI 가독성을 다시 검증한다.

## 최근 변경 — UX-F2-B 아이템·복합 시너지 VFX

- 개별 아이템 6종을 기존 실제 발동 지점에 연결했다. 폭풍 구슬은 단일 대상 위 짧은 낙뢰와 작은 적중, 축전 코일은 마지막 적중 위치에서 실제 추가 피해 대상별 짧은 방전과 적중, 유령 검집은 집중 검격, 자동 검진은 하강 검과 피격, 서리 인장은 실제 `frost_sigil` 둔화 동안 작은 서리 문양, 빙결 결정은 실제 피해 대상의 결정 피격과 실제 `frost_crystal` 둔화 동안 별도 결정 상태 표시를 사용한다. 축전 코일의 추가 대상이 없으면 가짜 방전이 생기지 않으며 모든 순간 VFX는 실제 피해 성공 후에만 생성한다.
- 뇌검 공명은 실제 전도 표식 런타임과 동기화된 단일 상태 표시를 사용한다. 같은 물고기 재부여는 오브젝트를 중복 생성하지 않고 실제 만료시각만 갱신하며, 표식 소비·만료·포획·도주·Pool 재사용·활성 조합 변경·Run 종료 시 즉시 정리한다. 실제 표식 소비와 원본 추가 피해 성공 때만 번개+검격 결합 폭발을 표시하고 실제 주변 피해 대상에만 별도 공명 타격을 표시한다.
- 초전도는 기존 `hadIceSynergySlow`와 내부 쿨다운을 통과한 뒤 실제 추가 피해 대상 방향으로만 차가운 번개와 적중을 표시한다. 빙검 공명은 기존 얼음 시너지 제어와 내부 쿨다운을 통과한 뒤 실제 추가 피해 대상에만 냉기 검격 전파와 피격을 표시한다. VFX는 `CombatDamageContext`를 만들지 않으며 기존 추가 피해 재귀 차단, 대상 수·범위·피해·둔화·쿨다운을 변경하지 않는다.
- 기존 `CombatVfxPool`의 LineRenderer, 단일 Sprites/Default 공용 Material, scaled `Time.deltaTime`, 활성 상한 기본 48과 지속 상태 보호를 재사용했다. 풀 포화 시 `복합 시너지 > 단일 시너지 > 개별 아이템 > 일반 반복 피격` 순서로 낮은 우선순위의 임시 표시만 대체한다. 표시를 생략해도 전투 피해·포획·보상은 계속 처리되며 매 프레임 전체 Fish 탐색이나 개별 Canvas/TMP 생성은 추가하지 않았다.
- `CombatVfxSettings`에 아이템 6종 및 복합 시너지 3종 색상, 복합 발동 기본 0.42초와 추가 적중 기본 0.28초를 추가했다. 기존 활성 상한 48, 선 굵기·크기 배율 1과 아이템별 기존 VFX 지속시간, 모든 Gameplay Inspector 값은 유지했다. 상태형 표시는 VFX 지속시간이 아니라 실제 modifier/전도 표식 수명을 따른다. Scene/Prefab/YAML과 별도 Editor 설정 메뉴는 추가하거나 수정하지 않았다.
- 변경 파일은 `Assets/Scripts/Core/CombatVfxPool.cs`, `Assets/Scripts/Core/ItemEffectManager.cs`, `Assets/Editor/Tests/CombatVfxTests.cs`, `NETBREAK_STATE.md`다. 신규 UX-F2-B EditMode 테스트 10개를 추가해 아이템 6종의 실제 대상/피해/상태 만료, 대상 없음, 복합 시너지 Active ID, 전도 표식 재부여·소비·만료·Pool 재사용·조합 변경 정리, 실제 공명/초전도/빙검 추가 대상, 내부 쿨다운, 풀 포화 우선순위를 검증했다. 기존 테스트는 삭제하거나 Skip하지 않았다.
- Unity 6000.3.11f1 Editor에서 Runtime 및 Editor/Test 스크립트 컴파일에 성공했다. 전체 EditMode 테스트 149/149가 통과했고 최종 Console은 로그 3, 경고 0, 오류 0이었다. 헤드리스 배치 실행은 Editor 라이선스가 없어 테스트 시작 전에 중단됐으며, 같은 버전의 열린 Unity Editor Test Runner에서 전체 검증을 완료했다.
- 사용자는 Play Mode에서 개별 아이템 6종과 복합 시너지 3종의 임시 VFX를 수동 검증했다. 동시 발동에서도 뚜렷한 가독성 문제는 보고되지 않았으나, 이는 임시 연출 검증이며 정식 VFX 적용 뒤 HUD/성장 UI 가독성, 대규모 어군 성능, Pause·포획·도주·재시작 잔상을 다시 확인한다.

## 최신 계획 — 버티컬 슬라이스 아트·피드백 및 외부 테스트 준비

- UX-F3는 새 E/R 또는 보스 연출을 만드는 단계가 아니라 **기존 E/R 및 MiniBoss/Boss 피드백 점검**이다. MiniBoss의 감속 후 돌진, Boss의 3회 회유와 안내, 기존 E/R 기능을 실제 플레이에서 먼저 확인하고 정보 전달 문제가 확인된 항목만 최소 수정한다. 문제가 없으면 별도 구현 없이 종료하며 기존 연출을 중복 구현하지 않는다.
- 정상 Area 1은 Boss 포획 뒤 R을 얻고 곧 결과로 진행하므로 R을 사용할 후속 전투가 없다. R 전투 연출의 완성도는 Area 2 등 후속 전투 구간이 실제로 연결된 뒤 다시 검증한다. UX-F3는 대규모 구현이나 이후 아트 작업의 필수 차단 단계가 아니다.
- 외부 플레이테스트 전 순서는 `VS-1 기존 피드백 최소 점검 → VS-2 Area 1 아트 방향 확정·별도 아트 기획서 작성 → VS-3 기본 아트·애니메이션 제작/적용 → VS-4 정식 핵심 VFX → VS-5 SFX·최소 BGM → VS-6 UX-F4 최소 튜토리얼 → VS-7 통합 검증 → VS-8 소규모 외부 플레이테스트`다. 상세 범위와 통과 기준은 `Docs/NETBREAK_ROADMAP.md`를 따른다.
- VS-2에서 정식 아트의 1차 방향을 **밝고 읽기 쉬운 탑다운 픽셀아트**로 확정했다. 간결한 표현을 기본으로 하고 특수어·MiniBoss·Boss·주요 전투 연출에는 포인트 디테일을 사용한다. `Docs/NETBREAK_ART_GUIDE.md`에 확정 원칙, 권장 초안, 미정 규격과 검증 계획을 기록했다.
- 현재 적용 폰트는 NanumGothic-Bold SDF, Dynamic atlas다. 정식 픽셀아트 UI는 갈무리 9를 우선 후보로 검토하지만 폰트 임포트, TMP Font Asset 생성, 기존 UI 교체와 공식 라이선스 검증은 아직 수행하지 않았다.
- 실제 Sprite·배경·아이콘 제작과 Unity 적용은 수행하지 않았다. 세부 Sprite 크기, PPU, Pixel Perfect Camera, 내부 해상도, 최종 팔레트와 애니메이션 규격도 미정이다. 다음 작업은 현재 Camera·Sprite·PPU·UI Scaling 조사와 일반 물고기 1종, 오징어, 낚싯대, 바다 배경, 기본 UI 아이콘 일부의 첫 프로토타입 제작이다.
- 정식 VFX, SFX와 BGM은 아직 완료되지 않았다. 이번 문서 작업은 Unity 실행·검증을 추가하지 않았고 기존 UX-F2-A/B 검증 결과를 변경하지 않는다.
- 외부 테스트 뒤에는 확인된 구조적 문제를 먼저 처리하고 G7 상점·Gold 경제, G8 해역별 성장·보상, Area 2와 해역 전환, 새 도구·아이템·스킬·시너지, Area 3~6, Meta·Hard Mode, 전체 밸런스·최적화·출시 준비 순으로 진행한다. 새 콘텐츠는 가능한 한 gameplay, 아트·애니메이션, VFX, SFX, UI와 검증을 한 단위로 묶고 최종 폴리싱·오디오 믹싱은 출시 준비에 남긴다.
- 이번 변경은 Markdown 계획 갱신만 수행했다. Unity 실행·테스트, 코드, Scene/Prefab, 에셋, ProjectSettings, Git add/commit/push는 수행하지 않았다.

## 이전 변경 — VS-2B-1 정어리 방향별 Sprite Pipeline 프로토타입 (역사적 12프레임 기록)

- `Assets/Art/Fish/Sardine/Sardine_Swim.png`에 128×96 RGBA 정어리 시트를 직접 픽셀 격자로 제작했다. 32×32 셀 3방향(동·북·북동)×4프레임이며 투명 여백이 있다. 현재 정어리 이미지는 후속 일반 어종 제작의 기준으로 사용할 **첫 프로토타입으로 승인**됐다. 최종 출시용 Art Lock은 아니며 32×32 셀·PPU 83·8 FPS와 Pixel Perfect 정책을 전체 프로젝트의 최종 규격으로 확정한 것은 아니다.
- `FishData`에 선택적 `FishVisualProfile` 참조를 추가하고 정어리에만 연결했다. `FishVisualDirectionResolver`는 실제 이동 방향을 8방향으로 분류해 3세트와 flipX/Y에 매핑하며 경계 히스테리시스와 저속 방향 유지를 적용한다. `FishVisualController`는 scaled time 4프레임 flipbook과 풀 재사용 초기화를 맡는다. 다른 어종은 기존 사각형 Sprite와 visualScale/visualColor를 유지한다.
- 기존 Fish 루트·CircleCollider2D·프리팹·씬은 변경하지 않았다. 정어리 시각 전용 자식은 런타임에만 생성하고 루트 배율을 보정해 Custom Visual 크기를 적용한다. FishMovement 경로·속도·판정은 그대로이며 실제 적용한 방향만 시각계에 노출한다.
- Unity 6000.3.11f1 배치 Editor에서 `NETBREAK/Art/Setup Sardine Prototype`을 실행해 12개 Sprite 분할·Import 설정·프로필·정어리 FishData 연결을 완료했다. `Validate Fish Sprite Pipeline` 메뉴가 통과했고 Setup 재실행 시 `Import changed: False`를 확인했다. 메뉴가 정어리 asset에 누락된 기존 기본 직렬화 필드 8개를 기록했으나 값은 코드 기본값과 같다.
- Runtime/Editor/Test 스크립트는 Unity에서 컴파일됐다. 신규 EditMode 테스트 14개와 기존 테스트 149개를 합친 전체 163/163이 실제 Test Runner에서 통과했다. 방향 8개·저속/경계·4프레임/일시정지·fallback·양방향 풀 재사용·크기/색/flip·Resistance/Collider 보존을 확인했다. 기존 테스트는 삭제하거나 Skip하지 않았다.
- `Docs/NETBREAK_SPRITE_PIPELINE.md`, `Docs/NETBREAK_ASSET_MANIFEST.md`를 추가하고 아트 가이드를 갱신했다. 사용자가 Unity Play Mode에서 컴파일 정상과 Console Error 0을 확인했다. 정어리 Sprite와 가로·세로·대각선, 반대 방향 flip, 4프레임 헤엄이 정상이며 방향 전환에 심각한 jitter가 없음을 확인했다. 여러 정어리의 동시 표시와 기존 대비 화면 크기, 다른 어종의 Prototype fallback, 정어리↔다른 어종의 양방향 Pool 재사용, Pause/선택 상태, 기존 VFX·Resistance·포획·UI, Run 재시작 후 시각 초기화도 정상으로 확인했다. 사용자는 현재 정어리 Prototype 이미지 방향에 만족한다고 밝혔다. 이는 **VS-2B-1 수동 시각 검증 완료 및 첫 프로토타입 승인**이며 최종 출시용 아트 승인이나 전체 프로젝트 Sprite 규격 확정은 아니다.
- Git add/commit/push는 사용자 지시에 따라 수행하지 않았다.

## 이전 변경 — VS-2B-2 고등어·참치 방향별 Sprite 확장 (역사적 12프레임 기록)

- 정어리 승인 프로토타입을 기준으로 고등어 192×144(48×48 셀)와 참치 256×192(64×64 셀) RGBA 시트를 픽셀 격자에서 직접 제작했다. 각 시트는 가로·세로·대각선 4프레임씩 12프레임이다. 공통 PPU 83, Point/무압축/Full Rect/Clamp, 기본 8 FPS와 프로필 (1,1) 크기·흰색 Tint를 사용한다. 가로 실루엣은 정어리 약 31×15px, 고등어 약 43×21px, 참치 약 59×29~31px이다. 이 값은 프로토타입이며 전체 최종 Sprite 규격이 아니다.
- `NETBREAK/Art/Setup Mackerel And Tuna`를 Unity 6000.3.11f1 Editor에서 실행해 각 12개 Sprite 분할, 신규 프로필 생성과 고등어·참치 FishData 연결을 완료했다. `Validate Fish Sprite Pipeline`은 세 어종과 나머지 fallback을 검사해 통과했다. 정어리 시트·프로필·FishData 연결은 보존했다. 고등어·참치 FishData에는 Unity가 누락된 기존 기본 직렬화 필드 8개를 추가 기록했으나 기존 값·게임플레이 설정은 바뀌지 않았다.
- Runtime `FishVisualController`, 방향 판정, `FishMovement`, Pool, 게임플레이 로직과 Scene/Prefab은 변경하지 않았다. 신규 EditMode 테스트 5개는 시트/프로필/연결, 정어리·미적용 어종 보존, 두 어종 게임플레이 수치, 정어리→고등어→참치→Prototype→고등어→참치→Prototype→참치→정어리 풀 재사용에서 Sprite·프레임·flip·scale·Tint·Resistance·Collider 초기화를 확인한다. Unity의 전체 EditMode 168/168이 통과했고 Console 경고 0, 오류 0이었다.
- 사용자가 VS-2B-2 Unity Play Mode 수동 시각 검증을 완료했다. Unity 컴파일과 전체 EditMode 테스트가 정상이고 Console Error는 0이었다. 정어리·고등어·참치의 크기 계층과 실루엣 구분, 고등어·참치의 가로·세로·대각 방향, 반대 방향 flip, 4프레임 헤엄, 심각한 방향 jitter 없음, 다수 개체와 어군 가독성, 적절한 참치 크기를 확인했다. 복어·오징어 Prototype fallback, 종간 Pool 재사용 초기화, Pause/선택 상태 애니메이션, 기존 VFX·Resistance·포획·UI와 Run 재시작 뒤 시각 상태도 정상으로 확인했다. 사용자는 고등어·참치 이미지가 현재 아트 방향으로 만족스럽다고 밝혔다.
- 고등어와 참치는 **정어리 기준 Directional Fish Sprite Pipeline을 성공적으로 확장한 승인된 프로토타입**이다. 서로 다른 32×32·48×48·64×64 셀을 공통 PPU 83과 3방향×4프레임 구조로 사용해 일반 어종의 자연스러운 상대 크기와 파이프라인 재사용을 확인했다. 이는 PPU 83, 8 FPS, 셀 크기, Pixel Perfect Camera, 향후 모든 어종의 프레임 수, 특수어·Boss 규격의 전체 프로젝트 최종 확정은 아니다. 정어리의 기존 승인 상태는 유지하며 세 어종의 최종 출시용 아트 승인은 Pending이다. 이번 수동 결과 문서 반영에서는 Unity 실행·테스트와 Git add/commit/push를 수행하지 않았다.

## 이전 변경 — VS-2B-3 복어·오징어 방향별 수영 Sprite 확장 (역사적 12프레임 기록)

- 복어 192×144 RGBA(48×48 셀), 오징어 256×192 RGBA(64×64 셀) 시트를 `Tools/generate_pufferfish_squid_sprites.py`의 픽셀 격자 제작으로 추가했다. 각각 가로·세로·대각선 4프레임씩 12프레임이고 반대 방향은 기존 flip 매핑을 사용한다. 복어는 둥근 몸통을 유지하며 꼬리·지느러미가 움직이고 오징어는 몸통·촉수·측면 지느러미의 수영 리듬을 표현한다. 공통 PPU 83, Point/무압축/Full Rect/Clamp, 중심 Pivot과 프로필 8 FPS·(1,1) 크기·흰색 Tint는 기존 프로토타입 기준이다.
- 새 Sprite Import 설정 파일, `Pufferfish_VisualProfile.asset`, `Squid_VisualProfile.asset`과 각 FishData 참조를 저장소 파일에 작성했다. 기존 검증된 고등어/참치 메타 형식을 기반으로 고유 GUID를 부여했고, `NETBREAK/Art/Setup Pufferfish And Squid` 메뉴로 Unity에서 다시 분할·연결할 수 있게 했다. 공용 `Validate Fish Sprite Pipeline`은 다섯 어종과 MiniBoss/Boss fallback을 검사하도록 확장했다. `FishVisualController`, `FishVisualDirectionResolver`, `FishMovement`, 게임플레이 루트·Collider·Resistance·이동·먹물 방해와 Scene/Prefab은 변경하지 않았다.
- `FishVisualExpansionTests`에 두 종의 정확한 셀/12개 프레임/프로필 연결, 특수 능력·Resistance·보상 수치 보존, 다섯 어종과 MiniBoss/Boss 간 Pool 재사용 경로를 추가했다. PNG와 참조의 정적 검증은 별도로 수행했다. 최초 구현 작업 당시 Unity 창 자동 조작은 승인 검토에서 거부되어 Editor 확인을 사용자에게 남겼다.
- 사용자가 VS-2B-3 Unity 수동 검증을 완료했다. Unity 컴파일 정상, Console Error 0, 전체 EditMode Test Runner **173/173 통과**를 확인했다. 복어·오징어 각각 가로·세로·대각선, 반대 방향 flip, 4프레임 수영, 방향 전환 시 심각한 jitter 없음, 기존 정어리·고등어·참치 정상, 다수 어종 동시 표시와 가독성, 종간 Pool 재사용 초기화, Pause, 기존 오징어 먹물 방해 Gameplay, VFX·Resistance·포획·UI, Run 재시작 후 시각 초기화를 확인했다. 두 어종은 **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**이다.
- 복어 팽창·가시 강화·특수 상태 전용 애니메이션과 오징어 먹물 발사 전용 프레임·애니메이션/VFX/SFX는 구현하지 않았다. 오징어의 기존 먹물 방해 Gameplay는 유지한다. 최종 VFX/SFX와 출시용 아트 승인은 후속이다. 사용자 Unity 검증 순서는 `Docs/NETBREAK_SPRITE_PIPELINE.md`에 기록했다. Git add/commit/push는 사용자 지시에 따라 수행하지 않는다.

## 현재 변경 — VS-2B-4 다섯 어종 방향별 16프레임 확장 (수동 검증 완료·미커밋)

- 정어리 128×96→128×128(32×32 셀), 고등어 192×144→192×192(48×48), 참치 256×192→256×256(64×64), 복어 192×144→192×192(48×48), 오징어 256×192→256×256(64×64) RGBA 시트로 확장했다. 세 기존 생성 스크립트가 각 시트의 NW 4프레임을 동일 팔레트·실루엣·꼬리 리듬으로 추가한다. 다섯 시트 모두 기존 첫 12프레임의 RGBA 해시가 변경 전과 같다.
- `FishVisualSet`에 NW 축을 기존 enum 순서를 보존하며 끝에 추가했다. E/N/NE/NW는 원본 프레임, W/S/SW/SE는 각각 E/N/NE/NW에 flipX+flipY를 함께 적용한다. 북쪽 원본의 배색 중심이 오른쪽인 5종 모두 남쪽에서 180도 반전 후 배색 중심이 왼쪽이다. `FishVisualProfile`은 4×4 완전한 프로필만 사용하고, 기존 `FishVisualController`의 프레임·타이머·방향·flip·Tint·Custom/Fallback 초기화 경로는 유지했다.
- 다섯 `*_Swim.png.meta`의 기존 12개 Sprite ID와 파일 ID를 보존하면서 4개 NW 분할을 추가하고, 다섯 `*_VisualProfile.asset`에 새 프레임 참조를 연결했다. `Tools/extend_fish_sprite_imports.py`는 검사한 메타데이터 형식을 엄격히 확인하는 이관 도구다. 공용·어종별 Editor Setup과 Validate는 16분할을 사용한다. FishData의 기존 VisualProfile 연결은 보존했다.
- 이번 작업의 수정 파일: `Assets/Art/Fish/{Sardine,Mackerel,Tuna,Pufferfish,Squid}/`의 각 `*_Swim.png`, `*_Swim.png.meta`, `*_VisualProfile.asset`; `Assets/Scripts/Fish/{FishVisualProfile,FishVisualDirectionResolver}.cs`; `Assets/Editor/{FishArtSetup,SardineArtSetup}.cs`; `Assets/Editor/Tests/{FishVisualPipelineTests,FishVisualExpansionTests}.cs`; `Tools/generate_sardine_sprite.py`, `Tools/generate_mackerel_tuna_sprites.py`, `Tools/generate_pufferfish_squid_sprites.py`; 이관 도구 `Tools/extend_fish_sprite_imports.py`와 정적 검사 `Tools/validate_fish_visual_sheets.py`; `NETBREAK_STATE.md` 및 Docs의 아트 가이드·Sprite Pipeline·Asset Manifest. 시작 전부터 있던 두 FishData 연결 및 NanumGothic 폰트 변경은 이번 작업에서 수정하지 않았다.
- `FishVisualPipelineTests`와 `FishVisualExpansionTests`를 8방향 원본 선택/두 축 flip, NE와 NW 구분, 다섯 어종의 16분할·배색 중심, 풀 재사용에 맞춰 갱신했다. 게임플레이 수치·이동·Resistance·Collider·먹물·Boss, Scene/Prefab, ProjectSettings/URP는 수정하지 않았다.
- 정적 검사: 다섯 PNG 규격·기존 12프레임 해시·16개 Sprite 이름/ID·프로필 참조 **5/5 통과**. 다섯 어종의 북쪽 배색 중심이 오른쪽, 남쪽 180도 변환 중심이 왼쪽인 픽셀 검사 **5/5 통과**. 이전 비대화형 배치 시도는 Editor 잠금과 Unity Package Manager IPC 연결 문제로 중단됐으나, 이후 사용자가 Unity에서 컴파일 정상·Console Error 0·전체 EditMode Test Runner **183/183 통과**를 확인했다. 실패 0; skip 수는 별도로 전달받지 않았다.
- 사용자 Play Mode 수동 검증 완료: 정어리·고등어·참치·복어·오징어의 E/W/N/S 및 NE/NW/SE/SW 방향, 서로 다른 NE/NW 원본 Sprite Set, 반대 방향 flipX+flipY, 4프레임 수영이 정상이다. S에서 배가 화면 왼쪽이며 E→SE→S 전환에서 배/등 방향이 자연스럽게 이어진다. 종간 Pool 재사용의 Sprite·frame·flip·tint·scale 잔상이 없고, 오징어 먹물 방해·Resistance·Collider·포획·VFX·UI·Pause·Run 재시작도 정상이다. 다섯 어종의 4방향축×4프레임 규칙은 **현재 프로토타입으로 승인**됐고 Final Production Art Approval은 Pending이다. PPU 83·8 FPS·셀 크기·Pixel Perfect Camera는 프로젝트 전체 최종 규격으로 확정하지 않았다.
- `git diff --check`는 전체 작업 트리에서 종료 코드 2다. 이번 문서 갱신 시작 전부터 변경된 `Assets/Art/Fish/Mackerel/Mackerel_Swim.png.meta`의 공백 8곳과 `Assets/UI/Fonts/NanumGothic-Bold SDF.asset`의 공백 3곳이 지적됐다. 이번에 수정한 네 문서만 대상으로 한 `git diff --check`는 종료 코드 0이다. 두 기존 파일은 수정하지 않았다.
- 문서 `Docs/NETBREAK_ART_GUIDE.md`, `Docs/NETBREAK_SPRITE_PIPELINE.md`, `Docs/NETBREAK_ASSET_MANIFEST.md`에 현재 규격, 역사적 12프레임 결과와의 구분, Unity Play Mode 수동 검증 순서를 반영했다. Git add/commit/push는 요청에 따라 수행하지 않는다.
