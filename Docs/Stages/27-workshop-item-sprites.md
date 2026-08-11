# 27단계 — Workshop 아이템 Sprite 적용

`WorkshopItemsAtlas.png` 원본을 유지하면서 Unity Multiple Sprite importer로 정확한 3×3 셀을
분할했습니다. 위에서 아래, 왼쪽에서 오른쪽 순서로 서버 `workshop` 체인의 레벨 1~9에
seed packet, sprout pot, leafy plant, flower bouquet, toolbox, watering can, honey jar, yarn ball,
lantern을 연결합니다.

`WorkshopItemArtCatalog`는 Resources 공개 자산으로 Sprite 참조만 저장하며 비밀값이나 게임 상태를
포함하지 않습니다. HUD는 서버 또는 Mock 응답의 `chainId`와 `level`로 Sprite를 조회해 실제 보드
슬롯과 드래그 유령에 표시합니다. 알 수 없는 체인이나 범위 밖 레벨에는 그림을 추측하지 않고
텍스트 fallback을 유지합니다. 기존 상단 전체 atlas 미리보기는 제거했습니다.

`WorkshopArtImporter.Configure` 메뉴/CI 메서드로 같은 분할과 카탈로그를 재생성할 수 있습니다.
EditMode 테스트는 9개 Sprite, 첫·마지막 이름과 알 수 없는 체인 fallback을 확인합니다.

최종 검증은 아틀라스 분할 및 카탈로그 생성 성공, 보안 스캔 성공, EditMode 21개 통과,
PlayMode 14개 통과 및 실제 서버 통합 테스트 1개 조건부 건너뜀입니다.
