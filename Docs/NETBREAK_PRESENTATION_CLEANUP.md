# Legacy Prototype Presentation Cleanup Report

기준: 2026-09-29, Area 1 Vertical Slice. 사용자 Full Run으로 Prototype Presentation/Font 안정화를 확인했다. Final Production Approval은 Pending이다.

### TMP Font 안정화 최종 정리 및 사용자 검증 (2026-09-29)

사용자가 Area 1 시작부터 Result까지 Full Run을 완료했다. 한국어/Galmuri UI에 깨진 글리프나 □가 없었고, Tutorial, Growth/TAB, Giant Tuna/Shark 저항 UI, Puffer/Squid 표현, 구 첫 등장 팝업 제거, 자홍색 경로선 숨김과 Gameplay가 정상으로 확인됐다. Full Run 중 `Importer(NativeFormatImporter) generated inconsistent result` 경고도 재발하지 않았다. Result는 정상 진행에 문제가 보고되지 않은 수준으로 확인했으며 세부 Production UI 승인은 아니다.

Full Run 전후 SDF diff를 사용자가 직접 비교하지는 않았다. 최종 정리에서는 네 에셋의 정리 전 파일을 무시되는 `Logs/TMPFontCleanupBackup_20260929/`에 보관하고, HEAD의 완전한 직렬화 데이터를 기준으로 각 에셋의 `m_ClearDynamicDataOnBuild: 1 → 0`만 보존했다. 따라서 Galmuri11의 138→235 글리프/Atlas 2→3/index 1→2, Bold의 117→111 글리프와 텍스처·packing 변화, Nanum Bold의 295→66 글리프/Atlas 4→1/index 3→0을 포함한 생성 churn은 최종 Git diff에서 제거됐다. Nanum Regular는 처음부터 설정 외 차이가 없었다. 최종 네 에셋은 각각 설정 한 줄 diff이며 `.meta` GUID, Source Font, Material, fallback, Dynamic Mode와 기존 Atlas 상태를 유지한다.

정리 뒤 Unity 6000.3.11f1 배치 컴파일과 `Area1FontStabilityTests` 6/6, 전체 EditMode **280/280 Passed**를 확인했다. 네 에셋의 연속 강제 임포트에서 파일 바이트는 변하지 않았고 테스트 로그에 해당 Importer 경고가 없었다. 전체 `git diff --check`도 통과했다. Runtime Font Clone 구조는 그대로다. Editor 재시작과 별도의 전후 SDF diff 비교는 아직 수행하지 않았으므로 그 반복 Dirty 검증은 별도 관찰 항목으로 남긴다. **Unity Manual Font/Presentation Validation: Passed / Prototype Font Stabilization Approval: Approved / Prototype Presentation Cleanup Approval: Approved / Final Production Approval: Pending.**

### TMP Font Importer 후속 조사 (정리 전 기록, 2026-09-29)

이 절은 아래의 초기 Dirty 조사 이후에 확인된 내용이다. 현재 Unity Editor 로그에서 Galmuri11과 Bold의 `Importer(NativeFormatImporter) generated inconsistent result` 호출 스택은 `TMP_EditorResourceManager.DoPostRenderUpdates` → `AssetDatabase.ImportAsset`이다. 설치된 TMP 2.0.0은 Dynamic Multi Atlas가 확장되면 `SetupNewAtlasTexture`에서 새 Texture2D를 만들고 `AddTextureToAsset` 및 재임포트를 등록한다. 실제 경고 세 건 모두 이 TMP 재임포트 경로를 탔다. 임포트 자체의 바이트 차이가 경고 원인이라는 증거는 없으며, 당시 두 artifact ID는 달랐지만 로그의 produced file content hash는 같았다. 따라서 로그로 확인한 직접 트리거는 **렌더 중 원본 TMP 에셋의 Atlas 하위 에셋 변경과 TMP 예약 재임포트**다. Unity 내부 artifact ID 차이의 더 낮은 수준 원인은 확인되지 않았다.

정리 전 HEAD → 작업 트리 직렬화 비교: Galmuri11은 문자/글리프 138→235, 실제 Atlas 2→3, 활성 Atlas index 1→2다. 새 문자 106개와 제거 문자 9개가 있으며 Texture2D 하위 에셋 하나와 픽셀 데이터·packing rect가 바뀌었다. Bold는 117→111, Atlas 2→2, index 1 유지이며 새 문자 12개·제거 18개, 픽셀 데이터·packing rect가 바뀌었다. Nanum Regular는 10→10, Atlas 1→1이고 `m_ClearDynamicDataOnBuild` 1→0만 다르다. Nanum Bold는 295→66, Atlas 4→1, index 3→0이고 문자 229개가 사라졌으며 텍스처와 packing rect가 바뀌었다. 네 에셋의 Source Font GUID, 기본 Material 참조, fallback 관계, Atlas Population Mode, 크기 1024×1024, multi-atlas 설정은 유지됐다. 네 에셋 모두 Clear Dynamic Data On Build는 1→0이다. `.meta` GUID도 유지됐다.

과거 EditMode 로그에는 `Clearing [NanumGothic-Bold SDF] dynamic font asset data` 바로 다음 임포트와 동일 경고가 있다. 설치 TMP의 Editor 종료 callback은 해당 설정이 켜진 Dynamic 폰트를 비운다. 이 경로는 이전 반복 Dirty의 직접 증거이며, Galmuri 현 경고는 설정을 끈 뒤에도 남은 런타임 Atlas 변경 경로다.

`Area1Typography`는 Galmuri 원본 대신 동일 TTF·Atlas 규격의 메모리 전용 Dynamic 폰트를 만들고 Nanum Bold fallback도 메모리 전용으로 생성한다. UI 배치·문구·Scene 참조는 변경하지 않았다. Font 테스트는 두 복제본에서 한국어와 키 문자열을 실제로 추가해 원본 파일 바이트와 글리프 표가 변하지 않음을 확인했다. 네 원본의 연속 강제 임포트도 파일 바이트가 불변이었고 테스트 로그에 inconsistent result 경고는 없었다. 전체 EditMode는 **280/280 Passed**다. Editor에서 직렬화된 원본 Nanum 참조와 `FishingRodController`의 Galmuri Bold 직접 로드는 남아 있으므로 모든 원본 폰트의 향후 Dynamic glyph 추가가 완전히 차단됐다고 판단하지 않는다. 사용자 실제 Play에서 경고 재발은 없었으며 Unity 재시작 전후 Git diff 비교는 아직 수행하지 않았다.

## 1. TMP Font Dirty Root Cause

작업 시작 때 변경된 파일은 아래 네 TMP 에셋뿐이었다. Diff를 조사한 뒤 기존 변경을 버리지 않았다. 네 에셋의 Atlas Population Mode는 모두 Dynamic(1), `m_ClearDynamicDataOnBuild`는 모두 1이었다. Unity 6.3의 설치된 TextMeshPro `TMP_EditorResourceManager`는 Editor 종료 시 이 설정이 켜진 Dynamic 폰트의 글리프/문자 표를 비우고, `TMP_PreBuildProcessor`도 빌드 전에 같은 데이터를 비운다. 이후 UI 렌더링으로 필요한 글리프가 다시 추가되는 것이 반복 Dirty의 주원인이다. 프로젝트 C#에서 `ClearFontAssetData` 또는 `TryAddCharacters` 호출은 발견되지 않았다. 실제 종료 순간의 호출 로그는 확보하지 않았으므로 호출 순서 자체는 패키지 코드와 diff에 근거한 추론이다.

| Font | 시작 시 HEAD → Working Tree 직렬화 차이 | 이번 조치 |
|---|---|---|
| Galmuri11 | 문자/글리프 138→0, Atlas 2→1 | Dynamic 유지, Clear Dynamic Data On Build 해제 |
| Galmuri11 Bold | 문자/글리프 117→0, Atlas 2→1 | 동일 |
| NanumGothic Regular | 문자/글리프 10→2, Atlas 1→1; 일부 Atlas 배치/텍스처 데이터 변동 | 동일 |
| NanumGothic Bold | 문자/글리프 295→2, Atlas 4→1 | 동일 |

Galmuri의 추가 Atlas가 사라졌고 기본 Atlas 중 일부는 1×1로 축소됐다. 이후 Play와 EditMode 테스트로 문자/글리프가 Galmuri11 52, Bold 31, Nanum Regular 10, Bold 66개까지 다시 늘었다. Font Source GUID, 기본 Material과 fallback 관계는 변경되지 않았고 추가 Atlas 참조만 초기화 과정에서 줄었다. 새로 필요한 글리프는 Dynamic Atlas에 한 번 추가되면서 에셋이 다시 수정될 수 있다. 자동 비우기와 재생성의 반복은 이번 설정으로 멈추도록 했다. Unity 에디터 메뉴 `Tools → NETBREAK → Stabilize TMP Font Assets`로 네 에셋의 설정을 `SerializedObject`를 통해 변경했고 로그에서 4개 갱신을 확인했다. 생성기를 다시 실행할 때도 같은 설정을 사용한다. 전체 Run 후 네 파일의 추가 변경 여부는 수동 검증이 필요하다.

## 2. Font Usage Inventory

| Font | 사용 위치 | 용도 | 최종 정책 |
|---|---|---|---|
| Galmuri11 | `Area1Typography`, HUD/튜토리얼/기존 GameCanvas TMP 전체의 기본 런타임 스타일 | 플레이어 기본 글자 | 유지 |
| Galmuri11 Bold | 제목·이름, 먹물 방해 월드 라벨, 에디터 UI 생성기 | 강조 글자 | 유지 |
| NanumGothic Bold | Main Scene 직렬화 TMP 109개, Galmuri fallback, 기존 폰트 에셋 | 이전 참조와 미지원 글리프 보완 | Scene 직렬화 참조 보존; 런타임 표시에는 Galmuri 적용 |
| NanumGothic Regular | Main Scene 직렬화 TMP 1개 | 이전 참조 | 보존; 런타임에는 Galmuri 적용 |
| TMP 기본 폰트 | Main Scene 직렬화 TMP 3개, TMP Settings의 default | 폰트 로드 실패 시 기본값 | 런타임 GameCanvas 하위에는 Galmuri 적용 |

Main Scene의 `m_fontAsset` 참조는 총 113개다. Scene/Prefab YAML은 수정하지 않았다. `Area1Typography.ApplyToHierarchy`가 기존 Canvas 자손의 활성·비활성 TMP를 처리하며, 이후 HUD 전용 스타일이 역할별 폰트를 다시 지정한다. `Area1TutorialController`의 런타임 TMP도 Galmuri를 지정한다. Tool Tree, Growth, Item 에디터 생성기는 이후 실행할 때 Galmuri Bold를 선택하도록 바꿨다. 기존 Scene에 생성기를 다시 실행하지 않았다. Galmuri의 Nanum fallback은 유지했다. 글리프 커버리지와 장문 가독성의 최종 검증은 Pending이다.

## 3. Player-facing Presentation Inventory

| 항목 | 현재 상태 | 분류 | 조치 |
|---|---|---|---|
| Main HUD: 골드·포획·어획률·레벨·경험치·구간·시간 | Pixel 프레임과 실제 Run 값 | KEEP | 기존 기능 유지, Canvas 폰트 통일 |
| Hotbar LMB/Q/W/E/R, 속도, TAB | 현재 도구/스킬 상태 | KEEP | 입력·쿨다운 유지 |
| 준비/시작, 일반 어군·MiniBoss·Boss 경고 | `FishSpawner` 공지 | RESTYLE | 기존 공지 프레임·폰트 변경 |
| Puffer 첫 등장 설명 | 구 공지 패널, 4초 | REMOVE | 문구 표시 제거, 기존 Spawn 대기 유지 |
| Squid 첫 등장 설명 | 구 공지 패널, 4초 | REMOVE | 문구 표시 제거, 기존 Spawn 대기 유지 |
| Area1TutorialController 5단계 | 최신 하단 contextual 패널 | KEEP | 단계·입력·시간 유지 |
| Growth/Tool Tree/E 선택, TAB/Inventory/Tooltip | 기존 선택 및 정보 UI | KEEP | 게임 Canvas 폰트 적용, 레이어·선택 기능 유지 |
| Giant Tuna Resistance·보상 | 이전 반투명 패널, 정확한 Resistance 값 | RESTYLE | Pixel 프레임·Galmuri·청록 막대 적용 |
| Shark Resistance·Phase·회유·회유 사이 회복 | 이전 반투명 패널, 정확한 Resistance 값 | RESTYLE | Pixel 프레임·Galmuri·청록 막대 적용, 정보 유지 |
| 결과 화면·월드 피드백·먹물 상태 | 실제 보상/상태 표시 | KEEP | Canvas/월드 폰트 통일 |
| 자홍색 경로선 및 위치 라벨 | Scene의 경로 4개에서 Preview On | DEV-ONLY | 런타임 기본 숨김, 개발 플래그로 표시 가능 |
| 휴면 Augment/Job UI와 TestSpeedPanel | 정상 Run에서 기능별 조건에 따라 노출 | DEFER | Gameplay/개발용 동작 보존; Full Run에서 노출 여부 확인 |

## 4. Puffer First Encounter

`FishSpawner.RunGrowthPhase`가 `복어 출현 - 그물 포획을 방해합니다.`를 `specialFishWarningTime` 동안 보여 주고, Scene 직렬화 값은 4초였다. 상단 중앙 `AnnouncementPanel`(700×90, Y=-120)이 `PrototypeHUDCanvas`에서 이를 표시했다. 입력 차단 코드는 없지만 패널 Image의 Raycast Target은 원래 켜져 있었다. 복어 상태 연출이 이미 있으므로 해당 문구를 제거했다. 네트/복어 Gameplay와 4초 대기는 유지했다.

## 5. Squid First Encounter

`FishSpawner.RunSpecialPhase`의 `오징어 출현 - 주변 설치 어구를 먹물로 정지시킵니다.`도 같은 패널/4초 경로였다. 먹물 투사체·도구 중단·상태 라벨이 원인을 전달하므로 구 문구를 제거했다. 4초 대기와 먹물 Gameplay는 유지했다. 외부 플레이테스트에서 원인 이해가 부족하면 새 contextual 안내의 짧은 1회 메시지로 재평가한다.

## 6. Giant Tuna UI

`MiniBossHUD`는 `MiniBossController.ResistanceRatio/CurrentResistance/MaxResistance`를 사용했고 표시 문구도 `저항력`이었다. HP 계산과 문구는 없었다. 기존 `MiniBossPanel`은 상단 중앙 620×110의 반투명 Image였다. 현재 HUD Pixel 프레임과 Galmuri, 청록 Resistance fill을 런타임에 적용했다. 보상 패널도 같은 프레임을 쓴다. 값과 돌진·E 보상 흐름은 유지했다.

## 7. Shark Boss UI

`BossHUD`는 `BossEncounterController`의 Resistance와 Phase/현재 회유/최대 회유, 상태, 회유 사이 Resistance 회복량과 남은 시간을 읽는다. 플레이어 HP 표시는 없고 중복 Boss 막대도 발견되지 않았다. 기존 `BossPanel`은 상단 중앙 720×170의 반투명 Image였다. Pixel 프레임·Galmuri·청록 fill을 적용하고 회유 패널도 같은 프레임을 사용한다. Attempt 수와 회복 정보는 유지했다.

## 8. Legacy Tutorial / Notification

새 Tutorial 5개 Step은 유지했다. Puffer/Squid의 오래된 설명 2개는 Tutorial에 추가하지 않고 제거했다. 일반 어군·MiniBoss·Boss 등장 경고와 결과 공지는 인카운터 상태 전달이므로 유지했다. 공지 패널의 Raycast Target은 런타임 스타일 적용으로 껐다.

## 9. Debug / Placeholder Presentation

`FishRoute` 네 개는 Scene에서 `showRoutePreview=1`, `showDuringFishing=1`이어서 자홍색 LineRenderer가 실제 Game 화면에 나타났다. `ShowDeveloperGuides` 기본 false 아래로 Preview·라벨·Play Mode Gizmo를 묶었다. 새 코드로 Unity Play Mode에서 자홍색 선이 사라진 것을 화면으로 확인했다. TestSpeedPanel은 기존 개발 도구이자 조작 가능한 UI여서 이번 범위에서 제거하지 않았다. Scene의 `Prototype*` 이름은 내부 식별자이고 플레이어 문구로 노출되지 않는다.

## 10. 생성 / 수정 파일

폰트 에셋 네 개와 `Area1FontStabilization`, `GalmuriFontAssetSetup`, 런타임 `Area1Typography`, `Area1HUDSkin`, `PrototypeHUDCanvas`, `MiniBossHUD`, `BossHUD`, `FishSpawner`, `FishRoute`, `FishingRodController`, 세 UI 생성기, `TMPFontBatchReplacer`, 관련 테스트와 이 문서를 수정했다. 새 `.meta`는 Unity가 생성했다. Scene/Prefab은 수정하지 않았다.

## 11. Gameplay 변경 여부

Resistance, 피해, 포획식, 도구·Spawn·EXP·Gold, 회유 수, 패턴·예고 시간, Audio, Item, 진행·저장은 변경하지 않았다. Puffer/Squid의 기존 4초 대기는 유지한다. LineRenderer와 Canvas 표시만 변경했다.

## 12. Tests

정리 뒤 Unity 전체 EditMode 280/280 통과했다(Font 안정화 테스트 6/6 포함). 사용자 Area 1 Full Run에서 한국어 UI, Giant Tuna/Shark 저항 UI와 자홍색 경로선 숨김을 확인했다.

## 13. Static Validation

Main Scene TMP 참조 113개와 경로 네 개의 Inspector 값을 조사했다. 폰트 네 개의 Glyph/Character/Atlas/Source/Fallback/Mode diff를 확인했다. Scene/Prefab `.unity`·`.prefab` 파일은 변경하지 않았다. 생성된 폰트 churn 정리 후 전체 `git diff --check`가 통과한다.

## 14. 문서 갱신

`NETBREAK_STATE.md`, `NETBREAK_DESIGN.md`, `NETBREAK_ROADMAP.md`의 UI 기준과 검증 상태를 갱신한다. 이 문서는 조사 근거와 수동 확인 목록이다.

## 15. 사용자 Unity 수동 검증 절차

1. Unity Import/Compile 후 Console을 비우고 새 Run을 시작한다.
2. 기본 HUD·튜토리얼·Growth·TAB·Tooltip·Hotbar의 한국어와 Font, 겹침을 확인한다.
3. 첫 복어와 오징어에서 옛 설명 공지가 없는지, 복어 방해와 먹물 상태가 이해되는지 확인한다.
4. Giant Tuna의 저항력 막대·경고·보상, Shark의 저항력·Phase·회유/회복, Result를 확인한다.
5. 게임 화면의 자홍색 경로선/위치 라벨이 없는지 확인한다.
6. Play 전후와 EditMode Test Runner 전후의 네 TMP 에셋 diff, Console Error/Warning, 전체 280개 테스트 결과를 확인한다.

## 16. Manual / Deferred 항목

Area 1 Full Run과 관찰된 한국어 UI·MiniBoss/Boss 패널은 사용자 검증을 통과했다. 전체 문자열의 완전한 글리프 커버리지, 장문 가독성, 다양한 해상도의 최종 UI 품질, Unity 재시작 전후 폰트 diff 반복 여부는 Production 또는 별도 검증 항목이다. Scene 직렬화 Nanum 참조의 영구 이관은 수행하지 않았다. External Playtest에서 경로 가독성 문제가 확인되면 pixel arrow, 물 흐름 애니메이션, 절제된 경로 표시를 검토하되 현재 자홍색 디버그 선은 숨김을 유지한다.

## 17. Approval

- Implementation Complete: 완료
- Static Validation: 컴파일·Font 6/6·전체 EditMode 280/280·전체 `git diff --check` 통과
- Unity Manual Font/Presentation Validation: Passed (사용자 Full Run)
- Prototype Font Stabilization Approval: Approved
- Prototype Presentation Cleanup Approval: Approved
- Final Production Approval: Pending

## 18. Git

`git add`, `commit`, `push`는 수행하지 않았다. 폰트 에셋의 의도적 설정 변경만 보존하고 생성 churn은 제거했다.
