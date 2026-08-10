# Project Merge Unity Client

서버 권위형 머지 퍼즐 게임의 Unity 클라이언트입니다. Unity `6000.3.19f1`을 사용하며,
공개 게임 API `/api/v1`만 호출합니다. 토큰, 재화, 보드와 revision의 최종 권한은 서버에
있습니다.

1단계 구현과 검증 내용은 [단계 문서](Docs/Stages/01-client-foundation-and-authentication.md)에
기록합니다.

## 자동 검증

설치·활성화된 Unity Editor 경로를 전달해 로컬과 Windows self-hosted CI에서 같은 검증을
실행합니다.

```powershell
.\scripts\verify-client.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
```

실제 개발 서버 smoke test는 `MERGEGAME_INTEGRATION_BASE_URL`이 있을 때만 실행됩니다.
스테이징·운영 배포 전에는 `-RequireDeploymentUrls`로 `.invalid` placeholder가 남지 않았는지
검사합니다. CI 변수 `UNITY_EDITOR_PATH`에는 실행 파일 경로만 저장하며 token이나 관리자
키를 저장하지 않습니다.

