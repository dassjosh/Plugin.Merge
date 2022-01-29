cd ../src/PluginMerge

dotnet build
dotnet pack
dotnet tool install --global --add-source ./bin/nupkg MJSU.Plugin.Merge