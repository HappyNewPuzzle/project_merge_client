# AGENTS.md

# Merge Game — Codex Working Instructions

이 저장소는 **고양이 캐릭터 중심의 모바일 캐주얼 머지게임** 클라이언트입니다.

Codex는 이 파일을 프로젝트 전역의 기본 작업 규칙으로 사용합니다.

## 1. 프로젝트 목표

- 귀엽고 따뜻한 고양이 세계관의 모바일 캐주얼 머지게임
- 서버 권위형(Server Authoritative) 게임 상태
- Unity UI Toolkit 기반의 모바일 Portrait UI
- Toy / Food / Rest 등 여러 머지 체인을 확장 가능한 구조로 운영
- 실제 상용 서비스에 사용할 수 있는 일관된 아트와 UI
- 기능 안정성을 유지하면서 점진적으로 Presentation과 콘텐츠를 확장

## 2. 작업 시작 전 반드시 읽을 문서

작업 시작 전에 아래 문서를 먼저 확인합니다.

1. `Docs/AI/PROJECT_STATE.md`
2. `Docs/AI/ARCHITECTURE.md`
3. `Docs/AI/ART_GUIDE.md`

문서와 실제 코드가 다르면 **실제 코드가 현재 구현의 진실 원천**입니다. 코드와 문서가 불일치하면 작업 완료 보고에 기록하고, 필요하면 `PROJECT_STATE.md`를 현재 코드 기준으로 갱신합니다.

## 3. 가장 중요한 아키텍처 원칙

### 서버 권위형 구조 유지

클라이언트는 다음을 최종 결정하지 않습니다.

- 머지 결과
- 생성기 결과
- 최종 아이템 레벨
- Economy 변경 결과
- Board revision
- Economy revision
- 서버 저장 상태

클라이언트는 입력을 전달하고 서버 응답을 표시합니다.

따라서 클라이언트에 다음과 같은 별도 진실 원천을 만들지 않습니다.

- `NextItem`
- `NextItemId`
- Production용 `level + 1` 결과 확정 로직
- 클라이언트 권위의 Generator 결과
- 클라이언트 권위의 Economy 차감

## 4. 현재 아이템 모델

현재 별도의 `Item`, `MergeItem`, `ItemData`, `ItemDefinition` 도메인 클래스는 없습니다.

보드 아이템은 서버 DTO인 `BoardItemState`를 기준으로 표시합니다.

핵심 필드:

- `itemId`
- `slotIndex`
- `chainId`
- `level`
- `name`
- `isMaxLevel`

새로운 Item 도메인 시스템을 임의로 만들지 않습니다. 기존 구조로 해결할 수 없는 명확한 이유가 있을 때만 별도 설계를 제안합니다.

## 5. Merge 규칙

클라이언트의 사전 검증은 기존 조건을 유지합니다.

- 서로 다른 슬롯
- 양쪽 슬롯에 아이템 존재
- 동일한 `chainId`
- 동일한 `level`
- 원본 아이템이 `isMaxLevel == false`

실제 머지 성공 여부와 결과 상태는 서버 응답이 결정합니다.

Cross-chain merge는 허용하지 않습니다.

예:

- Toy + Food → 불가
- Toy + Rest → 불가
- Food + Rest → 불가

현재 각 머지 체인의 최대 단계는 Lv08이며, 최종 단계 여부는 서버의 `isMaxLevel`을 우선합니다.

## 6. Production과 Offline Mock을 엄격히 분리

Offline Mock은 개발 및 테스트를 위해 서버를 모사할 수 있습니다.

허용:

- Mock Board fixture
- Mock에서 Toy/Food/Rest 상태 생성
- Mock 내부의 서버 역할 머지 결과 계산
- Mock 전용 chain fixture
- 개발용 Showcase Board

금지:

- Mock에서 만든 기능을 Production 계약인 것처럼 취급
- Production API에 없는 기능을 클라이언트 실제 기능처럼 노출
- Food/Rest Generator를 서버 계약 없이 Production 기능으로 구현
- Mock 편의를 위해 Production DTO 의미 변경

`MERGEGAME_USE_LIVE_SERVER` 경로는 기존 Production 연결을 보호합니다.

## 7. Unity UI 원칙

현재 메인 플레이 화면은 **UI Toolkit** 기반입니다.

다음을 우선 유지합니다.

- `GameHud.uxml`
- `GameHud.uss`
- `GameHudPresenter.cs`
- 기존 Safe Area 처리
- 기존 Responsive Layout
- UI Toolkit `VisualElement` 기반 Board
- Prefab 없는 동적 Board Cell 표시

특별한 이유 없이 uGUI로 전체 전환하지 않습니다.

## 8. 현재 기준 메인 화면 구조

Portrait 메인 화면의 기준 구조:

```text
Energy / Coin HUD
Quest Card
Toy Generator
4 × 4 Merge Board
Mascot + Speech Bubble
Bottom Navigation
```

이 구조는 현재 Presentation 기준 레이아웃으로 취급합니다. 새 작업 때문에 보드가 잘리거나 Scroll이 생기지 않도록 주의합니다.

우선순위:

```text
Board
> Generator
> HUD
> Quest
> Mascot
> Navigation 장식
```

## 9. Art 사용 원칙

실사용 아트 경로와 규칙은 `Docs/AI/ART_GUIDE.md`를 따릅니다.

핵심 원칙:

- 개별 게임 아이템은 개별 PNG
- `Sprite Mode = Single`
- 투명 배경
- 원본 PNG를 코드 작업 중 임의 resize/crop하지 않음
- 시각적 크기 차이는 Presentation용 `VisualScale`로 보정
- 같은 게임에서 제작된 것처럼 스타일 통일
- Reference/Source 이미지를 실제 UI Sprite처럼 임의 사용하지 않음

## 10. Art Catalog 원칙

현재 머지 아이템 Sprite는 `WorkshopItemArtCatalog`가 담당합니다.

개념:

```text
chainId + level
→ Sprite
→ Presentation VisualScale
```

HUD/Generator/Currency/Mascot/Navigation 아트는 현재 HUD 아트 카탈로그 구조를 우선 사용합니다.

기존 카탈로그의 책임 범위로 해결 가능한 경우, 새로운 Resource Manager나 Addressables 시스템을 만들지 않습니다. Addressables는 현재 사용하지 않습니다.

## 11. Generator 작업 원칙

현재 Production에서 실제 동작하는 Generator는 Toy Generator입니다.

```text
Generator_Toy
→ Production에서 기존 Toy 생성 흐름 사용

Generator_Food
→ 아트 준비됨, Production 생성 계약 미구현

Generator_Rest
→ 아트 준비됨, Production 생성 계약 미구현
```

Food/Rest Generator를 구현할 때는 먼저 서버 API 계약을 확정합니다. 클라이언트가 생성 결과의 `chainId`, `level`, item 결과를 결정해서는 안 됩니다.

## 12. Economy 원칙

현재 서버 Economy 계약에는 최소 다음 개념이 존재합니다.

- Energy
- Max Energy
- Coins
- Revision

Gem Sprite는 준비되어 있지만 서버 Gem 잔액 계약은 아직 없습니다. 따라서 서버 계약 없이 Gem 잔액, 소비, 구매 시스템을 임의로 만들지 않습니다.

## 13. Bottom Navigation 원칙

현재 하단 Navigation:

- Home — 활성
- Collection — Disabled
- Shop — Disabled
- Quest — Disabled

현재 전용 화면이나 Router가 없는 메뉴는 가짜 화면으로 연결하지 않습니다.

금지:

- 빈 화면 전환
- 임시 Shop
- 임시 Collection
- 기능처럼 보이는 "준비 중" 팝업
- 대규모 Router 선행 구현

## 14. Mascot 원칙

현재 기본 마스코트:

`Cat_Mascot_Default.png`

Presentation API:

- `ShowMascot(...)`
- `HideMascot()`
- `SetMascotMessage(...)`

현재 구조를 활용하며 대화 시스템, NPC State Machine, Character Framework를 임의로 확장하지 않습니다.

## 15. 변경 최소화 원칙

작업 요청을 받을 때 먼저 기존 구현으로 해결 가능한지 확인합니다.

우선순위:

1. 기존 코드 재사용
2. 기존 구조의 작은 확장
3. 작은 Presentation/Data 보강
4. 필요한 경우에만 신규 구조 제안
5. 대규모 리팩터링은 명시적 승인 후 진행

"더 깔끔해 보인다"는 이유만으로 아키텍처를 갈아엎지 않습니다.

## 16. 작업 방식

### Step 1 — 분석

- 관련 파일 검색
- 현재 데이터 흐름 확인
- 기존 테스트 확인
- Production / Mock 경계 확인
- 수정 범위 결정

### Step 2 — 최소 변경 구현

기존 구조를 유지한 상태에서 가장 작은 변경을 선택합니다.

### Step 3 — 검증

가능한 범위에서:

- Unity Compile
- EditMode
- PlayMode
- Regression
- `git diff --check`

을 확인합니다.

### Step 4 — 보고

반드시 다음을 요약합니다.

- 수정 파일
- 생성 파일
- 핵심 변경
- 테스트 결과
- Production 영향
- 미해결 사항
- 수동 확인 필요 사항

## 17. Git 정책

기본 정책:

- 사용자의 명시적 요청 전에는 commit 하지 않음
- 사용자의 명시적 요청 전에는 push 하지 않음
- 사용자 파일을 임의 삭제하지 않음
- 중복 파일도 사용자 승인 없이 삭제하지 않음
- 기존 변경사항을 임의 revert하지 않음

작업 완료 시 Git 상태만 보고합니다.

## 18. 테스트 실패 처리

Unity Licensing Client 등 환경 문제로 BatchMode가 실행되지 않으면:

- 테스트를 성공했다고 주장하지 않음
- 실행 전 중단인지 실제 테스트 실패인지 구분
- 가능한 정적 검사 / Editor Compile 결과는 별도로 기록
- 사람이 수동으로 확인해야 할 절차를 명확히 남김

## 19. 문서 갱신

의미 있는 기능 단계가 끝나면 `Docs/AI/PROJECT_STATE.md`를 현재 상태로 갱신합니다.

특히 다음이 변경되면 반드시 반영합니다.

- 구현 완료 기능
- API 계약
- Production/Mock 상태
- 리소스 추가
- 중요한 기술 결정
- 알려진 문제
- 다음 우선순위

`ART_GUIDE.md`와 `ARCHITECTURE.md`는 해당 영역의 기준 자체가 바뀔 때만 수정합니다.

## 20. 금지 사항

명시적인 요청이 없다면 다음을 하지 않습니다.

- 서버/DB 임의 수정
- Production API 임의 변경
- 대규모 리팩터링
- 새 UI Framework 도입
- Addressables 도입
- 외부 Tween/Animation 패키지 설치
- 임의 Asset 다운로드
- 게임 밸런스 임의 확정
- 실제 기능 없는 UI를 활성 기능처럼 노출
- 원본 Art 파일 파괴적 편집
- 사용자 승인 없는 commit/push

## 21. Codex 응답 원칙

장황한 일반론보다 현재 저장소 기준의 구체적인 정보를 우선합니다.

가능하면 다음을 포함합니다.

- 실제 클래스명
- 실제 파일 경로
- 실제 데이터 흐름
- 변경 전/후 차이
- Production 영향 여부
- 테스트 가능 여부
- 수동 확인 항목

추측은 추측이라고 명시합니다.
