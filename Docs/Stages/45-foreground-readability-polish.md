# 45단계: Background 위 전경 가독성 마감

## HUD

1080×1920 Portrait의 일반 모드에서 Currency Capsule을 높이 56, 최소 폭 142로 조정했다. 아이콘은 46, 숫자는 22이며 좌우 padding과 갈색 Border 대비를 소폭 높였다. Gem placeholder는 계속 숨긴다.

## Quest

Quest Card 최소 높이를 52, padding을 7로 조정했다. Eyebrow 13, 실제 Quest 문구 17, 보상 버튼 92×34와 글자 14를 사용한다. 표시 데이터와 Claim 흐름은 변경하지 않았다.

## Board와 Cell

UI Toolkit에서 플랫폼별 지원이 불안정한 box shadow 대신 따뜻한 전경 색과 Border 대비를 사용했다. Board frame은 Border 4와 조금 더 짙은 갈색을 사용한다. 빈 Cell과 Item Cell은 기존 구조를 유지하면서 배경색과 Border만 미세 조정했다. Item VisualScale과 Cell 크기 계산은 변경하지 않았다.

## Generator

크기와 위치는 그대로 310×210이다. 배경색을 조금 더 밝게 하고 Border를 5로 올려 방 배경에서 핵심 입력 오브젝트로 구분했다. Press, Rebound, Spawn Effect class는 변경하지 않았다.

## Mascot과 Speech Bubble

Mascot 폭 비례 계수를 0.24에서 0.25로 미세 조정하고 범위를 155~196으로 변경했다. 720 논리 폭에서는 180, 실제 `.screen` 콘텐츠 폭 약 696에서는 약 174다. 말풍선은 최소 높이 96, padding 18, 글자 19, 간격 12를 사용한다. 기존 API와 0.18초 등장 효과는 유지한다.

## 검증

- Unity USS import 성공
- 신규 C# 컴파일 오류 없음
- Mascot min/max 계산 테스트 갱신
- `git diff --check` 통과
- CAT MERGE 숨김 유지
- Background, 4×4 Board 구조, Server/DTO/Gameplay 코드 변경 없음
- 별도 BatchMode 회귀 테스트는 기존 Unity Licensing Client 문제로 실행하지 못함

## 수동 확인

1080×1920 Game View에서 마지막 행과 Mascot이 동시에 보이는지, 밝은 창문 영역에서도 Currency/Quest가 읽히는지, Board Border가 과도하게 강하지 않은지 확인한다. 실제 입력으로 Drag/Drop/Merge와 Generator feedback을 한 차례 확인한다.

서버는 수정하지 않았으며 commit/push도 수행하지 않았다.
