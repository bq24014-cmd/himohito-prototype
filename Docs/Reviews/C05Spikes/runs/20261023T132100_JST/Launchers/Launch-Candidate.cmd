@echo off
cd /d "%~dp0Runtime"
start "" "PlayerGroundingReview.exe" -screen-fullscreen 0 -screen-width 1600 -screen-height 900 -variant Candidate -drop 0.42 -contactMode cut -length 6
