# 44단계: 고양이 방 Background Presentation

## 리소스

`Assets/Art/CatMerge/Backgrounds/CatMerge_Room_Background.png`를 사용한다. 원본은 941×1672 RGB 이미지이며 비율은 약 0.563으로 9:16 Portrait와 거의 같다.

## 표시 구조

`GameHud.uxml`의 `.screen` 첫 번째 자식으로 `room-background` VisualElement를 추가했다. absolute positioning으로 Layout 흐름에서 제외하고, picking mode를 Ignore로 설정해 HUD/Board 입력을 가로채지 않는다. 이후 형제인 HUD, Quest, Generator, Board, Mascot, Speech Bubble이 항상 위에 그려진다.

단색 `.screen` 배경은 투명하게 바꾸었으며 별도 대형 dark overlay는 추가하지 않았다. 기존 HUD/Board/말풍선 패널의 불투명 크림색 배경과 Border는 유지했다.

## Scaling

`-unity-background-scale-mode: scale-and-crop`을 사용한다. Aspect Ratio를 유지한 채 전체 영역을 채우며 남는 부분만 중앙 기준으로 crop한다.

- 720×1280: 원본과 거의 같은 비율, 극소량 crop
- 1080×1920: 9:16, 극소량 crop
- 1080×2340: 더 긴 화면이므로 좌우 일부 crop
- 1440×2560: 9:16, 극소량 crop

Background는 absolute layer이므로 Scroll이나 콘텐츠 크기를 만들지 않는다.

## Import와 연결

Single Sprite, mipmap off, Bilinear, Clamp, PPU 100, max size 2048, Compressed 설정을 사용한다. `WorkshopHudArtCatalog.roomBackground`에서 참조하고 기존 Cat Merge Import 메뉴로 설정을 재현할 수 있다.

## 검증

- Unity Sprite import 성공
- UXML/USS import 성공
- 신규 C# 컴파일 오류 없음
- Background layer 존재 및 HUD 카탈로그 참조 테스트 추가
- `git diff --check` 통과
- Board, Generator, Mascot, Item 크기와 Animation 로직 변경 없음
- 별도 BatchMode 회귀 테스트는 기존 Unity Licensing Client 문제로 실행하지 못함

## 수동 확인

1080×1920 Game View에서 창·선반이 상단 HUD 가독성을 방해하지 않는지, 중앙 밝은 벽 위 Board Item이 선명한지, 하단 가구와 Mascot이 과도하게 겹쳐 보이지 않는지 확인한다. Device Simulator에서는 Safe Area 바깥 표시 상태를 추가 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
