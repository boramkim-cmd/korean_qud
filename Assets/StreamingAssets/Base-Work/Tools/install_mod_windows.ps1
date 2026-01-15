# ================================================
# Caves of Qud 한글화 모드 설치 스크립트 (Windows)
# PowerShell
# ================================================

Write-Host "🎮 Caves of Qud 한글화 모드 설치 시작..." -ForegroundColor Green

# 변수 설정
$WorkDir = "C:\Users\$env:USERNAME\Desktop\무제 폴더\StreamingAssets\Base-Work"
$ModName = "KoreanLocalization"
$GameModsDir = "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud\Mods"

# 1. Mods 폴더 확인/생성
Write-Host ""
Write-Host "📁 게임 Mods 폴더 확인 중..." -ForegroundColor Cyan
if (!(Test-Path $GameModsDir)) {
    Write-Host "   Mods 폴더가 없습니다. 생성 중..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $GameModsDir -Force | Out-Null
    Write-Host "   ✅ Mods 폴더 생성 완료" -ForegroundColor Green
} else {
    Write-Host "   ✅ Mods 폴더 존재" -ForegroundColor Green
}

# 2. 기존 모드 백업
$ModPath = Join-Path $GameModsDir $ModName
if (Test-Path $ModPath) {
    Write-Host ""
    Write-Host "📦 기존 모드 백업 중..." -ForegroundColor Cyan
    $BackupName = "${ModName}_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    $BackupPath = Join-Path $GameModsDir $BackupName
    Move-Item $ModPath $BackupPath
    Write-Host "   ✅ 백업 완료: $BackupName" -ForegroundColor Green
}

# 3. 모드 복사
Write-Host ""
Write-Host "📋 모드 복사 중..." -ForegroundColor Cyan
$SourcePath = Join-Path $WorkDir "Mod\$ModName"
Copy-Item -Recurse $SourcePath $GameModsDir

if ($?) {
    Write-Host "   ✅ 모드 복사 완료" -ForegroundColor Green
} else {
    Write-Host "   ❌ 모드 복사 실패" -ForegroundColor Red
    exit 1
}

# 4. 파일 확인
Write-Host ""
Write-Host "🔍 설치된 파일 확인..." -ForegroundColor Cyan
Write-Host ""
Get-ChildItem $ModPath -Recurse | Format-Table Name, Length

# 5. 완료 메시지
Write-Host ""
Write-Host "✅ 설치 완료!" -ForegroundColor Green
Write-Host ""
Write-Host "다음 단계:" -ForegroundColor Yellow
Write-Host "1. Caves of Qud 실행"
Write-Host "2. Main Menu → Mods"
Write-Host "3. 'Korean Localization' 활성화"
Write-Host "4. 게임 재시작"
Write-Host ""
Write-Host "로그 확인:" -ForegroundColor Yellow
Write-Host "Get-Content `"$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`" -Wait"
Write-Host ""
