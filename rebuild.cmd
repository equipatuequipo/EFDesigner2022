cd src
"C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\VSIXInstaller.exe" /q /u:"dd1c2ec0-b732-4b74-a591-4d78684bb231"
del "..\dist\Sawczyn.EFDesigner.EFModel.DslPackage.vsix"
REM msbuild efdesigner.sln /t:Rebuild /p:Configuration=Debug
"C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" EFDesigner2022.sln /restore /t:Rebuild /p:Configuration=Release
if errorlevel 1 (cd .. & exit /b 1)
copy /Y "DslPackage\bin\Release\Sawczyn.EFDesigner.EFModel.DslPackage.vsix" ..\dist
"C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\VSIXInstaller.exe" /q "..\dist\Sawczyn.EFDesigner.EFModel.DslPackage.vsix"
cd ..
