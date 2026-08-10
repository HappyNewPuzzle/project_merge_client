# 24단계 — 스토어 Internal 출시 기반

`BuildAndroidStoreRelease`는 keystore 경로와 비밀번호를 환경 변수에서만 읽고 빌드 후 Editor
설정을 복원합니다. 수동 GitHub workflow는 보호된 `google-play-internal` environment에서 서명
AAB를 만든 뒤 Google Play internal track에 `draft`로 업로드하며 임시 keystore를 항상 제거합니다.

스토어 설명, 개인정보 처리방침 초안과 출시 체크리스트를 `Store/`에 추가했습니다. 현재는 Play
Console 앱, 업로드 키, 서비스 계정, 법률 검토된 공개 개인정보 처리방침이 없어 실제 업로드를
실행하지 않았습니다. 이 외부 준비가 끝난 뒤 environment 승인으로만 배포해야 합니다.
