# NETBREAK Area 1 Marine UI 최종 후보 보고
작성: 2026-09-28. 39 PNG 프로젝트 교체 및 정적 검증 완료. Unity 시각·기능 검증과 승인은 Pending.

## 1. 현재 39개 Asset Audit
첨부 current_ui_39.zip과 시작 프로젝트의 39 PNG는 파일명과 SHA-256이 전부 일치했다. 32×32 28개(프레임 6·아이콘 22), 48×48 11개(장식)다. 파일별 경로·역할·호출·GUID·Border·장식 구분·ZIP 비교는 [ASSET_AUDIT.md](ASSET_AUDIT.md), 기계 판독 자료는 [asset-audit-before.json](asset-audit-before.json)에 기록했다.

## 2. 실제 사용 / Decoration / 미사용
39개 모두 정적 코드 참조가 있다. 프레임은 Area1HUDSkin.SetFrame과 PrototypeHUDCanvas가, 아이콘은 ToolIcon/SkillIcon/ItemIcon/StyleRunPanel/StyleSpeedButtons가, 장식 11종은 Decorate가 사용한다. 미참조 0개, 삭제 0개. 아이템 6종은 보유 상태에서 표시되는 조건부 참조이며 정상 Area 1은 계속 아이템을 지급하지 않는다. 정적 참조 확인을 실제 Play 표시 검증으로 간주하지 않았다.

## 3. 목표 Reference 디자인 분석
어두운 바닷빛 정보판에 입체적인 따뜻한 목재·로프 경계, Cyan 선택 강조, 모래색 버튼, 열대 식물·갈매기·산호·조개·포말이 결합된 디자인을 기준으로 삼았다. 정보 HUD와 Ready의 장식 밀도는 높고 Inventory/Hotbar/Growth는 중간, Timer/Speed/Key는 낮게 유지한다. Reference의 큰 UI 면적을 복제하지 않고 현재 크기·좌표를 보존했다. Sprite Sheet를 순서대로 절단하거나 임의 의미 매핑하지 않았다. 목표와 충분히 가까운지의 최종 판단은 사용자 검증 대기다.

## 4. 좌측 HUD 디자인
공유 panel을 따뜻한 목재/로프 모서리와 Dark Teal 내부로 재제작했다. 팔메트 형태였던 야자수·잎을 식물 실루엣으로, 갈매기를 흰 몸통/날개/부리로, 조개와 불가사리를 입체적 형태로 교체했다. 제목 바는 짧은 높이에서도 TMP가 어두운 바탕 위에 놓이도록 테두리 소재를 얇게 패킹했다. 정보·라벨·값 영역과 Drag는 그대로다.

## 5. Timer 디자인
목재 프레임과 불투명 Dark Teal 내부, 작은 해양 시계·조개를 사용한다. 시계는 밝은 테두리·바늘과 어두운 다이얼로 구분한다. 시간 TMP와 위치는 그대로다.

## 6. Ready/Start 디자인
야자수·빨강/흰색 찌·로프 매듭·산호·포말 파도를 재제작했다. Start 버튼은 따뜻한 모래색 면과 목재/황금빛 테두리를 사용한다. 기존 장식 좌표·크기·수량과 타이틀/버튼 RectTransform을 보존했다.

## 7. Inventory 디자인
4개 슬롯 구조를 유지하고 목재/로프 외곽과 Cyan 내부 선, 어두운 아이콘 공간을 결합했다. 상자·잎·조개·불가사리도 기존 위치에서 새 아트로 바뀐다. 슬롯 번호, 빈 상태, 아이템 갱신, Tooltip, Drag 로직은 변경하지 않았다.

## 8. Hotbar 디자인
공통 슬롯의 목재 경계가 연결된 세트처럼 보이도록 하고 LMB는 별도의 밝은 Aqua/Cyan 경계로 구분했다. 양 끝의 잎·산호·조개·로프를 재제작했다. 기존 LMB/Q/W/E/R 순서와 Q/W 동적 선택, E/R 잠금·보상·쿨다운을 유지한다.

## 9. Speed/Growth 디자인
Speed는 얇은 밝은 Cyan/아이보리 테두리, 어두운 내부와 두 겹의 파도 아이콘을 사용한다. Growth는 모래색 버튼과 잎/조개를 재사용한다. 배속 선택 Tint와 버튼 기능·좌표·크기는 그대로다. Key/Speed는 공유 Sprite이므로 작은 키 글자 영역 확보를 우선했다.

## 10. Decoration Asset 활용
이전 작업의 장식 11개를 모두 새로 제작해 기존 연결에 사용했다. 장식은 48×48을 유지하고 프레임의 Stretch 영역에 합치지 않았다. 별도 Image의 기존 preserveAspect/raycastTarget=false 및 위치를 유지한다. 신규 장식을 무분별하게 추가하지 않았다.

## 11. Icon 디자인
22개를 각각의 의미로 제작했다. 뜰채는 손잡이/타원 망, 설치 그물은 두 기둥 사이 직사각형 망, 투망은 원형 전개 망, 낚싯대는 대/릴/줄/바늘, 미끼는 찌다. E는 보상 병, R은 보물상자다. 전기 구체/코일, 검집/세 자루 검, 눈꽃 문장/얼음 결정으로 아이템을 구분했다. 정보는 동전/물고기/계기/별/성장 그래프/낚싯바늘/지도 위치 핀이다. PNG에 UI 문자열을 굽지 않았다.

제작 경로는 **내장 ImageGen**이며 CLI/API 우회는 사용하지 않았다. 개별 생성 프롬프트 전문과 원본 파일 경로는 [generation-manifest.json](generation-manifest.json)에 있다. 생성물은 원래 해상도로 최근접 샘플링하고 투명도를 0/255로 정리했다. 기존 Tools/generate_area1_hud_art.js는 이번 후보를 재현하는 생성기가 아니며 수정/실행하지 않았다.

## 12. 9-slice 보호 방식
6종은 기존 Border L/B/R/T=5px를 그대로 유지했다. 생성된 고정 모서리/가장자리 단면을 사용하되 중앙 22×22는 불투명 단색, 늘어나는 네 가장자리는 일정한 단면으로 패킹했다. 최초 생성 프레임의 중앙 투명 결손을 적용 전에 제거했다. 짧은 header/key는 기존 TMP와 겹치던 밝은 경계를 얇게 패킹하고 나머지 Slice 내부를 어두운 여백으로 확보했다. 6종 모두 중앙·가장자리 픽셀 검사를 통과했다. 야자수/불가사리/조개 등은 늘어나는 영역에 없으며 Border/.meta는 변경하지 않았다.

## 13. 수정한 Asset 전체 목록
모든 경로는 `Assets/Resources/UI/Area1/` 아래다.

프레임 6개: `button.png`, `header.png`, `key.png`, `panel.png`, `selected_slot.png`, `slot.png`

장식 11개: `decor_bobber.png`, `decor_clock.png`, `decor_coral.png`, `decor_crate.png`, `decor_gull.png`, `decor_leaf.png`, `decor_palm.png`, `decor_rope_knot.png`, `decor_shell.png`, `decor_starfish.png`, `decor_wave.png`

아이콘 22개: `icon_autonomous_sword_array.png`, `icon_bait.png`, `icon_capacitor_coil.png`, `icon_cast.png`, `icon_empty.png`, `icon_frost_crystal.png`, `icon_frost_sigil.png`, `icon_landing.png`, `icon_net.png`, `icon_rod.png`, `icon_signature.png`, `icon_spectral_scabbard.png`, `icon_speed.png`, `icon_stat_area.png`, `icon_stat_catch.png`, `icon_stat_exp.png`, `icon_stat_gold.png`, `icon_stat_level.png`, `icon_stat_rate.png`, `icon_stat_stage.png`, `icon_storm_orb.png`, `icon_tactical.png`

## 14. 신규 Asset
신규 Unity PNG/메타데이터 0개. Artifacts/MarineUI의 비교 시트와 정적 합성 PNG는 검토용이며 Unity Asset이 아니다.

## 15. .meta/GUID 보존
39개 .png.meta의 SHA-256이 시작 시점과 전부 같다. GUID, Sprite Border, PPU, Point Filter, Mipmap Off, 무압축, Max Size를 포함한 전체 메타데이터가 바이트 단위로 보존됐다. 이름/경로/Resources 호출도 그대로다.

## 16. 결과 ZIP
[NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip](NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip)

절대 경로: `C:\game_dev\unity\NetBreak\Artifacts\MarineUI\NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip`

ZIP 루트에 개별 PNG만 있으며 프로젝트 적용본과 39개 모두 해시가 같다. PNG 이외의 보고서·미리보기·생성 원본은 ZIP에 넣지 않았다.

## 17. 최종 PNG 총 개수
**39개 = 32×32 프레임 6개 + 32×32 아이콘 22개 + 48×48 장식 11개.** 원본에서 내용이 바뀐 파일도 39개다.

## 18. Static Validation
[static-validation.json](static-validation.json): PASS.

PNG Signature/청크 CRC/IEND/압축 데이터와 필터 복원, RGBA8, 원래 크기, 투명/불투명 Alpha, 파일명/개수/경로, 39개 변경 해시, 메타데이터 보존, 6개 9-slice 구조를 확인했다. 시작 파일 해시와 비교한 변경은 39 PNG와 상태 문서 3개뿐이다. UI·Gameplay C#·Scene·Prefab·ProjectSettings·기타 기존 Assets/Tools는 변하지 않았다. 새 결과·검증 자료는 Artifacts/MarineUI에 있다.

[PNG_BEFORE_AFTER.png](PNG_BEFORE_AFTER.png)는 원본 대비 시트다. [HUD_STATIC_PREVIEW.png](HUD_STATIC_PREVIEW.png)는 현재 런타임 좌표와 Canvas PPU로 만든 **정적 합성 근사 이미지**다. 실제 Unity 캡처가 아니며 TMP 글꼴 렌더링, 클리핑, 배속 Tint, Canvas/런타임 순서와 상호작용을 완전히 재현하지 않는다. 시야 방해·겹침·가독성·기능 회귀는 Unity에서 확인해야 한다.

## 19. git diff --check
최종 결과는 [git-diff-check.txt](git-diff-check.txt)에 기록한다. 기존 작업의 LF→CRLF 예고는 줄바꿈 경고이며 공백 오류와 구분한다. Git diff만으로 미추적 PNG를 비교할 수 없으므로 이번 변경 범위는 별도 시작 해시 검사로 확인했다.

## 20. 사용자가 직접 할 Unity 검증 순서
전체 33개 항목은 [UNITY_CHECKLIST.md](UNITY_CHECKLIST.md)에 있다.

1. Unity 실행 → Import/compile 완료 → Console Error 0 → Main Scene Play.
2. 목표 Reference 근접성, 단순 사각형 느낌 개선, Marine/Tropical Theme, 전체 세트 통일감.
3. 좌측 HUD 장식 → 정보 가독성 → Fish Spawn 가시성 → Drag.
4. Timer → Ready의 Palm/Bobber/Wave → Start 버튼 → Text 가독성.
5. Inventory 4슬롯 → Corner Decoration → 보유 상태 Tooltip → Drag.
6. Hotbar 키 → Tool Icon/Q/W 동적 갱신 → Selected Glow → End Decoration → gameplay 시야.
7. x1/x2/x3 → Growth 버튼/Tab.
8. Tool Input/UI 입력 차단 → Placement → Targeting → Pause → Run restart.
9. 최종 Console Error 0.

## 21. Pending 상태
Manual Visual Validation: **Pending**. Prototype Approval: **Pending**. Final Production UI Approval: **Pending**.

이번 작업에서 Computer Use, Unity 실행/Import 확인/컴파일/Console/PlayMode/Test Runner/Batch, 임시 프로젝트 복사, Git add/commit/push는 수행하지 않았다. 목표 Reference의 품질에 충분히 도달했다고 승인 기록을 하지 않았다.

## 22. git status
브랜치 `vertical-slice`, origin `https://github.com/jeonjiwon1/NetBreak.git`. 기존 미커밋 C# 8개와 미추적 UI 자산/스크립트/생성기를 보존했다. 세션 고유 변경은 39 PNG 교체, 상태 문서 3개에 최소 기록 추가, Artifacts/MarineUI 결과물이다. 최종 전체 목록은 [git-status-after.txt](git-status-after.txt)에 있다. 스테이징·커밋·푸시는 하지 않았다.

