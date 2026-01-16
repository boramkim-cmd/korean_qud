# 코드 작성 워크플로우

## 🎯 목적
중복 코드 방지 및 코드 품질 보장

## 📋 새 기능 추가 시 필수 절차

### 1단계: 기존 코드 확인 (필수!)
```bash
# 비슷한 함수가 있는지 검색
grep -r "함수명\|기능설명" Scripts/ --include="*.cs"

# 핵심 함수 위치 확인
python3 verify_code.py
```

### 2단계: CODEBASE_MAP.md 확인
- `01_TranslationEngine.cs`: 번역 관련 기능
- `LocalizationManager.cs`: 번역 데이터 접근
- `99_Utils/`: 유틸리티 함수

### 3단계: 코드 작성
- 기존 함수 **재사용** 우선
- 새 함수 필요 시 적절한 위치에 추가
  - 번역 엔진 로직 → `01_TranslationEngine.cs`
  - 범용 유틸리티 → `99_Utils/`
  - UI 패치 → `02_Patches/UI/`

### 4단계: 검증 (배포 전 필수!)
```bash
# 자동 검증 실행
python3 verify_code.py

# 중복 함수, 구문 오류 확인
# 오류 있으면 수정 후 재검증
```

### 5단계: 문서 업데이트
- `CODEBASE_MAP.md`에 새 함수 추가
- 주석으로 기능 설명

### 6단계: 배포
```bash
# 검증 통과 후에만 배포
cp -r Scripts/* "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization/Scripts/"
```

## ⚠️ 금지 사항

❌ **검증 없이 배포**
❌ **기존 코드 검색 없이 새 함수 작성**
❌ **중복 로직 작성**
❌ **문서화 없이 코드 추가**

## ✅ 권장 사항

✅ **TranslationEngine 우선 사용**
✅ **LocalizationManager 메서드 활용**
✅ **기존 Utils 재사용**
✅ **검증 스크립트 정기 실행**

## 🔧 자동화 도구

### verify_code.py
- 중복 클래스/함수 탐지
- 구문 오류 확인
- 핵심 함수 위치 표시

### 사용법
```bash
python3 verify_code.py
```

## 📝 체크리스트

코드 작성 전:
- [ ] CODEBASE_MAP.md 확인
- [ ] 기존 코드 검색
- [ ] 재사용 가능한 함수 확인

코드 작성 후:
- [ ] verify_code.py 실행
- [ ] 중복/오류 없음 확인
- [ ] CODEBASE_MAP.md 업데이트
- [ ] 배포

## 🎓 학습 자료

- `CODEBASE_MAP.md`: 프로젝트 구조
- `01_TranslationEngine.cs`: 번역 엔진 구현
- `LocalizationManager.cs`: 데이터 관리
