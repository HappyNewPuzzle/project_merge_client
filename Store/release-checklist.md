# Google Play Internal 출시 체크리스트

- Play Console 앱과 application ID `com.happynewpuzzle.projectmerge` 확인
- 업로드 키, 비밀번호, alias를 GitHub Environment secret으로 등록
- 최소 권한 Google Play 서비스 계정 JSON 등록
- 실제 HTTPS 운영 URL과 서버 상태·rate limit 확인
- 개인정보 처리방침 법률 검토 및 공개 URL 등록
- 콘텐츠 등급, 데이터 보안 양식, 스크린샷과 아이콘 등록
- `Google Play Internal Release` workflow를 승인 실행하고 draft 릴리스를 수동 검토
- 설치 후 신규·기존 게스트, 토큰 갱신, 계정 정지, revision 충돌 smoke test
