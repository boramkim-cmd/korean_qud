# 개발 가이드

> 작업 흐름, 명령어, 규칙

---

## 작업 흐름

```
1. 코드/JSON 수정
       ↓
2. python3 tools/project_tool.py (검증)
       ↓
3. ./deploy.sh (모드 폴더에 배포)
       ↓
4. 게임 실행 → 테스트
       ↓
5. 로그 확인 (에러 없는지)
       ↓
6. git commit (즉시!)
```

---

## 주요 명령어

### 배포
```bash
./deploy.sh                      # 빠른 배포
python3 tools/project_tool.py    # 검증
```

### 로그 확인
```bash
# 실시간
tail -f "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | grep -i "qud-kr"

# 에러만
grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -30
```

### Git
```bash
git status --short
git log --oneline -5
git add <files> && git commit -m "type: 설명"
```

### 게임 내 디버그 (Ctrl+W → Wish)
```
kr:reload       # JSON 리로드
kr:stats        # 번역 통계
kr:check <id>   # 특정 블루프린트 확인
kr:untranslated # 미번역 목록
kr:clearcache   # 캐시 클리어
```

---

## Git 커밋 규칙

### 자주 커밋하기
- **매 작업 단위 완료 시** 즉시 커밋
- "나중에 한꺼번에" 금지
- 대화 세션 종료 전 반드시 커밋

### 커밋 메시지 형식
```bash
fix: 버그 수정
feat: 새 기능
docs: 문서 수정
chore: 기타 작업
refactor: 리팩토링
```

### 예시
```bash
git commit -m "fix: Dictionary 중복 키 버그 수정"
git commit -m "feat: 새 접두사 추가 (frozen, burnt)"
git commit -m "docs: skills.md 작업 가이드 추가"
```

---

## 코드 작성 규칙

### 1. Dictionary 중복 키 금지
```bash
# 추가 전 반드시 확인!
grep -n "새키이름" ObjectTranslator.cs
```

### 2. 번역 태그 보존
```
{{tag}}  - 게임 변수, 절대 번역 금지
%var%    - 동적 값, 절대 번역 금지
```

### 3. 스코프 균형 유지
```csharp
// Prefix에서 Push
ScopeManager.PushScope(...);
_scopePushed = true;

// Postfix에서 Pop (반드시!)
if (_scopePushed) {
    ScopeManager.PopScope();
    _scopePushed = false;
}
```

### 4. Harmony 패치 패턴
```csharp
// 데이터 필드 직접 수정 금지!
❌ genotype.DisplayName = "한글";  // 데이터 오염

// UI 표시 시점에 Postfix로 수정
✅ [HarmonyPostfix]
   void Show_Postfix(ref string __result) {
       __result = "한글";  // UI만 변경
   }
```

---

## 새 번역 추가 방법

### 접두사 추가 (ObjectTranslator.cs)
```bash
# 1. 중복 확인
grep -n "새접두사" 02_20_00_ObjectTranslator.cs

# 2. 적절한 딕셔너리에 추가
_materialPrefixes      # 재료: bronze, steel
_qualityPrefixes       # 품질: flawless, crude
_processingPrefixes    # 가공: raw, dried
_descriptivePrefixes   # 기타: broken, luminous

# 3. 복합 접두사는 먼저! (길이순 정렬)
{ "folded carbide", "접힌 카바이드" },  // 먼저
{ "carbide", "카바이드" },              // 나중

# 4. 테스트
./deploy.sh
# 게임 실행 → 로그 확인
```

### 오브젝트 추가 (JSON)
```json
// LOCALIZATION/OBJECTS/items/weapons.json
{
  "NewWeapon": {
    "names": {
      "new weapon": "새 무기",
      "New Weapon": "새 무기"
    },
    "description": "A new weapon.",
    "description_ko": "새로운 무기."
  }
}
```

### UI 패치 추가
```csharp
// Scripts/02_Patches/10_UI/02_10_XX_NewScreen.cs
[HarmonyPatch(typeof(NewScreen), "Show")]
static void Prefix() {
    ScopeManager.Push("newscreen", "common");
}

[HarmonyPatch(typeof(NewScreen), "Hide")]
static void Postfix() {
    ScopeManager.Pop();
}
```

---

## 트러블슈팅

### 번역이 전혀 안 됨
1. 로그에서 `TypeInitializationException` 확인
2. 원인 90%: Dictionary 중복 키
3. 해결: `grep -n "키" 파일.cs`로 중복 찾아 삭제

### 특정 아이템만 번역 안 됨
1. JSON에 해당 아이템 있는지 확인
2. 키가 Blueprint ID와 일치하는지 확인 (대소문자!)
3. `names` 필드에 다양한 변형 포함했는지 확인

### UI 번역 안 됨
1. 해당 화면에서 `ScopeManager.Push()` 호출하는지 확인
2. JSON의 카테고리가 맞는지 확인
3. 대소문자 일치 확인

### 게임 크래시
1. 로그에서 에러 메시지 확인
2. 최근 변경된 패치 비활성화 후 테스트
3. null 체크 누락 여부 확인
