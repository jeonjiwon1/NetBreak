# NETBREAK 현재 설계 기준

기준일: 2026-09-21. 이 문서는 목표 설계이며 구현 상태는 `../NETBREAK_STATE.md`에서 확인한다. 기존 Docs 문서와 충돌하면 최신 사용자 지시와 본 문서를 따른다. 저장소의 기존 디렉터리 표기 `Docs/`를 유지한다.

## 게임 정체성
Unity 6.x, C#, Universal 2D 기반 1인 개발 2D 로그라이트 어업 디펜스/전략 게임이다. 플레이어 캐릭터 이동은 없고 마우스 커서가 상호작용 지점이다. 제작자가 작성한 물고기 경로를 관찰하고 도구·설치물·무기·유틸리티·스킬로 이동을 조작해 포획한다.

**LURE → TRAP → CATCH**

> 잡는 순간보다, 몰아넣는 과정이 전략이고, 한꺼번에 잡는 순간이 보상이다.

물고기의 자원은 HP가 아닌 Resistance다. Resistance 0은 포획을 뜻한다. 모든 플레이어 노출 UI, 도구명, 직업명, 안내, 결과, 툴팁, 튜토리얼과 설명은 한국어로 작성한다. 내부 코드 식별자는 영어, 새 텍스트 파일은 UTF-8을 사용한다.

## 도구와 입력
현재 구현된 도구는 뜰채(Landing Net), 미끼(Bait), 그물(Net), 투망(Cast Net), 낚싯대(Fishing Rod)다.

| 입력 | 목표 역할 |
|---|---|
| LMB | 뜰채, 영구 고정 |
| Q | Core Tool: Run의 주력 도구 |
| W | Partner Tool: Run의 보조 또는 두 번째 포획 도구 |
| E | Tactical Skill: Area 1 MiniBoss 포획 보상 |
| R | Signature Skill: Area 1 Boss 포획 보상, Core에 종속 |

Q/W는 도구 이름이 아니라 선택한 Core/Partner 역할에 대응하는 동적 슬롯이다. 장래 주낙, 통발, 작살/어총, 어뢰, 소나, 자동 미끼 장치 등을 추가해도 키가 늘어나지 않아야 한다. 이 미래 도구들은 현재 구현하지 않는다. E/R은 도구 슬롯이 아니라 각각 전술/시그니처 스킬 전용 입력이다.

입력은 Unity New Input System만 사용한다. 키보드와 클릭 UI는 향후 동일한 도구 동작으로 연결한다. 기존 도구의 즉시 사용, 누름/뗌 조준, 배치 모드와 취소 동작을 고려하는 점진적 이관이 필요하다.

## Run 시작과 도구 획득
시작 소유물은 뜰채뿐이다. Lv2에서 Core 도구를 선택해 Q에, Lv3에서 Core와 다른 Partner 도구를 선택해 W에 배정한다. Lv2부터 레벨마다 공용 숙련 포인트를 받고 Lv4부터 두 Tool Tree의 노드에 자유 투자한다. E는 Area 1 MiniBoss 포획 뒤 무료 3택1, R은 Area 1 Boss 포획 뒤 선택한 Core에 맞춰 해금된다.

Vertical Slice의 Core 후보는 낚싯대·그물·투망이며 Partner는 미끼와 선택한 Core가 아닌 도구를 사용한다. 같은 도구를 Core와 Partner에 중복 장착하지 않는다.

## Meta Unlock과 Run Ownership
Meta Unlock은 Run 간 유지되는 후보 자격이다. Run Ownership은 이번 판에서 Core/Partner로 실제 획득한 도구다. 메타 해금된 낚싯대를 이번 판에서 선택하지 않았다면 후보로는 나올 수 있지만 사용할 수 없고 낚싯대 Tool Tree에도 투자할 수 없다.

| 영구 Meta 상태 | 임시 Run 상태 |
|---|---|
| MaxUnlockedArea | CurrentArea |
| UnlockedTools | Core/Partner, Q/W, Tool Tree 투자 |
| MetaCurrency | Gold, EXP, Level, 숙련 포인트, E/R, 아이템 |
| HardModeUnlocked | Run 성장 전체 |

표의 미래 상태 항목은 즉시 구현하라는 뜻이 아니다.

## Tool Tree와 Legacy 성장
현재 성장의 중심은 Core/Partner별 Tool Tree다. 기존 Random Augment와 Job은 정상 진행에서 중단된 휴면 Legacy이며, 대체 완료가 검증되기 전까지 참조 호환을 위해 보존한다. 새 Tree와 구 Augment·Job 보상을 동시에 지급하지 않는다. G9에서 관련 시스템 대체가 확인된 범위만 정리한다.

## Hotbar
표시는 `[LMB] [Q] [W] [E] [R]`이다. LMB는 뜰채, Q/W는 실제 Core/Partner 도구, E/R은 해금된 스킬 또는 잠금 상태를 표시한다. 관련 쿨다운·충전·대상 지정 상태는 실제 런타임 상태를 읽으며 HUD에 별도 상태를 복제하지 않는다.

## 빌드 전문화
Tool은 플레이 수단을 제공하며 Core/Partner 역할과 두 Tool Tree의 투자가 빌드를 전문화한다. 기존 CastNetFisher·NetFisher·Angler·LandingNetFisher 효과 중 유효한 것은 Tool Tree로 이전되었고 자동 Job 효과는 중단되어 있다. 후속 직업 체계는 현재 확정하지 않는다.

## 패시브 아이템

아이템은 특정 Tool에 부착되지 않고 어떤 Core/Partner 조합에서도 독립적으로 자동 작동하는 패시브 전투 시스템이다. 한 Run에는 서로 다른 아이템을 최대 4개 소유하며 신규 획득은 기본 미보유 3택1이다. 정상 Area 1은 아이템을 지급하지 않는다. 해역별 신규 획득·업그레이드 계약은 G8에서 확정·연결하며 현재 임의로 바꾸지 않는다. G5~G6에서 전기/검/얼음 6종, 아이템 레벨, 단일 5단계 시너지와 세 복합 시너지 전투를 구현했다.

## 지역 해금과 Run 진행
**모든 Run은 항상 Area 1에서 시작한다. 시작 체크포인트는 없다.**

초기 MaxUnlockedArea=1이다. 첫 Run에서 Area 1 클리어 → Run 종료 → Area 2 해금. 다음 Run은 Area 1 → Area 2 클리어 → Run 종료 → Area 3 해금. 이후도 같은 규칙으로 Area 1부터 현재 해금 상한까지 연속 진행하며 Area 6까지 확장한다. 해금된 지역을 골라 시작하는 구조로 바꾸지 않는다.

| 순서 | 계획된 지역 |
|---|---|
| 1 | 연안 / Coast |
| 2 | 외해 / Open Sea |
| 3 | 산호해 / Coral |
| 4 | 심해 / Deep Sea |
| 5 | 폭풍해역 / Storm |
| 6 | 심연 / Abyss |

현재는 연안 Slice만 존재한다. FishSpawner의 연안 시퀀스는 프로토타입 구현이다. 지금 거대한 Area 리팩터링을 하지 않되 새 도구 구조를 Coast에 결합하지 않는다. 장래 AreaData ScriptableObject 또는 동등한 데이터 기반 지역 구성을 고려한다.

지역 전환 시 Core/Partner, Tool Tree, E/R, 아이템, Gold, EXP, Level과 기타 Run 빌드는 유지한다. 배치된 그물·낚싯대, 활성 미끼, 물고기, 임시 월드 오브젝트 및 지역 인카운터 상태는 초기화한다. Run 재시작과 같은 Run 안의 지역 전환을 혼동하지 않는다.

## 플레이타임 원칙

한 Run은 조업 시작부터 결과 화면까지의 전체 진행이다. 해역 이동은 새 Run 시작이 아니다. Area 1만 구현된 Vertical Slice에서는 Area 1 보스 포획 후 Run이 비교적 일찍 끝나는 것이 정상이며, Area 2~6이 추가되면 한 Run에서 순차 진행하는 해역 수가 늘어 전체 시간도 자연스럽게 변한다.

한 Run에 30분·45분·60분 같은 고정 목표나 절대 상한을 두지 않는다. 30분을 초과해도 그 자체로 문제가 아니고, 최종 Run이 1시간 이상이어도 허용한다. 반대로 1시간을 채우기 위해 어군 간격, 전투 시간, Boss Resistance나 콘텐츠를 억지로 늘리지 않는다. 플레이타임은 해역별 시간과 전체 Run 시간을 구분해 기록하는 분석 지표이며, 최우선 기준은 성장 밀도, 전투 변화, 전략적 판단과 지루함 여부다. 최종 길이는 구현된 콘텐츠와 플레이테스트 결과로 결정한다.

## 전체 성장 방향
Run 시작 → 뜰채만 소유 → Lv2 Core/Q → Lv3 Partner/W → Tool Tree 투자 → Area 1 MiniBoss와 E → Area 1 Boss와 R → 현재 해금 상한에 따라 다음 Area 또는 결과.

이후 Area 2는 빌드 발전/추가 전문화, Area 3은 Signature/Legendary 보상, Area 4는 가능한 보조 직업, Area 5는 Final Expertise/Capstone, Area 6은 완성 빌드 도전과 최종 보스 방향이다. **Area 2~6 성장 구조는 미확정이며 지금 구현하거나 밸런싱하지 않는다.**

## 성공·실패와 보스
일반 물고기의 Destination 도달은 미포획으로 처리하며 즉시 패배하지 않는다. 최종 보스 포획이 지역 진행 조건이다. 보스가 마지막 회유에서 도주하면 Run 실패다. 어획률은 보상/등급 지표이며 주 클리어 조건이 아니다. 제거된 Legacy Final Fishing 타이머를 복원하지 않는다.

현재 Area 1 보스는 3회 회유를 지원한다. 도구 로드아웃 작업 중 불필요하게 재설계하지 않는다. 직렬화 enum FishSpecialType의 순서는 `None → Pufferfish → Squid → MiniBoss → Boss`로 보존한다.

## UI·개발 방법
주요 런타임 UI는 Canvas/TMP로 이관되어 있다. HUD, 준비/시작, 공지, 성장 관리, 도구/E/아이템 선택, 미니보스, 보스, 결과, 월드 피드백을 유지한다. Canvas/TextMeshPro/Button을 쓰며 Legacy Text를 추가하지 않는다. Slice 글꼴은 NanumGothic-Bold SDF, Dynamic atlas 방향이다.

관련 코드와 제어 흐름을 읽고 직렬화 참조를 보존하며 점진적으로 변경한다. Manager 중복, enum 재정렬, Scene/Prefab Inspector 참조 파손을 피한다. 의미 있는 변경 후 컴파일 및 Unity Console 확인을 진행한다. 구조 작업에 무관한 밸런스 수정은 하지 않는다. 자세한 영구 작업/Git 규칙은 `../AGENTS.md`를 따른다.

## 현재 개발 순서

G6-C2까지 구현·테스트·수동 검증이 완료됐다. 다음 순서는 현재 버전 최소 안정화, 최소 온보딩·전투 피드백, 소규모 외부 플레이테스트, G7/G8, Area 2 확장성 검증, 점진적 도구·아이템 확장이다. 현재 Full Run은 최종 수치 확정이 아니라 기능 안정성과 핵심 경험을 확인하기 위한 것이다. 세부 장기 순서는 `NETBREAK_ROADMAP.md`를 따른다.
