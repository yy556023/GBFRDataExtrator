# GBFR Data Extractor

GBFR Data Extractor is a small .NET console app that automates extracting files from Granblue Fantasy Relink data archives using `GBFRDataTools.exe`. It reads a `modded_filelist.txt`, extracts each listed file, and copies the results into an `original/{gbfrelink.*}/GBFR/data` folder structure for mod workflows.

## Features
- Loads tool and game paths from `appsettings.json`.
- Extracts only the files listed in a text file.
- Skips files that already exist (including converted `.msg -> .json` and `.bxm -> .bxm.xml`).
- Creates target folders automatically.

## Requirements
- .NET SDK 10.0
- `GBFRDataTools.exe`
- Game data files: `data.i` and `data/` from Granblue Fantasy Relink

## Setup
1. Update `appsettings.json` to point to your local paths:
```json
{
  "GameSettings": {
    "DataIndexPath": "D:\\GAME\\Steam\\steamapps\\common\\Granblue Fantasy Relink\\data.i",
    "DataFolderPath": "D:\\GAME\\Steam\\steamapps\\common\\Granblue Fantasy Relink\\data"
  },
  "ToolSettings": {
    "GBFRDataToolsPath": "E:\\repo\\yy556023\\GitHub\\gbfrelink-powerup\\GBFRDataTools\\GBFRDataTools.exe"
  }
}
```
2. Prepare a `modded_filelist.txt` containing relative paths (one per line), for example:
```txt
system/param/player/char_param.bin
ui/message/msg_quest_001.msg
```

## Usage
Build:
```bash
dotnet build
```

Run:
```bash
dotnet run --project GBFRDataExtrator.csproj
```

When prompted, enter the full path to your `modded_filelist.txt`. The app will extract and copy files into:
```
original/{gbfrelink.*}/GBFR/data/{relative_path}
```

## Project Structure
- `Program.cs`: Main workflow and file processing logic.
- `GameSettings.cs`, `ToolSettings.cs`: Configuration models.
- `PathConfig.cs`: Derived paths used during extraction.
- `ProcessResult.cs`: Simple stats container.
- `appsettings.json`: Local machine configuration.

## Notes
- `appsettings.json` contains machine-specific paths; avoid committing personal paths if sharing publicly.
- The target folder name is derived by replacing `gbfr.` with `gbfrelink.` from the input file’s parent directory name.
