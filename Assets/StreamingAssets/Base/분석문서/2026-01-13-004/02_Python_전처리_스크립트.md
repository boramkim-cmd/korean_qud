# 02. Python 전처리 스크립트 가이드

**난이도:** ⭐⭐  
**예상 시간:** 1-2일  
**필요 기술:** Python 기초

---

## 🎯 목적

C# 모드 개발이 어려운 경우, **번역 파일을 미리 처리**하는 Python 스크립트

### 장점
- ✅ 구현 간단
- ✅ Python만 알면 됨
- ✅ 즉시 사용 가능

### 단점
- ❌ 번역 파일마다 실행 필요
- ❌ 동적 생성 텍스트 처리 어려움
- ❌ 게임 업데이트 시 재실행 필요

---

## 💻 완전한 스크립트

### josa_processor.py

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import re
import sys
import os
from pathlib import Path

class KoreanJosaProcessor:
    """한글 조사 자동 처리 클래스"""
    
    def __init__(self):
        self.cache = {}
    
    def has_jongseong(self, word):
        """받침 확인"""
        if not word:
            return False
        
        # 특수문자 제거 후 마지막 한글 찾기
        last_char = self._get_last_korean_char(word)
        
        if not last_char:
            return False
        
        # 한글 유니코드 범위 확인
        if not ('가' <= last_char <= '힣'):
            return False
        
        # 종성(받침) 계산
        jongseong = (ord(last_char) - 0xAC00) % 28
        
        return jongseong != 0
    
    def _get_last_korean_char(self, word):
        """마지막 한글 글자 추출"""
        for char in reversed(word):
            if '가' <= char <= '힣':
                return char
        return None
    
    def has_rieul_jongseong(self, word):
        """ㄹ 받침 확인"""
        if not word:
            return False
        
        last_char = self._get_last_korean_char(word)
        
        if not last_char or not ('가' <= last_char <= '힣'):
            return False
        
        jongseong = (ord(last_char) - 0xAC00) % 28
        
        # ㄹ 받침은 8번
        return jongseong == 8
    
    def get_josa(self, word, josa_type):
        """조사 선택"""
        if not word:
            # 기본값 (받침 없음으로 가정)
            defaults = {
                'i_ga': '가',
                'eul_reul': '를',
                'eun_neun': '는',
                'euro_ro': '로',
                'a_ya': '야'
            }
            return defaults.get(josa_type, '')
        
        has_jong = self.has_jongseong(word)
        
        if josa_type == 'i_ga':
            return '이' if has_jong else '가'
        elif josa_type == 'eul_reul':
            return '을' if has_jong else '를'
        elif josa_type == 'eun_neun':
            return '은' if has_jong else '는'
        elif josa_type == 'euro_ro':
            # ㄹ 받침 특수 처리
            if self.has_rieul_jongseong(word):
                return '로'
            return '으로' if has_jong else '로'
        elif josa_type == 'a_ya':
            return '아' if has_jong else '야'
        else:
            return ''
    
    def process_text(self, text, variables=None):
        """텍스트 내 조사 태그 처리"""
        if not text:
            return text
        
        # 캐시 확인
        cache_key = (text, str(variables))
        if cache_key in self.cache:
            return self.cache[cache_key]
        
        # 패턴: <변수><josa_type>
        pattern = r'<([^>]+)><josa_(\w+)>'
        
        def replacer(match):
            var_name = match.group(1)
            josa_type = match.group(2)
            
            # 변수 값 가져오기
            if variables and var_name in variables:
                value = variables[var_name]
            else:
                # 변수 값을 모르면 원본 유지
                return match.group(0)
            
            # 조사 선택
            josa = self.get_josa(value, josa_type)
            
            return value + josa
        
        result = re.sub(pattern, replacer, text)
        
        # 캐시 저장
        self.cache[cache_key] = result
        
        return result
    
    def process_xml_file(self, input_path, output_path=None):
        """XML 파일 처리"""
        if output_path is None:
            output_path = input_path.replace('.xml', '_processed.xml')
        
        print(f"Processing: {input_path}")
        
        with open(input_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # 간단한 변수 추출 (실제로는 더 복잡)
        # 여기서는 고정값으로 테스트
        variables = {
            'item.name': '검',
            'player.name': '철수',
            'entity.name': '바라스럼'
        }
        
        # 처리
        processed = self.process_text(content, variables)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(processed)
        
        print(f"Saved: {output_path}")
        
        return output_path


def main():
    """메인 함수"""
    if len(sys.argv) < 2:
        print("Usage: python josa_processor.py <xml_file>")
        print("Example: python josa_processor.py Conversations.xml")
        return
    
    input_file = sys.argv[1]
    
    if not os.path.exists(input_file):
        print(f"Error: File not found: {input_file}")
        return
    
    processor = KoreanJosaProcessor()
    processor.process_xml_file(input_file)
    
    print("Done!")


if __name__ == '__main__':
    main()
```

---

## 🚀 사용 방법

### 1. 기본 사용

```bash
# 단일 파일 처리
python josa_processor.py Conversations.xml

# 결과: Conversations_processed.xml 생성
```

### 2. 배치 처리

```python
# batch_process.py
from josa_processor import KoreanJosaProcessor
from pathlib import Path

processor = KoreanJosaProcessor()

# 모든 XML 파일 처리
for xml_file in Path('.').glob('*.xml'):
    processor.process_xml_file(str(xml_file))
```

### 3. 테스트

```python
# test_josa.py
from josa_processor import KoreanJosaProcessor

processor = KoreanJosaProcessor()

# 테스트 케이스
test_cases = [
    ("검", "eul_reul", "을"),
    ("사과", "eul_reul", "를"),
    ("책", "i_ga", "이"),
    ("연필", "i_ga", "가"),
    ("서울", "euro_ro", "로"),  # ㄹ 받침
    ("집", "euro_ro", "으로"),
]

for word, josa_type, expected in test_cases:
    result = processor.get_josa(word, josa_type)
    status = "✅" if result == expected else "❌"
    print(f"{status} {word} + {josa_type} = {result} (expected: {expected})")
```

---

## 📝 실전 예시

### 입력 (Conversations.xml)

```xml
<conversation ID="Test">
  <node ID="Start">
    <text><player.name><josa_i_ga> <item.name><josa_eul_reul> 발견했다</text>
  </node>
</conversation>
```

### 출력 (Conversations_processed.xml)

```xml
<conversation ID="Test">
  <node ID="Start">
    <text>철수가 검을 발견했다</text>
  </node>
</conversation>
```

---

## ⚠️ 한계점

### 1. 동적 변수 처리 불가

**문제:**
```xml
<text><spice.adjectives.!random><josa_eun_neun> 프리즘</text>
```

**해결:**
- 게임 실행 시점에만 값이 결정됨
- 전처리로는 불가능
- C# 모드 필요

### 2. 모든 경우의 수 처리

**대안:**
```python
# 모든 가능한 값에 대해 처리
adjectives = ["빛나는", "어두운", "투명한"]

for adj in adjectives:
    variables = {'spice.adjectives.!random': adj}
    result = processor.process_text(text, variables)
    # 여러 버전 생성...
```

---

**작성일:** 2026-01-13  
**난이도:** ⭐⭐  
**예상 완료:** 1-2일
