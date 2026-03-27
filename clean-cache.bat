@echo off
echo Menghapus folder bin/obj dari index Git...

REM Hapus bin/obj di Library.App
git rm -r --cached src\Library.App\bin
git rm -r --cached src\Library.App\obj

REM Hapus bin/obj di Library.UI
git rm -r --cached src\Library.UI\bin
git rm -r --cached src\Library.UI\obj

REM Hapus bin/obj di Library.Web
git rm -r --cached src\Library.Web\bin
git rm -r --cached src\Library.Web\obj

REM Hapus bin/obj di Library.Api (kalau ada)
git rm -r --cached src\Library.Api\bin
git rm -r --cached src\Library.Api\obj

echo Selesai. Jangan lupa commit perubahan:
echo   git commit -m "Remove build artifacts from index"
pause
