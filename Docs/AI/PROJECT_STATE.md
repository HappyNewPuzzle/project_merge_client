# PROJECT_STATE.md

# Cat Merge — Current Project State

Last updated: 2026-08-20

이 문서는 현재 머지게임 클라이언트의 진행 상태를 요약합니다.
실제 코드와 충돌할 경우 실제 코드가 우선이며, 작업 완료 후 이 문서를 현재 상태에 맞게 갱신합니다.

## 1. 프로젝트 개요

- 장르: 모바일 캐주얼 머지게임
- 테마: 귀엽고 따뜻한 고양이 세계관
- 클라이언트: Unity
- 메인 UI: UI Toolkit
- 목표 화면 방향: Portrait 모바일
- 게임 상태: 서버 권위형(Server Authoritative)
- 현재 개발 단계: 기본 머지 플레이 + 상용형 Presentation + 3체인 Mock 검증

## 2. 현재 핵심 게임 모델

별도 클라이언트 Item 도메인 클래스는 사용하지 않습니다.
현재 Board Item은 서버 응답 DTO `BoardItemState`로 표현됩니다.

주요 필드:

- `itemId`
- `slotIndex`
- `chainId`
- `level`
- `name`
- `isMaxLevel`

클라이언트는 서버 상태를 표시하며 Production 머지 결과를 자체적으로 확정하지 않습니다.

## 3. Merge 시스템

현재 동작:

- Drag & Drop
- 동일 체인 판정
- 동일 레벨 판정
- 서로 다른 슬롯 판정
- Max Level 판정
- 서버 응답 기준 Board 갱신
- Merge 성공 Presentation
- 실패 시 성공 연출 미재생

클라이언트 사전 조건:

```text
sourceSlot != targetSlot
source != null
target != null
source.chainId == target.chainId
source.level == target.level
source.isMaxLevel == false
```

최종 결과는 서버/Mock 서버가 확정합니다.

## 4. 머지 체인

### Toy

- chainId: `toy`
- Lv01 ~ Lv08
- Art 연결 완료
- Mock Merge 검증
- Production Toy Generator 사용 중

### Food

- chainId: `food`
- Lv01 ~ Lv08
- Art 연결 완료
- Offline Mock Merge 검증
- Production Generator 미구현

### Rest

- chainId: `rest`
- Lv01 ~ Lv08
- Art 연결 완료
- Offline Mock Merge 검증
- Production Generator 미구현

### Cross-chain

다음 조합은 Merge 불가:

- Toy + Food
- Toy + Rest
- Food + Rest

### Max Level

현재 Lv08은 최종 단계로 사용합니다.
단, 실제 플레이에서는 서버가 내려주는 `isMaxLevel`이 진실 원천입니다.

## 5. Offline Mock 상태

개발용 Mixed Art Showcase가 준비되어 있습니다.

개념적 초기 Board:

```text
Row 1:
Toy Lv01 | Toy Lv01 | Food Lv01 | Food Lv01

Row 2:
Rest Lv01 | Rest Lv01 | Toy Lv02 | Toy Lv02

Row 3:
Empty | Empty | Empty | Empty

Row 4:
Empty | Empty | Empty | Empty
```

Showcase 조건:

```text
Unity Editor
+ MERGEGAME_USE_LIVE_SERVER 미설정
+ Offline Mock
```

기본 Production 시작 보드는 변경하지 않았습니다.

검증 가능한 흐름:

- Toy Lv01 + Lv01 → Lv02
- Food Lv01 + Lv01 → Lv02
- Rest Lv01 + Lv01 → Lv02
- 각 체인 Lv07 + Lv07 → Lv08
- Cross-chain merge 실패
- Lv08 merge 실패
- 실패 시 Board revision 유지
- 기존 Toy Generator는 첫 빈 슬롯에 Toy Lv01 생성

## 6. Generator 상태

### Production

현재 실제 동작:

```text
Generator_Toy
→ 기존 Toy 생성 요청
→ 서버가 결과 확정
```

현재 미구현:

```text
Generator_Food
Generator_Rest
```

Food/Rest Generator는 아트만 준비되어 있으며 Production 서버 계약은 아직 확정되지 않았습니다.

### 다음 Generator 핵심 과제

Toy/Food/Rest 3종 Generator를 서버 권위 구조에서 지원하기 위한 API 계약 설계가 필요합니다.

검토 대상:

- generatorId 또는 generator type
- 서버 Generator 정의
- Output chain
- Output level
- Energy cost
- Board revision
- Economy revision
- Idempotency
- 기존 플레이어 호환성
- 보안 검증

## 7. Economy

현재 서버 계약에서 사용 중:

- Energy
- Max Energy
- Coins
- Economy Revision

HUD:

- Energy 아이콘/수치 표시
- Coin 아이콘/수치 표시

Gem:

- Sprite 준비됨
- 서버 잔액 필드 없음
- 실제 Gem 경제 시스템 미구현
- HUD 기본 노출 안 함

## 8. Quest

현재 실제 Quest 상태를 HUD Quest Card에 표시합니다.

현재 원칙:

- 서버/Mock 실제 데이터만 표시
- 가짜 Progress 생성 안 함
- 가짜 보상 데이터 생성 안 함
- 전용 Quest 화면 아직 없음

Bottom Navigation의 Quest 탭은 현재 Disabled입니다.

## 9. Main HUD / Presentation

현재 Portrait 메인 화면 구성:

```text
Energy / Coin HUD
Quest Card
Toy Generator
4 × 4 Board
Mascot + Speech Bubble
Bottom Navigation
```

현재 배경:

- 포근한 고양이 방
- Portrait용 Background
- UI는 기존 Safe Area 내 배치

현재 메인 화면은 기능 테스트용 어두운 Prototype UI에서 밝은 모바일 캐주얼 게임 Presentation으로 전환된 상태입니다.

## 10. Board Presentation

- 4 × 4
- UI Toolkit 동적 생성
- Prefab 없음
- 크림색/골드 계열 Board
- 둥근 Cell
- Empty Text 기본 숨김
- Slot Number 기본 숨김
- Item Name 기본 숨김
- Level Text 기본 숨김
- Item Sprite는 Cell 내부 중심 표시
- 개별 VisualScale 지원

Portrait에서는 Board가 화면 핵심 콘텐츠가 되도록 폭 비율 기반으로 계산됩니다.

## 11. Item VisualScale

Sprite 원본을 수정하지 않고 Presentation에서 시각 크기를 보정합니다.

현재 알려진 값:

### Food

```text
Lv01 1.35
Lv02 0.97
Lv03 0.97
Lv04 0.97
Lv05 0.97
Lv06 0.97
Lv07 0.97
Lv08 0.97
```

### Rest

```text
Lv01 0.95
Lv02 0.97
Lv03 0.97
Lv04 0.98
Lv05 0.95
Lv06 0.97
Lv07 0.95
Lv08 0.97
```

Toy 실제 값은 `WorkshopItemArtCatalog`의 현재 데이터를 진실 원천으로 사용합니다.
Animation Scale은 Base VisualScale과 결합되어야 하며 Animation 종료 후 VisualScale을 잃지 않아야 합니다.

## 12. Drag / Merge / Generator 연출

현재 Presentation 연출:

- Drag 시작 피드백
- Merge 가능 대상 Highlight
- Merge 성공 Pop
- Generator Press
- Generator Spawn Effect
- Mascot 등장 Animation

원칙:

- 서버 응답 전에 성공 상태를 확정하지 않음
- 실패 응답에 성공 Effect 재생하지 않음
- 전체 Board 입력 장시간 Lock하지 않음
- 외부 Tween Framework 미사용

## 13. Mascot

기본 Sprite:

```text
Assets/Art/CatMerge/Characters/Mascot/Cat_Mascot_Default.png
```

현재 Presentation:

- 고양이 이미지
- Speech Bubble
- 기본 메시지: `같은 장난감을 합쳐봐!`
- 화면 높이/Responsive 조건에 따라 표시
- 약 0.18초 등장 연출

현재 API:

- `ShowMascot(...)`
- `HideMascot()`
- `SetMascotMessage(...)`

대화 시스템은 아직 없습니다.

## 14. Background

현재 메인 Background는 포근한 고양이 방 스타일입니다.

원칙:

- UI보다 배경이 더 눈에 띄지 않음
- 화면 전체 Cover
- HUD/Board는 Safe Area 유지
- Background 자체 때문에 Scroll 발생 금지

## 15. Bottom Navigation

현재 구조:

```text
Home
Collection
Shop
Quest
```

상태:

- Home: Enabled / Selected
- Collection: Disabled
- Shop: Disabled
- Quest: Disabled

Navigation Art:

- `UI_Nav_Home.png`
- `UI_Nav_Collection.png`
- `UI_Nav_Shop.png`
- `UI_Nav_Quest.png`

현재 Router/화면 전환 시스템은 없습니다.
가짜 화면이나 준비 중 팝업은 구현하지 않았습니다.

## 16. Currency Art

현재 준비된 Sprite:

- `Currency_Coin.png`
- `Currency_Energy.png`
- `Currency_Gem.png`

실제 데이터 연결:

- Coin: 연결
- Energy: 연결
- Gem: Art only

## 17. Generator Art

현재 준비된 Sprite:

- `Generator_Toy.png`
- `Generator_Food.png`
- `Generator_Rest.png`

실제 Production 기능:

- Toy: 연결
- Food: 미구현
- Rest: 미구현

## 18. 주요 Art 리소스

### Merge Items

```text
Assets/Art/CatMerge/Items/Toy/Toy_Lv01.png ~ Toy_Lv08.png
Assets/Art/CatMerge/Items/Food/Food_Lv01.png ~ Food_Lv08.png
Assets/Art/CatMerge/Items/Rest/Rest_Lv01.png ~ Rest_Lv08.png
```

### Generators

```text
Assets/Art/CatMerge/Generators/Generator_Toy.png
Assets/Art/CatMerge/Generators/Generator_Food.png
Assets/Art/CatMerge/Generators/Generator_Rest.png
```

### Currency

```text
Assets/Art/CatMerge/Currency/Currency_Coin.png
Assets/Art/CatMerge/Currency/Currency_Energy.png
Assets/Art/CatMerge/Currency/Currency_Gem.png
```

### Mascot

```text
Assets/Art/CatMerge/Characters/Mascot/Cat_Mascot_Default.png
```

### Navigation

```text
Assets/Art/CatMerge/UI/Navigation/UI_Nav_Home.png
Assets/Art/CatMerge/UI/Navigation/UI_Nav_Collection.png
Assets/Art/CatMerge/UI/Navigation/UI_Nav_Shop.png
Assets/Art/CatMerge/UI/Navigation/UI_Nav_Quest.png
```

## 19. 주요 코드/자산

현재 중요 경로:

```text
Assets/MergeGame/UI/GameHud.uxml
Assets/MergeGame/UI/GameHud.uss

Assets/MergeGame/Runtime/Presentation/GameHudPresenter.cs
Assets/MergeGame/Runtime/Presentation/WorkshopItemArtCatalog.cs
Assets/MergeGame/Runtime/Presentation/WorkshopHudArtCatalog.cs

Assets/MergeGame/Resources/WorkshopItemArtCatalog.asset
Assets/MergeGame/Resources/WorkshopHudArtCatalog.asset

Assets/MergeGame/Runtime/Api/MockMergeGameApiClient.cs
Assets/MergeGame/Runtime/Bootstrap/GameClientRoot.cs
```

테스트/실제 파일명은 저장소 현재 상태를 검색하여 확인합니다.

## 20. 테스트 상태

반복적으로 확인된 항목:

- Unity Import 성공
- C# Compile 성공
- `git diff --check` 성공
- 정적/구조 테스트 다수 추가

현재 환경 이슈:

- BatchMode EditMode/PlayMode 테스트가 Unity Licensing Client 연결 문제로 실행 전에 중단되는 경우가 있음

이 경우 테스트 실패가 아니라 **테스트 실행 환경 실패**로 구분해야 합니다.
가능하면 Unity Editor에서 수동 Game View Regression 확인이 필요합니다.

## 21. 현재 알려진 아트 검수 상태

Toy/Food/Rest를 하나의 Mock Board에 배치해 육안 검수했습니다.

현재 판단:

- Toy / Food / Rest 카테고리 구분 가능
- 전체적인 게임 아트 톤은 통일됨
- Food Lv01 VisualScale 1.35는 현재 화면에서 허용 가능
- Rest Lv01은 작은 화면에서 쿠션/간식 실루엣 혼동 가능성이 있으므로 향후 사용자 테스트 시 관찰 필요

즉시 교체가 필요한 수준은 아닙니다.

## 22. 현재 남은 핵심 기능

### A. Production Generator 3종

- Toy Generator
- Food Generator
- Rest Generator
- 서버 권위형 API 계약 필요

### B. Collection

- 화면/데이터 구조 미구현
- Navigation만 준비

### C. Shop

- 화면/BM 미구현
- Navigation만 준비

### D. Quest Screen

- HUD Quest는 존재
- 전용 화면 미구현

### E. Gem

- Art만 존재
- 서버 Economy 계약 미구현

## 23. 다음 추천 작업

현재 가장 추천되는 다음 단계:

**Production Generator 3종 API 계약 설계**

구현 전에 먼저 분석해야 할 항목:

1. 현재 Generator Client → Server Flow
2. Generator Request/Response DTO
3. 서버 Energy 처리
4. Board revision
5. Economy revision
6. Persistence
7. Idempotency
8. Generator 식별 방식
9. Generator별 출력 정의
10. 기존 사용자 데이터 호환성

API 계약을 먼저 확정한 후 Server → Client 순서로 단계적으로 구현하는 것이 안전합니다.

## 24. Git 상태 원칙

현재 진행 흐름에서는 사용자의 명시적 승인 전까지:

- commit 금지
- push 금지

작업 완료 후 변경 파일과 Git 상태만 보고합니다.
