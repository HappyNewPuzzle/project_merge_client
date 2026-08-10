# 23단계 — 운영 보안·관측성·계정 정지 UX

지원용 진단은 오류 종류, HTTP 상태, 서버 trace ID만 포함하며 token, URL, 요청 본문과 서버
메시지는 제외합니다. `verify-production-security.ps1`은 관리자 키, 인증서 무조건 허용 구현,
PlayerPrefs 토큰 저장과 token 로그 패턴을 배포 전에 차단합니다. 기존 `account_suspended` 상태는
일반 오류와 분리되어 게임 동작을 중단하고 한국어 안내를 표시합니다.

실제 운영 HTTPS 주소가 제공되지 않아 endpoint·rate limit·관측 대시보드 검증은 실행하지
못했습니다. 배포 환경에서는 `-RequireProductionUrl`과 서버 trace ID 검색을 함께 검증해야 합니다.
