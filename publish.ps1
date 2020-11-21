dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64

rm GCaliper-Win-x64.zip
rm GCaliper-Linux-x64.zip

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

