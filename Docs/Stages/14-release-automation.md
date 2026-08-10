# 14단계 — 릴리스 배포 자동화

수동 승인형 GitHub Actions가 전체 검증과 배포 주소 검사를 통과한 뒤 서명된 Android
AAB를 생성합니다. semantic version, Android version code, 운영 공개 주소는 workflow
입력/변수로 받고 keystore 경로와 암호·alias는 production environment secret으로만
주입합니다. 저장소와 artifact 로그에는 비밀값을 기록하지 않습니다.

개발 APK는 실제 생성해 검증했지만 서명된 AAB는 운영 keystore와 승인된 production
environment가 없어 로컬에서 실행하지 않았습니다. iOS 릴리스는 macOS/Xcode runner와
Apple signing 자산을 별도 추가해야 합니다.

2026-08-10 Unity 컴파일 오류·경고 0개, EditMode 15개와 PlayMode 11개 통과로 release
entry point를 검증했습니다. 개발 서버 테스트 1개는 환경 부재로 건너뛰었습니다.
