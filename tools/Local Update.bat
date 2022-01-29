cd ../src/PluginMerge

dotnet build
dotnet pack
dotnet tool update --global --add-source ./bin/nupkg PluginMerge