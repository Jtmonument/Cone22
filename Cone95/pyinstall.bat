@echo off
pyinstaller --onefile create_pdf_script.py
cd dist
move create_pdf_script.exe ../bin/Debug/net6.0-windows/create_pdf.exe
cd ..