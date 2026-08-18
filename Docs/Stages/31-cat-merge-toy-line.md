# 31단계 — Cat Merge Toy Lv01~Lv08

## 기존 구조 재사용

아이템 상태와 다음 머지 결과는 서버 `BoardItemState(chainId, level, isMaxLevel)`가 권위입니다.
별도 ItemData, NextItemId 또는 단계별 Prefab을 만들지 않았습니다. 기존 공용 UI Toolkit 슬롯과
`WorkshopItemArtCatalog`를 확장해 `chainId="toy"`의 레벨 1~8 Sprite만 연결했습니다.

## Import 설정

사용자 제공 `Assets/Art/CatMerge/Items/Toy/Toy_Lv01~08.png`는 1254×1254 알파 PNG입니다.
각 파일을 Sprite/Single, alpha transparency, mipmap off, NPOT scale none, bilinear, clamp,
100 PPU, max 2048, compressed로 통일합니다. 원본 픽셀은 편집하지 않았습니다.

## 머지와 Mock

Mock 생성기는 Toy Lv01을 반환합니다. 같은 `toy` chain과 같은 level만 기존 서버형 머지 경로를
통과하며 Lv08 응답에는 `isMaxLevel=true`가 설정됩니다. 실제 서버에서도 toy chain Lv01~08과
최대 레벨 정의가 있어야 같은 동작을 합니다. 클라이언트는 NextItemId를 계산하지 않습니다.

## 중복 파일

`Assets/MergeGame/Art/Items/Toy`에도 SHA-256이 같은 미추적 사본이 있으나 사용자 파일일 수 있어
삭제하거나 연결하지 않았습니다. 정식 연결 경로는 요청한 `Assets/Art/CatMerge/Items/Toy`입니다.

## 검증

EditMode에서 Toy Sprite 8개와 Lv01/Lv08 이름·범위를 확인했습니다. PlayMode에서 생성기 두 번으로
Toy Lv01 두 개를 만들고 실제 머지 경로가 Toy Lv02 및 해당 Sprite로 연결되는지 검증했습니다.
별도 Lv07 두 개 머지는 서버 응답이 Lv08과 `isMaxLevel=true`를 반환하는지 확인했습니다.
최종 결과는 EditMode 23개 통과, PlayMode 15개 통과, 실제 서버 테스트 1개 조건부 건너뜀입니다.
