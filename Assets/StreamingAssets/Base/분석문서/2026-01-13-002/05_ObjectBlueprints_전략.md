# 05. ObjectBlueprints 번역 전략

**우선순위:** 🟡 높은 우선순위  
**파일 크기:** ~500 KB (14개 파일)  
**예상 작업 기간:** 20-30일  
**난이도:** ⭐⭐⭐⭐

---

## 파일 개요
- **경로:** `/StreamingAssets/Base/ObjectBlueprints/`
- **내용:** 모든 아이템, 무기, 방어구, 소비품
- **중요도:** 플레이어가 가장 자주 보는 텍스트

---

## 디렉토리 구조

```
ObjectBlueprints/
├── Armor.xml           (방어구)
├── BaseMelee.xml       (근접 무기 기본)
├── BaseRanged.xml      (원거리 무기 기본)
├── Books.xml           (책)
├── Consumables.xml     (소비품)
├── Cybernetics.xml     (사이버네틱스)
├── Furniture.xml       (가구)
├── Melee.xml           (근접 무기)
├── Misc.xml            (기타)
├── Mods.xml            (모드)
├── Ranged.xml          (원거리 무기)
├── Shields.xml         (방패)
├── Tinkering.xml       (땜질)
└── Tools.xml           (도구)
```

---

## 구조 분석

```xml
<object Name="ObjectID" DisplayName="아이템 이름">
  <part Name="Description" Short="짧은 설명" Long="긴 설명"/>
  <part Name="Commerce" Value="100"/>
</object>
```

---

## 번역 전략

### 우선순위
1. **Consumables.xml** (소비품 - 자주 사용)
2. **Melee.xml, Ranged.xml** (무기)
3. **Armor.xml** (방어구)
4. **Books.xml** (책)
5. **나머지 파일**

### 일일 목표
- 하루 50-100개 아이템
- 품질 우선

---

## 실전 예시

**원문:**
```xml
<object Name="Sword" DisplayName="sword">
  <part Name="Description" Short="A simple iron sword." Long="A well-crafted blade of iron."/>
  <part Name="Commerce" Value="50"/>
</object>
```

**번역:**
```xml
<object Name="Sword" DisplayName="검">
  <part Name="Description" Short="단순한 철제 검." Long="잘 만들어진 철제 검날."/>
  <part Name="Commerce" Value="50"/>
</object>
```

---

## 주의사항
- Name (ID) 변경 금지
- Value 값 유지
- 아이템 설명은 간결하게

---

**예상 완료:** 20-30일
