cd ../src/PluginMerge

dotnet tool uninstall -g MJSU.Plugin.Merge
dotnet build --configuration Release /p:Version=1.0.0
dotnet tool install --global --add-source ./bin/nupkg MJSU.Plugin.Merge --version 1.0.0