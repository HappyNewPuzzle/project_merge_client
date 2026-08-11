# 29단계 — Main Camera 구성

`MainGame.unity`에 `MainCamera` 태그를 가진 활성 Orthographic Camera와 AudioListener를
추가했습니다. Camera는 어두운 단색으로 화면을 지우므로 Game View의 `Display 1 / No cameras
rendering` 안내가 더 이상 나타나지 않습니다. UI Toolkit HUD는 기존 Screen Space Overlay로
Camera 위에 렌더링되므로 입력과 Safe Area 동작은 바뀌지 않습니다.

`ClientSceneBuilder.Build`도 동일한 Camera를 생성하므로 Scene을 다시 만들어도 설정이
유지됩니다. 향후 배경, 파티클과 월드 공간 머지 효과는 이 Camera의 렌더링 계층에 추가할 수
있습니다. EditMode Scene composition 테스트는 MainCamera, Orthographic, SolidColor clear와
AudioListener 구성을 검증합니다.

최종 검증은 Scene 재생성 성공, 보안 스캔 성공, EditMode 21개 통과, PlayMode 14개 통과 및
실제 서버 통합 테스트 1개 조건부 건너뜀입니다.
