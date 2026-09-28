# Area 1 해양 픽셀아트 UI 패스

현재 Game View의 배치·대략적인 점유 크기를 유지하고, 첨부 목표 이미지와 참고 스프라이트 시트의 간판 실루엣·목재/로프·해양 장식을 실제 UI 전용 PNG로 재구성했다. `HUD_STATIC_PREVIEW.png`는 정적 배치 근사 이미지이며 Unity 캡처가 아니다.

이번 추가 수정에서 다시 그린 전용 PNG는 `ref_ready_board`, `ref_wave_strip`, `ref_hud_sign`, `ref_inventory_board`, `ref_item_slot`, `ref_number_badge`, `ref_hotbar_board`, `ref_hotbar_slot`, `ref_hotbar_selected`, `ref_rope_connector`, `ref_speed_stack`, `ref_speed_button`, `ref_speed_selected` 13개다. 생성기, 정적 미리보기, `Area1HUDSkin.cs`, 본 문서와 `NETBREAK_STATE.md`도 갱신했다.

## 자산 구조와 연결

`Assets/Resources/UI/Area1/`에 기존 39 PNG/39 `.meta`를 그대로 두고, 전용 `ref_*.png` 26개와 각 `.meta`를 추가했다. `Area1HUDSkin.cs`가 기존 Resources 로드 경로에서 전용 보드·버튼·슬롯을 연결한다. 기존 도구/아이템/통계 아이콘과 기능 상태 연결은 유지한다. 새 PNG는 Sprite/Point/무압축 설정과 고유 GUID를 가진다.

| UI 영역 | 전용 PNG와 역할 |
|---|---|
| 좌측 HUD | `ref_hud_board` 본체와 야자수·갈매기·하단 조개/불가사리, `ref_hud_sign` 상단 프레임에 얹힌 NETBREAK 목재 간판, `ref_exp_track`/`ref_exp_fill` 게이지 |
| 시간 | `ref_time_board` 짧은 목재 시간판, `ref_decor_clock` 시계 |
| 조업 준비 | `ref_ready_board` 야자수·찌·물고기 무늬 본체, `ref_start_button` 밝은 목재 버튼, `ref_wave_strip` 암석·로프·이끼 하단 프레임을 덮는 324×28 연속 파도 |
| 아이템 | `ref_inventory_board` 장식형 보드, `ref_inventory_sign` 제목/잎/상자, `ref_item_slot` 4칸, `ref_number_badge` 번호 |
| 핫바 | `ref_hotbar_board` 긴 프레임, `ref_hotbar_slot`/`ref_hotbar_selected` 5칸, `ref_rope_connector` 다중 명암 꼬임 |
| 배속 | `ref_speed_stack` 세로 연결부, `ref_speed_button`/`ref_speed_selected` 3개 버튼 |
| 성장 | `ref_growth_board` 목재 간판형 버튼 |
| 보조 장식 | `ref_decor_starfish`, `ref_decor_shell`, `ref_decor_coral`, `ref_decor_leaf`, `ref_decor_rope_knot` |

`Tools/generate_area1_reference_boards.py`는 `Artifacts/MarineUIBoards/Sources/TropicalMarineUISheet.png`의 명시한 HUD/준비판/목재 버튼 부분을 역할에 맞게 자르고 프레임으로 재구성한다. 원본 시트의 순서대로 임의로 39개에 매핑하지 않았다. `Tools/preview_area1_reference_boards.py`는 배치 근사 미리보기를 만든다. 재생성 시 기존 `.meta`가 있으면 덮어쓰지 않아 GUID가 유지된다.

## 영역별 결과

- **HUD:** NETBREAK 간판을 166×46에서 174×44로 소폭 넓히고 아래로 7px 내려, 짧은 양측 지지부가 본체 윗목재와 겹치게 했다. 야자수는 좌측 프레임을 타고, 갈매기는 간판 오른쪽 위에만 있다. 7개 정보 항목과 가로 EXP 게이지를 유지한다.
- **시간·조업 준비:** 같은 목재/해양 톤을 공유한다. 준비판에는 야자수·찌·희미한 물고기 무늬와 분리된 시작 버튼이 있다. 본체 PNG의 이전 일자 로프와 양끝 파도를 제거했다. `ref_wave_strip` 하나에 해양 암석·짧게 꼬인 로프·이끼 하단 프레임과 좌우로 이어지는 파도 한 줄을 합치고, 본체 하단에 밀착시켰다. 별도로 떠 있는 두 번째 파도는 없다.
- **아이템:** 위쪽 간판에 잎·상자가 붙는다. 보드와 슬롯의 외곽에 작은 암석·이끼 픽셀을 더하면서 4개 번호 배지와 상태 글자의 내부 여백을 유지했다.
- **핫바:** 장식형 끝단·연결 로프·Cyan 선택 강조를 유지하고 보드, 슬롯, 연결부에 같은 암석·이끼 명암을 넣었다. 키 캡·아이콘·상태 글자의 영역은 유지했다.
- **배속·성장:** 세로 연결부와 버튼 외곽에 같은 재질 픽셀을 더했다. 성장 버튼의 기존 목재판과 장식, 텍스트 영역은 유지했다.

이번 추가 수정의 코드 변경은 `Assets/Scripts/UI/Area1HUDSkin.cs`의 준비판 하단 장식 위치와 간판 크기·위치뿐이다. `PrototypeHUDCanvas.cs`의 기존 표시 연결을 포함하여 Scene/Prefab 직렬화, 게임플레이, 입력, 드래그, 툴팁, 아이템/성장/배속 상태 로직은 변경하지 않았다. 기존 PNG 이름·`.meta`·GUID와 이전 사용자 변경을 보존했다.

## 직접 확인할 항목

- [ ] Unity에서 26개 새 Sprite의 import와 Console을 확인한다.
- [ ] 실제 Game View에서 HUD/아이템 시야 점유, NETBREAK 오른쪽 갈매기, 준비판 하단 파도 위치를 확인한다.
- [ ] 해상도별 한글·숫자·키 캡·아이템 번호·EXP 게이지의 잘림과 장식 겹침을 확인한다.
- [ ] 조업 시작, 정보판/아이템 드래그, 아이템 툴팁, 성장 관리, 배속 x1/x2/x3, 뜰채/Q/W/E/R 상태 표시를 확인한다.
- [ ] EXP·아이템·도구·E/R 상태 변화 뒤 표시 갱신을 확인한다.

정적 미리보기와 PNG/참조 검사는 완료했다. Unity 실행·컴파일·Play·수동 확인, 임시 복사 프로젝트 생성, Git add/commit/push는 수행하지 않았다. 레퍼런스와의 실제 일치감과 화면별 읽기 편함은 사용자 Unity 확인 대기다.
