# 46단계: Bottom Navigation Presentation

## 구조

기존에 숨겨져 있던 `bottom-navigation` placeholder를 정식 UI 구조로 확장했다.

```text
bottom-navigation
├─ nav-home
│  ├─ nav-home-icon
│  └─ navigation-label (홈)
├─ nav-collection
│  ├─ nav-collection-icon
│  └─ navigation-label (도감)
├─ nav-shop
│  ├─ nav-shop-icon
│  └─ navigation-label (상점)
└─ nav-quest
   ├─ nav-quest-icon
   └─ navigation-label (퀘스트)
```

## 기능 상태

- Home: 선택 및 활성 상태. 별도 callback이 없어 다시 눌러도 화면 재초기화나 API 요청이 없다.
- Collection: 전용 View와 데이터가 없어 disabled.
- Shop: 전용 View와 구매 계약이 없어 disabled.
- Quest: 현재 HUD Quest는 존재하지만 전용 View가 없어 disabled.

View Router, Scene 전환, 준비 중 Popup은 추가하지 않았다.

## 스타일

Navigation Panel은 화면 콘텐츠 폭의 90%, 최대 680, 일반 높이 74로 구성한다. 크림색 배경, 따뜻한 갈색 Border, 24px 둥근 모서리를 사용한다. Home은 금색 배경, 굵은 Label, 1.04 scale로 선택 상태를 나타낸다. 미구현 탭은 opacity 0.52의 disabled 상태다.

Icon Container는 각 탭에 준비했지만 적합한 프로젝트 Sprite가 없어 숨겼다. 현재는 Label 중심으로 표시한다.

## Layout 보호

`GameHudPresenter`가 Navigation의 실제 높이와 margin을 Board 하단 예약 공간에 포함한다. Layout resolve 전에는 80을 예약한다. 1080×1920의 논리 720×1280 화면에서는 기존 하단 여유를 사용하여 약 148 정사각 Cell을 거의 유지한다. Safe Area가 커지거나 높이가 짧을 때만 기존 세로 fitting이 Cell을 필요한 만큼 제한한다.

Compact 높이에서는 Navigation을 56, Very Compact에서는 48 수준으로 줄인다. 기존 SafeAreaController 안에 있으므로 하단 시스템 영역과 겹치지 않는다.

## Missing Navigation Art

- Home: 고양이 집 아이콘 필요
- Collection: 책/카드/고양이 도감 아이콘 필요
- Shop: 장바구니/상점 아이콘 필요
- Quest: 클립보드/별 아이콘 필요

외부 이미지를 다운로드하거나 임시 스타일 아이콘을 생성하지 않았다.

## 검증

- Unity UXML/USS import 성공
- 신규 C# 컴파일 오류 없음
- Home selected 및 나머지 disabled 구조 테스트 추가
- Navigation reserve fallback/hidden 테스트 추가
- `git diff --check` 통과
- ScrollView 추가 없음
- Server, DTO, Merge, Generator, Economy, Quest 및 Mascot API 변경 없음
- 별도 BatchMode 회귀 테스트는 기존 Unity Licensing Client 문제로 실행하지 못함

## 수동 확인

1080×1920 Game View에서 마지막 Board Row, Mascot, Speech Bubble과 Navigation이 동시에 보이는지 확인한다. Home 재클릭 시 API 요청이나 Board refresh가 발생하지 않는지, disabled 탭이 클릭되지 않는지 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
