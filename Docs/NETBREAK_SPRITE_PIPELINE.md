# NETBREAK 물고기 Sprite Pipeline — VS-2B-1 프로토타입

## 상태와 목적

첫 적용 대상은 정어리 한 종이다. 방향별 그림 제작, 재사용 가능한 시각 정의, 풀 재사용 규칙의 자동 검증 163/163과 사용자 Unity 수동 시각 검증을 마쳤다. 정어리 이미지는 후속 일반 어종 제작의 기준으로 사용 가능한 첫 프로토타입으로 승인됐다. 최종 출시용 Art Lock과 전체 어종 규격 확정은 아직 하지 않았다. 게임플레이 이동·충돌·포획과 시각 표현은 분리한다.

## 제작 원칙

- 밝은 청록 바다와 다수 어군에서 머리·몸통·꼬리의 실루엣을 읽을 수 있게 한다. 제한된 팔레트와 짙은 윤곽을 사용하며 배경보다 물고기와 전투 정보가 앞선다.
- 픽셀 격자에서 직접 제작하고, 투명 배경과 불투명 픽셀을 사용한다. 런타임 임의 회전이나 확대 보간으로 방향을 만들지 않는다.
- 정어리 시트는 `Assets/Art/Fish/Sardine/Sardine_Swim.png`, 원본 생성 절차는 `Tools/generate_sardine_sprite.py`다. 현재 128×96 RGBA 시트의 32×32 셀 3행×4열이다. 가로 그림의 실제 실루엣은 셀보다 작고 투명 여백이 있다.
- 32×32, 83 PPU, 8 FPS는 **정어리 프로토타입 값**이다. 공통 PPU는 어종 간 월드 크기 비교와 자산 교체를 쉽게 하지만, 전체 프로젝트에 적용할 값은 실제 화면 검증 후 결정한다.

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

정어리 Custom Visual은 기본 흰색 Tint와 프로필 크기 (1,1)을 사용한다. 기존 사각형용 `FishData.VisualScale`을 그림에 중복 적용하지 않는다. 게임플레이 루트와 CircleCollider2D는 그대로 두고 런타임 시각 전용 자식의 크기를 루트 배율에 맞춰 보정한다. 사각형 원본 렌더러는 정어리일 때만 숨긴다. 프리팹·씬 직렬화는 변경하지 않는다.

매 `FishController.Initialize`에서 현재 FishData로 프로필, Sprite, 프레임, 타이머, flipX/Y, 크기, Tint, 방향, Custom/Fallback 모드를 초기화한다. 정어리→다른 어종 및 반대 순서로 같은 풀 오브젝트가 재사용되어도 이전 시각이 남지 않아야 한다. Collider, Resistance, 보상, 경로와 풀 용량은 시각 시스템이 다루지 않는다.

## 가져오기와 이름

Unity 메뉴 `NETBREAK/Art/Setup Sardine Prototype`은 시트를 Sprite/Multiple, PPU 83, Point, 무압축, Mipmap Off, Full Rect, Clamp, 중심 Pivot으로 가져오고 12개 셀을 분할한다. Unity 6의 Sprite Editor Data Provider API를 사용한다. 이름은 `sardine_horizontal_0..3`, `sardine_vertical_0..3`, `sardine_diagonal_0..3`이다. Unity 이미지 좌표계에 맞춰 맨 위 행부터 연결한다. 프로필을 생성하거나 재사용해 정어리 FishData에만 연결하고 다른 어종은 건드리지 않는다. `NETBREAK/Art/Validate Fish Sprite Pipeline`으로 참조와 설정을 확인한다. 반복 실행 시 기존 Sprite ID와 프로필을 유지한다.

향후 어종은 `Assets/Art/Fish/<Species>/`에 시트를 두고 같은 이름·셀·Pivot 규칙을 기준으로 시도한다. 실제 어종 크기, 셀 수, 프레임 수가 다르면 프로필과 가져오기 설정을 검토한 뒤 확장한다. 시트 제작 → 가져오기 → 프로필 → FishData 연결 → 자동 테스트 → 실제 카메라의 크기·방향·어군 가독성·풀 재사용 시각 확인 순으로 진행한다.

## 남은 결정

사용자는 Unity Play Mode에서 정어리의 세 방향·반대 방향 flip·4프레임 헤엄, 심각한 방향 jitter 없음, 여러 정어리의 동시 표시와 기존 대비 크기, 다른 어종 fallback, 양방향 Pool 재사용, Pause/선택 상태, 기존 VFX·Resistance·포획·UI와 Run 재시작을 확인했다. Unity 컴파일 정상, Console Error 0이다. 현재 정어리 Prototype 이미지 방향에도 만족한다고 밝혔다.

32×32 셀, PPU 83, 8 FPS와 현재 팔레트는 승인된 정어리 프로토타입의 값으로, 최종 출시용 Art Lock은 아니다. 후속 제작·통합 검증에서 조정할 수 있다. 전체 어종 캔버스, Boss 규격, Pixel Perfect Camera 정책, 내부 렌더 해상도와 공통 PPU는 미정이다. 대규모 어군 성능과 최종 배경·VFX 적용 뒤의 전체 화면 가독성은 후속 통합 검증 대상이다.
