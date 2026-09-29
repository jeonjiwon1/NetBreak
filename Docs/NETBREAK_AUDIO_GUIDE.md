# NETBREAK 오디오 가이드

## Area 1 Prototype BGM (2026-09-29, Unity 청취 완료·Prototype 승인)

Area 1에는 프로젝트 내부 생성 Original Loop 세 곡을 연결했다. `Assets/Audio/BGM/Area1/`의 Normal(100 BPM, 153.60초)은 밝은 플럭·벨과 가벼운 리듬, MiniBoss(120 BPM, 24.00초)는 거대 참치 돌진에 맞춘 빠른 펄스, Boss(140 BPM, 약 82.29초)는 상어의 최종 Encounter를 위한 낮은 음역과 강한 리듬이다. 세 곡 모두 22.05 kHz/16-bit/mono WAV이며 Unity Import는 Streaming/Vorbis quality 0.8이다. 현재 WAV는 Gameplay 흐름과 음악적 위계 확인용 Prototype이며 최종판으로 확정하지 않았다.

`PrototypeGameFlowManager`가 `Area1BgmController`를 한 번 부착한다. Controller의 전용 2D loop AudioSource와 `Resources/Area1BgmProfile.asset`의 세 Clip, 공통 Source 볼륨 0.22, 0.5초 fade를 사용한다. 기존 `ItemEffectManager` SFX one-shot Source와 Mixer 없이 독립적이다. Gameplay 시작 Normal → MiniBoss 생성 MiniBoss → 포획 후 Normal → Boss Encounter 시작 Boss → Result/Restart Stop이다. MiniBoss 실패도 Stop, Boss 회유 1/2/3 동안 같은 곡을 유지한다. 동일 Track 재요청은 재시작하지 않는다. Fade는 unscaled time이고 pitch 1을 유지해 x1/x2/x3과 무관하다. 기존 성장/선택 `timeScale=0`은 BGM Pause로 취급하지 않는다. 사용자가 Unity에서 전환·수명·배속 독립, Tool/Squid/Puffer와 Giant Tuna/Shark Warning·Charge SFX의 가독성, 관련 Console Error/Exception 없음을 확인했다. 음악 작곡·음색 완성도에는 아쉬움이 있어 Composition·Arrangement·Instrumentation·Mix·Mastering은 Production 단계에서 재검토할 수 있다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play) / Static Validation: Passed (기존 Codex 정적 검사 범위) / EditMode Tests: Not Run / Unity Manual Audio Validation: Passed / Prototype BGM Approval: Approved / Final Production Approval: Pending.**

## Area 1 MiniBoss / Boss Charge Warning·Start Prototype (2026-09-29, Unity 수동 청각 검증 완료·Prototype 승인)

사용자 청취 결과 기존 “퉁” 효과음은 Charge Start에 더 자연스럽다. 따라서 기존 별도 파일 `GiantTuna_ChargeTelegraph.wav`(0.26초)와 `SharkBoss_ChargeTelegraph.wav`(0.36초)를 보존하고 실제 돌진 시작으로 옮겼다. 이름의 `Telegraph`는 초기 제작 명칭이며 현재 재생 역할은 Charge Start다. 기존 볼륨 0.44/0.50을 유지한다.

새 `GiantTuna_ChargeWarning.wav`(0.34초, Profile 볼륨 0.42)는 시작에서 끝으로 상승하는 가벼운 압력·Whoosh이고, `SharkBoss_ChargeWarning.wav`(0.40초, 0.48)는 더 낮고 무거운 상승형 경고다. 둘 다 Telegraph 길이보다 짧으며 Impact Accent를 넣지 않았다. 네 파일은 프로젝트 내부 합성 PCM mono/44.1 kHz/16-bit다. 새 Warning 생성기는 `Tools/generate_charge_warning_sfx.ps1`이다.

이벤트 계약은 **Telegraph Start → Warning 1회 → 점멸/감속 → 실제 Charge Start → 기존 Charge 1회**다. 점멸 중 반복하지 않고 Shark Phase 2/3은 같은 쌍을 사용한다. `FishVisualProfile`의 두 역할을 `ItemEffectManager` 공용 one-shot Source로 재생하며 Pause·준비·Run 종료 제한과 Source Pause/UnPause/Stop을 공유한다. 사용자가 Unity Area 1 Play Mode에서 Giant Tuna의 예고감과 두 역할의 구분, Shark의 더 무거운 경고 위계, Phase 2/3·다음 Attempt, 각 이벤트 1회와 기존 Tool/Squid/Puffer 소리와의 조화를 확인했다. Visual과 Audio Timing도 자연스럽고 볼륨·강도에 Prototype 품질 문제가 없었다. **Implementation Complete: Complete / Source Compile: Passed (사용자 Unity Play 실행) / Static Validation: Passed (Codex의 WAV·GUID·트리거·diff 검사 범위) / EditMode Tests: Not Run (최종 코드) / Unity Manual Audio Validation: Passed / Prototype Audio Approval: Approved / Final Production Approval: Pending.** Capture/Escape/Result/Reward/Phase Change/BGM, Element/Synergy, Mixer/Bus와 최종 loudness·mastering은 이번 승인에서 제외한다.

## VS-2D-4 Cast Net Open Prototype — Unity 수동 청각 검증 완료

`Assets/Audio/SFX/Tools/CastNet_Open.wav`는 프로젝트 내부 합성 PCM mono/44.1 kHz/16-bit/0.34초의 로프·망·물소리다. 실제 cast 완료 뒤 공용 one-shot Source에서 재생한다. **1 cast = 기본 SFX 최대 1회**이며 다중 명중 Fish마다 반복하지 않는다. 같은 소리의 0.09초 중복 제한은 판정에 영향을 주지 않는다. 사용자가 Unity Play Mode에서 Hit/Miss의 사용음, 다중 명중 시 기본음 1회, 취소·UI 차단·Pause 중 불필요한 재생 없음과 Run 재시작 후 잔상 없음을 확인하고 현재 Prototype 품질을 승인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.** Profile 볼륨 0.3과 최종 믹스는 출시용 확정값이 아니다.

## VS-2D-3 Scoop Net Swing Prototype — Unity 수동 청각 검증 완료

`Assets/Audio/SFX/Tools/ScoopNet_Swing.wav`는 프로젝트 내부 합성 PCM mono/44.1 kHz/16-bit/0.24초의 가벼운 물 휘두름 소리다. 직접 반복 사용 도구의 기본 SFX는 **1 use = 1회**로 연결하고, 다중 Fish를 맞춰도 대상마다 반복하지 않는다. Hit과 Miss 모두 Swing을 나타내며 공용 one-shot Source와 0.09초 동일음 중복 제한을 사용한다. 사용자가 Play Mode에서 Miss/Hit 사용음, 다중 명중 시 기본음 1회, 쿨타임·UI 차단 중 불필요한 재생 없음과 Run 재시작 후 잔상 없음을 확인했다. Profile 볼륨 0.27은 Prototype 값이며 최종 출시 확정값이 아니다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.**

## VS-2D-2 Net Place Prototype — Unity 수동 검증 완료

`Assets/Audio/SFX/Tools/Net_Place.wav`는 프로젝트 내부에서 합성한 44.1 kHz/16-bit/mono/0.29초의 짧은 로프·물소리다. 실제 Net 설치 완료 후 공용 one-shot Source에서 재생하며 같은 소리는 0.09초 중복 제한을 둔다. 사용자가 Play Mode에서 설치 완료 시 1회 재생과 취소 시 무음을 확인했다. 지속 Slow·Resistance 피해의 물리 Tick에는 소리를 내지 않으며 접촉 VFX도 매 Tick 반복하지 않는다. Profile 기본 볼륨 0.32는 Prototype 값이며 최종 출시 확정값이 아니다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.**

## VS-2D-1 낚싯대 Prototype

`Assets/Audio/SFX/Tools/FishingRod_Hit.wav`는 프로젝트 내부에서 합성한 44.1 kHz/16-bit/mono/0.19초의 짧은 Reel/Click/물방울 적중음이다. 실제 Resistance 감소 후 공용 one-shot AudioSource로 재생한다. Profile 볼륨 0.28과 전역 동일음 0.09초 제한은 반복 타격·여러 낚싯대의 청각 피로를 줄이는 프로토타입 값이다. 소리 제한은 공격 판정에 영향을 주지 않는다. 기존 AudioSource Pause·Run 초기화 정책을 공유한다. 사용자가 Play Mode에서 설치 후 적중음과 기존 연출 흐름을 확인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.**

## 1. 목적

SFX와 BGM을 게임플레이 원인, 화면 정보, 밝은 픽셀아트 톤에 맞춰 제작하고 기록한다. 이 문서는 제작 기준이며 최종 믹스 규격은 아니다.

## 2. 현재 Audio 상태

VS-2C-1의 `Squid_InkRelease.wav`가 첫 프로젝트 내부 생성 Prototype SFX다. VS-2C-3의 `Pufferfish_NetDisrupt.wav`는 두 번째 Fish/Special Prototype이다. 기존 공용 Audio Manager나 Mixer 정책은 없다. 두 효과는 `ItemEffectManager`의 재사용 one-shot `AudioSource`를 사용한다.

## 3. 전체 Sound Direction

짧고 둥글며 가벼운 물속 질감을 우선한다. 정보가 필요한 행동은 음색과 리듬으로 구분하고, 다수 물고기·도구가 겹쳐도 HUD와 전투 판단을 방해하지 않도록 한다.

## 4. Tool SFX

도구별 사용 시작·적중·설치 완료를 구분한다. Q/W 도구 소리는 키가 아니라 현재 선택된 도구의 행동에 연결한다. 현재 낚싯대 적중음 1종만 프로토타입으로 제작했다.

## 5. Fish / Capture SFX

물고기 종류보다 포획 성공, 대형 어획, 미포획 도착 등 결과의 차이를 먼저 설계한다. 포획은 Resistance 0의 결과이며 사망음으로 표현하지 않는다. 아직 제작하지 않았다.

## 6. Element SFX

전기·검·얼음은 각각 짧은 질감 차이로 구분한다. 반복 타격은 동시 음량 제한이 필요하다. 아직 제작하지 않았다.

## 7. Special Fish SFX

오징어 먹물은 물기 섞인 낮은 분사음 한 번이다. 공격 대상 선택 뒤 애니메이션의 Release 프레임에 재생한다. 실제 도구 중단은 Controller가 Projectile 이동 시간 뒤 유효성을 재확인해 적용하며, Impact Presentation 실패에도 대체 경로로 보장한다. 후속 Squid Timing Fix에서 사용자가 기존 SFX와 Impact 시점 도구 중단의 자연스러운 연결을 확인했다. **Timing Fix Unity Manual Validation: Passed / Timing Fix Prototype Approval: Approved / Final Production Approval: Pending.** 복어 그물 중단은 다른 음색의 짧고 둔탁한 물방울형 접촉음이며 실제 중단 성공 직후 재생한다.

## 8. Boss SFX

현재 Area 1에서는 Giant Tuna와 Shark Boss의 돌진 예고 Warning과 실제 Charge Start를 각각 분리했다. 회유 전환, 포획/마지막 도주와 최종 믹스는 후속 단계다.

## 9. UI SFX

선택, 취소, 획득, 오류의 의미를 짧게 전달한다. 반복 클릭의 과도한 중첩을 피한다. 아직 제작하지 않았다.

## 10. BGM 방향

밝은 해역과 성장하는 판타지 어부의 분위기를 유지한다. 위 Area 1 세 곡과 전환은 Vertical Slice Prototype으로 승인됐으며 최종 음악 제작과 믹스는 대기 중이다.

## 11. File Format

오징어는 PCM WAV, mono, 44.1 kHz, 16 bit, 0.28초이고 복어는 같은 규격의 0.22초다. 외부 파일을 내려받지 않았으며 코드로 생성했다. 향후 파일 형식·압축은 대상 플랫폼과 실제 메모리/음질을 보고 결정한다.

## 12. Volume / Priority

효과별 볼륨은 Profile/Inspector에서 조절한다. 오징어 현재 Prototype 기본값은 `0.38`, 복어는 `0.34`이며 최종 볼륨은 아니다. 핵심 결과음이 잦은 반복음보다 잘 들리도록 설계하되 최종 loudness, 버스, 믹서 우선순위는 미정이다.

## 13. 동시 재생

같은 오징어 먹물 SFX는 현재 Prototype에서 전역 scaled time 기준 0.08초 안의 추가 재생을 생략한다. 복어 접촉음은 전역 scaled time 기준 0.1초 안의 추가 재생만 생략한다. 여러 특수어의 게임플레이 판정과 VFX에는 영향을 주지 않는다. 사용자가 오징어와 복어의 동일음 중복 보호를 수동 확인했다. 최종 cooldown 정책과 다른 효과의 동시 정책은 실제 밀집 화면에서 결정한다.

## 14. Pool / One-shot

짧은 효과는 재사용 `AudioSource.PlayOneShot`을 사용한다. 특수어별 AudioSource나 매 공격 Instantiate/Destroy는 사용하지 않는다. Time.timeScale 0에서 Source를 Pause하고 재개 시 UnPause하며 Run 종료/재시작 때 Stop한다.

## 15. Naming

`<대상>_<행동>.wav` 형식으로 분명한 영어 파일명을 사용한다. 현재 이름은 `Squid_InkRelease.wav`, `Pufferfish_NetDisrupt.wav`다.

## 16. Folder

특수어 SFX는 `Assets/Audio/SFX/SpecialFish/`, 현재 MiniBoss/Boss의 두 단계 돌진 SFX는 `Assets/Audio/SFX/Boss/`에 둔다. Area 1 BGM은 `Assets/Audio/BGM/Area1/`에 둔다. UI SFX 폴더는 실제 자산이 생길 때 추가한다.

## 17. Asset Manifest

모든 도입 소리는 `NETBREAK_ASSET_MANIFEST.md`에 경로, 출처, 제작 방식, 검증과 승인 상태를 기록한다. Prototype과 Final 상태를 별도 관리한다.

## 18. 외부 Asset 라이선스

외부 소재 사용 시 원본 URL, 제작자, 라이선스 원문, 상업 이용·수정·재배포 조건을 도입 전에 확인한다. 확인되지 않은 권리를 임의로 적지 않는다.

## 19. Prototype / Final

오징어 먹물은 첫 실제 Prototype SFX다. 사용자가 Unity에서 1회 재생, 다중 오징어 중복 보호, Pause·Run 재시작 처리를 확인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.** 최종 loudness·Mixer와 출시용 오디오 품질은 확정하지 않았다.

복어 그물 중단음은 두 번째 Fish/Special Prototype SFX다. 활성 그물의 실제 중단 성공 직후에만 재생되며 동일 소리의 전역 0.1초 중복 보호가 있다. 사용자가 Play Mode에서 재생, 과도한 중첩 방지, Pause와 Run 재시작 후 정상 초기화를 확인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.** 현재 볼륨 0.34와 cooldown은 Prototype 값이며 최종 loudness·Mixer 정책은 미정이다.

## 20. 미정

최종 loudness, Audio Mixer/버스, 전체 우선순위, 공간화, 다른 효과별 동시 상한, 플랫폼 압축, BGM의 출시용 구조, 접근성 옵션은 아직 확정하지 않았다.
