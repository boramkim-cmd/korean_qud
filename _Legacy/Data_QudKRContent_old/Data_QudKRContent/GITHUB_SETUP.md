# GitHub 연동 가이드

## 현재 상태

✅ **로컬 Git 저장소 생성 완료**
- 초기 커밋 완료 (14개 파일)
- 브랜치: `main`
- 원격 저장소 설정: `https://github.com/codekkj/qud_korean.git`

⚠️ **GitHub 인증 필요**
- Push 시 인증 에러 발생
- GitHub Personal Access Token 또는 SSH 키 필요

---

## 해결 방법

### 방법 1: Personal Access Token (권장)

#### 1단계: GitHub에서 Token 생성

1. GitHub 웹사이트 접속: https://github.com
2. 우측 상단 프로필 → **Settings**
3. 좌측 메뉴 하단 → **Developer settings**
4. **Personal access tokens** → **Tokens (classic)**
5. **Generate new token** → **Generate new token (classic)**
6. 설정:
   - Note: `Qud Korean Translation`
   - Expiration: `90 days` 또는 원하는 기간
   - Scopes: **repo** 전체 체크 ✅
7. **Generate token** 클릭
8. **토큰 복사** (한 번만 표시됨!)

#### 2단계: Git Credential 설정

터미널에서 실행:

```bash
# macOS Keychain에 저장
git config --global credential.helper osxkeychain

# 원격 저장소 URL을 Token 방식으로 변경
cd /Users/ben/Desktop/QudKR_Translation
git remote set-url origin https://YOUR_TOKEN@github.com/codekkj/qud_korean.git
```

**YOUR_TOKEN**을 1단계에서 복사한 토큰으로 교체하세요.

#### 3단계: Push

```bash
cd /Users/ben/Desktop/QudKR_Translation
git push -u origin main
```

---

### 방법 2: SSH 키 (더 안전)

#### 1단계: SSH 키 생성 (없는 경우)

```bash
# SSH 키 확인
ls -la ~/.ssh

# 없으면 생성
ssh-keygen -t ed25519 -C "your_email@example.com"
# Enter 3번 (기본 경로, 비밀번호 없음)

# 공개 키 복사
cat ~/.ssh/id_ed25519.pub
```

#### 2단계: GitHub에 SSH 키 등록

1. GitHub → **Settings** → **SSH and GPG keys**
2. **New SSH key** 클릭
3. Title: `Mac - Qud Translation`
4. Key: 1단계에서 복사한 공개 키 붙여넣기
5. **Add SSH key** 클릭

#### 3단계: 원격 저장소 URL 변경

```bash
cd /Users/ben/Desktop/QudKR_Translation
git remote set-url origin git@github.com:codekkj/qud_korean.git
```

#### 4단계: Push

```bash
git push -u origin main
```

---

## 빠른 해결 (임시)

GitHub Desktop 앱을 사용하면 인증이 자동으로 처리됩니다:

1. GitHub Desktop 다운로드: https://desktop.github.com
2. GitHub 계정으로 로그인
3. **File** → **Add Local Repository**
4. `/Users/ben/Desktop/QudKR_Translation` 선택
5. **Publish repository** 클릭

---

## 수동 Push 명령어

인증 설정 후 사용할 명령어:

```bash
# 현재 디렉토리로 이동
cd /Users/ben/Desktop/QudKR_Translation

# 상태 확인
git status

# 변경사항 추가
git add .

# 커밋
git commit -m "커밋 메시지"

# Push
git push
```

---

## 향후 워크플로우

### 일반적인 작업 흐름

```bash
# 1. 파일 수정 후
cd /Users/ben/Desktop/QudKR_Translation

# 2. 변경사항 확인
git status
git diff

# 3. 스테이징
git add .

# 4. 커밋
git commit -m "feat: 메인 메뉴 번역 추가"

# 5. Push
git push
```

### 커밋 메시지 규칙

```
feat: 새로운 기능 추가
fix: 버그 수정
docs: 문서 수정
refactor: 코드 리팩토링
test: 테스트 추가
chore: 빌드, 설정 변경
```

**예시:**
```bash
git commit -m "feat: 옵션 화면 번역 데이터 추가"
git commit -m "fix: 색상 태그 처리 버그 수정"
git commit -m "docs: 개발 가이드 업데이트"
```

---

## 현재 커밋 내역

```
commit 0c047ad
Author: 기중
Date: 2026-01-15

Initial commit: QudKR_Translation v0.2.0

- Modular architecture with separated data and logic
- Core systems: ScopeManager, TranslationEngine, ModEntry
- Bug prevention: Stack-based scope management
- Complete documentation system
- Example implementations: MainMenu, Common data

Files: 14 files, 1397 insertions
```

---

## 문제 해결

### "Authentication failed" 에러
→ Personal Access Token 또는 SSH 키 설정 필요 (위 방법 1 또는 2)

### "Permission denied" 에러
→ GitHub 저장소 권한 확인 (본인 저장소인지)

### "Repository not found" 에러
→ 저장소 URL 확인: `git remote -v`

---

## 다음 단계

1. **인증 설정** (방법 1 또는 2 선택)
2. **Push 실행**
3. **GitHub에서 확인**: https://github.com/codekkj/qud_korean

Push 성공 후 알려주시면 다음 작업을 진행하겠습니다!
