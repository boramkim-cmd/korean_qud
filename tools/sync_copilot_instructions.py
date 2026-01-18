#!/usr/bin/env python3
"""
Sync Critical Errors from ERROR_LOG.md to copilot-instructions.md

This script extracts Critical/High severity errors from the error log
and updates the copilot-instructions.md file automatically.

Usage:
    python3 tools/sync_copilot_instructions.py

Integrated into: tools/sync-and-deploy.sh
"""

import re
import os
from datetime import datetime
from pathlib import Path

# Paths
PROJECT_ROOT = Path(__file__).parent.parent
ERROR_LOG_PATH = PROJECT_ROOT / "Docs" / "05_ERROR_LOG.md"
INSTRUCTIONS_PATH = PROJECT_ROOT / ".github" / "copilot-instructions.md"

# Markers in instructions file
LAYER3_START = "# LAYER 3: 과거 Critical 에러 요약 (반복 금지!)"
LAYER3_END = "# LAYER 4: 핵심 파일 경로"


def extract_critical_errors(error_log_content: str) -> list[dict]:
    """Extract Critical and High severity errors from error log."""
    errors = []
    
    # Split by error sections (## ERR-XXX:)
    sections = re.split(r'\n(?=## ERR-\d+:)', error_log_content)
    
    for section in sections:
        # Match header: ## ERR-XXX: Title
        header_match = re.match(r'## (ERR-\d+): (.+?)(?:\n|$)', section)
        if not header_match:
            continue
        
        err_id = header_match.group(1)
        title = header_match.group(2).strip()
        
        # Check severity - look for Critical or High
        severity_match = re.search(r'\*\*심각도\*\*\s*\|\s*([^\|]+)\|', section)
        if not severity_match:
            continue
        
        severity = severity_match.group(1).strip()
        if 'Critical' not in severity and 'High' not in severity:
            continue
        
        # Extract symptom from ### 증상 section
        symptom_match = re.search(r'### 증상\s*\n(?:.*?\n)*?1\.\s*([^\n]+)', section)
        symptom = symptom_match.group(1)[:35] if symptom_match else title[:35]
        
        # Extract cause - look for ### 원인 분석 or 근본 원인
        cause_match = re.search(r'### 원인 분석\s*\n(?:.*?\n)*?(?:\d+\.\s*)?([^\n]+)', section)
        if not cause_match:
            cause_match = re.search(r'\*\*근본 원인\*\*[:\s]*([^\n]+)', section)
        cause = cause_match.group(1)[:40] if cause_match else "Unknown"
        
        # Extract solution - look for ✅ 최종 해결 or 해결 방법
        solution_match = re.search(r'### ✅ 최종 해결\s*\n(?:.*?\n)*?(?:\d+\.\s*)?(?:\*\*[^*]+\*\*:\s*)?([^\n]+)', section)
        if not solution_match:
            solution_match = re.search(r'\*\*해결 방법\*\*[:\s]*([^\n]+)', section)
        solution = solution_match.group(1)[:40] if solution_match else "See error log"
        
        # Clean up extracted text
        symptom = re.sub(r'\*\*[^*]+\*\*:?\s*', '', symptom).strip()
        cause = re.sub(r'\*\*[^*]+\*\*:?\s*', '', cause).strip()
        solution = re.sub(r'\*\*[^*]+\*\*:?\s*', '', solution).strip()
        
        # Truncate for table readability
        symptom = symptom[:25] + "..." if len(symptom) > 28 else symptom
        cause = cause[:25] + "..." if len(cause) > 28 else cause
        solution = solution[:25] + "..." if len(solution) > 28 else solution
        
        errors.append({
            'id': err_id,
            'symptom': symptom,
            'cause': cause,
            'solution': solution
        })
    
    return errors


def generate_error_table(errors: list[dict]) -> str:
    """Generate markdown table from errors."""
    lines = [
        "| ID | 증상 | 원인 | 해결 |",
        "|----|------|------|------|"
    ]
    
    for err in errors:
        lines.append(f"| {err['id']} | {err['symptom']} | {err['cause']} | {err['solution']} |")
    
    return "\n".join(lines)


def update_instructions(instructions_content: str, error_table: str) -> str:
    """Update LAYER 3 section with new error table."""
    # Find LAYER 3 section
    start_idx = instructions_content.find(LAYER3_START)
    end_idx = instructions_content.find(LAYER3_END)
    
    if start_idx == -1 or end_idx == -1:
        print("Warning: Could not find LAYER 3 markers in instructions file")
        return instructions_content
    
    # Build new LAYER 3 content
    new_layer3 = f"""{LAYER3_START}
################################################################################

{error_table}

# 상세 내용: Docs/05_ERROR_LOG.md 참조

################################################################################
"""
    
    # Replace section
    before = instructions_content[:start_idx]
    after = instructions_content[end_idx:]
    
    return before + new_layer3 + after


def update_version(content: str) -> str:
    """Update version number and date."""
    today = datetime.now().strftime("%Y-%m-%d")
    
    # Update version line
    content = re.sub(
        r'# Version: [\d.]+ \| Updated: \d{4}-\d{2}-\d{2}',
        f'# Version: 2.1 | Updated: {today}',
        content
    )
    
    return content


def main():
    """Main sync function."""
    print("=== Syncing Copilot Instructions ===")
    
    # Check files exist
    if not ERROR_LOG_PATH.exists():
        print(f"Error: {ERROR_LOG_PATH} not found")
        return 1
    
    if not INSTRUCTIONS_PATH.exists():
        print(f"Error: {INSTRUCTIONS_PATH} not found")
        return 1
    
    # Read files
    error_log = ERROR_LOG_PATH.read_text(encoding='utf-8')
    instructions = INSTRUCTIONS_PATH.read_text(encoding='utf-8')
    
    # Extract errors
    errors = extract_critical_errors(error_log)
    print(f"Found {len(errors)} Critical/High errors")
    
    if not errors:
        print("No critical errors found, keeping existing table")
        return 0
    
    # Generate table
    error_table = generate_error_table(errors)
    
    # Update instructions
    new_instructions = update_instructions(instructions, error_table)
    new_instructions = update_version(new_instructions)
    
    # Write back
    INSTRUCTIONS_PATH.write_text(new_instructions, encoding='utf-8')
    print(f"Updated: {INSTRUCTIONS_PATH}")
    
    return 0


if __name__ == "__main__":
    exit(main())
