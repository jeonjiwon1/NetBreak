# Area 1 HUD Marine Refinement — 사용자 검증 대기

## 1. 구현 요약

프로젝트 상태/설계/관련 UI 코드와 실제 씬 값을 읽고 기존 미커밋 작업에서 이어서 진행했다. 기본 배치/크기/용도는 유지하며 39 PNG의 팔레트·픽셀 밀도·모서리를 재정리했다. 경험치 게이지는 기존 수치를 읽어 그리는 표현만 추가했다. 게임플레이·입력·드래그·인벤토리·성장 계산은 변경하지 않았다.

## 2. 수정 파일

Assets/Resources/UI/Area1/ 아래 PNG 39개:

- 프레임 6: panel, header, button, slot, selected_slot, key.
- 장식 11: decor_bobber, decor_clock, decor_coral, decor_crate, decor_gull, decor_leaf, decor_palm, decor_rope_knot, decor_shell, decor_starfish, decor_wave.
- 아이콘 22: icon_autonomous_sword_array, icon_bait, icon_capacitor_coil, icon_cast, icon_empty, icon_frost_crystal, icon_frost_sigil, icon_landing, icon_net, icon_rod, icon_signature, icon_spectral_scabbard, icon_speed, icon_stat_area, icon_stat_catch, icon_stat_exp, icon_stat_gold, icon_stat_level, icon_stat_rate, icon_stat_stage, icon_storm_orb, icon_tactical.

관련 파일:

- Assets/Scripts/UI/Area1HUDSkin.cs: 프레임 표시 두께, 간판, 게이지, 물고기/파도, 숫자 배지, 연결 기둥, 장식.
- Assets/Scripts/UI/PrototypeHUDCanvas.cs: 게이지 필드·생성·기존 EXP 값 표시 호출 3곳.
- Tools/refine_area1_hud_art.ps1: 이전 생성 이미지 팔레트/픽셀/모서리 패킹, 새 파도 소재 출력. 실행만으로 프로젝트 PNG를 덮어쓰지 않는다.
- NETBREAK_STATE.md, Docs/NETBREAK_ART_GUIDE.md, Docs/NETBREAK_ASSET_MANIFEST.md.
- Artifacts/MarineUIRefine/: 이전 39 PNG 백업, 새 후보/ZIP, 파도 원본·출처, 비교/정적 합성, 검증 자료, 본 보고서와 수동 체크리스트.

## 3. 영역별 개선

| 영역 | 적용 |
|---|---|
| 좌측 상단 | 패널 위 4px 돌출된 짙은 목재 간판, 양 끝 야자수/갈매기, EXP 숫자 밑 196×8 게이지 |
| 중앙 시간 | 공통 둥근 모서리와 얇은 목재 프레임, 단순화한 시계/조개 |
| 조업 준비 | 좌우 물고기 음영, 큰 로프, 8개 연속 파도 타일, 공통 Sand 버튼 |
| 우측 아이템 | 기존 4슬롯 유지, 16px 밝은 번호/어두운 배지, 번호·아이콘·문구 공간 분리 |
| 하단 Hotbar | 기존 716×120/5슬롯 유지, 슬롯 사이 목재 기둥, 확대 로프, 얇은 Cyan 선택 경계 |
| 우하단 버튼군 | 둥근 배속 배지/파도/조개, 성장 버튼의 목재·로프·잎 장식 |

## 4. 요청 디테일 반영

- 둥근 모서리: 기존 5px Slice 범위 안에 계단형 곡선과 닫힌 테두리 단면.
- 간판: 기존 NETBREAK 표시를 36px 높이의 짙은 목재 표지판으로 구성. 외부 패널 크기는 그대로.
- EXP: 숫자 아래 전용 공간. 기존 CurrentExp/ExpToNextLevel을 읽고 0~1로 제한해 표시하며, 분모가 0이면 비운다.
- 물고기: 기존 포획 아이콘을 낮은 불투명도의 청록 음영으로 재사용. 글자보다 뒤에 배치.
- 파도: 새 ImageGen 파도 소재를 기존 decor_wave.png에 패킹. 타일 양 끝 열을 맞추고 1px씩 겹쳐 연속 띠 구성.
- 번호: 1~4를 별도 배지와 밝은 글자로 구분. 슬롯 개수/번호 의미는 유지.
- 로프: 준비 패널 35→44, Hotbar 28→40 표시 크기. 성장 버튼에도 로프 연결.
- 픽셀 장식: 27색 공통 팔레트, 장식 48px 캔버스/24px 논리 그리드. 원본 ImageGen 실루엣을 단순화.

## 5. 정적 확인

- 39/39 PNG: 파일명·크기·RGBA8·CRC·압축 해제·Alpha 0/255 통과. 프레임/아이콘 32×32, 장식 48×48.
- 39/39 .meta 해시 보존. GUID, 5px 프레임 Slice/0px 아이콘·장식 Slice, Point, 무MipMap, 무압축 유지.
- 6/6 프레임: 중앙 불투명 단색, 늘어나는 가장자리 일정 단면 확인. 곡선은 고정 모서리 안에 위치.
- 세션 시작 baseline과 비교한 변경은 PNG 39 + UI 코드 2 + 문서 3 = 기존 파일 44개. Scene/Prefab/ProjectSettings/URP/기타 코드 변경 없음. 새 재패킹 스크립트와 관련 산출물은 별도 추가.
- EXP 텍스트 행 하단 127, 게이지 129~137, 구분선 139~141: 좌표상 겹치지 않는다. 아이템 번호 4~24, 아이콘 25~57, 문구 57~75: 별도 세로 구역이다. 새 장식/게이지는 raycastTarget=false. Hotbar 기둥은 ignoreLayout=true.
- 실제 TMP 글리프의 한글 잘림·긴 숫자·텍스트 오버플로·해상도별 9-slice·컴파일·상호작용은 미검증이다. Truncate/마스크만으로 한글 잘림이 없다고 보장하지 않는다. HUD_STATIC_PREVIEW.png는 정적 근사 합성이다.

## 6. 문서 반영

요청한 3문서 최상단에 이번 후보·변경 범위·규격·출처·표현 연결·검증 대기를 기록했다. 과거 구현 이력은 보존했다. TOOL_PRESENTATION 문서는 도구 의미가 바뀌지 않아 수정하지 않았다.

## 7. 사용자 검증 순서

UNITY_CHECKLIST.md의 순서대로 Import/Console → 기본 배치 → 정보/EXP/드래그 → 준비/시간 → 아이템 → Hotbar → 배속/성장 → 해상도별 잘림 → 최종 레퍼런스 비교를 확인한다.

## 8. git diff --check

종료 코드 0, 공백 오류 없음. Git의 LF→CRLF 안내는 출력되지만 공백 오류는 아니다. 새 텍스트 파일은 별도 공백 검사로 확인한다.

## 9. git status

vertical-slice 유지. 전체 결과는 git-status-after.txt에 저장한다. Core/Gear/ItemHUD 등의 기존 변경은 이번 작업 이전부터 존재했고 세션 시작 해시를 유지했다. 새 Tools/refine_area1_hud_art.ps1 및 Artifacts/MarineUIRefine 산출물이 추가됐다. UI 폴더와 Area1HUDSkin.cs는 이전 작업부터 untracked 상태이므로 일반 git diff만으로 이번 PNG 변경 전체가 표시되지는 않는다. baseline/검증 JSON으로 비교했다.

## 10. 실행하지 않은 작업

Git add/commit/push, Unity 실행·Import·컴파일·Play Mode·Console 확인·수동 게임 검증, 임시 Unity 프로젝트 복사본 생성을 하지 않았다. 실제 게임 검증과 최종 아트 승인은 사용자 확인 대기다.
