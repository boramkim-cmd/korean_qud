# 🚀 프로젝트 빠른 참조 (자동 생성)

**생성**: 2026-01-16 23:54:57

## ⭐ 핵심 경로
```
Scripts/00_Core/00_00_01_TranslationEngine.cs  → 핵심 엔진
Scripts/00_Core/00_00_03_LocalizationManager.cs → 데이터 관리
LOCALIZATION/glossary_*.json              → 용어집 데이터
```

## 📚 용어집 현황
- `glossary_chargen.json`: 191개 항목
- `glossary_cybernetics.json`: 63개 항목
- `glossary_location.json`: 23개 항목
- `glossary_mutations.json`: 139개 항목
- `glossary_options.json`: 403개 항목
- `glossary_pregen.json`: 34개 항목
- `glossary_proto.json`: 40개 항목
- `glossary_skills.json`: 240개 항목
- `glossary_terms.json`: 39개 항목
- `glossary_ui.json`: 159개 항목

## ⛔ 절대 금지 (DO NOT)
```
❌ _Legacy/ 폴더의 코드 사용
❌ TranslationEngine 로직 중복 구현
❌ 색상 태그/프리픽스 수동 처리
❌ project_tool.py 검증 없이 배포
```

## ✅ 작업 체크리스트
```
1. 01_CORE_PROJECT_INDEX.md에서 기존 함수 확인
2. Scripts/ 내부 로직 수정
3. python3 tools/project_tool.py 로 검증
4. ./tools/deploy-mods.sh 로 게임 적용
```