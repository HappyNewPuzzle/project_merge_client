# 18단계 — 릴리스 준비도와 산출물 무결성

`release-readiness.ps1`은 깨끗한 Git 상태, 전체 Unity 테스트, 실제 HTTPS 운영 주소,
요구 산출물 존재를 확인하고 SHA-256 manifest를 생성합니다. Android release workflow도
서명 AAB와 별도의 checksum artifact를 보존합니다.

현재 개발 Windows EXE와 Android APK는 생성됐지만 운영 URL과 signing environment가 없어
release-ready 판정은 의도적으로 실행하지 않았습니다. Docker/MySQL 실제 서버, iOS
macOS/Xcode, Android production keystore가 준비된 뒤 이 gate를 통과해야 출시할 수 있습니다.
PowerShell 구문과 전체 Unity 컴파일·테스트 기반은 검증했습니다.
