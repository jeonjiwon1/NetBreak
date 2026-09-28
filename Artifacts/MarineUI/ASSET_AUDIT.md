# Marine UI 39종 사전 조사

프로젝트/첨부 ZIP 비교. 사용 여부는 정적 참조 기준이며 Unity 실행 확인이 아니다. 정상 Area 1에서 아이템을 지급하지 않으므로 아이템 6종은 보유 시 표시 경로만 존재한다. 미참조 파일 0개.

공통 경로: `Assets/Resources/UI/Area1/`. 각 PNG 옆 동명 `.png.meta` 사용. 6종 프레임 Border L/B/R/T=5, 나머지=0. 모든 파일 Point/무 Mipmap/무압축.

| 파일 | 크기 | 유형 · 실제 연결 위치 · 호출 | GUID | ZIP 일치 |
|---|---|---|---|---|
| button.png | 32×32 | 프레임 · 조업 시작, 성장 관리 · Area1HUDSkin.StylePreparation/StyleGrowthButton | 625fbb98520372b00ed8c104a5f6308c | True |
| decor_bobber.png | 48×48 | 장식 · Ready · Area1HUDSkin.Decorate | d541711ce1b83fef16f597f5e939dbaf | True |
| decor_clock.png | 48×48 | 장식 · Timer · Area1HUDSkin.Decorate | a36b32c1a516dd5d9d49c223d4acda79 | True |
| decor_coral.png | 48×48 | 장식 · Ready, Hotbar · Area1HUDSkin.Decorate | 7093d6fbdf8013f948e6a7dcb60d086c | True |
| decor_crate.png | 48×48 | 장식 · Inventory · Area1HUDSkin.Decorate | ef1b4212d31af625cd94da4caa13e57b | True |
| decor_gull.png | 48×48 | 장식 · 정보 HUD 제목 · Area1HUDSkin.Decorate | 06108666476eda02e9342bf3200755b8 | True |
| decor_leaf.png | 48×48 | 장식 · 정보 HUD, Inventory, Hotbar, Growth · Area1HUDSkin.Decorate | 3fe7a44fd0697cdce759f080742a5e67 | True |
| decor_palm.png | 48×48 | 장식 · 정보 HUD, Ready · Area1HUDSkin.Decorate | 1efbf99769f06006e70bbcf16623c672 | True |
| decor_rope_knot.png | 48×48 | 장식 · Ready, Hotbar · Area1HUDSkin.Decorate | f5014d0fae924463cf9fd678374110ab | True |
| decor_shell.png | 48×48 | 장식 · 정보 HUD, Timer, Inventory, Hotbar, Growth · Area1HUDSkin.Decorate | 390e83c655d1e7562db2f9897627d367 | True |
| decor_starfish.png | 48×48 | 장식 · 정보 HUD, Inventory 모서리 · Area1HUDSkin.Decorate | 3e0e04c065e17b5f464d3ca973c216ae | True |
| decor_wave.png | 48×48 | 장식 · Ready 하단 좌우 · Area1HUDSkin.Decorate | 78a355b1b7f246e3b2a1a1d8bb29b0c6 | True |
| header.png | 32×32 | 프레임 · 정보/Inventory 제목 및 Drag 손잡이 · Area1HUDSkin.CreateDragHeader | 709461d7307b992c1c5cbd4dc5a42095 | True |
| icon_autonomous_sword_array.png | 32×32 | 아이콘 · 자율 검진 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | beff8950cf6bc9373c4782157fcaf2d3 | True |
| icon_bait.png | 32×32 | 아이콘 · 미끼/Q 또는 W 보유 시 · Area1HUDSkin.ToolIcon | 7b07fae098071390e545560f56782014 | True |
| icon_capacitor_coil.png | 32×32 | 아이콘 · 축전 코일 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | 7156fd5e2e294a8e6cb8942ef67e41f0 | True |
| icon_cast.png | 32×32 | 아이콘 · 투망/Q 또는 W 보유 시 · Area1HUDSkin.ToolIcon | e4898c443c7e712b3cfee8ccd105781b | True |
| icon_empty.png | 32×32 | 아이콘 · 빈 Q/W/Inventory 및 알 수 없는 항목 fallback · Area1HUDSkin.ToolIcon / ItemHUD.Refresh | cd6e4b5cb2af00c1e9ce18d88d049d35 | True |
| icon_frost_crystal.png | 32×32 | 아이콘 · 서리 결정 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | 244ea6daf338e8d23ec4e960b6027faa | True |
| icon_frost_sigil.png | 32×32 | 아이콘 · 서리 문장 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | a27527fd378dc92c483df627250deba6 | True |
| icon_landing.png | 32×32 | 아이콘 · 뜰채/LMB · Area1HUDSkin.ToolIcon | 29cf8815f204958b80f137d2a8ff2997 | True |
| icon_net.png | 32×32 | 아이콘 · 설치 그물/Q 또는 W 보유 시 · Area1HUDSkin.ToolIcon | ed568cb2faa604f47fd2779d94abf5ab | True |
| icon_rod.png | 32×32 | 아이콘 · 낚싯대/Q 또는 W 보유 시 · Area1HUDSkin.ToolIcon | 78c97ca4fd9cd4d582aa412cd820a303 | True |
| icon_signature.png | 32×32 | 아이콘 · R 보스 보상 · Area1HUDSkin.SkillIcon | 654643b1b2053749f2add7c43f3b269b | True |
| icon_spectral_scabbard.png | 32×32 | 아이콘 · 유령 검집 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | 29ee6831bb9dbde6d719513aaa70e8f9 | True |
| icon_speed.png | 32×32 | 아이콘 · x1/x2/x3 · Area1HUDSkin.StyleSpeedButtons | 5442eae1f56519ab8c49b85f7ace4e8a | True |
| icon_stat_area.png | 32×32 | 아이콘 · 현재 구간 · Area1HUDSkin.StyleRunPanel | 5b45c5efff0ad3edd8ba7d93a89bd215 | True |
| icon_stat_catch.png | 32×32 | 아이콘 · 포획 수 · Area1HUDSkin.StyleRunPanel | 1107feab0603bc81f70c6a57f19dee12 | True |
| icon_stat_exp.png | 32×32 | 아이콘 · 경험치/성장 · Area1HUDSkin.StyleRunPanel | b986a97deb892d4e12099e997c50795b | True |
| icon_stat_gold.png | 32×32 | 아이콘 · 골드 · Area1HUDSkin.StyleRunPanel | 26e4781841765b0d397eacfad5973626 | True |
| icon_stat_level.png | 32×32 | 아이콘 · 레벨 · Area1HUDSkin.StyleRunPanel | 07f316b44385677cb48d40ba11087453 | True |
| icon_stat_rate.png | 32×32 | 아이콘 · 어획률 · Area1HUDSkin.StyleRunPanel | 59c77c6aa1a316befd4793ddc9d827a2 | True |
| icon_stat_stage.png | 32×32 | 아이콘 · 조업 단계 · Area1HUDSkin.StyleRunPanel | 120de882d2f9a6c91f8326efc6197b4d | True |
| icon_storm_orb.png | 32×32 | 아이콘 · 폭풍 구체 보유 시 · Area1HUDSkin.ItemIcon / ItemHUD.Refresh | 202df8439f0d1b42fa59a9161fcdaa49 | True |
| icon_tactical.png | 32×32 | 아이콘 · E 미니보스 보상 · Area1HUDSkin.SkillIcon | 810e3933c5c5c23a2d442d6a95ac0024 | True |
| key.png | 32×32 | 프레임 · LMB/Q/W/E/R 키 배지, 배속 · PrototypeHUDCanvas.CreateHotbarSlot / Area1HUDSkin.StyleSpeedButtons | 2835708f3a9f1d3818e8902311cb5d78 | True |
| panel.png | 32×32 | 프레임 · 정보 HUD, Timer, Ready, Inventory, Hotbar · Area1HUDSkin.SetFrame / PrototypeHUDCanvas.BuildHotbar | d58f7eefad85e31075ccf95fa4fa23a5 | True |
| selected_slot.png | 32×32 | 프레임 · Hotbar LMB · PrototypeHUDCanvas.CreateHotbarSlot | 80795aec3d642aff05f75248840671b2 | True |
| slot.png | 32×32 | 프레임 · Inventory 4슬롯, Hotbar Q/W/E/R · Area1HUDSkin.StyleItemSlots / PrototypeHUDCanvas.CreateHotbarSlot | 0578efd33af37c0146cce852e3ad0119 | True |
