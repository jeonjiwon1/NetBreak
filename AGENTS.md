# NETBREAK 작업 규칙

## 매 세션 시작
- 사용자와 한국어로 대화한다. `NETBREAK_STATE.md`와 `Docs/NETBREAK_DESIGN.md`를 먼저 읽는다.
- 작업 전 `git status`, `git branch --show-current`, `git remote -v`를 확인한다. 개발 브랜치는 `vertical-slice`다. 기존 사용자 변경을 보존한다.
- 최신 사용자 지시가 우선한다. 새 설계와 충돌하는 기존 `README.md`, `Docs/GDD.md`, `Docs/PrototypeDesign.md`, `Docs/VerticalSliceDesign.md`는 역사적 자료로 취급한다. 목표 설계를 구현 완료로 기록하지 않는다.
- 기능 수정 전에 관련 스크립트 전체와 호출 흐름, Scene/Prefab의 실제 직렬화 값을 확인한다. 코드 기본값을 Inspector 값으로 가정하지 않는다.

## 변경하지 말아야 할 설계 원칙
- Unity 6 / C# / Universal 2D. 핵심은 LURE → TRAP → CATCH. 캐릭터 이동 없이 커서로 개입하며, 물고기는 제작자가 작성한 경로를 따른다.
- 물고기는 HP 대신 Resistance를 사용한다. 0은 사망이 아니라 포획이다.
- 모든 Run은 Area 1에서 시작한다. 해금된 지역에서 시작하는 체크포인트를 만들지 않는다. 해금 상한 지역을 클리어하면 Run 종료 후 다음 지역을 해금한다.
- 일반 물고기의 Destination 도달은 미포획이다. 최종 보스 포획이 클리어 조건이고 마지막 회유 도주는 실패다. 어획률은 보상/등급 지표다.
- 현재 입력 구조는 LMB 뜰채 고정, Q Core Tool, W Partner Tool, E Tactical Skill, R Signature Skill이다. Q/W 도구는 선택한 Run 도구에 따라 달라지며 특정 도구와 영구 결합하지 않는다.
- Run은 뜰채만 소유한 Lv1에서 시작한다. Lv2 Core/Q, Lv3 Partner/W를 획득하고 이후 숙련 포인트로 두 Tool Tree에 투자한다. 정상 Area 1은 아이템을 지급하지 않으며 구 Random Augment와 Job은 대체가 검증될 때까지 휴면 Legacy로 보존한다.
- Meta 해금과 Run 소유를 분리한다. 해금되었어도 Run에서 획득하지 않은 도구는 사용할 수 없다.
- 지역 전환 시 Run 빌드(Core/Partner, Tool Tree, E/R, 아이템, Gold/EXP/Level)는 유지하고 설치물·미끼·물고기·임시 오브젝트·인카운터 상태만 초기화한다.
- Tool은 플레이 수단이고 Core/Partner 및 Tool Tree가 현재 빌드 전문화 구조다. 구 Job 효과는 Tool Tree로 이전되었고 자동 Job 효과는 중단되어 있다.
- 한 Run의 고정 목표 플레이타임이나 절대 상한을 두지 않는다. 현재 구현된 해역 수를 기준으로 해역별 시간과 전체 Run 시간을 구분해 실측하며, 시간 길이보다 성장 밀도·전투 변화·전략적 판단·지루함 여부를 우선한다.
- 미래 도구, 후속 전직, 보조 직업, Capstone, Area 2~6은 방향만 유지한다. 별도 요청 없이 구현/밸런싱하거나 거대한 Area 리팩터링을 하지 않는다.

## 엔지니어링
- 입력은 New Input System만 사용한다(`Keyboard.current`, `Mouse.current`). `Input.GetKey*`, `Input.GetMouseButton*`, `Input.mousePosition` 등 Legacy API를 추가하지 않는다.
- 플레이어 노출 콘텐츠는 한국어, 코드 식별자는 영어다. 새 파일은 UTF-8. 기존 파일은 원본 인코딩을 확인한 후 건드리는 문자열만 안전하게 복원한다. 무관한 대량 인코딩 변환은 하지 않는다.
- UI는 Canvas + TextMeshPro + Button을 사용한다. Legacy Text를 추가하지 않는다. 현재 적용 폰트는 NanumGothic-Bold SDF, Dynamic atlas이며, 정식 픽셀아트 UI는 갈무리 9를 우선 후보로 검토한다. 실제 이관과 장문 가독성·라이선스 검증은 후속 작업이다.
- 직렬화된 필드/씬/프리팹 참조와 `.meta` GUID를 보존하며 점진적으로 이관한다. 중복 Manager를 만들지 않는다.
- `FishSpecialType` 순서 `None, Pufferfish, Squid, MiniBoss, Boss`를 절대 바꾸지 않는다. 다른 직렬화 enum도 재정렬하지 않는다.
- 도구 구조 작업 중 기존 보스 다중 회유를 불필요하게 재설계하거나 관련 없는 밸런스 값을 바꾸지 않는다.
- Scene/Prefab/Inspector 수정은 Computer Use가 가능하면 Unity Editor를 우선한다. 복잡한 Unity YAML을 추측해 수정하지 않는다.
- 의미 있는 코드 변경 후 컴파일 및 관련 검증을 수행하고 Unity Console을 확인한다. 컴파일 오류를 해결하기 전에 다음 주요 기능으로 넘어가지 않는다.
- 반복 가능한 기능 검사는 수동 Computer Use 재현보다 결정론적 Unity Editor/PlayMode 검증 스크립트를 우선한다. Computer Use는 시각·UI·상호작용처럼 코드로 신뢰성 있게 검증하기 어려운 항목에 주로 사용한다.

## 검증·인수인계·Git
- 검증한 주요 마일스톤마다 관련 테스트/Unity 확인 → diff 검토 → `NETBREAK_STATE.md` 갱신 → 의미 있는 커밋 1개 → origin의 현재 개발 브랜치에 push한다. 작은 수정마다 커밋하지 않는다.
- 커밋 메시지는 간결한 영어 명령형이다. main 직접 push, force push, 사용자 작업을 지우는 reset/checkout은 금지한다.
- 검증 불가 시 무엇을 확인하지 못했는지 정확히 기록한다. 미검증/깨진 작업을 자동 push하지 않는다.
- 초기 온보딩은 문서 검토까지다. 사용자가 이 검토를 마치고 후속 작업을 요청하기 전에는 STEP 10A 구현 및 이 온보딩 문서의 commit/push를 하지 않는다.
