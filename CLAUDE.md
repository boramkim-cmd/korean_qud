# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트 | v3.0 (2026-01-30)

## 세션 시작

1. `cat .claude/session-state.md` — "다음 액션"이 있으면 그것부터 실행
2. `git log --oneline -3`

## 트리거 테이블

| 상황 | 사용할 Skill/도구 |
|------|-------------------|
| 번역 안 되는 이유 파악 | `.claude/skills/diagnose/` |
| 패턴 코드 수정 전/후 | `.claude/skills/regression/` |
| C# 핵심 파일 수정 전 | `.claude/skills/code-context/` |
| 세션 종료 | `.claude/skills/session-end/` |
| 번역 커버리지 확인 | `python3 tools/build_asset_index.py --stats` |
| 배포 | `./deploy.sh` |

## 절대 규칙

1. **AttributeDataElement.Attribute** 직접 번역 → 크래시
2. **Dictionary 중복 키** → TypeInitializationException
3. **게임 테스트 생략** → Python 통과 ≠ 실제 동작
4. **커밋 지연** 금지 — 즉시 커밋
5. **셰이더 이름 번역** 금지 — `{{shader|content}}`에서 shader는 유지
6. **커밋 + 푸시 자동화** — 작업 완료 시 사용자 확인 없이 `git commit` + `git push` 즉시 실행

## 문서 레이어

| Layer | 파일 | 읽기 시점 |
|-------|------|----------|
| 0 | `.claude/session-state.md` | 항상 |
| 1 | `.claude/CONTEXT.md` | 필요시 |
| 2 | `.claude/danger-zones.md`, `.claude/anti-patterns.md` | 위험 작업 시 |
| 3 | `.claude/code-context/*.yaml` | C# 수정 시 |
| 4 | `.claude/skills/*/SKILL.md` | 트리거 테이블 참조 |
