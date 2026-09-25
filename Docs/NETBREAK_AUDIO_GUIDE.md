# NETBREAK 오디오 가이드

## 1. 목적

SFX와 BGM을 게임플레이 원인, 화면 정보, 밝은 픽셀아트 톤에 맞춰 제작하고 기록한다. 이 문서는 제작 기준이며 최종 믹스 규격은 아니다.

## 2. 현재 Audio 상태

VS-2C-1의 `Squid_InkRelease.wav`가 첫 프로젝트 내부 생성 Prototype SFX다. VS-2C-3의 `Pufferfish_NetDisrupt.wav`는 두 번째 Fish/Special Prototype이다. 기존 공용 Audio Manager나 Mixer 정책은 없다. 두 효과는 `ItemEffectManager`의 재사용 one-shot `AudioSource`를 사용한다.

## 3. 전체 Sound Direction

짧고 둥글며 가벼운 물속 질감을 우선한다. 정보가 필요한 행동은 음색과 리듬으로 구분하고, 다수 물고기·도구가 겹쳐도 HUD와 전투 판단을 방해하지 않도록 한다.

## 4. Tool SFX

도구별 사용 시작·적중·설치 완료를 구분한다. Q/W 도구 소리는 키가 아니라 현재 선택된 도구의 행동에 연결한다. 아직 제작하지 않았다.

## 5. Fish / Capture SFX

물고기 종류보다 포획 성공, 대형 어획, 미포획 도착 등 결과의 차이를 먼저 설계한다. 포획은 Resistance 0의 결과이며 사망음으로 표현하지 않는다. 아직 제작하지 않았다.

## 6. Element SFX

전기·검·얼음은 각각 짧은 질감 차이로 구분한다. 반복 타격은 동시 음량 제한이 필요하다. 아직 제작하지 않았다.

## 7. Special Fish SFX

오징어 먹물은 물기 섞인 낮은 분사음 한 번이다. 성공한 방해 판정 뒤 공격 애니메이션의 Release 프레임에 재생한다. 복어 그물 중단은 다른 음색의 짧고 둔탁한 물방울형 접촉음이며 실제 중단 성공 직후 재생한다.

## 8. Boss SFX

경고, 돌진, 회유 전환, 포획/마지막 도주를 서로 구분한다. 구현·믹스는 후속 단계다.

## 9. UI SFX

선택, 취소, 획득, 오류의 의미를 짧게 전달한다. 반복 클릭의 과도한 중첩을 피한다. 아직 제작하지 않았다.

## 10. BGM 방향

밝은 해역과 성장하는 판타지 어부의 분위기를 유지한다. 일반 조업과 보스 긴장감의 전환 방식, 루프 길이와 곡 수는 미정이다. 이번 단계에 BGM은 없다.

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

특수어 SFX는 `Assets/Audio/SFX/SpecialFish/`에 둔다. 후속 Tool, UI, Boss, BGM 폴더는 실제 자산이 생길 때 추가한다.

## 17. Asset Manifest

모든 도입 소리는 `NETBREAK_ASSET_MANIFEST.md`에 경로, 출처, 제작 방식, 검증과 승인 상태를 기록한다. Prototype과 Final 상태를 별도 관리한다.

## 18. 외부 Asset 라이선스

외부 소재 사용 시 원본 URL, 제작자, 라이선스 원문, 상업 이용·수정·재배포 조건을 도입 전에 확인한다. 확인되지 않은 권리를 임의로 적지 않는다.

## 19. Prototype / Final

오징어 먹물은 첫 실제 Prototype SFX다. 사용자가 Unity에서 1회 재생, 다중 오징어 중복 보호, Pause·Run 재시작 처리를 확인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.** 최종 loudness·Mixer와 출시용 오디오 품질은 확정하지 않았다.

복어 그물 중단음은 두 번째 Fish/Special Prototype SFX다. 활성 그물의 실제 중단 성공 직후에만 재생되며 동일 소리의 전역 0.1초 중복 보호가 있다. 사용자가 Play Mode에서 재생, 과도한 중첩 방지, Pause와 Run 재시작 후 정상 초기화를 확인했다. **Manual Audio Validation: Passed / Prototype Approval: Approved / Final Production Audio Approval: Pending.** 현재 볼륨 0.34와 cooldown은 Prototype 값이며 최종 loudness·Mixer 정책은 미정이다.

## 20. 미정

최종 loudness, Audio Mixer/버스, 전체 우선순위, 공간화, 다른 효과별 동시 상한, 플랫폼 압축, BGM 구조, 접근성 옵션은 아직 확정하지 않았다.
