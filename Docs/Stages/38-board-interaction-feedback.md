# 38단계 — Board 상호작용 시각 피드백

## 원칙

모든 연출은 UI Toolkit VisualElement의 class, scale, opacity와 scheduler만 사용한다. 서버 요청, DTO,
revision, Merge/Generator 규칙은 변경하지 않았다. 성공 연출은 서버 성공 Board를 상태 저장소와 화면에
적용한 뒤에만 실행한다. 실패·충돌·에너지 부족에서는 성공 효과를 호출하지 않는다.

## Drag와 Drop

Drag 시작 시 원본 Cell을 1.07배 확대하고 opacity 0.78, 주황색 테두리로 선택 상태를 표시한다.
기존 Ghost Sprite와 pointer 흐름은 유지한다. Merge 가능한 대상은 기존 `BoardMergeRules.CanMerge`만
사용해 1.03배 확대, 녹색 테두리와 밝은 배경으로 표시한다.

유효하지 않은 Drop은 source Cell에 0.96배 복귀 class를 160ms 적용한 뒤 기본 상태로 전환한다. Board
데이터나 슬롯 위치는 변경하지 않는다.

## Merge 성공

`BoardCommandOutcome.Succeeded` 응답을 Render한 뒤 targetSlot에만 다음 효과를 실행한다.

- 0ms: 결과 Item scale 0.82, 황금색 Cell highlight와 작은 별 2개 표시
- 30ms: scale 1.13
- 170ms: scale 1.0
- 320ms: highlight와 별 제거

연출 중 전체 Board 입력을 잠그지 않는다.

## Generator

버튼 active/producing 상태는 scale 0.92이며 140ms 후 rebound 1.06, 추가 110ms 후 기본 크기로 돌아온다.
서버 생성 성공 후 targetSlot Item은 0.82 → 1.09 → 1.0 Pop과 주황색 highlight를 320ms 표시한다.
실패 응답에는 생성 성공 효과가 없다.

## 검증

- 열린 원본 Unity Editor 컴파일 성공
- EditMode: 38/38 성공
- PlayMode: 16 성공, 외부 개발 서버 통합 테스트 1개 건너뜀, 실패 0
- Drag/Drop/Merge, Lv07→Lv08, Lv08 차단, Generator, Energy, Board refresh, Coin/Quest 회귀 통과
- 기존 `WorkshopArtImporter` obsolete 경고 1종 외 오류 없음

서버 변경과 Git commit/push는 수행하지 않았다.
