# QUD_KOREAN 프로젝트 컨텍스트

> **이 파일은 Claude Code가 세션 시작 시 반드시 읽어야 하는 핵심 문서입니다.**
> 최종 업데이트: 2026-01-26 18:45

---

## 프로젝트 개요

| 항목 | 값 |
|------|-----|
| 프로젝트 | Caves of Qud 한글화 모드 |
| 저장소 | https://github.com/boramkim-cmd/korean_qud |
| 작업 폴더 | `/Users/ben/Desktop/qud_korean` |
| 모드 위치 | `~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean` |
| 진행률 | 58% (Phase 2 진행 중) |

---

## 현재 상태

### 동작 중인 기능
- 폰트 시스템 (d2coding.bundle)
- JSON 기반 로컬라이제이션
- 캐릭터 생성 UI
- 옵션 화면
- 튜토리얼 팝업
- **ObjectTranslator V2** (Pipeline 아키텍처, 26개 모듈)
- 메시지 로그 패치
- **ObjectTranslator 테스트 스크립트** (111개 테스트 케이스, 100% 통과)
- **V1 vs V2 컨텍스트별 테스트** (`tools/test_display_contexts.py`, 100개 케이스)
- **비자기참조 색상태그 번역** (`{{glittering|glitter}}` → `{{glittering|글리터}}`)

### 최근 이슈 (2026-01-26)
| 이슈 | 상태 | 원인 | 해결 |
|------|------|------|------|
| **비자기참조 색상태그 미번역** | ✅ CLEAR | `{{glittering\|glitter}}` 패턴 미처리 | `ColorTagProcessor`에 Step 1 추가 |
| **부분매칭 시 색상태그 번역 손실** | ✅ CLEAR | `originalName` 대신 `withTranslatedMaterials` 필요 | `DirectMatchHandler` 수정 |
| **색상태그 내 복합어 분리** | ✅ CLEAR | `{{G\|fresh water}}` 접두사 추출 시 분리됨 | 태그 내 복합어 선처리 로직 추가 |
| 캐시 키 불일치 | ✅ CLEAR | 컬러 태그/수량 유무에 따라 캐시 키 불일치 | `NormalizeCacheKey()` 메서드 추가 |
| 부분 매칭 시 접두사 미번역 | ✅ CLEAR | partial match 경로에서 접두사 번역 누락 | 모든 경로에 `TranslateBaseNounsOutsideTags()` 적용 |

### 테스트 필요 항목 (v2.0 버그 수정 후)
- [ ] `{{c|basic toolkit}}` → `기본 공구함` (색상 태그 내 명사)
- [ ] `{{w|copper nugget}}` → `구리 덩어리` (색상 태그 내 명사)
- [ ] `{{m|violet}} tube` → `보라색 튜브` (색상 형용사)
- [ ] `ape fur cloak` → `유인원 모피 망토` (species 접두사)
- [ ] `sandals of the river-wives` → `강 아내들의 샌들` (of 패턴 어순)
- [ ] `fried processing core` → `튀긴 처리 코어` (신규 modifier)

---

## 핵심 규칙 (반드시 준수)

### 1. 코드 변경 시
```bash
# 1. 수정
# 2. 배포 + 테스트
./deploy.sh
# 게임 실행 → 로그 확인
grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -20

# 3. 즉시 커밋 (나중에 하지 않기!)
git add <files> && git commit -m "type: 설명"
```

### 2. Dictionary 수정 시
```bash
# 중복 키 확인 필수!
grep -n "키이름" ObjectTranslator.cs
```

### 3. 번역 태그 보존
```
{{tag}}  - 게임 변수, 번역 금지
%var%    - 동적 값, 번역 금지
```

### 4. 위험 필드 (절대 번역 금지)
| 클래스 | 필드 | 이유 |
|--------|------|------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직에 사용 |

---

## 주요 명령어

```bash
# 배포
./deploy.sh

# 로그 확인
tail -f "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | grep -i "qud-kr"

# 검증
python3 tools/project_tool.py

# ObjectTranslator 번역 테스트 (100개 케이스)
python3 tools/test_object_translator.py

# V1 vs V2 컨텍스트별 테스트 (100개 케이스, 4개 컨텍스트)
python3 tools/test_display_contexts.py
python3 tools/test_display_contexts.py --verbose   # 상세 출력
python3 tools/test_display_contexts.py --context inventory  # 특정 컨텍스트

# 게임 내 디버그 (Ctrl+W → Wish)
kr:reload       # JSON 리로드
kr:stats        # 번역 통계
kr:check <id>   # 특정 블루프린트 확인
```

---

## 핵심 파일 위치

| 용도 | 파일 |
|------|------|
| 모드 진입점 | `Scripts/00_Core/00_00_00_ModEntry.cs` |
| 번역 엔진 | `Scripts/00_Core/00_00_01_TranslationEngine.cs` |
| 오브젝트 번역 (V2) | `Scripts/02_Patches/20_Objects/V2/ObjectTranslatorV2.cs` |
| 공통 어휘 (재료/품질/수식어) | `LOCALIZATION/OBJECTS/items/_common.json` |
| UI 패치 | `Scripts/02_Patches/10_UI/` |
| 번역 JSON | `LOCALIZATION/` |
| **번역 테스트 스크립트** | `tools/test_object_translator.py` |
| **V1 vs V2 컨텍스트 테스트** | `tools/test_display_contexts.py` |

---

## 문서 구조

```
Docs/
├── 00_CONTEXT.md      ← 이 파일 (세션 시작 시 필독)
├── 01_ARCHITECTURE.md  # 시스템 구조, 메서드 시그니처
├── 02_DEVELOPMENT.md   # 개발 가이드, 작업 흐름
├── 03_DATA.md          # JSON 구조, 번역 데이터 현황
├── 04_TODO.md          # 작업 목록
├── 05_CHANGELOG.md     # 변경 이력
├── 06_ERRORS.md        # 에러 기록
└── Issues/             # 이슈 리포트 (별도 관리)
```

---

## Claude Code 작업 규칙

### 세션 시작 시
1. 이 파일(`00_CONTEXT.md`) 읽기
2. 필요시 `04_TODO.md` 확인
3. 최근 커밋 확인: `git log --oneline -5`

### 코드 변경 시
1. 관련 문서 업데이트 (특히 `00_CONTEXT.md`의 "현재 상태")
2. 테스트 후 즉시 커밋
3. 버그 발생 시 `Issues/`에 리포트 작성

### 세션 종료 시
1. 미커밋 변경사항 확인: `git status`
2. 모든 변경사항 커밋
3. 필요시 `00_CONTEXT.md` 업데이트

---

## 용어 기준

| 영문 | 한글 |
|------|------|
| Toughness | 건강 |
| Strength | 힘 |
| Agility | 민첩 |
| Intelligence | 지능 |
| Willpower | 의지 |
| Ego | 자아 |
