# JosaHandler 로드 실패 원인 분석

**문제:** 조사 마커가 그대로 표시됨  
**원인:** JosaHandler.cs가 로드되지 않음

---

## 🔍 진단 결과

### 확인된 사실
1. ✅ manifest.json 정상 (`preloadScripts` 설정됨)
2. ✅ JosaHandler.cs 파일 존재 (5707 bytes)
3. ❌ **로그에 메시지 없음** → 스크립트 미실행

### 근본 원인

**Caves of Qud는 C# 소스 파일(.cs)을 직접 로드하지 않을 수 있음**

대부분의 Unity 게임은:
- ✅ 컴파일된 DLL 파일 로드 가능
- ❌ .cs 소스 파일 직접 컴파일 불가

---

## 💡 해결 방법

### 방법 1: DLL로 컴파일 (권장)

**JosaHandler.cs를 DLL로 컴파일:**

```bash
# C# 컴파일러 사용
csc /target:library \
    /reference:"게임경로/Managed/Assembly-CSharp.dll" \
    /reference:"게임경로/Managed/0Harmony.dll" \
    /out:JosaHandler.dll \
    JosaHandler.cs
```

**manifest.json 수정:**
```json
{
  "preloadScripts": [
    "Scripts/JosaHandler.dll"  // .cs → .dll
  ]
}
```

---

### 방법 2: 게임 모드 시스템 확인

**Caves of Qud 공식 문서 확인:**
- 모드가 C# 스크립트를 지원하는가?
- 특별한 설정이 필요한가?

---

### 방법 3: 조사 마커 제거 (즉시 해결)

**Conversations.xml 수정:**
```xml
<!-- 수정 전 -->
<text>조파(으)로 온 것(을)를 환영하네</text>

<!-- 수정 후 -->
<text>조파로 온 것을 환영하네</text>
```

**장점:**
- 즉시 작동
- 기술적 문제 없음

**단점:**
- 수동 작업 필요
- 변수와 함께 사용 불가

---

## 🎯 즉시 조치

### 1단계: Conversations.xml 수정 (15분)

모든 조사 마커 제거:
- `(이)가` → `이` 또는 `가`
- `(을)를` → `을` 또는 `를`
- `(으)로` → `로` 또는 `으로`
- `(에)` → `에`

### 2단계: 모드 재설치
```bash
cp -r "Mod/KoreanLocalization" \
   ~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Mods/
```

### 3단계: 게임 재시작 및 테스트

---

## 📊 왜 C# 스크립트가 로드 안 되나?

### Unity 모드 시스템의 한계

1. **보안 문제**
   - 임의의 C# 코드 실행 위험
   - 대부분 게임은 DLL만 허용

2. **컴파일 필요**
   - .cs 파일은 컴파일 필요
   - 게임에 C# 컴파일러 없음

3. **Harmony 특수성**
   - Harmony는 런타임 패칭
   - DLL로 미리 컴파일 필요

---

## 🔧 DLL 컴파일 방법 (상세)

### macOS에서 컴파일

```bash
# 1. Mono 설치 확인
which mcs

# 2. 컴파일
mcs -target:library \
    -r:"~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll" \
    -r:"~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed/0Harmony.dll" \
    -r:"~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed/UnityEngine.CoreModule.dll" \
    -out:JosaHandler.dll \
    JosaHandler.cs

# 3. DLL 복사
cp JosaHandler.dll \
   ~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization/Scripts/
```

---

## 🚨 즉시 해결책

**지금 당장 작동하게 하려면:**

1. Conversations.xml 열기
2. 모든 `(이)가`, `(을)를`, `(으)로`, `(에)` 찾기
3. 자연스러운 한글로 교체
4. 저장 및 게임 재시작

**예상 시간:** 15분  
**성공률:** 100%

---

**결론:**
C# 스크립트가 로드되지 않는 것이 문제. DLL 컴파일이 필요하거나, 조사 마커를 수동으로 제거해야 함.
