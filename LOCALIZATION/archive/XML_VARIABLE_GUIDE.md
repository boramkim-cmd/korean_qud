# XML에서 변수 사용하기

## ❌ 불가능: XML 파일 자체에 변수

XML은 정적 파일이므로 **직접 변수를 사용할 수 없습니다**.

```xml
<!-- ❌ 이렇게는 안 됨 -->
<conversation>
  <text>{{glossary.factions.crystalism}}에 오신 것을 환영합니다</text>
</conversation>
```

---

## ✅ 해결책: 런타임 치환

XML을 로드한 **후** 게임 코드에서 치환합니다.

### 방법 1: 플레이스홀더 사용 (권장 ⭐)

#### 1단계: XML에 플레이스홀더 작성

```xml
<!-- Conversations.xml -->
<conversation>
  <text>{{FACTION_CRYSTALISM}}에 오신 것을 환영합니다</text>
  <text>{{FACTION_MECHANIMISTS}} 신자들이 이곳을 지키고 있습니다</text>
</conversation>
```

#### 2단계: 런타임에 치환

```csharp
// XMLProcessor.cs
using QudKRTranslation.Core;
using System.Text.RegularExpressions;

public static class XMLProcessor
{
    /// <summary>
    /// XML 텍스트에서 플레이스홀더를 용어집 값으로 치환
    /// </summary>
    public static string ProcessXMLText(string xmlText)
    {
        // 용어집 로드
        GlossaryLoader.LoadGlossary();
        
        // {{FACTION_CRYSTALISM}} → "크리스탈리즘"
        xmlText = xmlText.Replace("{{FACTION_CRYSTALISM}}", 
            GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘"));
        
        // {{FACTION_MECHANIMISTS}} → "메카니카신자"
        xmlText = xmlText.Replace("{{FACTION_MECHANIMISTS}}", 
            GlossaryLoader.GetTerm("factions", "mechanimists", "메카니카신자"));
        
        return xmlText;
    }
    
    /// <summary>
    /// 정규식 기반 자동 치환 (고급)
    /// </summary>
    public static string ProcessXMLTextAdvanced(string xmlText)
    {
        GlossaryLoader.LoadGlossary();
        
        // {{CATEGORY_KEY}} 패턴 찾기
        var pattern = @"\{\{([A-Z]+)_([A-Z]+)\}\}";
        
        return Regex.Replace(xmlText, pattern, match =>
        {
            string category = match.Groups[1].Value.ToLower() + "s"; // FACTION → factions
            string key = match.Groups[2].Value.ToLower(); // CRYSTALISM → crystalism
            
            return GlossaryLoader.GetTerm(category, key, match.Value);
        });
    }
}
```

#### 3단계: Harmony 패치로 적용

```csharp
// ConversationPatch.cs
using HarmonyLib;
using XRL.World.Conversations;

[HarmonyPatch(typeof(ConversationNode), "GetDisplayText")]
public static class ConversationTextPatch
{
    static void Postfix(ref string __result)
    {
        // XML에서 로드된 텍스트를 치환
        __result = XMLProcessor.ProcessXMLText(__result);
    }
}
```

---

### 방법 2: 명명 규칙 사용

#### XML 작성

```xml
<conversation>
  <text>@crystalism@에 오신 것을 환영합니다</text>
  <text>@mechanimists@ 신자들이 이곳을 지키고 있습니다</text>
</conversation>
```

#### 런타임 치환

```csharp
public static string ProcessXMLText(string xmlText)
{
    GlossaryLoader.LoadGlossary();
    
    // @term@ 패턴 찾기
    var pattern = @"@([a-z]+)@";
    
    return Regex.Replace(xmlText, pattern, match =>
    {
        string key = match.Groups[1].Value;
        
        // 카테고리별로 검색
        string[] categories = { "factions", "weapons", "items", "ui" };
        
        foreach (var category in categories)
        {
            if (GlossaryLoader.HasTerm(category, key))
            {
                return GlossaryLoader.GetTerm(category, key, match.Value);
            }
        }
        
        return match.Value; // 못 찾으면 원본 유지
    });
}
```

---

## 📋 실전 예시

### XML 파일

```xml
<!-- Conversations.xml -->
<conversations>
  <conversation ID="Merchant_Greeting">
    <node ID="Start">
      <text>환영합니다! {{FACTION_CRYSTALISM}} 상점입니다.</text>
      <text>{{WEAPON_SHORTBOW}}{을/를} 찾으시나요?</text>
      <choice GotoID="Trade">
        <text>{{WEAPON_LONGBLADE}}{을/를} 보여주세요</text>
      </choice>
    </node>
  </conversation>
</conversations>
```

### glossary.json

```json
{
  "factions": {
    "crystalism": "크리스탈리즘"
  },
  "weapons": {
    "shortbow": "짧은 활",
    "longblade": "장검"
  }
}
```

### 처리 코드

```csharp
// ConversationLoader.cs
public class ConversationLoader
{
    public static string LoadAndProcess(string xmlPath)
    {
        // 1. XML 파일 읽기
        string xmlContent = File.ReadAllText(xmlPath);
        
        // 2. 플레이스홀더 치환
        xmlContent = XMLProcessor.ProcessXMLText(xmlContent);
        
        // 결과:
        // "환영합니다! 크리스탈리즘 상점입니다."
        // "짧은 활{을/를} 찾으시나요?"
        // "장검{을/를} 보여주세요"
        
        return xmlContent;
    }
}
```

---

## 🎯 권장 플레이스홀더 규칙

### 명명 규칙

```
{{CATEGORY_KEY}}

예시:
{{FACTION_CRYSTALISM}}    → factions.crystalism
{{WEAPON_SHORTBOW}}       → weapons.shortbow
{{ITEM_WATERSKIN}}        → items.waterskin
{{UI_CONTINUE}}           → ui.continue
```

### 카테고리 매핑

```csharp
private static Dictionary<string, string> CategoryMap = new Dictionary<string, string>()
{
    { "FACTION", "factions" },
    { "WEAPON", "weapons" },
    { "ITEM", "items" },
    { "UI", "ui" },
    { "ATTRIBUTE", "attributes" }
};
```

---

## 🔧 완전한 구현 예시

```csharp
// XMLGlossaryProcessor.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QudKRTranslation.Core;
using UnityEngine;

namespace QudKRTranslation.XML
{
    public static class XMLGlossaryProcessor
    {
        private static Dictionary<string, string> _categoryMap = new Dictionary<string, string>()
        {
            { "FACTION", "factions" },
            { "WEAPON", "weapons" },
            { "ITEM", "items" },
            { "UI", "ui" }
        };
        
        /// <summary>
        /// [[category.key]] 형식의 플레이스홀더 치환 (점 표기법)
        /// </summary>
        public static string Process(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            try
            {
                // 용어집 로드
                GlossaryLoader.LoadGlossary();
                
                // [[category.key]] 패턴 (점 구분자, 소문자)
                var pattern = @"\[\[([a-z]+)\.([a-zA-Z]+)\]\]";
                
                return Regex.Replace(text, pattern, match =>
                {
                    string category = match.Groups[1].Value;  // phrase, faction, item 등
                    string key = match.Groups[2].Value;       // happy, crystalism 등
                    
                    string term = GlossaryLoader.GetTerm(category, key, null);
                    
                    if (!string.IsNullOrEmpty(term))
                    {
                        return term;
                    }
                    
                    // 못 찾으면 경고 및 원본 유지
                    Debug.LogWarning($"[XMLGlossary] 용어를 찾을 수 없음: {match.Value}");
                    return match.Value;
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XMLGlossary] 처리 오류: {ex.Message}");
                return text;
            }
        }
    }
}
```

### Harmony 패치

```csharp
// Patches/ConversationPatch.cs
using HarmonyLib;
using XRL.World.Conversations;
using QudKRTranslation.XML;

[HarmonyPatch(typeof(ConversationNode), "GetDisplayText")]
public static class ConversationDisplayTextPatch
{
    static void Postfix(ref string __result)
    {
        __result = XMLGlossaryProcessor.Process(__result);
    }
}

[HarmonyPatch(typeof(ConversationChoice), "GetDisplayText")]
public static class ConversationChoiceTextPatch
{
    static void Postfix(ref string __result)
    {
        __result = XMLGlossaryProcessor.Process(__result);
    }
}
```

---

## 📊 비교

| 방법 | 장점 | 단점 |
|------|------|------|
| **하드코딩** | 간단 | 용어 변경 시 XML 수정 필요 |
| **플레이스홀더** | 용어 변경 쉬움 | 초기 설정 필요 |
| **런타임 치환** | 중앙 관리 | 성능 약간 저하 |

---

## ✅ 권장 사항

### 개인 사용 (현재)
- **옵션 A**: XML 하드코딩 유지 (간단)
- **옵션 B**: 자주 바뀌는 용어만 플레이스홀더 사용

### 향후 확장
- 플레이스홀더 시스템 구축
- 모든 용어를 JSON으로 중앙 관리

---

## 🚀 빠른 시작

1. **XMLGlossaryProcessor.cs** 생성 (위 코드 복사)
2. **Harmony 패치** 추가
3. **XML에 플레이스홀더** 작성: `{{FACTION_CRYSTALISM}}`
4. **glossary.json** 업데이트
5. **게임 테스트**

---

**요약:**
- ❌ XML 자체에는 변수 불가
- ✅ 플레이스홀더 `{{CATEGORY_KEY}}` 사용
- ✅ 런타임에 Harmony 패치로 치환
- ✅ glossary.json에서 중앙 관리

이제 XML에서도 용어를 변수화할 수 있습니다! 🎉
