# Legacy Scripts Archive (2026-01-18)

이 디렉토리에는 Localization 구조 개편 과정에서 사용된 일회성 스크립트들이 보관되어 있습니다.

## 보관 이유
- 프로젝트 루트 디렉토리 정리 (DEVELOPMENT_GUIDE Part E.2 준수)
- 향후 참조 가능성을 위해 삭제 대신 보관
- Git 히스토리 추적 용이성

## 스크립트 목록 및 목적

### Mutation 관련
- `add_ko_fields.py` - JSON에 한글 필드 추가
- `add_korean_translations.py` - 한글 번역 추가
- `bulk_update_mutations.py` - 변이 대량 업데이트
- `cleanup_mutation_code.py` - 변이 코드 정리
- `convert_mutation_json.py` - JSON 구조 변환
- `extract_extra_mutation_texts.py` - 추가 텍스트 추출
- `final_mutation_cleanup.py` - 최종 정리
- `rescue_mutations.py` - 변이 데이터 복구
- `unify_mutation_strings.py` - 문자열 통합
- `validate_mutations.py` - 변이 검증
- `reorganize_mutations_by_category.py` - 카테고리별 재구성

### Glossary 관련
- `refactor_glossary.py` - 용어집 리팩토링
- `update_glossary_pass3.py` - 용어집 업데이트 (Pass 3)
- `distribute_pass3.py` - 배포 (Pass 3)

### 리팩토링 도구
- `grand_refactor.py` - 대규모 리팩토링
- `perfect_refactor.py` - 완벽 리팩토링
- `ultra_robust_refactor.py` - 강건한 리팩토링

### 유틸리티
- `debug_mapping.py` - 디버그 매핑
- `manual_translation_template.py` - 수동 번역 템플릿
- `populate_korean_translations.py` - 한글 번역 채우기

## 활용 방법
필요 시 참조하거나 코드 재사용 가능하지만, **새로운 스크립트는 반드시 `tools/` 디렉토리에 작성**하세요.

## 관련 문서
- [DEVELOPMENT_GUIDE.md Part E.2](../Docs/01_DEVELOPMENT_GUIDE.md) - Python 스크립트 작성 규칙
- [03_CHANGELOG.md](../Docs/03_CHANGELOG.md) - 변경 이력

---
**보관일**: 2026-01-18  
**작업자**: AI Agent (Antigravity)
