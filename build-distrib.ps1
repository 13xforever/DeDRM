Remove-Item Distrib -Recurse -Force
& dotnet publish --self-contained --runtime win-x64 --configuration Release --verbosity Minimal -o Distrib /p:PublishSingleFile=true SimpleLauncher/SimpleLauncher.csproj
Remove-Item Distrib\*.xml
Remove-Item Distrib\*.pdb
#Remove-Item *.zip
#Compress-Archive Distrib\*.* ZqlDownloader.zip