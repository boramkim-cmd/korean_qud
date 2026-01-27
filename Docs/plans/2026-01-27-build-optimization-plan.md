# qud_korean 빌드 시스템 최적화 계획

> **목표:** 게임 시작/플레이 중 버퍼 감소를 위한 빌드 시스템 구축
> **접근:** 작업 폴더는 개발 친화적으로 유지, 배포 시 최적화 + 소스맵으로 에러 추적
> **작성일:** 2026-01-27
> **상태:** Phase 1 완료

---

## 목차

1. [배경 및 문제 분석](#1-배경-및-문제-분석)
2. [전체 아키텍처](#2-전체-아키텍처)
3. [Phase 1: JSON 번들링 + 소스맵](#3-phase-1-json-번들링--소스맵) ✅ 완료
4. [Phase 2: 바이너리 포맷 + 인덱스](#4-phase-2-바이너리-포맷--인덱스) (필요시)
5. [구현 상태](#5-구현-상태)

---

## 1. 배경 및 문제 분석

### 1.1 현재 시스템 현황

| 항목 | 현재 값 |
|------|---------|
| JSON 파일 수 | 302개 |
| 총 데이터 크기 | 1.6MB |
| 번역 항목 수 | 2,847개 blueprint + 4,320개 어휘 |
| 번역 커버리지 | 99% (1,617/1,634 복합어) |
| 테스트 케이스 | 197개, 100% 통과 |

### 1.2 확인된 성능 문제

- **시작 시 지연 (500ms - 2초)**: 302개 파일 순차 로딩
- **게임 중 지연**: 절차적 생성 이름의 캐시 미스
- **Unity 특수 상황**: Dictionary 룩업이 배열보다 10-17배 느림

---

## 2. 전체 아키텍처

```
작업 폴더 (302개 JSON)
        ↓
  build_optimized.py
        ↓
dist/ (5개 번들 + sourcemap.json)
        ↓
    deploy.sh
        ↓
게임 모드 폴더 (번들 로딩)
```

---

## 3. Phase 1: JSON 번들링 + 소스맵 ✅ 완료

### 3.1 구현된 파일

| 파일 | 역할 |
|------|------|
| `tools/build_optimized.py` | 302개 JSON → 5개 번들 + sourcemap |
| `Scripts/.../Data/SourceMap.cs` | 소스맵 로딩 및 에러 추적 |
| `Scripts/.../Data/JsonRepository.cs` | 번들/소스 파일 자동 선택 로딩 |
| `Scripts/.../ObjectTranslatorV2.cs` | 소스맵 기반 에러 로깅 |
| `deploy.sh` | 빌드 실행 + 배포 |

### 3.2 번들 구조

| 번들 | 크기 | 항목 수 |
|------|------|---------|
| objects.json | 470KB | creatures 312, items 565, furniture 74, terrain 100, hidden 20, widgets 42, vocabulary 2156 |
| shared.json | 17KB | 5 |
| chargen.json | 24KB | 12 |
| gameplay.json | 48KB | 25 |
| ui.json | 48KB | 18 |
| **총계** | **607KB** | **3,329** |

### 3.3 소스맵 구조

```json
{
  "_meta": { "buildTime": "...", "phase": 1 },
  "blueprints": {
    "Bear": { "file": "LOCALIZATION/OBJECTS/creatures/...", "line": 40 }
  },
  "vocabulary": {
    "iron": { "file": "...", "line": 10, "korean": "철" }
  }
}
```

---

## 4. Phase 2: 바이너리 포맷 + 인덱스 (필요시)

> Phase 1 효과 측정 후 필요시 진행

- MessagePack 바이너리 포맷 (5-10배 빠른 파싱)
- Prefix Trie 인덱스 (O(L) prefix 매칭)
- 배열 기반 해시 인덱스 (Unity에서 17배 빠름)

---

## 5. 구현 상태

### Phase 1 체크리스트 ✅

- [x] `tools/build_optimized.py` 생성
- [x] `Scripts/.../Data/SourceMap.cs` 생성
- [x] `Scripts/.../Data/JsonRepository.cs` 수정 (번들 로딩)
- [x] `Scripts/.../ObjectTranslatorV2.cs` 수정 (에러 로깅)
- [x] `deploy.sh` 수정 (빌드 단계 추가)
- [x] 빌드 테스트 통과
- [x] 배포 테스트 통과
- [ ] 게임 내 테스트 (다음 단계)

### 다음 단계

1. **게임 테스트**: `kr:stats` 명령으로 번들 모드 확인
2. **성능 측정**: 로딩 시간 비교 (번들 vs 소스)
3. **Phase 2 결정**: 성능 측정 결과에 따라 진행 여부 결정

---

## 6. 롤백 계획

```bash
# 번들 제거하고 소스 파일 모드로 복귀
rm -rf ~/Library/.../Mods/qud_korean/data
rm -f ~/Library/.../Mods/qud_korean/sourcemap.json
# LOCALIZATION/이 있으면 자동으로 소스 모드로 동작
```
