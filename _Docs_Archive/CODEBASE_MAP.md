# 프로젝트 구조 및 코드베이스 맵

## 📁 Scripts 폴더 구조

### 00_Core/ - 핵심 시스템
- `00_03_LocalizationManager.cs`: 번역 데이터 로드 및 관리
  - `GetCategory(string)`: 카테고리별 번역 딕셔너리 반환
  - `TryGetAnyTerm(string, out string, params string[])`: 여러 카테고리에서 번역 검색
  
- `01_TranslationEngine.cs`: **핵심 번역 엔진** ⭐
  - `TryTranslate(string, out string, Dictionary<string,string>[])`: 메인 번역 메서드
  - `ExtractPrefix(ref string)`: 체크박스/접두사 자동 추출
  - `StripColorTags(string)`: Qud 색상 태그 제거 ({{w|text}})
  - `RestoreColorTags(...)`: 번역 후 태그 복원
  - **모든 프리픽스와 색상 태그를 자동 처리!**
  
- `02_ScopeManager.cs`: 번역 스코프 관리
- `00_ModEntry.cs`: 모드 진입점 및 Harmony 패치 적용

### 99_Utils/ - 유틸리티 함수
- `TranslationUtils.cs`: 태그 보존 번역
  - `TryTranslatePreservingTags()`: HTML/게임 태그 보존
  - `IsControlValue()`: 체크박스/숫자 등 제어값 판단
  
- `ChargenTranslationUtils.cs`: 캐릭터 생성 전용
  - `TranslateLongDescription()`: 다중 라인 번역 (TranslationEngine 사용)
  - `TranslateMenuOptions()`: MenuOption 번역
  - `TranslateBreadcrumb()`: Breadcrumb 번역

### 02_Patches/ - Harmony 패치
- `Core/`: 핵심 시스템 패치
- `UI/`: UI 관련 패치
  - `10_10_P_CharacterCreation.cs`: 캐릭터 생성 화면 통합 패치

## 🚨 **새 코드 작성 전 체크리스트**

### 1. 기존 유틸리티 확인
```bash
# 비슷한 기능이 있는지 검색
grep -r "함수명\|기능설명" Scripts/ --include="*.cs"
```

### 2. 필수 확인 사항
- [ ] `01_TranslationEngine.cs`에 이미 있는 기능인가?
- [ ] `LocalizationManager`에 필요한 메서드가 있는가?
- [ ] 다른 Utils 파일에 비슷한 기능이 있는가?

### 3. 새 유틸리티 추가 시
- [ ] 기존 엔진/유틸리티를 **재사용**할 수 있는가?
- [ ] 정말 새로운 기능이 필요한가?
- [ ] 문서화 (이 파일에 추가)

## 📝 **코딩 규칙**

### 번역 관련 코드 작성 시
1. **항상 `TranslationEngine.TryTranslate()` 우선 사용**
   - 색상 태그, 프리픽스 자동 처리
   - 대소문자 변형 자동 시도
   
2. **LocalizationManager 메서드 확인**
   - `GetCategory()`: null 반환 가능
   - `TryGetAnyTerm()`: 여러 카테고리 검색
   
3. **중복 방지**
   - 프리픽스 추출: TranslationEngine 사용
   - 색상 태그 처리: TranslationEngine 사용
   - 새 로직 추가 전 기존 코드 검색

## 🔧 **자주 사용하는 패턴**

### 패턴 1: 단일 텍스트 번역
```csharp
if (LocalizationManager.TryGetAnyTerm(text.ToLowerInvariant(), out string translated, "category1", "category2"))
{
    // 번역 성공
}
```

### 패턴 2: 다중 라인 번역 (색상 태그 포함)
```csharp
var scopes = categories.Select(cat => LocalizationManager.GetCategory(cat)).Where(d => d != null).ToArray();
if (TranslationEngine.TryTranslate(line, out string translated, scopes))
{
    // 자동으로 색상 태그, 프리픽스 처리됨
}
```

### 패턴 3: Harmony 패치에서 필드 접근
```csharp
var tr = Traverse.Create(obj);
string value = tr.Field<string>("FieldName").Value;
tr.Field<string>("FieldName").Value = newValue;
```

## 🎯 **핵심 원칙**

1. **DRY (Don't Repeat Yourself)**: 기존 코드 재사용
2. **검색 우선**: 새 코드 작성 전 기존 코드 검색
3. **문서화**: 새 기능 추가 시 이 파일 업데이트
4. **검증**: 컴파일 확인 후 배포

## 📚 **참고 자료**

- TranslationEngine: 모든 번역의 핵심
- LocalizationManager: 번역 데이터 접근
- Harmony 문서: https://harmony.pardeike.net/
