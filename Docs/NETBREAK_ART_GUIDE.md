# NETBREAK 아트 스타일 가이드 — VS-2 초안

## Reference Frame Rebuild (2026-09-28, 현재 후보)

- 최신 목표는 첨부 NETBREAK UI의 디자인 재현이다. 단색 중앙과 단색으로 늘어난 긴 테두리를 사용한 이전 후보를 대체한다. 프레임도 아이콘과 같은 픽셀 단위의 재질을 가져야 한다.
- 프레임 6종을 64×64 픽셀로 재제작했다. 중앙에는 낮은 대비의 청록 픽셀 군집, 목재에는 Cream 하이라이트/황토 중간색/짙은 갈색 홈/부분적인 청록 벗겨짐을 남긴다. 12px 고정 모서리에는 실제 가닥이 보이는 로프 감김이 들어간다. Tiled로 그려 중앙과 레일의 픽셀이 늘어나지 않게 한다.
- 별도 로프는 세 가닥의 꼬임과 겹친 접합부를 갖는다. 간판·아이템 프레임·Hotbar 상하 접합부에 걸치며 raycast/레이아웃 공간을 차지하지 않는다. 파도는 반복 산 모양 대신 양 끝 큰 말림과 중앙 포말을 갖는 한 장의 띠다.
- 좌측/시간/준비/아이템/Hotbar/배속/성장에 공통 재질을 적용한다. 성장 내부는 레퍼런스의 Navy, 슬롯은 청록 경계, 뜰채·선택 배속은 Cyan 강조다. 아이콘과 기존 TMP 폰트, 주요 배치는 유지한다.
- 참조와 완전히 동일하다는 승인 기록이 아니다. 실제 Unity 화면 비교·글자 잘림·프레임 정합성은 사용자 확인 대기다. 프롬프트/원본/정적 합성/검증은 Artifacts/MarineUIReference에 보존한다.

## Area 1 Marine Refinement (2026-09-28, 현재 후보)

- 현재 레이아웃을 유지하고 Deep Navy/Teal, Cyan, 목재·로프·조개·산호·야자수 색을 27색으로 통일한다. 기존 ImageGen 도형을 재패킹하며 48px 장식은 24px 논리 그리드로 단순화한다. 완전히 새 레이아웃이나 고해상도 장식 세트로 바꾸지 않는다.
- 32px 프레임은 기존 5px Slice 안에 계단형 곡선 모서리를 닫힌 형태로 넣는다. 중앙 단색/가장자리 일정 단면을 유지한다. 표시 두께는 UI Image multiplier로 줄이되 .meta는 유지한다. header 내부는 짙은 목재, 기본 패널은 Navy/Teal, 조업/성장 버튼은 Sand다.
- NETBREAK 목재 간판은 패널 위로 약간 돌출하며 야자수·갈매기가 양 끝에 걸린다. 경험치 숫자 밑의 얇은 게이지는 읽기 전용이다. 준비 패널의 물고기 음영은 기존 물고기 아이콘을 낮은 불투명도로 재사용한다. 하단 파도는 별도 ImageGen 소재를 타일로 반복하고 양 끝 픽셀을 맞춘다.
- 슬롯 번호는 독립된 어두운 배지/밝은 숫자, Hotbar는 목재 연결 기둥/명확한 끝 로프로 정리한다. 장식은 raycastTarget=false이고 Hotbar 장식은 Layout을 차지하지 않는다.
- 정적 합성은 미술 배치 참고용이다. 실제 TMP·한글·해상도별 프레임/텍스트 겹침과 사용자 레퍼런스 부합 여부는 Unity에서 사용자 승인 대기다. 자세한 파일/검증 결과는 Artifacts/MarineUIRefine/FINAL_REPORT.md 참조.

## Area 1 HUD 39종 Marine Rebuild 후보 (2026-09-28, 승인 대기)

현재 Game View의 배치·크기와 기존 Text RectTransform을 보존하고, 39종 PNG를 내장 ImageGen의 개별 생성 아트로 교체했다. Wood/Rope 재질 프레임과 Deep Navy/Teal 내부, Cyan 선택 경계, Sand 버튼을 공유하며 야자수·갈매기·산호·조개·잎·로프·찌·포말 등 11종 독립 장식과 22종 의미별 아이콘을 재제작했다. 원래 32×32/48×48 규격과 모든 Import/.meta를 유지한다. 9-slice 고정 모서리와 가장자리 단면을 보존·패킹하고 중앙은 불투명 단색으로 정리했다. 제목 바와 키 배지는 기존 작은 높이에서 글자가 읽히도록 얇은 테두리와 어두운 안쪽 여백을 사용한다. 외곽 장식의 위치·수량·크기는 기존 코드 그대로다.

정적 합성 및 원본 대비 시트는 ../Artifacts/MarineUI/에 있다. 실제 게임 화면 검증을 뜻하지 않는다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** 이전 Iteration 4 기록은 이번 재제작 이전 상태다.

## Area 1 HUD 해양 장식 시안 (2026-09-27, 승인 대기)

현재 배치·정보 우선순위를 유지하면서 각 UI의 독립 9-slice 프레임 바깥에 작은 해양 장식을 놓는다. 좌측 정보 HUD는 야자수·갈매기·불가사리·잎/조개, 조업 준비는 야자수·낚시 찌·로프 매듭·산호·파도, 아이템은 잎·상자·조개, 핫바와 성장 버튼은 잎·산호·조개·로프를 사용한다. 시간은 시계/조개만, 배속은 기존 작은 파도 아이콘만 사용한다. 장식은 텍스트·슬롯 아이콘·클릭을 가리지 않고 9-slice Stretch 영역과 분리한다. Tropical Marine Sprite Sheet는 개별 장식 종류와 픽셀아트 색감·밀도 참고로 사용하며 icon.zip의 28개와 순서 매핑하지 않는다. 실제 Unity 작은 Game View에서 표시를 확인했으나 최종 전체 화면의 세부 가독성과 Prototype 승인은 대기한다.

## VS-2D-4 Cast Net Prototype — Unity 수동 검증 완료

일회성 범위 도구는 실제 판정 중심과 반경을 먼저 읽히게 한다. 투망은 기존 커서 원형 표시와 같은 실제 `captureRadius`를 사용하고, 중심의 작은 접힌 Ghost를 투명하게 표시한다. 사용 후에는 64×64 셀/PPU 64의 5프레임 방사형 망이 목표 위치에서 약 0.42초 펼쳐진다. 확장 물결과 실제 피해 Fish의 작은 접촉 표시는 잠깐만 남는다. 지속 설치 Net의 길게 뻗은 Mesh/Rope와 실루엣 및 수명을 구분한다. 플레이어 캐릭터나 투척 원점이 없어서 임의의 비행 경로를 만들지 않았다. 사용자가 Unity Play Mode에서 Q/W 조준 Ghost와 실제 범위의 동위치 추적, 범위 업그레이드, 전개 Animation, 일회성 Area VFX, 실제 피해 대상만의 작은 Hit VFX, Miss와 다중 명중의 가독성을 확인하고 Prototype 품질을 승인했다. **Cast Net Pixel Art/Ghost: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending. Cast/Open Animation: Manual Validation: Passed / Prototype Approval: Approved / Final Production Animation Approval: Pending. Area/Hit VFX: Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.** PPU·셀 크기·5프레임·속도·VFX 수명은 최종 출시 규격이 아니다.

## VS-2D-3 Scoop Net Prototype — Unity 수동 검증 완료

직접 사용형 Tool은 몸체 장식보다 실제 공격 위치와 범위의 가독성을 먼저 맞춘다. 뜰채는 Scene의 원형 범위와 커서 월드 좌표를 그대로 사용하고, 약한 Range 원·Ready 실루엣을 사용 가능할 때만 표시한다. 기존 카메라 orthographic size 6.5, 공격 반경 1.1과 어종 32~64px 셀의 상대 크기를 보고 48×48 셀/PPU 64(0.75 world unit)의 손잡이·타원형 망 실루엣을 첫 후보로 정했다. PPU 64나 83은 Tool 전체 최종 규격이 아니다.

5프레임 Swing은 픽셀을 임의 각도로 회전시키지 않고 망을 커서 주변에서 짧게 이동시킨다. 공격은 원형이므로 별도 방향별 시트를 제작하지 않는다. 실제 Resistance가 감소한 Fish 위치마다 24×24 작은 Hit 물보라를 표시하고, 다중 타격에도 공용 VFX Pool의 RepeatedHit 우선순위와 상한을 따른다. Fish를 가리는 큰 효과를 늘리지 않는다. 사용자가 Play Mode에서 Ready Sprite·커서 추적·실제 공격 범위와 일치하는 Range Feedback, 화면 혼잡도, Hit/Miss 구분, 단일·다중 Hit VFX, Ready→Swing→Ready 복귀와 방향 독립 Swing을 확인했다. **Scoop Net Sprite: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending. Scoop Net Swing: Manual Validation: Passed / Prototype Approval: Approved / Final Production Animation Approval: Pending. Scoop Net Hit VFX: Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.** 현재 셀 크기·PPU·5프레임·animation duration·VFX size는 최종 출시 확정값이 아니다.

## VS-2D-2 Net Prototype — Unity 수동 검증 완료

실제 그물은 선분을 중심으로 한 두께 0.3의 회전 직사각형이다. 16×16 반투명 Mesh Tile과 16×4 Rope Tile을 길이 방향으로 반복해 범위를 읽히게 하며, Ghost와 설치물은 동일한 authoritative 시작점·끝점·두께·최대 길이와 배치 변환을 공유한다. 한 장의 그림을 길이에 맞춰 늘이지 않는다. 물고기를 가리지 않는 성긴 격자, Cyan 바다와 구분되는 밝은 로프, 조용한 Mesh 알파 변화를 사용한다. 지속 Tool의 접촉 VFX는 첫 영향 등록에만 짧게 표시하고 매 물리 Tick에는 반복하지 않는다. 비작동 중에는 기존 어두운 상태가 우선한다.

사용자가 Play Mode에서 Q/W 배치 진입, 시작점 Preview, 드래그 Ghost와 실제 Net의 위치·길이·두께·각도 일치, Pixel Art 가독성, 첫 접촉 VFX, 과다 반복 없음, 복어 중단의 어두운 상태·Active visual 정지·복귀, 다중 Net 독립 상태와 기존 연출 회귀를 확인했다. **Net Pixel Art: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending. Net Placement Preview: Manual Validation: Passed / Prototype Approval: Approved. Net Contact VFX: Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.** 현재 길이·두께·animation 속도·VFX lifetime은 최종 출시 확정값이 아니다.

## VS-2D-1 낚싯대 Tool Presentation Prototype

낚싯대는 장식보다 설치 도구로 읽히는 실루엣을 우선한다. 32×32 Idle 본체에 받침대·기울어진 대·릴·낚싯대 끝을 분리해 그렸고, 공격 본체 3프레임은 짧은 휨만 보여준다. 공격 대상 정보는 본체와 분리된 얇은 줄 및 대상 위치의 16×16 Hook/Splash Hit으로 전달한다. 줄은 사거리 원과 구별되도록 공격 성공 직후에만 0.18초 표시한다. 설치 Preview에는 본체 Idle을 재사용한다.

기존 프리팹 루트 Scale 0.35와 화면 footprint 약 29px를 보존하기 위해 본체 PPU 32를 선택했다. Hit PPU는 현재 물고기·VFX와 같은 83이다. Tool 몸체 PPU·셀 크기·Pixel Perfect Camera는 최종 규격이 아니며, 다른 Tool에 적용할 때 실제 화면 크기와 픽셀 밀도를 재검토한다. 사용자가 Play Mode에서 Pixel Sprite, 공격 애니메이션·줄·Hit VFX와 Ghost·Range 커서 추적을 확인했다. **Fishing Rod Sprite: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending. Attack Presentation·Line 및 Hit VFX 각각: Manual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending. Placement Preview: Manual Validation: Passed / Prototype Approval: Approved.**

기준일: 2026-09-22. 이 문서는 VS-2의 아트 기획 기준이다. 게임 설계는 [NETBREAK_DESIGN.md](NETBREAK_DESIGN.md), 구현·검증 상태는 [NETBREAK_STATE.md](../NETBREAK_STATE.md), 제작 순서는 [NETBREAK_ROADMAP.md](NETBREAK_ROADMAP.md)를 따른다.

## 1. 문서 상태

- **확정:** 1차 아트 방향과 가독성 원칙.
- **권장 초안:** 요소별 표현 방향과 첫 프로토타입 범위.
- **미정:** 전체 프로젝트의 최종 픽셀·카메라·애니메이션·팔레트 제작 규격.
- **검증 완료:** 정어리·고등어·참치 프로토타입의 Unity 수동 시각 검증과 승인. 공통 PPU 83에서 서로 다른 셀 크기로 상대 크기와 어군 가독성을 확인했다.
- **검증 완료:** VS-2B-3 복어·오징어 방향별 수영 시트와 FishData 연결. 사용자 Unity 컴파일 정상, Console Error 0, 전체 EditMode 173/173 통과, Play Mode 수동 시각 검증을 확인했다. 두 어종의 프로토타입 승인은 완료했고 최종 출시용 아트 승인은 대기 중이다.
- **현재 프로토타입 검증 완료:** VS-2B-4에서 다섯 어종의 원본 12프레임을 픽셀 단위로 보존하고 NW 4프레임을 더해 16프레임으로 확장했다. E/N/NE/NW 네 원본 축과 반대 방향의 flipX+flipY를 사용자가 Play Mode에서 확인했다. Unity 컴파일 정상, Console Error 0, 전체 EditMode 183/183 통과와 다섯 어종의 수동 시각 검증을 마쳤다. 위 VS-2B-1~3 승인 수치는 이전 12프레임 버전의 역사적 기록이다. 최종 출시용 아트 승인은 Pending이다.
- **VS-2C-1 프로토타입 검증 완료:** 오징어 먹물 공격 Animation/VFX/SFX를 사용자가 Unity에서 확인하고 승인했다. 컴파일 정상, Console Error 0, 전체 EditMode 190/190 통과. 최종 출시용 Art/VFX/Audio 승인은 Pending이다.
- **VS-2C-3 프로토타입 검증 완료:** 복어 그물 중단 순간 애니메이션과 Impact VFX를 사용자가 Play Mode에서 확인하고 승인했다. 컴파일 정상, Console Error 0, 전체 EditMode 200/200 통과. 최종 출시용 Art/VFX 승인은 Pending이다.
- **검증 필요:** 전체 성능, 최종 배경·VFX와의 통합 화면, 폰트와 외부 에셋 라이선스.
- **미완료:** 그 외 정식 에셋 제작·적용, 정식 VFX·SFX·BGM 및 최종 출시용 Art Lock.

## 2. 아트 콘셉트

확정 방향은 **밝고 읽기 쉬운 탑다운 픽셀아트**다. 핵심 키워드는 밝은 바다, 청록 계열, 픽셀아트, 귀여운 해양 생물, 가독성 높은 대규모 어군, 판타지로 확장 가능한 분위기다. 간결한 스타일을 기본으로 하고 특수어·MiniBoss·Boss·주요 전투 연출에는 상대적으로 풍부한 디테일을 포인트로 사용한다.

초반 연안은 밝고 친숙한 어업 분위기로 제안한다. 미래 판타지 해역도 동일한 픽셀 표현 체계를 유지하되 Area 2~6의 최종 배경 스타일은 이번 단계에서 확정하지 않는다. 콘셉트 이미지의 배·섬·미니맵·등대·UI 배치·신규 장비는 스타일 참고이며 확정 콘텐츠가 아니다.

## 3. 화면 시각 계층

월드 전투 정보의 계층은 `물고기·특수어 → 도구·설치물 → 상태 이상·핵심 전투 피드백 → 바다 배경·장식` 순으로 설계한다. HUD와 성장 정보는 월드 계층과 별개로 항상 읽을 수 있어야 하며 배경보다 낮은 가독성을 허용한다는 뜻이 아니다. 색상만으로 역할을 구분하지 않고 실루엣, 크기, 명암과 움직임을 함께 사용한다.

## 4. 바다 및 지형

중앙 전투 구역은 시각적 복잡도를 낮추고 물고기와 제작자 작성 경로가 잘 보이게 한다. 청록 계열 안에서 적당한 색상 변화와 물결을 사용하되 강한 고주파 디테일은 피한다. 가장자리에는 암초, 수초, 바위, 얕은 바다 디테일을 둘 수 있다.

배경 장식이 설치 가능 영역이나 실제 이동 경로를 바꾼 것처럼 보이면 안 된다. 현재 구현되지 않은 지형 판정은 아트로 암시하더라도 구현 기능으로 기록하지 않는다.

### Area 1 고품질 정적 배경 Prototype (2026-09-27, 사용자 수동 검증 완료)

현재 후보는 사용자 첨부의 1672×941 고품질 탑다운 바다 이미지를 경로에 맞춰 국소 편집한 단일 불투명 PNG다. 세밀한 청록색 수면광·해저 깊이감·픽셀아트 질감을 유지하고, 큰 암초·바위·해초·산호와 강한 모래는 실제 Fish/Boss 동선을 피한다. 특히 좌상단→우하단 일반 어군/보스 1차, 좌하단→우상단 보스 2차, 좌측→우측 지그재그 보스 3차를 보호한다. 요청된 우상단 Spawn·좌하단 Exit와 역대각선도 열린 바다로 둔다. 중앙은 비교적 깨끗하고 모래는 상·하단 일부의 물 아래 해저 포켓으로만 보인다.

기존 Resources 로드·Main Camera 자식 SpriteRenderer(-1000)·중복 방지·카메라 맞춤은 유지하고, 직전 미검증 반사선 오버레이는 정적 평가에서 제거했다. Sprite Single/Point/무압축/Mipmap Off/PPU 32/Max Size 2048이며 원본 이미지를 저해상도로 축소하지 않는다. 사용자가 실제 Play Mode에서 밝기와 Fish/Tool/VFX 가독성, Spawn/Exit, 일반 Fish·Boss 경로와 지형의 시각적 조화를 확인하고 현재 Vertical Slice용 Prototype으로 승인했다. 배경은 계속 Fish/Tool/VFX보다 낮은 시각 우선순위를 유지한다. **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending.**

약간의 이질감·정적인 느낌의 원인은 아직 확정하지 않았다. 배경 움직임 부족, 세부 Art Density 차이, 수면 패턴·반복감, UI와 다른 아트 완성 후의 통합 화면 조화는 후반 Polish 검토 후보일 뿐이다. 단일 PNG는 현재 Prototype 방식이며 최종 구조는 미정이다. 단일 PNG 유지, 레이어 분리, Water Base·Underwater Detail·Decoration 분리, 미세한 수면 Animation/Overlay 중 어느 방식을 쓸지도 확정하지 않는다. 아래 이전 배경 톤·흐름 기록은 역사적 자료다.

### Area 1 선택 배경 톤·흐름 개선 (이전 후보 기록, 2026-09-27)

사용자가 선택한 탑다운 바다 시안의 구도는 유지하되, 물색을 차분한 터콰이즈로 낮추고 연결된 밝은 다각형 격자 무늬를 낮은 대비의 짧고 불규칙한 물결로 정돈했다. 이전 PNG 대비 평균 휘도는 약 12% 감소했다. 수중 큰 군집은 네 곳에서 두 곳으로 줄이고 상단·하단에 작은 바위만 드물게 남겼다. 모래는 가장자리 일부의 **물 아래 해저**로만 보이며 중앙을 넓게 덮거나 해변 띠를 만들지 않는다. Fish·Tool·Range·VFX·UI보다 배경이 조용해야 한다.

우상단 Spawn Zone, 좌하단 Exit Zone 및 두 지점 사이 대각선은 요청한 아트 보호 구역이다. 실제 `Main.unity`의 활성 `CoastRoute_01`은 현재 좌상단 `(-10, 8)`에서 우하단 `(10, -8)`으로 향하며, `FishSpawner`의 일반 어군도 카메라 왼쪽에서 생성한다. 따라서 실제 반대 대각선도 함께 열어 둔다. Boss 2차의 좌하단→우상단과 3차의 좌측→우측 이동 구간에는 큰 모래띠·암초·바위·해초를 배치하지 않는다. 이는 배경 배치 원칙이며 경로·Spawn·Boss Gameplay 수정이 아니다.

현재 파일은 1671×941 불투명 RGB PNG다. Sprite Single/Point/무압축/Mipmap 없음/PPU 32를 유지하고 기존 Resources 로드와 카메라 자식 SpriteRenderer(-1000)를 재사용한다. 같은 컨트롤러가 -999 뒤쪽 계층에 짧은 투명 반사선 오버레이를 한 번 만들고 최대 0.09×0.06 world unit만 천천히 움직인다. 이 오버레이는 scaled time으로 Pause 중 정지하며 초기화·파괴 시 정리된다. Collider나 게임플레이 판정은 없다. 단일 PNG와 이 약한 오버레이는 현재 Prototype 검증 방식이며 최종 Production Background 구조는 미확정이다. 이전 절차형 생성기는 `Tools/CoastBackground_ProceduralDraft.png`로만 출력하여 선택 배경을 덮어쓰지 않는다.

현재 배경은 **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production Art Approval: Pending**이다. Unity Play Mode에서 밝기·격자감·수면 흐름의 강도, 수중 지형·양쪽 대각선 및 Boss 회유, 물고기·도구·VFX·UI 가독성, Pause·Run 재시작 후 중복·잔상을 사용자가 확인한다. 자동 검사 결과와 전체 파일 목록은 [NETBREAK_ASSET_MANIFEST.md](NETBREAK_ASSET_MANIFEST.md)와 `../NETBREAK_STATE.md`에 기록한다.

## 5. 물고기 스타일

- **정어리:** 작고 단순하며 군집에서도 읽히는 실루엣.
- **고등어:** 기준형 중간 크기의 물고기 실루엣.
- **참치:** 상대적으로 길고 크며 강한 형태.
- **복어:** 둥글고 팽창성이 드러나는 형태.
- **오징어:** 몸통과 촉수로 즉시 구분되는 형태.
- **MiniBoss/Boss:** 현재 구현된 특수 유형이며 일반어와 크기·형태·대비에서 구분한다. Boss의 3회 회유 등 실제 기능은 유지한다.

위 목록은 전체 어종의 제작·교체 완료를 뜻하지 않는다. 대규모 어군에서도 개체와 역할을 읽을 수 있어야 한다.

VS-2B-2에서 고등어·참치 방향별 프로토타입을 정어리의 픽셀 윤곽·제한된 청록 팔레트·어두운 등/밝은 배·작은 눈·4프레임 꼬리 리듬에 맞춰 제작하고 FishData에 연결했다. 고등어는 두꺼운 중간 몸체와 짧은 등 무늬, 참치는 깊고 긴 몸체·좁은 꼬리자루·갈라진 꼬리로 실루엣을 구별한다. 사용자가 Unity 플레이에서 세 어종의 크기 계층·실루엣·방향·반전·헤엄·어군 가독성과 기존 기능을 확인하고 고등어·참치 이미지를 현재 아트 방향의 프로토타입으로 승인했다.

VS-2B-3 당시 복어는 48×48 셀에서 일정한 둥근 몸통과 짧은 꼬리·지느러미, 밝은 황금색 배와 짙은 윤곽으로 구별했다. 오징어는 64×64 셀에서 뾰족한 몸통과 분리된 촉수, 연보라·산호색 면과 같은 짙은 윤곽을 사용했다. 당시 각 3방향×4프레임, 공통 PPU 83·8 FPS의 수영 프로토타입을 사용자가 Unity에서 수동 검증해 **프로토타입으로 승인**했다. 현재는 두 어종에도 NW 4프레임을 추가했다. VS-2C-1에서 오징어 먹물 공격 전용 4방향×4프레임 Prototype을 별도 시트로 제작했다. VS-2C-3의 복어 접촉 반응도 Prototype으로 승인했지만 지속 팽창 gameplay는 구현하지 않았고 최종 출시용 아트 승인은 남아 있다.

## 6. 도구 및 설치물

현재 입력은 `LMB 뜰채`, `Q Core Tool`, `W Partner Tool`, `E Tactical Skill`, `R Signature Skill`이다. Q/W/E/R 아이콘을 특정 도구에 영구 결합하지 않고 실제 Run 소유·장착 상태를 표시한다.

- **낚싯대:** 설치 위치와 방향성.
- **그물:** 길이와 봉쇄 범위.
- **투망:** 조준 범위와 확산.
- **뜰채:** 직접 개입 범위.
- **미끼:** 어군 유도와 압축 효과.

장식보다 기능과 사용 위치를 먼저 읽히게 한다. 이 기획만으로 새 도구나 배·항구 시스템을 추가하지 않는다.

## 7. VFX 스타일

UX-F1/F2의 실제 이벤트 연결과 임시 연출을 정식 제작의 기반으로 사용한다. 전기는 뾰족한 번개와 방전, 검은 짧고 날카로운 참격 궤적, 얼음은 결정·서리 문양·파동으로 구분한다. 복합 시너지는 두 속성의 결합이 형태에서도 드러나게 한다.

정식 픽셀 VFX는 기존 실제 전투 이벤트와 대상 판정을 보존한다. 전투 판정을 VFX로 재구현하지 않는다. 다수 효과 동시 발동 시 구분, HUD 가독성, 잔상과 성능은 실제 Unity 화면에서 다시 검증한다.

특수어 행동은 Swim과 별도 Animation Set으로 표현한다. 게임플레이 성공이 먼저 발생하고 특수 프레임과 VFX/SFX는 이를 보여준다. 공격 중에는 시작 방향을 잠그고 종료 즉시 현재 이동 방향의 Swim으로 복귀한다. VS-2C-1 오징어 먹물은 기존 몸통·눈·윤곽·팔레트를 유지한 64px 셀의 Anticipation→Release→Recovery 4프레임이다. 발사 원인은 오징어 부근의 짙은 검보라 픽셀 구름으로, 기존 도구 방해 상태 표시와 역할을 구분한다. 사용자가 Unity에서 방향·Swim 복귀와 VFX를 수동 검증하고 현재 Prototype 품질을 승인했다. **Ink Attack Manual Visual Validation: Passed / Ink VFX Manual Validation: Passed / Prototype Approval: Approved / Final Production Art·VFX Approval: Pending**이다.

VS-2C-2는 성공한 먹물 판정 뒤 Attack animation → 기존 Release 구름·SFX → 16px 검보라 먹물 덩어리 → 대상의 24px 짧은 튐 → 기존 지속 방해 상태 표시 순서로 연결한다. 과거 번개형 대상 연결선과 즉시 타격 마커는 이 경로에서 교체됐으며 Play Mode에서도 중복 표시되지 않았다. Projectile은 발사 시점에 이미 판정된 어구 위치로 현재 Prototype 기준 0.22초 직선 이동하고, Impact는 0.3초 표시된다. 충돌·대상 재검색은 없다. 오징어 중심에서 시작하는 단순한 방식을 택했다. 네 방향축의 시각 소켓이 없고 짧은 이동 거리에서 방향별 추정 오프셋이 오히려 틀린 위치를 가리킬 수 있기 때문이다. 사용자가 Play Mode에서 실제 gameplay 대상 연결, 이동 속도·가독성, 도착 시 Impact, 이후 지속 방해 상태, 다중 오징어·대상 누락·Pause·Pool/Run 재시작을 확인했다. Gameplay 판정·타겟 선정·방해 지속시간은 변경하지 않았다. **Ink Projectile 및 Ink Impact 각각 Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.** 현재 이동시간·크기·Pool priority는 최종 출시 확정값이 아니다.

VS-2C-3 복어 연출은 **실제 활성 그물 접촉으로 기존 3초 중단이 먼저 성공한 경우**에만 재생한다. 48px 복어 몸체를 두 프레임 동안 조금 키워 가시·외곽의 접촉 반응을 보여준 뒤 현재 방향 Swim으로 복귀한다. 이는 순간 반응이며 지속 팽창·가시 피해·경고 행동을 뜻하지 않는다. 접촉점의 24px 노랑·물색 Impact는 중단 원인을 짧게 알리고, 기존 그물 어두워짐은 중단 지속 상태를 알린다. 사용자는 Play Mode에서 실제 중단 판정과 애니메이션·Swim 복귀·Impact, 지속 접촉의 중복 방지, Pause·Pool 재사용·Run 재시작 및 기존 전투 흐름을 확인했다. Net 중단 시간·판정·감속·포획·Resistance 수치는 변경하지 않았다. 4방향축×4프레임·12 FPS·약 0.33초 및 0.28초 Impact는 최종 출시 확정값이 아니다. **Puffer Disrupt Sprite: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending. Puffer Net Impact VFX: Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production VFX Approval: Pending.**

## 8. UI 및 폰트

Area 1 HUD Visual Iteration 3은 사용자가 선택한 두 번째 콘셉트의 Deep Teal/Navy 패널·얇은 Cyan 경계·큰 아이콘을 기준으로 한다. 좌우 패널을 화면 모서리 20px 여백에 배치하고 정보 256×254, 아이템 388×132, 핫바 716×120으로 한국어 가독성을 회복했다. 정보 행은 아이콘·라벨·값 열을 분리하고 Content Mask와 TMP 줄바꿈 해제·잘림 제한을 적용한다. 바깥 패널에만 작은 목재·로프 강조를 두고 슬롯은 어두운 해양 프레임으로 처리한다. `panel`, `slot`, `selected_slot`, `header`, `button`, `key`의 독립 9-slice Sprite와 정보·도구·아이템 아이콘을 재사용한다. 기존 Drag/Canvas Clamp와 Tooltip/입력 차단, HUD 정보 의미를 유지한다. 초기 모서리 배치가 Fish/Boss 경로를 실제 Game View에서 얼마나 가리는지는 사용자가 확인해야 한다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.**

Visual Iteration 2는 사용자 Game View에서 좌우 패널이 화면 안쪽에 있고 글자가 작거나 프레임과 겹치며 작은 목재 테두리가 과도하다고 확인되어 승인되지 않았다. 당시 정보 225×232, 아이템 255×79, 핫바 602×84의 축소 배치는 역사적 기록이다.

픽셀아트와 일관된 프레임·아이콘을 사용하되 한국어 가독성을 우선한다. 기존 Canvas, TextMeshPro, Button 구조와 동적 Q/W/E/R 슬롯, 성장 관리·아이템·툴팁의 정보량을 유지한다.

현재 적용 폰트는 **NanumGothic-Bold SDF, Dynamic atlas**다. 정식 픽셀아트 UI의 우선 후보는 **갈무리 9(Galmuri 9)**이며 HUD, 버튼, 숫자, 짧은 라벨, 탭, 간단한 타이틀부터 검토한다. 장문은 실제 해상도, 작은 크기, 숫자·영문 혼합, 한글 글리프, TMP 품질, 확대 방식, 줄바꿈과 잘림을 검증한 뒤 결정한다. 갈무리 9 하나로 모든 텍스트를 통일한다고 확정하지 않는다.

갈무리 9는 아직 임포트하지 않았고 TMP Font Asset도 만들지 않았다. 기존 NanumGothic을 제거하거나 UI를 교체하지 않았다. 공식 배포처의 라이선스 문서에서 상업 이용, 게임 패키지 포함과 재배포 조건을 확인하기 전에는 검증 완료로 표시하지 않는다.

## 9. 제작 규격 — 미확정

다음 항목은 현재 Camera, Sprite Importer, 기존 Sprite 크기·PPU와 UI Scaling을 확인한 뒤 정한다.

- 일반 물고기·특수어·Boss Sprite 캔버스 크기
- Pixels Per Unit과 Pixel Perfect Camera 도입 여부
- Camera Orthographic Size와 최종 내부 렌더링 해상도
- 픽셀 확대·필터링과 UI 아이콘 규격
- 픽셀 폰트 표시 크기와 애니메이션 프레임 수
- 최종 색상 팔레트

현재 정어리·고등어·참치 프로토타입에는 각각 32px·48px·64px 셀과 4방향×4프레임 구조를 사용한다. 이전 3방향×4프레임 버전의 상대 크기 검증은 역사적 기록이며, 이번 16프레임 버전의 방향·수영 표현도 사용자가 Unity에서 확인했다. PPU 83, 기본 8 FPS, 셀 크기, Pixel Perfect Camera 정책, 다른 어종과 Boss 규격은 전체 프로젝트의 최종 확정값이 아니다.

복어 48px·오징어 64px 셀도 같은 프로토타입 Import 규격을 사용했다. 사용자가 Unity에서 두 특수어와 기존 어종의 동시 표시·가독성을 확인했다. 이 결과가 전체 프로젝트의 PPU·셀 크기·FPS 정책을 최종 확정하지는 않는다.

## 10. 에셋 소싱 및 관리

직접 제작, 생성형 이미지 콘셉트 초안, 무료·유료 게임 에셋 활용, 픽셀 단위 보정과 애니메이션 제작을 후보로 둔다. 각 에셋은 이름·용도, 출처, 제작자, 취득일, 라이선스, 상업 이용, 수정 허용, 재배포 조건과 원본 위치를 기록한다.

저장소가 공개될 수 있으므로 유료·제한 라이선스 원본을 공개 Git 저장소에 포함할 수 있는지 도입 전에 확인한다. 확인하지 않은 에셋이나 폰트를 CC0 또는 상업 이용 가능으로 단정하지 않는다.

## 11. 첫 아트 프로토타입

첫 스타일 검증 후보는 일반 물고기 1종, 오징어 1종, 낚싯대 1종, 바다 배경 1종과 기본 UI 아이콘 일부다. 이는 Area 1 전체 확정 수량이 아니다.

적용 뒤 실제 카메라에서의 크기, 밀집 실루엣, 배경 대비, 도구와 물고기 구분, 픽셀 크기 일관성, 기존 VFX와의 조화, 주요 해상도 가독성과 다수 물고기 성능을 확인한다. 정어리는 VS-2B-1, 고등어·참치는 VS-2B-2에서 제작·적용하고 각각 수동 시각 검증과 프로토타입 승인을 마쳤다. 다른 후보 에셋과 전체 화면·성능 검증은 후속 작업이다.

VS-2B-3에서 복어·오징어 수영 시트를 추가하고 사용자가 [Sprite Pipeline 문서](NETBREAK_SPRITE_PIPELINE.md)의 주요 Play Mode 항목을 확인했다. 기존 먹물 방해 Gameplay도 정상이다. VS-2C-1의 먹물 공격 Prototype 애니메이션·VFX·SFX는 Unity 수동 검증과 프로토타입 승인을 마쳤다.

## 12. 후속 제작 순서

1. 기존 Unity 화면과 Sprite 규격 조사
2. 실제 사용 가능한 제작 규격 후보 선정
3. 일반 물고기 대표 1종 제작
4. 오징어와 낚싯대 시안 제작
5. 바다 배경 시안 제작
6. Unity 소규모 적용 및 검증
7. 가이드 보정
8. Area 1 나머지 에셋 순차 제작

제작 전에 전체 에셋 수량을 임의로 확정하지 않는다.

## 13. VS-2B-1 물고기 Sprite 첫 프로토타입

VS-2B-1 당시 정어리 한 종을 대상으로 3방향(동·북·북동) × 4프레임 시트를 제작했다. 이 문단은 첫 프로토타입의 역사적 기록이며 현재는 다섯 어종 모두 4방향축 16프레임이다. 그림을 런타임에 임의 각도로 회전하지 않고 방향별 픽셀 표현을 선택한다. 밝은 바다와 밀집 어군에서도 읽히도록 제한된 색과 명료한 윤곽을 사용한다.

사용자가 Unity에서 정어리의 가로·세로·대각선과 반대 방향 flip, 4프레임 헤엄, 방향 전환, 동시 표시·화면 크기, 다른 어종 fallback, 양방향 Pool 재사용, Pause/선택 상태, 기존 전투·UI와 Run 재시작을 확인했다. Console Error는 0이었다. 정어리 이미지는 **후속 일반 어종 제작의 기준으로 사용 가능한 첫 프로토타입으로 승인**됐다.

32×32 셀, PPU 83, 8 FPS는 이 정어리의 **프로토타입 설정**이다. 최종 출시용 Art Lock이나 전체 어종의 최종 캔버스·PPU·애니메이션·Pixel Perfect 정책 확정은 아니다. 제작·가져오기·방향·풀 재사용 세부 사항은 [NETBREAK_SPRITE_PIPELINE.md](NETBREAK_SPRITE_PIPELINE.md), 출처와 승인 상태는 [NETBREAK_ASSET_MANIFEST.md](NETBREAK_ASSET_MANIFEST.md)를 따른다.

## 14. VS-2B-4 현재 방향별 아트 규칙

| 어종 | 기존 시트 → 현재 시트 | 셀 | 현재 프레임 |
|---|---|---|---:|
| 정어리 | 128×96 → 128×128 | 32×32 | 16 |
| 고등어 | 192×144 → 192×192 | 48×48 | 16 |
| 참치 | 256×192 → 256×256 | 64×64 | 16 |
| 복어 | 192×144 → 192×192 | 48×48 | 16 |
| 오징어 | 256×192 → 256×256 | 64×64 | 16 |

위에서 아래로 E, N, NE, NW 원본 4행이며 각 행은 기존 4프레임 리듬을 유지한다. W=E, S=N, SW=NE, SE=NW의 해당 프레임에 flipX+flipY를 함께 적용한다. 동쪽에서 배가 아래인 그림의 북쪽 원본은 배가 오른쪽이므로, 남쪽을 180도 뒤집으면 배가 왼쪽이 된다. 새 NW 행은 기존 도형과 색을 같은 픽셀 생성식으로 북서 방향에 그렸다. 기존 첫 3행의 RGBA 해시는 다섯 시트 모두 이전과 일치한다. 사용자는 Play Mode에서 S의 왼쪽 배색, E→SE→S의 자연스러운 배/등 연결, NE/NW 구분과 다섯 어종의 방향·4프레임 수영을 확인했다. **Manual Visual Validation: Passed / Prototype Approval: Approved / Final Production Art Approval: Pending**이다.
