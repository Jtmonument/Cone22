@echo off
pyinstaller --onefile create_pdf_script.py
mkdir Resources
cd dist
move create_pdf_script.exe ../Resources/create_pdf.exe
cd ..
rmdir /s /q dist
rmdir /s /q build
del *.spec
echo.& echo Make sure to update the Resources.resx file through Microsoft Visual Studio if this is the first time!
pause