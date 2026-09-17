# NETBREAK 현재 설계 기준

기준일: 2026-09-14. 출처: 사용자가 제공한 NETBREAK 온보딩 안내. 이 문서는 목표 설계이며 구현 상태는 `../NETBREAK_STATE.md`에서 확인한다. 기존 Docs 문서와 충돌하면 최신 사용자 지시와 본 문서를 따른다. 저장소의 기존 디렉터리 표기 `Docs/`를 유지한다(요청의 `docs/`와 Windows에서는 같은 경로).

## 게임 정체성
Unity 6.x, C#, Universal 2D 기반 1인 개발 2D 로그라이트 어업 디펜스/전략 게임이다. 플레이어 캐릭터 이동은 없고 마우스 커서가 상호작용 지점이다. 제작자가 작성한 물고기 경로를 관찰하고 도구·설치물·무기·유틸리티·스킬로 이동을 조작해 포획한다.

**LURE → TRAP → CATCH**

> 잡는 순간보다, 몰아넣는 과정이 전략이고, 한꺼번에 잡는 순간이 보상이다.

물고기의 자원은 HP가 아닌 Resistance다. Resistance 0은 포획을 뜻한다. 모든 플레이어 노출 UI, 도구명, 직업명, 안내, 결과, 툴팁, 튜토리얼과 설명은 한국어로 작성한다. 내부 코드 식별자는 영어, 새 텍스트 파일은 UTF-8을 사용한다.

## 도구와 입력
현재 구현된 도구는 뜰채(Landing Net), 미끼(Bait), 그물(Net), 투망(Cast Net), 낚싯대(Fishing Rod)다. 현재 Q=미끼/W=그물/E=투망/R=낚싯대 고정 구조는 이관 대상이다.

| 입력 | 목표 역할 |
|---|---|
| LMB | 뜰채, 영구 고정 |
| Q | Active Tool Slot 1 |
| W | Active Tool Slot 2 |
| E | Active Tool Slot 3 |
| R | Active Tool Slot 4 |

Q/W/E/R은 도구 이름이 아닌 슬롯이다. 장래 주낙, 통발, 작살/어총, 어뢰, 소나, 자동 미끼 장치 등을 추가해도 키가 늘어나지 않아야 한다. 이 미래 도구들은 현재 구현하지 않는다. 내부 분류 Weapon/Installation/Skill/Utility는 가능하지만 호환되는 액티브 도구는 동일한 슬롯/로드아웃 시스템을 사용한다.

입력은 Unity New Input System만 사용한다. 키보드와 클릭 UI는 향후 동일한 도구 동작으로 연결한다. 기존 도구의 즉시 사용, 누름/뗌 조준, 배치 모드와 취소 동작을 고려하는 점진적 이관이 필요하다.

## Run 시작과 도구 획득
시작 소유물은 뜰채뿐이다. Q/W/E/R은 모두 빈 슬롯이다. 초반 Tool Acquisition 선택을 받고 선택한 도구가 빈 슬롯을 채운다. 두 번째 도구 선택 후, 액티브 도구 약 2개가 확보된 시점부터 일반 Augment 진행을 시작한다. 정확한 획득 트리거와 시점은 후속 구현에서 정한다.

뜰채만 가진 상태에서 증강을 먼저 제공하면 후보가 과도하게 좁아지므로 도구 획득과 증강을 구분한다. 현재 Slice의 획득 후보는 기존 미끼·그물·투망·낚싯대만 사용한다.

## Meta Unlock과 Run Ownership
Meta Unlock은 Run 간 유지되는 후보 자격이다. Run Ownership은 이번 판에서 실제 획득한 도구다. 메타 해금된 낚싯대를 이번 판에서 선택하지 않았다면 획득 후보로는 나올 수 있지만 사용할 수 없고 낚싯대 전용 증강도 나오면 안 된다.

| 영구 Meta 상태 | 임시 Run 상태 |
|---|---|
| MaxUnlockedArea | CurrentArea |
| UnlockedTools | OwnedTools, ToolSlots |
| MetaCurrency | Gold, EXP, Level |
| HardModeUnlocked | Augments, PrimaryJob, SecondaryJob, FinalExpertise |

표의 미래 상태 항목은 즉시 구현하라는 뜻이 아니다.

## 증강
증강은 주로 현재 소유한 도구를 강화한다. 예를 들어 뜰채·미끼·투망 소유 시 LandingNet/Bait/CastNet/General만 유효하고 Net/FishingRod는 획득 전까지 제외한다. 직업별 가중치가 소유 조건을 우회하면 안 된다. 현재 Unique 증강의 1회 획득 동작을 유지한다. 도구가 충분히 확보되기 전에는 일반 증강을 지연한다.

## Hotbar
목표 표시는 `[LMB] [Q] [W] [E] [R]`이다. LMB는 뜰채를 고정 표시하고 나머지는 실제 슬롯의 도구 또는 빈 상태를 표시한다. 관련 쿨다운/충전 상태를 노출하고 슬롯 클릭은 단축키와 같은 동작을 실행한다. 특정 키와 도구의 고정 매핑을 HUD에 재도입하지 않는다. 기존 `투망 [E]: 준비 완료` 등의 중복 HUD는 동등한 Hotbar 정보가 정상 작동한 뒤 제거한다.

## 직업과 전문화
Tool은 플레이 수단을 제공하며 Job은 빌드를 전문화하거나 동작을 바꾼다. 현재 1차 직업은 CastNetFisher(투망꾼), NetFisher(그물잡이), Angler(낚시꾼), LandingNetFisher(뜰채잡이)다. Slice의 밸런스 범위는 1차 직업만이다.

## 패시브 아이템

아이템은 특정 Tool에 부착되지 않고 어떤 Core/Partner 조합에서도 독립적으로 자동 작동하는 패시브 전투 시스템이다. 한 Run에는 서로 다른 아이템을 최대 4개 소유하며 신규 획득은 기본 미보유 3택1이다. Area 1은 아이템을 지급하지 않고, Area 2 MiniBoss/Boss와 Area 3 MiniBoss/Boss가 차례로 첫째~넷째 아이템을 지급한다. Area 4부터의 보상은 보유 아이템 업그레이드로 전환한다. 현재 G5-A는 이 획득 기반만 다루며 실제 효과는 G5-B, 레벨 상승과 전기/검/얼음 속성 시너지는 G6 범위다.

정식 버전은 Area 1 보스 포획 후 1차 직업을 제공한다. Area 1만 있는 Slice에서는 직업을 체험할 수 있도록 앞당길 수 있으며 이는 임시 페이싱이다. 2차 전직, 보조/이중 직업, Final Expertise/Capstone은 미래 확장 여지만 유지한다.

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

지역 전환 시 로드아웃, 증강, 직업, Gold, EXP, Level과 기타 Run 빌드는 유지한다. 배치된 그물·낚싯대, 활성 미끼, 물고기, 임시 월드 오브젝트 및 지역 인카운터 상태는 초기화한다. Run 재시작과 같은 Run 안의 지역 전환을 혼동하지 않는다.

## 전체 성장 방향
Run 시작 → 뜰채만 소유 → 도구 획득 → 도구 획득 → 증강 시작 → Area 1 보스 → 1차 직업.

이후 Area 2는 빌드 발전/추가 전문화, Area 3은 Signature/Legendary 보상, Area 4는 가능한 보조 직업, Area 5는 Final Expertise/Capstone, Area 6은 완성 빌드 도전과 최종 보스 방향이다. **Area 2~6 성장 구조는 미확정이며 지금 구현하거나 밸런싱하지 않는다.**

## 성공·실패와 보스
일반 물고기의 Destination 도달은 미포획으로 처리하며 즉시 패배하지 않는다. 최종 보스 포획이 지역 진행 조건이다. 보스가 마지막 회유에서 도주하면 Run 실패다. 어획률은 보상/등급 지표이며 주 클리어 조건이 아니다. 제거된 Legacy Final Fishing 타이머를 복원하지 않는다.

현재 Area 1 보스는 3회 회유를 지원한다. 도구 로드아웃 작업 중 불필요하게 재설계하지 않는다. 직렬화 enum FishSpecialType의 순서는 `None → Pufferfish → Squid → MiniBoss → Boss`로 보존한다.

## UI·개발 방법
주요 런타임 UI는 Canvas/TMP로 이관되어 있다. HUD, 준비/시작, 공지, 직업/증강 선택, 미니보스, 보스, 결과, 월드 피드백을 유지한다. Canvas/TextMeshPro/Button - TextMeshPro를 쓰며 Legacy Text를 추가하지 않는다. Slice 글꼴은 NanumGothic-Bold SDF, Dynamic atlas 방향이다.

관련 코드와 제어 흐름을 읽고 직렬화 참조를 보존하며 점진적으로 변경한다. Manager 중복, enum 재정렬, Scene/Prefab Inspector 참조 파손을 피한다. 의미 있는 변경 후 컴파일 및 Unity Console 확인을 진행한다. 구조 작업에 무관한 밸런스 수정은 하지 않는다. 자세한 영구 작업/Git 규칙은 `../AGENTS.md`를 따른다.

## 다음 로드맵
| 단계 | 작업 |
|---|---|
| 10A | Dynamic Tool Slot Foundation |
| 10B | Tool Acquisition flow |
| 10C | 소유 도구 증강 필터링 및 증강 시작 지연 |
| 10D | 동적 클릭형 Hotbar |
| 10E | 새 성장 진행을 Area 1 Slice에 통합 |
| 11 | 전체 Run 실측 및 첫 본격 밸런스 조정 |
| 12 | 핵심 VFX |
| 13 | 핵심 SFX/BGM |
| 14 | UX/온보딩 개선 |
| 15 | 회귀 테스트 |
| 16 | 빌드, README, 포트폴리오 스크린샷/영상 |

현재 온보딩은 저장소 조사 및 문서 검토만 수행한다. STEP 10A 구현과 commit/push는 이번 작업에서 제외한다.
