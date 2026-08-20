# 42단계: 모바일 Portrait 레이아웃 검증

## 기준 구성

`GamePanelSettings`는 Scale With Screen Size, Reference Resolution `720×1280`, Match `0.5`를 사용한다. 일반 플레이 UXML은 ScrollView를 사용하지 않으며 `SafeAreaController`가 실제 화면 Safe Area 픽셀을 Panel 단위 여백으로 환산한다.

## 목표 해상도 환산

| 실제 해상도 | Panel 논리 크기(근사) | Board Cell | Mascot |
|---|---:|---:|---|
| 1080×1920 | 720×1280 | 112 | 표시 |
| 720×1280 | 720×1280 | 112 | 표시 |
| 1080×2340 | 652.2×1413.1 | 112 | 표시 |
| 1440×2560 | 720×1280 | 112 | 표시 |

네 해상도 모두 Panel 높이가 980 이상이므로 기본 마스코트와 말풍선이 표시된다. Board는 최대 폭 620, 셀 높이 112, 4×4 구조를 유지한다.

## 1080×1920 검증

- Energy/Coin: 상단 캡슐형 HUD 유지, Gem은 숨김
- Quest: 기존 compact 카드 유지
- Generator: Board 바로 위의 기존 270×190 표시 유지
- Board: 4열×4행, 최대 폭 620, Cell 높이 112
- Mascot: 128×128, Board 아래 말풍선과 함께 표시
- Scroll: UXML에 ScrollView 없음, Screen overflow hidden 유지
- Safe Area: 기존 `SafeAreaController` 재사용

Panel 논리 높이 1280에서 HUD, Quest, Generator, Board 4행, Mascot 영역의 현재 높이 합계가 가용 공간 안에 들어간다. Board Cell 축소나 요소 재배치가 필요하지 않다고 판단해 UXML/USS/Presenter는 변경하지 않았다.

## 회귀 검토

이번 단계는 코드와 UI 레이아웃을 변경하지 않았으므로 Drag, Drop, Merge, Merge Pop, Generator 요청, Energy/Coin, Quest 및 Item VisualScale 경로에 변화가 없다. 원본 Unity Editor 컴파일 로그에 신규 오류가 없고 `git diff --check`를 사용해 작업 트리를 확인한다.

## 수동 확인

자동화 환경에서는 실행 중인 사용자 Editor의 Game View 화면을 직접 캡처할 수 없었다. Unity Game View에서 `1080×1920 Portrait`를 선택해 실제 폰트 렌더링, Safe Area 프리뷰, 마스코트 0.18초 등장 효과와 마지막 Board Row를 최종 확인해야 한다. 추가 비율도 가능하면 같은 방식으로 한 번씩 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
