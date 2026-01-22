# 🔧 Tools Directory

> **Last Updated**: 2026-01-22

도구 스크립트 모음. 프로젝트 검증, 배포, 세션 관리에 사용됩니다.

---

## 📌 핵심 도구 (Core Tools)

| 도구 | 설명 | 사용법 |
|------|------|--------|
| `project_tool.py` | **통합 검증 도구 v2.0** | `python3 tools/project_tool.py [command]` |
| `session_manager.py` | 세션 상태 저장/복원 | `python3 tools/session_manager.py save` |
| `quick-save.sh` | Git 커밋 & 푸시 | `bash tools/quick-save.sh` |
| `sync-and-deploy.sh` | 모드 배포 | `bash tools/sync-and-deploy.sh` |

---

## 🚀 project_tool.py 서브커맨드

```bash
python3 tools/project_tool.py           # 전체 검증 (기본)
python3 tools/project_tool.py validate  # 코드/JSON 검증만
python3 tools/project_tool.py build     # 빌드만
python3 tools/project_tool.py glossary  # 용어집 분석
python3 tools/project_tool.py stats     # 통계 출력
python3 tools/project_tool.py help      # 도움말
```

---

## 📁 파일 목록

### Active Scripts
| 파일 | 역할 |
|------|------|
| `project_tool.py` | 코드/JSON/빌드 검증 + 메타데이터 생성 |
| `session_manager.py` | AI 세션 핸드오프 관리 |
| `analyze_glossary.py` | 용어집 중복/구조 분석 (상세) |
| `sort_json.py` | JSON 키 정렬 |
| `deploy-mods.sh` | 게임 모드 폴더로 배포 |
| `quick-save.sh` | Git 빠른 저장 |
| `sync-and-deploy.sh` | 검증 + 배포 통합 |

### Config Files
| 파일 | 용도 |
|------|------|
| `config.json.example` | 배포 경로 설정 예시 |
| `session_state.json` | 세션 상태 데이터 |
| `project_metadata.json` | 자동 생성 메타데이터 |

### Shell Scripts
| 파일 | 용도 |
|------|------|
| `create-issue.sh` | GitHub 이슈 생성 |
| `list-issues.sh` | 로컬 이슈 목록 |
| `update-issue-status.sh` | 이슈 상태 변경 |
| `validate-mod.sh` | 모드 구조 검증 |

---

## 📦 _legacy 폴더

더 이상 사용되지 않는 스크립트들. 참고용으로 보관.

| 파일 | 원래 역할 |
|------|----------|
| `check_missing_translations.py` | XML 기반 미번역 검사 (JSON으로 전환됨) |
| `verify_structure_data.py` | 구조 데이터 검증 (project_tool로 통합) |
| `fix_empty_descriptions.py` | 빈 설명 수정 (일회성) |
| `populate_all_subtypes.py` | 서브타입 JSON 생성 (완료) |
| ... | (기타 마이그레이션 완료 스크립트) |

---

## ⚡ 빠른 참조

```bash
# 일일 작업 플로우
python3 tools/project_tool.py   # 검증
bash tools/quick-save.sh        # 커밋

# 배포
bash tools/sync-and-deploy.sh   # 검증 + 배포

# 세션 관리
python3 tools/session_manager.py save    # 세션 저장
python3 tools/session_manager.py status  # 상태 확인
```

---

*이 문서는 P3-06 Tool Consolidation 작업의 일부로 생성됨 (2026-01-22)*
