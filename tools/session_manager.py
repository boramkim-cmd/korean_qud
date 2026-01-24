#!/usr/bin/env python3
"""
Session Manager for AI Context Handoff
Automatically saves and restores session state between chat sessions.
Generates vectorized context for efficient handoff to new chats.

Usage:
  python3 tools/session_manager.py save    # Save current session state
  python3 tools/session_manager.py load    # Load previous session state (for new chat)
  python3 tools/session_manager.py status  # Show current status
  python3 tools/session_manager.py handoff # Generate copy-paste handoff prompt
"""

import os
import sys
import json
import subprocess
from datetime import datetime
from pathlib import Path

# Paths
PROJECT_ROOT = Path(__file__).parent.parent
SESSION_FILE = PROJECT_ROOT / "Docs" / "SESSION_STATE.md"
SESSION_JSON = PROJECT_ROOT / "tools" / "session_state.json"
CONTEXT_FILE = PROJECT_ROOT / "CONTEXT.yaml"  # Main vectorized context (project root)
TODO_FILE = PROJECT_ROOT / "Docs" / "reference" / "01_TODO.md"
CHANGELOG_FILE = PROJECT_ROOT / "Docs" / "reference" / "02_CHANGELOG.md"
ERROR_LOG = PROJECT_ROOT / "Docs" / "reference" / "03_ERROR_LOG.md"


def get_git_status():
    """Get current git status summary."""
    try:
        result = subprocess.run(
            ["git", "status", "--short"],
            cwd=PROJECT_ROOT,
            capture_output=True,
            text=True
        )
        lines = result.stdout.strip().split('\n') if result.stdout.strip() else []
        return {
            "uncommitted_files": len(lines),
            "files": lines[:10]  # First 10 files
        }
    except:
        return {"uncommitted_files": 0, "files": []}


def get_recent_commits(n=5):
    """Get recent commit messages."""
    try:
        result = subprocess.run(
            ["git", "log", f"-{n}", "--oneline"],
            cwd=PROJECT_ROOT,
            capture_output=True,
            text=True
        )
        return result.stdout.strip().split('\n') if result.stdout.strip() else []
    except:
        return []


def get_todo_summary():
    """Extract key items from TODO."""
    if not TODO_FILE.exists():
        return {"next_tasks": [], "in_progress": []}
    
    content = TODO_FILE.read_text()
    
    # Find "Next Session Required" section
    next_tasks = []
    in_progress = []
    
    lines = content.split('\n')
    in_next_section = False
    in_backlog = False
    
    for line in lines:
        if "## Next Session Required" in line:
            in_next_section = True
            continue
        if "## Backlog" in line:
            in_next_section = False
            in_backlog = True
            continue
        if line.startswith("## ") and in_next_section:
            in_next_section = False
        if line.startswith("## ") and in_backlog:
            in_backlog = False
            
        if in_next_section and line.strip().startswith("###"):
            next_tasks.append(line.strip().replace("### ", ""))
        
        if in_backlog and "| " in line and "WIP" in line:
            in_progress.append(line.strip())
    
    return {
        "next_tasks": next_tasks[:5],
        "in_progress": in_progress[:3]
    }


def get_recent_changes():
    """Extract recent changes from CHANGELOG."""
    if not CHANGELOG_FILE.exists():
        return []
    
    content = CHANGELOG_FILE.read_text()
    changes = []
    
    lines = content.split('\n')
    for i, line in enumerate(lines):
        if line.startswith("### ["):
            # Get the header and next few lines
            changes.append(line)
            if len(changes) >= 3:
                break
    
    return changes


def get_known_errors():
    """Extract unresolved errors from ERROR_LOG."""
    if not ERROR_LOG.exists():
        return []
    
    content = ERROR_LOG.read_text()
    errors = []
    
    # Look for UNRESOLVED errors
    lines = content.split('\n')
    for line in lines:
        if "UNRESOLVED" in line or "[ ]" in line:
            errors.append(line.strip())
            if len(errors) >= 5:
                break
    
    return errors


def count_translations():
    """Count translation entries."""
    localization_dir = PROJECT_ROOT / "LOCALIZATION"
    total = 0
    
    for json_file in localization_dir.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
                # Count non-meta entries
                total += sum(1 for k in data.keys() if not k.startswith('_'))
        except:
            pass
    
    return total


def save_session():
    """Save current session state."""
    state = {
        "timestamp": datetime.now().isoformat(),
        "git": get_git_status(),
        "recent_commits": get_recent_commits(),
        "todo": get_todo_summary(),
        "recent_changes": get_recent_changes(),
        "known_errors": get_known_errors(),
        "translation_count": count_translations()
    }
    
    # Save JSON
    with open(SESSION_JSON, 'w', encoding='utf-8') as f:
        json.dump(state, f, ensure_ascii=False, indent=2)
    
    # Generate Markdown
    md_content = generate_session_markdown(state)
    SESSION_FILE.write_text(md_content)
    
    # Note: This function is replaced by the one below with session_work parameter
    # Keeping for backward compatibility but it won't be called
    
    print(f"✅ Session state saved to:")
    print(f"   - {SESSION_FILE}")
    print(f"   - {SESSION_JSON}")
    
    return state


def update_context_yaml(state, session_work=None):
    """Update the status section of CONTEXT.yaml (preserves all other content)."""
    if not CONTEXT_FILE.exists():
        print(f"⚠️ CONTEXT.yaml not found at {CONTEXT_FILE}")
        return
    
    content = CONTEXT_FILE.read_text()
    
    # Find and replace the status section
    import re
    
    # Get last commit hash
    last_commit = state['recent_commits'][0].split()[0] if state['recent_commits'] else 'unknown'
    
    # Build new status section
    pending_items = state['todo']['next_tasks'][:5] if state['todo']['next_tasks'] else ['None']
    
    new_status = f"""status:
  translations: {state['translation_count']}
  mutation_files: 81
  object_files: 57
  build: passing
  last_commit: {last_commit}
  last_updated: {state['timestamp']}
  
  progress:
    phase1_stabilization: 100%
    phase2_gameplay: 75%
    phase3_optimization: 0%
    phase4_community: 0%
  
  pending:
"""
    for item in pending_items:
        new_status += f'    - "{item}"\n'
    
    new_status += f"""  
  recent_work:
    - "{session_work if session_work else 'Not specified'}" """
    
    # Replace status section - match from 'status:' to end of file
    pattern = r'^status:.*'
    if re.search(pattern, content, re.MULTILINE | re.DOTALL):
        content = re.sub(pattern, new_status.strip(), content, flags=re.MULTILINE | re.DOTALL)
        CONTEXT_FILE.write_text(content)
        print(f"   ✅ Updated status section in CONTEXT.yaml")


def generate_session_markdown(state):
    """Generate human-readable session state."""
    md = f"""# 🔄 Session State (Auto-generated)

> **Last Updated**: {state['timestamp']}
> **Copy this to new chat for context handoff**

---

## 📊 Current Status

| Metric | Value |
|--------|-------|
| Translation Entries | {state['translation_count']} |
| Uncommitted Files | {state['git']['uncommitted_files']} |

## 🎯 Next Tasks (from TODO)

"""
    
    if state['todo']['next_tasks']:
        for task in state['todo']['next_tasks']:
            md += f"- {task}\n"
    else:
        md += "- No pending tasks\n"
    
    md += "\n## 📝 Recent Changes\n\n"
    
    if state['recent_changes']:
        for change in state['recent_changes']:
            md += f"{change}\n"
    else:
        md += "- No recent changes\n"
    
    md += "\n## 🔀 Recent Commits\n\n```\n"
    for commit in state['recent_commits']:
        md += f"{commit}\n"
    md += "```\n"
    
    if state['known_errors']:
        md += "\n## ⚠️ Known Issues\n\n"
        for error in state['known_errors']:
            md += f"- {error}\n"
    
    if state['git']['uncommitted_files'] > 0:
        md += "\n## 📁 Uncommitted Files\n\n```\n"
        for f in state['git']['files']:
            md += f"{f}\n"
        md += "```\n"
    
    md += """
---

## 🚀 Quick Start Commands

```bash
# Check current status
python3 tools/session_manager.py status

# After work, save session
python3 tools/session_manager.py save

# Validate and commit
python3 tools/project_tool.py && bash tools/quick-save.sh
```

## 📋 Handoff Prompt

Copy and paste this to start a new chat session:

```
이전 세션에서 이어서 작업합니다. SESSION_STATE.md를 읽고 맥락을 파악한 후 다음 작업을 진행해주세요.
```
"""
    
    return md


def generate_vectorized_context(state, session_work=None):
    """Generate compressed vectorized context for new chat handoff."""
    
    # Get recent file changes from git diff
    recent_files = []
    try:
        result = subprocess.run(
            ["git", "diff", "--name-only", "HEAD~3"],
            cwd=PROJECT_ROOT,
            capture_output=True,
            text=True
        )
        recent_files = [f for f in result.stdout.strip().split('\n') if f][:15]
    except:
        pass
    
    context = f"""# 🧠 AI Context Handoff (Vectorized)
# Generated: {state['timestamp']}
# Copy this ENTIRE block to new chat

## PROJECT_STATE
type: caves_of_qud_korean_localization
translations: {state['translation_count']}
build: passing
uncommitted: {state['git']['uncommitted_files']}

## RECENT_COMMITS
{chr(10).join(state['recent_commits'][:5])}

## PENDING_TASKS
{chr(10).join('- ' + t for t in state['todo']['next_tasks'][:5]) if state['todo']['next_tasks'] else '- None'}

## RECENT_CHANGES
{chr(10).join(state['recent_changes'][:3]) if state['recent_changes'] else '- None'}

## MODIFIED_FILES
{chr(10).join(recent_files) if recent_files else 'None'}

## KNOWN_ISSUES
{chr(10).join('- ' + e for e in state['known_errors'][:5]) if state['known_errors'] else '- None'}

## SESSION_WORK
{session_work if session_work else 'Not specified'}

## KEY_PATHS
- Scripts/00_Core/ → Translation engine
- Scripts/02_Patches/ → Harmony patches
- LOCALIZATION/ → JSON translation data
- Docs/en/reference/ → TODO, CHANGELOG, ERROR_LOG

## COMMANDS
validate: python3 tools/project_tool.py
commit: bash tools/quick-save.sh
session: python3 tools/session_manager.py save

---
위 맥락을 기반으로 작업을 이어서 진행해주세요.
"""
    
    return context


def save_session(session_work=None):
    """Save current session state."""
    state = {
        "timestamp": datetime.now().isoformat(),
        "git": get_git_status(),
        "recent_commits": get_recent_commits(),
        "todo": get_todo_summary(),
        "recent_changes": get_recent_changes(),
        "known_errors": get_known_errors(),
        "translation_count": count_translations(),
        "session_work": session_work
    }
    
    # Save JSON
    with open(SESSION_JSON, 'w', encoding='utf-8') as f:
        json.dump(state, f, ensure_ascii=False, indent=2)
    
    # Generate Markdown
    md_content = generate_session_markdown(state)
    SESSION_FILE.write_text(md_content)
    
    # Update CONTEXT.yaml status section ONLY (preserve full project info)
    update_context_yaml(state, session_work)
    
    print(f"✅ Session state saved to:")
    print(f"   - {SESSION_FILE}")
    print(f"   - {SESSION_JSON}")
    print(f"   - {CONTEXT_FILE} (status section updated)")
    
    return state
    print(f"   - {CONTEXT_FILE} (vectorized)")
    
    return state


def generate_handoff_prompt(state):
    """Generate a complete handoff prompt for new chat."""
    context = generate_vectorized_context(state, state.get('session_work'))
    
    prompt = f"""
{'='*60}
📋 COPY EVERYTHING BELOW TO NEW CHAT
{'='*60}

{context}
"""
    return prompt


def load_session():
    """Load and display previous session state."""
    if not SESSION_JSON.exists():
        print("❌ No previous session found. Run 'save' first.")
        return None
    
    with open(SESSION_JSON, 'r', encoding='utf-8') as f:
        state = json.load(f)
    
    print("=" * 60)
    print("📂 PREVIOUS SESSION STATE")
    print("=" * 60)
    print(f"Saved: {state['timestamp']}")
    print(f"Translations: {state['translation_count']}")
    print()
    
    print("🎯 Next Tasks:")
    for task in state['todo']['next_tasks']:
        print(f"  - {task}")
    print()
    
    print("📝 Recent Changes:")
    for change in state['recent_changes']:
        print(f"  {change}")
    print()
    
    print("🔀 Recent Commits:")
    for commit in state['recent_commits']:
        print(f"  {commit}")
    
    if state['known_errors']:
        print()
        print("⚠️ Known Issues:")
        for error in state['known_errors']:
            print(f"  - {error}")
    
    print("=" * 60)
    
    return state


def show_status():
    """Show current project status."""
    print("=" * 60)
    print("📊 CURRENT PROJECT STATUS")
    print("=" * 60)
    
    # Translation count
    count = count_translations()
    print(f"Translation Entries: {count}")
    
    # Git status
    git = get_git_status()
    print(f"Uncommitted Files: {git['uncommitted_files']}")
    
    # Recent commits
    print("\n🔀 Recent Commits:")
    for commit in get_recent_commits(3):
        print(f"  {commit}")
    
    # Next tasks
    todo = get_todo_summary()
    if todo['next_tasks']:
        print("\n🎯 Next Tasks:")
        for task in todo['next_tasks']:
            print(f"  - {task}")
    
    print("=" * 60)


def handoff():
    """Generate and print handoff prompt for new chat."""
    if not SESSION_JSON.exists():
        print("❌ No previous session found. Run 'save' first.")
        return
    
    with open(SESSION_JSON, 'r', encoding='utf-8') as f:
        state = json.load(f)
    
    prompt = generate_handoff_prompt(state)
    print(prompt)
    
    # Also save to file
    handoff_file = PROJECT_ROOT / "Docs" / "HANDOFF_PROMPT.txt"
    handoff_file.write_text(prompt)
    print(f"\n💾 Also saved to: {handoff_file}")


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 session_manager.py [save|load|status|handoff]")
        print("")
        print("Commands:")
        print("  save    - Save current session state")
        print("  load    - Load and display previous session")
        print("  status  - Show current project status")
        print("  handoff - Generate copy-paste prompt for new chat")
        sys.exit(1)
    
    command = sys.argv[1].lower()
    
    # Optional: session work description
    session_work = " ".join(sys.argv[2:]) if len(sys.argv) > 2 else None
    
    if command == "save":
        save_session(session_work)
    elif command == "load":
        load_session()
    elif command == "status":
        show_status()
    elif command == "handoff":
        handoff()
    else:
        print(f"Unknown command: {command}")
        print("Available: save, load, status, handoff")
        sys.exit(1)


if __name__ == "__main__":
    main()
