
import json
import os
from pathlib import Path

CONFIG_FILE = Path(__file__).parent / "config.json"
# MacOS Default
DEFAULT_PATH = "~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization"

def get_path():
    path = DEFAULT_PATH
    if CONFIG_FILE.exists():
        try:
            content = CONFIG_FILE.read_text(encoding='utf-8')
            data = json.loads(content)
            path = data.get("game_mod_dir", DEFAULT_PATH)
        except Exception as e:
            # If config is broken, use default but warn (stderr)
            pass
    
    return os.path.expanduser(path)

if __name__ == "__main__":
    print(get_path())
