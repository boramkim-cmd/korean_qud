# 용어집 편집 가이드 (Glossary Editing)

> 이 문서는 Layer 1 용어집(glossary_*.json) 편집 시 참조합니다.

## 파일 위치

```
LOCALIZATION/
├── glossary_ui.json           ← UI 공통
├── glossary_skills.json       ← 스킬
├── glossary_options.json      ← 설정
├── glossary_chargen_*.json    ← 캐릭터 생성
└── ...
```

## JSON 구조

```json
{
  "category_name": {
    "English Key": "한글 값",
    "Another Key": "또 다른 값"
  }
}
```

## 편집 절차

### 1. 기존 항목 확인
```bash
grep -r "검색어" LOCALIZATION/*.json
```

### 2. 수정/추가
```json
{
  "ui": {
    "New Game": "새 게임",
    "Continue": "계속하기"
  }
}
```

### 3. 검증
```bash
python3 tools/project_tool.py
```

## ⚠️ 주의사항

### 태그 제외

```json
// ❌ 잘못됨
{"{{c|ù}} text": "{{c|ù}} 번역문"}

// ✅ 올바름
{"{{c|ù}} text": "번역문"}
```

엔진이 자동으로 원본 태그를 복원합니다.

### 키 정규화

- 검색 시 자동으로 소문자 변환
- 공백 유지
- 특수문자 허용

### 충돌 주의

`integrity_report.md`에 102건의 용어집 간 충돌이 있습니다.
수정 시 다른 glossary와 중복되지 않는지 확인하세요.

## 카테고리별 용도

| 파일 | 용도 | 비고 |
|------|------|------|
| `glossary_ui.json` | UI 공통 텍스트 | 버튼, 메뉴 |
| `glossary_skills.json` | 스킬 이름/설명 | |
| `glossary_options.json` | 설정 화면 | 가장 많은 항목 |
| `glossary_chargen_*.json` | 캐릭터 생성 | 3개로 분리 |
| `glossary_terms.json` | 일반 용어 | |

## 담당 컴포넌트

- **LocalizationManager**: JSON 로드, 카테고리별 검색
- **TranslationEngine**: 태그 보존/복원, 번역 수행
