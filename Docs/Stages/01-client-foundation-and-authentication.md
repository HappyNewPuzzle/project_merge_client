# 1단계 — 클라이언트 기반 및 인증

## 목표와 환경

빈 클라이언트 저장소에 Unity `6000.3.19f1` 프로젝트를 생성하고, 서버의 안정 계약인
`/api/v1`에 연결되는 인증·초기화 기반을 구현했습니다. 서버 저장소는 계약 확인에만
사용했으며 변경하지 않았습니다.

## 구조

- `Runtime/Api`: 서버 DTO, `UnityWebRequest` 클라이언트, 정규화 오류 모델
- `Runtime/Authentication`: 인증 상태, 보안 저장 경계, 개발용 메모리 저장소, refresh 조정자
- `Runtime/Bootstrap`: 게스트 생성부터 네 가지 게임 상태 초기화까지의 흐름과 결과
- `Runtime/Configuration`: 개발·스테이징·운영의 공개 base URL 선택
- `Tests/EditMode`, `Tests/PlayMode`: 순수 상태·오류 분류 및 코루틴 동시성 검증

보드, 경제, 퀘스트 DTO의 `revision`은 서버 응답을 그대로 보관합니다. 클라이언트가
코인, 에너지, 아이템 레벨 또는 revision을 계산해 최종 확정하지 않습니다.

## 인증과 Bootstrap 흐름

1. `ISecureTokenStore`에서 저장된 `GuestCredential`을 조회합니다.
2. 없으면 `POST /players/guest`를 호출하고 player ID와 guest token을 즉시 저장합니다.
3. 게스트 자격 증명으로 `POST /auth/guest`에 로그인합니다.
4. access/refresh token 쌍을 한 `SaveSession` 호출로 저장하고 API 클라이언트에 access token을 설정합니다.
5. 보드, 경제, 퀘스트, 소셜 프로필을 서버의 멱등 초기화 API로 순서대로 초기화합니다.
6. 모두 성공한 경우에만 네 가지 서버 스냅샷을 포함한 완료 결과를 반환합니다.

네트워크 실패, 401, `403 account_suspended`, 409 충돌을 `ApiErrorKind`로 구분합니다.
409 이후에는 다음 단계의 상태 관리 계층에서 최신 상태를 다시 조회하고 사용자 동작의
유효성을 재판단해야 합니다.

## 토큰 보관과 보안 경계

`ISecureTokenStore`는 저장 방식을 런타임 흐름에서 분리합니다. 현재 제공되는
`InMemoryTokenStore`는 Editor 개발·테스트 전용이며 종료 시 사라집니다. 모바일 출시
전에 Android Keystore와 iOS Keychain 기반 구현을 별도 assembly로 추가해야 합니다.

guest token, refresh token, access token 원문은 로그, PlayerPrefs, 분석 이벤트에
기록하지 않습니다. 환경 설정에는 공개 base URL만 두며 비밀값을 ScriptableObject나
Git에 저장하지 않습니다. 관리자 API, `X-Admin-Key`, 인증서 검증 우회 구현은 포함하지
않았습니다.

## Refresh 단일화

서버 refresh token은 사용 즉시 회전하므로 동시에 두 번 제출하면 재사용 공격으로
탐지될 수 있습니다. `TokenRefreshCoordinator`의 첫 코루틴만 refresh API를 호출하고,
나머지는 진행 플래그가 해제될 때까지 기다린 뒤 같은 결과를 공유합니다. 성공 시 새
access/refresh token을 함께 교체합니다. refresh가 401이면 폐기·만료된 세션으로 보고
로컬 세션을 제거합니다.

## 검증 결과

2026-08-04에 Unity `6000.3.19f1` batchmode로 확인했습니다.

- 프로젝트 스크립트 컴파일: 성공, C# 오류 0개, 경고 0개
- EditMode: 6개 통과, 실패 0개, 건너뜀 0개
- PlayMode: 1개 통과, 실패 0개, 건너뜀 0개
- 동시 refresh 코루틴 2개에 대한 refresh API 호출 횟수: 1회
- 보안 문자열 정적 검사: 관리자 키·인증서 우회·토큰 로그 코드 없음

## 다음 단계 제안

2단계에서는 플랫폼별 보안 저장소 어댑터, 보호 API의 401 자동 refresh 및 원 요청 1회
재시도, 409 상태 재조회 정책을 구현하는 것이 적합합니다. 이어서 Bootstrap 결과를
게임 상태 저장소와 첫 화면에 연결하고, 실제 개발 서버를 사용한 통합 PlayMode 테스트를
추가할 수 있습니다. 스테이징·운영의 `.invalid` 예시 주소는 배포 전에 CI가 관리하는
공개 환경 설정으로 반드시 교체해야 합니다.
