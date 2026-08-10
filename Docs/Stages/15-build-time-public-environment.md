# 15단계 — 빌드 시 공개 환경 설정

릴리스 workflow의 `MERGEGAME_PRODUCTION_BASE_URL`을 검증만 하고 실제 플레이어에 포함하지
않던 결함을 수정했습니다. Release builder가 실제 HTTPS 주소를 임시 Resources JSON으로
생성하고 BuildPlayer가 끝난 뒤 해당 파일만 삭제합니다. 런타임은 이 공개 파일을 우선
읽으며 `.invalid`, 비 HTTPS 주소 또는 누락된 운영 변수는 빌드 전에 실패합니다. 이
파일에는 공개 base URL과 환경 이름만 들어가며 token·관리자 키·서명 비밀은 없습니다.

2026-08-10 전체 Unity 검증 결과 컴파일 오류·경고 0개, EditMode 15개와 PlayMode 11개가
통과했습니다. `Assets/Resources/PublicServerConfiguration.json`은 저장소에 남지 않았고
운영 변수·서명 자산이 없으므로 실제 release build는 안전 gate에서 보류했습니다.
