# 47단계: Bottom Navigation 실제 아트 연결

## Navigation Art

기존 `WorkshopHudArtCatalog`에 `navHome`, `navCollection`, `navShop`, `navQuest` 표시 참조를 추가했다. 네 PNG는 모두 512×512 RGBA이고 외곽 Alpha가 0인 정사각 UI 아트다.

- Home: `Assets/Art/CatMerge/UI/Navigation/UI_Nav_Home.png`
- Collection: `Assets/Art/CatMerge/UI/Navigation/UI_Nav_Collection.png`
- Shop: `Assets/Art/CatMerge/UI/Navigation/UI_Nav_Shop.png`
- Quest: `Assets/Art/CatMerge/UI/Navigation/UI_Nav_Quest.png`

## Import

네 아트에 Sprite (2D and UI), Single, Alpha Is Transparency, Mipmap Off, Bilinear, Clamp, PPU 100 정책을 적용했다. Cat Merge Configure 메뉴에도 같은 경로를 추가해 재현 가능하게 했다.

## 표시

기존 UXML 구조와 전체 Button Touch Area는 변경하지 않았다. Presenter가 각 Icon Container의 backgroundImage에 Sprite만 연결한다. 일반 Portrait에서는 32×32, Compact에서는 24×24로 Aspect Ratio를 유지한다. Navigation 전체 높이는 74로 유지한다.

Home은 기존 금색 선택 배경과 1.04 Tab scale을 유지하며 Icon만 1.06으로 약하게 강조한다. Collection, Shop, Quest는 Button disabled와 opacity 0.52를 유지하므로 Sprite와 Label은 보이지만 클릭되지 않는다.

## 검증

- 네 Sprite import 성공
- Unity 최신 Tundra build success
- 이전 VisualElement 컴파일 오류 해소 확인
- HUD Catalog 네 Sprite 이름 검증 추가
- `git diff --check` 통과
- Board, Mascot, Speech Bubble, Navigation 높이와 Safe Area 구조 변경 없음
- Drag/Merge/Generator/Economy/Quest/서버 코드 변경 없음

## 수동 확인

1080×1920 Game View에서 네 Icon이 Label 위에 표시되는지, Home 선택 강조가 과하지 않은지, disabled 세 탭에 Press feedback이 없는지 확인한다. 마지막 Board Row, Mascot, Speech Bubble과 Navigation이 동시에 표시되는지도 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
