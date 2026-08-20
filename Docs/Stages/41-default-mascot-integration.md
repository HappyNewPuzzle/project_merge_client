# 41단계: 기본 마스코트 연결

## 적용 리소스

`Assets/Art/CatMerge/Characters/Mascot/Cat_Mascot_Default.png`를 메인 HUD 기본 마스코트로 연결했다. 원본은 1536×1024 RGBA 이미지이며 네 모서리와 외곽 표본의 Alpha가 0임을 확인했다.

## Import 설정

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: On
- Mip Maps: Off
- Filter Mode: Bilinear
- Wrap Mode: Clamp
- Pixels Per Unit: 100
- Max Size: 2048
- Compression: Compressed

`CatMergeToyArtImporter`에도 동일 경로를 추가해 Configure 메뉴 재실행 시 설정과 카탈로그 연결이 재현된다.

## 연결 방식

기존 `WorkshopHudArtCatalog`에 표시 전용 `defaultMascot` 참조만 추가했다. `GameHudPresenter.ApplyHudArt`가 기존 `ShowMascot`에 이 Sprite와 `같은 장난감을 합쳐봐!` 메시지를 전달한다. 새 마스코트 시스템이나 게임 로직은 추가하지 않았다.

## 반응형 및 레이아웃

Panel 높이가 계산되기 전과 980px 미만에서는 숨기고, 980px 이상에서만 표시한다. 기존 0.18초 진입 효과를 그대로 사용한다. HUD, Quest, Generator, Board, Cell 크기와 Item VisualScale 값은 변경하지 않았다.

## 검증

- 원본 Unity Editor 컴파일 및 Sprite import 성공
- 신규 컴파일 오류 없음
- PNG Alpha 표본 검사 성공
- HUD 카탈로그 Sprite 이름 검증 테스트 추가
- 0px/759px/979px 숨김, 980px 표시 경계 테스트 유지
- `git diff --check` 실행 대상
- 복제본 BatchMode 테스트는 Unity Licensing Client 연결 실패로 실행 전에 중단

## 수동 확인

Game View 높이 980px 이상에서 고양이와 말풍선, 0.18초 등장 효과를 확인한다. 높이 979px 이하에서 마스코트 전체가 숨겨지고 4×4 Board, Drag/Merge/Generator가 기존대로 동작하는지 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
