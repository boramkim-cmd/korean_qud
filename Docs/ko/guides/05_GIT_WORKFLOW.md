# Git 자동 커밋 가이드

## 빠른 커밋 스크립트

### 1. 대화형 빠른 커밋 (권장)
```bash
bash tools/quick-commit.sh
```
- 현재 변경사항 표시
- 커밋 메시지 입력 프롬프트
- 커밋 타입 자동 감지 (feat/fix/docs/i18n/chore)
- 검증 실행
- 커밋 및 푸시

**예시:**
```bash
$ bash tools/quick-commit.sh
=== Current Changes ===
M Scripts/02_Patches/10_UI/CharacterCreation.cs
M LOCALIZATION/CHARGEN/ui.json

Enter commit message: Add stat translation support
Commit: fix: Add stat translation support
✓ Successfully committed and pushed!
```

### 2. 메시지 직접 지정 커밋
```bash
bash tools/auto-commit.sh "커밋 메시지"
```
- 커밋 메시지를 인자로 전달
- 검증 실패 시 커밋 차단
- 커밋 및 푸시

**예시:**
```bash
bash tools/auto-commit.sh "feat: 새 번역 화면 구현"
```

### 3. VS Code 작업 (키보드 단축키)

`Cmd+Shift+P` → "Run Task" 입력 → 선택:
- **"Quick Commit & Push"** - 대화형 커밋
- **"Validate & Deploy"** - 검증 및 게임 배포
- **"Run Validation Only"** - project_tool.py만 실행

또는 키보드 단축키 설정:
1. `Cmd+K Cmd+S`로 단축키 설정 열기
2. "Tasks: Run Task" 검색
3. 단축키 할당 (예: `Cmd+Shift+G`)

## 커밋 타입 규칙

변경된 파일 기반 자동 감지:

| 변경된 파일 | 타입 | 설명 |
|------------|------|------|
| `Scripts/02_Patches/` | `fix` | 패치 버그 수정 |
| `Scripts/` (기타) | `feat` | 새 기능 |
| `LOCALIZATION/` | `i18n` | 번역 업데이트 |
| `Docs/` | `docs` | 문서 업데이트 |
| `tools/` | `chore` | 도구/빌드 업데이트 |

## 수동 Git 명령어

수동 제어를 선호하는 경우:

```bash
# 1. 상태 확인
git status

# 2. 모든 변경사항 추가
git add -A

# 3. 커밋
git commit -m "타입: 메시지"

# 4. 푸시
git push origin main
```

## 검증

모든 스크립트는 커밋 전 `python3 tools/project_tool.py` 실행

검증 실패 시:
- **auto-commit.sh**: 커밋 차단
- **quick-commit.sh**: 강제 커밋 여부 묻기

검증 건너뛰기:
```bash
git add -A
git commit -m "메시지" --no-verify
git push origin main
```

## 모범 사례

1. **자주 커밋**: 논리적 변경 단위마다
2. **명확한 메시지**: 무엇을, 왜 변경했는지 설명
3. **검증 실행**: 모든 커밋 전
4. **원자적 커밋**: 하나의 기능/수정당 하나의 커밋

### 좋은 커밋 메시지
```
fix: 캐릭터 생성에서 스탯 번역 중복 해결
feat: 서브타입 설명 번역 지원 추가
i18n: 적절한 형식의 변이 번역 업데이트
docs: 언어와 목적별로 문서 재구성
```

### 나쁜 커밋 메시지
```
업데이트
수정
작업중
변경사항
```

## 문제 해결

### 푸시 거부됨 (원격보다 뒤처짐)
```bash
git pull --rebase origin main
git push origin main
```

### 병합 충돌
```bash
git status  # 충돌 파일 확인
# 파일을 편집하여 충돌 해결
git add -A
git rebase --continue
git push origin main
```

### 마지막 커밋 취소 (푸시 전)
```bash
git reset --soft HEAD~1  # 변경사항 유지
# 또는
git reset --hard HEAD~1  # 변경사항 삭제
```

### 마지막 커밋 취소 (푸시 후)
```bash
git revert HEAD
git push origin main
```
