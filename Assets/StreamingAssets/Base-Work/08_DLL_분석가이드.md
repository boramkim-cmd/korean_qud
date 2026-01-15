# 게임 DLL 분석 가이드

**목적:** 정확한 클래스 이름 찾기

---

## 🔍 DLL 위치

### Mac
```bash
~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ_Data/Managed/Assembly-CSharp.dll
```

### Windows
```
C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud\CoQ_Data\Managed\Assembly-CSharp.dll
```

---

## 🛠️ 디컴파일 도구

### ILSpy (권장)
```bash
# Mac
brew install --cask ilspy

# 또는 다운로드
https://github.com/icsharpcode/ILSpy/releases
```

### dotPeek (대안)
```
https://www.jetbrains.com/decompiler/
```

---

## 📚 찾아야 할 클래스

### XRL 네임스페이스
```
XRL.Messages.MessageQueue
XRL.World.Parts.Mutation
XRL.World.Quests
XRL.UI.ConversationUI
```

### 대화 관련
```
ConversationNode (추정)
ConversationChoice (추정)
ConversationUI (확인 필요)
```

---

## 🎯 다음 단계

1. ILSpy로 DLL 열기
2. XRL 네임스페이스 탐색
3. 정확한 클래스 이름 확인
4. Harmony 패치 수정

---

**작성일:** 2026-01-13 10:46
