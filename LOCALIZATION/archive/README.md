# GLOSSARY_Korean.md 사용 가이드

## ⚠️ 중요: 이 파일의 역할

**이 파일은 참고 문서입니다.** 여기서 용어를 수정해도 게임 내 번역이 자동으로 변경되지 않습니다.

## 🎯 권장 방법: 수동 변경 (안전 ⭐)

용어를 변경할 때는 **직접 코드를 수정**하는 것이 가장 안전합니다.

## 📍 실제 번역이 적용되는 위치

### 1. UI 번역 데이터
**파일**: `Data_QudKRContent/Scripts/01_Data/TranslationData.cs`

```csharp
// 예시
AddTranslation("MainMenu_NewGame", "새 게임");
AddTranslation("Attribute_Strength", "힘");
AddTranslation("Weapon_ShortBow", "숏보우");  // ← 이걸 "짧은 활"로 바꾸고 싶다면?
```

## 🔄 용어 변경 방법

### 방법 1: 자동화 스크립트 (권장 ⭐)

```bash
# 1. 미리보기 (실제 변경 안 함)
python tools/sync_glossary.py --old "숏보우" --new "짧은 활" --dry-run

# 2. 실제 적용
python tools/sync_glossary.py --old "숏보우" --new "짧은 활"
```

**장점:**
- ✅ 모든 `.cs` 파일에서 자동 치환
- ✅ `AddTranslation()` 내부만 안전하게 변경
- ✅ 변경 내역 미리보기 가능

**예시 출력:**
```
🔍 용어 치환: '숏보우' → '짧은 활'
📄 검색 대상 파일: 25개

✅ Data_QudKRContent/Scripts/01_Data/TranslationData.cs (3개 변경)
  숏보우를 획득했습니다 → 짧은 활을 획득했습니다
  숏보우 장착 → 짧은 활 장착
  숏보우 설명 → 짧은 활 설명

📊 요약:
  - 변경된 파일: 1개
  - 총 변경 횟수: 3개
```

### 방법 2: 수동 변경

```bash
# VS Code에서 전체 검색 및 치환
# 1. Cmd+Shift+F (전체 검색)
# 2. "숏보우" 검색
# 3. "짧은 활"로 치환
# 4. 파일별로 확인 후 변경
```

## � 권장 워크플로우

### 1단계: GLOSSARY 업데이트
```markdown
# GLOSSARY_Korean.md
| Short Bow | 숏보우 | 무기 |  ← 기존
| Short Bow | 짧은 활 | 무기 |  ← 변경
```

### 2단계: 자동 스크립트 실행
```bash
cd /Users/ben/Desktop/qud_korean
python tools/sync_glossary.py --old "숏보우" --new "짧은 활" --dry-run  # 미리보기
python tools/sync_glossary.py --old "숏보우" --new "짧은 활"           # 실제 적용
```

### 3단계: 테스트
```bash
# 모드 폴더에 복사
cp -r Core_QudKREngine "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/"
cp -r Data_QudKRContent "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/"

# 게임 재시작 후 확인
```

## 🛠️ 스크립트 옵션

```bash
python tools/sync_glossary.py --help

옵션:
  --old TEXT        기존 용어 (필수)
  --new TEXT        새 용어 (필수)
  --dry-run         미리보기만 (실제 변경 안 함)
  --base-path PATH  프로젝트 루트 경로 (기본: 현재 디렉토리)
```

## � 현재 상태

✅ **GLOSSARY_Korean.md**: 참고 문서 (수동 관리)  
✅ **TranslationData.cs**: 실제 번역 데이터 (하드코딩)  
✅ **sync_glossary.py**: 자동 동기화 스크립트  

## 🎯 사용 예시

### 예시 1: 단일 용어 변경
```bash
python tools/sync_glossary.py --old "힘" --new "근력" --dry-run
```

### 예시 2: 여러 용어 변경
```bash
# 스크립트를 여러 번 실행
python tools/sync_glossary.py --old "숏보우" --new "짧은 활"
python tools/sync_glossary.py --old "롱소드" --new "장검"
python tools/sync_glossary.py --old "숏소드" --new "단검"
```

### 예시 3: 조사 포함 변경
```bash
# "물{을/를}" → "생수{를}" 같은 변경도 가능
python tools/sync_glossary.py --old "물" --new "생수"
```

## ⚠️ 주의사항

1. **백업 필수**: 스크립트 실행 전 Git 커밋 또는 백업
2. **미리보기 확인**: 항상 `--dry-run`으로 먼저 확인
3. **부분 일치**: "물"을 변경하면 "물약", "물건"도 변경될 수 있음
4. **대소문자**: 한글은 대소문자 구분 없지만 정확히 일치해야 함

## 🔍 문제 해결

### 스크립트가 실행되지 않음
```bash
# Python 3 설치 확인
python3 --version

# 권한 부여
chmod +x tools/sync_glossary.py
```

### 변경되지 않는 파일이 있음
- Legacy 폴더는 자동으로 제외됨
- `.cs` 파일만 대상 (XML 파일은 제외)
- `AddTranslation()` 내부만 변경됨

---

**요약**: GLOSSARY 용어를 변경하려면 `sync_glossary.py` 스크립트를 사용하세요! 🚀
