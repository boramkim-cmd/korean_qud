# Caves of Qud 한글화 프로젝트 - 도구 및 빌드 가이드

> **문서 버전**: 1.0 | **최종 업데이트**: 2026-01-18

> [!NOTE]
> 이 문서는 `01_DEVELOPMENT_GUIDE.md`를 보완하며, 프로젝트의 기술적 검증 도구(Build System)와 유틸리티 스크립트 사용법을 상세히 다룹니다.

---

# Part 1: C# 빌드 시스템 (.NET)

C# 스크립트의 문법 오류와 참조 오류를 런타임(게임 실행) 전에 정적으로 검증하기 위해 **.NET 빌드 시스템**을 도입했습니다.

## 1.1 필요성
기존에는 코드를 수정하고 게임을 실행해야만 오타(Typos)나 네임스페이스 오류를 확인할 수 있었습니다. 이는 2가지를 의미합니다:
1. **긴 피드백 루프**: 게임 로딩까지 기다려야 함
2. **숨겨진 버그**: 특정 상황에서만 실행되는 코드는 테스트 누락 시 런타임 에러 발생

`.NET Build`를 사용하면 **컴파일 타임**에 모든 참조 오류를 즉시 잡아낼 수 있습니다.

## 1.2 사전 준비 (Prerequisites)

이 프로젝트는 현재 `net48` (.NET Framework 4.8)을 타겟으로 합니다. (게임의 `0Harmony.dll` 호환성 때문)

1. **.NET SDK 설치**:
   ```bash
   brew install --cask dotnet-sdk
   ```
2. **프로젝트 파일 생성**:
   `tools/generate_csproj.py` 스크립트는 게임 설치 경로의 모든 DLL을 자동으로 스캔하여 `QudKorean.csproj`를 생성합니다.
   ```bash
   python3 tools/generate_csproj.py
   ```
   > **주의**: 스팀(Steam) 기본 설치 경로가 아닌 경우, 스크립트 내부의 `managed_dir` 경로를 수정해야 합니다.

## 1.3 빌드 및 검증 (Usage)

터미널에서 다음 명령을 실행하여 코드를 검증합니다.

```bash
dotnet build QudKorean.csproj
```

### ✅ 성공 메시지 예시
```
QudKorean net48 0 오류와 3 경고와 함께 성공 (2.3초) -> bin/Debug/net48/QudKorean.dll
```
*경고(Warning) 메시지는 Unity 엔진 DLL 간의 버전 충돌로 인한 것으로, '성공'이 떴다면 무시해도 안전합니다.*

### ❌ 실패 시 대처
- **CS0246 (네임스페이스/형식 찾을 수 없음)**: `tools/generate_csproj.py`를 다시 실행해 보세요.
- **CS0103, CS1501 (메서드 시그니처 불일치)**: 실제 코드 오류입니다. 해당 파일을 수정하세요.

---

# Part 2: 도구 모음 (Tool Chain)

## 2.1 프로젝트 통합 도구 (`project_tool.py`)
프로젝트의 상태를 점검하고 메타데이터를 관리하는 핵심 도구입니다.

- **실행**: `python3 tools/project_tool.py`
- **검증 항목**:
  1. C# 파일들의 중괄호(`{}`) 균형
  2. JSON 번역 파일의 문법 오류
  3. JSON 파일 내 중복 키(Key) 및 빈 값(Value)
  4. 중복 함수 정의 (오버로딩 제외, 동일 시그니처 중복 탐지)
- **자동 기능**:
  - `HEADER`가 없는 C# 파일에 표준 주석 헤더 추가 (파일 인덱싱용)

> [!IMPORTANT]
> **Git Push 전 반드시 이 도구를 통과해야 합니다.**

## 2.2 프로젝트 파일 생성기 (`generate_csproj.py`)
`dotnet build`를 위한 `.csproj` 파일을 생성합니다.

- **실행**: `python3 tools/generate_csproj.py`
- **특징**:
  - 게임의 `Managed` 폴더 내 모든 `.dll`을 자동으로 참조 (`<Reference>`)
  - 시스템 어셈블리(`mscorlib`, `System` 등)는 제외하여 .NET SDK 충돌 방지
  - `TargetFramework`를 `net48`로 자동 설정

## 2.3 모드 배포 스크립트 (`deploy-mods.sh`)
작업한 결과물을 실제 게임의 모드 폴더로 복사합니다.

- **실행**: `./tools/deploy-mods.sh "커밋 메시지"`
- **기능**:
  1. `project_tool.py`를 실행하여 무결성 검증 (실패 시 중단)
  2. `Scripts/` 및 `LOCALIZATION/` 폴더를 게임 모드 경로로 복사

  3. 불필요한 파일(`.DS_Store`, `_Legacy`) 제거
  4. (옵션) Git Commit 및 Push 자동 수행

## 2.4 설정 (Configuration)
사용자 환경에 따라 도구 설정을 변경할 수 있습니다.

- **설정 파일**: `tools/config.json` (없을 경우 `config.json.example` 참조)
- **주요 설정**:
  ```json
  {
      "game_mod_dir": "~/Library/Application Support/..." 
  }
  ```
- **기능**: `deploy-mods.sh`가 이 경로를 참조하여 배포합니다. 여러 PC에서 작업할 때 유용합니다.

---

# Part 3: 추천 워크플로우 (Recommended Workflow)

안정적인 개발을 위해 다음 순서로 작업하는 것을 권장합니다.

### Step 1: 코드 수정
VS Code 등의 에디터에서 C# 코드를 수정합니다.
*(예: `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs` 수정)*

### Step 2: 정적 컴파일 검증
터미널에서 즉시 빌드를 수행하여 문법 오류를 잡습니다.
```bash
dotnet build QudKorean.csproj
```
*(오류가 있다면 수정 후 반복)*

### Step 3: 프로젝트 무결성 검증 & 배포
빌드가 성공하면 게임 폴더로 보내 테스트합니다.
```bash
./tools/deploy-mods.sh "Fix variable shadowing in CharacterCreation"
```

### Step 4: 인게임 테스트
게임을 실행하여 실제 동작을 확인합니다.
*(빌드 시스템은 로직 오류나 런타임 NullReference는 잡지 못하므로 확인이 필수입니다)*

---

# Part 4: 트러블슈팅 (Troubleshooting)

### Q1. `HarmonyPatch` 형식을 찾을 수 없다고 나옵니다.
- **원인**: `.csproj`의 타겟 프레임워크가 게임의 `0Harmony.dll` 버전과 맞지 않음.
- **해결**: `tools/generate_csproj.py`에서 `TargetFramework`가 `net48`인지 확인하고 스크립트를 재실행하세요.

### Q2. `Stack<>` 형식이 `mscorlib`에 없다고 나옵니다.
- **원인**: 게임 폴더의 `mscorlib.dll`을 강제로 참조하여 SDK 기본 라이브러리와 충돌.
- **해결**: `tools/generate_csproj.py`에는 이미 `mscorlib`를 제외하는 로직이 포함되어 있습니다. 스크립트를 재실행하여 `.csproj`를 갱신하세요.

### Q3. `generate_csproj.py` 실행 시 경로 오류가 뜹니다.
- `generated_csproj.py` 파일을 열고 `managed_dir` 변수가 본인의 PC 환경(Steam 설치 경로)과 일치하는지 확인하세요. macOS 기본 경로는 다음과 같습니다:
  `/Users/[User]/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed`
