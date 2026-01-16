# 번역 작업 워크플로우

이 문서는 Caves of Qud 한글화 프로젝트에 기여하는 번역자를 위한 가이드입니다.

## 📋 시작하기 전에

### 필수 문서 읽기
1. [용어 가이드 (11_LOC_GLOSSARY_GUIDE.md)](11_LOC_GLOSSARY_GUIDE.md) - 용어 시스템 상세 설명
2. [스타일 가이드 (13_LOC_STYLE_GUIDE.md)](13_LOC_STYLE_GUIDE.md) - 번역 스타일 규칙
3. [QA 체크리스트 (14_LOC_QA_CHECKLIST.md)](14_LOC_QA_CHECKLIST.md) - 품질 검증 기준

### 개발 환경 설정
1. Git 설치
2. 저장소 클론: `git clone https://github.com/boramkim-cmd/korean_qud.git`
3. 텍스트 에디터 준비 (VS Code 권장)
4. Caves of Qud 게임 설치 (테스트용)

## 🔄 번역 워크플로우

### 1단계: 작업 선택
1. [GitHub Issues](https://github.com/boramkim-cmd/korean_qud/issues)에서 작업 확인
2. 작업 할당 받기 또는 새 이슈 생성
3. 브랜치 생성: `git checkout -b translate/[작업명]`

### 2단계: 번역 작업

#### 파일 위치
- **UI 및 공통 용어**: `LOCALIZATION/glossary_ui.json`, `glossary_options.json` 등
- **스킬 및 돌연변이**: `LOCALIZATION/glossary_skills.json`, `glossary_mutations.json`
- **대화 및 퀘스트**: `Assets/StreamingAssets/Base/Conversations.xml`, `Quests.xml` (원본 XML 형식 유지)

#### 번역 규칙
1. **용어 일관성**: 용어집 참조
2. **조사 처리**: `{이/가}`, `{을/를}` 등의 플레이스홀더 사용
3. **존댓말/반말**: 캐릭터 성격에 맞게 선택
4. **고유명사**: 원문 유지 또는 용어집 참조

#### 예시 (JSON 활용)
```json
// glossary_ui.json
{
  "ui": {
    "newGame": "새 게임",
    "pickupMessage": "{{item}}{을/를} 주웠습니다."
  }
}
```
게임 내에서 `G._("ui.newGame")` 또는 `[[ui.newGame]]` 형태로 자동 적용됩니다.

### 3단계: 품질 검증 및 기술 체크

#### 기술적 검증 (필수)
PR 생성 전 반드시 통합 도구를 실행하여 기술적 오류를 확인합니다.
```bash
python3 tools/project_tool.py
```
- [ ] 중복 키 또는 구문 오류 없음
- [ ] 모든 항목 번역 완료

#### 게임 내 테스트 및 QA
상세한 검증 방법은 **[14_LOC_QA_CHECKLIST.md](14_LOC_QA_CHECKLIST.md)**를 참조하십시오.
- [ ] Wish(치트) 명령어를 통한 텍스트 출력 확인
- [ ] 플레이어 로그(`Player.log`) 에러 모니터링
- [ ] 조사({이/가}) 및 문체 일관성 확인

### 4단계: 커밋 및 PR

#### 커밋
```bash
git add .
git commit -m "feat: [작업 설명]"
git push origin translate/[작업명]
```

#### 커밋 메시지 규칙
- `feat:` - 새로운 번역 추가
- `fix:` - 번역 오류 수정
- `docs:` - 문서 업데이트
- `style:` - 스타일 가이드 적용

#### Pull Request
1. GitHub에서 PR 생성
2. 템플릿에 따라 설명 작성
3. 리뷰어 지정
4. 피드백 반영

## 📚 번역 우선순위

### Tier 1 (최우선)
- 메인 메뉴
- 캐릭터 생성
- 인벤토리 UI
- 기본 메시지

### Tier 2 (중요)
- 대화 시스템
- 퀘스트 텍스트
- 아이템 설명
- 스킬/능력 설명

### Tier 3 (추가)
- 책 내용
- 역사 텍스트
- 고급 UI

## 🔍 번역 팁

### 게임 용어
- **Mutation**: 돌연변이
- **Cybernetics**: 사이버네틱스
- **Faction**: 세력
- **Quest**: 퀘스트
- **Artifact**: 유물

자세한 용어는 `LOCALIZATION/` 내의 JSON 파일들을 참조하세요.

### 문체
- **UI 텍스트**: 간결하고 명확하게
- **대화 텍스트**: 캐릭터 성격 반영
- **설명 텍스트**: 상세하고 정확하게

### 조사 처리 및 문체 규칙
상세한 한국어 문법 및 스타일 규정은 **[13_LOC_STYLE_GUIDE.md](13_LOC_STYLE_GUIDE.md)**에 정의되어 있습니다.

- **조사 플레이스홀더**: `{이/가}`, `{을/를}`, `{은/는}`, `{와/과}`, `{으로/로}` 등
- **주의**: 플레이스홀더는 반드시 중괄호`{}`를 사용해야 엔진에서 처리됩니다.

## 🐛 문제 해결

### 번역이 게임에 표시되지 않음
1. 모드가 활성화되었는지 확인
2. 게임을 재시작했는지 확인
3. 번역 키가 올바른지 확인

### 조사가 올바르게 처리되지 않음
1. 플레이스홀더 형식 확인: `{이/가}` (중괄호 사용)
2. 엔진 모드(Core_QudKREngine)가 활성화되었는지 확인

### XML 파일 오류
1. 태그가 올바르게 닫혔는지 확인
2. 특수 문자 이스케이프 확인 (`&lt;`, `&gt;`, `&amp;`)

## 📞 도움 요청

- **GitHub Issues**: 버그 보고 및 질문
- **Discord**: [커뮤니티 링크]
- **이메일**: [연락처]

## 🎯 품질 기준

모든 번역은 다음 기준을 충족해야 합니다:
- ✅ 용어집 준수
- ✅ 스타일 가이드 준수
- ✅ 게임 내 테스트 완료
- ✅ 동료 리뷰 통과

## 📝 참고 자료

- [Caves of Qud Wiki](https://cavesofqud.gamepedia.com/)
- [공식 Discord](https://discord.gg/cavesofqud)
- [Steam 커뮤니티](https://steamcommunity.com/app/333640)
