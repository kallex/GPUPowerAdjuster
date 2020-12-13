rmdir /s /q bin\Release\net5.0\win10-x64\publish
del /Q ..\setup\publish\*
dotnet publish GPUPowerAdjusterSvc.csproj -c Release --self-contained --runtime win10-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
copy bin\Release\net5.0\win10-x64\publish\appsettings.json ..\setup\publish\
copy bin\Release\net5.0\win10-x64\publish\*.dll ..\setup\publish\
copy bin\Release\net5.0\win10-x64\publish\*.exe ..\setup\publish\

