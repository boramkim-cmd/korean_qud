# Reference Files

이 폴더는 번역 작업 시 참고할 원본 게임 데이터 파일을 보관합니다.

## 파일 목록

### Options.xml
- **원본 경로**: `/Users/ben/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/StreamingAssets/Base/Options.xml`
- **용도**: 설정 화면의 모든 옵션 정의
- **번역 파일**: 
  - 00_02_TranslationDB_Sound.cs
  - 00_03_TranslationDB_Display.cs
  - 00_04_TranslationDB_Controls.cs
  - 00_05_TranslationDB_Accessibility.cs
  - 00_06_TranslationDB_UI.cs
  - 00_07_TranslationDB_Automation.cs
  - 00_08_TranslationDB_Mods.cs
  - 00_09_TranslationDB_AppSettings.cs
  - 00_10_TranslationDB_Performance.cs

## 사용 방법

1. 이 폴더의 XML 파일에서 번역할 텍스트 확인
2. 해당 카테고리의 번역 파일 수정
3. 게임 재시작하여 확인

## 주의사항

⚠️ **이 폴더의 파일은 참고용입니다!**
- 게임에서 직접 사용되지 않음
- 번역 작업 시 원본 확인용
- 수정해도 게임에 영향 없음

✅ **실제 번역은 .cs 파일에서 수행**
- Translation/Options/*.cs 파일을 수정하세요
- 이 폴더의 XML은 읽기 전용으로 사용
