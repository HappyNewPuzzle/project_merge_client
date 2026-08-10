# 9단계 — 실제 개발 서버 통합

`verify-development-server.ps1`은 `/health` HTTP 200을 먼저 확인한 뒤, 서버 주소를 현재
프로세스 환경에만 주입하고 Unity의 실제 `/api/v1` smoke test를 실행합니다. 테스트는
게스트 생성·로그인과 보드·경제·퀘스트·소셜 초기화를 검증하며 토큰을 출력하지 않습니다.

```powershell
.\scripts\verify-development-server.ps1 -UnityPath '<Unity.exe>' -BaseUrl http://localhost:5158
```

2026-08-10 현재 Docker daemon이 실행되지 않아 MySQL 기반 서버를 기동할 수 없었습니다.
따라서 자동화 스크립트와 Unity 통합 계약 테스트의 컴파일은 검증했지만 실제 HTTP 실행은
외부 환경 차단으로 보류했습니다. 서버 저장소와 DB 설정은 변경하지 않았습니다.
