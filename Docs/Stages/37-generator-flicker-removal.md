# 37단계 — Generator 요청 시 화면 깜빡임 제거

## 원인

모든 명령 시작 시 `GameClientRoot.StartExclusive`가 Presenter의 Root 전체를 disabled 처리했다. UI
Toolkit은 전체 하위 트리에 disabled tint와 스타일 재평가를 적용하므로 Generator 요청마다 화면이
어두워졌다가 복구됐다. 응답 Render에서는 `_board.Clear()` 후 16개 Cell을 다시 생성하고 반응형
레이아웃을 재계산해 한 프레임 동안 Board가 비거나 이동할 수 있었다.

## 변경

`SetInteractionEnabled`는 이제 Root의 시각 상태를 변경하지 않고 `_inputBlocked`로 사용자 입력 의도만
차단한다. 요청 중 Board pointer 입력은 무시하고 Generator 버튼만 비활성화한다. 서버 요청과 `_busy`
직렬화 흐름은 변경하지 않았다.

Board는 첫 Render 또는 서버가 Board 크기를 바꾼 경우에만 Cell VisualElement를 생성한다. 일반적인
Generate/Merge 응답에서는 기존 16개 Cell, Item art, Debug label을 그대로 재사용하고 다음 값만
갱신한다.

- `userData`와 Tooltip
- empty/item class
- Sprite와 표시 크기
- 숨겨진 Debug text

응답마다 Board를 Clear하지 않으며 Cell 트리가 유지되면 높이 재계산도 반복하지 않는다. Generator의
짧은 pressed/producing 피드백은 해당 버튼에만 남는다.

## 검증

- 열린 원본 Unity Editor C# 컴파일 성공
- EditMode: 37/37 성공
- PlayMode: 16 성공, 외부 개발 서버 통합 테스트 1개 건너뜀, 실패 0
- Root 전체 `SetEnabled` 사용 금지와 슬롯 재사용 경로 정적 회귀 검증
- Generate, Energy 차감, Merge, Board refresh 회귀 통과
- 기존 `WorkshopArtImporter` obsolete 경고 1종 외 오류 없음

서버 변경과 Git commit/push는 수행하지 않았다.
