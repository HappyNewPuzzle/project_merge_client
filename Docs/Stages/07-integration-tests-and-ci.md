# 7단계 — 통합 테스트와 CI·배포 기반

## 단계 목표

Unity 컴파일, EditMode, PlayMode, 보안 회귀 검사를 한 명령으로 반복하고 GitHub Actions의
Windows self-hosted runner에서 동일하게 실행하도록 구성했습니다.

## 검증 스크립트

`scripts/verify-client.ps1`은 다음 순서로 실행합니다.

1. 관리자 키, 인증서 우회, PlayerPrefs token 저장, token 로그 패턴 검사
2. 선택적으로 스테이징·운영 `.invalid` 주소 잔존 검사
3. Unity batchmode 컴파일
4. EditMode 및 PlayMode 테스트
5. NUnit XML의 실패 수 확인

Unity 실행은 `Start-Process -Wait`를 사용해 각 단계 종료와 exit code를 확인합니다.

## GitHub Actions

`.github/workflows/unity-client.yml`은 `main` push, pull request와 수동 실행을 지원합니다.
Unity `6000.3`과 라이선스가 준비된 Windows self-hosted runner를 사용하고 저장소 변수
`UNITY_EDITOR_PATH`로 Editor 실행 파일 위치만 받습니다. 테스트 XML과 로그는 실패 시에도
artifact로 보존합니다. 토큰, 관리자 키, 자체 서명 인증서 우회 설정은 CI에 넣지 않습니다.

## 실제 서버 통합 테스트

`DevelopmentServerIntegrationTests`는 `MERGEGAME_INTEGRATION_BASE_URL`이 명시된 경우에만
게스트 생성, 로그인, 보드·경제·퀘스트·소셜 초기화를 실제 `/api/v1` 서버에서 검사합니다.
주소가 없으면 테스트를 명시적으로 건너뜁니다. 토큰 원문은 assertion이나 로그에 포함하지
않고 실패 문의에는 trace ID만 사용합니다.

## 배포 전 확인

- `verify-client.ps1 -RequireDeploymentUrls` 통과
- 서명된 Android/iOS 빌드에서 Keystore/Keychain 재시작 복원 확인
- 개발용 `InMemoryTokenStore`가 플레이어 빌드 조립에 사용되지 않는지 확인
- 관리자 API 및 `X-Admin-Key` 문자열 부재 확인
- 실제 서버 smoke test 통과 후 테스트 계정 정리 정책 확인

## 검증 결과

2026-08-10에 Unity `6000.3.19f1`을 전달해 `verify-client.ps1` 전체를 실행했습니다.

- 보안 금지 패턴 검사 통과
- Unity batchmode 컴파일 성공, C# 오류 0개, 경고 0개
- EditMode 13개 통과, 실패 0개, 건너뜀 0개
- PlayMode 11개 통과, 실패 0개, 개발 서버 통합 테스트 1개 건너뜀
- 통합 테스트 건너뜀 이유: `MERGEGAME_INTEGRATION_BASE_URL`을 의도적으로 제공하지 않음
- CI 스크립트 최종 exit code 0

실제 서버 smoke test는 개발 서버와 테스트 데이터 생성 승인이 준비된 runner에서 환경
변수를 제공해 별도로 실행해야 합니다.
현재 작업 PC에서는 Docker daemon이 실행 중이지 않아 MySQL 기반 개발 서버를 안전하게
기동할 수 없었으며, 서버 저장소나 데이터베이스 설정을 임의 변경하지 않았습니다.
