# 25단계 — 오프라인 Mock 서버

## 구조

`MockMergeGameApiClient`가 `IMergeGameApiClient` 전체 계약을 구현하고 `MockServerState`가 보드,
경제, 퀘스트와 소셜 상태의 개발용 서버 권한을 가집니다. 클라이언트 명령 계층은 실제 API와
동일하게 revision을 요청에 포함하며 Mock 응답을 받은 뒤에만 로컬 상태를 갱신합니다.

Unity Editor는 기본적으로 `GameClientContextFactory.CreateOffline()`을 사용하므로 서버와 socket
없이 Bootstrap, 아이템 생성·머지, 보상과 소셜 UI를 실행할 수 있습니다. 실제 개발 서버를
사용할 때만 `MERGEGAME_USE_LIVE_SERVER` scripting define을 명시적으로 추가합니다.

## 오류 시나리오

`NextScenario`에 NetworkError, Unauthorized, AccountSuspended 또는 RevisionConflict를 지정하면
다음 요청 한 번만 해당 오류가 발생합니다. `LatencyFrames`로 동시 요청과 로딩 UI도 재현할 수
있으며 토큰 원문을 로그에 출력하지 않습니다.

## 배포 차단

Staging/Production 런타임에서는 Offline factory가 실패하며 Android Release 빌드는
`MERGEGAME_OFFLINE_MOCK` define을 거부합니다. 배포 빌드는 기존 HTTPS 공개 설정과 실제 API만
사용합니다.

## 테스트

PlayMode에서 서버 없이 게스트 Bootstrap → 아이템 2개 생성 → 머지 → revision·에너지·퀘스트
응답 적용을 검증합니다. 별도 테스트는 403 `account_suspended` 단발 주입과 자동 초기화를
검증합니다. 실제 서버 계약 테스트는 환경 변수가 있을 때만 실행되는 기존 테스트로 유지합니다.

최종 검증 결과는 Unity 컴파일 성공, 보안 스캔 성공, EditMode 18개 통과, PlayMode 14개 통과,
실제 서버 통합 테스트 1개 조건부 건너뜀입니다. 테스트 과정에서 머지 후 퀘스트는 같은 객체
참조로 바뀌지 않고 명시적 Reload 응답으로만 갱신되는 것도 확인했습니다.
