# NETBREAK 아트·오디오 에셋 목록

## Area 1 Boss Shark 1차 자산 (2026-09-28)

| 항목 | 내용 |
|---|---|
| 자산 | `Assets/Art/Fish/CoastBoss/CoastBoss_Swim.png` 및 `.meta`, `CoastBoss_VisualProfile.asset` 및 `.meta` |
| 출처·제작 | 이 프로젝트 내부 제작. 내장 ImageGen 상어 초안을 실루엣 참고로 사용하고 `Tools/generate_coast_boss_shark.py`에서 탑다운 픽셀 시트로 재구성. 외부 에셋 직접 사용 없음 |
| 규격 | 384×384 투명 RGBA, 96×96 셀×16, E/N/NE/NW 각 4 수영 프레임, PPU 83·8 FPS·Point·무압축·Mipmap Off |
| 연결 | `FishData_CoastBoss.asset` → `CoastBoss_VisualProfile.asset` → 시트의 16 Sprite. 공용 FishVisualController 재사용, Animator 없음 |
| 검증·승인 | 정적 확인 Complete. 사용자 Unity에서 Sprite import·Profile 연결·방향·수영·Boss encounter 전 흐름과 기존 Fish/MiniBoss 정상 동작 확인, 관련 Console Error/Exception 없음. **Unity Manual Validation: Passed / Prototype Approval: Approved / Final Production Approval: Pending** |

Phase별 변형, 포획·도주·피격 전용 연출은 아직 없다. 기존 Boss gameplay·HUD·Reward·Run 결과와 MiniBoss/일반 Fish 자산은 유지한다.

## TAB 성장 관리 UI Prototype 승인 자산 상태 (2026-09-28)

사용자가 Unity Game View에서 현재 성장 관리 UI의 해양 픽셀 프레임·Galmuri11 짧은 텍스트·탭 대비·4개 아이템 슬롯·단일/복합 시너지 구분과 공통 빈 슬롯 + 표시를 확인했다. 메인 `ItemHUD`와 TAB `GrowthItemPage`는 기존 `Assets/Resources/UI/Area1/icon_empty.png`(GUID `cd6e4b5cb2af00c1e9ce18d88d049d35`)를 공유한다. 메인 HUD의 기존 Rect와 TAB 전용 빈 슬롯 Rect를 분리하며 새 TAB PNG는 없다. 긴 Tooltip/설명은 기존 NanumGothic 계열을 유지한다. **Implementation: Complete / Static Validation: Complete / Unity Manual Growth Management Visual Validation: Passed / Prototype Growth Management UI Approval: Approved / Final Production Growth Management UI Approval: Pending.** 아래 성장 관리 관련 Pending 표기는 승인 전 자산 반복의 이력이다. 이번 마감에서 자산 파일은 수정하지 않았다.

## TAB 전용 공통 + 표시 Rect 보정 (2026-09-28)

`icon_empty.png`와 기존 `.meta`/GUID는 변경하지 않는다. 메인 `Area1ItemIcon`은 기존 32×32 배치 그대로이고, TAB의 `GrowthItemIcon`만 빈 슬롯에서 32×32/중앙 Anchor·Pivot/Offset (-4,-2)로 표시한다. 보유 아이템은 이전 크기 영역을 유지한다. 이번 갱신 파일은 표시 코드 `GrowthManagementSkin.cs`, `GrowthItemPage.cs`와 문서 3종이며 신규 PNG는 없다. Unity 수동 시각 확인과 승인은 Pending이다.

## 공통 + 2px 후보·TAB 중심 보정 (2026-09-28)

| 항목 | 파일·역할 | 상태 |
|---|---|
| 공통 + Sprite | `Assets/Resources/UI/Area1/icon_empty.png` | 32×32 RGBA8, 십자 26px 길이/2px 두께/4톤, 기존 `.meta` GUID `cd6e4b5cb2af00c1e9ce18d88d049d35` 유지 |
| 자산 재생성 | `Tools/refine_area1_empty_icon.js` | 공통 PNG 한 장만 생성; 다른 Sprite나 `.meta` 수정 없음 |
| TAB 위치 | `Assets/Scripts/UI/GrowthManagementSkin.cs` | `GrowthItemIcon` 중심을 `(0.48, 0.50)`으로 보정; 슬롯 크기·번호·상태 텍스트 유지 |

메인 `ItemHUD`와 TAB `GrowthItemPage`는 공통 `icon_empty`를 계속 참조한다. 메인 HUD 아이콘은 기존 슬롯 중앙 배치를 유지한다. Scene/Prefab, 탭·시너지·버튼 구조는 변경하지 않았다. Unity Import·시각/Console 확인과 승인은 **Pending**이다.

## 공통 빈 아이템 슬롯 십자·TAB 선택 대비 (2026-09-28)

| 자산/코드 | 경로/역할 | 상태 |
|---|---|---|
| 공통 빈 슬롯 십자 | `Assets/Resources/UI/Area1/icon_empty.png` | 기존 32×32 RGBA PNG 교체, `.meta`/GUID `cd6e4b5cb2af00c1e9ce18d88d049d35` 유지; 메인 `ItemHUD`와 TAB `GrowthItemPage`가 동일 Sprite 로드. 빈 Q/W 도구 및 알 수 없는 아이템 fallback도 이 Sprite 사용 |
| 결정론적 재생성 | `Tools/refine_area1_empty_icon.js` | 신규. `icon_empty.png`만 생성하고 `.meta`는 건드리지 않음 |
| TAB 표시 | `Assets/Scripts/UI/GrowthManagementSkin.cs` | 같은 `selected_slot` Sprite를 선택 시 원색/비선택 시 어두운 청록 착색, TAB 슬롯 아이콘 위치 및 복합 시너지 제목 띠 높이 조정 |

기존 메인 HUD 코드·RectTransform, 다른 PNG 38종과 `ref_` PNG, Scene/Prefab/입력·데이터 구조는 유지한다. 기존 HUD 일괄 생성기는 이전 후보를 다시 출력할 수 있으므로 이 십자 디자인을 재생성할 때는 위 전용 스크립트를 사용한다. **Static Validation: Complete / Unity Import·Visual Validation: Pending / Prototype Approval: Pending.**

## TAB 성장 관리 UI 스킨 (2026-09-28)

| 항목 | 프로젝트 경로/사용 | 상태 |
|---|---|---|
| 성장 창 표시 코드 | `Assets/Scripts/UI/GrowthManagementSkin.cs` + `.meta`; `SkillTreeCanvas`, `SkillTreeBranchView`, `SkillTreeNodeView`, `GrowthItemPage`, `SkillTreeTooltip` 표시 연결 | 신규 코드 1쌍, 기존 코드 5개 수정 |
| 공용 프레임 | `Assets/Resources/UI/Area1/{panel,header,button,slot,selected_slot,ref_item_slot}.png` | 기존 PNG/.meta/GUID 재사용, 수정 0 |
| 아이템/장식 | 같은 폴더의 `icon_empty`, 6종 기존 아이템 아이콘, `decor_{rope_knot,shell,coral,leaf}` | 기존 PNG/.meta/GUID 재사용, 수정 0 |
| 서체 | `Assets/Resources/UI/Fonts/Galmuri11 SDF.asset`, `Galmuri11 Bold SDF.asset`; 기존 NanumGothic-Bold SDF | 짧은 UI/숫자 Galmuri11, 긴 설명·Tooltip NanumGothic |

변경 화면: 전체 창·헤더·스킬 트리/아이템 탭, Q/W/E/R Branch·노드·연결선, 네 아이템 슬롯, 단일/복합 시너지 섹션·카드·상세·확인 버튼, 성장/아이템/속성 Tooltip. Scene/Prefab, PNG/PNG `.meta`, gameplay·TAB·페이지/선택 이벤트는 수정하지 않았다. Unity Import·컴파일·시각/상호작용 검증과 승인은 사용자 확인 대기다. **Static Validation: Complete / Manual Validation: Pending / Prototype Approval: Pending.**

## Galmuri11 UI Typography Prototype (2026-09-28)

| 자산 | 출처 | 프로젝트 경로 | 사용/상태 |
|---|---|---|---|
| Galmuri11.ttf | 공식 `quiple/galmuri` `dist/Galmuri11.ttf` | `Assets/UI/Fonts/Galmuri/Galmuri11.ttf` | HUD Body·Numeric·짧은 상태 Text, Prototype 승인 |
| Galmuri11-Bold.ttf | 공식 `quiple/galmuri` `dist/Galmuri11-Bold.ttf` | `Assets/UI/Fonts/Galmuri/Galmuri11-Bold.ttf` | Major Title·주요 Button/Heading·Key 강조, Prototype 승인 |
| LICENSE.txt | 공식 `quiple/galmuri` `dist/LICENSE.txt` | `Assets/UI/Fonts/Galmuri/LICENSE.txt` | SIL Open Font License 1.1 원문 |

원본 커밋: `71e1cacf1437a11220307120e63e30bc275312d4`. 공식 SIL Open Font License 1.1 원문은 프로젝트에 보존한다. `Assets/Resources/UI/Fonts/Galmuri11 SDF.asset`와 `Galmuri11 Bold SDF.asset`이 Dynamic atlas로 생성되었고 각 원본 TTF 및 기존 `NanumGothic-Bold SDF` fallback을 참조한다. 긴 Tooltip/설명문에는 NanumGothic 계열을 유지한다. 사용자가 Unity Game View의 현재 스타일을 확인하여 Area 1 Prototype Typography의 기본 Font Family로 승인했다. **Implementation: Complete / Static Validation: Complete / Unity Manual Typography Visual Validation: Passed / Prototype Typography Approval: Approved / Final Production Font Approval: Pending.**

## Reference Frame Rebuild (2026-09-28, 현재 적용)

- 기존 39 PNG 경로/GUID 유지. 변경 8개: panel.png, header.png, button.png, slot.png, selected_slot.png, key.png, decor_rope_knot.png, decor_wave.png. 나머지 31 PNG는 세션 시작본 그대로다.
- 프레임 6개: 32×32→64×64, 최대 Import 크기 64, PPU 100, Border 사방 12px. 로프: 기존 48×48/Importer 유지. 파도: 48×48→256×48, 최대 Import 크기 256, 기존 Border 0/PPU 유지. 모든 Point/무압축/무MipMap/GUID는 유지한다. 프레임 이미지가 커진 것은 화면 UI 크기 변경이 아니다.
- 내장 ImageGen 8회 제작, 원본·프롬프트는 Artifacts/MarineUIReference/Sources 및 generation-manifest.json. 출력은 Tools/pack_area1_reference_frames.ps1, 연결은 Area1HUDSkin.cs. 기존 생성 스크립트를 다시 실행하면 이전 후보로 덮어쓸 수 있으므로 최신 스크립트/산출물을 사용한다.
- 현재 PNG와 필요한 메타데이터는 Artifacts/MarineUIReference/REFERENCE_UI_ASSETS.zip에 보관한다. 이 ZIP만으로 코드 변경을 대신할 수는 없다. 이전 자산은 BEFORE_ASSETS.zip. 정적 검사는 static-validation.json, 사용자 검증은 UNITY_CHECKLIST.md.
- 실제 Unity Import/컴파일/Play 및 Reference 동일성 승인은 Pending. 이번 작업에서는 실행하지 않았다.

## Area 1 Marine Refinement (2026-09-28, 39 PNG 적용)

- Assets/Resources/UI/Area1/의 기존 39 PNG: 프레임 6개/아이콘 22개는 32×32, 장식 11개는 48×48. RGBA8, Alpha 0/255. 파일명·경로·39 .meta/GUID·Point/무압축/무MipMap·Sprite Border를 보존했다. 신규 Unity 에셋은 없다.
- 이전 후보 39종을 공통 팔레트와 픽셀 밀도로 재패킹했다. decor_wave만 새 ImageGen 연속 파도 소재를 사용한다. 원본과 프롬프트: Artifacts/MarineUIRefine/wave-source.png 및 WAVE_PROVENANCE.md. 다른 원본은 BEFORE_39.zip과 이전 MarineUI/generation-manifest.json에서 추적한다.
- 생성·연결 범위: Tools/refine_area1_hud_art.ps1, Assets/Scripts/UI/Area1HUDSkin.cs, Assets/Scripts/UI/PrototypeHUDCanvas.cs. 정적 비교/미리보기/검증/ZIP은 Artifacts/MarineUIRefine/에 저장한다. 전체 39개 파일별 결과는 static-validation.json, 목록은 FINAL_REPORT.md 참조.
- 실제 Unity Import/컴파일/Play/TMP 한글·오버플로·버튼/드래그/툴팁 검증은 수행하지 않았다. 사용자 수동 확인 대기이며 제작 완료 승인으로 기록하지 않는다.

## Area 1 HUD 39 PNG 전면 재제작 후보 (2026-09-28, 현재 적용·승인 대기)

Assets/Resources/UI/Area1/의 39 PNG 전체를 교체했다. 프레임 6개와 아이콘 22개는 32×32, 기존 장식 11개는 48×48이다. 신규 Unity Asset은 없다. current_ui_39.zip과 일치했던 시작본 39개 모두 새 해시로 바뀌었고, 기존 파일명·경로·39개 .meta/GUID/PPU/Point/무 Mipmap/무압축 및 Border(프레임 사방 5px, 나머지 0)는 보존됐다. 파일별 실제 연결·역할·GUID·원본 ZIP 비교는 ../Artifacts/MarineUI/ASSET_AUDIT.md와 asset-audit-before.json에 있다.

제작은 내장 ImageGen의 파일별 개별 생성이며 CLI/API 대체는 사용하지 않았다. 프롬프트/원본 경로는 generation-manifest.json, 원래 규격의 최근접 샘플링·hard alpha·9-slice 패킹은 export-candidates.ps1에 기록했다. 이전 Tools/generate_area1_hud_art.js는 이번 후보의 원본 생성기가 아니다. PNG CRC·RGBA8·크기·알파·메타 보존·참조 및 6종 Stretch 영역 검사가 통과했고 ZIP 39개는 적용 PNG와 모두 같은 해시다. 최종 ZIP/비교 시트/정적 합성/검증 JSON/33개 사용자 확인 항목은 ../Artifacts/MarineUI/에 있다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** Unity 실행·컴파일·Console·Play/Tests 검증은 이번에 수행하지 않았다.

## Area 1 HUD/UI Marine Decoration Iteration 4 (2026-09-27, 승인 대기)

`icon.zip`의 28개 PNG는 현재 `Assets/Resources/UI/Area1/`와 동일한 시작본이었다. 파일명·용도·기존 `.meta` GUID를 보존한 채 6개 9-slice 프레임과 22개 의미별 아이콘의 Deep Navy/Teal·Cyan·Wood/Rope·Sand 표현을 갱신했다. `Tools/generate_area1_hud_art.js`는 기존 `.meta`가 있으면 덮어쓰지 않으며 28개 PNG 모두 원본 ZIP과 해시가 달라졌다.

새 장식 11종 `decor_palm`, `decor_gull`, `decor_starfish`, `decor_shell`, `decor_coral`, `decor_leaf`, `decor_rope_knot`, `decor_bobber`, `decor_wave`, `decor_crate`, `decor_clock`는 각각 48×48 RGBA/Point/무 Mipmap/무압축 Sprite다. 출처는 첨부 Sprite Sheet의 디자인 언어를 참고한 프로젝트 내부 결정론적 픽셀 제작이며, 원본 Sheet에서 7×4 순서 절단·임의 의미 매핑을 하지 않았다. 장식은 `Area1HUDSkin`이 Canvas의 각 패널 외곽에 Raycast를 막지 않는 `Image`로 별도 배치한다. 9-slice의 Stretch 영역에는 포함하지 않는다.

Unity 6000.3.11f1에서 39 PNG Import 및 컴파일을 확인하고 작은 Game View에서 장식과 텍스트를 확인했다. 최종 전체 화면 확대 검토와 기능 회귀·사용자 승인은 대기한다. **Manual Visual Validation: Partial / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## Area 1 HUD/UI Visual Iteration 3 (2026-09-27, 사용자 Unity 검증 대기)

Iteration 2의 작은 목재 프레임 반복을 줄이고 선택한 두 번째 UI 레퍼런스에 맞춰 Deep Navy/Teal 내부, 얇은 Cyan 경계, 필요한 바깥 모서리에만 목재·로프 강조를 사용한다. `panel`, `slot`, `button`, `key`의 기존 GUID를 유지하면서 `header`, `selected_slot`을 별도 32×32 9-slice Sprite로 추가했다. 기존 14종 도구·스킬·아이템 아이콘에 정보 7종(`icon_stat_*`)과 배속 파도(`icon_speed`)를 더해 총 28개 독립 PNG다. 모두 `Assets/Resources/UI/Area1/`에 있고 Point/무 Mipmap/무압축, 9-slice는 5px Border다. `Tools/generate_area1_hud_art.js`가 PNG와 `.meta`를 재생성한다. UI 전체를 고정 이미지로 만들지 않고 런타임 RectTransform 크기에 맞춰 재사용한다. Unity Import/Play 시각 검증과 승인은 남아 있다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## Area 1 HUD/UI 픽셀 프레임 (2026-09-27, 사용자 Unity 검증 대기)

Visual Iteration 2에서 `Assets/Resources/UI/Area1/`의 `panel`, `slot`, `button`, `key`는 32×32 RGBA 9-slice 경계를 사방 8→5px로 줄이고 Deep Navy/Teal·얇은 Cyan 테두리로 다시 제작했다. 기존 도구/스킬/빈 상태 아이콘 8종에 `icon_storm_orb`, `icon_capacitor_coil`, `icon_spectral_scabbard`, `icon_autonomous_sword_array`, `icon_frost_sigil`, `icon_frost_crystal` 6종을 더해 총 18 PNG다. 모두 Point/무 Mipmap/무압축 Sprite Import 설정이며 `Tools/generate_area1_hud_art.js`로 결정론적으로 재생성한다. 기존 GameCanvas의 정적 HUD와 런타임 핫바가 Resources 경로로 읽고 새 Item HUD 아이콘은 기존 Inventory 상태에 맞춰 표시한다. Unity Import/Play 화면/Console 검증과 최종 UI 승인은 사용자 확인 전이다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

## AREA1-BG-001 — 고품질 정적 배경 후보 (2026-09-27, 현재 적용)

| 항목 | 내용 |
|---|---|
| Asset ID | AREA1-BG-001 |
| 파일 | `Assets/Resources/Area1/CoastBackground.png` 및 기존 `.meta` GUID `86f44e43f8174c7d95f748a04009f8b1` |
| 출처·제작 | 사용자 첨부 16:9 이미지를 기준으로 내장 ImageGen 정밀 편집. 실제 경로와 겹치는 네 모서리의 큰 장식, 좌측 중간 산호, 우측 상부 군집을 제거·축소하고 주변 고품질 바다 질감으로 메움 |
| 권리 | 사용자 제공 원본의 최종 소유·배포 권한과 출시 사용 승인은 별도 확인 |
| 형식·Import | 1672×941 불투명 RGB PNG, Sprite Single, Point, Mipmap Off, PPU 32, 기본 무압축, Max Size 2048. Import 축소 없음 |
| 표현·경로 | 일반 어군/Boss 1차 좌상단→우하단, Boss 2차 좌하단→우상단, Boss 3차 좌측→우측 지그재그 및 요청된 우상단→좌하단 보호. 중앙 열린 바다, 상·하단 일부만 물 아래 모래·작은 장식 |
| 연결 | `Area1BackgroundController`가 Main Scene 로드 시 `Resources.Load<Sprite>`로 Main Camera 자식 SpriteRenderer(-1000)에 단일 이미지 연결·카메라 맞춤·중복 방지. 이전 수면 오버레이 제거 |
| 검증 | 이전 Asset·meta·Scene 좌표·참조·코드 정적 점검에 이어 사용자가 실제 Unity Play Mode에서 밝기·Fish/Tool/VFX 가독성·Spawn/Exit·일반 Fish 및 Boss 경로의 시각 충돌을 확인. 이번 문서 갱신에서 Unity 재검증은 하지 않음 |
| 시각 검증·승인 | Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending |

현재 단일 PNG는 Area 1 Vertical Slice에서 사용 승인된 Prototype 방식이며 Final Production Architecture 확정이 아니다. 약간의 이질감·정적인 느낌은 원인 미확정으로 후반 Polish 검토에 남긴다. 아래 톤·흐름 개선 기록은 이전 후보의 역사적 자료다.

## AREA1-BG-001 — 탑다운 연안 배경 톤·수면 흐름 개선 (이전 후보 기록, 2026-09-27)

| 항목 | 내용 |
|---|---|
| Asset ID | AREA1-BG-001 |
| 파일 | `Assets/Resources/Area1/CoastBackground.png` 및 기존 `.meta` GUID |
| 출처·제작 | 사용자 선택 시안의 직전 적용 PNG를 ImageGen 내장 편집으로 톤·물결·수중 장식 밀도 조정. 약한 수면 반사선은 기존 Controller에서 결정론적으로 생성. 이전 절차형 생성기는 별도 Draft로만 출력 |
| 권리 | 사용자 제공 원본 시안의 최종 소유·배포 권한 확인은 별도. 배경의 최종 출시용 사용 승인과 구분 |
| 형식·Import | 1671×941 불투명 RGB PNG, 기존 GUID, Sprite Single, Point, Mipmap 없음, 무압축, PPU 32 (배경 한정 프로토타입). Runtime 반사선 1024×576 RGBA/Point/무 Mipmap |
| 표현 | 이전 PNG 대비 평균 휘도 약 12% 감소, 연결 격자 하이라이트 완화. 큰 수중 군집 네 곳→두 곳과 드문 작은 바위, 상·하단 가장자리의 약한 해저 모래. 중앙·양쪽 대각선·가로 통로 열린 바다; 물고기 실루엣·Collider 없음 |
| 연결 | `Assets/Scripts/Core/Area1BackgroundController.cs`가 Main Scene 로드 때 Resources Sprite를 Main Camera 자식(-1000)에 연결; 같은 자식 계층의 투명 반사선(-999)이 scaled time으로 약하게 왕복·Pause 정지·재시작 리셋 |
| 자동 검증 | 최종 PNG 1671×941 RGB/불투명, 격리 Unity 6000.3.11f1 Runtime·Editor/Test 컴파일 성공, 배경 3/3·전체 EditMode 244/244 통과(실패·Skip 0), 기존 GUID Import 및 오버레이 중복·정렬·Pause/리셋 검사. 원본 Play Mode/Console은 수동 검증 전 |
| 시각 검증·승인 | Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending |

요청한 우상단→좌하단 이동과 Boss 2·3차 회유 구간에 큰 장식을 두지 않았다. 실제 Scene의 활성 일반 경로는 좌상단→우하단이고 일반 어군 스폰도 왼쪽이므로 반대 대각선도 열린 바다로 유지했다. 기존 Main Scene 로드·Resources 참조를 재사용하며 Scene/Prefab YAML, ProjectSettings/URP, Camera, Gameplay·Balance·UI는 수정하지 않았다. 현재 단일 PNG와 약한 반사 오버레이는 Prototype 연결 방식이며 최종 Production 구조는 미확정이다. Unity Play Mode 수동 시각 검증·Prototype 승인·최종 출시용 아트 승인은 모두 Pending이다.

## VS-2D-4 신규 Cast Net Prototype (2026-09-27, Unity 수동 검증 완료)

| 필드 | 접힌 투망·전개 | 범위 물결 | 실제 명중 접촉 | 사용음 |
|---|---|---|---|---|
| Asset ID | TOOL-CAST-NET-BODY-001 | VFX-CAST-NET-AREA-001 | VFX-CAST-NET-HIT-001 | SFX-CAST-NET-OPEN-001 |
| File Path | `Assets/Art/Tools/CastNet/CastNet_Folded.png`, `CastNet_Open.png` | `Assets/Art/VFX/Tools/CastNet_Area.png` | `Assets/Art/VFX/Tools/CastNet_Hit.png` | `Assets/Audio/SFX/Tools/CastNet_Open.wav` |
| 규격 | 16×16 Ghost, 320×64/64×64×5프레임, PPU 64, 0.42초 | 256×64/64×64×4프레임, PPU 64, 0.28초 | 96×24/24×24×4프레임, PPU 64, 0.18초 | PCM mono/44.1 kHz/16-bit/0.34초, Profile 볼륨 0.3 |
| Production Method / Source | `Tools/generate_cast_net_presentation.py` 프로젝트 내부 직접 제작 | 같은 스크립트 | 같은 스크립트 | 같은 스크립트 내부 합성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 같은 조건 | 같은 조건 | 같은 조건 |
| Unity Linked | Resources `CastNetPresentation.asset`, 기존 조준 원과 런타임 Ghost/전개 Sprite | Profile 및 공용 CombatVfxPool | Profile 및 공용 CombatVfxPool | Profile 및 공용 one-shot Source |
| Automated Validation | 정적 규격·Unity Validate 통과, Cast Net EditMode 10/10 및 전체 241/241 통과 | 같은 Unity Validate·EditMode 통과 | 같은 EditMode 통과 | WAV 규격·EditMode 통과 |
| Manual Validation | Manual Visual Validation: Passed; Ghost/Preview Passed | Manual Validation: Passed | Manual Validation: Passed | Manual Audio Validation: Passed |
| Prototype Approval | Approved | Approved | Approved | Approved |
| Final Approval | Final Production Art·Animation Approval: Pending | Final Production VFX Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |

Scene/Prefab YAML은 수정하지 않았다. 실제 게임플레이 반경과 피해 Fish 결과를 Presentation에 전달하며 별도 target search가 없다. 사용자가 Q/W 조준·범위 업그레이드·전개·Area/Hit/Miss·다중 명중·SFX·취소·Pause/Run 초기화와 기존 도구 회귀를 Play Mode에서 확인하고 현재 Prototype 품질을 승인했다. 셀 크기·PPU·Animation/VFX 시간·SFX 볼륨은 최종 출시용 확정값이 아니다.

## VS-2D-3 신규 Scoop Net Prototype (2026-09-27, Unity 수동 검증 완료)

| 필드 | Ready 뜰채 | Swing | 명중 물보라 | 사용음 |
|---|---|---|---|---|
| Asset ID | TOOL-SCOOP-NET-BODY-001 | TOOL-SCOOP-NET-SWING-001 | VFX-SCOOP-NET-HIT-001 | SFX-SCOOP-NET-SWING-001 |
| File Path | `Assets/Art/Tools/ScoopNet/ScoopNet_Ready.png` | `Assets/Art/Tools/ScoopNet/ScoopNet_Swing.png` | `Assets/Art/VFX/Tools/ScoopNet_Hit.png` | `Assets/Audio/SFX/Tools/ScoopNet_Swing.wav` |
| 규격 | 48×48, PPU 64 | 240×48/5프레임, PPU 64, 0.26초 | 96×24/24×24×4프레임, PPU 83, 0.2초 | PCM WAV mono/44.1 kHz/16-bit/0.24초, 볼륨 0.27 |
| Production Method / Source | `Tools/generate_scoop_net_presentation.py` 프로젝트 내부 직접 제작 | 같은 스크립트 내부 제작 | 같은 스크립트 내부 제작 | 같은 스크립트 내부 합성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 같은 조건 | 외부 소재 없음; 같은 조건 | 외부 소재 없음; 같은 조건 |
| Unity Linked | Resources `ScoopNetPresentation.asset`, 런타임 뜰채 SpriteRenderer | 같은 Profile의 5 Sprite | 같은 Profile의 4 Sprite, 공용 CombatVfxPool | 같은 Profile AudioClip, 공용 one-shot Source |
| Automated Validation | Unity Setup/Validate, EditMode 전체 231/231 통과 | 같은 Setup/Validate·EditMode 통과 | 같은 Setup/Validate·VFX 풀 검증 | WAV Import·중복 제한 EditMode 검증 |
| Manual Validation | Manual Visual Validation: Passed | Manual Validation: Passed | Manual Validation: Passed | Manual Audio Validation: Passed |
| Prototype Approval | Approved | Approved | Approved | Approved |
| Final Approval | Final Production Art Approval: Pending | Final Production Animation Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |

Scene/Prefab YAML 수정 없이 기존 뜰채 Controller에서 실제 피해 Fish 목록과 당시 위치를 Presentation에 전달하며 별도 target search는 없다. 커서 Preview는 기존 Range SpriteRenderer와 동일 공격 반경을 사용한다. 사용자가 Unity compile 정상·Console Error 0·전체 EditMode 231/231 통과와 Play Mode 화면 footprint·Hit/Miss·다중 타격·1 use = 1 기본 SFX·UI 차단·Pause/Run 초기화·기존 Presentation 회귀를 확인하고 Prototype 품질을 승인했다. Damage·range·cooldown·max targets·input binding 값은 변경하지 않았다. 48×48·5프레임·animation duration·VFX size·SFX volume은 최종 출시 확정값이 아니다.

## VS-2D-2 신규 Net Prototype (2026-09-26, Unity 수동 검증 완료)

| 필드 | 반복 Net Mesh/Rope | 접촉 VFX | 설치 SFX |
|---|---|---|---|
| Asset ID | TOOL-NET-BODY-001 | VFX-NET-CONTACT-001 | SFX-NET-PLACE-001 |
| File Path | `Assets/Art/Tools/Net/Net_Mesh.png`, `Net_Rope.png` | `Assets/Art/VFX/Tools/Net_Contact.png` | `Assets/Audio/SFX/Tools/Net_Place.wav` |
| 규격 | 16×16 Mesh, 16×4 Rope, PPU 64, tiled | 96×24, 24×24 셀×4, PPU 83, 0.2초 | PCM WAV mono/44.1 kHz/16-bit/0.29초, 볼륨 0.32 |
| Production Method / Source | `Tools/generate_net_presentation.py` 프로젝트 내부 픽셀 제작 | 같은 스크립트 내부 제작 | 같은 스크립트 내부 합성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 같은 조건 | 외부 소재 없음; 같은 조건 |
| Unity Linked | `NetPresentation.asset`의 Mesh/Rope와 Net 프리팹; Preview 런타임 공유 | Profile 4프레임, 공용 CombatVfxPool | Profile AudioClip, 공용 one-shot Source |
| Automated Validation | Unity Setup/Validate, EditMode 전체 222/222 통과 | Unity Setup/Validate, EditMode 전체 222/222 통과 | Unity Setup/Validate, EditMode 전체 222/222 통과 |
| Manual Validation | Manual Visual Validation: Passed; Placement Preview Manual Validation: Passed | Manual Validation: Passed | Manual Audio Validation: Passed |
| Prototype Approval | Approved; Placement Preview Approved | Approved | Approved |
| Final Approval | Final Production Art Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |

Scene/Prefab YAML 변경 없이 Unity Editor API로 Resources Profile을 연결하고, Net 프리팹에는 런타임에 시각 컴포넌트를 붙인다. Preview와 실제 Net gameplay 영역은 동일한 authoritative 시작점·끝점·두께·최대 길이와 배치 변환을 쓴다. 사용자가 Pixel Art·Preview·Contact VFX·Place SFX와 복어 중단의 어두운 상태·정상 복귀를 Play Mode에서 검증하고 Prototype 품질을 승인했다. 지속 Tick의 VFX/SFX 남발은 없고 Slow·Resistance damage·Tick·area·설치 수·비용은 변경하지 않았다. 길이·두께·animation 속도·VFX lifetime·SFX volume은 최종 출시 확정값이 아니다.

## VS-2D-1 신규 낚싯대 Prototype (2026-09-26, Unity 수동 검증 완료)

| 필드 | Idle/Attack 본체 | 대상 Hit | 적중음 |
|---|---|---|---|
| Asset ID | TOOL-FISHING-ROD-BODY-001 | VFX-FISHING-ROD-HIT-001 | SFX-FISHING-ROD-HIT-001 |
| File Path | `Assets/Art/Tools/FishingRod/FishingRod_Idle.png`, `FishingRod_Attack.png` | `Assets/Art/VFX/Tools/FishingRod_Hit.png` | `Assets/Audio/SFX/Tools/FishingRod_Hit.wav` |
| 규격 | 32×32 Idle, 96×32 Attack 3프레임, PPU 32, 0.27초 | 64×16, 16×16 셀×4프레임, PPU 83, 0.22초 | PCM WAV mono/44.1 kHz/16-bit/0.19초, 볼륨 0.28 |
| Production Method / Source | `Tools/generate_fishing_rod_presentation.py`에서 프로젝트 내부 픽셀 제작 | 같은 내부 스크립트의 Hook/Splash 픽셀 제작 | 같은 내부 스크립트의 감쇠 Reel/물방울음 합성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 |
| Unity Linked | `FishingRodPresentation.asset`와 Resources 런타임 참조; Setup 메뉴가 프리팹도 연결 | 같은 Profile의 4개 Hit Sprite, 공용 CombatVfxPool | 같은 Profile의 AudioClip, 공용 one-shot AudioSource |
| Automated Validation | PNG/메타데이터 정적 검사; Unity EditMode 전체 212/212 통과 | PNG/참조 정적 검사; Unity EditMode 전체 212/212 통과 | WAV 규격 정적 검사; Unity EditMode 전체 212/212 통과 |
| Manual Validation | Manual Visual Validation: Passed — Sprite·Attack Animation·Line 확인 | Manual Validation: Passed — 실제 대상 Hit VFX 확인 | Manual Audio Validation: Passed — 설치 후 SFX 확인 |
| Prototype Approval | Approved | Approved | Approved |
| Final Approval | Final Production Art Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |

사용자가 원본 Unity에서 compile 정상·Console Error 0·전체 EditMode 212/212 통과와 Play Mode 시각·청각 검증을 확인했다. Fishing Rod Sprite **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**. Attack Presentation·Line 및 Hit VFX 각각 **Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending**. SFX **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending**. Placement Preview **Manual Validation: Passed / Prototype Approval: Approved**이며 Q/W 동적 슬롯과 빠른 커서 이동 중 Ghost·Range 동위치 추적, 실제 gameplay range 일치를 확인했다. 본체 PPU 32는 기존 낚싯대 화면 footprint를 보존하는 첫 Tool 값이고 전체 Tool 표준이 아니다. Scene/Prefab YAML을 직접 수정하지 않았으며 Unity Editor Setup 메뉴가 프리팹 참조를 설정한다.

## VS-2C-3 신규 Prototype (2026-09-25, Unity 수동 검증 완료)

| 필드 | 복어 접촉 애니메이션 | 복어 그물 Impact | 복어 그물 중단음 |
|---|---|---|---|
| Asset ID | FISH-PUFFER-DISRUPT-ANIM-001 | VFX-PUFFER-NET-IMPACT-001 | SFX-PUFFER-NET-DISRUPT-001 |
| File Path | `Assets/Art/Fish/Pufferfish/Pufferfish_Disrupt.png` | `Assets/Art/VFX/Pufferfish/Pufferfish_NetImpact.png` | `Assets/Audio/SFX/SpecialFish/Pufferfish_NetDisrupt.wav` |
| 규격 | 192×192 RGBA, 48×48 셀 4방향축×4프레임, 12 FPS | 96×24 RGBA, 24×24 셀 4프레임, 0.28초 | PCM WAV mono/44.1 kHz/16-bit/0.22초, Profile 볼륨 0.34 |
| Production Method / Source | 승인된 `Pufferfish_Swim.png` 픽셀을 바탕으로 `Tools/generate_pufferfish_disruption.py`에서 순간 크기 반응 제작 | 같은 내부 스크립트의 픽셀 격자 접촉 burst | 같은 내부 스크립트의 감쇠 물방울음 합성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 |
| Unity Linked | `PufferfishDisruption.asset` 16개 Sprite 참조, Resources 로드 | 같은 Profile의 4개 Impact 참조, CombatVfxPool | 같은 Profile의 AudioClip 참조, 공용 one-shot AudioSource |
| Automated Validation | Unity 복사본 Setup/Validate 2회 통과, 전체 EditMode 200/200 통과 | 같은 Setup/Validate·EditMode 통과, 풀 반환 검사 | WAV 정적 규격 및 EditMode 중복 보호 검사 통과 |
| Manual Validation | Manual Visual Validation: Passed — 실제 복어 접촉, 특수 애니메이션, 현재 방향 Swim 복귀 확인 | Manual Visual Validation: Passed — 접촉점 표시, 중복 방지, Pause·Pool·Run 초기화 확인 | Manual Audio Validation: Passed — 성공음, 과도한 중첩 방지, Pause·Run 초기화 확인 |
| Prototype Approval | Approved | Approved | Approved |
| Final Approval | Final Production Art Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |

순간 접촉 연출은 실제 복어→활성 그물 중단 성공 후에만 발생한다. 기존 그물 어두워짐은 지속 상태 표시로 유지한다. Impact는 RepeatedHit 우선순위로 공용 상한 48을 따르며, 연출 누락이나 풀 포화가 게임플레이 판정을 지연시키지 않는다. 사용자 Unity compile 정상·Console Error 0·전체 EditMode 200/200 통과와 Play Mode 시각·청각 검증을 확인했다. 일반 어종에는 전용 연출이 없고 기존 Squid Ink Presentation·Fish animation·Resistance·포획·UI·VFX는 정상이다. 지속 팽창 gameplay는 없으며 Net 중단 시간·판정·감속·포획·Resistance 수치는 변경하지 않았다. 12 FPS·약 0.33초·Impact 0.28초·SFX 볼륨/중복 제한은 최종 출시 확정값이 아니다. 최종 제작 승인은 Pending이다.

## VS-2C-2 신규 Prototype (2026-09-24, Unity 수동 검증 완료)

| 필드 | Ink Projectile | Ink Impact |
|---|---|---|
| Asset ID | VFX-SQUID-INK-PROJECTILE-001 | VFX-SQUID-INK-IMPACT-001 |
| Display Name | 오징어 먹물 덩어리 | 오징어 먹물 타격 |
| File Path | `Assets/Art/VFX/Squid/Squid_InkProjectile.png` | `Assets/Art/VFX/Squid/Squid_InkImpact.png` |
| 규격 | 64×16 RGBA, 16×16 셀 4프레임, PPU 83 | 96×24 RGBA, 24×24 셀 4프레임, PPU 83 |
| Production Method / Source | `Tools/generate_squid_ink_projectile.py`로 프로젝트 내부 직접 제작 | 같은 스크립트로 프로젝트 내부 직접 제작 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 |
| Unity Linked | Sprite 분할 메타데이터와 `SquidInkPresentation.asset` 참조 작성; Play Mode 표시 확인 | Sprite 분할 메타데이터와 같은 Profile 참조 작성; Play Mode 표시 확인 |
| Automated Validation | PNG·메타데이터·참조 정적 검사 완료; Unity EditMode 전체 193/193 통과 | PNG·메타데이터·참조 정적 검사 완료; Unity EditMode 전체 193/193 통과 |
| Manual Validation | Manual Visual Validation: Passed — 실제 선택 대상 이동, 속도·가독성·다중 오징어 확인 | Manual Visual Validation: Passed — 대상 도착 시 표시, 지속 방해 상태·초기화 확인 |
| Prototype Approval | Approved | Approved |
| Final Approval | Final Production VFX Approval: Pending | Final Production VFX Approval: Pending |
| Notes | 현재 Prototype의 `CombatVfxPool` 상한 48·RepeatedHit, 0.22초 직선 이동; 타격 판정 없음 | 현재 Prototype의 도착 시 0.3초 표시; 기존 지속 방해 상태와 별개 |

두 시트는 기존 Ink Puff와 같은 검보라 팔레트와 Point/무압축/Full Rect/Clamp 설정을 사용한다. 기존 Release WAV를 그대로 사용하며 새 SFX는 없다. Unity compile 정상·Console Error 0·전체 EditMode 193/193 통과와 Play Mode의 Release → Projectile → Impact → 지속 방해 상태 표시를 확인했다. 기존 임시 연결선·즉시 target marker는 현재 구현에서 교체됐고, 대상 비활성·누락·Pause·Pool/Run 재시작에도 오류·잔상이 없었다. Gameplay 판정·타겟 선정·방해 지속시간은 변경하지 않았다. 현재 Projectile 이동시간·크기·Pool priority는 최종 출시 확정값이 아니다.

## VS-2C-1 신규 Prototype (2026-09-24)

| 필드 | 오징어 먹물 공격 Sprite | 오징어 먹물 Burst VFX | 오징어 먹물 SFX |
|---|---|---|---|
| Asset ID | FISH-SQUID-INK-ANIM-001 | VFX-SQUID-INK-BURST-001 | SFX-SQUID-INK-RELEASE-001 |
| Display Name | 오징어 먹물 공격 | 오징어 먹물 분출 구름 | 오징어 먹물 분사음 |
| Category | Special Fish Animation | Special Fish VFX | Special Fish SFX |
| File Path | `Assets/Art/Fish/Squid/Squid_InkAttack.png` | `Assets/Art/VFX/Squid/Squid_InkPuff.png` | `Assets/Audio/SFX/SpecialFish/Squid_InkRelease.wav` |
| Production Method | 기존 승인된 수영 시트를 바탕으로 64px 픽셀 그리드에서 몸통·눈을 보존하고 촉수/먹물만 직접 생성 | 32px 픽셀 그리드에 4단계 검보라 구름을 직접 생성 | Python에서 저역 노이즈·짧은 물방울 성분을 합성해 직접 생성 |
| Source | NETBREAK 프로젝트 내부 원본 수영 시트 | NETBREAK 프로젝트 내부 생성 | NETBREAK 프로젝트 내부 생성 |
| License | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 | 외부 소재 없음; 프로젝트 소유·배포 정책에 따름 |
| Unity Linked | Yes — Editor Setup/Validate가 Profile의 16개 공격 Sprite 연결 확인 | Yes — 같은 Profile의 puffFrames 4개 연결 확인 | Yes — 같은 Profile의 inkClip 연결 확인 |
| Automated Validation | PNG 256×256 RGBA, 4축×4셀·alpha 0/255 정적 규격 확인; Unity 메뉴 Validate 및 EditMode 전체 통과 | PNG 128×32 RGBA, 4셀·alpha 0/255 정적 규격 확인; Unity 메뉴 Validate 및 EditMode 전체 통과 | PCM WAV mono/44.1kHz/16-bit/0.28초 정적 규격 확인; Unity 메뉴 Validate 및 EditMode 전체 통과 |
| Manual Validation | Manual Visual Validation: Passed — E/N/NE/NW·반대 방향, 특수 프레임과 현재 방향 Swim 복귀 확인 | Manual Validation: Passed — 발동·Pool 반환, 영향 대상 없음, Pause·Run 재시작 확인 | Manual Audio Validation: Passed — 1회 재생, 다중 오징어 중복 보호, Pause·Run 재시작 확인 |
| Prototype Approval | Approved | Approved | Approved |
| Final Approval | Final Production Art Approval: Pending | Final Production VFX Approval: Pending | Final Production Audio Approval: Pending |
| Notes | 기존 `Squid_Swim.png`와 16프레임 Swim Profile은 변경하지 않음. Profile 경로 `Assets/Resources/SquidInkPresentation.asset` | `CombatVfxPool` 공용 상한 48, RepeatedHit 우선순위. 기존 방해 상태 UI와 별도 공격 원인 표시 | 첫 실제 프로젝트 내부 생성 Prototype SFX. 기본 Profile 볼륨 0.38, 전역 0.08초 재생 제한 |

위 세 에셋의 Unity 분할·연결, 전체 EditMode 190/190, 사용자 Play Mode 수동 검증을 확인했고 현재 Prototype 품질을 승인했다. 최종 출시용 Art/VFX/Audio 승인은 Pending이며 현재 볼륨·cooldown·loudness·Mixer 정책은 최종 확정값이 아니다. 기존 먹물 Gameplay 수치는 변경하지 않았다.

실제 도입한 에셋의 출처와 검증 상태를 기록한다. 외부 에셋은 도입 전에 원본 위치, 제작자, 라이선스 원문과 상업 이용·수정·재배포 조건을 별도로 확인한다. 프로토타입 승인은 최종 출시용 아트 승인을 뜻하지 않는다.

VS-2B-4 현재 파일은 아래의 16프레임 시트다. 이전 163/168/173 Test Runner 통과와 수동 승인 기록은 **12프레임 버전의 역사적 결과**다. 현재 다섯 시트의 픽셀·메타데이터 정적 검사 5/5와 사용자 Unity 컴파일·Console Error 0·전체 EditMode **183/183 통과** 및 Play Mode 수동 시각 검증을 완료했다. E/N/NE/NW 원본 4축×4프레임과 반대 방향 flipX+flipY가 현재 승인된 프로토타입 규칙이다. Final Production Art Approval은 다섯 어종 모두 Pending이며 PPU 83·8 FPS·셀 크기·Pixel Perfect Camera를 프로젝트 전체 최종 규격으로 확정하지 않는다.

| 항목 | 값 |
|---|---|
| Asset ID | FISH-SARDINE-SWIM-001 |
| 표시명 / 종류 | 정어리 헤엄 Sprite Sheet / 픽셀아트 |
| 파일 | `Assets/Art/Fish/Sardine/Sardine_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | 기존 `Tools/generate_sardine_sprite.py`의 도형·팔레트로 NW 4프레임을 추가. 기존 12프레임 픽셀 보존 |
| 라이선스 | 프로젝트 내부 제작물. 별도 외부 에셋 라이선스 없음; 소유·배포 권한은 프로젝트 정책에 따름 |
| 상업 이용 / 재배포 | 외부 소재 제한 없음. 프로젝트 소유·배포 정책은 별도 확인 |
| 시트 / 셀 | 128×128 RGBA / 32×32 |
| PPU | 83, 프로토타입 |
| 프레임 / 방향 | 16 / E·N·NE·NW 4원본 세트, 반대 방향은 flipX+flipY |
| Import | 16개 분할 메타데이터·기존 Sprite ID 보존 정적 확인. Unity 컴파일 및 Play Mode 표시 확인 |
| Unity 연결 | 정어리 FishData → `Sardine_VisualProfile.asset` 연결 완료 |
| 자동 검증 | 이전 버전 EditMode 163/163 통과. 현재 16프레임 정적 검사 통과, 사용자 확인 전체 EditMode 183/183 통과 |
| Manual Visual Validation / 수동 시각 검증 | Passed — VS-2B-4 방향·South 배색·4프레임 및 Console Error 0 확인 |
| Prototype Approval / 프로토타입 승인 | Approved — VS-2B-4 4방향축×4프레임 방향 규칙. 첫 정어리 기준 승인 기록도 유지 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending — Art Lock 아님 |
| 비고 | 정어리 한 종의 첫 파이프라인 프로토타입. 32×32 셀·PPU 83·8 FPS·Pixel Perfect를 전체 프로젝트 공통 최종 규격으로 확정하지 않음 |

| 항목 | 값 |
|---|---|
| Asset ID | FISH-MACKEREL-SWIM-001 |
| 표시명 / 종류 | 고등어 헤엄 Sprite Sheet / 픽셀아트 프로토타입 |
| 파일 | `Assets/Art/Fish/Mackerel/Mackerel_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | 기존 `Tools/generate_mackerel_tuna_sprites.py`로 NW 4프레임 추가. 기존 12프레임 픽셀 보존 |
| 라이선스 | 프로젝트 내부 제작물. 별도 외부 에셋 라이선스 없음; 소유·배포 권한은 프로젝트 정책에 따름 |
| 시트 / 셀 | 192×192 RGBA / 48×48 |
| PPU / FPS | 83 / 기본 8, 프로토타입 |
| 프레임 / 방향 | 16 / E·N·NE·NW 4원본 세트, 반대 방향은 flipX+flipY |
| Import / Unity 연결 | 16개 분할 메타데이터·프로필 참조 정적 확인. 고등어 FishData 연결 및 Unity Play Mode 표시 확인 |
| 자동 검증 | 이전 버전 EditMode 168/168·Pipeline Validate 통과. 현재 정적 검사 통과, 사용자 확인 전체 EditMode 183/183 통과 |
| Manual Visual Validation / 수동 시각 검증 | Passed — VS-2B-4 방향·South 배색·4프레임 및 Console Error 0 확인 |
| Prototype Approval / 프로토타입 승인 | Approved — VS-2B-4 4방향축×4프레임 방향 규칙 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending |

| 항목 | 값 |
|---|---|
| Asset ID | FISH-TUNA-SWIM-001 |
| 표시명 / 종류 | 참치 헤엄 Sprite Sheet / 픽셀아트 프로토타입 |
| 파일 | `Assets/Art/Fish/Tuna/Tuna_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | 기존 `Tools/generate_mackerel_tuna_sprites.py`로 NW 4프레임 추가. 기존 12프레임 픽셀 보존 |
| 라이선스 | 프로젝트 내부 제작물. 별도 외부 에셋 라이선스 없음; 소유·배포 권한은 프로젝트 정책에 따름 |
| 시트 / 셀 | 256×256 RGBA / 64×64 |
| PPU / FPS | 83 / 기본 8, 프로토타입 |
| 프레임 / 방향 | 16 / E·N·NE·NW 4원본 세트, 반대 방향은 flipX+flipY |
| Import / Unity 연결 | 16개 분할 메타데이터·프로필 참조 정적 확인. 참치 FishData 연결 및 Unity Play Mode 표시 확인 |
| 자동 검증 | 이전 버전 EditMode 168/168·Pipeline Validate 통과. 현재 정적 검사 통과, 사용자 확인 전체 EditMode 183/183 통과 |
| Manual Visual Validation / 수동 시각 검증 | Passed — VS-2B-4 방향·South 배색·4프레임 및 Console Error 0 확인 |
| Prototype Approval / 프로토타입 승인 | Approved — VS-2B-4 4방향축×4프레임 방향 규칙 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending |

| 항목 | 값 |
|---|---|
| Asset ID | FISH-PUFFERFISH-SWIM-001 |
| 표시명 / 종류 | 복어 방향별 수영 Sprite Sheet / 픽셀아트 프로토타입 |
| 파일 | `Assets/Art/Fish/Pufferfish/Pufferfish_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | 기존 `Tools/generate_pufferfish_squid_sprites.py`로 NW 4프레임 추가. 기존 12프레임 픽셀 보존 |
| 라이선스 / 상업 이용 | 별도 외부 소재 없음. 프로젝트 소유·배포 정책에 따름 |
| 시트 / 셀 | 192×192 RGBA / 48×48 |
| PPU / FPS | 83 / 기본 8, 프로토타입 |
| 프레임 / 방향 | 수영 16 / E·N·NE·NW 4원본 세트, 반대 방향은 flipX+flipY |
| Import / Unity 연결 | 16개 분할 메타데이터와 복어 FishData → `Pufferfish_VisualProfile.asset` 참조. 현재 Unity Play Mode 표시 확인 |
| 자동 검증 | 이전 버전 사용자 확인 EditMode 173/173 통과. 현재 정적 검사 통과, 사용자 확인 전체 EditMode 183/183 통과 |
| Manual Visual Validation / 수동 시각 검증 | Passed — VS-2B-4 방향·South 배색·flipX+flipY·4프레임·기존 기능 확인, Console Error 0 |
| Prototype Approval / 프로토타입 승인 | Approved — VS-2B-4 4방향축×4프레임 수영 프로토타입 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending |
| 비고 | 둥근 몸통 유지. 팽창·가시 강화·특수 상태 전용 애니메이션 없음 |

| 항목 | 값 |
|---|---|
| Asset ID | FISH-SQUID-SWIM-001 |
| 표시명 / 종류 | 오징어 방향별 수영 Sprite Sheet / 픽셀아트 프로토타입 |
| 파일 | `Assets/Art/Fish/Squid/Squid_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | 기존 `Tools/generate_pufferfish_squid_sprites.py`로 NW 4프레임 추가. 기존 12프레임 픽셀 보존 |
| 라이선스 / 상업 이용 | 별도 외부 소재 없음. 프로젝트 소유·배포 정책에 따름 |
| 시트 / 셀 | 256×256 RGBA / 64×64 |
| PPU / FPS | 83 / 기본 8, 프로토타입 |
| 프레임 / 방향 | 수영 16 / E·N·NE·NW 4원본 세트, 반대 방향은 flipX+flipY |
| Import / Unity 연결 | 16개 분할 메타데이터와 오징어 FishData → `Squid_VisualProfile.asset` 참조. 현재 Unity Play Mode 표시 확인 |
| 자동 검증 | 이전 버전 사용자 확인 EditMode 173/173 통과. 현재 정적 검사 통과, 사용자 확인 전체 EditMode 183/183 통과 |
| Manual Visual Validation / 수동 시각 검증 | Passed — VS-2B-4 방향·South 배색·flipX+flipY·4프레임·먹물 방해 Gameplay 확인, Console Error 0 |
| Prototype Approval / 프로토타입 승인 | Approved — VS-2B-4 4방향축×4프레임 수영 프로토타입 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending |
| 비고 | 이 Asset은 몸통·촉수 수영 리듬만 포함한다. 기존 먹물 방해 Gameplay는 유지하며 발사 전용 애니메이션·VFX/SFX는 위 VS-2C-1 별도 Asset으로 구현·검증했다 |
