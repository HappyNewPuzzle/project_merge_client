# 39단계: 아이템 시각 크기 정규화

## 목표

PNG 내부의 투명 여백과 오브젝트 실루엣 차이 때문에 같은 보드 셀에서도 아이템 크기가 달라 보이는 문제를 Presentation 계층에서 보정한다. 보드 상태, 슬롯, 머지 규칙 및 서버 계약은 변경하지 않는다.

## 적용 구조

`WorkshopItemArtCatalog`가 기존의 `chainId + level -> Sprite` 매핑과 함께 `VisualScale`을 제공한다. `GameHudPresenter`는 공통 아이템 VisualElement의 기본 크기와 종횡비를 유지하면서 이 값만 UI Toolkit `scale`에 적용한다. 알 수 없는 체인 또는 범위를 벗어난 레벨은 안전하게 `1.0`을 사용한다.

## Toy VisualScale

| Level | 아이템 | VisualScale |
|---:|---|---:|
| 1 | 털실 | 1.69 |
| 2 | 리본 털실 | 1.18 |
| 3 | 방울 공 | 1.45 |
| 4 | 깃털 장난감 | 1.25 |
| 5 | 생선 장난감 | 1.06 |
| 6 | 캣닢 가방 구성 | 0.82 |
| 7 | 바구니 | 1.03 |
| 8 | 장난감 상자 | 0.90 |

값은 PNG 캔버스 안에서 실제 오브젝트가 차지하는 면적과 가늘고 긴 실루엣의 체감 크기를 함께 비교해 정했다. PNG 자체는 수정하지 않았다.

## Food / Rest 준비

동일한 카탈로그 배열 구조를 적용했다. 현재 리소스의 투명 여백을 기준으로 Food는 `1.35, 0.97, 0.97, 0.97, 0.97, 0.97, 0.97, 0.97`, Rest는 `0.95, 0.97, 0.97, 0.98, 0.95, 0.97, 0.95, 0.97`을 사용한다. 향후 최종 아트 교체 시 카탈로그 값만 조정하면 된다.

## 애니메이션 결합

최종 UI Scale은 `BaseVisualScale * AnimationScale`로 계산한다. Merge/Generator Pop은 각각의 아이템 보정값을 유지한 채 `0.82 -> 1.13(Generator는 1.09) -> 1.0` 배율로 재생된다. 종료 시 최종값을 절대 `1.0`으로 덮어쓰지 않고 AnimationScale만 `1.0`으로 복원하므로 BaseVisualScale이 유지된다.

## 검증

- 원본 Unity Editor 자동 컴파일: 성공, 신규 컴파일 오류 없음
- 기존 경고: `WorkshopArtImporter.cs`의 obsolete `TextureImporter.spritesheet` 경고만 존재
- EditMode에 Toy/Food/Rest 배열, fallback 및 Scale 곱셈/복원 테스트 추가
- 별도 프로젝트 복제본의 BatchMode 테스트: Unity Licensing Client 재연결이 완료되지 않아 테스트 실행 전 중단
- 정적 검사: 이전 `FindVisualSizePercent` 참조 없음
- HUD, Quest, Generator, Board, Cell 레이아웃 파일은 변경하지 않음

## 수동 확인

Unity Game View에서 Toy Lv01~08을 같은 셀 크기로 순차 표시하여 체감 크기를 최종 확인한다. 특히 가늘고 긴 Lv04와 면적이 큰 Lv06/Lv08을 확인하고, 필요하면 카탈로그의 Presentation 값만 미세 조정한다. Drag 및 Merge/Generator Pop 종료 뒤에도 각 아이템의 기본 보정 크기가 유지되는지 확인한다.

서버 저장소는 수정하지 않았으며 Git 커밋과 푸시도 수행하지 않았다.
