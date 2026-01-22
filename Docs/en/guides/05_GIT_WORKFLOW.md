# Git Auto-Commit Guide

## Quick Commit Scripts

### 1. Interactive Quick Commit (Recommended)
```bash
bash tools/quick-commit.sh
```
- Shows current changes
- Prompts for commit message
- Auto-detects commit type (feat/fix/docs/i18n/chore)
- Runs validation
- Commits and pushes

**Example:**
```bash
$ bash tools/quick-commit.sh
=== Current Changes ===
M Scripts/02_Patches/10_UI/CharacterCreation.cs
M LOCALIZATION/CHARGEN/ui.json

Enter commit message: Add stat translation support
Commit: fix: Add stat translation support
✓ Successfully committed and pushed!
```

### 2. Direct Commit with Message
```bash
bash tools/auto-commit.sh "your commit message"
```
- Requires commit message as argument
- Runs validation (blocks if fails)
- Commits and pushes

**Example:**
```bash
bash tools/auto-commit.sh "feat: Implement new translation screen"
```

### 3. VS Code Tasks (Keyboard Shortcut)

Press `Cmd+Shift+P` → Type "Run Task" → Select:
- **"Quick Commit & Push"** - Interactive commit
- **"Validate & Deploy"** - Run validation and deploy to game
- **"Run Validation Only"** - Just run project_tool.py

Or set up keyboard shortcut in VS Code:
1. `Cmd+K Cmd+S` to open keyboard shortcuts
2. Search "Tasks: Run Task"
3. Assign shortcut (e.g., `Cmd+Shift+G`)

## Commit Type Conventions

Auto-detected based on changed files:

| Changed Files | Type | Example |
|---------------|------|---------|
| `Scripts/02_Patches/` | `fix` | Bug fixes in patches |
| `Scripts/` (other) | `feat` | New features |
| `LOCALIZATION/` | `i18n` | Translation updates |
| `Docs/` | `docs` | Documentation updates |
| `tools/` | `chore` | Tool/build updates |

## Manual Git Commands

If you prefer manual control:

```bash
# 1. Check status
git status

# 2. Add all changes
git add -A

# 3. Commit
git commit -m "type: message"

# 4. Push
git push origin main
```

## Validation

All scripts run `python3 tools/project_tool.py` before committing.

If validation fails:
- **auto-commit.sh**: Blocks commit
- **quick-commit.sh**: Asks if you want to force commit

To skip validation:
```bash
git add -A
git commit -m "your message" --no-verify
git push origin main
```

## Best Practices

1. **Commit Often**: After each logical change
2. **Descriptive Messages**: Explain what and why
3. **Run Validation**: Before every commit
4. **Atomic Commits**: One feature/fix per commit

### Good Commit Messages
```
fix: Resolve stat translation duplication in character creation
feat: Add subtype description translation support
i18n: Update mutation translations with proper formatting
docs: Reorganize documentation by language and purpose
```

### Bad Commit Messages
```
update
fix stuff
wip
changes
```

## Troubleshooting

### Push Rejected (Behind Remote)
```bash
git pull --rebase origin main
git push origin main
```

### Merge Conflicts
```bash
git status  # Check conflicted files
# Edit files to resolve conflicts
git add -A
git rebase --continue
git push origin main
```

### Undo Last Commit (Not Pushed)
```bash
git reset --soft HEAD~1  # Keep changes
# or
git reset --hard HEAD~1  # Discard changes
```

### Undo Last Commit (Already Pushed)
```bash
git revert HEAD
git push origin main
```
