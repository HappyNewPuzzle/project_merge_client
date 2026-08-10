# 19단계 — 실제 서버 통합 실행

Docker Desktop을 실제 실행하고 daemon 준비를 60초 동안 확인했으나 engine이 시작되지
않았습니다. 서버의 기존 검증 스크립트가 난수 DB/JWT 비밀을 메모리에서만 생성하고 기존
볼륨을 삭제하지 않음을 확인했지만 daemon 부재로 MySQL·마이그레이션·서버 smoke를 실행할
수 없었습니다. 서버 저장소는 변경하지 않았습니다. daemon 준비 후 서버의
`scripts/verify-docker-environment.ps1`, 이어서 클라이언트의
`scripts/verify-development-server.ps1`을 실행하면 실제 `/api/v1` 검증을 완료할 수 있습니다.

