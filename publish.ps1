rm -Force -ErrorAction SilentlyContinue GCaliper-Win-x64.zip
rm -Force -ErrorAction SilentlyContinue GCaliper-Linux-x64.zip

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue src/bin/Release

dotnet publish -c Release -r win-x64 /p:PublishWindows=true
dotnet publish -c Release -r linux-x64

$compress = @{
    Path = "src\bin\Release\net5.0\win-x64\publish\*"
    CompressionLevel = "Optimal"
    DestinationPath = "GCaliper-Win-x64.zip"
  }
  Compress-Archive @compress  

$compress = @{
    Path = "src\bin\Release\net5.0\linux-x64\publish\*"
    CompressionLevel = "Optimal"
    DestinationPath = "GCaliper-Linux-x64.zip"
  }
  Compress-Archive @compress

