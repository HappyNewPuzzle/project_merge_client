# 서버 세션 전달용 프롬프트 — 서버 권위형 생성기 API

아래 요청을 `C:\Users\happy\mergegame` 서버 저장소에서 수행해주세요. 클라이언트 저장소는
수정하지 말고, 서버 저장소의 `AGENTS.md`와 단계 문서를 먼저 확인해주세요.

## 목표

요즘 머지 게임처럼 고정 생성기를 눌렀을 때 서버가 빈 슬롯과 생성 아이템을 결정하는 API를
`/api/v1`에 추가합니다. 기존 `POST /api/v1/economy/generate` 계약은 현재 클라이언트 호환성을
위해 즉시 삭제하거나 의미를 바꾸지 말고 유지해주세요.

## 요구사항

1. `POST /api/v1/board/generators/{generatorId}/produce`를 추가합니다.
2. 요청에는 `expectedBoardRevision`, `expectedEconomyRevision`, 재시도 안전성을 위한
   `idempotencyKey`를 포함하고 `targetSlot`, 아이템 종류·레벨·보상값은 받지 않습니다.
3. 인증된 플레이어만 호출할 수 있고 기존 정지 계정 흐름을 그대로 적용합니다.
4. 서버가 generator ID를 검증하고, 서버 보드의 첫 빈 슬롯 또는 명시한 서버 정책으로 슬롯을
   선택하며, 아이템 catalog/확률표에서 결과를 결정합니다.
5. 에너지 비용, 쿨다운, 잔여 충전 횟수와 생성 가능 테이블은 서버 권위 상태입니다. 초기 버전은
   결정적 레벨 1 workshop 아이템이어도 되지만 확장 가능한 generator definition을 둡니다.
6. 보드가 가득 참, 에너지 부족, 알 수 없는 생성기, 쿨다운, revision 충돌을 서로 다른 안정적인
   error code로 반환합니다. 409 revision 충돌에는 최신 revision과 가능한 최신 snapshot을 줍니다.
7. 보드 변경과 에너지 차감을 하나의 DB transaction으로 처리하고 동시 요청에서 이중 차감이나
   이중 생성을 허용하지 않습니다.
8. 같은 player와 idempotencyKey 재시도는 같은 성공 응답을 반환하며 추가 차감하지 않습니다.
9. 성공 응답에는 `board`, `economy`, 생성된 `item`, `targetSlot`, generator cooldown/charge 상태를
   포함합니다. 클라이언트가 결과를 계산할 필요가 없어야 합니다.
10. 관리자 API나 `X-Admin-Key`를 이 endpoint에 사용하지 않습니다. token 원문을 로그에 남기지 않습니다.
11. OpenAPI DTO·예제·오류 응답을 갱신하고 기존 Unity DTO와의 호환 영향을 문서화합니다.
12. domain/application/endpoint/integration 테스트로 정상 생성, full board, insufficient energy,
    unknown generator, stale revisions, suspension, 동시성, idempotent replay를 검증합니다.
13. 기존 `/economy/generate`의 deprecation 계획을 문서에만 기록하고 이번 변경에서는 제거하지 않습니다.
14. 새 단계 문서를 작성하고 전체 서버 테스트를 실행한 뒤, 변경 파일과 테스트 결과를 요약하고
    서버 저장소에만 하나의 명확한 커밋으로 커밋·`main` 푸시해주세요.
