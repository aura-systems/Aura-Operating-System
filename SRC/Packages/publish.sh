#!/usr/bin/env bash
# Publishes the packages of this folder to the online repository that pkg /update and pkg /add read:
# each folder zipped to packages/<name>.pkg, and repository.json listing them from their package.xml.
#
#   SRC/Packages/publish.sh <destination> [base-url]
#
# destination: a directory or an rsync target (user@host:/path), the document root of base-url.
# base-url:    where that root is served, https://aura.valentin.bzh by default (the links Aura follows).
#
# Only packages/ and repository.json are replaced: anything else in the destination (os.json) stays.
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "usage: $0 <destination> [base-url]" >&2
  exit 1
fi

destination="${1%/}"
base_url="${2:-https://aura.valentin.bzh}"
base_url="${base_url%/}"
packages_dir="$(cd "$(dirname "$0")" && pwd)"
build="$(mktemp -d)"
trap 'rm -rf "$build"' EXIT

mkdir -p "$build/packages"

for manifest in "$packages_dir"/*/package.xml; do
  folder="$(dirname "$manifest")"
  name="$(python3 -c 'import sys, xml.etree.ElementTree as ET; print(ET.parse(sys.argv[1]).getroot().get("name"))' "$manifest")"

  # The same archive as the AuraBuildPackages target: the folder's contents at the root of the zip.
  (cd "$folder" && zip -q -r -X "$build/packages/$name.pkg" . -x '.*')
  echo "packages/$name.pkg"
done

# Every value is a string: Aura's reader (PackageManager.Update) reads each property as one.
python3 - "$packages_dir" "$base_url" > "$build/repository.json" <<'EOF'
import json, pathlib, sys
import xml.etree.ElementTree as ET

packages_dir, base_url = pathlib.Path(sys.argv[1]), sys.argv[2]
entries = []

for manifest in sorted(packages_dir.glob("*/package.xml")):
    package = ET.parse(manifest).getroot()
    name = package.get("name")
    entries.append({
        "name": name,
        "display-name": package.get("displayName") or name,
        "description": package.get("description") or "",
        "author": package.get("author") or "",
        "version": package.get("version") or "",
        "link": f"{base_url}/packages/{name}.pkg",
    })

json.dump(entries, sys.stdout, indent=2, ensure_ascii=False)
print()
EOF
echo "repository.json: $(python3 -c 'import json, sys; print(len(json.load(open(sys.argv[1]))))' "$build/repository.json") packages"

# Readable by the server's web user.
chmod 755 "$build/packages"
chmod 644 "$build/packages/"*.pkg "$build/repository.json"

rsync -a --delete "$build/packages/" "$destination/packages/"
rsync -a "$build/repository.json" "$destination/repository.json"
echo "Published to $destination ($base_url)"
