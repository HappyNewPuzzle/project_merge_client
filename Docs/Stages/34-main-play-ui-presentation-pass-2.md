# 34단계 — 메인 플레이 UI Presentation 2차 개선

## 범위

게임 기능과 서버 계약을 변경하지 않고 Board, Item, Generator, HUD, Quest의 크기와 세로 공간만
재배분했다. 4×4 보드, Drag & Drop, Generator 이벤트, Economy 및 Quest 데이터 흐름은 유지한다.

## Item 시각 크기 정규화

기존 Item 요소는 Cell의 78%였지만 PNG 내부 투명 여백 때문에 실제 실루엣 크기가 달랐다. 샘플
측정에서 Toy Lv01의 불투명 영역은 원본의 약 47%, Toy Lv08은 약 88%였다. 따라서 Lv01 털실은
같은 UI 박스에서도 작은 점처럼 보였다.

공통 요소 기본 크기를 88%로 확대하고 `WorkshopItemArtCatalog.FindVisualSizePercent`에서 현재
Sprite의 투명 여백만 보정한다. Toy/Food/Rest 모두 동일한 Presenter 경로를 사용하며 목표 실제
실루엣은 Cell의 약 65~75%다. 원본 PNG, Sprite 비율, 서버 level은 수정하지 않는다. Cell의
`overflow: hidden`과 `scale-to-fit`이 경계를 보호한다.

## Board와 반응형 높이

Board는 세로 공간 최우선이다. 760px 미만 viewport의 고정 UI 예약 공간을 340px, 980px 미만을
360px로 조정하고 나머지를 4개 행에 배분한다. Cell 높이는 계속 62~112px 범위이며 720px 화면은
약 90.48px다. Compact 모드에서 제목과 부가 문구를 숨기고 Quest/Generator 여백을 줄여 일반 PC
테스트 높이와 모바일 세로 비율에서 네 번째 행이 화면 안에 들어오도록 했다.

## Generator, HUD, Quest

- Generator: 기본 172×132에서 220×155, Compact에서 180×120, Very Compact에서 150×100
- HUD: 기본 Capsule 42px에서 48px, 아이콘 34px에서 40px, 숫자 17px에서 19px
- Compact HUD: Capsule 42px, 아이콘 34px, 숫자 17px
- Quest: 기본 최소 높이 76px에서 62px, Compact 50px, Very Compact 44px

Generator는 Board 중앙 바로 위 정렬, 둥근 배경, 강조 테두리, hover/pressed 피드백을 유지한다.
Quest는 실제 문자열과 기존 보상 버튼만 사용한다.

## 검증 상태

열려 있는 Unity Editor에서 C# Domain Reload와 UXML/USS Import가 성공했으며 컴파일 오류와 UI Import
오류는 없다. 실행 중인 원본 Editor를 방해하지 않도록 현재 Assets/Packages/ProjectSettings를 임시
프로젝트에 복제해 Test Runner를 실행했다.

- EditMode: 34/34 성공
- PlayMode: 16 성공, 외부 개발 서버 통합 테스트 1개 건너뜀, 실패 0
- 최종 테스트 로그: C# 경고 0, 오류 0

Drag/Drop/Merge, Board refresh, Toy Generator, Energy 차감, Coin/Quest 상태의 기존 회귀 테스트가
통과했다. Git commit과 push는 수행하지 않았다.
