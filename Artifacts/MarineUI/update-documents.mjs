import fs from 'node:fs';
import path from 'node:path';
const root=path.resolve(import.meta.dirname,'../..');
const sections={
'NETBREAK_STATE.md':`## Area 1 HUD/UI Marine Rebuild — 39 PNG 후보 적용 (2026-09-28, 사용자 검증 대기)

- 시작 자료는 current_ui_39.zip이다. 프로젝트 PNG 39개와 전부 SHA-256이 일치했으며, 32×32 프레임 6개/아이콘 22개와 48×48 장식 11개로 조사했다. 39개 모두 정적 코드 참조가 있고 미참조 파일은 없다. 아이템 6종은 보유 시 표시 경로이며 정상 Area 1의 아이템 지급 규칙은 변경하지 않았다.
- 내장 ImageGen으로 39종을 역할별로 개별 제작했다. 생성 한도로 중단됐던 마지막 key도 재개 후 완료했다. Tropical Marine Sprite Sheet의 임의 순서 절단/매핑은 하지 않았다. 목재·로프 프레임, Cyan 선택 경계, Sand 버튼, 입체적 해양 장식과 도구/아이템/정보 실루엣을 갱신했다. 실제 UI 문자열은 PNG에 넣지 않았다.
- 기존 규격으로 최근접 샘플링하고 Alpha를 0/255로 정리했다. 프레임은 생성물의 고정 모서리·가장자리 단면을 재사용하면서 중앙을 불투명 단색, 늘어나는 가장자리를 일정한 단면으로 패킹했다. 짧은 제목 바/키 배지는 글자 영역을 확보하도록 테두리 소재를 더 얇게 패킹했다. 6종 Sprite Border 사방 5px, 나머지 0과 모든 .meta/GUID/Import 설정을 바이트 단위로 보존했다.
- Assets/Resources/UI/Area1/의 39 PNG를 실제 교체했다. 신규 Unity Asset 0개. 세션 시작 해시와 비교해 UI/Gameplay 코드, Anchor/Position/Size/Text RectTransform, DraggableHudPanel, Scene/Prefab, 입력/Inventory/Tooltip/Growth/Run/Fish/Boss/Tool/Item/Synergy는 변경하지 않았다. 기존 미커밋 변경은 보존했다.
- Static Validation: 39/39 PNG CRC·압축 데이터·RGBA8·투명도·원래 규격·이름·경로·변경 해시 및 39/39 메타데이터 보존, 6/6 프레임 Stretch 안전성 통과. 최종 ZIP의 39 PNG는 프로젝트 적용본과 모두 동일하다. 정적 합성 미리보기는 Unity 화면이나 TMP 렌더링/상호작용 검증이 아니다.
- 결과: Artifacts/MarineUI/NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip, ASSET_AUDIT.md, FINAL_REPORT.md, UNITY_CHECKLIST.md, PNG_BEFORE_AFTER.png, HUD_STATIC_PREVIEW.png. 개별 생성 프롬프트와 원본 경로는 generation-manifest.json에 있다. 이전 Tools/generate_area1_hud_art.js는 이번 ImageGen 후보를 재현하는 생성기가 아니며 이번 작업에서 수정/실행하지 않았다.
- 사용자 요청에 따라 Computer Use, Unity 실행/Import/compile/Console/PlayMode/Test Runner/Batch, 임시 프로젝트 복사, Git add/commit/push는 하지 않았다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** 목표 Reference와의 충분한 유사성 및 실제 가독성/기능은 사용자 Unity 검증 전이다.
`,
'Docs/NETBREAK_ART_GUIDE.md':`## Area 1 HUD 39종 Marine Rebuild 후보 (2026-09-28, 승인 대기)

현재 Game View의 배치·크기와 기존 Text RectTransform을 보존하고, 39종 PNG를 내장 ImageGen의 개별 생성 아트로 교체했다. Wood/Rope 재질 프레임과 Deep Navy/Teal 내부, Cyan 선택 경계, Sand 버튼을 공유하며 야자수·갈매기·산호·조개·잎·로프·찌·포말 등 11종 독립 장식과 22종 의미별 아이콘을 재제작했다. 원래 32×32/48×48 규격과 모든 Import/.meta를 유지한다. 9-slice 고정 모서리와 가장자리 단면을 보존·패킹하고 중앙은 불투명 단색으로 정리했다. 제목 바와 키 배지는 기존 작은 높이에서 글자가 읽히도록 얇은 테두리와 어두운 안쪽 여백을 사용한다. 외곽 장식의 위치·수량·크기는 기존 코드 그대로다.

정적 합성 및 원본 대비 시트는 ../Artifacts/MarineUI/에 있다. 실제 게임 화면 검증을 뜻하지 않는다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** 이전 Iteration 4 기록은 이번 재제작 이전 상태다.
`,
'Docs/NETBREAK_ASSET_MANIFEST.md':`## Area 1 HUD 39 PNG 전면 재제작 후보 (2026-09-28, 현재 적용·승인 대기)

Assets/Resources/UI/Area1/의 39 PNG 전체를 교체했다. 프레임 6개와 아이콘 22개는 32×32, 기존 장식 11개는 48×48이다. 신규 Unity Asset은 없다. current_ui_39.zip과 일치했던 시작본 39개 모두 새 해시로 바뀌었고, 기존 파일명·경로·39개 .meta/GUID/PPU/Point/무 Mipmap/무압축 및 Border(프레임 사방 5px, 나머지 0)는 보존됐다. 파일별 실제 연결·역할·GUID·원본 ZIP 비교는 ../Artifacts/MarineUI/ASSET_AUDIT.md와 asset-audit-before.json에 있다.

제작은 내장 ImageGen의 파일별 개별 생성이며 CLI/API 대체는 사용하지 않았다. 프롬프트/원본 경로는 generation-manifest.json, 원래 규격의 최근접 샘플링·hard alpha·9-slice 패킹은 export-candidates.ps1에 기록했다. 이전 Tools/generate_area1_hud_art.js는 이번 후보의 원본 생성기가 아니다. PNG CRC·RGBA8·크기·알파·메타 보존·참조 및 6종 Stretch 영역 검사가 통과했고 ZIP 39개는 적용 PNG와 모두 같은 해시다. 최종 ZIP/비교 시트/정적 합성/검증 JSON/33개 사용자 확인 항목은 ../Artifacts/MarineUI/에 있다. **Manual Visual Validation: Pending / Prototype Approval: Pending / Final Production UI Approval: Pending.** Unity 실행·컴파일·Console·Play/Tests 검증은 이번에 수행하지 않았다.
`
};
for(const [relative,section] of Object.entries(sections)){
 const file=path.join(root,relative),bytes=fs.readFileSync(file),lineEnd=bytes.indexOf(10)+1;
 if(bytes.toString('utf8').includes(section.split('\n')[0]))throw Error('Already inserted: '+relative);
 const newline=bytes[lineEnd-2]===13?'\r\n':'\n';
 const addition=Buffer.from(newline+section.replace(/\n/g,newline),'utf8');
 fs.writeFileSync(file,Buffer.concat([bytes.subarray(0,lineEnd),addition,bytes.subarray(lineEnd)]));
}
console.log('Inserted three minimal status sections; pre-existing document bytes retained.');
