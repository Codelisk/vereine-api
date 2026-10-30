#!/usr/bin/env bash
# Holt die Shiny-Skills (github.com/shinyorg/skills, Plugin "shiny") nach .claude/skills.
# Die Skills liegen bewusst im Repo statt als Plugin auf Benutzerebene (Workspace-Regel:
# Konfiguration lokal und eingecheckt). Eigene Skills daneben bleiben unberührt.
#
# Aufruf:  tools/sync-shiny-skills.sh [ref]     ref = Branch, Tag oder Commit, Standard: main
# Danach:  git diff --stat .claude/skills       Änderungen prüfen und committen
#
# Auswahl: Steht in .claude/skills/.shiny-skills-select je Zeile ein Skill-Name, werden nur diese
# übernommen (vereine-api). Ohne die Datei kommen alle Shiny-Skills (vereine-app).
set -euo pipefail

ref="${1:-main}"
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
target="$repo_root/.claude/skills"
list="$target/.shiny-skills-list"
select="$target/.shiny-skills-select"

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

git -C "$work" init -q
git -C "$work" remote add origin https://github.com/shinyorg/skills.git
git -C "$work" fetch -q --depth 1 origin "$ref"
git -C "$work" checkout -q FETCH_HEAD
commit="$(git -C "$work" rev-parse HEAD)"

source_dir="$work/plugins/shiny/skills"
[ -d "$source_dir" ] || { echo "plugins/shiny/skills fehlt in $ref" >&2; exit 1; }

mkdir -p "$target"
touch "$list"

names=()
if [ -f "$select" ]; then
  while IFS= read -r name; do
    [ -z "$name" ] && continue
    [ -d "$source_dir/$name" ] || { echo "Ausgewählter Skill $name fehlt in $ref" >&2; exit 1; }
    names+=("$name")
  done < "$select"
else
  for dir in "$source_dir"/*/; do names+=("$(basename "$dir")"); done
fi

# Vor dem Löschen prüfen: ein gleichnamiger Skill, der nicht aus Shiny stammt, wird nie überschrieben.
for name in "${names[@]}"; do
  if [ -e "$target/$name" ] && ! grep -qxF "$name" "$list"; then
    echo "Konflikt: .claude/skills/$name ist ein eigener Skill und heißt wie ein Shiny-Skill." >&2
    exit 1
  fi
done

# Bisher übernommene Shiny-Skills entfernen, damit upstream gelöschte Skills mitverschwinden.
while IFS= read -r name; do
  [ -n "$name" ] && rm -rf "${target:?}/$name"
done < "$list"

: > "$list"
for name in "${names[@]}"; do
  cp -R "$source_dir/$name" "$target/$name"
  echo "$name" >> "$list"
done
echo "$commit" > "$target/.shiny-skills-version"

echo "Shiny-Skills $commit: $(wc -l < "$list") Skills in .claude/skills"
