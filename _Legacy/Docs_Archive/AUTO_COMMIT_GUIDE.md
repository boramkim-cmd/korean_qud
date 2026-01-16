# 자동 커밋 가이드

## 🚀 3가지 방법

### 방법 1: 수동 실행 (가장 간단) ⭐

파일 수정 후 터미널에서:

```bash
cd /Users/ben/Desktop/qud_korean
./sync-and-deploy.sh
```

**한 번에:**
- ✅ Git 커밋 + Push
- ✅ 게임 Mods 폴더 업데이트

---

### 방법 2: VS Code 단축키 설정

#### 1단계: 키보드 단축키 설정
`Cmd + K, Cmd + S` → 다음 추가:

```json
{
  "key": "cmd+s",
  "command": "workbench.action.tasks.runTask",
  "args": "Git: 빠른 저장 + 모드 배포",
  "when": "editorTextFocus"
}
```

#### 2단계: tasks.json에 작업 추가

`.vscode/tasks.json`:
```json
{
  "label": "Git: 빠른 저장 + 모드 배포",
  "type": "shell",
  "command": "./sync-and-deploy.sh",
  "options": {
    "cwd": "${workspaceFolder}"
  }
}
```

이제 `Cmd + S` 누르면 자동으로:
1. 파일 저장
2. Git 커밋
3. GitHub Push
4. 모드 배포

---

### 방법 3: 파일 감시 (완전 자동) 🤖

#### 필수: fswatch 설치

```bash
# Homebrew 권한 수정 (한 번만)
sudo chown -R ben /usr/local/Homebrew

# fswatch 설치
brew install fswatch
```

#### 사용법

터미널에서 실행:
```bash
cd /Users/ben/Desktop/qud_korean
./watch-and-sync.sh
```

**자동으로:**
- 파일 저장 감지
- 10초 쿨다운 후 자동 커밋
- GitHub Push
- 모드 배포

**종료:** `Ctrl + C`

---

## 📋 스크립트 비교

| 스크립트 | 용도 | 사용 시기 |
|---------|------|----------|
| `quick-save.sh` | Git만 (자동 메시지) | GitHub만 업데이트 |
| `sync.sh` | Git만 (메시지 지정) | GitHub만 업데이트 |
| `deploy-mods.sh` | 모드만 배포 | 게임 폴더만 업데이트 |
| `sync-and-deploy.sh` | Git + 모드 | **추천!** 모든 것 한 번에 |
| `watch-and-sync.sh` | 자동 감시 | 완전 자동화 원할 때 |

---

## 🎯 권장 워크플로우

### 일반 작업
```bash
# 1. 파일 수정 (VS Code)
# 2. 터미널에서
cd /Users/ben/Desktop/qud_korean
./sync-and-deploy.sh

# 3. 게임 재시작
```

### 집중 작업 (자동화)
```bash
# 터미널 1: 파일 감시 시작
cd /Users/ben/Desktop/qud_korean
./watch-and-sync.sh

# 터미널 2 또는 VS Code: 작업
# → 파일 저장하면 자동으로 모든 것 처리!
```

---

## 🔧 fswatch 설치 문제 해결

### 권한 에러
```bash
sudo chown -R $(whoami) /usr/local/Homebrew
chmod u+w /usr/local/Homebrew
```

### 설치 확인
```bash
which fswatch
# /usr/local/bin/fswatch 출력되면 성공
```

### 대안: 수동 실행
fswatch 없이도 `sync-and-deploy.sh`만으로 충분합니다!

---

## ⚡ 빠른 참조

```bash
# 가장 많이 사용할 명령어
cd /Users/ben/Desktop/qud_korean
./sync-and-deploy.sh

# 또는 커밋 메시지 지정
./sync-and-deploy.sh "feat: 새로운 기능 추가"
```

**이것만 기억하세요!** ☝️

---

## 📝 현재 설정

### 작업 폴더
`/Users/ben/Desktop/qud_korean`

### 게임 Mods 폴더
`/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/`
- `Core_QudKREngine`
- `QudKR_Translation`

### GitHub
https://github.com/codekkj/qud_korean

---

## ✨ 완료!

이제 파일 수정 후 `./sync-and-deploy.sh` 한 번이면 모든 것이 자동으로 처리됩니다!
