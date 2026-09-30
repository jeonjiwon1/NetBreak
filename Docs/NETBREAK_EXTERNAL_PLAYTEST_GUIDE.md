# NETBREAK Area 1 외부 플레이테스트

## 플레이테스터에게 전달할 안내

- 폴더 전체를 압축 해제한 뒤 `NETBREAK.exe`를 실행해 처음부터 자유롭게 플레이해주세요.
- 막히거나 이해되지 않아도 먼저 스스로 진행해보세요. 도중에 궁금했던 점을 기억해두면 좋습니다.
- 플레이가 끝나면 별도 피드백 질문에 답해주세요.
- 버그가 생기면 가능할 때 화면과 발생 직전 상황을 기록해주세요.

## 개발자용: 배포와 첫 실행

- 권장 폴더: `Builds/ExternalPlaytest/NETBREAK_Area1_VS_RC1/`. `NETBREAK.exe`, `NETBREAK_Data/`, `UnityPlayer.dll`과 Unity가 생성한 다른 파일을 **폴더째** `NETBREAK_Area1_VS_RC1.zip`으로 압축한다. 실행 파일만 전달하면 실행되지 않는다.
- Windows x86_64 일반 빌드를 사용한다. Development Build, Script Debugging, Autoconnect Profiler, Deep Profiling은 모두 끈다. 빌드에는 `Main` 씬 하나만 포함한다.
- 새 PC의 첫 실행에는 Tutorial 완료 기록이 없다. 기존 설치 이력이 있는 PC에서는 완료 기록이 남을 수 있다. Editor 검증에는 `Tools > NETBREAK > Reset Area 1 Tutorial Progress`를 사용한다. **Editor Reset은 Standalone의 저장 기록을 초기화하지 않는다.**
- Standalone 검증의 Fresh Start는 Windows 레지스트리 `HKEY_CURRENT_USER\Software\DefaultCompany\NetBreak`에서 `NetBreak.Area1Tutorial.v1.{Goal,CoreTool,PartnerTool,TacticalSkill,BossRule}` 다섯 PlayerPrefs 키만 삭제한 뒤 실행한다. Unity가 키 이름을 변환해 저장할 수 있으므로 실제 레지스트리 항목을 확인한다. 처음 실행하는 별도 Windows 사용자 계정에서도 검증할 수 있다. 전체 제품 키 삭제는 다른 설정도 지우므로 권장하지 않는다.
- 현재 런타임에서 별도 save 파일과 `Application.persistentDataPath` 사용은 확인되지 않았다. Windows Player 로그는 `%USERPROFILE%\AppData\LocalLow\DefaultCompany\NetBreak\Player.log`에서 확인한다. 제품/회사 이름이 바뀌면 경로도 달라진다.
- 외부 배포 전 개발자가 실제 Standalone에서 첫 실행부터 성공 Result까지 한 번, 별도 Run에서 실패 Result와 Restart를 확인한다. 검증 전에는 외부 전달 승인으로 표시하지 않는다.

## 개발자용: 관찰 원칙과 항목

플레이 중에는 설명하거나 힌트를 주지 않는다. 진행 불가·기술 장애 시에만 개입하고 그 시점을 기록한다. 개인 식별 정보는 수집하지 않는다.

- 첫 행동까지 걸린 시간과 기본 목표를 이해한 순간
- LMB 안내 이해 여부, Q/W 선택 뒤 실제 사용까지 걸린 시간
- Growth 선택 망설임과 TAB 발견 여부
- 복어 효과 및 오징어 방해 원인 이해 여부
- 거대 참치 대응과 E 사용 여부
- 상어 3회 회유 규칙 및 실패 원인 이해 여부
- 물고기 진행·탈출 방향 이해 여부
- UI 가독성, BGM/SFX의 과소·과다 또는 누락
- 막힘·실패·지루함이 발생한 구간과 도구 사용 편중

## 개발자용: 세션 기록

- Tester ID(익명):
- Date / Build:
- First-time player: Yes / No
- Run Duration / Attempts / Final Level:
- Core Tool / Partner Tool / E:
- Catch Rate / Remaining Gold:
- MiniBoss Result / Boss Result / Run Result:
- 첫 혼란 / 첫 재미 포인트:
- 실패 지점 / 사용하지 않은 시스템:
- 플레이어가 질문한 내용 / 개입 시점:
- 경로 방향 이해 / UI / BGM·SFX:
- 버그와 재현 상황 / 기타:

## 개발자용: 판단 기준

한 명의 의견만으로 밸런스를 크게 바꾸거나 도구·시스템을 삭제하거나 UI 전체를 다시 만들거나 음악을 교체하지 않는다. 먼저 여러 플레이의 행동과 이유를 모은다.

- **P0 즉시 수정 후보:** Crash, 진행·입력·Boss 진행 불가, Save/Restart 불가, 심각한 UI 차단.
- **P1 우선 수정 후보:** 핵심 목표나 필수 도구 사용법 이해 불가, Tutorial 실패, 심각한 시인성, 실패 이유 이해 불가.
- **P2 수집 후 판단:** 밸런스, 피로감, 특정 도구 선호, BGM 취향, 사소한 UI.

## Release Candidate 체크리스트

- [x] 작업 트리의 기존 사용자 변경과 RC 변경을 구분해 확인
- [x] 전체 EditMode 280/280 통과, 컴파일 및 Build Static Validation 통과
- [x] Test Mode OFF, 경로 디버그 숨김, 개발용 UI 비활성화 확인(정적)
- [x] 실제 외부 빌드에서 x1/x2/x3 UI 없음, 배속 입력 불가, 시작·재시작 x1 확인
- [x] 한국어/Galmuri 글리프 깨짐 없음, Audio 정상 및 참조 누락 없음
- [x] Fresh Start의 Tutorial 5개 표시 확인
- [x] Windows x86_64 Release 빌드 생성, Development Build OFF, 빌드 로그 Error 0
- [x] Standalone 실행 및 조업 시작
- [x] LMB/Q/W Tutorial, Growth/TAB
- [x] 복어/오징어, 거대 참치/E, 상어/Boss Tutorial
- [x] 성공 Result, 별도 Run의 실패 Result, Restart
- [x] Player.log에 치명 오류 없음
- [ ] 전체 빌드 폴더 ZIP과 다른 개발 PC 경로에 대한 의존성 없음

2026-09-30 사용자 Standalone 검증 통과. **External Playtest Ready: Approved** — Area 1 Vertical Slice RC1의 외부 테스트 전달 승인이다. **Final Production Approval: Pending.** 전체 빌드 폴더 ZIP과 별도 PC 의존성 확인은 보고되지 않아 위 체크박스를 미완료로 둔다.
