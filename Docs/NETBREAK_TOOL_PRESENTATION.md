# NETBREAK Tool Presentation 원칙 — VS-2D-3

## 직접 사용형 Tool — Scoop Net Prototype

뜰채는 LMB 고정 즉발형 Tool이다. 설치형 Fishing Rod/Net의 Ghost·설치물 수명주기를 사용하지 않는다. Controller가 실제 입력·커서 좌표·원형 반경·쿨타임·대상 정렬·`TakeCaptureDamage`를 소유한다. 사용 가능한 동안만 기존 Range 원과 뜰채 Ready Sprite를 커서에 표시한다. Range 크기는 Controller의 실제 `captureRadius` 변경 경로를 따르며 UI/HUD에 별도 반경 상수를 두지 않는다.

한 번의 사용에서 모든 기본·연쇄 피해 판정이 끝난 뒤, 실제 Resistance가 감소한 Fish와 당시 위치의 목록을 Presentation에 전달한다. Presentation은 Fish를 다시 검색하지 않고 Swing 1회, 실제 피해 위치마다 작은 Hit VFX, 기본 SFX 1회를 요청한다. 0명일 때는 Swing과 사용음만 나타낸다. 공용 CombatVfxPool RepeatedHit/상한과 one-shot Source/중복 제한을 재사용한다. Swing 길이는 공격 쿨타임과 독립적이며 Animation Event에서 피해를 발생시키지 않는다. 선택 UI·Pause·Run 초기화에서 커서/Animation/VFX/Audio 상태를 정리한다. 이 규칙은 이후 Cast Net 등 즉발형 Tool의 최소 후보이며 해당 도구의 판정·입력은 구현할 때 별도로 조사한다. 사용자가 실제 hit Fish 목록과 VFX 대상 일치, Miss·다중 Hit·1 use = 1 기본 SFX, Ready→Swing→Ready, UI/Pause/Run 정리 및 기존 Tool Presentation 회귀를 Play Mode에서 확인했다. Damage·range·cooldown·max targets·input binding은 변경하지 않았다. **Scoop Net Manual Validation: Passed / Prototype Approval: Approved / Final Production Art·Animation·VFX·Audio Approval: Pending.**

낚싯대와 그물에 공통으로 확인된 최소 규칙이다. 각 도구의 배치와 공격 방식은 해당 Controller가 소유한다.

VS-2D-2는 사용자가 Unity compile 정상·Console Error 0·전체 EditMode 222/222 통과와 Play Mode 수동 검증을 확인했다. Net Pixel Art·Placement Preview·Contact VFX·Place SFX의 Prototype Approval은 Approved이고 최종 출시용 Art·VFX·Audio Approval은 Pending이다. Preview와 실제 gameplay 영역은 동일한 authoritative 배치 값을 사용한다. 복어 중단 시 어두운 상태와 Active 표시 중단, 약 3초 뒤 복귀, 다중 Net의 독립 상태를 확인했다. Slow·Resistance damage·Tick·area·설치 수·비용은 변경하지 않았다. 길이·두께·animation 속도·VFX lifetime·SFX volume은 최종 출시 확정값이 아니다.

- Q/W는 `ToolSlotInput.Read(ToolId)`와 Run 소유 슬롯으로 해석한다. 특정 키에 Tool 그림이나 배치 모드를 묶지 않는다.
- Preview는 해당 Tool의 기존 입력·취소·UI 차단 경로에서만 보인다. 실제 프리팹·Controller의 위치, 방향, 크기, 업그레이드 반영값을 읽고 Collider·피해·감속·오디오를 갖지 않는다. 확정·취소·Pause·도구 변경·Run 종료 시 제거한다.
- 게임플레이가 실제 판정과 비용을 먼저 적용한다. 그 후 Presentation hook이 Sprite/VFX/SFX를 요청한다. 자산 누락, 풀 포화, 오디오 중복 제한은 판정을 바꾸지 않는다.
- 반복 효과는 큰 VFX와 매 Tick SFX를 피한다. 낚싯대는 실제 피해 직후 Hit, 그물은 첫 접촉 등록 때 Contact와 설치 완료 때 Place SFX를 쓴다. VFX는 공용 `CombatVfxPool`의 기존 priority를 따르고 짧은 효과음은 재사용 one-shot Source를 쓴다.
- 작동·중단·이동·비활성 상태는 각 Tool의 실제 상태를 따른다. 일시 중단의 지속 표시는 순간 Hit 연출보다 우선한다. 시간 기반 연출은 scaled time과 공용 Run 정리를 따른다.
- 자산은 `Assets/Art/Tools/<Tool>/`, `Assets/Art/VFX/Tools/`, `Assets/Audio/SFX/Tools/`에 의미 있는 영어 이름으로 둔다. Point/무압축/Full Rect와 Tool별 실제 화면 크기에 맞춘 PPU를 Editor Setup/Validate에서 확인한다. 참조는 Editor API 또는 Resources Profile로 연결하고 Scene/Prefab YAML을 직접 고치지 않는다.
