@echo off
rem GenFragments - 32.bat
rem Version 1.0.0.25

rem Latest template version 1.0.0.0

set Fragments=%CD%
for /f "delims=" %%x in (config_Batch.cfg) do (set "%%x")
cd %BinDir%
set InstallFiles="%CD%"
set File="%Fragments%\FilesFragment.wxs"
set Template="%Fragments%\FragmentTemplate - 32.xslt"
set CopyPath="..\..\..\%ProjectName%\bin\Release\"

echo ************************************
echo *  GenFragments - Deleting .pdb's  *
echo ************************************
del /s /q /f %InstallFiles%\*.pdb 1>NUL 2>NUL
del /s /q /f %InstallFiles%\*.xml 1>NUL 2>NUL
del /s /q /f %InstallFiles%\*.vshost.* 1> NUL 2>NUL
del /s /q /f %InstallFiles%\*.exe.config 1> NUL 2>NUL
echo Lingering .pdb's, .xml, .vshost.exe, and .config files have been removed.
echo.

echo ****************************************************
echo *  GenFragments - Adding dlls from child projects  *
echo ****************************************************
echo Only projects that have a 'bin/Release' folder are listed
echo Skipping DLLs which already exist in the target project
echo.

cd %InstallFiles%
cd ../../..

for /d %%d in (*) do (
    if not "%%d" == "%ProjectName%" (
        if exist "%%d\bin\Release" (
            echo Found project %%d
            cd "%%d\bin\Release"

            for %%f in (*.dll) do (
                if exist "..\..\..\%ProjectName%\bin\Release\%%f" (
                    rem echo  -Skipping %%f
                )

                if not exist "..\..\..\%ProjectName%\bin\Release\%%f" (
                    echo  -Copying %%f
                    copy "%%f" %CopyPath% 1>NUL 2>NUL
                )
            )
            cd ..\..\..
        )
    )
)

echo.


rem echo ************************************
rem echo * GenFragments - Variable Display  *
rem echo ************************************
rem echo.
rem echo Fragments Folder: %Fragments%
rem echo Bin Folder: %InstallFiles%
rem echo.
rem echo Component Group: %CmpGroup%
rem echo Template: %Template%
rem echo Output File: %File%
rem echo.
rem echo Heat Command: C:\Program Files (x86)\WiX Toolset v3.9\bin\heat.exe dir %InstallFiles% -cg %CmpGroup% -dr INSTALLFOLDER -gg -scom -sreg -sfrag -srd -t %Template% -var var.MySource InstallFiles -out %File%
rem echo.

echo ************************************
echo * GenFragments - Heat output below *
echo ************************************
echo.
"C:\Program Files (x86)\WiX Toolset v3.9\bin\heat.exe" dir %InstallFiles% -cg %CmpGroup% -dr INSTALLFOLDER -gg -scom -sreg -sfrag -srd -t %Template% -var var.MySource InstallFiles -out %File%
pause