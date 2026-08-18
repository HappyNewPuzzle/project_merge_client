# 30단계 — 생성기 UX

보드 위에 고정 생성기를 배치했습니다. 생성기를 누르면 현재 서버 스냅샷의 첫 빈 슬롯을 기존
`GenerateItem` 요청의 `targetSlot`으로 전달합니다. 보드 빈 칸 직접 클릭 생성은 제거했습니다.
에너지 차감, 아이템 종류·레벨, 보드·경제 revision은 이전과 동일하게 Mock 또는 실제 서버가
확정한 응답만 적용합니다.

생성기는 도구상자 Sprite를 사용하고 클릭 시 짧은 눌림 피드백을 표시합니다. 에너지가 없거나
빈 슬롯이 없으면 비활성화되며, composition root에서도 같은 조건을 다시 검사합니다. 향후 서버
전용 generator endpoint가 배포되면 UI는 유지하고 Board command 구현만 교체할 수 있습니다.

서버 변경 요청은 `Docs/Prompts/server-generator-api.md`에 별도로 기록했습니다.

최종 통합 검증 기준으로 EditMode 23개와 PlayMode 15개가 통과했고, 실제 서버 통합 테스트
1개는 서버 주소가 없어 조건부 건너뛰었습니다.
