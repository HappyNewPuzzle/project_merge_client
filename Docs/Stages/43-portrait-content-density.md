# 43단계: Portrait 콘텐츠 밀도 개선

## 원인

1080×1920은 PanelSettings에 의해 논리 크기 720×1280으로 표시된다. 기존 Board 폭은 최대 620으로 충분했지만 Cell 높이가 112 상한에 걸려 Board가 정사각형보다 낮았고, Mascot도 128 고정 크기라 콘텐츠가 상단에 작게 모여 보였다.

## 1080×1920 예상 Layout

Safe Area inset이 없는 기본 Game View의 논리 크기를 기준으로 한 예상 resolved size다.

- Board Frame: 약 626×626 (화면 콘텐츠 폭의 90%)
- Board Content: 약 612×612
- Cell: 약 148×148
- Item: Cell의 기존 88% 영역 × 기존 개별 VisualScale
- Generator: 310×210
- Mascot: 약 167×167 (`panel content width × 0.24`, 150~190 제한)
- Speech Bubble: 약 443×88 이상
- Currency Capsule: 높이 52, 아이콘 42, 글자 20

Board Cell은 실제 Board content width를 4행에 나눈 정사각형 크기를 우선 사용한다. 남은 세로 공간이 부족할 때만 Mascot 영역을 예약한 뒤 가능한 높이로 축소한다.

## 반응형

- Board: 부모 콘텐츠 폭 90%, 최대 680
- Mascot: Panel 폭 비례, 150~190
- 980 미만: 기존 compact/very-compact 규칙과 Mascot 숨김 유지
- 720×1280, 1080×1920, 1440×2560: 동일한 720×1280 논리 화면에서 유사 비율
- 1080×2340: 약 652×1413 논리 화면에서 폭에 맞춰 Board와 Mascot이 자연스럽게 축소

## CAT MERGE

`CAT MERGE` Label은 로고 Sprite, 화면 이동, 상태 또는 데이터 바인딩이 없는 정적 문구였다. 개발 단계 임시 Title로 판단해 UXML 구조는 유지하되 일반 플레이 USS에서 숨겼다.

## 보존 사항

BoardItemState, 서버 권위 구조, Merge/Generator/Economy/Quest 흐름, Item VisualScale 값 및 Drag/Merge/Generator/Mascot Animation 코드는 변경하지 않았다. PNG 원본도 변경하지 않았다.

## 검증

- 원본 Unity Editor UXML/USS import 성공
- 신규 C# 컴파일 오류 없음
- 폭 기반 정사각 Cell, 세로 부족 fallback, Mascot min/max 계산 테스트 추가
- `git diff --check` 통과
- 일반 플레이 ScrollView 없음 및 overflow hidden 유지
- Unity Licensing Client 문제로 별도 BatchMode 회귀 테스트는 실행하지 못함

## 수동 확인

1080×1920 Game View에서 실제 resolved size, 4번째 행, 하단 여백, Mascot 표정과 말풍선 가독성을 확인한다. Safe Area가 큰 Device Simulator에서는 정사각 Cell이 세로 가용 공간에 맞춰 축소되는지 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
