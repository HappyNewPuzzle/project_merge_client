# 17단계 — Scene E2E와 장시간 품질 검증

EditMode가 `MainGame.unity`를 직접 열어 composition root, HUD, 모바일 세션, safe area,
PanelSettings와 UXML 연결을 검사합니다. 10,000회 요청 관측을 모사해 진단 메모리가 50개
상한을 유지하는 장시간 세션 테스트도 추가했습니다. 실제 서버 E2E는 Docker 환경이 없어
기존 선택적 PlayMode 테스트로 유지합니다. Windows 플레이어와 Android APK smoke 산출물은
11단계에서 실제 생성했습니다.

2026-08-10 Scene E2E가 누락된 PanelSettings 직렬화를 발견해 builder를 수정했습니다.
재검증 결과 EditMode 17개가 모두 통과했으며 기존 PlayMode 11개 통과 기준을 유지합니다.
