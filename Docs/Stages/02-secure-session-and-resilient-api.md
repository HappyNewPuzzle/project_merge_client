# 2단계 — 보안 세션과 복원력 있는 API

## 단계 목표

1단계 인증 기반 위에 모바일 보안 저장소와 보호 API 복구 정책을 연결했습니다. 서버의
`/api/v1` 계약은 변경하지 않았고 서버 저장소도 수정하지 않았습니다. 클라이언트는
서버가 반환한 상태와 revision만 적용하며 변경 충돌을 임의로 덮어쓰지 않습니다.

## 플랫폼 보안 저장소

`SecureTokenStore`는 `IPlatformSecretStore` 뒤에서 게스트 자격 증명과 인증 세션을 각각
하나의 값으로 직렬화합니다. refresh 회전 성공 시 access/refresh token 쌍을 한 번의
저장 호출로 교체합니다.

- Android: `AndroidKeyStore`에서 비내보내기 AES 키를 생성하고 AES-GCM 암호문과 IV만
  앱 전용 `SharedPreferences`에 저장합니다. API 23 이상을 전제로 합니다.
- iOS: Keychain generic password와
  `kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly`를 사용합니다.
- Editor/테스트: `InMemoryTokenStore`를 명시적으로 주입합니다. 모바일 보안 저장소가
  없을 때 PlayerPrefs나 평문 파일로 자동 폴백하지 않습니다.

토큰 원문은 로그, 분석 이벤트, ScriptableObject, Resources에 기록하지 않습니다.
관리자 API, `X-Admin-Key`, 인증서 검증 우회도 포함하지 않습니다.

## 401 복구와 refresh 단일화

`ResilientMergeGameApiClient`가 공개 전송 클라이언트를 감쌉니다.

1. 보호 API를 현재 access token으로 호출합니다.
2. 401이면 `TokenRefreshCoordinator`에 refresh를 요청합니다.
3. 동시에 여러 요청이 대기해도 서버 refresh API는 한 번만 호출됩니다.
4. 새 token 쌍을 보안 저장소에 교체하고 원 요청을 정확히 한 번만 다시 호출합니다.
5. refresh 실패, 새 token으로도 401, 또는 `403 account_suspended`이면 로컬 세션을
   더 이상 보호 요청에 사용하지 않습니다.

네트워크 오류는 자동 refresh 조건이 아닙니다. 멱등성이 보장되지 않은 변경 요청을
전송 결과 불명 상태에서 자동 반복하지 않기 위해서입니다.

## Revision 충돌과 상태 저장

`RevisionConflictResolver`는 변경 요청의 409를 받으면 같은 변경을 자동 재시도하지
않습니다. 대응하는 조회 API를 한 번 호출해 최신 서버 상태를 반환하고,
`ConflictResynchronized` 결과를 상위 화면에 전달합니다. 화면은 새 상태에서 사용자의
의도가 여전히 유효한지 다시 판단해야 합니다.

`GameStateStore`는 Bootstrap의 보드·경제·퀘스트·소셜 초기화가 모두 성공한 후 한 번에
초기 상태를 공개합니다. 이후에도 `ApplyBoard`, `ApplyEconomy`, `ApplyQuest`에는 서버
응답만 전달합니다. 로컬에서 코인, 에너지, 아이템 레벨 또는 revision을 증가시키는 API는
제공하지 않습니다.

## 테스트와 검증 결과

2026-08-10 기준 검증 항목은 다음과 같습니다.

- Unity `6000.3.19f1` batchmode 컴파일 성공, C# 오류 0개, 경고 0개
- EditMode 7개 통과, 실패 0개, 건너뜀 0개
- PlayMode 3개 통과, 실패 0개, 건너뜀 0개
- 보안 저장 경계를 통한 guest/session 저장·조회·삭제 검증
- 동시 refresh 요청의 서버 호출 1회 공유 검증
- 보호 API 401 후 refresh 1회와 원 요청 1회 재시도 검증
- 409 변경 호출 1회 유지 및 최신 상태 조회 1회 검증
- 네트워크/401/정지 403/409 오류 분류 검증

## 다음 단계 제안

3단계에서는 실제 보드 화면 상태와 입력을 `GameStateStore`에 연결하고, 머지·생성·일일
보상·퀘스트 보상의 명령 계층을 구축하는 것이 적합합니다. 각 명령은 서버 revision과
멱등성 키를 유지하고, 409 재동기화 뒤 사용자 의도 재확인 UI를 제공해야 합니다. Android
및 iOS 실제 기기에서는 앱 재시작 후 KeyStore/Keychain 복원과 OS 백업 제외 정책도
검증해야 합니다.
