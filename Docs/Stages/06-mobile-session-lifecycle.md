# 6단계 — 모바일 세션 수명주기

## 단계 목표

Android/iOS 앱의 시작과 포그라운드 복귀에서 보안 저장 세션을 복원하고, access token
만료가 임박했을 때 refresh를 선제 실행하도록 런타임 객체를 조립했습니다.

## 세션 복원 정책

`SessionLifecycleCoordinator`는 저장된 `AuthSession`을 읽고 만료 UTC를 파싱합니다.
만료까지 2분보다 많이 남았으면 access token을 API 클라이언트 메모리에 복원합니다.
만료가 임박했거나 날짜를 파싱할 수 없으면 `TokenRefreshCoordinator`를 사용합니다.
따라서 포그라운드 이벤트가 겹쳐도 회전형 refresh token은 병렬 제출되지 않습니다.

`MobileSessionController`는 `OnApplicationFocus`와 `OnApplicationPause` 복귀를 처리하며,
자체 진행 플래그로 한 컴포넌트의 중복 검사를 막습니다. 토큰 원문은 로그하지 않습니다.

## 런타임 조립

`GameClientContextFactory`는 공개 환경 주소, 플랫폼 보안 저장소, 원시 API, refresh
조정자, 복원력 API, 게임 상태와 기능별 명령을 하나의 앱 수명 객체로 조립합니다.
Editor 테스트는 `Create(baseUrl, InMemoryTokenStore)`를 사용하고 실제 플레이어 빌드는
`CreateForPlayer`로 Keystore/Keychain을 선택합니다. 지원하지 않는 플랫폼에서는 평문
저장으로 폴백하지 않습니다.

Android 최소 SDK는 프로젝트 설정상 API 25로, AndroidKeyStore AES-GCM 요구 API 23
이상을 충족합니다. iOS Keychain 항목은 기기 한정 접근 속성을 유지합니다.

## 검증 결과

2026-08-10 Unity `6000.3.19f1` batchmode로 검증했습니다.

- 컴파일 성공, C# 오류 0개, 경고 0개
- EditMode 13개 통과, 실패 0개, 건너뜀 0개
- PlayMode 11개 통과, 실패 0개, 건너뜀 0개
- 유효 access token의 refresh 없는 메모리 복원 검증
- 만료 임박 access token의 refresh 1회와 새 token 적용 검증

실제 Android/iOS 기기에서 앱 삭제·재설치, OS 백업, 생체 인증 정책을 포함한 검증은
서명된 빌드와 기기가 필요한 배포 전 체크리스트로 남깁니다.

## 다음 단계 제안

7단계에서는 자동 Unity 테스트, 보안 문자열 검사, 환경 주소 검사와 빌드 entry point를
CI에 연결하고 실제 개발 서버용 선택적 통합 테스트를 추가합니다.
