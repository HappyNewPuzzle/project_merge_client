# 33단계 — 상용형 Cat Merge 메인 플레이 UI

## 목표

서버 권위형 `BoardItemState`와 기존 UI Toolkit 입력 흐름을 유지하면서 기능 테스트용 HUD를 밝고
따뜻한 모바일 캐주얼 머지게임 화면으로 정리했다. 레퍼런스 이미지는 레이아웃과 분위기만 참고했으며
게임 자산으로 포함하거나 잘라 쓰지 않았다.

## Board와 Item

기존 4×4 동적 보드는 그대로 사용한다. 보드 프레임과 셀을 크림·베이지 색상의 둥근 패널로 바꾸고,
아이템 표시 영역을 고정 54px에서 셀 너비와 높이의 78%로 확대했다. `scale-to-fit`을 유지해 원본
비율과 셀 경계를 보호한다. 드래그, 드롭, 머지 대상 강조와 서버 응답 기반 refresh는 변경하지 않았다.

슬롯 번호, 아이템 이름, 레벨 및 빈 슬롯 문구는 기본 화면에서 숨긴다. 데이터와 Tooltip은 유지하며,
`GameHudPresenter.showBoardDebugLabels`를 Inspector에서 켤 때만 작은 개발 라벨을 표시한다. 드래그
Ghost도 텍스트 없이 실제 Sprite만 사용한다.

## HUD와 Generator

Energy와 Coin은 기존 Sprite와 서버 수치를 각각 둥근 캡슐에 표시한다. Energy는
`energy / maxEnergy`, Coin은 천 단위 구분 형식이다. Gem 카탈로그 참조와 UXML 자리는 유지하지만
서버 잔액 계약이 없어 기본 화면에서는 숨긴다.

Quest 카드는 기존 `QuestText`만 표시하고 가짜 진행률이나 보상을 만들지 않는다. Toy Generator는
기존 이벤트/API를 그대로 사용하며 Board 바로 위에서 `Generator_Toy`를 크게 표시한다. Food/Rest
Generator는 노출하거나 동작시키지 않는다.

## 확장 구조와 개발 요소

마스코트·말풍선 컨테이너는 적절한 투명 고양이 Sprite가 준비될 때까지 숨긴다. Home/Collection/Shop/
Quest 하단 탭 구조도 실제 화면 전환이 없으므로 숨긴다. Retry는 오류 상태 패널 안에서만 표시하고,
Logout·친구·일일 보상 등 기존 액션은 기능을 보존한 채 일반 플레이 화면에서 숨겼다.

ScrollView의 스크롤바는 숨기되 작은 화면에서는 터치 스크롤이 가능하다. 주요 패널은 백분율 너비와
620px 최대 너비를 함께 사용하며 기존 Safe Area 처리를 유지한다. Panel 높이가 980px 미만이면
compact 스타일을, 760px 미만이면 very-compact 스타일을 적용한다. 보드 셀 높이도 viewport에서
HUD·Quest·Generator 공간을 제외해 62~112px 범위로 계산하므로 일반 세로 화면에서는 스크롤 없이
4×4 보드 전체가 우선 표시된다. 극단적으로 작은 화면에서만 ScrollView가 안전장치로 동작한다.

## 검증

열려 있던 Unity Editor가 변경된 C#·UXML·USS를 자동 Import했으며 컴파일 오류와 UI Import 오류는
없었다. 기존 `WorkshopArtImporter`의 obsolete 경고 1종은 유지된다. 같은 프로젝트를 Editor가
점유하고 있어 별도 BatchMode EditMode/PlayMode 실행은 Editor 종료 후 진행한다.

## 다음 단계

투명 배경의 개별 마스코트 Sprite를 준비해 말풍선과 연결하고, 실제 기기에서 16:9·19.5:9·태블릿
비율의 보드 크기와 아이템 투명 여백을 시각 확인한다. 다음 연출 단계에서는 머지 Pop, Generator 생성
피드백과 Particle을 Presentation 계층에서 추가할 수 있다.
