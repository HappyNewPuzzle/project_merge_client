# ART_GUIDE.md

# Cat Merge — Art Direction & Asset Guide

이 문서는 고양이 머지게임의 아트 스타일과 Unity 리소스 관리 기준입니다.
모든 신규 아트는 **하나의 동일한 상용 모바일 게임에서 제작된 리소스 세트처럼** 보여야 합니다.

## 1. 전체 게임 아트 방향

- 장르: 모바일 캐주얼 머지게임
- 메인 테마: 귀엽고 따뜻한 고양이 세계관
- 분위기: 포근함, 사랑스러움, 밝음, 친근함
- 대상: 작은 모바일 화면
- 목표: 실제 게임에서 바로 사용할 수 있는 읽기 쉬운 상용 리소스

## 2. 기본 아트 스타일

- 귀엽고 세련된 모바일 캐주얼 게임 스타일
- 둥글고 부드러운 형태
- 밝고 따뜻한 파스텔 계열
- 약한 3/4 시점
- 작은 화면에서도 잘 읽히는 명확한 실루엣
- 과도한 실사 금지
- 약간의 볼륨감이 있는 2D 일러스트
- 복잡한 재질보다 형태와 색상으로 구분
- 부드러운 하이라이트
- 부드러운 그림자
- 높은 가독성
- 과한 세부 묘사 금지

이미지별로 가능한 한 다음을 통일합니다.

- 카메라 각도
- 광원 방향
- 채도
- 명암
- 외곽선/윤곽 느낌
- 재질 표현
- 귀여움의 정도
- 아이템 중심 배치

## 3. 고양이 캐릭터 기준

메인 마스코트는 크림색 + 연한 주황색 계열의 친근한 고양이입니다.

주요 특징:

- 둥근 얼굴
- 큰 갈색 눈
- 작은 분홍색 코
- 부드러운 삼각형 귀
- 통통한 꼬리
- 밝고 친근한 표정
- 귀여운 머리/몸 비율
- 모바일 UI에서 얼굴이 잘 읽혀야 함

현재 기본 Sprite:

```text
Assets/Art/CatMerge/Characters/Mascot/Cat_Mascot_Default.png
```

이후 고양이 표정/포즈를 만들 때 같은 캐릭터 계열처럼 보여야 합니다.

## 4. 머지 아이템 디자인 원칙

머지 아이템은 같은 계열이 단계적으로 성장합니다.

단계 상승 시 증가해야 할 요소:

- 크기
- 품질
- 디테일
- 재질
- 장식
- 희귀도
- 보상감

단순히 크기만 키우지 않습니다.
같은 체인이라는 점은 유지하면서 각 단계가 작은 모바일 화면에서도 분명히 구분되어야 합니다.

## 5. 현재 머지 체인

### Toy

경로:

```text
Assets/Art/CatMerge/Items/Toy/
```

파일:

```text
Toy_Lv01.png
Toy_Lv02.png
Toy_Lv03.png
Toy_Lv04.png
Toy_Lv05.png
Toy_Lv06.png
Toy_Lv07.png
Toy_Lv08.png
```

시각 방향:

- 핑크 털실
- 리본/방울
- 깃털
- 생선 장난감
- 캣닢/놀이 세트
- 고급 장난감 바구니/상자

Toy 계열은 밝고 장난스러운 색감이 핵심입니다.

### Food

경로:

```text
Assets/Art/CatMerge/Items/Food/
```

파일:

```text
Food_Lv01.png
Food_Lv02.png
Food_Lv03.png
Food_Lv04.png
Food_Lv05.png
Food_Lv06.png
Food_Lv07.png
Food_Lv08.png
```

성장 방향:

- 작은 사료
- 사료 봉지
- 밥그릇
- 캔
- 생선 요리
- 고급 식사
- 프리미엄 식사
- 왕실급 만찬

Food는 사진 같은 실사 음식이 아니라 캐주얼 게임 아이콘으로 보이게 유지합니다.

현재 Presentation 참고:

```text
Food Lv01 VisualScale = 1.35
Food Lv02~08 ≈ 0.97
```

실제 값은 Catalog를 진실 원천으로 사용합니다.

### Rest

경로:

```text
Assets/Art/CatMerge/Items/Rest/
```

파일:

```text
Rest_Lv01.png
Rest_Lv02.png
Rest_Lv03.png
Rest_Lv04.png
Rest_Lv05.png
Rest_Lv06.png
Rest_Lv07.png
Rest_Lv08.png
```

성장 방향:

- 쿠션
- 바구니/침대
- 고양이 동굴
- 고양이 소파
- 캣타워
- 고급 캣타워
- 대형 휴식 공간
- 고양이 캐슬

Rest 계열은 "푹신함 + 주거/휴식"이 한눈에 느껴져야 합니다.

현재 주의사항:

Rest Lv01은 작은 화면에서 간식/쿠키로 오인될 가능성이 있으므로 향후 사용자 테스트에서 가독성을 관찰합니다.

## 6. Generator

경로:

```text
Assets/Art/CatMerge/Generators/
```

파일:

```text
Generator_Toy.png
Generator_Food.png
Generator_Rest.png
```

시각 원칙:

- 일반 머지 아이템보다 특별하게 보여야 함
- 클릭/사용 가능한 오브젝트라는 인상
- 같은 카테고리의 재료가 내부에서 살짝 보임
- 고양이 발바닥 모티프 활용
- 보드 아이템보다 약간 더 복잡해도 됨

현재 Production 기능:

- Generator_Toy 사용 중
- Generator_Food Art only
- Generator_Rest Art only

## 7. Currency

경로:

```text
Assets/Art/CatMerge/Currency/
```

파일:

```text
Currency_Coin.png
Currency_Energy.png
Currency_Gem.png
```

디자인:

### Coin
- 금색
- 고양이 발바닥 심볼
- 작은 HUD에서도 즉시 인식

### Energy
- 발바닥 + 번개
- 활기찬 핑크/노랑
- Energy라는 의미가 즉시 전달

### Gem
- 발바닥/보석 조합
- 파랑/보라/핑크 계열
- Premium 느낌

현재 Gem은 Art only입니다.

## 8. Navigation

경로:

```text
Assets/Art/CatMerge/UI/Navigation/
```

파일:

```text
UI_Nav_Home.png
UI_Nav_Collection.png
UI_Nav_Shop.png
UI_Nav_Quest.png
```

의미:

- Home: 고양이 집
- Collection: 도감/책
- Shop: 상점/바구니
- Quest: 클립보드/퀘스트

Navigation Icon은 작은 크기에서도 실루엣이 읽혀야 합니다.

현재 표시 상태:

- Home selected
- Collection disabled
- Shop disabled
- Quest disabled

## 9. Background

현재 메인 플레이 Background는 포근한 고양이 방입니다.

배경 원칙:

- 중앙 Gameplay 영역은 비교적 단순
- 장식은 가장자리 중심
- 창문, 식물, 캣타워, 쿠션, 러그 등 활용
- Board보다 눈에 띄면 안 됨
- 따뜻하고 밝은 크림/오렌지 계열
- 공포/어두운 분위기 금지

Background는 UI와 경쟁하는 콘텐츠가 아니라 세계관을 만들어주는 후경입니다.

## 10. 출력 규칙

새 게임 리소스는 특별한 요청이 없으면:

- 투명 배경
- 오브젝트 중앙 배치
- 텍스트 없음
- 숫자 없음
- 워터마크 없음
- 불필요한 배경 장식 없음
- 잘리지 않도록 여백 확보
- Unity에서 개별 Sprite로 사용하기 쉬운 형태

## 11. Unity 파일 관리 원칙

실사용 리소스 예:

```text
Assets/Art/CatMerge/
├─ Characters/
│  └─ Mascot/
├─ Items/
│  ├─ Toy/
│  ├─ Food/
│  └─ Rest/
├─ Generators/
├─ Currency/
├─ UI/
│  └─ Navigation/
└─ Backgrounds/
```

Reference/원본 작업물은 가능하면 Unity Assets 외부 ArtSource에 둡니다.

```text
ArtSource/CatMerge/
├─ References/
└─ OriginalSheets/
```

Unity에서 직접 사용하지 않는 통합 생성 시트는 ArtSource에 보관하는 것을 권장합니다.

## 12. Sprite Import 기본 기준

개별 PNG:

```text
Texture Type = Sprite (2D and UI)
Sprite Mode = Single
Alpha Is Transparency = On
Mip Maps = Off
Filter Mode = Bilinear
Wrap Mode = Clamp
```

PPU / Compression / Max Size는 기존 프로젝트의 현재 CatMerge Import 설정을 우선합니다.
현재 여러 UI/Art에서 PPU 100이 사용되고 있습니다.

## 13. Sprite Sheet 사용 원칙

머지 아이템은 개별 PNG를 우선합니다.

Sprite Sheet를 고려할 수 있는 경우:

- Walk animation
- Run animation
- 반복 프레임
- 규칙적인 동일 크기 animation cells

AI 생성 통합 시트를 자동 Slice하여 상용 리소스로 쓰는 것은 기본적으로 권장하지 않습니다.

최종 최적화는 Unity Sprite Atlas를 별도로 사용할 수 있습니다.

```text
작업/관리 = 개별 PNG
런타임 최적화 = Unity Sprite Atlas
```

## 14. VisualScale 원칙

PNG의 실제 오브젝트 점유 면적이 서로 다를 수 있습니다.

이를 해결하기 위해:

```text
BaseVisualScale
×
AnimationScale
```

개념으로 Presentation 크기를 보정합니다.

원칙:

- PNG 자체 resize 금지
- Board Cell 크기 체인별 변경 금지
- Item별 Prefab 생성 금지
- 데이터/게임 로직에 Scale 의미를 섞지 않음
- Animation 종료 시 BaseVisualScale 유지

## 15. 피해야 할 스타일

- 실사
- 사진
- 과한 3D 렌더
- 지나치게 사실적인 털
- 지나치게 복잡한 Texture
- 어두운 호러 분위기
- 다른 게임처럼 보이는 혼합 스타일
- 작은 화면에서 구분 어려운 복잡한 실루엣
- 과한 반사/Glow
- 배경이 Gameplay보다 눈에 띄는 구성

## 16. 신규 Art 제작 체크리스트

1. 기존 CatMerge 스타일과 동일한가?
2. 작은 모바일 화면에서 실루엣이 읽히는가?
3. 해당 Category를 즉시 알 수 있는가?
4. 같은 Merge Chain에서 성장 관계가 보이는가?
5. 다른 Chain과 혼동되지 않는가?
6. 투명 배경인가?
7. 텍스트/숫자가 없는가?
8. Unity Single Sprite로 관리하기 쉬운가?
9. 기존 광원/채도/명암과 조화되는가?
10. 실제 보드에서 너무 작거나 커 보이지 않는가?

## 17. 최우선 아트 원칙

모든 리소스는 각각 따로 생성한 이미지처럼 보이면 안 됩니다.

최종 목표:

**캐릭터, 머지 아이템, Generator, Currency, UI, Background가 모두 하나의 고양이 머지게임 아트 팀이 만든 것처럼 통일되어 보이는 것.**
