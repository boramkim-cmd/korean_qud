# Git 자동 동기화 가이드

## 🚀 빠른 시작

### 방법 1: 터미널에서 실행

#### 빠른 저장 (가장 간단)
```bash
cd /Users/ben/Desktop/QudKR_Translation
./quick-save.sh
```
자동으로 커밋 메시지 생성 후 push합니다.

#### 커밋 메시지 지정
```bash
cd /Users/ben/Desktop/QudKR_Translation
./sync.sh "feat: 메인 메뉴 번역 추가"
```

### 방법 2: VS Code에서 실행

1. **Command Palette** 열기: `Cmd + Shift + P`
2. **Tasks: Run Task** 입력
3. 다음 중 선택:
   - `Git: 빠른 저장` - 자동 커밋 메시지
   - `Git: 동기화 (메시지 입력)` - 커밋 메시지 입력
   - `Git: Pull (원격 변경사항 가져오기)`
   - `Git: 상태 확인`

### 방법 3: 키보드 단축키 설정 (선택사항)

VS Code에서 `Cmd + K, Cmd + S` → 키보드 단축키 설정:

```json
{
  "key": "cmd+shift+s",
  "command": "workbench.action.tasks.runTask",
  "args": "Git: 빠른 저장"
}
```

---

## 📋 스크립트 설명

### sync.sh
완전한 Git 동기화 스크립트

**기능:**
1. 현재 상태 확인
2. 모든 변경사항 스테이징 (`git add .`)
3. 커밋 생성
4. 원격 저장소에서 최신 변경사항 가져오기 (`git pull --rebase`)
5. Push (`git push`)

**사용법:**
```bash
./sync.sh "커밋 메시지"
```

### quick-save.sh
빠른 저장 스크립트 (커밋 메시지 자동 생성)

**기능:**
- 타임스탬프로 자동 커밋 메시지 생성
- `sync.sh` 호출

**사용법:**
```bash
./quick-save.sh
```

**생성되는 커밋 메시지 예시:**
```
chore: quick save - 2026-01-15 14:46:30
```

---

## 🎯 사용 시나리오

### 시나리오 1: 작업 중 자주 저장
```bash
# 파일 수정 후
./quick-save.sh
```

### 시나리오 2: 의미 있는 커밋
```bash
# 새 기능 추가 후
./sync.sh "feat: 옵션 화면 번역 추가"

# 버그 수정 후
./sync.sh "fix: 색상 태그 처리 버그 수정"

# 문서 업데이트 후
./sync.sh "docs: 개발 가이드 업데이트"
```

### 시나리오 3: 원격 변경사항 가져오기
```bash
git pull --rebase origin main
```

---

## ⚙️ VS Code 설정

### 자동 저장 활성화
`.vscode/settings.json`에 다음 설정이 추가되었습니다:

```json
{
  "files.autoSave": "afterDelay",
  "files.autoSaveDelay": 1000,
  "git.autofetch": true,
  "git.confirmSync": false,
  "git.enableSmartCommit": true
}
```

**의미:**
- 파일 수정 후 1초 뒤 자동 저장
- Git 자동 fetch 활성화
- 동기화 확인 대화상자 비활성화

---

## 🔄 워크플로우 예시

### 일반적인 작업 흐름

```bash
# 1. 프로젝트 디렉토리로 이동
cd /Users/ben/Desktop/QudKR_Translation

# 2. 파일 수정 (VS Code 또는 다른 에디터)

# 3. 빠른 저장
./quick-save.sh

# 4. 계속 작업...

# 5. 의미 있는 단위로 커밋
./sync.sh "feat: 인벤토리 화면 번역 완료"
```

### 하루 작업 시작 시

```bash
# 원격 저장소의 최신 변경사항 가져오기
cd /Users/ben/Desktop/QudKR_Translation
git pull --rebase origin main
```

### 하루 작업 종료 시

```bash
# 모든 변경사항 저장
cd /Users/ben/Desktop/QudKR_Translation
./sync.sh "chore: end of day save - $(date +%Y-%m-%d)"
```

---

## 🛠️ 문제 해결

### "변경사항이 없습니다" 메시지
→ 정상입니다. 저장할 내용이 없다는 의미입니다.

### "Pull 실패 - 충돌 해결 필요"
→ 수동으로 충돌 해결 필요:
```bash
# 충돌 파일 확인
git status

# 충돌 해결 후
git add .
git rebase --continue

# 또는 rebase 취소
git rebase --abort
```

### "Push 실패"
→ 네트워크 또는 권한 문제:
```bash
# 원격 저장소 확인
git remote -v

# 다시 시도
git push origin main
```

---

## 📊 커밋 메시지 규칙

### 권장 형식
```
<type>: <subject>

<body> (선택사항)
```

### Type 종류
- `feat`: 새로운 기능 추가
- `fix`: 버그 수정
- `docs`: 문서 수정
- `refactor`: 코드 리팩토링
- `test`: 테스트 추가
- `chore`: 빌드, 설정 변경
- `style`: 코드 스타일 변경 (포맷팅)

### 예시
```bash
./sync.sh "feat: 메인 메뉴 번역 데이터 추가"
./sync.sh "fix: ScopeManager 스택 오버플로우 수정"
./sync.sh "docs: INSTALLATION.md 업데이트"
./sync.sh "refactor: TranslationEngine 성능 개선"
```

---

## 🔐 보안 주의사항

### .gitignore 확인
다음 파일들은 Git에 포함되지 않습니다:
- `.DS_Store` (macOS 시스템 파일)
- `*.log` (로그 파일)
- `.vscode/` (VS Code 설정 - 선택적)

### Personal Access Token
- Token은 절대 코드에 포함하지 마세요
- `.gitignore`에 민감한 정보 추가

---

## 📚 추가 Git 명령어

### 상태 확인
```bash
git status
```

### 커밋 히스토리
```bash
git log --oneline -10
git log --graph --oneline --all
```

### 변경사항 확인
```bash
git diff
git diff --staged
```

### 특정 파일만 커밋
```bash
git add Scripts/01_Data/MainMenu.cs
git commit -m "feat: 메인 메뉴 데이터 업데이트"
git push
```

### 마지막 커밋 수정
```bash
git commit --amend -m "새로운 커밋 메시지"
git push --force
```

---

## 🎉 완료!

이제 다음 명령어만 기억하세요:

```bash
# 빠른 저장
./quick-save.sh

# 의미 있는 커밋
./sync.sh "커밋 메시지"
```

또는 VS Code에서 `Cmd + Shift + P` → `Git: 빠른 저장`
