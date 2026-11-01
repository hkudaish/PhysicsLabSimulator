"""Check bundled glyph coverage, without claiming text shaping or rendering correctness."""
import json
from pathlib import Path

from fontTools.ttLib import TTFont

ROOT = Path(__file__).resolve().parents[1]
SAMPLES = ['مختبر العلوم الافتراضي', 'التركيز', 'C = 0.5 mol/L', 'H₂O', 'NaCl', 'I = V/R', '−0.009 A', '1 kΩ']


def main():
    resource = ROOT / 'src/UnityClient/Assets/Resources/Phase0'
    maps = []
    for filename in ('Tajawal-Regular.ttf', 'DejaVuSans.ttf'):
        with TTFont(resource / filename) as font:
            maps.append(font.getBestCmap())
    missing = {sample: sorted({f'U+{ord(character):04X}' for character in sample
                               if not any(ord(character) in mapping for mapping in maps)}) for sample in SAMPLES}
    result = {'samples': missing, 'fallbackRequiredForSubscript2': 0x2082 not in maps[0] and 0x2082 in maps[1],
              'scope': 'Glyph presence only; actual Arabic shaping/BiDi/Unity rendering unverified.'}
    print(json.dumps(result, indent=2, ensure_ascii=False))
    return 1 if any(missing.values()) else 0


if __name__ == '__main__':
    raise SystemExit(main())
