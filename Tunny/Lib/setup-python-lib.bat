:: This batch file based on https://github.com/lmbelo/python3-embeddable
echo off

set PYVER=3.10.11
set ARCH=amd64

if exist python.zip del python.zip
if exist whl.zip del whl.zip

:: Download the Python embeddable
curl.exe https://www.python.org/ftp/python/%PYVER%/python-%PYVER%-embed-%ARCH%.zip > python-embed.zip
powershell expand-archive -Path python-embed.zip -DestinationPath python-embed
del python-embed.zip

:: Enable site packages
cd python-embed\
for /r %%x in (*._pth) do (call :FindReplace "#import site" "import site" %%x)
cd ..

:: Run the get-pip script
curl -sSL https://bootstrap.pypa.io/get-pip.py -o get-pip.py
python-embed\python.exe get-pip.py

:: Get python wheels
python-embed\python.exe -m pip download -r requirements.txt -d whl

:: Create the final embeddable dir and moves Python distribution into it
if exist "embeddable\" rmdir /S /Q "embeddable\"
move /Y python-embed embeddable

powershell Compress-Archive -Path embeddable/* -DestinationPath python.zip
powershell Compress-Archive -Path whl/* -DestinationPath whl.zip
del get-pip.py
rmdir /S /Q "embeddable\"
rmdir /S /Q "whl\"

goto:eof

:FindReplace <findstr> <replstr> <file>
set tmp="%temp%\tmp.txt"
If not exist %temp%\_.vbs call :MakeReplace
for /f "tokens=*" %%a in ('dir "%3" /s /b /a-d /on') do (
  for /f "usebackq" %%b in (`Findstr /mic:"%~1" "%%a"`) do (
    echo(&Echo Replacing "%~1" with "%~2" in file %%~nxa
    <%%a cscript //nologo %temp%\_.vbs "%~1" "%~2">%tmp%
    if exist %tmp% move /Y %tmp% "%%~dpnxa">nul
  )
)
del %temp%\_.vbs
exit /b

:MakeReplace
>%temp%\_.vbs echo with Wscript
>>%temp%\_.vbs echo set args=.arguments
>>%temp%\_.vbs echo .StdOut.Write _
>>%temp%\_.vbs echo Replace(.StdIn.ReadAll,args(0),args(1),1,-1,1)
>>%temp%\_.vbs echo end with

