#!/bin/bash
Plib=`find $MSBuildProjectDirectory/../packages -type f -name "PLib.dll"`
ILRepack=`find $MSBuildProjectDirectory/../packages -type f -name "ILRepack.exe"`
Lib=$MSBuildProjectDirectory/../lib

echo "Plib found at $Plib"
echo "ILRepack found at $Plib"

mono $ILRepack /ndebug /lib:$Lib /out:$TargetName.dll $TargetName.dll $Plib /targetplatform:v4