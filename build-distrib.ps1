Remove-Item Distrib -Recurse -Force
dotnet publish --self-contained --runtime win-x64 --configuration Release --verbosity Minimal -o Distrib SimpleLauncher/SimpleLauncher.csproj /p:PublishSingleFile=true /p:PublishTrimmed=false /p:IncludeNativeLibrariesForSelfExtract=true 
Remove-Item Distrib\*.xml
Remove-Item Distrib\*.pdb
#Remove-Item *.zip
#Compress-Archive Distrib\*.* ZqlDownloader.zip