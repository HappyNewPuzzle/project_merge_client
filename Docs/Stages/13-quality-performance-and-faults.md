# 13단계 — 품질·성능·장애 대응

`ClientDiagnostics`는 제한된 ring buffer에 HTTP 상태, 오류 종류, 지연과 trace ID만
저장합니다. URL 본문과 access/refresh/guest token은 수집하지 않습니다. 장시간 세션에도
관측 데이터가 무한 증가하지 않습니다.

`NetworkRetryPolicy`는 네트워크 오류 중 읽기 요청 또는 안정적인 멱등성 키가 있는 요청만
최대 2회 시도 범위에서 허용합니다. 결과를 알 수 없는 머지·생성·일일 보상은 자동
반복하지 않습니다. EditMode에서 메모리 상한과 재시도 안전 규칙을 검증합니다.

2026-08-10 전체 검증 결과 컴파일 오류·경고 0개, EditMode 15개와 PlayMode 11개가
통과했습니다. 개발 서버 통합 테스트 1개는 환경 부재로 건너뛰었습니다.
