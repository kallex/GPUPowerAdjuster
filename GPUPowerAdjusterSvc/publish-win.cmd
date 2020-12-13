rmdir /s /q bin\Release\net5.0\win10-x64\publish
dotnet publish GPUPowerAdjusterSvc.csproj -c Release --self-contained --runtime win10-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true

