# 11단계 — 모바일 실제 빌드 기반

`ClientBuild`는 Build Settings의 Scene으로 Windows 개발 빌드와 Android APK를 생성합니다.
Android application ID는 `com.happynewpuzzle.projectmerge`, 최소 SDK는 API 25입니다.
개발 빌드는 서명된 스토어 산출물이 아니며 실제 릴리스 키는 저장소에 포함하지 않습니다.

현재 PC에는 Android와 Windows Unity 모듈이 설치되어 있으나 iOS 모듈과 Xcode/macOS가
없습니다. 따라서 iOS 네이티브 Keychain 코드는 컴파일 조건으로 격리했으며 실제 Xcode
빌드·기기 검증은 macOS runner가 필요합니다.

2026-08-10 Windows x64 개발 플레이어와 Android APK를 실제 생성했습니다. Windows 실행
파일은 약 652KB이며 데이터 폴더와 함께 생성됐고, Android는 설치된 SDK/NDK/JDK와
Unity Android 모듈로 APK 빌드에 성공했습니다. 산출물은 `Builds/`로 Git에서 제외됩니다.
iOS는 이 PC 환경 제약으로 실행하지 못했습니다.
