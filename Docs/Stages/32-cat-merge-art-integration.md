# 32단계 — Cat Merge 아트 통합

## 구조와 계약 경계

서버 권위형 `BoardItemState(chainId, level, isMaxLevel)` 구조와 기존 UI Toolkit 보드 표시를 유지했다.
클라이언트에 `NextItem`, 별도 아이템 도메인 모델 또는 머지 결과 규칙을 추가하지 않았다.

서버 저장소 검색 결과 정식 콘텐츠에는 현재 `garden` 체인이 있으며 Food/Rest 체인 계약은 없다.
클라이언트의 기존 오프라인 생성기는 `toy`를 사용한다. 따라서 Food/Rest는 Sprite 표시 준비와 주입된
Mock 보드의 공통 머지 회귀 검증만 제공하며, Food/Rest 생성 API나 DTO는 추가하지 않았다.

## 아트 카탈로그

기존 `WorkshopItemArtCatalog`에 Toy, Food, Rest의 Lv01~Lv08 배열을 연결했다. `Find`의
`food`/`rest` 키는 서버에서 해당 값을 받았을 때 그림을 찾기 위한 표시 후보 키일 뿐 서버 계약을
정의하지 않는다.

머지 아이템이 아닌 Generator와 Currency는 작은 `WorkshopHudArtCatalog`로 분리했다.
Toy 생성기와 Coin/Energy는 현재 HUD에서 사용한다. Food/Rest 생성기와 Gem은 향후 서버 계약을 위해
Sprite 참조만 보관한다.

## Sprite Import

Toy에서 검증된 정책을 모든 개별 PNG에 동일하게 적용했다.

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: 활성
- Mip Maps: 비활성
- NPOT Scale: None
- Filter Mode: Bilinear
- Wrap Mode: Clamp
- Pixels Per Unit: 100
- Max Size: 2048
- Compression: Compressed

기존 Toy `.meta`, 중복 `Assets/MergeGame/Art/Items/Toy`, 캐릭터 원본 Sprite Sheet는 변경하지 않았다.

## HUD

기존 임시 Workshop Lv5 생성기 그림을 `Generator_Toy`로 교체했다. Energy와 Coin을 각각 아이콘과
값으로 분리했으며 Energy는 서버의 `energy / maxEnergy`, Coin은 천 단위 구분 형식으로 표시한다.
재화 revision 처리와 생성기 요청 흐름은 변경하지 않았다.

## Mock와 테스트

Mock 생성기는 계속 Toy Lv01만 생성한다. Food/Rest 생성 기능은 추가하지 않았다. 주입된 Food/Rest
보드 상태에 대해서만 기존 공통 머지 경로가 Lv01→Lv02 및 Lv07→Lv08을 처리하는지 검증한다.
Lv08에서는 Mock 응답의 `isMaxLevel=true`와 클라이언트 머지 차단을 확인한다.

- Unity Compile: 성공, 오류 0개
- EditMode: 28/28 성공
- PlayMode: 16 성공, 1개 건너뜀, 실패 0개
- 건너뜀 1개: 외부 개발 서버가 필요한 기존 통합 테스트

Import 로그에는 Unity License Client 재연결 메시지가 있었다. EditMode 컴파일에는 이번 변경과
무관한 기존 `WorkshopArtImporter`의 `TextureImporter.spritesheet` obsolete 경고 1종이 두 번
출력되었다. PlayMode 로그에는 C# 경고와 오류가 없다.

## 다음 단계

실제 서버 콘텐츠 카탈로그에 Food/Rest 체인과 생성기 종류가 추가되면 서버가 정한 정확한 chainId로
표시 키를 확정하고 Food/Rest 생성 UI를 활성화한다. Gem은 경제 DTO와 서버 권위 잔액이 추가되기
전까지 화면 수치를 만들지 않는다. Unity Game View에서는 아이콘의 체감 크기와 투명 여백을 수동으로
확인한다.
