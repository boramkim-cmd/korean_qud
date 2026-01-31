# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트 | v3.5 (2026-02-01)

## 다음 세션 할 일

> `.claude/session-state.md` 참조 (상세 맥락)

### 1. 게임 테스트 (전체 UI 번역 검증) ← **최우선**
- 게임 재시작 → `kr:stats`에서 **Species/Nouns ≠ 0** 확인
- HUD: 무게(kg), 드램, 허기/갈증 상태 한글 확인
- 능력치 약어(힘/민/건/지/의/자), 스킬 화면, 장비 슬롯 한글 확인
- 능력 바(AbilityBar), 활성화된 능력 이름 한글 확인
- Pipeline/Partial 비율 감소 확인

### 2. 남은 코드 리뷰 이슈 (낮은 우선순위)
- Status 스크린 스코프 누수 (Finalizer 미구현)
- GlobalUI Regex 핫패스, 중복 TryGetAnyTerm 3곳

### 3. 동적 패턴 85개 (게임 테스트 후 판단)
- `=creatureRegionAdjective= X` (58개), `*SultanName*` (26개)

### 4. Phase 4: 커뮤니티
- Steam Workshop 배포, README 한글화, 기여 가이드

**주의:** 성능 카운터/핫스팟 코드는 디버깅용. 안정화 후 제거할 것.

---

## 세션 시작

1. **위 "다음 세션 할 일"이 있으면 그것부터 실행**
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
7. **사용자 대면 한글 사용** — 선택지 제시, 작업 보고, 질문 등 사용자에게 보이는 텍스트는 한글로 작성 (내부 사고·코드·커밋 메시지는 영어 가능)
8. **아이템 수집 기록** — 스크린샷이나 아이템 관련 데이터가 제공되면, 아이템명과 해당 번역 패턴(DirectMatch/PrefixSuffix/Partial 등)을 분류하여 `.claude/item-observations.md`에 누적 기록. 개발 검증 시 체크리스트로 활용
9. **게임 테스트 = 커밋+푸시+배포 선행** — "게임에서 확인"이 필요한 경우, 반드시 `git commit` → `git push` → `./deploy.sh` 를 먼저 완료한 후 게임 테스트 진행

## 문서 레이어

| Layer | 파일 | 읽기 시점 |
|-------|------|----------|
| 0 | `.claude/session-state.md` | 항상 |
| 1 | `.claude/CONTEXT.md` | 필요시 |
| 2 | `.claude/danger-zones.md`, `.claude/anti-patterns.md` | 위험 작업 시 |
| 3 | `.claude/code-context/*.yaml` | C# 수정 시 |
| 4 | `.claude/skills/*/SKILL.md` | 트리거 테이블 참조 |
