@echo off
cd /d "%~dp0..\Runtime"
start "" "PlayerGroundingReview.exe" -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -variant Candidate -length 7 -vfxLegacy 1
