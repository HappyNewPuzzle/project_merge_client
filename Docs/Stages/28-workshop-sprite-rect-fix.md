# 28단계 — Workshop Sprite 분할 좌표 수정

원본 atlas는 1254×1254이지만 최초 importer는 Unity가 NPOT 규칙으로 1024×1024로 축소한
`Texture2D.width`를 사용했습니다. 그 결과 각 셀이 올바른 418×418 대신 341×341로 기록되어
Sprite가 잘리거나 인접 아이템 일부를 표시했습니다.

Importer는 이제 `GetSourceTextureWidthAndHeight`로 원본 파일 크기를 읽고, 가로·세로가 3으로
정확히 나누어지는지 검사합니다. `TextureImporterNPOTScale.None`을 적용해 1254px 원본을
유지하고 9개 셀을 각각 418×418로 생성합니다. EditMode 테스트도 첫 Sprite의 rect 크기를
검증해 같은 오류의 재발을 차단합니다.

최종 검증은 Sprite 재임포트 성공, EditMode 21개 통과, PlayMode 14개 통과 및 실제 서버
통합 테스트 1개 조건부 건너뜀입니다.
