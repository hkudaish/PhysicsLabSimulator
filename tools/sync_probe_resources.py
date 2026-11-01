"""Copy the authoritative SQL design into the Unity-only diagnostic resource."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
destination = ROOT / 'src/UnityClient/Assets/Resources/Phase0/local.sql.txt'
data = (ROOT / 'specs/data/local.sql').read_bytes()
destination.write_bytes(data)
provenance = ROOT / 'src/UnityClient/compatibility-assets.json'
doc = json.loads(provenance.read_text())
doc['schema']['sha256'] = hashlib.sha256(data).hexdigest()
provenance.write_text(json.dumps(doc, indent=2) + '\n')
print('Unity probe schema and provenance match specs/data/local.sql')
