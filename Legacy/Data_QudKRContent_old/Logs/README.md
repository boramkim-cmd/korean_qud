# Logs 폴더

이 폴더는 게임 실행 시 자동으로 생성된 로그 파일을 보관합니다.

## 자동 로그 복사

게임 종료 시 `Player.log`가 자동으로 이 폴더에 복사됩니다.

### 파일 형식
- `Player_YYYY-MM-DD_HH-mm-ss.log` - 게임 종료 시 자동 복사
- `Player_YYYY-MM-DD_HH-mm-ss_manual.log` - 수동 복사

### 자동 정리
- 최근 10개 로그 파일만 유지
- 오래된 파일은 자동 삭제

## 로그 확인 방법

### 최신 로그 확인
```bash
ls -lt | head -5
```

### 특정 텍스트 검색
```bash
grep "Qud-KR" Player_*.log
```

### 오류 확인
```bash
grep -i "error\|exception" Player_*.log
```

### 번역 관련 로그만 확인
```bash
grep "발견된 텍스트\|번역 적용" Player_*.log | tail -50
```

## 수동 로그 복사

게임 실행 중 로그를 즉시 복사하려면:
1. 게임 콘솔에서 `LogCopier.CopyLogNow()` 실행
2. 또는 원본 로그 직접 복사:
   ```bash
   cp "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" .
   ```

## 원본 로그 위치
`/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log`

## 주의사항

⚠️ **이 폴더의 로그는 복사본입니다**
- 게임은 원본 로그 파일을 사용
- 이 폴더의 파일을 수정해도 게임에 영향 없음

✅ **디버깅 용도**
- 번역 작업 시 로그 확인 편의성
- 문제 발생 시 로그 보관
- 게임 업데이트 전후 비교
