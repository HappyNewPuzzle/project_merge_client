# 36단계 — 4×4 Board 최종 세로 화면 맞춤

## 핵심 변경

일반 플레이 최상위 요소를 `ScrollView`에서 고정 `VisualElement`로 변경해 세로 스크롤과 Scrollbar를
제거했다. Board가 시작되는 실제 Y 좌표부터 화면 contentRect 하단까지 남은 높이를 측정하고,
Board frame padding·border와 Cell margin을 제외한 공간을 정확히 네 행에 배분한다.

초기 반응형 Cell 높이는 62~112px이며 720px viewport에서는 약 92.86px다. 실제 레이아웃 이후에는
`CalculateFittedBoardSlotHeight`가 남은 공간으로 재계산하며 48~112px 범위에서 마지막 행 전체가
들어오게 한다. Board Cell 자체 축소 전에 HUD·Quest·Generator 주변 공백을 우선 제거했다.

## 유지한 크기

Item은 이전 단계의 기본 88%와 Toy/Food/Rest 투명 여백 보정을 그대로 유지한다. Generator Sprite도
기본 270×190, Compact 220×145, Very Compact 185×120을 유지했다.

## 세로 공간 축소

- 화면 padding: 기본 12px, Compact 6px
- Quest 최소 높이: 기본 44px, Compact 34px, Very Compact 31px
- Quest 버튼: 기본 30px, Compact 27px
- Generator container top/bottom padding과 margin: 0
- Board frame padding: 기본 4px, Compact 3px
- Cell 간격: margin 0.5%에서 0.4%
- Compact HUD Capsule: 40px, 아이콘 32px

빈 슬롯, 4×4 구조, Item 크기, Generator 이벤트, Quest/Economy 데이터 흐름은 변경하지 않았다.

## 검증

열려 있는 원본 Editor에서 C#·UXML·USS Import와 컴파일이 성공했다. 임시 복제 프로젝트에서 다음을
검증했다.

- EditMode: 37/37 성공
- PlayMode: 16 성공, 외부 개발 서버 통합 테스트 1개 건너뜀, 실패 0
- 일반 플레이 UXML에 ScrollView가 없음을 검증
- 720/640/480px 남은 공간 계산과 최소/최대 Cell 높이 검증
- Drag/Drop/Merge, Board refresh, Generator, Energy, Coin, Quest 회귀 통과
- 기존 `WorkshopArtImporter` obsolete 경고 1종 외 오류 없음

서버 변경과 Git commit/push는 수행하지 않았다.
