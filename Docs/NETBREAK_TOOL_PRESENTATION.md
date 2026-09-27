# NETBREAK Tool Presentation 원칙 — VS-2D-4

## 조준형 일회성 범위 Tool — Cast Net Prototype

투망 Q/W는 슬롯 키 누름에 조준, 놓음에 확정한다. 기존 우클릭·Esc 취소를 사용한다. 전술 E 긴급 투망은 별도 스킬 상태에서 같은 Controller에 확대 반경을 전달한다. 조준 Preview의 중심·원은 Controller의 실제 커서 월드 위치와 `captureRadius`를 사용한다. 타겟 유효성 제한이 없는 현재 Tool에 임의의 invalid 상태는 추가하지 않는다. 조준은 순수 시각 표시이며 피해·충전 소모·아이템 발동을 일으키지 않는다.

확정 시 게임플레이가 먼저 원형 판정과 Resistance 감소를 완료하고, 실제 감소한 Fish 위치 목록을 Presentation에 전달한다. Presentation은 Fish를 검색하지 않는다. Hit/Miss 모두 목표 위치에서 원형 망 전개와 Area 물결, 사용음 1회를 표시하고 Hit일 때만 대상별 작은 접촉을 표시한다. `CombatVfxPool`의 RepeatedHit 우선순위·상한, 공용 one-shot AudioSource·중복 제한을 재사용한다. Animation Event는 판정에 관여하지 않는다. Profile/Pool/Audio가 없어도 투망 판정은 계속된다. 취소·Pause·선택 UI·슬롯 변경·Run 종료에서 조준을 정리하며 Run 초기화는 공용 VFX/Audio를 정리한다. 사용자가 Unity Play Mode에서 Q/W 동적 슬롯과 범위 업그레이드, Hit/Miss·다중 명중, 1 cast = 1 기본 SFX, 취소·입력 차단·Run 재시작, 기존 E/R·Item/Synergy·낚싯대/Net/뜰채 회귀를 확인했다. **Cast Net Manual Validation: Passed / Prototype Approval: Approved / Final Production Art·Animation·VFX·Audio Approval: Pending.**

현재 Play Mode에서 확인한 도구 연출 유형은 Fishing Rod의 설치형 단일 대상 공격, Net의 드래그 설치형 지속 범위, Scoop Net의 커서 즉발형 다중 명중, Cast Net의 조준 후 확정하는 일회성 범위 다중 명중이다. 각 도구의 입력·판정·수명주기는 해당 Controller가 관리한다.

Unity 수동 확인 순서:

1. Unity의 Import가 끝나면 Console 컴파일 오류를 확인하고 `NETBREAK/Art/Setup Cast Net Presentation` → `NETBREAK/Art/Validate Cast Net Presentation`을 실행한다. 전체 EditMode Test Runner의 통과/실패/Skip 수를 기록한다.
2. 새 Run에서 Lv2 또는 Lv3 선택으로 투망을 Q 또는 W에 장착한다. 해당 키를 누른 채 커서를 움직이며 작은 Ghost·원형 범위가 함께 따라가는지, 실제 반경 업그레이드와 일치하는지 확인한다. 충전 소진 중 조준이 시작되지 않는지 본다.
3. 키를 놓아 빈 곳에 사용한다. 망 전개·물결·기본 사용음은 한 번, Fish 접촉 효과는 0개여야 한다. 한 마리 및 여러 마리에게 사용해 실제 Resistance 감소 대상만 작은 접촉이 있는지, 대상 수만큼 소리가 반복되지 않는지 확인한다.
4. 조준 중 우클릭과 Esc를 각각 시험한다. Ghost·범위가 사라지고 피해·충전·쿨다운·VFX·SFX가 없어야 한다. E 비상 투망은 E 누름/뗌과 취소를 시험하고 확대 반경·일반 충전 보존을 확인한다.
5. 도구 슬롯 변경, 선택 UI/성장 UI, 일시정지에서 조준·입력 차단과 새 판정/소리 없음, 일시정지 중 기존 VFX·오디오의 멈춤/재개를 확인한다. Run 종료·재시작에서 조준·망·물결·오디오 잔상이 없는지 본다. 기존 R 천망·아이템/시너지 발동 횟수와 낚싯대·설치 Net·뜰채 연출도 확인한다.

## 직접 사용형 Tool — Scoop Net Prototype

뜰채는 LMB 고정 즉발형 Tool이다. 설치형 Fishing Rod/Net의 Ghost·설치물 수명주기를 사용하지 않는다. Controller가 실제 입력·커서 좌표·원형 반경·쿨타임·대상 정렬·`TakeCaptureDamage`를 소유한다. 사용 가능한 동안만 기존 Range 원과 뜰채 Ready Sprite를 커서에 표시한다. Range 크기는 Controller의 실제 `captureRadius` 변경 경로를 따르며 UI/HUD에 별도 반경 상수를 두지 않는다.

한 번의 사용에서 모든 기본·연쇄 피해 판정이 끝난 뒤, 실제 Resistance가 감소한 Fish와 당시 위치의 목록을 Presentation에 전달한다. Presentation은 Fish를 다시 검색하지 않고 Swing 1회, 실제 피해 위치마다 작은 Hit VFX, 기본 SFX 1회를 요청한다. 0명일 때는 Swing과 사용음만 나타낸다. 공용 CombatVfxPool RepeatedHit/상한과 one-shot Source/중복 제한을 재사용한다. Swing 길이는 공격 쿨타임과 독립적이며 Animation Event에서 피해를 발생시키지 않는다. 선택 UI·Pause·Run 초기화에서 커서/Animation/VFX/Audio 상태를 정리한다. 이 단락은 뜰채의 직접 사용형 패턴이고 투망의 Q/W 조준형 패턴은 위 절을 따른다. 사용자가 실제 hit Fish 목록과 VFX 대상 일치, Miss·다중 Hit·1 use = 1 기본 SFX, Ready→Swing→Ready, UI/Pause/Run 정리 및 기존 Tool Presentation 회귀를 Play Mode에서 확인했다. Damage·range·cooldown·max targets·input binding은 변경하지 않았다. **Scoop Net Manual Validation: Passed / Prototype Approval: Approved / Final Production Art·Animation·VFX·Audio Approval: Pending.**

낚싯대와 그물에 공통으로 확인된 최소 규칙이다. 각 도구의 배치와 공격 방식은 해당 Controller가 소유한다.

VS-2D-2는 사용자가 Unity compile 정상·Console Error 0·전체 EditMode 222/222 통과와 Play Mode 수동 검증을 확인했다. Net Pixel Art·Placement Preview·Contact VFX·Place SFX의 Prototype Approval은 Approved이고 최종 출시용 Art·VFX·Audio Approval은 Pending이다. Preview와 실제 gameplay 영역은 동일한 authoritative 배치 값을 사용한다. 복어 중단 시 어두운 상태와 Active 표시 중단, 약 3초 뒤 복귀, 다중 Net의 독립 상태를 확인했다. Slow·Resistance damage·Tick·area·설치 수·비용은 변경하지 않았다. 길이·두께·animation 속도·VFX lifetime·SFX volume은 최종 출시 확정값이 아니다.

- Q/W는 `ToolSlotInput.Read(ToolId)`와 Run 소유 슬롯으로 해석한다. 특정 키에 Tool 그림이나 배치 모드를 묶지 않는다.
- Preview는 해당 Tool의 기존 입력·취소·UI 차단 경로에서만 보인다. 실제 프리팹·Controller의 위치, 방향, 크기, 업그레이드 반영값을 읽고 Collider·피해·감속·오디오를 갖지 않는다. 확정·취소·Pause·도구 변경·Run 종료 시 제거한다.
- 게임플레이가 실제 판정과 비용을 먼저 적용한다. 그 후 Presentation hook이 Sprite/VFX/SFX를 요청한다. 자산 누락, 풀 포화, 오디오 중복 제한은 판정을 바꾸지 않는다.
- 반복 효과는 큰 VFX와 매 Tick SFX를 피한다. 낚싯대는 실제 피해 직후 Hit, 그물은 첫 접촉 등록 때 Contact와 설치 완료 때 Place SFX를 쓴다. VFX는 공용 `CombatVfxPool`의 기존 priority를 따르고 짧은 효과음은 재사용 one-shot Source를 쓴다.
- 작동·중단·이동·비활성 상태는 각 Tool의 실제 상태를 따른다. 일시 중단의 지속 표시는 순간 Hit 연출보다 우선한다. 시간 기반 연출은 scaled time과 공용 Run 정리를 따른다.
- 자산은 `Assets/Art/Tools/<Tool>/`, `Assets/Art/VFX/Tools/`, `Assets/Audio/SFX/Tools/`에 의미 있는 영어 이름으로 둔다. Point/무압축/Full Rect와 Tool별 실제 화면 크기에 맞춘 PPU를 Editor Setup/Validate에서 확인한다. 참조는 Editor API 또는 Resources Profile로 연결하고 Scene/Prefab YAML을 직접 고치지 않는다.
