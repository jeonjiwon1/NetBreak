# NETBREAK 아트·오디오 에셋 목록

실제 도입한 에셋의 출처와 검증 상태를 기록한다. 외부 에셋은 도입 전에 원본 위치, 제작자, 라이선스 원문과 상업 이용·수정·재배포 조건을 별도로 확인한다. 프로토타입 승인은 최종 출시용 아트 승인을 뜻하지 않는다.

| 항목 | 값 |
|---|---|
| Asset ID | FISH-SARDINE-SWIM-001 |
| 표시명 / 종류 | 정어리 헤엄 Sprite Sheet / 픽셀아트 |
| 파일 | `Assets/Art/Fish/Sardine/Sardine_Swim.png` |
| 출처 / 제작자 | 이 NETBREAK 작업에서 프로젝트 내부 생성 |
| 제작 방식 | Pillow를 이용해 32픽셀 격자에 직접 도형·픽셀을 그린 `Tools/generate_sardine_sprite.py` |
| 라이선스 | 프로젝트 내부 제작물. 별도 외부 에셋 라이선스 없음; 소유·배포 권한은 프로젝트 정책에 따름 |
| 상업 이용 / 재배포 | 외부 소재 제한 없음. 프로젝트 소유·배포 정책은 별도 확인 |
| 시트 / 셀 | 128×96 RGBA / 32×32 |
| PPU | 83, 프로토타입 |
| 프레임 / 방향 | 12 / 3 원본 세트, flip으로 8방향 |
| Import | Unity 6000.3.11f1에서 12개 분할·Point·83 PPU·무압축·Full Rect·Clamp 확인 |
| Unity 연결 | 정어리 FishData → `Sardine_VisualProfile.asset` 연결 완료 |
| 자동 검증 | 전체 EditMode 163/163 통과. `NETBREAK_STATE.md` 참조 |
| Manual Visual Validation / 수동 시각 검증 | Passed — 사용자 Unity 확인, Console Error 0 |
| Prototype Approval / 프로토타입 승인 | Approved — 후속 일반 어종 제작의 기준으로 사용 가능 |
| Final Production Art Approval / 최종 출시용 아트 승인 | Pending — Art Lock 아님 |
| 비고 | 정어리 한 종의 첫 파이프라인 프로토타입. 32×32 셀·PPU 83·8 FPS·Pixel Perfect를 전체 프로젝트 공통 최종 규격으로 확정하지 않음 |
