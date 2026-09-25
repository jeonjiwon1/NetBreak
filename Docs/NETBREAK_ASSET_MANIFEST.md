# NETBREAK 아트·오디오 에셋 목록

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
