@echo off
rem
rem Build file to build Effect (.fx) using MonoGame content builder tool
rem Use this with post-build command:
rem $(ProjectDir)\MonoGame-Effect-build.bat $(ProjectDir) $(OutDir)
rem
cd %1
"..\..\MGCB\MGCB.exe"^
 /outputDir:..\Game1\%2\Content^
 /intermediateDir:obj^
 /importer:EffectImporter^
 /processor:EffectProcessor^
 /build:GameIcon.fx^
 /build:Background.fx^
 /build:Background2.fx