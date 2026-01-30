# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트 | v3.1 (2026-01-30)

## 다음 세션 할 일 (2026-01-30)

> 이 작업 완료 후 이 섹션을 비우고 커밋할 것

### 1단계: 배포 + 기본 확인
```bash
./deploy.sh
# 게임 실행 후:
# kr:stats    → Mode: bundle 확인, 항목 수 증가 확인
# kr:perf     → 스킵율 50%+, 폰트 캐시 히트 확인
```

### 2단계: 신규 JSON 로드 확인
새로 추가된 6개 파일이 번들에 포함되었는지:
```bash
python3 -c "
import json
d = json.load(open('dist/data/objects.json'))
for key in ['phenomena','data_objects','widgets','worlds']:
    print(f'{key}: {key in str(d)[:10000]}')
"
```
안 보이면 → `tools/build_optimized.py`에 새 경로 등록 필요

### 3단계: 인게임 번역 확인
- **팩션**: 평판 화면에서 "영양", "유인원", "거미류" 등 확인
- **지역 생물**: 소금 습지/사막 협곡/언덕에서 "습지 쿠두", "협곡 유인원" 등 확인
- **위시 커맨드**: `wish` → 목록에서 한글 표시 확인
- **음식**: 인벤토리에서 "응고된 불꽃", "뼈 가루" 등 확인

### 문제 발생 시
| 증상 | 원인 | 해결 |
|------|------|------|
| 신규 JSON 미로드 | build_optimized.py 경로 미등록 | `tools/build_optimized.py`에 경로 추가 |
| 팩션명 영어 | factions.json 로드 패치 없음 | C# 패치 확인: `Scripts/02_Patches/10_UI/` |
| 지역 생물 영어 | CompoundTranslator 사전 미참조 | modifiers.json 로드 경로 확인 |
| 위시 커맨드 영어 | wish_commands.json 패치 없음 | UI 패치 추가 필요 |

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

## 문서 레이어

| Layer | 파일 | 읽기 시점 |
|-------|------|----------|
| 0 | `.claude/session-state.md` | 항상 |
| 1 | `.claude/CONTEXT.md` | 필요시 |
| 2 | `.claude/danger-zones.md`, `.claude/anti-patterns.md` | 위험 작업 시 |
| 3 | `.claude/code-context/*.yaml` | C# 수정 시 |
| 4 | `.claude/skills/*/SKILL.md` | 트리거 테이블 참조 |
