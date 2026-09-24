# NETBREAK 아트·오디오 에셋 목록

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
| 비고 | 몸통·촉수 수영 리듬만 포함. 기존 먹물 방해 Gameplay 유지; 발사 전용 애니메이션·VFX/SFX 미구현 |
