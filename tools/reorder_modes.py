import json
import os

SOURCE = "LOCALIZATION/glossary_chargen_modes.json"

def reorder_modes():
    with open(SOURCE, 'r', encoding='utf-8') as f:
        data = json.load(f)
        
    # Flatten
    all_pairs = {}
    for cat in data.values():
        all_pairs.update(cat)
        
    # Define Groups
    groups = [
        # Classic
        ["classic", "roguelike mode. permadeath: you lose your character when you die.", "permadeath: lose your character when you die."],
        # Roleplay
        ["roleplay", "checkpoints at settlements.", "checkpointing at settlements.", "{{c|ù}} checkpointing at settlements."],
        # Wander
        ["wander", "{{c|ù}} no xp for killing.", "{{c|ù}} more xp for discoveries and performing the water ritual.", "{{c|ù}} most creatures begin neutral to you."],
        # Daily
        ["daily", "start a new game with one button.", "{{c|ù}} currently in day {{w|{day_of_year}}} of {{w|{year}}}.", "{{c|ù}} one chance with a fixed character and world seed."],
        # Tutorial
        ["tutorial", "learn the basics of caves of qud."]
    ]
    
    new_data = {}
    
    for group in groups:
        # Use first item (Title) as a 'header' comment? No, can't in JSON.
        # Just insert them in order.
        # We assume the user reads top-down.
        for key in group:
            if key in all_pairs:
                new_data[key] = all_pairs[key]
                del all_pairs[key]
                
    # Add remaining
    for k, v in all_pairs.items():
        new_data[k] = v
        
    # Wrap in single category "game_modes_bundled"
    final_output = {
        "game_modes_bundled": new_data
    }
    
    with open(SOURCE, 'w', encoding='utf-8') as f:
        json.dump(final_output, f, indent=2, ensure_ascii=False)
        
    print("Reordered modes.")

if __name__ == "__main__":
    reorder_modes()
