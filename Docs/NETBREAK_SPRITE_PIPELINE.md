# NETBREAK 물고기 Sprite Pipeline — VS-2B 방향별 수영 프로토타입

## 상태와 목적

첫 적용 대상인 정어리는 후속 일반 어종의 첫 프로토타입 기준으로 승인됐다. VS-2B-2에서 고등어·참치 시트와 프로필을 같은 파이프라인으로 확장했고 전체 EditMode 168/168과 사용자 Unity 수동 시각 검증을 완료했다. 고등어·참치는 **정어리 기준 Directional Fish Sprite Pipeline을 성공적으로 확장한 승인된 프로토타입**이다. 최종 출시용 Art Lock과 전체 어종 규격 확정은 아직 하지 않았다. 게임플레이 이동·충돌·포획과 시각 표현은 분리한다.

VS-2B-3에서는 복어·오징어의 방향별 **이동 중 수영** 시트와 FishVisualProfile을 추가하고 각 FishData에 연결했다. 기존 `FishVisualController`와 `FishVisualDirectionResolver`를 그대로 사용한다. 사용자가 Unity 컴파일 정상, Console Error 0, 전체 EditMode **173/173 통과**와 Play Mode 수동 시각 검증을 확인했다. 두 어종의 수영 이미지는 프로토타입으로 승인됐으며 최종 출시용 아트 승인은 대기 중이다.

VS-2B-4 현재 구현은 다섯 어종 모두 **E/N/NE/NW 원본 4축 × 4프레임 = 16프레임**이다. 기존 12프레임 RGBA 픽셀은 전부 보존하고 각 시트의 네 번째 행에 NW 4프레임을 생성했다. 반대 방향은 원본 프레임의 flipX와 flipY를 함께 켜서 180도 돌린다. 사용자가 Unity 컴파일 정상·Console Error 0·전체 EditMode 183/183 통과와 Play Mode 수동 시각 검증을 완료해 현재 프로토타입 방향 규칙으로 승인했다. 아래 VS-2B-1~3 승인과 수치는 각 당시 12프레임 구조에 대한 역사적 결과다. Final Production Art Approval은 Pending이다.

## 제작 원칙

- 밝은 청록 바다와 다수 어군에서 머리·몸통·꼬리의 실루엣을 읽을 수 있게 한다. 제한된 팔레트와 짙은 윤곽을 사용하며 배경보다 물고기와 전투 정보가 앞선다.
- 픽셀 격자에서 직접 제작하고, 투명 배경과 불투명 픽셀을 사용한다. 런타임 임의 회전이나 확대 보간으로 방향을 만들지 않는다.
- 세 생성 스크립트 `Tools/generate_sardine_sprite.py`, `Tools/generate_mackerel_tuna_sprites.py`, `Tools/generate_pufferfish_squid_sprites.py`가 기존 팔레트·실루엣·프레임 리듬을 그대로 사용해 네 번째 NW 행을 추가한다. `Tools/extend_fish_sprite_imports.py`는 확인된 12분할 Import 기록의 기존 Sprite ID를 보존하고 새 4분할 및 프로필 참조를 추가하는 일회성 이관 도구다.
- 다섯 시트는 모두 투명 RGBA, 4열×4행이다. 정어리 128×128 / 32×32 셀, 고등어 192×192 / 48×48 셀, 참치 256×256 / 64×64 셀, 복어 192×192 / 48×48 셀, 오징어 256×256 / 64×64 셀이다. 공통 PPU 83·기본 8 FPS와 기존 상대 크기·투명 여백을 유지한다. 각 어종의 첫 3행 픽셀은 이전 시트와 해시가 일치한다.

## 방향과 애니메이션

행 0은 동쪽 가로, 행 1은 북쪽 세로, 행 2는 북동쪽 대각선, 행 3은 북서쪽 대각선 원본이다. 각 행의 4프레임은 기존 중립·꼬리 휨 리듬을 유지한다. 머리 방향과 셀 중심 Pivot은 일정하며 Transform 이동은 프레임에 포함하지 않는다.

| 이동 | 세트 | flipX | flipY |
|---|---|---:|---:|
| 동 | 가로 | 아니요 | 아니요 |
| 서 | 가로 | 예 | 예 |
| 북 | 세로 | 아니요 | 아니요 |
| 남 | 세로 | 예 | 예 |
| 북동 | 대각선 | 아니요 | 아니요 |
| 북서 | 북서 대각선 | 아니요 | 아니요 |
| 남동 | 북서 대각선 | 예 | 예 |
| 남서 | 대각선 | 예 | 예 |

`FishMovement`가 실제로 사용한 방향을 읽고 `FishVisualDirectionResolver`가 45도 단위로 분류한다. 정지 또는 매우 작은 방향은 마지막 방향을 유지한다. 경계에서 7.5도 여유를 둬 진동을 줄인다. 동쪽 배가 아래에 있는 기준에서 북쪽 원본의 배는 오른쪽이다. 남쪽은 북쪽 프레임을 X/Y 함께 뒤집으므로 배가 왼쪽으로 이동한다. 북동·북서 원본도 각각 반대인 남서·남동으로 180도 뒤집어 배/등 관계를 유지한다. 경로와 속도는 변경하지 않는다. `FishVisualController`가 scaled time으로 4프레임 flipbook을 재생하므로 일시정지 시 프레임이 멈춘다. 인스턴스별 시작 프레임 분산은 아직 없다.

## 데이터와 풀

`FishData.VisualProfile`은 선택적 참조다. `FishVisualProfile`에는 네 방향 원본의 각 4프레임, FPS, 시각 크기, 기본 Tint만 저장한다. 애니메이션 시간·방향은 인스턴스의 `FishVisualController`가 소유한다. 프로필이 없거나 16프레임이 완전하지 않으면 기존 사각형 Sprite, `FishData.VisualScale`, `FishData.VisualColor`를 사용한다.

다섯 어종 Custom Visual은 기본 흰색 Tint와 프로필 크기 (1,1)을 사용한다. 기존 사각형용 `FishData.VisualScale`을 그림에 중복 적용하지 않는다. 게임플레이 루트와 CircleCollider2D는 그대로 두고 런타임 시각 전용 자식의 크기를 루트 배율에 맞춰 보정한다. 사각형 원본 렌더러는 Custom Visual이 있을 때만 숨긴다. 프리팹·씬 직렬화는 변경하지 않는다.

매 `FishController.Initialize`에서 현재 FishData로 프로필, Sprite, 프레임, 타이머, flipX/Y, 크기, Tint, 방향, Custom/Fallback 모드를 초기화한다. 기존 정어리→고등어→참치→Prototype 재사용 검증은 VS-2B-2의 역사적 결과다. 이번에는 다섯 어종과 MiniBoss/Boss fallback을 섞은 재사용 테스트를 갱신했다. Collider, Resistance, 보상, 경로와 풀 용량은 시각 시스템이 다루지 않는다.

## 가져오기와 이름

Unity 메뉴 `NETBREAK/Art/Setup All Fish Visuals`는 다섯 시트를 Sprite/Multiple, PPU 83, Point, 무압축, Mipmap Off, Full Rect, Clamp, 중심 Pivot으로 가져오고 각 16개 셀을 분할한다. Unity 6의 Sprite Editor Data Provider API를 사용한다. 이름은 각 어종의 소문자 접두사 뒤에 `_horizontal_0..3`, `_vertical_0..3`, `_diagonal_0..3`, `_diagonal_nw_0..3`을 붙인다. Unity 이미지 좌표계에 맞춰 맨 위 행부터 연결한다. 기존 Sprite ID와 프로필을 재사용한다. `NETBREAK/Art/Validate Fish Sprite Pipeline`으로 참조와 설정을 확인한다.

기존 `Setup Sardine Prototype`, `Setup Mackerel And Tuna`, `Setup Pufferfish And Squid` 메뉴도 대상 어종만 16개로 다시 설정한다. VS-2B-2 당시 공용 검증은 세 어종의 12개 프레임을 확인했다는 역사적 기록이다. 향후 어종은 `Assets/Art/Fish/<Species>/`에 시트를 두고 같은 이름·방향·Pivot 규칙을 따른다. 셀 크기와 PPU는 실제 화면 검증에 따라 재평가한다.

VS-2B-3 당시 복어·오징어에 12프레임 프로필과 FishData 연결을 추가했다. 현재 공용 검증은 다섯 어종 각각의 16개 Sprite와 MiniBoss/Boss fallback을 검사한다.

## VS-2B-3 Unity 수동 검증 — 역사적 기록

사용자가 Unity 컴파일과 Console Error 0, 전체 EditMode Test Runner 173/173 통과를 확인했다. Play Mode에서는 복어·오징어의 가로·세로·대각선, 반대 방향 flip, 4프레임 수영과 심각한 방향 jitter 없음, 기존 세 어종 정상 표시, 다수 어종 가독성, 종간 Pool 재사용 초기화, Pause, 오징어 먹물 방해, 기존 VFX·Resistance·포획·UI, Run 재시작 뒤 시각 초기화를 확인했다. 두 어종의 **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**이다.

당시 사용한 순서는 아래와 같다. 현재 16프레임 검증 순서는 다음 절을 따른다.

1. Unity가 파일을 가져온 뒤 Console 컴파일 오류를 확인한다. `NETBREAK/Art/Setup Pufferfish And Squid`를 실행하고 이어서 `NETBREAK/Art/Validate Fish Sprite Pipeline`의 성공 로그를 확인한다.
2. EditMode 전체 테스트를 실행한다. 당시 `FishVisualExpansionTests`에서 복어·오징어의 12개 Sprite, 프로필, FishData, 기존 특수 능력 수치와 풀 재사용을 확인했다.
3. Play Mode에서 복어와 오징어의 가로·세로·대각선 이동, 반대 방향 flip, 4프레임 수영, 크기와 실루엣, 다수 개체의 가독성을 확인한다.
4. 정어리·고등어·참치와 MiniBoss/Boss fallback, 종간 Pool 재사용, 일시정지/선택 상태, Resistance·Collider·포획·기존 먹물 방해, Run 재시작 및 Console 오류를 확인한다.

## VS-2B-4 현재 검증과 Unity 수동 확인 순서

- `Tools/validate_fish_visual_sheets.py` 정적 검사: 다섯 시트 4×4, 기존 12프레임 픽셀 해시 보존, 새 NW 4프레임, 팔레트, 16개 분할 이름·ID·참조 확인. 5/5 통과.
- 배색 픽셀 검사: 다섯 어종의 북쪽 원본 배색 중심이 오른쪽이며 180도 반전한 남쪽 중심은 왼쪽이다. 5/5 통과.
- Unity 검증: 앞선 비대화형 배치 시도는 Editor 잠금과 Package Manager IPC 연결 문제로 중단됐다. 이후 사용자가 Unity 컴파일 정상, Console Error 0, 전체 EditMode Test Runner **183/183 통과**를 확인했다. 실패 0; skip 수는 별도로 전달받지 않았다.
- Play Mode 수동 시각 검증: 다섯 어종의 E/W/N/S와 NE/NW/SE/SW, NE/NW 별도 원본, 반대 방향 flipX+flipY, 4프레임 수영이 정상이다. S의 배는 화면 왼쪽이며 E→SE→S 전환의 배/등 방향도 자연스럽다. Pool 재사용의 Sprite·frame·flip·tint·scale 잔상 없음, 오징어 먹물 방해·Resistance·Collider·포획·VFX·UI·Pause·Run 재시작 정상. **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**.
- `git diff --check`: 이번에 수정한 네 문서의 종료 코드 0. 전체 작업 트리는 이번 문서 갱신 시작 전부터 변경된 고등어 Sprite `.meta`의 공백 8곳과 NanumGothic-Bold SDF의 공백 3곳으로 종료 코드 2다. 두 파일은 수정하지 않았다.

재검증 시 사용할 Play Mode 확인 순서:

1. Unity가 파일을 다시 가져오게 한 뒤 Console 컴파일 오류를 확인한다. `NETBREAK/Art/Setup All Fish Visuals`와 `NETBREAK/Art/Validate Fish Sprite Pipeline`을 순서대로 실행해 성공 로그를 확인한다.
2. Test Runner의 전체 EditMode 테스트를 실행해 전체/통과/실패/skip 수를 기록한다. `FishVisualPipelineTests`와 `FishVisualExpansionTests`의 방향·배색·풀 재사용 항목을 확인한다.
3. Play Mode에서 정어리·고등어·참치·복어·오징어 각각 E→W, N→S, NE→SW, NW→SE를 확인한다. S에서 배가 왼쪽인지, NW와 NE가 다른 그림인지, 모든 반대 방향에서 X/Y 두 축이 함께 뒤집히는지 본다.
4. 다섯 어종을 연속 재사용할 때 이전 Sprite·프레임·flip·Tint가 남지 않는지 확인한다. MiniBoss/Boss 사각형 fallback, 일시정지, 오징어 먹물 방해, Resistance·Collider·포획, Run 재시작과 Console Error를 확인한다.

## 남은 결정

다음 두 단락의 Unity 수동 확인은 VS-2B-1/2 당시 12프레임 프로토타입의 역사적 결과다. 이번 16프레임 변경의 Play Mode 결과로 읽지 않는다.

사용자는 Unity Play Mode에서 정어리의 세 방향·반대 방향 flip·4프레임 헤엄, 심각한 방향 jitter 없음, 여러 정어리의 동시 표시와 기존 대비 크기, 다른 어종 fallback, 양방향 Pool 재사용, Pause/선택 상태, 기존 VFX·Resistance·포획·UI와 Run 재시작을 확인했다. Unity 컴파일 정상, Console Error 0이다. 현재 정어리 Prototype 이미지 방향에도 만족한다고 밝혔다.

VS-2B-2에서는 사용자가 고등어·참치의 가로·세로·대각선과 반대 방향 flip, 4프레임 헤엄, 심각한 방향 jitter 없음, 세 어종의 크기 계층·실루엣 구분, 다수 개체·어군 가독성, 적절한 참치 크기를 확인했다. 복어·오징어 fallback, 종간 Pool 재사용 초기화, Pause/선택 상태, 기존 VFX·Resistance·포획·UI 및 Run 재시작 뒤 시각 상태도 정상이다. Unity 컴파일과 전체 EditMode 테스트가 정상이고 Console Error는 0이었다. 고등어·참치 이미지는 현재 아트 방향의 프로토타입으로 승인됐다.

32×32·48×48·64×64 셀, PPU 83, 기본 8 FPS와 현재 팔레트는 이전에 승인된 다섯 어종 프로토타입의 값으로, 최종 출시용 Art Lock은 아니다. 후속 제작·통합 검증에서 조정할 수 있다. 다른 어종의 프레임 수, 특수어·Boss 규격, Pixel Perfect Camera 정책, 내부 렌더 해상도와 전체 프로젝트 공통 PPU·Cell·FPS 정책은 미정이다. 대규모 어군 성능과 최종 배경·VFX 적용 뒤의 전체 화면 가독성은 후속 통합 검증 대상이다.

복어 팽창·가시 강화·특수 상태 전용 애니메이션과 오징어 먹물 발사 전용 프레임·애니메이션은 이번 단계에 없다. 오징어의 기존 먹물 방해 게임플레이는 유지하며 향후 발사 애니메이션/VFX/SFX와 연결할 수 있다. 복어 특수 연출, 최종 VFX/SFX 및 최종 아트 승인은 후속 후보이고 이번 작업에서 완료로 간주하지 않는다.
