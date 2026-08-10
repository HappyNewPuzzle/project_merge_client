# 8단계 — 실행 가능한 Scene 조립

`GameClientRoot`가 Bootstrap, 보드, 경제·퀘스트, 소셜 명령과 UI Toolkit HUD를 연결합니다.
빈 슬롯 생성, 두 슬롯 머지, 일일·퀘스트 보상과 친구 추가가 서버 응답 이후 화면에
반영됩니다. `ClientSceneBuilder`가 PanelSettings와 `MainGame.unity`를 재현 가능하게
생성하며 Build Settings의 첫 Scene으로 등록합니다. Editor는 개발용 메모리 저장소,
플레이어 빌드는 플랫폼 보안 저장소를 사용합니다.

2026-08-10 Unity `6000.3.19f1`에서 Scene 생성과 전체 검증을 실행했습니다. 컴파일 오류와
경고는 0개이며 EditMode 13개, PlayMode 11개가 통과했습니다. 개발 서버 환경 변수가
없어 실제 서버 테스트 1개는 건너뛰었습니다.
