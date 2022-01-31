## Plugin Merge

[![NuGet version (MJSU.Plugin.Merge)](https://img.shields.io/nuget/v/MJSU.Plugin.Merge?style=flat-square)](https://www.nuget.org/packages/MJSU.Plugin.Merge/)
![Main Branch status](https://img.shields.io/github/workflow/status/dassjosh/plugin.merge/build/main?label=main&style=flat-square)
![Develop Branch status](https://img.shields.io/github/workflow/status/dassjosh/plugin.merge/build/develop?label=develop&style=flat-square)

Plugin Merge is a .net 6 CLI tool that allows merging multiple .cs files into a single Oxide / uMod plugin file.

## Installation
`dotnet tool install --global MJSU.Plugin.Merge` from the shell/command line.

## Init Command

Create config in current directory:  
`plugin.merge init -f merge.json -p ./`  
Create config in specified directory:  
`plugin.merge init -f merge.json -p C:\Users\USERNAME\Source\Repos\MyFirstMergePlugin`

`-p`, `--path`  (Default: ./) Path to create the merge.json configuration file in  
`-f`, `--filename` (Default: merge.yaml) Name of the outputted configuration file  
`-d`, `--debug` Enable debug log output  
`--help` Display this help screen.  
`--version` Display version information.

## Merge Command (Default Command)

Merge and Compile:  
`plugin.merge -c -m -p ./merge.json`  
Merge Only:  
`plugin.merge -m -p C:\Users\USERNAME\Source\Repos\MyFirstMergePlugin\merge.json`  
Merge Additional Output Paths:  
`plugin.merge -c -m -o ./additional/output/path -p ./merge.json`  
Compile Only:  
`plugin.merge -c -p ./merge.json`

`-p`, `--path`(Default: ./merge.yaml) Path to the merge.json configuration file  
`-m`, `--merge` (Group: Mode) (Default: false) Enables merge mode to merge files into a single plugin/framework file  
`-c`, `--compile` (Group: Mode) (Default: false) Enables compile mode. Will attempt to compile merged file and display
any errors if it fails.  
`-o`, `--output` Additional output paths for the generated code file  
`-d`, `--debug` Enable debug log output  
`--help` Display this help screen.  
`--version` Display version information.

## Getting Started

To get started using plugin merge open a command prompt / terminal and type `plugin.merge init`. 
This will created the default configuration file named merge.yaml in the directory that is currently open.
Plugin Merge also supports JSON. You can use `plugin.merge init -f merge.json`
Place your config file a directory near your plugin .cs files.
Update the config paths to point to the input and output paths you would like to use.
The configuration supports relative pathing and all paths use the configuration files directory as it's staring point.

Once your configuration file is setup it's time to merge the files together.
You can run the merge by typing `plugin.merge -m -p ./merge.yaml`. 
This will merge all the .cs files together and create a final file in the output paths specified.
You can also enable compilation to compile your plugin to check for any issues before loading it onto your server.
To enable compilation add the `-c` argument while merging Ex: `plugin.merge -m -c -p ./merge.yaml`

### File Settings
You can control certain settings about imported .cs files by adding specific comments into the file before the class declaration  
`//Define:FileOrder=100` - This will control which order the file is added into final output file. Default value is 100  

`//Define:ExcludeFile` - This will prevent a file from being processed  

`//Define:Framework` - This defines a file as a framework. Framework files are added at the very bottom in separate partial classes 
(Note: This should not be added manually and is already added by the Plugin Merge Tool)  

## Configuration

### Creator Modes
There are 3 types of merge options when using Plugin Merge.  
`Plugin` - will merge all files into a final Plugin to be used.  
`Framework` - Will output a framework file that can be copied into another plugin that isn't a merge framework plugin  
`MergeFramework` - Will output a framework file that can be used with a plugin that is a merge framework plugin  

### YAML Configuration File
```yaml
# What platform to write the code file for (Oxide, uMod)
Platform: Oxide
Merge Settings:
# Outputted plugin name
  Plugin Name: MyPluginName
  # Outputted plugin base class
  Plugin Base Class: CovalencePlugin
  # Which type of file to output (Plugin, Framework, or MergeFramework)
  Creator Mode: Plugin
  # Paths to use when reading in source code relative to the merge config
  Plugin Input Paths:
  - ./
  # Paths to use when writing the plugin file relative to the merge config
  Plugin Output Paths:
  - ./build
  # Oxide //References: definitions
  Reference Definitions: []
  # #define definitions
  Define Definitions:
  - DEBUG
  # Paths to be ignored when searching for source code relative to merge config
  Ignore Paths:
  - ./IgnoreThisPath
  # Files to be ignored when searching for source code relative to merge config
  Ignore Files:
  - ./IgnoreThisFile.cs
  # Namespaces to ignore when processing output file
  Ignore Namespaces:
  - IgnoreThisNameSpace
  Code Style:
  # Character to use for code indents
    Indent Character: ' '
    # The amount of characters to use when indenting once
    Indent Char Amount: 4
    # Indent value will increase / decrease by this number
    Indent Multiplier: 1
    # String to use for new lines in code
    New Line String: "\r\n"
    # Adds the code file path in a region
    Write The Relative File Path In Region: true
    # Adds the code file path in a region
    Keep Code Comments: true
Compile Settings:
  AssemblyPaths:
  - ./Assemblies
  # Ignores the following paths relative to the merge config
  Ignore Paths:
  - ./Assemblies/x86
  - ./Assemblies/x64
  # Ignores the following files relative to the merge config
  Ignore Files:
  - ./Assemblies/Newtonsoft.Json.dll
  Compile Log Level (Hidden, Info, Warning, Error): Error

```

### JSON Configuration File
```json
{
  "Platform": "Oxide",
  "Merge Settings": {
    "Plugin Name": "MyPluginName",
    "Plugin Base Class": "CovalencePlugin",
    "Creator Mode": "Plugin",
    "Plugin Input Paths": [
      "./"
    ],
    "Plugin Output Paths": [
      "./build"
    ],
    "Reference Definitions": [],
    "Define Definitions": [
      "DEBUG"
    ],
    "Ignore Paths": [
      "./IgnoreThisPath"
    ],
    "Ignore Files": [
      "./IgnoreThisFile.cs"
    ],
    "Ignore Namespaces": [
      "IgnoreThisNameSpace"
    ],
    "Code Style": {
      "Indent Character": " ",
      "Indent Char Amount": 4,
      "Indent Multiplier": 1,
      "New Line String": "\r\n",
      "Write The Relative File Path In Region": true,
      "Keep Comments": true
    }
  },
  "Compile Settings": {
    "Assembly Paths": [
      "./Assemblies"
    ],
    "Ignore Paths": [
      "./Assemblies/x86",
      "./Assemblies/x64"
    ],
    "Ignore Files": [
      "./Assemblies/Newtonsoft.Json.dll"
    ],
    "Compile Log Level (Hidden, Info, Warning, Error)": "Error"
  }
}
```