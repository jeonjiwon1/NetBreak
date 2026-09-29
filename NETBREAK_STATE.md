# NETBREAK 인수인계

## TMP Font Stabilization 최종 정리 및 Area 1 Full Run 검증 (2026-09-29, Prototype 승인)

- 사용자가 Area 1 시작부터 Result까지 Full Run을 완료했다. 한국어/Galmuri 글리프 깨짐·□ 없음, Tutorial·Growth/TAB·Giant Tuna/Shark 저항 UI·Puffer/Squid 표현·구 첫 등장 팝업 제거·자홍색 경로선 숨김·Gameplay 정상. Full Run에서 `Importer(NativeFormatImporter) generated inconsistent result` 경고는 재발하지 않았다. Result는 정상 진행에 명백한 문제 보고가 없었던 수준이며 세부 Production UI 품질 승인으로 확대하지 않는다.
- Full Run 전후의 SDF diff를 사용자가 직접 비교하지는 않았다. 네 폰트의 정리 전 직렬화에서 `m_ClearDynamicDataOnBuild: 1 → 0`만 의도적 설정이고 글리프·문자·Atlas·index·packing 변화는 GENERATED CHURN임을 재확인했다. 정리 전 파일은 무시되는 `Logs/TMPFontCleanupBackup_20260929/`에 백업했다. HEAD의 완전한 에셋 데이터에 해당 설정만 반영해 최종 네 SDF diff는 각각 한 줄이다. GUID·Source Font·Material·fallback·Dynamic Mode·원래 Atlas 데이터와 Runtime Font Clone은 유지한다. Scene/Prefab, Tutorial/Gameplay/UI Layout은 이번 정리에서 수정하지 않았다.
- Unity 6000.3.11f1 배치 컴파일 성공, Font 안정화 테스트 **6/6**, 전체 EditMode **280/280 Passed**, 네 에셋 연속 강제 임포트의 파일 바이트 불변, 테스트 로그의 해당 경고 없음, 전체 `git diff --check` 통과. **Unity Manual Font/Presentation Validation: Passed / Integrated Area 1 Full Run: Passed / Prototype Font Stabilization Approval: Approved / Prototype Presentation Cleanup Approval: Approved / Final Production Approval: Pending.** Unity 재시작 전후의 별도 Git diff 비교, 전체 문자열·해상도별 Production 가독성은 후속 관찰 항목이다. Git add/commit/push는 사용자 지시로 수행하지 않았다.

## TMP Font Importer Warning 후속 안정화 (2026-09-29, 정리 전 조사 기록)

- Galmuri11/Bold 경고의 실제 스택은 TMP의 렌더 후 예약 재임포트(`TMP_EditorResourceManager.DoPostRenderUpdates` → `AssetDatabase.ImportAsset`)였다. Dynamic Multi Atlas의 새 Texture2D 하위 에셋 추가가 재임포트를 등록한다. 이전 Nanum Bold 로그에는 Editor 종료 시 Dynamic 데이터 삭제 직후 임포트와 경고가 기록돼 있다. Unity 내부 artifact ID 차이의 더 낮은 수준 원인은 미확인이다.
- 기존 네 폰트 에셋 변경은 버리지 않았다. 현재 Galmuri11 글리프 138→235/Atlas 2→3, Bold 117→111/2→2, Nanum Regular 10→10/1→1, Nanum Bold 295→66/4→1이다. Source Font·Material·fallback·GUID는 유지되고 `m_ClearDynamicDataOnBuild`는 네 에셋 모두 1→0이다. Galmuri/Nanum Bold의 잔여 글리프·Atlas diff는 과거 생성 데이터와 현재 생성 데이터의 혼합이라 원인 확인 없이 원복하지 않는다.
- `Area1Typography`는 Galmuri 두 폰트와 Nanum Bold fallback을 메모리 전용 Dynamic 복제본으로 사용해 주요 Canvas의 글리프 생성이 원본을 수정하지 않도록 했다. `Area1FontStabilityTests`는 복제본 글리프 추가 후 원본 파일 불변과 네 에셋의 연속 강제 임포트 파일 불변을 확인했다. Unity 6000.3.11f1 배치 컴파일 성공, 전체 EditMode **280/280 Passed**, 실행 로그의 Importer 경고 없음. 실제 Play, Editor 재시작 뒤 Dirty 재발, Console 및 한국어 UI 확인은 Pending이다. 기존 Scene 원본 Nanum 참조와 낚싯대 월드 라벨의 Galmuri Bold 직접 참조는 별도 잔여 경로다. Git add/commit/push는 사용자 지시로 수행하지 않았다.

## Legacy Prototype Presentation Cleanup + TMP Font Stabilization (2026-09-29, Full Run 전 기록)

- 시작 당시 Working Tree에는 Galmuri11/Bold, NanumGothic Regular/Bold TMP 에셋 네 개의 Unity 직렬화 변경만 있었다. 실제 diff에서 Dynamic 글리프·문자 표와 Atlas 축소를 확인했다. 설치된 TextMeshPro 패키지는 `Clear Dynamic Data On Build`가 켜진 Dynamic 에셋을 Editor 종료/빌드 전에 비운다. 네 에셋의 해당 설정을 Unity `SerializedObject`로 껐으며 기존 변경을 버리지 않았다. 추가 글리프의 1회 기록은 가능하므로 전체 Run/Unity 재시작 후 반복 Dirty 여부는 Pending이다.
- Main Scene TMP 113개 중 Nanum Bold 109, Nanum Regular 1, TMP 기본 3개를 직렬화 참조한다. Scene/Prefab YAML을 수정하지 않고 GameCanvas 활성·비활성 텍스트에 Galmuri를 런타임 적용한다. 월드 먹물 라벨과 UI 생성기도 Galmuri로 이관하고 Nanum은 fallback으로 보존했다. 기존 Scene 직렬화 참조의 영구 이관과 전체 글리프/장문 검증은 후속이다.
- 복어·오징어 첫 등장 구형 설명 공지를 제거했다. 기존 4초 대기와 전투·Spawn 수치는 그대로다. Giant Tuna/Shark Resistance 패널과 일반 공지에 현재 Pixel 프레임·폰트를 적용했다. Boss Phase/회유·회복, MiniBoss 보상은 유지했다. 자홍색 경로선/라벨은 기본 숨김과 개발 플래그로 전환했다.
- Unity Refresh 뒤 최종 코드 컴파일 완료, 컴파일 로그의 C# error 없음과 변경 중 Console Error 0을 확인했다. Play Mode 기본 HUD와 경로선 숨김을 확인했다. 최종 변경 후 전체 EditMode 테스트 274/274 통과했다. Puffer/Squid/MiniBoss/Boss/Result의 실제 화면과 폰트 재시작 Dirty 확인은 Pending이다. 상세 감사·수동 절차는 `Docs/NETBREAK_PRESENTATION_CLEANUP.md`를 따른다. Git add/commit/push는 사용자 지시로 수행하지 않는다.

## Area 1 Minimal Contextual Tutorial (2026-09-29, Unity 수동 검증 완료·Prototype 승인)

- `PrototypeGameFlowManager`의 기존 오브젝트에 `Area1TutorialController`를 런타임으로 한 번 부착한다. 조업 시작, Lv2 Core/Q 및 Lv3 Partner/W 도구 획득, MiniBoss 포획 보상의 E 선택 완료, Shark Boss 시작에서만 한 줄 안내를 요청한다. 도구 문구는 실제 선택 도구에 따라 바뀐다. 보스 회유 횟수는 `BossEncounterController.MaxPasses`를 읽는다.
- 기존 `GameCanvas`에 가로 640×64의 작은 Galmuri 패널을 런타임 생성한다. 하단 Hotbar 위에 놓고 Raycast를 받지 않는다. 한 번에 한 메시지만 표시하며 4.5초 후 사라지고, 선택 UI가 열리면 숨기고 시간을 멈춘다. 메시지 시간은 unscaled time이다. 보스 시작은 남은 일반 안내를 비운다. Run 성공·실패·Scene 재시작에서는 대기 메시지를 정리한다.
- 완료 플래그는 `PlayerPrefs`의 `NetBreak.Area1Tutorial.v1.{Goal,CoreTool,PartnerTool,TacticalSkill,BossRule}` 키에 저장한다. 개발용 `Tools/NETBREAK/Reset Area 1 Tutorial Progress` 메뉴로 모두 초기화한다. Tutorial은 Gameplay 입력·물고기·Spawn·timeScale을 변경하지 않는다.
- 성장 선택 UI와 TAB 관리창은 자체 제목·선택 설명이 있어 추가 안내를 생략했다. Puffer는 상태 표현, Squid는 먹물 충돌/암전, Giant Tuna는 경고·점멸·HUD로 전달하므로 별도 문구를 넣지 않았다. Boss의 회유 횟수는 HUD에도 있으나 첫 방문의 실패 규칙을 알리기 위해 한 줄을 유지한다. R은 Boss 포획 직후 Run이 종료되어 조작 안내를 넣지 않았다.
- `git diff --check`, 새 파일의 공백 검사, Assets의 362개 `.meta` GUID 중복 검사, 실제 Q/W/E 입력·도구별 동작·HUD/보스 회유 직렬화 확인을 통과했다. 구현 당시 Unity 재컴파일 기록이 없어 컴파일·EditMode 테스트·Console을 통과로 기록하지 않았으며, 이후 사용자의 실제 Unity Play로 소스 컴파일을 확인했다. EditMode 테스트 실행과 Console Error 0 결과는 별도로 전달받지 않았다.
- 사용자가 조업 시작, 선택한 Core/Q·Partner/W 도구별 조작, MiniBoss 보상 E, Shark Boss의 런타임 3회 회유 안내까지 5개 Step을 실제 Area 1에서 확인했다. 문구·위치·표시 방식은 Prototype에서 사용 가능하고 기존 HUD·Growth·Boss HUD 및 Gameplay Input을 심각하게 방해하지 않으며 과도하지 않다고 판단했다. 추가 Step 없이 Vertical Slice 다음 단계로 진행 가능하다. Growth 선택, TAB, Puffer, Squid, Giant Tuna Pattern, R 조작 및 Boss Pattern별 설명은 제외를 유지한다. External Playtest에서 문구·시점·시간·Step 수를 재평가할 수 있으며 현재 승인은 최종 Production UI/UX 승인이 아니다.
- **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Static Validation: Partial (위 정적 검사 통과) / EditMode Tests: Not Run / Unity Manual Tutorial Validation: Passed / Prototype Tutorial Approval: Approved / Final Production Approval: Pending.** Tutorial은 Gameplay/Input/Balance를 소유하지 않는 Presentation 계층이다. Scene/Prefab/ProjectSettings와 Gameplay·밸런스 수치는 변경하지 않았다. 이번 문서 정리에서 Code/UI/Gameplay/Audio/Scene/Prefab/Font Asset과 Git add/commit/push는 수행하지 않는다.

## Area 1 Normal / MiniBoss / Boss Prototype BGM (2026-09-29, Unity 수동 청각 검증 완료·Prototype 승인)

- `Area1BgmController`가 기존 `PrototypeGameFlowManager` 오브젝트에 런타임으로 한 번 붙고 별도 자식 `Area1BgmSource`(2D, loop, pitch 1)를 소유한다. `ItemEffectManager`의 공용 one-shot SFX Source와 분리했다. `Resources/Area1BgmProfile.asset`이 세 Clip, 공통 Source 볼륨 0.22, 0.5초 전환 시간을 보관한다.
- Normal은 `StartFishing`에서 시작한다. Giant Tuna 실제 생성 뒤 MiniBoss로 전환하고, 포획하면 Normal로 돌아온다. 미포획은 기존 Run Failure와 함께 정지한다. Shark Boss Encounter 시작 때 Boss로 전환하며 1→2→3회 회유 사이에는 재요청하지 않는다. Boss 완료/실패, 결과, Restart에는 정지한다. TAB/성장 선택 등 기존 `timeScale=0` 중에는 BGM을 임의로 Pause하지 않으며 페이드는 unscaled time이다. 게임 x1/x2/x3도 음악 pitch에 영향이 없다.
- `Tools/generate_area1_bgm.py`가 외부 음악 없이 22.05 kHz/16-bit/mono Original WAV 세 개를 결정론적으로 생성한다. Normal 100 BPM/153.60초, MiniBoss 120 BPM/24.00초, Boss 140 BPM/약 82.29초. 마디 단위 루프와 순환 잔향을 사용하며 Unity Import는 Streaming/Vorbis 0.8 설정이다. 세 곡은 Vertical Slice의 Gameplay 흐름과 전투 중요도 위계 확인을 위한 Original Prototype Asset이며 최종 WAV로 확정하지 않았다.
- Codex는 정적 WAV 규격·길이·경계 불연속(16-bit 샘플 차이 0/56/32), 359개 `.meta` GUID 고유성, Profile의 세 Clip 참조, 트리거 위치와 `git diff --check`를 확인했다. 사용자는 Unity에서 Import·컴파일과 Area 1 Play를 확인했다. Gameplay 시작 Normal, Giant Tuna의 MiniBoss 전환·종료 후 처리, Shark Boss 전환과 Attempt 사이 Track 유지, Result/Restart/New Run의 수명, x1/x2/x3의 정상 속도·Pitch를 검증했다. 기존 Tool/Squid/Puffer와 Giant Tuna/Shark Warning·Charge SFX가 BGM 위에서도 정상적으로 들리고 Gameplay 정보를 심각하게 가리지 않으며 관련 Console Error/Exception은 없었다. 음악의 작곡·음색 완성도에는 아쉬움이 있으나 현재 Vertical Slice Prototype 용도로 승인하고 추가 제작 없이 다음 작업으로 진행한다. Composition·Arrangement·Instrumentation·Mix·Mastering과 WAV 교체 여부는 Final Production에서 재검토할 수 있다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Static Validation: Passed (위 Codex 정적 검사 범위) / EditMode Tests: Not Run / Unity Manual Audio Validation: Passed / Prototype BGM Approval: Approved / Final Production Approval: Pending.** Scene/Prefab/ProjectSettings와 기존 Gameplay/SFX 수치는 변경하지 않았다. 이번 문서 정리에서는 코드·WAV·Profile·Test를 변경하거나 git add/commit/push를 하지 않는다.

## Area 1 MiniBoss / Boss Charge Warning·Start Prototype SFX (2026-09-29, Unity 수동 청각 검증 완료·Prototype 승인)

- 사용자 청취에서 기존 짧은 “퉁” 계열이 Telegraph Warning보다 실제 Charge Start에 자연스럽다고 판단했다. 따라서 기존 참치·상어 별도 WAV `GiantTuna_ChargeTelegraph.wav`(0.26초), `SharkBoss_ChargeTelegraph.wav`(0.36초)는 **보존**하고 파일명은 역사적 이름으로 유지하되 재생 역할을 Charge Start로 옮겼다. 이후 새 Warning+Charge 타이밍도 사용자 Unity Play Mode에서 검증·승인했다.
- 새 `GiantTuna_ChargeWarning.wav`(0.34초, 상승하는 가벼운 수중 압력)와 `SharkBoss_ChargeWarning.wav`(0.40초, 더 낮고 무거운 상승형 경고)를 각 Telegraph 시작에 한 번 요청한다. 점멸 중 추가 요청은 없고, 실제 돌진 속도 배율을 적용한 직후 기존 소리를 한 번 요청한다. Shark Phase 2/3은 각각 같은 Warning/Charge 쌍을 사용한다. Telegraph·돌진·Phase·시각·Gameplay 수치는 유지한다.
- 네 파일은 PCM mono/44.1 kHz/16-bit의 프로젝트 내부 생성 Prototype이다. 기존 Charge 파일의 볼륨 0.44/0.50을 유지하고 새 Warning은 참치 0.42, 상어 0.48이다. `FishVisualProfile`이 두 역할의 클립/볼륨을 보유하고 `ItemEffectManager` 공용 one-shot Source를 공유한다. Pause·준비·Run 종료 요청 제한 및 공용 Source Pause/UnPause/Stop은 유지한다. 새 Warning 생성기는 `Tools/generate_charge_warning_sfx.ps1`이다.
- `ChargeTelegraphAudioTests`는 두 역할의 Asset 참조와 Warning → Charge 요청 순서, 점멸·돌진 중 중복 없음, 참치 다음 예고, 상어 Phase 2/3 및 Disable 후 재등장을 검사하도록 갱신했으나 최종 코드 기준으로 실행하지 않았다. Codex는 WAV 헤더·길이·Profile GUID 참조·트리거 위치·`git diff --check`를 정적으로 확인했다. 사용자는 Unity Area 1 Play Mode에서 컴파일·실행을 확인하고, Giant Tuna의 예고음 1회·점멸 중 무반복·실제 Charge음 1회·볼륨과 두 역할의 구분을 검증했다. Shark Phase 2/3도 같은 순서와 보스다운 위계·재등장 후 정상 재생·중복 없음을 확인했다. 두 종 모두 기존 Telegraph Visual과 Audio Timing이 자연스럽고 기존 Tool/Squid/Puffer SFX와 충돌하지 않으며 Gameplay Timing 변화가 없었다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play 실행) / Static Validation: Passed (Codex의 WAV·GUID·트리거·diff 검사 범위) / EditMode Tests: Not Run (최종 코드) / Unity Manual Audio Validation: Passed / Prototype Audio Approval: Approved / Final Production Approval: Pending.** Capture/Escape/Result/Reward/Phase Change/BGM, Element/Synergy, Mixer/Bus와 최종 마스터링은 이번 승인에서 제외한다. 이번 문서 정리에서는 Code·Audio Asset을 변경하거나 Git add/commit/push를 하지 않는다.

## Shark Boss Charge Telegraph Sprite binding 수정 (2026-09-29, Unity 수동 검증 완료·Prototype 승인)

- VFX/SFX Audit와 사용자 Unity 확인에서 Phase 2/3 감속·HUD·돌진은 정상이나 실제 Shark 본체가 예고 중 점멸하지 않는 문제를 확인했다. `BossBehaviorController.Awake`가 프리팹 루트의 `SpriteRenderer`를 저장하지만, `FishVisualController.Initialize`는 그 루트를 숨기고 런타임 `FishPixelVisual` 자식에 Shark Sprite를 표시한다. 또한 Shark VisualProfile의 기본 Tint와 기존 예고 밝은 색이 모두 흰색이라 Renderer 참조만 바꿔도 대비가 없다.
- `FishVisualController.DisplayRenderer`로 현재 활성화된 실제 Fish Visual Renderer를 제공하고 Boss는 매 Telegraph 시작 때 이를 다시 바인딩한다. 기존 흰색 Tint를 기준으로 흰색일 때만 점멸 단계의 alpha를 0.35로 낮춰 본체가 보이게 점멸하도록 했다. 다른 Tint에서는 기존 흰색 단계와 원색 사이를 전환한다. 방향 전환은 동일 Renderer의 Sprite/flip만 바꾸므로 점멸 대상이 유지된다. Telegraph 종료 및 Boss Disable/Capture/Escape 때 바인딩된 Renderer의 원래 색을 복구하고 참조를 해제한다. 재등장·풀 재사용 시 Fish Visual 초기화와 다음 Telegraph 바인딩을 사용한다. Shadow 및 숨겨진 루트 Renderer는 변경하지 않는다.
- Phase 2/3 실제 Sprite 바인딩·색 변화/복구·방향 전환·Shadow/루트 비변경과 Disable/풀 재사용을 확인하는 EditMode 테스트 코드를 추가했다. 수정 당시 Unity Editor 프로세스가 열려 있어 Codex는 Unity Import/컴파일·EditMode 실행·Console과 Game View를 확인하지 못했다. 대신 소스·직렬화 흐름과 변경 diff를 정적으로 검사해 통과했다. 기존 Boss Phase/Charge 수치·타이밍·HUD·보상·Scene/Prefab/Asset은 수정하지 않았다. EditMode 테스트는 최종 코드 기준으로 실행하지 않았다.
- 후속 사용자 Unity Area 1 Boss Encounter 검증에서 Import/컴파일과 실제 Play가 정상이며 Phase 1, Phase 2/3 Charge Telegraph의 감속·실제 Shark 본체 Sprite 점멸·종료 후 원래 Color/Alpha 복구·Charge 실행이 정상임을 확인했다. 방향 전환 중에도 본체 점멸이 유지되고 1/2차 Escape 후 재등장에서도 정상이며, 숨겨진 Root Renderer와 Fish Shadow 등 다른 Renderer는 점멸하지 않았다. Capture/Run Fail 등 기존 Boss 흐름이 정상이고 이번 작업 관련 Console Error/Exception은 없었다. 기존 P1인 실제 Shark Visual Renderer에 점멸이 적용되지 않는 문제는 해결됐다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Static Validation: Passed (Codex의 기존 소스·직렬화 흐름 및 diff 검사 범위) / EditMode Tests: Not Run (최종 코드) / Unity Manual Validation: Passed / Boss Telegraph Prototype Approval: Approved / 기존 Shark Boss Art Prototype Approval: Approved (유지) / Final Production Approval: Pending.** 이번 문서 정리에서는 Code·Asset 변경, Unity Editor·Computer Use 실행, git add/commit/push를 하지 않는다.

## Squid Ink Attack P0 타이밍 수정 (2026-09-29, Unity 수동 검증 완료·Prototype 승인)

- VFX/SFX Audit에서 기존 `ReleaseInk`가 그물·낚싯대 `DisableTemporarily`를 공격 시작 전에 호출해, 원인인 먹물 Release·Projectile·Impact보다 도구 중단이 먼저 보이는 P0 문제를 확인했다. 아래 VS-2C 기록의 즉시 중단 설명은 수정 전 이력이다.
- 대상 종류·범위·중복 제거·공격 간격·2.5초 중단 시간은 유지한다. 공격 시작 시 대상 참조와 위치를 저장하고 기존 8 FPS 공격의 세 번째 프레임에서 Release한다. 기존 Scene의 0.22초 Projectile 이동 시간을 `ItemEffectManager` 설정에서 읽어 Controller의 대체 타이머와 Presentation에 함께 사용한다. 정상 Impact 콜백 또는 Controller 타이머 중 먼저 도달한 경로가 단발 완료 가드를 통과해 대상 유효성·원위치·Run 및 물고기 생명주기를 다시 확인한 뒤 중단한다. 이동·제거·비활성화된 대상에는 적용하거나 재표적하지 않는다. 다른 먹물 효과와 겹치면 기존 도구 API의 종료 시각 최댓값 정책을 유지한다. Pool 정리 시 Presentation 콜백은 폐기하지만 Gameplay 타이머는 유지한다. Presentation 프로필·VFX Manager 누락 또는 Pool 포화에도 예상 Impact 시점에 유효 대상 중단을 적용한다. 오징어 비활성화·Run 종료는 지연 효과를 취소한다.
- Squid Sprite, Puff·Projectile·Impact PNG, Release SFX, FPS·프레임·2.5초 중단 시간, Puffer와 다른 도구/어종은 수정하지 않았다. 타이밍·대상 소실·중복·정리 계약의 EditMode 테스트 코드를 갱신했으나 이번 최종 코드 기준으로 실행하지 않았다. Codex의 이전 정적 참조·직렬화 값 검토와 `git diff --check`는 통과했다. 별도 MSBuild 시도는 .NET Framework 4.7.1 참조 어셈블리 부재로 실패했다. 이후 사용자가 실제 Unity Area 1 Play Mode에서 공격 시작·Release 전·Projectile 이동 중 도구 정상 작동, Impact와 거의 동시 중단, 암전 연결, 약 2.5초 뒤 복구, 낚싯대·그물 대상, 기존 Squid VFX/SFX와 자연스러운 인과관계를 확인했고 이번 수정 관련 문제는 없었다. 실제 Unity Play 실행으로 소스 컴파일을 확인했으며 Console Error 0이나 EditMode 실행 결과는 별도로 확인한 것으로 기록하지 않는다. 기존 P0인 선행 도구 중단은 해결됐다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Static Validation: Partial (기수행 정적 검사 범위) / EditMode Tests: Not Run (이번 최종 코드) / Unity Manual Validation: Passed / 기존 Squid Art Asset Prototype Approval: Approved / Squid Timing Fix Prototype Approval: Approved / Final Production Approval: Pending.** 사용자 지시에 따라 git add/commit/push는 하지 않는다.

## Area 1 Water Motion / Background Architecture 현재 기준 (2026-09-29, Prototype 승인)

- 사용자 Unity Area 1 Game View에서 강도 조정 후 미세한 Water/Caustics Motion이 실제로 보이고 과하게 튀지 않음을 확인했다. 초기 alpha 0.26·속도 (0.025, 0.012)는 너무 약했다. **현재 코드 기준은 Renderer alpha 0.40, 속도 (0.035, 0.018) world unit/초**다. 아래 강도 조정·최초 구현 항목의 Pending과 이전 수치는 해당 시점의 이력이다.
- 현재 Base Background는 `CoastBackground.png` **Static Single PNG Prototype**이다. 원본 PNG는 이번 Water 작업에서 수정하지 않았다. Background와 1레이어 Water Overlay는 Main Camera에 런타임 연결되고 Fish Shadow는 Fish 자식으로 표시되는 별도 Presentation Layer다. Overlay는 반복 Sprite를 화면에 맞춰 타일로 배치하고 `Time.unscaledDeltaTime`으로 천천히 이동해 x1/x2/x3 Gameplay Speed와 독립적이다. Default Sorting은 Background -1000 < Water -999 < Fish Shadow -1 < Fish 0이며 Collider·Input·Gameplay 영향은 없다. 현재 표현은 Vertical Slice Prototype 기준으로 승인됐다.
- **Final Production Background Architecture: Pending / TBD.** 현재 PNG는 Prototype Base로 계속 사용하며, Production 단계에서 필요하면 Base Background와 Decoration/Water Presentation을 더 모듈화하거나 Layered 구조로 확장할 수 있다. 기존 PNG를 반드시 폐기한다는 결정은 없고 지금 구조를 재구축하지 않는다.
- **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play 가능) / Static Asset·Reference Validation: Passed (기존 PNG 규격·alpha·GUID 고유성·Resources 경로·정렬·Collider/Input 소스 점검 범위) / EditMode Tests: Not Run (최종 조정 후) / Unity Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Approval: Pending.** 사용자가 Console Error 0이나 전체 Gameplay·VFX 항목별 재검증 결과를 별도로 전달한 것은 아니다.

## Area 1 Water Overlay 강도 1차 조정 (2026-09-29, Unity 재검증 대기)

- 사용자 실제 Game View 검증에서 기존 물빛은 "뭐가 바뀌었는지 잘 모르겠다"고 느낄 만큼 약했다. `Area1WaterOverlay`의 Renderer alpha를 **0.26 → 0.40**, scroll speed를 **(0.025, 0.012) → (0.035, 0.018) world unit/초**로 상향했다. PNG·단일 레이어·타일 수·Default Sorting(-999)·unscaled time·Collider 없음·배경 원본·Fish Shadow·Gameplay는 그대로다. PNG 최대 alpha 76/255 기준 최대 합성 alpha는 약 0.077 → 0.119다.
- 아래 2026-09-28 기록의 값은 조정 전 기준이다. 새 강도의 Unity Import/컴파일·Console·EditMode·Game View 재검증은 아직 수행하지 않았다. **Implementation: Complete / Static Validation: Partial / Unity Manual Validation: Pending / Prototype Approval: Pending / Final Production Approval: Pending.**

## Area 1 Subtle Water Motion / Overlay 1차 (2026-09-28, Unity 수동 검증 대기)

- Scene/Prefab 변경 없이 기존 `Area1BackgroundController`가 Main Scene의 Main Camera에 `Area1WaterOverlay`를 붙인다. 배경 Sprite와 카메라의 런타임 연결을 재사용하며 `Resources/Area1/CoastWaterCaustics.png`의 256×256 RGBA/16 PPU/Point/무 Mipmap/무압축 타일을 로드한다. 배경 원본 PNG와 Fish·Shadow·Route·Tool·UI·Gameplay는 변경하지 않았다.
- 같은 Default Sorting Layer에서 배경 -1000 < 물빛 -999 < Fish Shadow -1 < Fish 0이다. 배경의 공유 Sprite Material을 사용한다. 카메라 Orthographic Size 6.5, 전체 Viewport, 기본 16:9에서 표시 범위는 약 23.11×13 world unit이고 물빛 타일은 16×16 world unit이다. 초기 3×3 타일은 카메라 비율/크기에 따라 필요한 수로 갱신한다. Collider, Raycaster, 입력 컴포넌트는 없다.
- 단일 레이어의 낮은 밝기 픽셀 카우스틱스를 Renderer alpha 0.26으로 표시한다. PNG alpha는 0/24/50/76으로 유효 최대 합성 alpha는 약 0.077이다. `(0.025, 0.012)` world unit/초로 움직이고 `Time.unscaledDeltaTime`을 사용하여 x1/x2/x3 배속에서 물빛 속도를 일정하게 유지한다. 반복 가능한 타일과 한 타일 단위 위치 순환으로 화면 가장자리를 채운다. 최종 강도·반복 무늬·Fish Shadow 대비는 사용자 Game View 검증에서 조정한다.
- 정적 검사: PNG 크기/alpha 분포, 새 GUID 고유성, 소스의 Sorting·Collider/Input 부재와 `git diff --check`를 확인했다. EditMode 테스트는 새 자산 설정·중복 생성·정렬·Collider 부재·카메라 맞춤·이동을 검사하도록 확장했다. 열린 Unity 인스턴스로 인해 이번 변경의 Unity 컴파일, Asset Import, EditMode 실행, Console은 아직 확인하지 못했다. **Implementation Complete: Complete / Static Validation: Partial / Unity Manual Validation: Pending / Prototype Approval: Pending / Final Production Approval: Pending.** Git add/commit/push는 사용자 지시에 따라 하지 않는다.

## Fish Shadow 가시성 수정 (2026-09-28, 사용자 수동 검증 완료·Prototype 승인)

- 사용자 Area 1 Game View 검증에서 Fish는 정상이나 그림자가 거의 식별되지 않았다. 이전 구현의 PNG 알파 72/112/144에 Renderer alpha 0.48이 다시 곱해져 실제 중심 알파가 최대 약 0.271이었다. 그림자 일부는 Fish Sprite 아래에 가려지므로 노출 면적과 대비가 더 줄었다. 정어리 기준 그림자 월드 크기는 약 0.301×0.170, 아래 간격은 약 0.069(83 PPU에서 약 25×14px, 5.76px)다. 같은 Default Sorting Layer에서 배경 -1000, 그림자 -1, Fish 0으로 정렬되며 PNG의 Runtime Import 기록이 있어 Sorting/자산 누락보다 낮은 합성 알파가 확인된 원인이다. 다만 실제 Play Hierarchy 상태는 이번 작업에서 직접 관찰하지 않았다.
- 최소 수정으로 `FishVisualController`의 Renderer alpha만 0.48→0.8로 올렸다. PNG·크기·간격·Sorting·Fish Art는 유지한다. 실제 알파는 72/112/144 기준 약 0.226/0.351/0.452다. 일곱 FishData의 실제 프로필을 사용하는 테스트에 Sprite/활성/Sorting/월드 크기/중심 합성 알파 검사를 추가했다.
- PNG 중앙 알파·Import 설정·일곱 FishData/Profile 연결·GUID 342개 고유성·정어리 계산값·변경 파일 diff 공백 검사는 통과했다. 당시 열린 Unity 인스턴스 때문에 Codex가 변경 후 EditMode 테스트와 Console을 확인하지 못했다. 이후 사용자가 실제 Area 1 Game View를 Play해 현재 C# 소스가 실행을 막지 않음을 확인했다. **EditMode Tests: Not Run**이며 Console Error 0은 별도로 확인된 결과로 기록하지 않는다.
- 사용자가 수정 결과에서 밝은 Turquoise Ocean 위 그림자 표시, Fish보다 낮은 시각 우선순위, 여러 Fish가 함께 있을 때의 절제된 진하기와 혼잡도, Fish 아래 수중 깊이감, 적절한 크기·Offset 및 기존 Fish Sprite/Background 가독성을 확인했다. Renderer alpha 0.8과 폭 0.78·높이 0.44·아래 간격 0.18을 **Prototype Visual Baseline**으로 승인했다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Asset·Reference Static Checks: Passed / EditMode Tests: Not Run / Unity Manual Validation: Passed / Prototype Approval: Approved / Final Production Approval: Pending.** 이전 1차 구현의 alpha 0.48과 수동 가시성 실패·대기 기록은 수정 전 이력이다. Water Motion과 Water Overlay는 각각 별도 후속 단계다. 이번 문서 마감에서 Computer Use 및 git add/commit/push는 수행하지 않는다.

## Fish Shadow / Underwater Depth Presentation 1차 (2026-09-28, 가시성 수동 검증 실패·수정 전 기록)

- `FishVisualController`가 유효한 `FishVisualProfile`로 표시되는 Fish마다 `FishUnderwaterShadow` 자식 SpriteRenderer를 만들고 공통 `Resources/Fish/FishShadow` Sprite를 표시한다. 정어리·고등어·참치·복어·오징어·거대 참치·상어 보스의 기존 FishData/Profile 연결을 공통 경로로 사용한다. 프리팹·씬 직렬화와 Gameplay/Collider/Input/경로/Resistance/보상은 변경하지 않았다.
- 그림자는 32×16 RGBA 픽셀 타원, 83 PPU, Point, Mipmap Off, 무압축이다. 기준 수영 Sprite 셀의 월드 크기에 폭 0.78·높이 0.44를 곱하고 높이 0.18만큼 아래로 둔다. Sprite 자체의 남색 3단계 알파와 Renderer 불투명도 0.48을 조합한다. Fish와 같은 Sorting Layer에서 Fish보다 order 1 낮게 배치한다. Area 1 배경 order -1000과 Fish order 0 사이의 -1이며 방향별 flip/프레임과 독립이다.
- `FishController.Initialize` 호출 때 크기·위치·표시를 갱신한다. 프로필이 없으면 숨기고 재사용 시 다시 표시하며, Fish 루트 Disable/Destroy 때 함께 사라진다. `FishVisualPipelineTests`에 자산·정렬·콜라이더 부재·추적·풀 재사용 검사를 추가했다.
- PNG 32×16 RGBA·Import 83 PPU/Point/Single/Mipmap Off/무압축·Assets GUID 342개 중복 없음·코드 참조·`git diff --check`를 정적으로 확인했다. Unity 배치 EditMode 검증은 같은 프로젝트를 다른 Unity 인스턴스가 사용 중이라 시작되지 못했다. 따라서 C# 컴파일·테스트·Console은 미검증이다.
- **Implementation Complete: Complete / Static Validation: Partial (asset/reference checks passed; Unity compile/tests pending) / Unity Manual Validation: Pending / Prototype Approval: Pending / Final Production Approval: Pending.** 실제 화면에서 불투명도·크기·간격·어군 혼잡도를 조정할 수 있다. Water Motion/Overlay는 별도 후속 작업이다. 사용자 지시대로 Computer Use와 git add/commit/push는 수행하지 않는다.

## Area 1 MiniBoss Giant Tuna Sprite / Presentation 1차 연결 (2026-09-28, Prototype 승인)

- Area 1 MiniBoss 어종을 거대 참치로 확정했다. 기존 Tuna를 중심으로 정어리·고등어·복어·오징어 및 승인된 Boss Shark의 픽셀 밀도와 실루엣·팔레트를 참고했다. ImageGen 초안은 형태 참고만 했고 실제 게임용 `Tools/generate_coast_miniboss_tuna.py`에서 88×88 셀, E/N/NE/NW 4방향×4프레임의 탑다운 RGBA 수영 시트를 제작했다. 일반 Tuna보다 긴 유선형 몸통·큰 갈라진 꼬리·가는 지느러미·소량의 금색 finlet으로 구분한다.
- `Assets/Art/Fish/CoastMiniBoss/CoastMiniBoss_Swim.png`와 분할 `.meta`, `CoastMiniBoss_VisualProfile.asset`과 `.meta`를 추가하고 기존 `FishData_CoastMiniBoss.asset`에 Profile 참조 한 필드만 연결했다. 공용 `FishVisualController`가 사각형 fallback을 숨기고 방향 프레임을 표시한다. Animator/돌진 전용 프레임은 없다. 기존 `MiniBossController`의 예고 점멸은 실제로 보이는 자식 Renderer를 선택해 흰색/금색으로 표시하도록 최소 보정했다. 돌진 속도·시간·판정·Resistance 220·E 보상은 유지한다.
- `FishArtSetup`과 `FishVisualExpansionTests`의 자산·방향 기대값을 갱신했다. PNG 352×352 RGBA, 16개 88×88 분할·Sprite ID·Profile/FishData/Main Scene 참조·GUID 중복 없음·기존 MiniBoss 수치 및 수정 코드 diff를 정적으로 확인했다. 첫 Unity 배치 실행은 Package Manager IPC 연결 실패로 중단됐지만, 재실행에서 Import·C# 컴파일과 `FishArtSetup.Validate`가 성공했다. 관련 EditMode `FishVisualExpansionTests`는 **23/23 통과**했다. 관련 C# 컴파일 오류·Exception은 없고 기존 UI의 TMP `enableWordWrapping` 폐기 경고 3건이 남아 있다. **Implementation: Complete / Static Validation: Complete / Unity Compile·Import·Pipeline Validate·Related EditMode Validation: Passed / Unity Manual Play Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending.** Scene/Prefab, Boss Shark와 기존 일반 Fish 자산·게임플레이·UI를 수정하지 않았고 Computer Use, 임시 Unity 프로젝트, git add/commit/push는 수행하지 않았다.
- 후속 사용자 Unity 수동 검증에서 Area 1 MiniBoss Encounter의 거대 참치 Sprite 표시·Import·VisualProfile 연결, 방향 전환·수영, 일반 Tuna/Fish 대비 크기와 Boss Shark와의 스타일 정합성을 확인했다. MiniBoss HUD·Resistance, 돌진 예고 점멸·이동 중 Sprite, 포획·E Reward·도주 흐름이 정상이고 일반 Fish/Boss Shark 이상 및 이번 작업 관련 Console Error/Exception이 없었다. **Implementation Complete: Complete / Static Validation: Complete / Unity Manual Validation: Passed / Prototype Approval: Approved / Final Production Approval: Pending.** 위 수동 검증 대기 표기는 이전 작업 시점의 이력이다.

## Area 1 Boss Shark Sprite / Presentation 1차 연결 (2026-09-28, Prototype 승인)

- Area 1 Boss 어종을 상어로 확정했다. 정어리·고등어·참치·복어·오징어의 방향 시트, 83 PPU, Point/무압축 Import, 짙은 외곽과 청록 면 분리를 참고했다. ImageGen 초안의 상어 지느러미·꼬리 실루엣을 참고하되 실제 게임용으로는 탑다운 픽셀 격자에서 `Tools/generate_coast_boss_shark.py`로 96×96 셀, 4방향(E/N/NE/NW)×4프레임의 투명 RGBA 시트를 제작했다.
- `Assets/Art/Fish/CoastBoss/CoastBoss_Swim.png`와 분할 `.meta`, `CoastBoss_VisualProfile.asset`과 `.meta`를 추가했다. Profile은 8 FPS, visualScale 1, white tint다. 기존 `FishData_CoastBoss.asset`에 Profile 참조 한 필드만 추가했다. 공용 `FishVisualController`가 root placeholder를 숨기고 자식 SpriteRenderer에 방향 프레임을 표시하며 반대 방향은 flipX+flipY다. Animator는 사용하지 않는다.
- `FishArtSetup` 검증 범위와 `FishVisualExpansionTests`의 Boss 기대값을 갱신했다. Boss gameplay·3회 회유·Phase·Resistance·HUD·R 보상·Run 결과, MiniBoss, Scene/Prefab, 기존 5종 물고기 자산은 수정하지 않았다. 임시 미리보기 파일은 최종 자산이 아니다.
- PNG 384×384 RGBA, 16개 96×96 분할, Sprite 참조·GUID 중복 없음·Main Scene의 기존 Boss FishData 참조, `git diff --check`를 정적으로 확인했다. 같은 프로젝트를 연 Unity 인스턴스가 있어 배치 모드 컴파일/Validate는 프로젝트 중복 열기 오류로 중단됐다. 따라서 Unity import·C# 컴파일·Console·EditMode·Play Mode는 이번 세션에서 확인하지 못했다. **Static Asset/Reference Validation: Complete / Unity Import·Compile·Console·Gameplay/Visual Validation: Pending / Boss Art Approval: Pending.** 사용자 수동 검증 전 Passed로 표시하지 않는다. 요청에 따라 Computer Use, 임시 Unity 프로젝트, git add/commit/push는 수행하지 않았다.
- 후속 사용자 Unity 수동 검증에서 상어 Sprite import·VisualProfile 연결, 방향 전환·기본 수영, 일반 Fish/MiniBoss 대비 크기와 실루엣을 확인했다. 실제 Boss encounter에서 Resistance HUD·Phase·1/2차 도주 후 재등장과 Resistance 회복·3차 도주 Run Fail·포획 후 R Reward/결과 흐름이 정상이고 기존 Fish/MiniBoss 동작 이상과 이번 작업 관련 Console Error/Exception이 없음을 확인했다. **Implementation Complete: Complete / Static Validation: Complete / Unity Manual Validation: Passed / Prototype Approval: Approved / Final Production Approval: Pending.** 위 배치 모드 중단과 당시 미검증 기록은 이전 작업 시점의 이력이며, 별도 EditMode Test Runner 통과를 의미하지 않는다.

## Area 1 TAB 성장 관리 UI Prototype 승인 (2026-09-28)

- 사용자가 Unity Game View에서 현재 성장 관리 UI를 직접 확인했다. Area 1 메인 HUD와 같은 해양 픽셀 UI 계열, 관리 창의 구조·정보 계층, Galmuri11 계열의 짧은 텍스트, 스킬 트리/아이템 탭의 밝은 청록 선택·어두운 청록 비선택 상태를 Prototype 기준으로 승인했다. 비선택 탭의 노란/로프색 강조 문제는 해결됐다.
- 아이템 페이지의 4칸·01~04·`비어 있음` 표시와 공통 `icon_empty.png`의 픽셀 +를 확인했다. 메인 HUD와 TAB은 같은 Sprite를 쓰고, 메인 HUD는 기존 표시 크기, TAB은 빈 슬롯 전용 32×32 Simple Image/Offset (-4,-2)를 쓴다. 4개 +의 크기·위치 정렬이 확인됐다. 단일 시너지 구조와 복합 시너지 제목 프레임의 하단이 카드 영역을 침범하지 않는 것도 확인했다. Q/W/E/R 성장 영역의 기본 구조·잠금 UI, 기존 TAB/페이지 구조를 확인했으며 성장 로직은 이번 UI 작업에서 변경하지 않았다.
- **Implementation: Complete / Static Validation: Complete / Unity Manual Growth Management Visual Validation: Passed / Prototype Growth Management UI Approval: Approved / Final Production Growth Management UI Approval: Pending.** 이 승인은 Vertical Slice의 시각 UI 기준이다. 실제 아이템·성장 노드·시너지 콘텐츠가 더 채워진 뒤 출시용 UI를 재검토할 수 있다. 이번 승인만으로 별도의 Gameplay 회귀나 Console Error 0 검증을 완료했다고 기록하지 않는다. 아래 성장 관리 관련 Pending 표기는 승인 전 반복 작업의 이력이다.
- 이번 마감은 문서 3종만 갱신한다. 구현·아트·Scene/Prefab·Font Asset 변경, Unity/Play/Test Runner/Computer Use, 임시 프로젝트, Git add/commit/push는 수행하지 않는다. 전체 `git diff --check`의 기존 `NanumGothic-Bold SDF.asset` 공백 3곳은 별도로 구분해 보고한다.

## Area 1 TAB 빈 슬롯 + 표시 크기만 보정 (2026-09-28, 사용자 Unity 검증 대기)

- 최신 사용자 요청에 따라 메인 HUD의 +와 공통 `icon_empty.png`를 그대로 보존했다. `ItemHUD`와 `GrowthItemPage`는 모두 `Area1HUDSkin.Frame("icon_empty")`를 읽는다. 메인 HUD는 런타임 `Area1ItemIcon`이 32×32, 중앙 Anchor/Pivot, `anchoredPosition (0,-25)`로 84×82 슬롯 중심에 놓인다. TAB의 직전 `GrowthItemIcon`은 1920×1080 기준 약 192×204 슬롯의 38% 폭/높이(약 73×78)여서 같은 32×32 Sprite가 크게 표시됐다.
- TAB의 **빈 슬롯만** `GrowthItemIcon` Rect를 32×32, 중앙 Anchor/Pivot, 정수 `anchoredPosition (-4,-2)`로 바꿨다. 이는 기존 TAB 안쪽 프레임의 시각 중심에 맞춘 작은 좌·하 이동이다. 기존 공용 스킨에서 아이콘에도 적용됐던 `Image.Type.Tiled`를 빈 +에 한해 `Simple`로 표시하고 `preserveAspect=true`를 유지한다. 보유 아이템 아이콘은 이전 Rect 영역/Tiled 타입을 그대로 사용하며, 슬롯 크기·번호·`비어 있음`·탭·시너지·툴팁·버튼과 기능 연결은 유지한다. 메인 HUD 코드/Rect와 공통 PNG/`.meta`/GUID는 수정하지 않았다.
- Scene 직렬화상 TAB의 ItemPage→OwnedItems→OwnedItemSlot Scale은 모두 1이고 슬롯 부모에 LayoutGroup/ContentSizeFitter가 없다. 공통 Sprite Import는 Point, 무압축, Mipmap Off, PPU 32다. CanvasScaler는 1920×1080 Match 0.5이고 Pixel Perfect는 꺼져 있으므로 실제 출력 해상도별 최종 픽셀 정렬은 Unity 확인이 필요하다. 이번 변경 파일의 `git diff --check`는 통과했고 전체 검사는 시작 전부터 변경돼 있던 `NanumGothic-Bold SDF.asset`의 trailing whitespace 3곳에서 실패한다. **Scoped Static Validation: Complete / Unity Visual·Console Validation: Pending / Approval: Pending.** 임시 프로젝트·Unity·Play·Test Runner·Computer Use·Git add/commit/push 미수행.

## Area 1 공통 + 두께·TAB 슬롯 중심 보정 (2026-09-28, Unity 검증 대기)

- 사용자 제공 현재 TAB 화면을 기준으로 보유 아이템 4칸의 +를 안쪽 청록 사각 프레임 중심에 맞췄다. `GrowthManagementSkin`의 `GrowthItemIcon` 영역 중심을 슬롯 기준 `(0.50, 0.55)`에서 `(0.48, 0.50)`으로 옮겼다. 슬롯/번호/`비어 있음`/탭/시너지/버튼의 배치와 이벤트는 유지했다. 메인 HUD의 `Area1ItemIcon`은 기존 84×82 슬롯 정중앙 `(0.50, 0.50)`에 있어 위치 코드를 변경하지 않았다.
- `Assets/Resources/UI/Area1/icon_empty.png`의 십자 막대를 직전 8px에서 **2px**로 축소했다(픽셀 격자에서 가능한 75% 두께 감소, 최초 10px 체감 기준 약 80%). 양축 범위 `3..28`의 26px 길이와 이미지 중심 `(15.5, 15.5)`는 유지한다. 4톤(어두운 끝, 로프 중간, 밝은 면, 청록 포인트)의 단순한 픽셀 음영을 쓴다. `Tools/refine_area1_empty_icon.js`도 같은 결과를 재생성하도록 갱신했다. 메인 `ItemHUD`와 TAB `GrowthItemPage`는 동일한 공용 Sprite를 계속 읽는다.
- 기존 `.meta`/GUID, Scene/Prefab, 메인 HUD 표시 코드, Gameplay/입력/아이템·시너지 로직은 건드리지 않았다. 이번 세션 시작 시 이미 변경돼 있던 Galmuri11/Nanum TMP Font Asset 3종도 보존했다. 전체 `git diff --check`는 기존 `NanumGothic-Bold SDF.asset`의 trailing whitespace 3곳으로 실패했고, 이번 수정 파일만의 `git diff --check`와 신규 소스의 공백 검사는 통과했다. PNG 규격·중앙·두께·공용 경로·GUID도 정적으로 확인했다. Unity Import·컴파일·Console·Play·수동 시각 검증은 사용자 담당이며 승인 **Pending**이다. Git add/commit/push와 임시 프로젝트 생성 없음.

## Area 1 TAB 성장 관리 UI 후속 정리·공통 빈 슬롯 십자 (2026-09-28, 사용자 검증 대기)

- `vertical-slice`의 직전 미커밋 성장 관리 스킨 작업을 유지한 채 표시만 조정했다. 메인 HUD `ItemHUD`와 TAB `GrowthItemPage`가 모두 `Area1HUDSkin.Frame("icon_empty")`를 쓰는 것을 코드로 확인했다. 이 공용 Sprite는 비어 있는 Q/W 도구 표시와 알 수 없는 아이템의 fallback에도 쓰이므로 해당 십자 모양도 함께 바뀐다. 기존 `Assets/Resources/UI/Area1/icon_empty.png` 한 장만 32×32 RGBA8의 가운데 정렬된 픽셀 십자로 다시 그렸고 `.meta`/GUID `cd6e4b5cb2af00c1e9ce18d88d049d35`는 보존했다. 기존 십자의 주 몸통 약 10px을 8px로 줄이고 양축 길이는 약 26px로 유지했다. 짙은 외곽·로프 중간색·밝은 모서리·작은 청록 음영을 사용한다. `Tools/refine_area1_empty_icon.js`는 이 PNG만 재생성한다.
- TAB 빈 아이템 슬롯의 아이콘 영역을 세로 중앙에 더 가깝게 내리고 상태 텍스트 영역을 분리했다. 메인 HUD의 프레임·아이콘 RectTransform·배치·텍스트·기능은 수정하지 않았다. 두 화면은 여전히 동일한 `icon_empty` Sprite를 사용한다.
- 성장 관리 스킨의 비선택 탭을 노란 목재 `button`에서 어둡게 착색한 청록 `selected_slot`로 바꾸고, 선택 탭은 같은 Sprite의 밝은 원색을 유지했다. 탭/페이지 이벤트와 기존 선택 제한은 그대로다. `CombinedSynergies` 제목 띠의 하단 Anchor를 `0.81→0.875`로 올려 아래 카드 영역(`0.85` 상단)과 간격을 확보했다. 다른 섹션 띠와 하단 상세 설명의 배치는 유지했다.
- 수정/생성: `GrowthManagementSkin.cs`, `icon_empty.png`, 신규 `Tools/refine_area1_empty_icon.js` 및 본 문서·아트 가이드·에셋 목록. 앞선 미커밋 성장 UI 변경과 분리해 삭제하거나 되돌리지 않았다. Scene/Prefab, PNG `.meta`, 게임플레이, 입력, 데이터와 메인 HUD 코드에는 변경이 없다. **Static Validation: Complete / Unity Import·Visual·Interaction Validation: Pending / Prototype Approval: Pending.** 사용자 검증: 메인/TAB 4칸의 같은 십자와 중심, 보유 아이템으로 교체, 선택·비선택 탭 대비, 복합 시너지 제목 하단과 카드 간격, 작은 화면 한글/툴팁/버튼, Console 확인. Unity 실행·임시 복사 프로젝트·Play·Git add/commit/push 미수행.

## Area 1 TAB 성장 관리 UI 해양 스타일 정리 (2026-09-28, 사용자 검증 대기)

- 목적: 기존 Area 1 메인 HUD와 TAB 성장 관리 창을 같은 해양 픽셀 UI 세트로 맞추고, 전체 창·헤더·탭·도구 트리·아이템·시너지·툴팁의 정보 계층과 가독성을 정리한다. Scene의 `SkillTreeUI/TreePanel` 직렬화 참조와 배치, TAB 입력, 성장/아이템/시너지 판정은 유지한다.
- `GrowthManagementSkin.cs`와 `.meta`를 추가했다. 기존 `Assets/Resources/UI/Area1/`의 `panel`, `header`, `button`, `slot`, `selected_slot`, `ref_item_slot`, `icon_empty`, 6종 아이템 아이콘과 조개·산호·잎·로프 장식을 재사용한다. 신규 PNG/PNG `.meta`는 0개이며 기존 PNG/GUID는 수정하지 않는다. 창은 어두운 해양 오버레이 위의 둥근 픽셀 프레임, 목재 헤더와 표지판형 활성/비활성 탭으로 표현한다.
- 스킬 트리는 Q/W/E/R Branch 배경과 도구 Root/동적 성장 노드에 같은 프레임을 적용하고, 연결선과 노드의 잠김·선택 가능·완료 상태를 청록·회청·모래색으로 구분한다. 아이템 페이지는 4칸 각각에 번호/아이콘/이름·상태 영역을 나누고, 전기·검·얼음 단일 시너지와 복합 카드/설명/변경 버튼을 별도 섹션으로 묶는다. 툴팁은 같은 프레임을 사용하며 기존 최상위 표시 경로를 유지한다.
- Typography: 짧은 제목·탭·버튼·노드/상태는 Galmuri11 Bold 또는 Galmuri11, 숫자·레벨·숙련 포인트는 Galmuri11을 사용한다. 긴 복합 시너지 상세/카드 설명과 성장·아이템 툴팁은 기존 NanumGothic을 보존한다. 텍스트는 영역 내 자동 크기·줄바꿈/잘림을 설정했다. 실제 한국어 줄바꿈과 세로 정렬은 Unity 확인 대기다.
- 코드 변경은 `SkillTreeCanvas.cs`, `SkillTreeBranchView.cs`, `SkillTreeNodeView.cs`, `GrowthItemPage.cs`, `SkillTreeTooltip.cs`의 표시 부분이다. 기존 버튼 이벤트, TAB 열기/닫기, 페이지 기억/전환, Q/W/E/R 획득·투자, 아이템 보유, 단일/복합 시너지 선택·쿨다운, 툴팁 내용/위치 계산은 유지한다. `Assets/Scenes/Main.unity`, Prefab, Gameplay 스크립트는 수정하지 않았다.
- 정적 확인: `git diff --check`, 리소스 PNG/`.meta` 존재·GUID 중복·코드 경로와 Main Scene의 연결 대상 확인. Unity Import·컴파일·Console·Play·수동 시각/상호작용 검증은 사용자 요청으로 수행하지 않는다. 사용자는 ① TAB 열기/닫기와 모달 계층 ② 스킬 트리/아이템 탭 전환과 필수 획득 제한 ③ Q/W/E/R 잠김·가능·완료 노드/연결선과 툴팁 ④ 4칸 빈/보유 아이템 아이콘·텍스트 ⑤ 단일/복합 시너지 카드·상세·버튼/툴팁 ⑥ 1920×1080 및 축소 Game View의 한글 잘림·Console을 확인해야 한다. **Static Validation: Complete / Unity Manual Validation: Pending / Prototype Approval: Pending.** Git add/commit/push 및 Computer Use 미수행.

## Area 1 UI Galmuri11 Typography Pass (2026-09-28, Prototype 승인)

- 공식 `quiple/galmuri` 저장소의 커밋 `71e1cacf1437a11220307120e63e30bc275312d4`에서 `Galmuri11.ttf`, `Galmuri11-Bold.ttf`, `dist/LICENSE.txt`를 원본 이름과 바이트 그대로 `Assets/UI/Fonts/Galmuri/`에 추가했다. 라이선스는 SIL Open Font License 1.1이다.
- `Assets/Resources/UI/Fonts/`에 Galmuri11 / Galmuri11 Bold Dynamic SDF TMP Font Asset이 생성되었다. 두 Asset은 각 원본 TTF를 참조하고 기존 NanumGothic-Bold SDF를 fallback으로 참조한다. 기존 Nanum 폰트와 Scene 직렬화 참조는 보존된다.
- `Area1Typography`를 통해 Area 1의 Major Title·주요 버튼/Heading·Key 강조에는 Galmuri11 Bold, HUD Body·숫자·짧은 상태 Text에는 Galmuri11을 사용한다. 긴 Tooltip과 설명문은 기존 NanumGothic 계열을 유지한다. 두 서체는 현재 Area 1 Prototype Typography의 기본 Font Family로 승인되었다. 전용 TMP Material preset/outline/glow는 추가하지 않고 기본 SDF Material과 기존 텍스트 색상을 사용한다.
- 사용자 Unity Game View에서 NETBREAK, 조업 준비/시작, 플레이 시간, 좌측 HUD Label/Value와 EXP, Inventory 제목·1/2/3/4·비어 있음, Hotbar LMB/Q/W/E/R·Tool 이름·잠김 상태, x1/x2/x3, 성장 관리 [Tab]의 Typography를 직접 확인했다. 현재 Galmuri11 스타일이 해양 Pixel UI와 어울리는 것으로 판단하여 Prototype Typography를 승인했다. 이 승인은 별도의 Gameplay 회귀 또는 Console Error 0 확인을 뜻하지 않는다.
- 공식 원본 파일·라이선스, 두 TMP Asset의 원본 TTF 및 Nanum fallback 참조, 기존 Nanum Asset 보존과 문서 상태를 정적으로 확인했다. 이번 문서 마감에서는 Unity/Computer Use/Play Mode/Test Runner를 실행하지 않았고 구현 코드·UI Art도 추가 수정하지 않았다. 전체 `git diff --check`는 이번 마감 전부터 변경돼 있던 `NanumGothic-Bold SDF.asset`의 공백 3곳을 지적하며, 이번에 수정한 문서 3개만의 검사는 통과한다. **Implementation: Complete / Static Validation: Complete / Unity Manual Typography Visual Validation: Passed / Prototype Typography Approval: Approved / Final Production Font Approval: Pending.** 최종 출시용 폰트 승인은 별도이며, Git add/commit/push는 사용자가 직접 한다.

## Area 1 UI 하단 프레임·간판 재질 수정 (2026-09-28, 사용자 Unity 검증 대기)

- 현 `vertical-slice` 작업 트리의 기존 39 PNG/26 `ref_` PNG 구조와 모든 `.meta`/GUID, 전체 UI 배치·상태·입력·슬롯·드래그 연결을 유지했다. 추가 수정된 PNG는 `ref_ready_board`, `ref_wave_strip`, `ref_hud_sign` 및 아이템/핫바/배속 보드·슬롯·연결 장식 등 13개다. 코드 수정은 `Area1HUDSkin.cs`의 장식 배치뿐이다.
- 준비판 원본 하단의 일자 로프와 짧은 양끝 파도를 삭제했다. `ref_wave_strip` 하나에 암석·로프 꼬임·이끼 하단 프레임과 연속 포말 한 줄을 합쳐 본체 바닥에 밀착했다. 본체 내부 물고기 실루엣과 야자수·찌는 유지한다. NETBREAK 간판은 166×46→174×44로 소폭 넓히고 본체 상단에 7px 더 걸쳐, 짧은 지지부가 프레임과 이어지게 했다. 갈매기는 여전히 좌측 간판 오른쪽에만 있다.
- 아이템·핫바·배속의 외곽과 연결부에 작은 암석·이끼 픽셀을 더해 같은 목재/로프/해양 재질감을 공유한다. 정적 미리보기와 결과 설명은 `Artifacts/MarineUIBoards/`에 갱신했다. 실제 Unity import·컴파일·Console·Play·수동 시각/상호작용 확인은 사용자 요청대로 미실시한다. 임시 복사 프로젝트 및 Git add/commit/push 없음. **Manual Visual Validation: Pending / Reference Match Approval: Pending.**

## Area 1 레퍼런스 아트 적용 갱신 (2026-09-28, 사용자 Unity 검증 대기)

- 최신 사용자 지시를 우선한다. 화면의 기본 배치·크기와 입력·상태 연결은 유지하면서, 첨부 해양 픽셀아트 시트의 HUD/준비판/목재 버튼 부분을 역할별로 재구성했다. 좌측 HUD는 분리된 짧은 NETBREAK 간판과 프레임에 통합된 야자수·갈매기·조개·불가사리를 쓴다. **갈매기는 NETBREAK 간판 오른쪽에만 있고 아이템 UI에는 없다.**
- 조업 준비 보드는 야자수·찌·희미한 물고기 무늬가 있는 전용 자산이다. 324×28 파도와 로프는 패널 하단에 걸치고 패널 아래로 약 10px만 나온다. 시간판·시작 버튼·성장 버튼은 동일 목재/밧줄 소재로 맞췄다. 아이템은 4칸과 번호 배지, 핫바는 5칸과 로프 연결/선택 강조, 배속은 3개 세로 세트다.
- 새 전용 자산은 `Assets/Resources/UI/Area1/ref_*.png` **26개**와 대응 importer 26개이며, 원본 39 PNG/39 `.meta`는 이전 묶음과 바이트 단위로 일치한다. `Artifacts/MarineUIBoards/Sources/TropicalMarineUISheet.png`를 사용한 재생성 스크립트와 역할표는 `Artifacts/MarineUIBoards/FINAL_REPORT.md`에 있다. 정적 화면 근사 미리보기는 `HUD_STATIC_PREVIEW.png`다.
- Scene/Prefab, 게임플레이, 입력, 드래그, 툴팁, 상태 업데이트 로직은 건드리지 않았다. Unity import·컴파일·Console·Play와 최종 시각/상호작용 검증은 사용자 요청대로 미실시한다. **Manual Visual Validation: Pending / Reference Match Approval: Pending.** 임시 Unity 프로젝트, Git add/commit/push 없음.

## Area 1 레퍼런스 실루엣 최종 패스 (2026-09-28, 사용자 검증 대기)

- 직전 패널별 아트 후보를 기준으로 공용 둥근 네모 느낌을 줄였다. HUD·시간·준비·아이템·핫바·성장 보드가 각기 다른 픽셀 외곽을 갖도록 6종을 다시 생성했다. NETBREAK 간판은 236→206px로 줄이고, 왼쪽 프레임에 타고 오른 픽셀 야자수 1종을 추가했다. 갈매기는 좌측 HUD에서 빼고 아이템 보드 위에 앉힌 전용 Sprite로 옮겼다.
- 조업 준비 하단 파도는 기존 큰 포말 원본을 336×48 전용 자산으로 제작해 패널 아래 22px까지 내려오게 했다. 핫바 아이콘은 38→45px, 기존 다중 명암 로프 접합부는 유지하며 외곽 end-cap을 강화했다. 배속 3개에는 투명한 세로 연결 프레임을 추가했다. 기존 해양 장식 10종의 픽셀 밀도를 맞춘 전용 변형을 새 파일로 추가해 원본 39 PNG와 .meta/GUID는 그대로 보존했다.
- 신규 전용 자산은 총 32 PNG/32 importer다. 정적 미리보기 `Artifacts/MarineUIBoards/HUD_STATIC_PREVIEW.png`를 갱신했다. PNG 파싱, 신규 GUID 고유성, 기존 39 PNG 바이트 보존을 확인했다. 실제 Unity import·컴파일·Console·Play·TMP·버튼/드래그 검증은 사용자 요청대로 수행하지 않았다. **Manual Visual Validation: Pending / Reference Match Approval: Pending.** Git add/commit/push 없음.

## Area 1 패널별 레퍼런스 아트 연결 (2026-09-28, 사용자 Unity 검증 대기)

- 최신 UI 요청에 따라 기존 39개 PNG와 GUID를 유지하면서 패널별 전용 픽셀 아트 19개 및 importer를 추가했다. 좌측 정보판/간판/EXP 트랙, 시간바, 물고기 무늬 준비판/목재 시작 버튼/긴 파도, 아이템 보드/번호 배지, 핫바 슬롯/로프 연결부, 성장/배속 버튼을 `Area1HUDSkin`과 `PrototypeHUDCanvas`의 표시 경로에 연결했다.
- 런타임 상태 값, 입력, 클릭, 드래그, 툴팁, 아이템/성장/배속/게임플레이 로직과 Scene/Prefab은 이번 작업에서 변경하지 않았다. 이전 미커밋 작업은 보존했다. 새 아트는 고정 화면 점유 크기에 맞춘 Simple Sprite이며 공용 원본 Sprite는 그대로 남는다.
- 정적 검사에서 새 PNG 19개, importer 19개, GUID 중복 없음, 코드 참조 존재를 확인했다. `Artifacts/MarineUIBoards/HUD_STATIC_PREVIEW.png`는 배치 근사 합성이다. 실제 Unity import, 컴파일, Console, Play, TMP 잘림·클릭/드래그·시야 검증은 사용자 요청에 따라 수행하지 않았다. **Manual Visual Validation: Pending / Reference Match Approval: Pending.** Git add/commit/push 없음.

## Area 1 Reference Frame Rebuild (2026-09-28, 사용자 검증 대기)

- 사용자가 첨부 레퍼런스와 동일한 디자인을 목표로 창 배경/목재/여러 가닥 로프를 재제작하도록 요청했다. 이전 단색 Stretch 후보는 최종 승인본이 아니다. 내장 ImageGen으로 프레임 6종, 로프, 파도 총 8종을 재제작했다. 기존 아이콘을 포함한 나머지 PNG 31종은 유지했다.
- 프레임은 64×64, Border 12px/PPU 100/Point로 갱신하고 Area1HUDSkin에서 Tiled로 표시해 픽셀 무늬와 나뭇결을 길게 늘리지 않는다. 중앙 음영은 낮은 대비의 두 톤으로 정리하고 반복 구간 양 끝 픽셀을 일치시켰다. .meta 6종은 크기/PPU/Border만, 파도 .meta는 최대 크기만 변경했다. 모든 GUID 보존.
- 로프는 여러 가닥의 명암/꼬임이 있는 48×48 감김 소재로 변경하고 간판·아이템 외곽·Hotbar 접합부에 연결했다. 파도는 256×48의 한 장짜리 포말 띠로 교체해 양 끝 큰 곡선과 중앙 낮은 거품을 유지한다. 성장 버튼은 목재/짙은 청록 내부/밝은 글자, 아이템 슬롯은 청록 경계, 배속 선택은 Cyan 경계로 정리했다.
- 변경 코드: Area1HUDSkin.cs의 표현만. 주요 UI Anchor/위치/크기, 텍스트 의미, 슬롯 수, PrototypeHUDCanvas/드래그/입력/툴팁/게임플레이/Scene/Prefab/ProjectSettings는 세션 시작 해시 대비 보존했다. 문서 3종과 Tools/pack_area1_reference_frames.ps1 및 Artifacts/MarineUIReference 산출물을 추가/갱신했다.
- 정적 PNG/알파/GUID/Importer 허용 필드/타일 이음새/변경 범위 확인 통과. 실제 Unity Import/컴파일/Play/한글·상호작용은 사용자 확인 대기. 기존 화면 크기와 TMP 폰트를 유지하므로 참조 이미지와 픽셀 단위 동일함을 확인한 결과는 아니다. Unity 실행/임시 프로젝트/Git add·commit·push를 하지 않았다. **Manual Visual Validation: Pending / Reference Match Approval: Pending.**

## Area 1 HUD Marine Refinement (2026-09-28, 현재 적용·사용자 검증 대기)

- 최신 사용자 요청에 따라 기존 39 PNG 후보를 재정리했다. 패널 기본 Anchor/위치/크기와 4개 아이템/5개 Hotbar 슬롯을 유지했다. 공통 27색 팔레트, 24 논리 픽셀 장식, 닫힌 라운드 모서리, 얇은 표시 테두리를 적용했다. PNG의 이름·32/48 규격·RGBA8와 39개 .meta/GUID는 보존했다.
- 좌측 NETBREAK를 위로 4px 돌출된 목재 간판으로 변경했다. 경험치 숫자 아래에 기존 RunManager.CurrentExp/ExpToNextLevel을 읽는 게이지를 추가했다. 경험치/레벨 계산은 수정하지 않았다. 준비 패널은 물고기 음영과 8개 연속 파도 타일, 확대된 로프를 사용한다. 아이템 번호는 고대비 배지로 분리하고 Hotbar에는 연결 기둥, 성장/배속에는 해양 장식을 적용했다.
- 이번 코드 변경은 Area1HUDSkin.cs의 표현과 PrototypeHUDCanvas.cs의 게이지 생성/표시 호출뿐이다. 세션 시작 해시 대비 Scene/Prefab/ProjectSettings/URP/게임플레이/입력/드래그/툴팁/아이템·성장 로직은 보존했다. 기존 미커밋 변경은 그대로 유지한다.
- 산출물: Artifacts/MarineUIRefine/의 BEFORE_39.zip, NETBREAK_UI_MARINE_REFINED_CANDIDATE.zip, PNG_BEFORE_AFTER.png, HUD_STATIC_PREVIEW.png, static-validation.json, FINAL_REPORT.md, UNITY_CHECKLIST.md. 재출력: Tools/refine_area1_hud_art.ps1. 이전 MarineUI 폴더는 이전 후보 이력이다.
- 정적 PNG/9-slice/.meta/변경 범위 확인만 수행한다. 실제 TMP 한글 잘림·오버플로·9-slice 표시·컴파일·상호작용은 사용자 Unity 검증 대기다. Unity 실행/Play/임시 프로젝트/컴파일/Console 확인 및 Git add/commit/push는 하지 않았다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## Area 1 HUD/UI Marine Rebuild — 39 PNG 후보 적용 (2026-09-28, 사용자 검증 대기)

- 시작 자료는 current_ui_39.zip이다. 프로젝트 PNG 39개와 전부 SHA-256이 일치했으며, 32×32 프레임 6개/아이콘 22개와 48×48 장식 11개로 조사했다. 39개 모두 정적 코드 참조가 있고 미참조 파일은 없다. 아이템 6종은 보유 시 표시 경로이며 정상 Area 1의 아이템 지급 규칙은 변경하지 않았다.
- 내장 ImageGen으로 39종을 역할별로 개별 제작했다. 생성 한도로 중단됐던 마지막 key도 재개 후 완료했다. Tropical Marine Sprite Sheet의 임의 순서 절단/매핑은 하지 않았다. 목재·로프 프레임, Cyan 선택 경계, Sand 버튼, 입체적 해양 장식과 도구/아이템/정보 실루엣을 갱신했다. 실제 UI 문자열은 PNG에 넣지 않았다.
- 기존 규격으로 최근접 샘플링하고 Alpha를 0/255로 정리했다. 프레임은 생성물의 고정 모서리·가장자리 단면을 재사용하면서 중앙을 불투명 단색, 늘어나는 가장자리를 일정한 단면으로 패킹했다. 짧은 제목 바/키 배지는 글자 영역을 확보하도록 테두리 소재를 더 얇게 패킹했다. 6종 Sprite Border 사방 5px, 나머지 0과 모든 .meta/GUID/Import 설정을 바이트 단위로 보존했다.
- Assets/Resources/UI/Area1/의 39 PNG를 실제 교체했다. 신규 Unity Asset 0개. 세션 시작 해시와 비교해 UI/Gameplay 코드, Anchor/Position/Size/Text RectTransform, DraggableHudPanel, Scene/Prefab, 입력/Inventory/Tooltip/Growth/Run/Fish/Boss/Tool/Item/Synergy는 변경하지 않았다. 기존 미커밋 변경은 보존했다.
- Static Validation: 39/39 PNG CRC·압축 데이터·RGBA8·투명도·원래 규격·이름·경로·변경 해시 및 39/39 메타데이터 보존, 6/6 프레임 Stretch 안전성 통과. 최종 ZIP의 39 PNG는 프로젝트 적용본과 모두 동일하다. 정적 합성 미리보기는 Unity 화면이나 TMP 렌더링/상호작용 검증이 아니다.
- 결과: Artifacts/MarineUI/NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip, ASSET_AUDIT.md, FINAL_REPORT.md, UNITY_CHECKLIST.md, PNG_BEFORE_AFTER.png, HUD_STATIC_PREVIEW.png. 개별 생성 프롬프트와 원본 경로는 generation-manifest.json에 있다. 이전 Tools/generate_area1_hud_art.js는 이번 ImageGen 후보를 재현하는 생성기가 아니며 이번 작업에서 수정/실행하지 않았다.
- 사용자 요청에 따라 Computer Use, Unity 실행/Import/compile/Console/PlayMode/Test Runner/Batch, 임시 프로젝트 복사, Git add/commit/push는 하지 않았다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** 목표 Reference와의 충분한 유사성 및 실제 가독성/기능은 사용자 Unity 검증 전이다.

## Area 1 HUD/UI Marine Decoration Iteration 4 (2026-09-27, 전체 화면 재검증 대기)

- **입력 자료:** 현재 Game View는 배치·크기 기준, 목표 화면은 시각 기준, Tropical Marine Sprite Sheet는 해양 픽셀아트의 형태·색·밀도 참고로만 사용했다. Sprite Sheet를 7×4로 분할하거나 gameplay 의미가 다른 파일에 순서대로 연결하지 않았다. `icon.zip` 28개는 작업 전 `Assets/Resources/UI/Area1/`의 28개 PNG와 SHA-256까지 같았고, 작업 후 28개 모두 새 해시로 교체됐다.
- **표현:** 기존 32×32의 `panel`, `slot`, `selected_slot`, `header`, `button`, `key`와 도구·스킬·아이템·정보·배속 아이콘 22개를 의미별 파일명/기존 `.meta` GUID를 유지해 다시 그렸다. Deep Navy/Teal 바탕, Cyan 선택 강조, Sand 버튼, 얇은 Wood/Rope 경계와 아이콘 외곽선을 사용한다. 새 독립 48×48 Point Sprite 장식 11종(`palm`, `gull`, `starfish`, `shell`, `coral`, `leaf`, `rope_knot`, `bobber`, `wave`, `crate`, `clock`)을 9-slice 이미지의 늘어나는 부분과 분리했다. 프레임 외부 일부 돌출, `raycastTarget=false`, 버튼/Slot Icon 우선을 유지한다.
- **배치:** 기존 HUD의 패널 크기·Anchor·슬롯 위치와 1920×1080 기준 배치를 유지한다. 좌측 정보 HUD와 조업 준비에 장식을 가장 많이, 아이템 HUD·핫바·성장 버튼에는 중간, 시간·배속에는 적게 적용했다. 좌측 제목의 글자 시작을 야자수와 분리하고 시작/성장 버튼 글자를 Sand 위의 어두운 색으로 조정했다. 드래그 손잡이·아이템 갱신·Q/W 동적 표시·버튼 이벤트는 기존 경로를 유지한다. Scene/Prefab YAML과 gameplay 판정은 변경하지 않았다.
- **검증:** 생성기 실행, PNG 39종과 `.meta` 존재, 기존 28개 전부 교체, Unity 6000.3.11f1의 Sprite Import·C# 컴파일을 확인했다. 컴파일 오류는 없고 기존 TMP `enableWordWrapping` 폐기 경고가 남는다. 새로고침 후 Play 화면에서 좌측 HUD의 야자수·갈매기, 준비 패널의 찌/파도, 아이템의 상자, 핫바·성장 버튼의 장식과 주요 정보/슬롯 글자 표시를 확인했다. Play 중 스크립트 Domain Reload 직후 한때 `RunManager.Update` 172행의 `NullReferenceException` 반복이 발생했으며, Play 종료·에셋 새로고침 후 다시 연 화면에서는 새 오류가 보이지 않았다. 이는 정식 Gameplay 회귀 테스트를 대체하지 않는다. 최종 배치의 Game View 확대 더블클릭은 자동 승인 검토가 게임 입력 부작용 위험을 이유로 거부해 수행하지 않았다. **Manual Visual Validation: Partial / Prototype Approval: Pending / Final Production UI Approval: Pending.**
- **Git:** 이번 Iteration은 이전 미커밋 HUD 작업을 이어받아 수정한 것이므로 기존 사용자 작업을 분리 보존했다. 전체 변경을 검증·승인받기 전에는 add/commit/push하지 않는다.

## Area 1 HUD/UI Visual Iteration 3 (2026-09-27, 사용자 Unity 검증 대기)

- **Iteration 2 사용자 확인 결과:** 좌우 HUD가 모서리에서 과하게 안쪽에 있었고, 글자가 프레임과 겹치거나 너무 작았다. 밝은 목재 조각의 반복도 선택한 두 번째 Deep Teal/Navy 레퍼런스와 달랐다. Iteration 2는 승인되지 않았다.
- **기본 배치:** 1920×1080 Canvas 기준 좌측 정보 패널은 Top Left/Pivot Top Left `(20,-20)`, 256×254, 우측 네 슬롯은 Top Right/Pivot Top Right `(-20,-20)`, 388×132다. 시간·준비 패널은 Top Center에서 수직 정렬하고, LMB/Q/W/E/R 핫바는 Bottom Center에서 아래 14px, 716×120이다. 배속은 오른쪽 아래 130px부터, 성장 버튼은 오른쪽/아래 20px 여백에 맞춘다. 초기 모서리 배치에서는 Route의 가장자리 진입·이탈 구간과 겹칠 가능성이 남으므로 실제 Fish/Boss 가시성은 사용자가 확인한다.
- **정보·글자:** 일곱 정보의 기존 직렬화 TMP 값 참조를 유지하면서 런타임에 Header/마스킹된 Content/아이콘/라벨/값 열로 분리했다. 25px 행과 구분선, 14~16px 값, 줄바꿈 해제·잘림 제한·Content RectMask2D로 프레임 침범을 막는다. 슬롯은 84×82, 상태 텍스트 13~15px와 38px 아이콘, 핫바는 136×106 슬롯과 38px 아이콘 및 15~16px 상태로 키와 정보를 구분한다. 상단 시간·준비와 우하단 UI도 읽기 쉬운 크기로 조정했다.
- **아트·구조:** 32×32 독립 Point Sprite 9-slice `panel`, `slot`, `selected_slot`, `header`, `button`, `key`를 사용한다. 패널 본체는 Deep Navy/Teal, 경계는 얇은 Cyan, 목재/로프는 바깥 프레임의 작은 강조로 제한한다. 기존 도구/아이템 아이콘에 정보 7종과 배속 파도 아이콘을 더했다. 기존 Sprite GUID와 TMP 폰트 자산을 유지했다. 고정 크기 전체 패널 이미지는 사용하지 않는다.
- **유지·범위:** 두 HUD의 제목 바 전용 Drag, Canvas Clamp, Tool 입력 차단, 기존 Inventory/Run 정보와 버튼 이벤트를 유지한다. Main Scene 직렬화상 `ItemHUD` 컴포넌트가 슬롯 패널의 상위 `ItemSystemUI`에 있어 아이콘 갱신 연결과 Tooltip 위치 기준을 실제 슬롯 패널에 맞게 바로잡았다. Gameplay, Fish/Boss/Route, Tool/Item/Synergy/Growth 판정과 Scene/Prefab YAML을 변경하지 않았다. `Tools/generate_area1_hud_art.js`로 UI PNG와 `.meta`를 재생성했고 코드·자산·문서만 수정했다.
- **검증·승인:** 정적 자산 참조·GUID/Import 설정·코드 diff·`git diff --check`를 확인한다. 사용자 요청에 따라 Computer Use, Unity Editor/Batch/PlayMode/EditMode/Test Runner/Console, 임시 프로젝트 복사, Git add/commit/push는 수행하지 않는다. 실제 Import/컴파일/시각 가독성/겹침/상호작용은 사용자 직접 확인 대기다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## Area 1 HUD/UI Visual Iteration 2 (2026-09-27, 사용자 Unity 검증 대기)

- **사용자 관찰·방향:** 이전 HUD가 Game View에서 크고 좌상단 Fish 진입을 가렸으며 밝은 목재 프레임이 시안보다 강했다. 프레임을 Deep Navy/Teal 내부, 얇은 Cyan 경계, 작은 목재·로프 모서리 강조로 다시 제작했다. 기존 32×32 Point/무 Mipmap/무압축 Sprite 경로와 GUID를 유지하고 9-slice 경계를 8px→5px로 줄였다. 아이템 6종의 작은 개별 아이콘을 추가했다. TMP 폰트 자산은 변경하지 않았다.
- **실제 Route와 기본 배치:** Main Scene의 `FishSpawner.activeRoute=CoastRoute_01`, 일반 경로 `(-10,8)→(-8,2)→(8,-2)→(10,-8)`, `spawnHalfWidth=1.2`, 카메라 Orthographic Size 6.5/16:9를 정적 확인했다. 어종별 Spawn 확산과 전방 흔들림까지 고려한 첫 화면 진입은 상단 왼쪽 대략 x=20~320px 범위다. 좌측 패널은 기존 300×314→225×232(약 25~26% 축소), 기본 위치를 Canvas 좌상단 `(355,16)`으로 옮겨 초기 진입 띠를 비웠다. 우측 슬롯은 318×100→255×79(약 20~21% 축소), 오른쪽에서 283px 떨어져 보스 2차 우상단 이탈(화면 x≈1750px)과 분리했다. 중앙 핫바는 708×100→602×84(약 15~16% 축소)로 하단 중앙 고정이다. 시간/조업 준비도 소폭 줄였다. 이는 정적 좌표 검토이며 실제 Game View 시각 검증은 아니다.
- **UI 표시·드래그:** 좌측 일곱 정보와 기존 갱신 경로는 유지한다. 두 패널 각각의 21px 제목 바에만 `DraggableHudPanel`을 붙여 독립 이동·Canvas 모서리 8px Clamp를 구현했다. UI 이벤트는 New Input System EventSystem을 사용하고 Pause 중에도 시간값 없이 동작한다. 비활성화/Run 재시작 시 포인터 캡처를 해제하며 위치 영구 저장은 없다. 손잡이의 LMB는 뜰채·그물·낚싯대 설치·장비 재배치·전술/시그니처 대상 지정과 겹치지 않도록 해당 입력 진입점에서만 차단한다. Tool 판정·쿨다운·Resistance·Fish/Route·Inventory/Tooltip 로직은 바꾸지 않았다.
- **아이템·핫바:** 아이템 네 슬롯의 번호·아이콘·`비어 있음`/짧은 이름·Lv 표기를 작은 영역에 나누고 기존 Inventory/Tooltip 연결과 Tooltip의 속성·효과 정보를 유지했다. 슬롯 Tooltip은 패널 이동 위치를 따라 Canvas 안에서 아래/위로 배치하고, 속성 Tooltip은 이동한 기본 패널과 겹치지 않게 내렸다. 획득 아이템은 6종 전용 아이콘으로 표시한다. LMB/Q/W/E/R 키·도구/스킬 아이콘·상태·잠금·쿨다운과 동적 Q/W는 기존 런타임 상태를 그대로 읽는다. x1/x2/x3, 성장 관리 [Tab]은 기능과 크기를 유지하면서 새 프레임 색만 공유한다.
- **수정·생성:** `Assets/Scripts/UI/Area1HUDSkin.cs`, `PrototypeHUDCanvas.cs`, `ItemHUD.cs`, 신규 `DraggableHudPanel.cs`와 `.meta`; 손잡이 입력 차단에 한한 `ToolSlotInput.cs`, `NetPlacementController.cs`, `FishingRodPlacementController.cs`, `GearRepositionController.cs`, `TacticalSkillManager.cs`, `SignatureSkillManager.cs`; `Tools/generate_area1_hud_art.js`, `Assets/Resources/UI/Area1/`의 기존 프레임/아이콘과 신규 아이템 아이콘 6쌍, 본 문서·아트 가이드·에셋 목록. Scene/Prefab YAML은 수정하지 않았다.
- **검증·승인:** Computer Use, Unity Editor/Batch/Play/EditMode/Console, 임시 프로젝트 복사, Git add/commit/push는 사용자 요청에 따라 수행하지 않았다. Import/컴파일, 실제 Game View의 Fish·Boss 가시성·텍스트 잘림·드래그/Clamp·입력 충돌·Tooltip·Pause/재시작·Console은 사용자 직접 확인 대기다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## 이전 Area 1 HUD/UI 시안 반영 (Iteration 1, 사용자 검증에서 크기·배치 문제 확인)

- **범위:** 첨부된 열대 바다 픽셀 UI 시안의 청록 패널·나무/로프 테두리·밝은 모래색 버튼과 슬롯 구성을 현재 GameCanvas에 맞게 반영했다. 1920×1080 CanvasScaler와 16:9 화면을 기준으로 좌상단 300×314, 상단 시간 304×52/준비 316×105, 우상단 아이템 318×100, 하단 핫바 708×100, 우하단 성장 211×57/배속 106×34로 정리했다. Scene/Prefab YAML과 기존 Inspector 참조는 변경하지 않고 `PrototypeHUDCanvas.Awake`에서 기존 UI의 외형·배치를 적용한다.
- **표시·동작:** 좌상단 골드·포획 수·어획률·레벨·경험치·조업 단계·현재 구간, 상단 시간·조업 시작, 우상단 네 아이템 슬롯, LMB/Q/W/E/R 상태·쿨다운, 성장 관리 [Tab], x1/x2/x3의 기존 텍스트 의미와 버튼 이벤트를 유지했다. Q/W 아이콘은 현재 Run 도구에 따라 바뀐다. 빈 슬롯과 E/R 잠금도 기존 런타임 상태를 표시한다. 시각 선택 표시만 배속 버튼에 추가했다. Fish/Tool/Boss/Item/Synergy/VFX/SFX 및 게임플레이 로직은 변경하지 않았다.
- **아트·폰트:** `Assets/Resources/UI/Area1/`에 Point 필터·Mipmap Off·무압축 32×32 RGBA 픽셀 프레임 4종과 도구/스킬 아이콘 8종을 추가했다. 프레임은 8px 9-slice이며 생성기는 `Tools/generate_area1_hud_art.js`다. 현재 TMP 폰트 자산은 변경하지 않았다. 갈무리 9는 기존 아트 가이드의 우선 후보 상태를 유지하며 이번 UI 배치의 검증 완료를 뜻하지 않는다.
- **변경 파일:** `Assets/Scripts/UI/PrototypeHUDCanvas.cs`, 신규 `Area1HUDSkin.cs`와 `.meta`, `Assets/Resources/UI.meta`, 신규 `Assets/Resources/UI/Area1.meta` 및 12쌍의 `.png`/`.png.meta`, `Tools/generate_area1_hud_art.js`, 본 문서와 `Docs/NETBREAK_ART_GUIDE.md`·`Docs/NETBREAK_ASSET_MANIFEST.md`.
- **검증 상태:** 사용자 요청에 따라 Computer Use, Unity 실행/컴파일/Console, PlayMode, EditMode, 수동 화면 확인, 임시 복사본 프로젝트, Git add/commit/push를 수행하지 않았다. 따라서 실제 Import, 16:9 Game View 배치·텍스트 잘림·시안 대비 시야 점유, 빈/획득 슬롯 Hover, Q/W 도구 교체·E/R 해금·쿨다운, 준비→시작, Tab/배속 버튼, Boss/공지/결과/성장 창과의 겹침, Pause/Run 재시작 및 Console은 **사용자 직접 확인 대기**다. 정적 변경 검토와 `git diff --check` 결과는 이번 작업 보고에 별도 기록한다.

## Area 1 고품질 정적 배경 Prototype 승인 (2026-09-27, 사용자 Play Mode 검증 완료)

- **적용:** 사용자 첨부 1672×941 RGB 탑다운 바다 이미지를 실제 적용 기준으로 삼고 내장 ImageGen 정밀 편집으로 경로와 겹치는 큰 암초·바위·해초·산호를 정리했다. 원본과 같은 해상도·16:9 구도·세밀한 수면광·픽셀아트 질감을 유지했다. 최종 후보를 기존 `Assets/Resources/Area1/CoastBackground.png` 경로에 덮어썼으며 `.meta` GUID는 유지했다. 중앙은 열린 바다, 장식은 주로 상·하단의 경로 밖에 둔다. 모래는 물 아래 작은 해저 포켓이다.
- **실제 경로:** `Main.unity`의 `FishSpawner.activeRoute`는 `CoastRoute_01`이다. 일반 물고기 및 Boss 1차 경로는 `(-10,8)→(-8,2)→(8,-2)→(10,-8)`, Boss 2차는 `(-10,-8)→(-8,-2)→(8,2)→(10,8)`, Boss 3차는 `(-13,0)→(-8,5)→(8,-5)→(13,0)`이다. 각 Route의 `spawnHalfWidth=1.2`, `travelHalfWidth=0.7`, `spawnForwardJitter=0.3`이다. 카메라 위치 `(0,0,-10)`·Orthographic Size 6.5이므로 16:9 표시 범위는 대략 x±11.56/y±6.5이며, 출발/도착의 일부는 화면 밖이다. 사용자가 체감한 우상단→좌하단 방향도 함께 열린 바다로 보호했다. Gameplay 경로 값과 Scene/Prefab은 바꾸지 않았다.
- **시각 충돌 조정:** 좌상단 일반 어군·Boss 1차 진입부, 좌하단 Boss 2차 진입부/요청된 일반 이탈부, 우상단 Boss 2차 이탈부/요청된 일반 진입부, 우하단 일반 어군·Boss 1차 이탈부에 있던 큰 군집을 제거하거나 경로 밖으로 줄였다. 좌측 중간 산호와 우측 상부 군집, Boss 3차 지그재그 주변에 남았던 작은 군집도 정리했다. 화면 상단 중앙·하단 중앙의 경로 밖 장식과 섬세한 수면 표현은 유지했다.
- **정적 Prototype:** 기존 `Area1BackgroundController`의 `Resources.Load<Sprite>`, Main Camera 자식 단일 SpriteRenderer, Sorting Order -1000, 중복 방지, 카메라 맞춤을 유지했다. 이전 미검증 `Area1SurfaceFlow` 수면 오버레이의 생성·애니메이션 코드를 제거해 정적 이미지 자체만 평가한다. Collider·입력·판정은 없다. 단일 PNG는 현재 검증 방식이지 최종 Production 구조 확정이 아니다.
- **Import·이전 정적 점검:** 기존 `.meta`의 Sprite Single, Point, Mipmap Off, PPU 32, 기본 무압축, Max Size 2048을 유지한다. 1672×941은 Max Size 이하로 Import 축소 대상이 아니다. `Area1BackgroundTests`의 크기·정적 자식 기대값을 새 후보에 맞췄다. 이전 작업에서 Asset 존재·PNG 크기/불투명도·참조 경로·Scene 직렬화·코드 구조와 Git diff만 정적으로 확인했다. 이번 승인 기록 작업에서는 Unity 검증을 다시 수행하지 않았다.
- **사용자 수동 검증·승인:** 사용자가 실제 Unity Play Mode에서 이전 배경보다 큰 품질 향상, 적절한 밝기, Fish/Tool/VFX의 치명적 가독성 문제 없음, 일반 Fish 경로와 Spawn/Exit의 자연스러움, Boss 회유와 암초·바위·모래의 심각한 시각 충돌 없음을 확인했다. 현재 Area 1 Vertical Slice에서 사용 가능한 Prototype으로 승인했다. **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending.** 이는 최종 배경 아트·단일 PNG 구조·수면 표현·색상/명도/디테일·Area 2~6 구조의 확정이 아니다.
- **후반 Polish 검토:** 약간의 시각적 이질감과 정적인 느낌은 남아 있으나 원인은 미확정이다. 정적 배경의 움직임 부족, Fish/Tool과 배경의 세부 Art Density, 수면 패턴·반복감, UI 및 다른 아트가 완성된 뒤의 통합 화면 조화를 검토 후보로만 기록한다. 이번 단계에서 추가 수정하지 않는다.

## Area 1 배경 톤·수면 흐름 개선 (2026-09-27, Unity 수동 검증 대기)

- **목표·아트:** 선택한 탑다운 바다의 구도와 열린 동선을 유지하면서 눈부신 Cyan과 강한 다각형 수면 격자를 낮췄다. `CoastBackground.png`를 ImageGen으로 편집해 차분한 터콰이즈 중간톤, 끊어진 낮은 대비 물결, 상·하단 가장자리의 희미한 해저 모래로 정리했다. 이전 PNG와 같은 1671×941 불투명 RGB이며 평균 휘도(Rec.709 RGB 가중 평균)는 170.33→149.59, 약 12.2% 감소했다. 큰 수중 군집은 네 곳에서 우상단·좌하단 두 곳으로 줄이고, 상단·하단에는 작은 바위만 드물게 남겨 화면의 장식 밀도를 대략 절반 수준으로 낮췄다. 중앙에는 넓은 모래·큰 암초가 없다.
- **동선:** 요청한 우상단→좌하단 일반 어군과 Boss 2차 역방향 대각선, Boss 3차 좌→우 가로 통로는 열린 바다로 유지했다. Scene 직렬화의 실제 활성 일반 경로는 좌상단→우하단, 일반 어군 생성은 카메라 왼쪽이므로 그 대각선도 함께 비웠다. 우상단 장식은 화면 위쪽 진입부보다 아래·바깥쪽에 두고, 큰 물체를 경로 한복판에 새로 놓지 않았다. Fish Spawn·Path·Boss·Tool·VFX·UI·Balance는 수정하지 않았다.
- **약한 수면 흐름:** 기존 `Area1BackgroundController`가 Main Camera 자식 `Area1SurfaceFlow`를 중복 없이 하나만 만든다. 1024×576 Point/무 Mipmap의 투명한 짧은 반사선 텍스처를 결정론적으로 한 번 생성하고 배경 정렬 순서 -1000 바로 위의 -999에 둔다. 텍스처 선 alpha 45~75/255와 Renderer alpha 0.11~0.15를 곱해 국소 효과를 약하게 제한한다. 반사선은 최대 x 0.09·y 0.06 world unit 범위에서 천천히 왕복하며 카메라 변화 시 화면보다 약간 넓게 맞춘다. `Time.deltaTime`의 scaled time으로 움직여 Pause에 멈추고, 재초기화 시 위상·위치를 리셋하며 파괴 시 임시 Sprite/Texture를 정리한다. Collider·입력·게임플레이 판정은 없다.
- **파일·연결:** `Assets/Resources/Area1/CoastBackground.png`(기존 GUID, SHA-256 `4051afc1b3403ff33d10037d97995ac7e40578fb6c0deadccf23e328b84a73b5`), `Assets/Scripts/Core/Area1BackgroundController.cs`, `Assets/Editor/Tests/Area1BackgroundTests.cs`, 이 상태 문서와 `Docs/NETBREAK_ART_GUIDE.md`, `Docs/NETBREAK_ASSET_MANIFEST.md`를 수정했다. 기존 Resources/Main Camera child 연결·PNG Sprite Single/Point/무압축/Mipmap Off/PPU 32를 유지했다. Scene/Prefab YAML, ProjectSettings/URP는 수정하지 않았다.
- **자동 검증:** Unity 6000.3.11f1 임시 복사본에서 최종 PNG가 기존 GUID로 Import되고 Runtime·Editor/Test 컴파일 성공, C# 컴파일 오류 0, 배경 EditMode **3/3**, 전체 EditMode **244/244 통과**(실패·Skip 0). 중복 생성, 뒤쪽 정렬, Point/무 Mipmap, 낮은 alpha, 카메라 맞춤, scaled delta 0일 때 정지, 재초기화 위치 리셋을 검사했다. 실제 Unity Play Mode의 화면·파도 체감·원본 Console은 아직 사용자 수동 검증 전이다.
- **승인·Git:** Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending. Computer Use와 Git add/commit/push는 하지 않는다.

## Area 1 선택 배경 시안 적용 (개선 전 기록, 2026-09-27)

- **적용:** 사용자가 고른 첫 번째 탑다운 청록 바다 이미지를 현재 `Assets/Resources/Area1/CoastBackground.png`에 적용했다. 내장 ImageGen 편집으로 배경 물고기형 그림자와 왼쪽 아래 작은 물고기처럼 보이는 형태를 제거했다. 기존 PNG 경로·`.meta` GUID, `Area1BackgroundController`의 Resources 로드·Main Camera 자식 연결·정렬 순서 -1000·중복 방지·카메라 맞춤을 유지했다. 새 배경 시스템, Collider, Scene/Prefab YAML, ProjectSettings/URP, Camera, Gameplay·Balance·UI는 변경하지 않았다.
- **경로와 지형:** 선택 시안의 중앙과 우상단 Spawn Zone·좌하단 Exit Zone·우상단↔좌하단 대각선·좌측→우측 Boss 3차 회유 구간을 열린 바다로 유지했다. Boss 2차 좌하단→우상단도 큰 지형으로 막지 않는다. 모래는 상단·하단 일부의 물 아래 해저이며 연속 해변 띠가 아니다. 암초·바위·해초는 드문 작은 군집으로 배치했다. 실제 `Main.unity`의 활성 `CoastRoute_01`은 요청서의 방향과 달리 좌상단 `(-10, 8)`→우하단 `(10, -8)`이고 일반 어군도 카메라 왼쪽에서 스폰한다. 따라서 반대 대각선 역시 열린 바다로 보호했다. 경로 설정은 바꾸지 않았다.
- **파일·형식:** 선택 PNG는 1671×941 불투명 RGB, SHA-256 `f419fccf221485032c8e5932e451749e2eba2ae0c4f43a6ef7714561a6b82afe`다. 기존 Import Sprite Single/Point/무압축/Mipmap 없음/PPU 32, max texture size 2048을 유지한다. `Assets/Editor/Tests/Area1BackgroundTests.cs`는 현재 크기·불투명도·하단 대부분 물색과 기존 연결·정렬·중복 방지·카메라 맞춤을 검사한다. 옛 `Tools/generate_area1_background.py`는 선택 배경을 덮어쓰지 않도록 별도 ProceduralDraft 출력으로 돌렸다. `Docs/NETBREAK_ART_GUIDE.md`, `Docs/NETBREAK_ASSET_MANIFEST.md`도 현재 시안으로 갱신했다. 단일 PNG는 Prototype 검증 방식이고 최종 Production Background 구조는 미확정이다.
- **자동 검증:** 최종 PNG는 1671×941 RGB/불투명, 화면 하단 16px 간격 표본의 92/105가 물색이다. Unity 6000.3.11f1 원본 배치 시도와 첫 임시 복사본 시도는 Package Manager IPC 오류로 테스트 시작 전에 종료됐다. 격리 임시 복사본을 정상 권한으로 실행한 뒤 Runtime·Editor/Test 컴파일 성공, 최종 PNG 기존 GUID Import, 배경 EditMode **3/3**, 전체 EditMode **244/244 통과**(실패·Skip 0)를 확인했다. 배치 로그에 C# 컴파일 오류가 없다. 원본 프로젝트의 Play Mode·Console은 사용자 수동 검증 전이다. `git diff --check`는 통과했다.
- **승인 상태:** Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending. 사용자가 Main Scene Play에서 Spawn/Exit·Boss 회유·물고기/도구/VFX/UI 가독성·Pause·Run 재시작과 Console을 직접 확인한다. Computer Use 및 Git add/commit/push는 하지 않는다.

## 이전 Area 1 탑다운 수중 배경 시안 (2026-09-27, 선택 시안 적용 전 기록)

- **문제·목표:** 직전 미커밋·미검증 배경의 화면 하단 모래띠는 측면 해변처럼 보였다. 같은 `CoastBackground.png`와 GUID를 안전하게 교체해 맑은 얕은 바다를 위에서 내려다보는 픽셀아트 화면으로 수정했다. 이전 배경으로 사용자는 Unity 검증을 하지 않았다.
- **조사:** Main Scene은 배경 오브젝트 없이 Main Camera의 단색 Clear Color를 사용한다. 카메라 Orthographic Size 6.5, 위치 `(0,0,-10)`, 전체 Viewport; `GameCanvas`는 Screen Space Overlay/1920×1080 기준이다. `HUD_Left` 좌상단, `HUD_Right`·`ItemHUD` 우상단, 동적 Hotbar 하단 중앙, MiniBoss/Boss Panel 상단 중앙이다. Fish·낚싯대·그물 Sprite는 정렬 순서 0, 일부 Range는 -1이며 Fish 5종은 어두운 윤곽을 쓴다. 현재 배경은 Main Scene 로드 때 `Area1BackgroundController`가 카메라 자식 SpriteRenderer로 연결한다. 경로·어군이 중앙과 상하로 지날 수 있어 중앙과 UI 뒤는 낮은 밀도로 유지했다.
- **아트·구현:** 256×144 픽셀 격자에서 제작한 뒤 Point 방식으로 512×288 RGBA로 확대한다. 세 가지 비슷한 청록 물색의 불규칙한 덩어리를 기본 물층으로 쓰고, 물 아래 모래 얼룩·암반 그림자·바위·방사형 해초·작은 산호를 낮은 알파로 합성해 단일 불투명 PNG로 저장한다. 바닥까지 물색이 이어지며 직접 노출된 해변·수평선·측면 모래층은 없다. 디테일은 좌우 가장자리에 모으고 중앙은 단순하다. Point/무압축/Mipmap 없음/PPU 32와 기존 카메라 자식 Renderer, 정렬 순서 -1000을 유지한다. Collider·입력·GamePlay 컴포넌트는 없다. 기존 연결이 반복 실행에 안전하여 Setup 메뉴·Scene/Prefab YAML 변경은 불필요하다.
- **파일:** 교체 `Assets/Resources/Area1/CoastBackground.png`(동일 경로·GUID). 수정 `Tools/generate_area1_background.py`, `Assets/Editor/Tests/Area1BackgroundTests.cs`, `NETBREAK_STATE.md`, `Docs/NETBREAK_ART_GUIDE.md`, `Docs/NETBREAK_ASSET_MANIFEST.md`. 이전 작업의 신규 메타·컨트롤러·테스트 파일은 미커밋 상태 그대로 보존한다. Scene/Prefab, ProjectSettings/URP, Camera 설정, 게임플레이·밸런스·UI 코드는 수정하지 않았다.
- **자동 검증:** 새 PNG는 512×288 RGBA/alpha 전부 255, 색상 23개, 하단 전폭 청록 물색이며 중앙은 3색만 사용한다. 생성기 재실행 SHA-256 일치. Unity 6000.3.11f1 배치 Runtime·Editor/Test 컴파일 성공, 배경 EditMode **3/3**, 전체 EditMode **244/244 통과**(실패·Skip 0). Import의 Single Sprite/Point/무압축/Mipmap 없음/PPU 32, Resources 참조, 정렬 순서·중복 방지·카메라 크기 변경, 하단 물색을 검사한다. `git diff --check` 종료 코드 0이며 새 텍스트 파일 공백을 별도 검사했다. 배치 로그에 C# 컴파일 오류가 없다. 실제 Play Mode 시각 승인은 별도다. 직전 배경의 전체 243/243은 변경 전 기록이다.
- **사용자 Unity 검증:** Main Scene 열기 → Import/컴파일·Console 확인 → 전체 EditMode 실행 → Play → 하단 해변·수평선이 없는 탑다운 바다, 희미한 해저 요소와 단순한 중앙을 확인한다. 물고기 5종·도구 4종·Range/Targeting Preview·오징어 먹물/복어/Item/Synergy VFX·MiniBoss/Boss UI·Pause/선택 UI를 확인하고 화면 비율 변경, Run 재시작·종료 후 중복 배경과 Console Error를 확인한다. 상세 순서는 본 작업 최종 보고를 따른다. 별도 Setup/Validate 메뉴는 없다.
- **Pending·후속:** Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending. 실제 Play Mode의 가독성·비율별 픽셀 모양·성능은 사용자가 확인한다. Fish 그림자와 수중 입자는 배경 가독성 평가 뒤 후속 후보이며 이번에는 구현하지 않았다. Area 2 이상의 연결과 최종 Palette·PPU·Pixel Perfect Camera 정책은 별도 작업이다. 사용자 지시에 따라 Computer Use와 Git add/commit/push는 하지 않는다.

## VS-2D-4 — Cast Net Presentation Prototype (2026-09-27, Unity 수동 검증 완료)

- 실제 투망은 Run에서 Lv2 Core/Q 또는 Lv3 Partner/W로 획득하는 동적 슬롯 도구다. `ToolSlotInput.Read(ToolId.CastNet)`의 키 누름으로 조준, 키 뗌으로 커서 월드 좌표에 즉시 사용한다. 우클릭·Esc·슬롯 변경·선택 UI에서 기존 취소가 작동한다. Scene 직렬화 값은 피해 15, 원형 반경 1, 1충전·7초 재충전, 8마리 포획 시 재충전 2초 환급이다. 최대 명중 수 제한과 배치 유효성 제한은 없다. E의 긴급 투망은 별도 전술 스킬로 같은 Controller의 조준/확정 경로를 사용하며, R 천상 투망은 별도 Signature 흐름이다.
- 기존 `CastNetVisual` 원형 SpriteRenderer를 실제 `captureRadius`(E 배율 적용 시 확대 반경)와 같은 커서 위치에 놓는다. 조준 중 작은 접힌 투망 Ghost를 함께 표시한다. 사용 확정 시 기존 `OverlapCircleAll`→중복 Fish 제거→`TakeCaptureDamage` 후 실제 Resistance가 감소한 Fish의 당시 위치만 모아 Presentation에 넘긴다. 별도 Fish 재검색은 없다. 원형 망 5프레임/0.42초, 전개 60% 지점에 시작하는 물결 Area 4프레임/0.28초, 실제 명중별 작은 접촉 4프레임/0.18초, 사용음 1회다. 캐릭터나 Tool 발사 원점이 없어서 비행 궤적 없이 목표 위치에서 펼친다.
- 프로젝트 내부 생성 `CastNet_Folded.png`, `CastNet_Open.png`, `CastNet_Area.png`, `CastNet_Hit.png`, `CastNet_Open.wav`를 추가했다. PPU 64는 현재 카메라 orthographic size 6.5와 반경 1의 Prototype 선택이며 Tool 공통 최종 규격이 아니다. 지속 설치 Net과 구분되는 원형·방사형 일회성 연출이다. 공용 `CombatVfxPool` 상한과 RepeatedHit 우선순위, 공용 one-shot AudioSource/0.09초 중복 제한을 재사용한다. SFX는 명중 수와 관계없이 1 cast에 최대 1회다.
- `NETBREAK/Art/Setup Cast Net Presentation`이 Sprite Import와 Resources Profile을 생성·연결하고 `Validate Cast Net Presentation`이 자산·반경 hook을 점검한다. Scene/Prefab YAML, ProjectSettings/URP, 피해·반경·충전·환급·도구 획득·아이템/시너지 판정은 수정하지 않았다. Pause 및 UI 차단 중 조준·연출 상태를 정리하고 공용 Run 초기화가 VFX/Audio를 정리한다. 전술 E의 기존 입력 게이트에 `timeScale > 0` 확인을 더해 Pause 중 확정을 차단한다. 정상 Q/W 및 E 경로의 기존 취소는 구현되어 있었으므로 새 키는 추가하지 않았다.
- **Warning 정리:** VS-2D-4에서 기존 `ShowCastEffect` 코루틴의 사용 기간이 `CastNetPresentationProfile.openingDuration`(현재 Resources Profile 0.42초)으로 이동했다. `CastNetController.visualDuration`은 더 이상 읽히지 않아 CS0414를 냈으므로 해당 obsolete 직렬화 필드 선언만 제거했다. Scene YAML에 남은 과거 직렬화 값은 직접 수정하지 않았다. 실제 Animation/VFX 시간과 피해·반경·충전·재충전·대상 수·Q/W 입력·취소·Item/Synergy·E/R 판정은 변경하지 않았다.
- **자동 검증:** PNG 4개 크기/RGBA와 WAV mono·44.1 kHz·16-bit·0.34초 정적 규격을 확인했다. 이전 격리 배치 Setup 시도 2회는 Package Manager IPC 오류로 컴파일 전에 종료됐지만, 이후 격리 복사본 Unity 6000.3.11f1에서 Runtime 및 Editor/Test assembly 컴파일과 `Validate Cast Net Presentation` 실행이 성공했다. 해당 배치 로그에 CS0414 및 C# 컴파일 오류가 없다. 신규 Cast Net EditMode 테스트 **10/10**, 전체 EditMode **241/241 통과**(실패 0·Skip 0). 전체에는 기존 Tool Presentation 테스트가 포함된다.
- **사용자 수동 검증:** 원본 Unity compile 정상·VS-2D-4 관련 Console Error 0. Play Mode에서 실제 Q/W 동적 슬롯의 hold 조준 Ghost·Area Preview, 커서 중심 동기화, Preview와 실제 captureRadius 일치, release 확정, 5프레임 전개, Area VFX, Miss의 Hit VFX 없음, 실제 Resistance 감소 Fish에만 Hit VFX, 다중 명중과 대상 수 보존, cast당 기본 SFX 1회, 범위 업그레이드 연동을 확인했다. 우클릭/Esc 취소에 피해·충전·SFX 소비가 없고 충전/재충전, UI 선택·Inventory·Pause 입력 차단, 슬롯 전환·Run 재시작 후 잔상 없음도 확인했다. 기존 E/R 별도 스킬, Item/Synergy, Fishing Rod/Net/Scoop Net Presentation도 정상이다. 원본 Editor Console에서 warning 제거 후 상태는 별도로 확인하지 않았으며 격리 배치 컴파일 로그에서 CS0414 소멸을 확인했다.
- **승인:** Cast Net Pixel Art·Targeting Ghost·Area Preview **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**. Cast/Open Animation **Manual Validation: Passed / Prototype Approval: Approved / Final Production Animation Approval: Pending**. Area VFX·실제 Hit VFX **Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending**. Cast SFX **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending**. Sprite/Sheet 크기·5프레임·0.42초·VFX lifetime·SFX volume·PPU는 최종 출시 확정값이 아니다. 이번 마감 작업은 Computer Use 및 Git add/commit/push를 수행하지 않았다.

## VS-2D-3 — Scoop Net Presentation Prototype (2026-09-27, Unity 수동 검증 완료)

- 실제 뜰채는 Q/W 슬롯이 아닌 LMB 고정 도구다. Scene 직렬화 값은 Resistance 피해 3, 원형 반경 1.1, 공격 쿨타임 0.45초, 기본 최대 3마리, 연쇄 반경 1.5·피해 2다. New Input System의 커서 월드 좌표를 중심으로 Collider를 거리순 정렬해 피해를 적용한다. 누르고 있는 동안 쿨타임마다 재사용하며 쿨타임 입력은 버퍼링하지 않는다. Miss가 가능하고 기존 HUD에 쿨타임 표시가 있다.
- 기존 `LandingNetRange` SpriteRenderer를 같은 커서 위치와 `captureRadius * 2` 크기로 사용한다. 사용 가능할 때 약한 원과 뜰채 Ready Sprite를 표시하고 쿨타임 중 원을 더 흐리게 한다. 범위 업그레이드는 기존 `UpdateRangeVisual`이 같은 값으로 즉시 반영한다. 기존 Scene 원본 알파는 약 0.039였다. Scene/Prefab YAML은 수정하지 않았다.
- 48×48 Ready 및 5프레임 Swing Pixel Sprite(PPU 64, 본체 0.75 world unit), 24×24×4프레임 Hit 물보라(PPU 83), 0.24초 mono WAV를 내부 생성했다. `LandingNetController`의 모든 실제 `TakeCaptureDamage` 후 Resistance가 감소한 Fish와 당시 위치를 기록하고 공격 루프 완료 뒤 Presentation에 넘긴다. 연쇄 피해도 실제 감소 시 기록한다. Swing은 Hit/Miss 공통 0.26초, Hit VFX는 실제 피해 위치마다 0.2초, 기본 SFX는 Tool use당 1회다. VFX는 공용 `CombatVfxPool` RepeatedHit, 사운드는 공용 one-shot Source와 0.09초 전역 중복 제한을 사용한다. 피해·대상 선택·기본 최대 타격 수·쿨타임·아이템/시너지 큐를 바꾸지 않았다.
- `NETBREAK/Art/Setup Scoop Net Presentation`이 Import와 Resources Profile을 연결하고 `Validate Scoop Net Presentation`이 참조·규격·런타임 hook을 점검한다. 런타임에 뜰채 자식 SpriteRenderer만 생성하며 Collider는 추가하지 않는다. UI 선택·Pause·배치 모드 중 커서 표시와 Swing은 지우고 입력을 차단한다. Run 초기화는 공용 VFX/Audio 정리를 사용한다.
- **자동 검증:** Unity 6000.3.11f1 배치 Setup/Validate 2회와 Runtime·Editor/Test 컴파일 성공. Import 메타데이터·Profile 해시는 반복 Setup 후 불변이다. 신규 EditMode 테스트 9개는 LMB/커서·원형 범위·쿨타임·다중 피해 위치·Hit/Miss·중복음·선택 UI/Pause·에셋 누락/풀 포화·초기화/풀 반환을 검사한다. 전체 EditMode **231/231 통과**(실패·Skip 0).
- **사용자 수동 검증:** 원본 Unity compile 정상, Console Error 0, 전체 EditMode **231/231 통과**. Play Mode에서 Ready Sprite·커서 추적·실제 gameplay 범위와 일치하는 Range Feedback 및 화면 가독성, Miss의 Swing/SFX와 Hit VFX 없음, Resistance가 실제 감소한 단일·다중 대상별 Hit VFX, 1 use = 1 기본 SFX, 쿨타임 중 가짜 연출 없음, Ready→Swing→Ready 복귀와 방향 독립 Swing의 자연스러움을 확인했다. 선택 UI·배치/재배치·Pause 입력 차단, Run 재시작 후 잔상 없음, 기존 Fishing Rod/Net/Squid/Puffer/Item/Synergy Presentation도 확인했다. 실제 gameplay hit Fish 목록과 당시 위치를 Presentation이 그대로 사용하며 별도 target search가 없음을 검증했다.
- **승인:** Scoop Net Sprite **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**. Scoop Net Swing **Manual Validation: Passed / Prototype Approval: Approved / Final Production Animation Approval: Pending**. Scoop Net Hit VFX **Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending**. Scoop Net SFX **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending**. Damage·range·cooldown·max targets·input binding 값은 변경하지 않았다. 48×48·5프레임·animation duration·VFX size·SFX volume은 최종 출시 확정값이 아니다. 이번 문서 갱신에서 구현 에셋은 수정하지 않고 Git add/commit/push도 수행하지 않는다.

## VS-2D-2 — Net Presentation Prototype (2026-09-26, Unity 수동 검증 완료)

- 실제 Net은 Q/W 동적 슬롯으로 모드 진입 후 LMB 시작점→끝점 드래그·릴리스로 설치하는 회전 직사각형 Trigger다. Scene 직렬화 값은 두께 0.3, 길이 0.5~8, 상한 3, 비용 8+길이당 6·설치 수 증가 0.5다. 프리팹은 감속 0.5, Resistance DPS 1.5다. OnTriggerStay2D마다 `Time.fixedDeltaTime`을 곱하며 별도 긴 Tick 타이머는 없다. 설치 후 Ctrl+드래그 이동이 이미 있다.
- 16×16 반복 Mesh와 16×4 Rope Pixel Tile을 월드 길이·두께에 맞춰 타일링한다. 실제 Collider와 Preview가 기존 `NetPlacementController`의 같은 시작점·끝점·두께 및 `NetController.Initialize` 변환을 쓴다. 모드 진입 시 시작점 표식, 드래그 중 전체 Ghost를 보이고 설치/취소/차단 시 지운다. 시각 자식에는 Collider가 없다. 작동 중 Mesh 알파만 미세하게 변하고 기존 비작동 어두운 틴트가 우선한다.
- 일반 물고기가 실제 `EnterNet`에 새로 등록된 뒤에만 24×24×4프레임 Net Contact VFX를 공용 `CombatVfxPool`의 RepeatedHit 우선순위로 요청한다. 지속 피해 Tick에는 VFX/SFX를 요청하지 않는다. 실제 설치와 Gold 지불 뒤에만 0.29초 Net Place WAV를 재사용 one-shot Source로 1회 요청한다. Profile/Pool/오디오 누락은 판정에 영향을 주지 않는다. 기존 복어 3초 중단·Collider 비활성·어두운 상태·기존 복어 연출은 유지한다.
- `Tools/generate_net_presentation.py`로 프로젝트 내부 PNG/WAV를 생성하고 Unity Editor 메뉴 `NETBREAK/Art/Setup Net Presentation`에서 Import·Resources Profile을 연결한다. 기존 Net 프리팹에는 런타임에서 작은 시각 컴포넌트를 붙인다. `Validate Net Presentation`은 참조·Import·WAV를 확인한다. Scene/Prefab YAML은 최종 diff에서 변경하지 않았다. 수치와 판정 규칙은 바꾸지 않았다.
- **검증:** Unity 6000.3.11f1 배치 Setup/Validate 반복 실행과 Runtime·Editor/Test 컴파일, 전체 EditMode **222/222 통과**(실패·Skip 0). 사용자가 원본 Unity에서 compile 정상, Console Error 0, 전체 EditMode **222/222 통과**와 Play Mode 수동 검증을 확인했다. Q/W 동적 배치, 시작점·드래그 Ghost와 실제 위치·길이·두께·각도 일치, 최소 길이·Gold·설치 상한, 확정·취소, 설치음 1회, 첫 Fish 접촉 VFX와 지속 Tick의 과다 반복 없음, 기존 Slow·Resistance 피해, 복어 중단 시 어두운 상태와 Active visual 정지·약 3초 뒤 복귀, 다중 Net 독립 상태, 도구 전환·Pause·선택 UI·Run 재시작의 잔상 없음, 기존 Fishing Rod·Squid·Puffer 연출을 확인했다. 새 Net 테스트 10개를 추가했고 기존 복어 테스트의 일반 Net 접촉 VFX 기대값만 갱신했다.
- **승인:** Net Pixel Art **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**. Net Placement Preview **Manual Validation: Passed / Prototype Approval: Approved**. Net Contact VFX **Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending**. Net Place SFX **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending**. Preview와 실제 gameplay 영역은 같은 authoritative 배치 값으로 생성한다. 지속 Tick VFX/SFX를 남발하지 않으며 Slow·Resistance damage·Tick·area·설치 수·비용은 변경하지 않았다. 길이·두께·animation 속도·VFX lifetime·SFX volume은 최종 출시 확정값이 아니다. 이번 문서 갱신에서 Computer Use와 Git add/commit/push는 수행하지 않는다.

## VS-2D-1 — 낚싯대 Presentation Prototype (2026-09-26, Unity 수동 검증 완료)

- 실제 배치형 낚싯대 공격을 조사했다. 프리팹 직렬화 값은 포획력 4, 간격 0.8초, 사거리 1.5, 루트 Scale 0.35, BoxCollider2D Trigger 2×2이다. Scene 배치 상한은 3개다. 범위 안 물고기 중심만 대상으로 하고 CatchValue 내림차순·거리 오름차순으로 선택한다. `TakeCaptureDamage`가 Resistance를 적용하고 포획 시 물고기를 비활성화한다.
- 기존 상시 Target Line을 성공한 피해 직후 0.18초 줄로 바꿨다. 같은 성공에서 낚싯대 본체 3프레임/0.27초, 실제 선택 대상 위치의 4프레임 Hit/0.22초, 0.19초 Reel/물방울 WAV를 연출한다. 여러 대상은 각각 Hit을 표시하며 줄은 첫 실제 피해 대상에 연결한다. Target 탐색·피해·간격·사거리·Collider·이동·설치 수치는 변경하지 않았다.
- 본체 32×32 셀·PPU 32는 기존 화면 약 29px footprint를 유지하기 위한 Tool Prototype 규격이다. Hit은 16×16 셀·PPU 83이다. 현재 Tool 전체 또는 Fish 전체의 최종 PPU 기준이 아니다. 설치 Preview도 같은 Idle Sprite를 표시한다.
- **Placement Preview 보완:** 사용자 Play Mode에서 실제 공격 연출은 정상이나 Q 배치 중 Ghost/Range가 보이지 않는 문제를 확인했다. Scene의 기존 `FishingRodPreview`는 이미 커서를 따라가지만 SpriteRenderer 알파가 약 0.039였고, Range Preview 오브젝트는 없었다. 기존 Preview의 RGB 틴트를 유지하고 실제 낚싯대 Idle Sprite를 적용하면서 알파를 보이게 올렸고, 프리팹의 `RangeVisual`을 Preview 자식으로 한 번만 복제했다. 프리팹의 실제 `CaptureRange`와 적용되는 Run 사거리 보너스로 원 크기를 계산하며 둘 다 같은 커서 위치를 따른다. 확정 시 화면에 보인 위치에 설치하고 Ghost/Range를 먼저 숨긴다. 취소·선택 UI·Pause·도구 슬롯 변경·Run 재시작 시 비활성화한다. 배치 좌표 유효성 규칙은 기존에 없고, 기존 Gold/설치 상한 검사는 유지했다.
- `FishingRodPresentationProfile`을 Resources에 두고 런타임에서 낚싯대별 `FishingRodPresentation`을 연결한다. Profile 누락, VFX 풀 포화, 오디오 제한 시에도 포획 판정은 유지한다. 공용 `CombatVfxPool`과 재사용 AudioSource를 사용하며 동일 효과음은 전역 0.09초 중복 제한이다. 낚싯대별 타이머·줄은 비활성/이동/Run 종료에 초기화되고 scaled time에서 Pause된다.
- 프로젝트 내부 생성 PNG/WAV, 안정적 Sprite/Audio 메타데이터와 Profile 참조, 반복 실행 가능한 Setup/Validate 메뉴, `FishingRodPresentationTests` 7개와 Placement Preview 테스트 5개를 추가했다. Scene/Prefab YAML, ProjectSettings, URP는 직접 수정하지 않았다. Setup 메뉴는 Unity Editor API로 프리팹 참조를 연결한다.
- **검증 상태:** PNG/WAV 및 GUID/참조 정적 검사를 통과했다. 사용자의 원본 Unity EditMode 실행은 처음에 211/212 통과·1 실패였고, 실패한 Preview 진입 테스트의 합성 Q 입력은 `isPressed=true`였지만 `wasPressedThisFrame=false`여서 진입 신호가 없었다. EditMode Fixture에서 슬롯 연결을 확인한 뒤 컨트롤러에 해석된 배치 입력을 전달하도록 수정했다. 2026-09-26 Unity 6000.3.11f1 격리 프로젝트 복사본에서 Runtime·Editor/Test 컴파일 후 실패 테스트 단독 1/1, Preview 5/5, Presentation 7/7, 전체 EditMode **212/212 통과**(실패·Skip 0)를 실제 실행했다. 이후 사용자가 원본 Unity에서 compile 정상, Console Error 0, 전체 EditMode **212/212 통과**를 확인했다.
- **수동 검증·승인:** 사용자가 Play Mode에서 Fishing Rod Pixel Sprite, Q/W 동적 슬롯 배치 진입, 빠른 커서 이동을 포함한 Ghost·Range 동위치 추적과 실제 공격 사거리 일치, 확정 위치 일치, 확정·취소·도구 변경·Pause·선택 UI·Run 재시작 후 잔상 없음, Preview의 공격·Collider·피해 없음, 설치 후 Attack Animation·Line·Hit VFX·SFX, 대상 없음·다중 낚싯대·이동/재설치, 기존 Fish/Squid/Puffer/Item/Synergy Presentation을 확인했다. Fishing Rod Sprite **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**. Attack Presentation·Line과 Hit VFX는 각각 **Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending**. SFX **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending**. Placement Preview **Manual Validation: Passed / Prototype Approval: Approved**. 기존 damage·range·attack interval·target selection·placement gameplay 값은 변경하지 않았다. Git add/commit/push는 요청에 따라 수행하지 않았다.

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
- 이 계획 작성 당시 적용 폰트는 NanumGothic-Bold SDF, Dynamic atlas였다. 갈무리 9 우선 검토와 미도입 기록은 이후 상단의 Galmuri11 Prototype Typography 승인 이전 상태다.
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
