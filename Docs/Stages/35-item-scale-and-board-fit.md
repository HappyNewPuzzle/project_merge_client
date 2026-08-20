# 35단계 — Item 확대와 4×4 Board 화면 맞춤

## Item View

이전 단계의 투명 여백 보정 크기가 Flex 레이아웃에서 Cell 크기로 다시 축소될 수 있어
`board-item-art`에 `flexShrink=0`을 적용했다. 기본 요소는 Cell의 88%이며 Toy/Food/Rest의 PNG별
투명 여백 보정값은 그대로 사용한다. Toy Lv01은 149% 요소로 표시되어 약 47%인 실제 불투명 영역이
Cell의 약 70%로 보인다. Cell의 `overflow: hidden`과 `scale-to-fit`으로 비율과 경계를 유지한다.

## Board 화면 맞춤

초기 Cell 높이는 viewport 높이에서 HUD·Quest·Generator 예약 공간을 제외해 62~112px로 계산한다.
720px viewport에서는 약 78.57px다. Board Cell은 너비 24%, margin 0.5%로 바꿔 4열을 유지하면서
간격을 줄였다.

초기 계산 후 실제 ScrollView content 높이가 viewport를 넘으면 초과분과 8px 안전 여백을 네 Board
행에 균등 배분해 Cell 높이를 한 번 더 줄인다. 최소 터치 높이는 54px이며 Item은 Cell 대비 백분율로
표시되므로 시각 비중을 유지한다. 일반 화면에서 마지막 행을 자르지 않고 극단적으로 작은 화면에서만
ScrollView가 안전장치가 된다.

## 공간 재배분

- Generator: 기본 270×190, Compact 220×145, Very Compact 185×120
- Quest: 기본 50px, Compact 40px, Very Compact 36px
- HUD·Quest·Generator·Board 사이 margin과 Board frame padding 축소
- 빈 슬롯 스타일과 모든 게임 이벤트 유지

## 검증

열려 있는 원본 Editor에서 C#·USS 자동 Import와 컴파일이 성공했다. 원본 Editor를 방해하지 않도록
최신 Assets/Packages/ProjectSettings를 임시 프로젝트에 동기화해 테스트했다.

- EditMode: 34/34 성공
- PlayMode: 16 성공, 외부 개발 서버 통합 테스트 1개 건너뜀, 실패 0
- Drag/Drop/Merge, Board refresh, Toy Generator, Energy 차감, Coin/Quest 회귀 통과
- 기존 `WorkshopArtImporter` obsolete 경고 1종 외 오류 없음

서버 변경과 Git commit/push는 수행하지 않았다.
