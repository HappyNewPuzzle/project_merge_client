# 12단계 — 콘텐츠·지역화·접근성

한국어 화면 문자열을 `KoreanStrings`로 분리해 향후 Unity Localization table로 교체할
경계를 만들었습니다. 보드 슬롯에는 빈 슬롯/아이템·레벨을 설명하는 tooltip과 키보드
focus가 가능한 Button을 사용합니다. `SafeAreaController`는 노치와 시스템 UI 영역을
피하도록 UI Toolkit root 여백을 갱신합니다. 아이템 이름과 레벨은 서버 DTO를 표시할
뿐 클라이언트 콘텐츠가 레벨을 확정하지 않습니다.
