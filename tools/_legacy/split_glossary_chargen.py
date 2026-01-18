import json
import os

SOURCE = "LOCALIZATION/glossary_chargen.json"

# Targets
TARGET_MODES = "LOCALIZATION/glossary_chargen_modes.json"
TARGET_UI = "LOCALIZATION/glossary_chargen_ui.json"
TARGET_STATS = "LOCALIZATION/glossary_chargen_stats.json"

def load_json(path):
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(path, data):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def split_glossary():
    data = load_json(SOURCE)
    
    # 1. Modes
    modes_data = data.get("chargen_mode", {})
    game_mode = {}
    game_mode_desc = {}
    
    for k, v in modes_data.items():
        # Heuristic: Description is long or has periods?
        # Names: "classic", "roleplay", "wander", "tutorial", "daily"
        # Descs: "permadeath...", "checkpoints...", "{{c|u}}..."
        if len(k) > 15 or " " in k or "{{" in k: # Quick heuristic
             game_mode_desc[k] = v
        else:
             game_mode[k] = v
             
    modes_out = {
        "game_mode": game_mode,
        "game_mode_desc": game_mode_desc
    }
    
    # 2. Stats
    stats_data = data.get("chargen_stats", {})
    stats_out = {
        "stat_desc": stats_data
    }
    
    # 3. UI
    ui_data = data.get("chargen_ui", {})
    ui_text = {}
    reputations = {}
    
    for k, v in ui_data.items():
        if "reputation" in k.lower():
            reputations[k] = v
        else:
            ui_text[k] = v
            
    ui_out = {
        "ui_text": ui_text,
        "reputations": reputations
    }
    
    # Save
    save_json(TARGET_MODES, modes_out)
    save_json(TARGET_STATS, stats_out)
    save_json(TARGET_UI, ui_out)
    
    print("Split complete.")

if __name__ == "__main__":
    split_glossary()
