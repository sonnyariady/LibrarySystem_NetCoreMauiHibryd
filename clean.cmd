rmdir /s /q .vs
rmdir /s /q src\Library.App\bin
rmdir /s /q src\Library.App\obj
rmdir /s /q src\Library.UI\bin
rmdir /s /q src\Library.UI\obj
rmdir /s /q src\Library.Web\bin
rmdir /s /q src\Library.Web\obj

dotnet restore
dotnet build
print "Press any key..."
pause