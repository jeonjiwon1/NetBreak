# NETBREAK Tool Presentation 원칙 — VS-2D-2

낚싯대와 그물에 공통으로 확인된 최소 규칙이다. 각 도구의 배치와 공격 방식은 해당 Controller가 소유한다.

VS-2D-2는 사용자가 Unity compile 정상·Console Error 0·전체 EditMode 222/222 통과와 Play Mode 수동 검증을 확인했다. Net Pixel Art·Placement Preview·Contact VFX·Place SFX의 Prototype Approval은 Approved이고 최종 출시용 Art·VFX·Audio Approval은 Pending이다. Preview와 실제 gameplay 영역은 동일한 authoritative 배치 값을 사용한다. 복어 중단 시 어두운 상태와 Active 표시 중단, 약 3초 뒤 복귀, 다중 Net의 독립 상태를 확인했다. Slow·Resistance damage·Tick·area·설치 수·비용은 변경하지 않았다. 길이·두께·animation 속도·VFX lifetime·SFX volume은 최종 출시 확정값이 아니다.

- Q/W는 `ToolSlotInput.Read(ToolId)`와 Run 소유 슬롯으로 해석한다. 특정 키에 Tool 그림이나 배치 모드를 묶지 않는다.
- Preview는 해당 Tool의 기존 입력·취소·UI 차단 경로에서만 보인다. 실제 프리팹·Controller의 위치, 방향, 크기, 업그레이드 반영값을 읽고 Collider·피해·감속·오디오를 갖지 않는다. 확정·취소·Pause·도구 변경·Run 종료 시 제거한다.
- 게임플레이가 실제 판정과 비용을 먼저 적용한다. 그 후 Presentation hook이 Sprite/VFX/SFX를 요청한다. 자산 누락, 풀 포화, 오디오 중복 제한은 판정을 바꾸지 않는다.
- 반복 효과는 큰 VFX와 매 Tick SFX를 피한다. 낚싯대는 실제 피해 직후 Hit, 그물은 첫 접촉 등록 때 Contact와 설치 완료 때 Place SFX를 쓴다. VFX는 공용 `CombatVfxPool`의 기존 priority를 따르고 짧은 효과음은 재사용 one-shot Source를 쓴다.
- 작동·중단·이동·비활성 상태는 각 Tool의 실제 상태를 따른다. 일시 중단의 지속 표시는 순간 Hit 연출보다 우선한다. 시간 기반 연출은 scaled time과 공용 Run 정리를 따른다.
- 자산은 `Assets/Art/Tools/<Tool>/`, `Assets/Art/VFX/Tools/`, `Assets/Audio/SFX/Tools/`에 의미 있는 영어 이름으로 둔다. Point/무압축/Full Rect와 Tool별 실제 화면 크기에 맞춘 PPU를 Editor Setup/Validate에서 확인한다. 참조는 Editor API 또는 Resources Profile로 연결하고 Scene/Prefab YAML을 직접 고치지 않는다.
