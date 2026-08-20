# 40단계: 마스코트 Presentation 기반

## 리소스 조사

`Assets/Art/CatMerge`와 `Assets/MergeGame/Art`를 검색했다. 고양이가 포함된 파일은 `Assets/Art/CatMerge/Source/cat_merge_sprite_sheet.png`뿐이며, 여러 요소와 배경이 함께 포함된 원본이므로 임의로 잘라 사용하지 않았다. 투명 배경의 개별 고양이 Sprite가 없어 현재 마스코트 UI는 최종 사용자 화면에서 숨긴다.

## UXML 구조

향후 개별 Sprite를 연결할 수 있도록 다음의 단순한 Presentation 구조를 준비했다.

```text
mascot-root
├─ mascot-image
└─ mascot-speech-bubble
   └─ mascot-message
```

기본 문구는 `같은 장난감을 합쳐봐!`이며, Sprite가 없으면 말풍선을 포함한 Root 전체가 보이지 않는다.

## Presenter API

- `ShowMascot(Sprite, string)`: 유효한 개별 Sprite가 있을 때만 표시
- `HideMascot()`: 이미지와 전체 영역 숨김
- `SetMascotMessage(string)`: 대화 시스템 없이 현재 문구만 교체

마스코트는 BoardItemState, Merge, Generator, Economy, Quest 데이터에 접근하지 않는 독립 Presentation UI다.

## 반응형 및 등장 연출

Panel 높이가 980px 미만이면 Sprite가 연결되어도 마스코트를 숨긴다. Board 셀을 축소하거나 Scroll을 추가하지 않는다. 충분한 높이에서 표시할 때 `0.95 / opacity 0 -> 1.03 -> 1.0`의 약 0.18초 진입 피드백을 사용한다. 외부 Tween 패키지는 사용하지 않았다.

## 검증

- 원본 Unity Editor 컴파일 및 UXML import 성공
- 신규 컴파일 오류 없음
- 기존 `WorkshopArtImporter.cs` obsolete API 경고만 존재
- UXML 이름과 Presenter query 일치 확인
- compact 경계값 EditMode 테스트 추가
- 기존 Scene Composition 테스트를 정식 Mascot 구조에 맞게 갱신
- 별도 복제본 BatchMode: Unity Licensing Client 연결 실패로 테스트 실행 전에 중단
- HUD, Quest, Generator, Board, Cell 및 Item VisualScale 값 변경 없음

## 다음 준비물과 수동 확인

투명 배경의 개별 크림/주황색 고양이 PNG를 Sprite로 추가한 뒤 `ShowMascot`에 전달하는 카탈로그 연결이 필요하다. 연결 후 980px 이상 Game View에서 표정 가독성, 말풍선 화면 이탈 여부와 등장 연출을 확인하고, 980px 미만에서 전체 영역이 숨겨져 4×4 Board가 유지되는지 확인한다.

서버 저장소는 수정하지 않았고 Git commit/push도 수행하지 않았다.
