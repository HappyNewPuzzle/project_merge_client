# 48단계: Mock Toy/Food/Rest 체인 검증

## chainId

- Toy: `toy` — 기존 Mock Generator, PlayMode 테스트와 Art Catalog에서 사용 중인 키
- Food: `food` — 기존 Mock 머지 허용 목록과 Art Catalog에서 준비된 Mock 표시 키
- Rest: `rest` — 기존 Mock 머지 허용 목록과 Art Catalog에서 준비된 Mock 표시 키

Food/Rest 키는 현재 Production 콘텐츠 계약을 추가한 것이 아니며 Editor Offline Mock과 표시 카탈로그 검증에만 사용한다.

## Mock Showcase Board

`MockServerState.CreateArtShowcase()`를 명시적 개발 fixture로 추가했다.

```text
Row 1: Toy Lv01 | Toy Lv01 | Food Lv01 | Food Lv01
Row 2: Rest Lv01 | Rest Lv01 | Toy Lv02 | Toy Lv02
Row 3: Empty × 4
Row 4: Empty × 4
```

Editor에서 `MERGEGAME_USE_LIVE_SERVER`가 없는 Offline Game View만 이 fixture를 사용한다. `CreateOffline()`의 기본 빈 상태, Live Server 분기와 Player/Production 조립은 변경하지 않았다. Toy Generator는 첫 빈 슬롯에 계속 `toy` Lv01만 생성한다.

## Art와 VisualScale

기존 `WorkshopItemArtCatalog.Find(chainId, level)` 경로를 그대로 사용한다. Toy/Food/Rest Lv01~Lv08 총 24개 Sprite 조회 테스트가 이미 있으며 fallback 없이 각 체인 배열을 사용한다.

- Food: `1.35, 0.97, 0.97, 0.97, 0.97, 0.97, 0.97, 0.97`
- Rest: `0.95, 0.97, 0.97, 0.98, 0.95, 0.97, 0.95, 0.97`

이번 단계에서 VisualScale 값, PNG, Cell 또는 UI Layout은 변경하지 않았다.

## Merge 검증

기존 Mock API는 동일 chainId/level만 합치며 Mock 서버 상태에서 level과 revision을 갱신한다. 세 체인의 Lv01→Lv02, Lv07→Lv08 테스트를 유지하고 다음을 추가했다.

- Toy/Food, Toy/Rest, Food/Rest Cross-chain 거부
- Toy/Food/Rest Lv08 서버 표시 최종 단계 거부
- Showcase에서 Generator가 여전히 Toy만 생성

클라이언트 Production 경로에 `level + 1`, NextItem 또는 NextItemId 로직을 추가하지 않았다.

## 검증 상태

- 정적 코드 및 `git diff --check`: 성공
- Art Catalog: Toy/Food/Rest 각 8개 참조 테스트 존재
- 신규 PlayMode 테스트: 작성 완료
- 원본 Editor: 현재 Play Mode/assembly reload 상태로 최신 변경 재컴파일 대기
- 복제본 BatchMode: Unity Licensing Client 연결 실패로 테스트 실행 전에 중단

Unity Editor에서 Play Mode를 멈춘 뒤 compile이 완료되면 Game View에 Showcase Board가 표시된다. 이후 각 쌍을 직접 Merge해 Sprite와 체감 크기를 확인한다.

서버 저장소, Production API/DTO와 DB는 수정하지 않았으며 commit/push도 수행하지 않았다.
