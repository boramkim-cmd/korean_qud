# ⚠️ AI 에이전트 필독 사항

## 🎯 작업 시작 전 필수 실행

```bash
# 1. 프로젝트 상태 확인 (가장 중요!)
cat QUICK_REFERENCE.md

# 2. 코드 검증
python3 verify_code.py
```

## 📋 핵심 규칙

### ✅ 해야 할 것
1. **QUICK_REFERENCE.md 먼저 읽기** - 모든 핵심 정보가 여기 있음
2. **기존 함수 재사용** - TranslationEngine, LocalizationManager 활용
3. **검증 후 배포** - verify_code.py 실행 필수

### ❌ 하지 말아야 할 것
1. **_Legacy/ 폴더 코드 사용 금지**
2. **중복 로직 구현 금지** - 특히 TranslationEngine 기능
3. **검증 없이 배포 금지**

## 🔄 자동화 시스템

### 프로젝트 상태 업데이트
```bash
python3 generate_quick_reference.py
```
→ QUICK_REFERENCE.md 자동 생성 (모든 핵심 정보 통합)

### 코드 검증
```bash
python3 verify_code.py
```
→ 중복 함수, 구문 오류 자동 탐지

## 📁 핵심 파일 위치

- **QUICK_REFERENCE.md**: 👈 **가장 중요! 항상 먼저 읽기**
- **verify_code.py**: 코드 검증
- **generate_quick_reference.py**: 참조 가이드 생성

## 🚀 워크플로우

```
1. cat QUICK_REFERENCE.md
   ↓
2. 기존 함수 확인
   ↓
3. 코드 작성
   ↓
4. python3 verify_code.py
   ↓
5. 오류 수정
   ↓
6. 배포
```

## 💡 자주 사용하는 명령

```bash
# 함수 검색
grep -r "함수명" Scripts/ --include="*.cs"

# 프로젝트 상태 갱신
python3 generate_quick_reference.py

# 검증
python3 verify_code.py

# 빠른 참조
cat QUICK_REFERENCE.md
```

---

**이 파일을 매 작업 시작 시 확인하세요!**
