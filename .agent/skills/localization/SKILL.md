---
name: qud-korean-localization
description: |
  Caves of Qud 한글화 프로젝트 번역 작업을 위한 스킬.
  새 화면 번역, 용어집 추가/수정, 변이 데이터 편집, 버그 수정 등의 작업에 사용.
  프로젝트 진입 시 자동으로 활성화되며, CONTEXT.yaml로 현재 상태를 파악.
---

# Caves of Qud 한글화 프로젝트 스킬

이 스킬은 Caves of Qud 게임의 한국어 번역 모드 개발을 지원합니다.

## 빠른 시작

```bash
# 1. 현재 상태 파악 (최우선!)
cat CONTEXT.yaml

# 2. 필요시 상세 문서 참조
cat Docs/00_PRINCIPLES.md      # 대원칙
cat Docs/06_ARCHITECTURE.md    # 시스템 구조
cat Docs/07_WORKFLOW.md        # 작업 절차
cat Docs/08_STYLE_GUIDE.md     # 번역 스타일
```

## 핵심 규칙 (반드시 준수)

| 규칙 | 설명 |
|------|------|
| 🔴 **추측 금지** | `grep`으로 실제 코드 확인 후 작업 |
| 🔴 **검증 필수** | `python3 tools/project_tool.py` 실행 |
| 🔴 **태그 보존** | `{{tag}}`, `%var%` 번역 금지 |
| 🟢 **문서 우선** | 변경 시 관련 문서 업데이트 |

## 과거 교훈 (피해야 할 실수)

> **ERR-012**: ModelShark 툴팁 시스템을 분석하지 않고 일반적인 Unity 접근법 적용

| 실수 | 올바른 접근 |
|------|-------------|
| `TooltipTrigger`에서 직접 TMP 검색 | `tooltipTrigger.Tooltip.GameObject`에서 검색 |
| `as Component` 캐스팅 후 null 체크 생략 | 캐스팅 후 항상 디버그 로그 |
| 리플렉션으로 public 프로퍼티 접근 | 리플렉션 전 public API 확인 |

→ 상세: `ISSUES/ERR-012_Tooltip_Font_Display.md`

## 작업 유형별 가이드

### 1. 용어집 수정 (Layer 1)
```bash
# glossary_*.json 수정
# 태그 제외, 순수 한글 번역만
```
→ 상세: `references/glossary-editing.md`

### 2. 변이 번역 (Layer 2)
```bash
# LOCALIZATION/MUTATIONS/**/*.json 수정
# C# 소스 확인 필수!
find Assets/core_source -name "MutationName.cs"
```
→ 상세: `references/mutation-editing.md`

### 3. 새 화면 번역
```bash
# Scripts/02_Patches/10_UI/ 에 패치 추가
# 스코프 Push/Pop 균형 필수
```
→ 상세: `Docs/07_WORKFLOW.md`

### 4. 버그 수정
```bash
# 로그 확인
tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep "Qud-KR"
```
→ 상세: `Docs/05_ERROR_LOG.md`

## 검증 명령어

```bash
# 코드/JSON 무결성 검사
python3 tools/project_tool.py

# 게임에 배포
./tools/deploy-mods.sh

# Git 상태 확인
git status --short
```

## 폴더 구조 요약

```
qud_korean/
├── CONTEXT.yaml         ← 현재 상태 (첫 번째로 읽기!)
├── LOCALIZATION/        ← 번역 데이터
│   ├── README.md        ← 데이터 구조 가이드
│   ├── glossary_*.json  ← Layer 1 (단순 키-값)
│   └── MUTATIONS/       ← Layer 2 (구조화된 데이터)
├── Scripts/             ← C# 모드 코드
├── Docs/                ← 상세 가이드
└── tools/               ← 개발 도구
```

## Progressive Disclosure

이 스킬은 **필요한 정보만 단계적으로 로드**하도록 설계되었습니다:

1. **Level 0**: `CONTEXT.yaml` (60줄) - 현재 상태
2. **Level 1**: 이 `SKILL.md` (100줄) - 작업 개요
3. **Level 2**: `references/*` - 작업별 상세 정보
4. **Level 3**: `Docs/*` - 완전한 참조 문서
