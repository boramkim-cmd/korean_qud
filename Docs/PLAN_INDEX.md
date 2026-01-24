# Caves of Qud 한글화 모드 - 계획 문서 인덱스

> **최종 업데이트**: 2026-01-23
> **프로젝트 위치**: `/Users/ben/Desktop/qud_korean/`

---

## 문서 목록

| 순서 | 파일명 | 상태 | 설명 |
|------|--------|------|------|
| 1 | [PLAN_01_ORIGINAL.md](./PLAN_01_ORIGINAL.md) | ⚠️ 검토 필요 | 원본 계획 - 디버그 시스템 + Parts/Events 전환 |
| 2 | [PLAN_02_CRITICAL_ANALYSIS.md](./PLAN_02_CRITICAL_ANALYSIS.md) | ✅ 완료 | 원본 계획의 문제점 분석 |
| 3 | PLAN_03_REVISED.md | 📝 미작성 | 수정된 실행 계획 (다음 세션에서 작성) |

---

## 다음 세션에서 할 일

### 1단계: 컨텍스트 복원
```
1. 이 인덱스 문서 읽기
2. PLAN_02_CRITICAL_ANALYSIS.md 읽기 (문제점 파악)
3. 현재 코드 상태 확인
```

### 2단계: 실제 문제 수집
```
1. 게임 실행
2. 메인 메뉴 → 번역 안 되는 텍스트 스크린샷
3. 캐릭터 생성 → 번역 안 되는 텍스트 스크린샷
4. kr:untranslated 명령어로 오브젝트 목록 확인
```

### 3단계: 수정된 계획 작성
```
1. 실제 버그 목록 기반으로 우선순위 정리
2. PLAN_03_REVISED.md 작성
3. 최소한의 코드 변경으로 해결책 설계
```

---

## 핵심 결정 사항

### 하지 않기로 한 것
- [ ] TranslationResult struct 도입 (성능 문제)
- [ ] ScopeManager 인터페이스 변경 (호환성 파괴)
- [ ] 디버그 텍스트를 반환값에 포함 (데이터 오염)
- [ ] Parts/Events 전면 전환 (검증 없이)

### 해야 할 것
- [ ] 실제 버그 목록 작성
- [ ] JSON 데이터 보완 (번역 누락)
- [ ] 기존 Debug.Log 활용
- [ ] Parts/Events PoC (선택적)

---

## 코드베이스 요약

### 핵심 파일 (수정 시 주의)
| 파일 | 역할 | 라인 수 |
|------|------|---------|
| `00_Core/00_00_01_TranslationEngine.cs` | 번역 핵심 로직 | 215 |
| `00_Core/00_00_02_ScopeManager.cs` | 스코프 스택 관리 | 101 |
| `00_Core/00_00_03_LocalizationManager.cs` | JSON 로딩 | 483 |
| `02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` | 오브젝트 번역 | 745 |

### 디버그 명령어 (기존)
| 명령어 | 위치 | 설명 |
|--------|------|------|
| `kr:reload` | DebugWishes.cs:35 | JSON 리로드 |
| `kr:check <blueprint>` | DebugWishes.cs:56 | 특정 블루프린트 번역 확인 |
| `kr:untranslated` | DebugWishes.cs:91 | 현재 존 미번역 목록 |
| `kr:stats` | DebugWishes.cs:145 | 번역 통계 |
| `kr:clearcache` | DebugWishes.cs:163 | 캐시 클리어 |
| `kr:fontinfo` | DebugWishes.cs:181 | 폰트 정보 조사 |

---

## 세션 기록

| 날짜 | 작업 내용 | 결과 |
|------|----------|------|
| 2026-01-23 | 원본 계획 검토 + 비판적 분석 | 문서 2개 생성 |
| | | |

---

## 빠른 참조

### 프로젝트 열기
```bash
cd /Users/ben/Desktop/qud_korean
code .
```

### 빌드 (추정)
```bash
# 프로젝트 파일 확인 필요
dotnet build QudKorean.csproj
```

### 모드 설치 위치 (macOS)
```
~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization/
```
