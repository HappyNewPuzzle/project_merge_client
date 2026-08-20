# ARCHITECTURE.md

# Cat Merge — Client / Server Architecture

이 문서는 현재 고양이 머지게임의 핵심 기술 구조와 변경 시 지켜야 할 경계를 설명합니다.

## 1. 핵심 철학

이 프로젝트는 **서버 권위형(Server Authoritative)** 게임 구조를 사용합니다.

서버가 최종 진실 원천인 영역:

- Board 상태
- Merge 결과
- Generator 결과
- Economy 변경
- Revision
- Persistence

클라이언트가 담당하는 영역:

- 상태 표시
- 입력
- 요청 전 최소 검증
- 서버 API 호출
- 서버 응답 반영
- Presentation Animation
- UI

## 2. Board Item

현재 클라이언트에 별도의 Item Domain Model을 두지 않습니다.
보드 상태는 서버 DTO `BoardItemState` 중심입니다.

```text
BoardItemState
├─ itemId
├─ slotIndex
├─ chainId
├─ level
├─ name
└─ isMaxLevel
```

이 DTO의 상태를 클라이언트가 임의로 재해석하여 별도 진실 원천을 만들지 않습니다.

## 3. Merge 데이터 흐름

개념적 Flow:

```text
Player Drag
↓
Client pre-check
↓
Merge Request
↓
Server validation
↓
Server merge decision
↓
Persistence / revisions
↓
Server response
↓
Client Board refresh
↓
Success presentation
```

클라이언트 사전 검증은 UX와 불필요한 요청 감소를 위한 것입니다. 최종 성공 판정은 서버입니다.

## 4. Client Merge pre-check

현재 조건:

```text
source slot != target slot
source item exists
target item exists
source.chainId == target.chainId
source.level == target.level
source.isMaxLevel == false
```

이 조건을 통과해도 서버에서 실패할 수 있습니다.

예:

- revision mismatch
- 서버 상태가 이미 변경됨
- 기타 서버 검증 실패

따라서 클라이언트가 pre-check 결과를 최종 게임 결과로 사용하면 안 됩니다.

## 5. Revision

현재 Board와 Economy는 Revision 기반 동시성 제어를 사용합니다.

```text
Client sends known revision
↓
Server validates current revision
↓
Mutation
↓
Revision increment
↓
Response
```

Generator 확장이나 다른 기능 추가 시에도 이 구조를 우회하지 않습니다.
특히 클라이언트가 stale 상태에서 요청을 보내는 상황을 고려합니다.

## 6. Generator 현재 구조

현재 Production Generator는 단일 Toy 생성 흐름입니다.

```text
Player clicks Generator_Toy
↓
Client finds first empty slot
↓
Generate request
  - targetSlot
  - boardRevision
  - economyRevision
  - existing required fields
↓
Server validates
↓
Server creates Toy item
↓
Server deducts Energy
↓
Board/Economy revision update
↓
Response
↓
Client applies authoritative state
```

정확한 Request/Response 필드는 실제 코드가 진실 원천입니다.

## 7. Generator 3종 확장 원칙

향후 목표:

```text
Generator_Toy
→ Toy

Generator_Food
→ Food

Generator_Rest
→ Rest
```

클라이언트가 결과 Item을 지정해서는 안 됩니다.

잘못된 설계 예:

```text
client sends:
chainId = food
level = 8
```

권장 방향:

```text
client sends:
generatorId
targetSlot
boardRevision
economyRevision

server decides:
output chain
output level
energy cost
generated item
```

Generator 식별 방식은 실제 서버 구조 분석 후 확정합니다.

## 8. Generator Definition

향후 Generator 정의는 개념적으로 다음 데이터를 가질 수 있습니다.

```text
GeneratorDefinition
├─ GeneratorId
├─ OutputPool / OutputChain
├─ Min/Max Level or deterministic output
├─ EnergyCost
├─ UnlockCondition
└─ Future balance data
```

초기 버전에서는:

```text
Toy Generator  → Toy Lv01
Food Generator → Food Lv01
Rest Generator → Rest Lv01
```

처럼 단순할 수 있습니다.

하지만 클라이언트 코드 여러 곳에 하드코딩하는 것은 피합니다.
정확한 저장 위치는 서버 아키텍처 분석 후 결정합니다.

## 9. Generator 보안 원칙

클라이언트 입력은 신뢰하지 않습니다.

서버에서 검증해야 할 후보:

- generatorId 존재 여부
- 해당 플레이어가 Generator를 사용할 수 있는지
- targetSlot 유효성
- targetSlot 빈 슬롯 여부
- boardRevision
- economyRevision
- Energy 충분 여부
- Generator cooldown/capacity가 있다면 해당 상태
- 요청 중복 여부

서버가 결과 chain/level을 결정해야 합니다.

## 10. Idempotency

모바일 환경에서는 다음 상황을 고려합니다.

```text
Request sent
↓
Server mutation completed
↓
Response lost
↓
Client retries
```

Generator / Economy mutation에서 중복 처리 위험이 있으므로 기존 Idempotency 구조가 있는지 먼저 확인합니다.

이미 존재한다면 재사용합니다. 없다면 실제 필요성/현재 API 패턴을 분석한 후 별도 단계로 설계합니다. 무조건 새로운 Idempotency Framework를 먼저 만들지 않습니다.

## 11. Offline Mock Architecture

Offline Mock은 서버 역할을 로컬에서 모사합니다.

현재 Mock은 다음 검증에 사용됩니다.

- Toy/Food/Rest 혼합 Board
- 같은 chain/level merge
- Cross-chain 실패
- Lv08 실패
- Revision 유지/증가
- Toy Generator

Mock에서 `level + 1`을 계산하는 것은 서버를 모사하기 위한 내부 구현으로 허용될 수 있습니다.
단, Production Client presentation/domain 코드에 같은 로직을 옮기지 않습니다.

## 12. Art Presentation Architecture

머지 아이템:

```text
BoardItemState
↓
chainId + level
↓
WorkshopItemArtCatalog
↓
Sprite + VisualScale
↓
GameHud UI Toolkit VisualElement
```

아이템별 Prefab은 사용하지 않습니다.
Board Cell은 동적으로 생성됩니다.

## 13. HUD Art

HUD/Generator/Currency/Mascot/Navigation은 현재 HUD Art Catalog 구조를 사용합니다.

```text
WorkshopHudArtCatalog
├─ Generator art
├─ Currency art
├─ Mascot art
└─ Navigation art
```

실제 필드명은 코드가 진실 원천입니다.
새 Resource Manager나 Addressables 시스템은 현재 필요하지 않습니다.

## 14. Main UI Architecture

현재 핵심 파일:

```text
GameHud.uxml
GameHud.uss
GameHudPresenter.cs
```

### GameHud.uxml
UI 구조.

### GameHud.uss
Layout / Style / Responsive / Visual state.

### GameHudPresenter
서버/Mock 상태를 Presentation에 반영하고 입력 Event를 연결.
게임 규칙 자체를 Presenter에 집중시키지 않습니다.

## 15. Board View

현재 Board:

- 4 × 4
- UI Toolkit
- Dynamic Cell
- Sprite background/image
- Presentation scale
- Drag/drop event
- Empty cell representation

Board의 시각적 크기는 Portrait 화면 폭과 세로 가용 공간을 고려해 계산됩니다.

## 16. Presentation Animation

Animation은 게임 결과를 결정하지 않습니다.

```text
Authoritative response
↓
Presentation state update
↓
Visual effect
```

예:

- Merge Pop
- Spawn Pop
- Drag selection
- Target highlight
- Mascot entrance

Base VisualScale을 Animation이 덮어쓰지 않도록 주의합니다.

```text
FinalScale = BaseVisualScale × AnimationScale
```

## 17. Mascot Architecture

현재 Mascot은 단순 Presentation 요소입니다.

```text
MascotRoot
├─ MascotImage
└─ SpeechBubble
   └─ MascotMessage
```

현재 API:

- Show
- Hide
- Message 변경

현재 범위에 없는 것:

- Dialogue Manager
- NPC State Machine
- Character AI
- Quest-driven Character System

필요해질 때 별도 설계합니다.

## 18. Bottom Navigation Architecture

현재 Navigation은 Presentation 기반입니다.

```text
Home
Collection
Shop
Quest
```

실제 Router는 아직 없습니다.

현재 상태:

- Home active
- 나머지 disabled

View Routing은 실제 두 번째 화면이 생기는 시점에 설계하는 것이 적절합니다.

## 19. Currency Architecture

현재:

```text
Energy
MaxEnergy
Coins
EconomyRevision
```

를 사용합니다.

Gem은 서버 계약이 없습니다.
Gem Sprite 존재와 Gem Economy 기능 존재를 혼동하지 않습니다.

## 20. Safe Area / Responsive

모바일 UI는 기존 Safe Area 구조를 유지합니다.
배경은 전체 화면을 채울 수 있지만 실제 interactive UI는 Safe Area 내에서 동작합니다.

Portrait 기준 주요 목표:

- 720×1280
- 1080×1920
- 1080×2340
- 1440×2560

특정 해상도 하드코딩보다 상대적인 폭/높이와 min/max를 우선합니다.

## 21. 현재 Production / Mock 경계

### Production

- 서버 권위
- Toy Generator
- Economy
- Quest
- Board/Merge

### Mock

추가로 허용:

- Mixed Toy/Food/Rest Board
- Food/Rest merge simulation
- Cross-chain validation
- Lv08 validation

Food/Rest가 Mock에서 동작한다는 사실만으로 Production 기능이 완성된 것은 아닙니다.

## 22. 저장/Persistence 원칙

서버의 영속성 구조를 클라이언트가 추측해서 구현하지 않습니다.
Generator, Board, Economy 변경은 서버 Transaction/Repository/DB 구조를 실제 분석한 뒤 수정해야 합니다.

DB 함수/프로시저로 게임 결과를 임의 이동하지 않습니다.
게임 로직은 서버 Application/Domain 구조를 우선합니다.

## 23. 확장 작업의 추천 순서

```text
1. 현재 구조 분석
2. API/데이터 계약 설계
3. Server authoritative implementation
4. Server tests
5. Client contract update
6. Client presentation/input
7. Offline/Integration tests
8. Manual Game View verification
9. Documentation update
10. User approval
11. Commit / Push
```

## 24. 현재 가장 중요한 다음 설계

Production Generator 3종 API.

구현 전에 반드시 답해야 할 질문:

- Generator를 어떻게 식별할 것인가?
- Generator 정의는 어디에 둘 것인가?
- Energy Cost는 어디에서 관리할 것인가?
- Output Item Pool은 누가 결정하는가?
- 기존 Generator 요청과 하위 호환 가능한가?
- Board/Economy revision은 어떻게 유지하는가?
- Idempotency가 필요한가?
- 기존 Player 데이터 Migration이 필요한가?
- 잠금/Transaction 범위는 어디인가?

이 질문을 해결한 뒤 구현합니다.

## 25. 최우선 아키텍처 원칙

**클라이언트는 보이는 것과 입력을 담당하고, 서버는 게임 결과와 정합성을 담당한다.**

새 기능을 추가할 때 이 경계를 흐리지 않습니다.
