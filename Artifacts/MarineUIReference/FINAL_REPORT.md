# Reference Frame Rebuild

## 적용

첨부 이미지를 기준으로 창 배경과 로프를 재제작했다. 단색 Stretch가 창을 매끈한 사각형처럼 보이게 하는 문제를 해결하기 위해 원화뿐 아니라 UI Image 표시를 Tiled로 바꿨다. 새로운 기능/게임플레이 변경은 없다.

- 좌측 HUD: 목재 재질과 간판의 감긴 로프, 픽셀 모서리/어두운 정보 행.
- 중앙 시간: 공통 목재·청록 소재를 작은 단독 바에 적용.
- 중앙 준비: 낮은 대비의 픽셀 내부와 기존 물고기 음영, 여러 가닥 로프, 양 끝이 말린 새 포말 띠.
- 우측 아이템: 목재 외곽과 가닥이 보이는 로프, 청록 슬롯 경계.
- 좌하단 LMB/중앙 Hotbar: Cyan 선택 프레임과 나뭇결, 상하 접합부의 교차 로프.
- 성장/배속: Navy 내부·밝은 글자, 선택 배속 Cyan 강조.

## 파일

- Assets/Resources/UI/Area1/: panel.png, header.png, button.png, slot.png, selected_slot.png, key.png (64×64), decor_rope_knot.png (48×48), decor_wave.png (256×48).
- 프레임 6개 .meta: 최대 크기 64, PPU 100, Border 12px. 파도 .meta: 최대 크기 256. 39 GUID 모두 보존, 다른 Import 항목 유지.
- Assets/Scripts/UI/Area1HUDSkin.cs: Tiled 표시와 장식/색/프레임 연결만 변경.
- Tools/pack_area1_reference_frames.ps1: 보관 원본에서 최근접 샘플링, 계단형 색조, 반복 이음새 패킹.
- NETBREAK_STATE.md, Docs/NETBREAK_ART_GUIDE.md, Docs/NETBREAK_ASSET_MANIFEST.md: 최신 후보와 확인 대기 기록.
- Artifacts/MarineUIReference/: 생성 프롬프트 8종, 원본, 이전 백업, 적용 자산 ZIP, 정적 합성, 검증 JSON, 체크리스트.

## 정적 확인과 한계

39개 PNG의 CRC/압축 해제/RGBA8/Alpha 0·255/규격, GUID 보존, Import 허용 변경 필드를 검사한다. 프레임 6개의 중앙은 불투명하고 2색 이상의 무늬를 유지하며, 반복 영역 양 끝 픽셀은 일치한다. 나머지 31 PNG, 기존 코드(Area1HUDSkin 제외), Scene/Prefab/ProjectSettings는 시작 해시로 보존을 확인한다.

기본 UI 크기/Anchor/위치, 키·텍스트 의미·슬롯 수·기존 EXP 표시·드래그·툴팁·게임 로직은 유지한다. 새 장식은 raycastTarget=false, Hotbar 장식은 ignoreLayout=true다. 제작에는 내장 ImageGen을 사용했고 CLI/API 대체는 사용하지 않았다.

HUD_STATIC_PREVIEW.png는 근사 정적 합성으로 실제 TMP/Unity 화면이 아니다. 실제 한글 잘림·텍스트 오버플로·배경 반복 빈도·모서리 정합성·컴파일·입력은 사용자 검증 대기다. 기존 크기와 TMP 폰트를 유지했으므로 첨부 이미지와 픽셀 단위로 동일하다고 판정한 결과는 아니다. 디자인 동일성의 목표와 승인 완료를 구분한다.

git diff --check 결과와 전체 상태는 git-diff-check.txt/git-status-after.txt에 기록한다. 기존 미커밋 변경을 보존했고 add/commit/push는 하지 않았다. Unity 실행/임시 프로젝트/컴파일/Play/Console 확인도 하지 않았다.

사용자 확인 순서는 UNITY_CHECKLIST.md를 따른다.
