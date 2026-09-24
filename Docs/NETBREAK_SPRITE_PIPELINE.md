# NETBREAK 물고기 Sprite Pipeline — VS-2B 일반 어종 프로토타입

## 상태와 목적

첫 적용 대상인 정어리는 후속 일반 어종의 첫 프로토타입 기준으로 승인됐다. VS-2B-2에서 고등어·참치 시트와 프로필을 같은 파이프라인으로 확장했고 전체 EditMode 168/168과 사용자 Unity 수동 시각 검증을 완료했다. 고등어·참치는 **정어리 기준 Directional Fish Sprite Pipeline을 성공적으로 확장한 승인된 프로토타입**이다. 최종 출시용 Art Lock과 전체 어종 규격 확정은 아직 하지 않았다. 게임플레이 이동·충돌·포획과 시각 표현은 분리한다.

## 제작 원칙

- 밝은 청록 바다와 다수 어군에서 머리·몸통·꼬리의 실루엣을 읽을 수 있게 한다. 제한된 팔레트와 짙은 윤곽을 사용하며 배경보다 물고기와 전투 정보가 앞선다.
- 픽셀 격자에서 직접 제작하고, 투명 배경과 불투명 픽셀을 사용한다. 런타임 임의 회전이나 확대 보간으로 방향을 만들지 않는다.
- 정어리 시트는 `Assets/Art/Fish/Sardine/Sardine_Swim.png`, 원본 생성 절차는 `Tools/generate_sardine_sprite.py`다. 현재 128×96 RGBA 시트의 32×32 셀 3행×4열이다. 가로 그림의 실제 실루엣은 셀보다 작고 투명 여백이 있다.
- 현재 셀은 정어리 32×32, 고등어 48×48, 참치 64×64다. **모든 어종이 같은 셀 크기일 필요는 없다.** 사용자 Unity 검증에서 세 어종이 공통 PPU 83·3방향×4프레임·기본 8 FPS를 공유하면서 원본 실루엣 크기와 투명 여백으로 자연스러운 크기 계층을 이루는 것을 확인했다. 이 값들은 현재 프로토타입 설정이며 전체 프로젝트의 최종 규격은 아니다.
- 고등어 시트는 `Assets/Art/Fish/Mackerel/Mackerel_Swim.png`의 192×144 RGBA, 참치는 `Assets/Art/Fish/Tuna/Tuna_Swim.png`의 256×192 RGBA다. 둘 다 `Tools/generate_mackerel_tuna_sprites.py`에서 픽셀 격자로 직접 제작했다. 가로 프레임의 불투명 영역은 고등어 약 43×21px, 참치 프레임별 약 59×29~31px이며 정어리 약 31×15px보다 크다.

## 방향과 애니메이션

행 0은 동쪽 가로, 행 1은 북쪽 세로, 행 2는 북동쪽 대각선 원본이다. 각 행의 4프레임은 중립 → 꼬리 한쪽 휨 → 중립 → 반대쪽 휨 → 반복이다. 머리 방향과 셀 중심 Pivot은 일정하다. Transform 이동은 프레임에 포함하지 않는다.

| 이동 | 세트 | flipX | flipY |
|---|---|---:|---:|
| 동 | 가로 | 아니요 | 아니요 |
| 서 | 가로 | 예 | 아니요 |
| 북 | 세로 | 아니요 | 아니요 |
| 남 | 세로 | 아니요 | 예 |
| 북동 | 대각선 | 아니요 | 아니요 |
| 북서 | 대각선 | 예 | 아니요 |
| 남동 | 대각선 | 아니요 | 예 |
| 남서 | 대각선 | 예 | 예 |

`FishMovement`가 실제로 사용한 방향을 읽고 `FishVisualDirectionResolver`가 45도 단위로 분류한다. 정지 또는 매우 작은 방향은 마지막 방향을 유지한다. 경계에서 7.5도 여유를 둬 진동을 줄인다. 경로와 속도는 변경하지 않는다. `FishVisualController`가 scaled time으로 4프레임 flipbook을 재생하므로 일시정지 시 프레임이 멈춘다. 인스턴스별 시작 프레임 분산은 아직 없다.

## 데이터와 풀

`FishData.VisualProfile`은 선택적 참조다. `FishVisualProfile`에는 세 방향의 4프레임, FPS, 시각 크기, 기본 Tint만 저장한다. 애니메이션 시간·방향은 인스턴스의 `FishVisualController`가 소유한다. 프로필이 없거나 12프레임이 완전하지 않으면 기존 사각형 Sprite, `FishData.VisualScale`, `FishData.VisualColor`를 사용한다.

세 어종 Custom Visual은 기본 흰색 Tint와 프로필 크기 (1,1)을 사용한다. 기존 사각형용 `FishData.VisualScale`을 그림에 중복 적용하지 않는다. 게임플레이 루트와 CircleCollider2D는 그대로 두고 런타임 시각 전용 자식의 크기를 루트 배율에 맞춰 보정한다. 사각형 원본 렌더러는 Custom Visual이 있을 때만 숨긴다. 프리팹·씬 직렬화는 변경하지 않는다.

매 `FishController.Initialize`에서 현재 FishData로 프로필, Sprite, 프레임, 타이머, flipX/Y, 크기, Tint, 방향, Custom/Fallback 모드를 초기화한다. 정어리→고등어→참치→Prototype→고등어→참치→Prototype→참치→정어리 순서의 같은 풀 오브젝트 재사용도 자동 검증했다. Collider, Resistance, 보상, 경로와 풀 용량은 시각 시스템이 다루지 않는다.

## 가져오기와 이름

Unity 메뉴 `NETBREAK/Art/Setup Sardine Prototype`은 시트를 Sprite/Multiple, PPU 83, Point, 무압축, Mipmap Off, Full Rect, Clamp, 중심 Pivot으로 가져오고 12개 셀을 분할한다. Unity 6의 Sprite Editor Data Provider API를 사용한다. 이름은 `sardine_horizontal_0..3`, `sardine_vertical_0..3`, `sardine_diagonal_0..3`이다. Unity 이미지 좌표계에 맞춰 맨 위 행부터 연결한다. 프로필을 생성하거나 재사용해 정어리 FishData에만 연결하고 다른 어종은 건드리지 않는다. `NETBREAK/Art/Validate Fish Sprite Pipeline`으로 참조와 설정을 확인한다. 반복 실행 시 기존 Sprite ID와 프로필을 유지한다.

`NETBREAK/Art/Setup Mackerel And Tuna`는 두 신규 시트를 각각 48/64px로 분할하고 해당 프로필만 생성·갱신해 정확한 FishData에 연결한다. 기존 정어리 Setup은 정어리만 다룬다. `Validate Fish Sprite Pipeline`은 세 어종의 시트·Import·12개 프레임·프로필·연결과 나머지 어종의 fallback을 검사한다. Setup을 반복해도 Sprite ID와 기존 프로필을 재사용한다. 향후 어종은 `Assets/Art/Fish/<Species>/`에 시트를 두고 같은 이름·방향·Pivot 규칙을 따른다. 셀 크기와 PPU는 실제 화면 검증에 따라 재평가한다.

## 남은 결정

사용자는 Unity Play Mode에서 정어리의 세 방향·반대 방향 flip·4프레임 헤엄, 심각한 방향 jitter 없음, 여러 정어리의 동시 표시와 기존 대비 크기, 다른 어종 fallback, 양방향 Pool 재사용, Pause/선택 상태, 기존 VFX·Resistance·포획·UI와 Run 재시작을 확인했다. Unity 컴파일 정상, Console Error 0이다. 현재 정어리 Prototype 이미지 방향에도 만족한다고 밝혔다.

VS-2B-2에서는 사용자가 고등어·참치의 가로·세로·대각선과 반대 방향 flip, 4프레임 헤엄, 심각한 방향 jitter 없음, 세 어종의 크기 계층·실루엣 구분, 다수 개체·어군 가독성, 적절한 참치 크기를 확인했다. 복어·오징어 fallback, 종간 Pool 재사용 초기화, Pause/선택 상태, 기존 VFX·Resistance·포획·UI 및 Run 재시작 뒤 시각 상태도 정상이다. Unity 컴파일과 전체 EditMode 테스트가 정상이고 Console Error는 0이었다. 고등어·참치 이미지는 현재 아트 방향의 프로토타입으로 승인됐다.

32×32·48×48·64×64 셀, PPU 83, 기본 8 FPS와 현재 팔레트는 승인된 일반 어종 프로토타입의 값으로, 최종 출시용 Art Lock은 아니다. 후속 제작·통합 검증에서 조정할 수 있다. 향후 모든 어종의 프레임 수, 특수어·Boss 규격, Pixel Perfect Camera 정책, 내부 렌더 해상도와 전체 프로젝트 공통 PPU는 미정이다. 대규모 어군 성능과 최종 배경·VFX 적용 뒤의 전체 화면 가독성은 후속 통합 검증 대상이다.
