// ==================================================
// Caves of Qud 한글 조사 처리 시스템 v15
// 수정: Harmony 2.0 업그레이드 및 패치 대상 최적화
// ==================================================

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using XRL;

namespace XRL
{
    using KoreanLocalization.HarmonyPatches;

    // ==================== Harmony 초기화 ====================
    
    [HasGameBasedStaticCache]
    public class JosaHandlerInit
    {
        private static bool _patched = false;

        [GameBasedCacheInit]
        public static void Init()
        {
            DoPatching();
        }

        public static void DoPatching()
        {
            if (_patched) return;

            // 0. [검증] 조사 처리 시뮬레이션 (사용자 요청: 정적/동적 대화 생성 테스트)
            RunJosaSimulation();

            // 1. [진단] 런타임 리플렉션 분석
            InspectType("XRL.World.Conversations.Elements.Choice");
            InspectType("XRL.World.Conversations.Elements.ConversationText");
            InspectType("XRL.World.Conversations.Conversation");
            InspectType("XRL.World.Conversations.GetTextElementEvent");

            // 2. [해결] XML 데이터 직접 변환 (Blueprint Swap)
            try
            {
                SwapBlueprints();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Korean Josa] Swap logic failed: {ex}");
            }

            // 3. [패치] Harmony 패치 적용
            try
            {
                var harmony = new Harmony("com.korean.josa");
                harmony.PatchAll(typeof(JosaHandlerInit).Assembly);
                UnityEngine.Debug.Log("[Korean Josa] Harmony Patches Applied Successfully (v16)!");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[Korean Josa] Patching Failed (v16): " + e.ToString());
            }

            _patched = true;
        }

        private static void RunJosaSimulation()
        {
            UnityEngine.Debug.Log("========================================");
            UnityEngine.Debug.Log("[Korean Josa] Processing Simulation Start");
            
            var testCases = new string[]
            {
                "아르고브(은)는 조파에 있다.",       // 정적 대화 (받침 유)
                "메히(은)는 침묵했다.",             // 정적 대화 (받침 무)
                "{{R|화염(을)를}} 내뿜었다.",       // 동적 대화 (색상 태그 + 받침 유)
                "{{W|단검(이)가}} 떨어졌다.",       // 동적 대화 (색상 태그 + 받침 유)
                "Player(은)는 죽었다.",             // 영문 이름 (받침 무 처리 가정)
                "Level 5(으)로 상승했다.",          // 숫자 (받침 무 처리 가정)
                "물(이)가 차오른다.",               // 일반 명사 (받침 유)
                "바다(이)가 보인다."                // 일반 명사 (받침 무)
            };

            foreach (var node in testCases)
            {
                string result = Korean.ReplaceJosa(node);
                UnityEngine.Debug.Log($"[JosaSim] Input: '{node}' -> Output: '{result}'");
            }
            UnityEngine.Debug.Log("========================================");
        }

        private static void InspectType(string typeName)
        {
            try
            {
                Type type = AccessTools.TypeByName(typeName);
                
                // 이름으로 찾기 실패 시 어셈블리 전체 검색
                if (type == null)
                {
                    UnityEngine.Debug.Log($"[JosaInspector] TypeByName failed for {typeName}. Searching assembly...");
                    foreach (var t in typeof(XRL.World.GameObject).Assembly.GetTypes())
                    {
                        if (t.Name == typeName || t.FullName == typeName)
                        {
                            type = t;
                            UnityEngine.Debug.Log($"[JosaInspector] Found Type: {t.FullName}");
                            break;
                        }
                    }
                }

                if (type != null)
                {
                    UnityEngine.Debug.Log($"[JosaInspector] Inspecting {type.FullName}:");
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        UnityEngine.Debug.Log($"[JosaInspector] Field: {field.Name}, Type: {field.FieldType.Name}");
                    }
                    foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        UnityEngine.Debug.Log($"[JosaInspector] Property: {prop.Name}, Type: {prop.PropertyType.Name}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"[JosaInspector] Type definitely not found: {typeName}");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[JosaInspector] Error inspecting {typeName}: {ex}");
            }
        }

        public static void SwapBlueprints()
        {
            try
            {
                if (_patched) return; // 이미 패치되었으면 중복 실행 방지 (하지만 강제 실행 필요시 제거 고려)

                UnityEngine.Debug.Log("[Korean Josa] Starting Blueprint Swap...");
                Type convType = AccessTools.TypeByName("XRL.World.Conversations.Conversation");
                if (convType == null) return;

                // _Blueprints 필드 접근 -> Property로 변경
                PropertyInfo blueprintsProp = AccessTools.Property(convType, "Blueprints");
                if (blueprintsProp == null)
                {
                    UnityEngine.Debug.LogError("[Korean Josa] Blueprints property not found.");
                    return;
                }

                IDictionary blueprints = blueprintsProp.GetValue(null, null) as IDictionary;
                if (blueprints == null)
                {
                    UnityEngine.Debug.LogError("[Korean Josa] Blueprints dictionary is null.");
                    return;
                }

                int count = 0;
                foreach (DictionaryEntry entry in blueprints)
                {
                    object conv = entry.Value;
                    if (conv == null) continue;

                    // 1. Conversation 자체의 Text 처리 (Elements, Texts 리스트 순회)
                    ProcessConversationElements(conv);

                    // 2. Nodes 순회 및 처리
                    FieldInfo nodesField = AccessTools.Field(convType, "Nodes");
                    if (nodesField != null)
                    {
                        IDictionary nodes = nodesField.GetValue(conv) as IDictionary;
                        if (nodes != null)
                        {
                            foreach (DictionaryEntry nodeEntry in nodes)
                            {
                                ProcessConversationNode(nodeEntry.Value);
                            }
                        }
                    }
                    count++;
                }
                UnityEngine.Debug.Log($"[Korean Josa] Swapped {count} conversations successfully.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Korean Josa] Blueprint Swap Failed: {ex}");
            }
        }

        private static void ProcessConversationElements(object conv)
        {
             // Elements 리스트 (Choice 포함 가능성)
            FieldInfo elementsField = AccessTools.Field(conv.GetType(), "Elements");
            if (elementsField != null)
            {
                IList elements = elementsField.GetValue(conv) as IList;
                if (elements != null)
                {
                    foreach (var elem in elements) ProcessObjectText(elem);
                }
            }
             // Texts 리스트
            FieldInfo textsField = AccessTools.Field(conv.GetType(), "Texts");
            if (textsField != null)
            {
                IList texts = textsField.GetValue(conv) as IList;
                if (texts != null)
                {
                    foreach (var txt in texts) ProcessObjectText(txt);
                }
            }
        }

        private static void ProcessConversationNode(object node)
        {
             // Node 내부의 Text 필드 처리
             ProcessObjectText(node);

             // Node 내부의 Choices 처리 (만약 있다면)
             // Node 타입에 Choices 리스트가 있는지 확인 필요 (리플렉션으로)
             FieldInfo choicesField = AccessTools.Field(node.GetType(), "Choices"); // 이름 추측: Choices, Elements 등
             if (choicesField == null) choicesField = AccessTools.Field(node.GetType(), "Elements");

             if (choicesField != null)
             {
                 IList choices = choicesField.GetValue(node) as IList;
                 if (choices != null)
                 {
                     foreach (var match in choices) ProcessObjectText(match);
                 }
             }
        }

        private static void ProcessObjectText(object target)
        {
            if (target == null) return;
            Type t = target.GetType();

            // Text 필드 찾기 (대소문자 무관)
            FieldInfo textField = AccessTools.Field(t, "Text");
            if (textField == null) textField = AccessTools.Field(t, "text");
            if (textField == null) textField = AccessTools.Field(t, "DisplayText"); // Choice용 추측

            if (textField != null && textField.FieldType == typeof(string))
            {
                string original = textField.GetValue(target) as string;
                if (!string.IsNullOrEmpty(original))
                {
                    string processed = Korean.ReplaceJosa(original);
                    if (original != processed)
                    {
                        textField.SetValue(target, processed);
                        // UnityEngine.Debug.Log($"[JosaSwap] Swapped: {original.Substring(0, Math.Min(20, original.Length))}...");
                    }
                }
            }
        }
    }

    // ==================== 게임 엔진 트리거 ====================

    namespace World.Parts
    {
        /// <summary>
        /// 게임 로더가 모든 IPart 클래스를 스캔하는 것을 이용한 강제 초기화 트리거
        /// </summary>
        public class KoreanJosaTriggerPart : IPart
        {
            public KoreanJosaTriggerPart() { }
            static KoreanJosaTriggerPart()
            {
                JosaHandlerInit.DoPatching();
            }
        }
    }
}

namespace KoreanLocalization.HarmonyPatches
{
    // ==================== 핵심 조사 처리 ====================
    
    public static class Korean
    {
        private const int HANGUL_START = 0xAC00;
        private const int HANGUL_END = 0xD7A3;
        private const int JONGSEONG_COUNT = 28;
        private const int RIEUL_JONGSEONG = 8;
        
        // 지원하는 형식: 
        // 1. 단어(이)가  (기존 방식)
        // 2. 단어이(가)
        // 3. 단어(이/가)
        private static readonly Regex JosaPattern = 
            new Regex(@"([가-힣A-Za-z0-9]+)\s*(\([가-힣/]+\)[가-힣]?|[가-힣]\([가-힣]+\))", RegexOptions.Compiled);
        
        public static string ReplaceJosa(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            try
            {
                if (text.IndexOf('(') == -1)
                    return text;

                return JosaPattern.Replace(text, match =>
                {
                    string word = match.Groups[1].Value;
                    string marker = match.Groups[2].Value;
                    
                    string josa1 = "", josa2 = "";
                    
                    // 마커 분석
                    if (marker.Contains("/")) // 형식 3: (이/가)
                    {
                        var parts = marker.Trim('(', ')').Split('/');
                        if (parts.Length >= 2) { josa1 = parts[0]; josa2 = parts[1]; }
                    }
                    else if (marker.StartsWith("(") && marker.EndsWith(")")) // 형식 1 or 단일: (이), (가)
                    {
                        // 뒤에 글자가 없는 경우(단일) or 내부 분석
                        josa1 = marker.Trim('(', ')');
                        josa2 = ""; // 이후 로직에서 처리
                    }
                    else if (marker.StartsWith("(") && !marker.EndsWith(")")) // 형식 1: (이)가
                    {
                        int closeBracket = marker.IndexOf(')');
                        josa1 = marker.Substring(1, closeBracket - 1);
                        josa2 = marker.Substring(closeBracket + 1);
                    }
                    else if (!marker.StartsWith("(") && marker.Contains("(")) // 형식 2: 이(가)
                    {
                        int openBracket = marker.IndexOf('(');
                        josa1 = marker.Substring(0, openBracket);
                        josa2 = marker.Substring(openBracket + 1).TrimEnd(')');
                    }

                    // 기본값 보정 (josa2가 비어있을 때 josa1에서 유추 시도 - 선택적)
                    if (string.IsNullOrEmpty(josa2))
                    {
                        // 자주 쓰는 패턴 보정
                        if (josa1 == "이") josa2 = "가";
                        else if (josa1 == "은") josa2 = "는";
                        else if (josa1 == "을") josa2 = "를";
                        else if (josa1 == "과") josa2 = "와";
                        else josa2 = ""; // 알 수 없는 경우 유지
                    }

                    bool hasJong = HasJongseong(word);
                    string selectedJosa;
                    
                    // (으)로 특수 처리 (ㄹ 받침 예외)
                    if ((josa1 == "으" && josa2 == "로") || (josa1 == "으로" && josa2 == "로"))
                    {
                        if (HasRieulJongseong(word))
                            selectedJosa = "로";
                        else
                            selectedJosa = hasJong ? "으로" : "로";
                    }
                    else
                    {
                        selectedJosa = hasJong ? josa1 : josa2;
                        // 만약 선택된 조사가 비어있다면 원본 마커 유지 (안전장치)
                        if (string.IsNullOrEmpty(selectedJosa)) return word + marker;
                    }
                    
                    return word + selectedJosa;
                });
            }
            catch
            {
                return text;
            }
        }
        
        private static bool HasJongseong(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT > 0;
        }
        
        private static bool HasRieulJongseong(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT == RIEUL_JONGSEONG;
        }
        
        private static char GetLastKoreanChar(string word)
        {
            for (int i = word.Length - 1; i >= 0; i--)
            {
                char c = word[i];
                if (c >= HANGUL_START && c <= HANGUL_END)
                    return c;
            }
            return '\0';
        }
    }
    
    // ==================== Harmony 패치 ====================
    
    /// <summary>
    /// 메시지 큐 패치 - 로그, 팝업 메시지 등
    /// </summary>
    [HarmonyPatch(typeof(XRL.Messages.MessageQueue))]
    public class MessageQueue_Patch
    {
        private static bool _initialized = false;
        
        [HarmonyPrefix]
        [HarmonyPatch("Add")]
        static void Prefix(ref string Message)
        {
            // 첫 실행 시 로그 출력
            if (!_initialized)
            {
                try
                {
                    UnityEngine.Debug.Log("[Korean Josa] v15 - ConversationUI Patch Added!");
                    UnityEngine.Debug.Log("[Korean Josa] MessageQueue Patch Active");
                    UnityEngine.Debug.Log("[Korean Josa] ConversationUI Patch Active");
                    UnityEngine.Debug.Log("========================================");
                    _initialized = true;
                }
                catch
                {
                    // 로그 실패해도 계속 진행
                }
            }
            
            if (!string.IsNullOrEmpty(Message))
            {
                Message = Korean.ReplaceJosa(Message);
            }
        }
    }
    
    /// <summary>
    /// 대화 텍스트 요소 이벤트 패치 (GetDisplayText 대체 추정)
    /// </summary>
    [HarmonyPatch(typeof(XRL.World.Conversations.GetTextElementEvent))]
    public class GetTextElementEvent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("HandleEvent")]
        static void Postfix(XRL.World.Conversations.GetTextElementEvent __instance)
        {
            // Text 속성이 있다고 가정 (다른 Event들처럼)
            // 만약 StringBuilder라면 ToString() 후 처리
            if (__instance != null)
            {
                // 리플렉션으로 안전하게 Text 속성 접근 (필드명 모름 대비)
                var textField = AccessTools.Field(typeof(XRL.World.Conversations.GetTextElementEvent), "Text") ?? 
                                AccessTools.Field(typeof(XRL.World.Conversations.GetTextElementEvent), "text");
                
                if (textField != null) 
                {
                    object val = textField.GetValue(__instance);
                    if (val is string strVal && !string.IsNullOrEmpty(strVal))
                    {
                        UnityEngine.Debug.Log($"[JosaDebug] GetTextElementEvent Text: {strVal}");
                        string processed = Korean.ReplaceJosa(strVal);
                        if (strVal != processed)
                        {
                             textField.SetValue(__instance, processed);
                             UnityEngine.Debug.Log($"[JosaDebug] GetTextElementEvent Output: {processed}");
                        }
                    }
                    else if (val is System.Text.StringBuilder sbVal && sbVal.Length > 0)
                    {
                        UnityEngine.Debug.Log($"[JosaDebug] GetTextElementEvent StringBuilder: {sbVal}");
                        string original = sbVal.ToString();
                        string processed = Korean.ReplaceJosa(original);
                        if (original != processed)
                        {
                             sbVal.Clear().Append(processed);
                             UnityEngine.Debug.Log($"[JosaDebug] GetTextElementEvent Output: {processed}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 대화 텍스트 준비 이벤트 패치
    /// </summary>
    [HarmonyPatch(typeof(XRL.World.Conversations.PrepareTextLateEvent))]
    public class PrepareTextLateEvent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("HandleEvent")]
        static void Postfix(XRL.World.Conversations.PrepareTextLateEvent __instance)
        {
            if (__instance != null && __instance.Text != null && __instance.Text.Length > 0)
            {
                UnityEngine.Debug.Log($"[JosaDebug] PrepareTextLateEvent Input: {__instance.Text}");
                // PrepareTextLateEvent.Text is a string
                string processed = Korean.ReplaceJosa(__instance.Text);
                if (__instance.Text != processed)
                {
                    __instance.Text = processed;
                    UnityEngine.Debug.Log($"[JosaDebug] PrepareTextLateEvent Output: {processed}");
                }
            }
        }
    }

    /// <summary>
    /// 대화 텍스트 표시 이벤트 패치
    /// </summary>
    [HarmonyPatch(typeof(XRL.World.Conversations.DisplayTextEvent))]
    public class DisplayTextEvent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("HandleEvent")]
        static void Postfix(XRL.World.Conversations.DisplayTextEvent __instance)
        {
            if (__instance != null && __instance.Text != null && __instance.Text.Length > 0)
            {
                UnityEngine.Debug.Log($"[JosaDebug] DisplayTextEvent Input: {__instance.Text}");
                // DisplayTextEvent.Text is a StringBuilder
                string original = __instance.Text.ToString();
                string processed = Korean.ReplaceJosa(original);
                if (original != processed)
                {
                    __instance.Text.Clear().Append(processed);
                    UnityEngine.Debug.Log($"[JosaDebug] DisplayTextEvent Output: {processed}");
                }
            }
        }
    }

    /// <summary>
    /// 아이템/지능지물 툴팁 패치
    /// </summary>
    [HarmonyPatch(typeof(XRL.UI.Look))]
    public class Look_Tooltip_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GenerateTooltipContent", new Type[] { typeof(XRL.World.GameObject) })]
        static void PostfixContent(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = Korean.ReplaceJosa(__result);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GenerateTooltipInformation", new Type[] { typeof(XRL.World.GameObject) })]
        static void PostfixInfo(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = Korean.ReplaceJosa(__result);
            }
        }
    }

    /// <summary>
    /// 팝업 메시지 패치
    /// </summary>
    [HarmonyPatch(typeof(XRL.UI.Popup))]
    public class Popup_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Show", new Type[] { typeof(string) })]
        static void PrefixShow1(ref string Text)
        {
            Text = Korean.ReplaceJosa(Text);
        }

        [HarmonyPrefix]
        [HarmonyPatch("AskYesNo", new Type[] { typeof(string) })]
        static void PrefixAskYesNo(ref string Question)
        {
            Question = Korean.ReplaceJosa(Question);
        }
    }

    /// <summary>
    /// 객체 명칭 및 설명 패치
    /// </summary>
    [HarmonyPatch(typeof(XRL.World.GameObject))]
    public class GameObject_Text_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetDisplayName", new Type[] { typeof(bool), typeof(int), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool?), typeof(bool), typeof(XRL.World.GameObject), typeof(bool), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        static void PostfixDisplayName(ref string __result)
        {
            try
            {
                if (!string.IsNullOrEmpty(__result))
                {
                    __result = Korean.ReplaceJosa(__result);
                }
            }
            catch { }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GetDescription", new Type[] { typeof(bool) })]
        static void PostfixDescription(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = Korean.ReplaceJosa(__result);
            }
        }
    }

    /// <summary>
    /// 돌연변이 및 기술 설명 패치
    /// </summary>
    [HarmonyPatch]
    public class Ability_Description_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(XRL.World.Parts.Mutation.BaseMutation), "GetDescription")]
        static void PostfixMutation(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = Korean.ReplaceJosa(__result);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(XRL.World.Parts.Skill.BaseSkill), "GetDescription")]
        static void PostfixSkill(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = Korean.ReplaceJosa(__result);
            }
        }
    }
}
