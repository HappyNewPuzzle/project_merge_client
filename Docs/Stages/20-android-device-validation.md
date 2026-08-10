# 20단계 — Android 실제 기기 검증

Unity SDK의 adb로 연결 기기를 실제 조회했으며 authorized device는 0대였습니다.
`install-android-device.ps1`은 정확히 한 기기만 허용하고 검증된 APK를 `install -r`로
설치한 뒤 application ID를 실행합니다. 기기가 없거나 unauthorized·복수 연결이면 안전하게
실패합니다. 실제 Keystore 재시작·비행기 모드·safe area 검증은 물리 기기 연결이 필요합니다.

