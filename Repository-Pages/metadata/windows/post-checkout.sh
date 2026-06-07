#!/bin/sh

# This script is automatically executed by the system Git Bash, and run
# when the current active Branch is changed in this Repository, locally on this
# machine.

echo "===================================================="
echo "Git Hook: Running Smart Branch Switch"
echo "===================================================="

# Get the current real path of this Bash Script
DIR="$(dirname "$0")"
# Run the Smart Branch Switch
# $3 = Change Flag. If 1, is a Branch change. If 0, is just a file change.
# $1 = Hash of the old Branch
# $2 = Hash of the current Branch
"$DIR/Smart-Branch-Switch.exe" "$3" "$1" "$2"