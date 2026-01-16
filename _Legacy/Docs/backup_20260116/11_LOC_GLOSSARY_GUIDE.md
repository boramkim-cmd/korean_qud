# 용어집 완전 가이드

Caves of Qud 한글화 프로젝트의 용어 관리 및 사용 방법

---

## 📋 목차

1. [용어집 개요](#용어집-개요)
2. [JSON 용어집 시스템](#json-용어집-시스템)
3. [플레이스홀더 사용법](#플레이스홀더-사용법)
4. [용어 변경 방법](#용어-변경-방법)
5. [주요 용어 목록](#주요-용어-목록)

---

## 용어집 개요

### 역할
- **참고 문서**: 번역 일관성 유지
- **중앙 관리**: JSON 파일로 용어 통합 관리
- **자동 적용**: 플레이스홀더로 코드/XML에서 사용

### 파일 구조
```
Docs/
├── 10_LOC_WORKFLOW.md          # 번역 작업 프로세스
├── 11_LOC_GLOSSARY_GUIDE.md    # 이 문서
├── 13_LOC_STYLE_GUIDE.md       # 한글화 스타일 가이드
└── 14_LOC_QA_CHECKLIST.md      # 품질 검증 체크리스트
```

---

## JSON 용어집 시스템

### glossary_*.json 구조 (예시: glossary_ui.json)

```json
{
  "ui": {
    "newGame": "새 게임",
    "continue": "계속하기",
    "options": "옵션"
  }
}
```

여러 전용 파일로 나뉘어 관리됩니다:
- `glossary_ui.json`: UI 텍스트
- `glossary_skills.json`: 스킬 이름 및 설명
- `glossary_mutations.json`: 돌연변이
- `glossary_options.json`: 게임 옵션
- `glossary_location.json`: 장소명
- ... (기타 카테고리별 파일)

### 카테고리 설명
- `phrase` - 자주 쓰는 문장/구문
- `faction` - 세력 이름
- `item` - 아이템 이름
- `weapon` - 무기 이름
- `attribute` - 능력치 이름
- `ui` - UI 텍스트
- `common` - 공통 용어

---

## 플레이스홀더 사용법

### 형식
```
[[category.key]]
```

### XML에서 사용

```xml
<!-- 단순 사용 -->
<text>[[phrase.greeting]]</text>

<!-- 조사와 함께 -->
<text>[[item.water]]{을/를} 마셨습니다</text>

<!-- 게임 명령어와 혼합 -->
<text>{{color|cyan|[[faction.crystalism]]}}</text>

<!-- 게임 변수와 혼합 -->
<text>[[phrase.greeting]], =player.name=</text>
```

### C# 코드에서 사용

```csharp
using QudKRTranslation.Core;

// 1. G 클래스 사용 (가장 간편)
string text = G._("ui.newGame");

// 2. LocalizationManager 직접 사용
string term = LocalizationManager.GetTerm("ui", "newGame", "새 게임");
```

---

## 용어 변경 방법

### 1. JSON 파일 수정 (권장 ⭐)

**변경 전:**
```json
// glossary_item.json
{"item": {"milk": "우유"}}
```

**변경 후:**
```json
// glossary_item.json
{"item": {"milk": "생수"}}
```

→ 게임 재시작 → 모든 곳에 자동 적용!

### 2. 일괄 변경 시 주의사항
용어 일괄 변경이 필요한 경우, 텍스트 에디터(VS Code 등)의 '전체 바꾸기(Ctrl+Shift+F)' 기능을 사용하거나, AI 에이전트에게 요청하십시오. JSON 파일 수정 후 반드시 `python3 tools/project_tool.py`를 실행하여 무결성을 확인하십시오.

---

## 주요 용어 확인

모든 최신 용어 데이터는 `LOCALIZATION/glossary_*.json` 파일들에 정의되어 있습니다. 

일관된 번역을 위해 작업 전 해당 카테고리의 JSON 파일을 열어 기존 용어를 확인하십시오.

- **세계관/세력/아이템**: `glossary_terms.json`
- **스킬**: `glossary_skills.json`
- **돌연변이**: `glossary_mutations.json`
- **UI/시스템**: `glossary_ui.json`


---

## 빠른 참조

### 용어 추가
```json
// glossary_item.json
{
  "item": {
    "newItem": "새 아이템"  // ← 추가
  }
}
```

### XML에서 사용
```xml
<text>[[item.newItem]]{을/를} 발견했습니다</text>
```

### 결과
→ "새 아이템을 발견했습니다"

---

## 주의사항

1. **JSON 구문**: 큰따옴표(`"`) 사용, 마지막 항목 뒤 쉼표 제거
2. **키명**: 영문 소문자, camelCase 사용
3. **플레이스홀더**: `[[category.key]]` 형식 (점 구분자)
4. **게임 명령어**: `{{}}`, `=...=` 는 건드리지 마세요

---

## 문제 해결

### 용어가 적용 안 됨
1. glossary_*.json 구문 오류 확인
2. 플레이스홀더 형식 확인: `[[category.key]]`
3. 게임 재시작

### 플레이스홀더가 그대로 표시됨
1. XMLGlossaryProcessor.cs 패치 확인
2. 카테고리/키명 일치 확인
3. 로그 확인

---

**요약:**
- ✅ `glossary_*.json`에 용어 정의
- ✅ `[[category.key]]` 형식으로 사용
- ✅ JSON만 수정하면 모든 곳에 적용
- ✅ 게임 재시작으로 반영

🎉 완료!
