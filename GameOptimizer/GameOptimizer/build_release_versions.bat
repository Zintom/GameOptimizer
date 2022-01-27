@echo off

echo Building Windows 64-Bit release..
echo.
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained
echo Done.
echo.

pause
timeout /t -1000

exit

echo Building Windows 32-Bit release..
echo.
dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -r win-x86
echo Done.
echo.

echo Building LINUX 64-BIT release..
echo.
dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -r linux-x64
echo Done.
echo.

echo Building Cross-platform binaries..
echo.
dotnet publish -c Release
echo Done.

echo.
echo.
echo Finished all builds.

timeout /t -1