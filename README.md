[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/license/mit)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/idSaveDataResigner?label=Version)](https://github.com/mi5hmash/idSaveDataResigner/releases/latest)
[![Visual Studio 2022](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202022-5C2D91.svg?&logo=visual-studio&logoColor=white)](https://visualstudio.microsoft.com/)
[![.NET9](https://img.shields.io/badge/.NET%209-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# 🆔 idSaveDataResigner - What is it :interrobang:
<img src="https://github.com/mi5hmash/idSaveDataResigner/blob/main/.resources/images/idSaveDataResignerLogo.png" alt="icon" width="256"/>

This command-line utility can **encrypt and decrypt SaveData files** from various games running on idTech Engine versions 7 and 8. It can also **resign SaveData files** with your own UserID so you can **load them on your User Account**.

## Supported titles
| Game Title                         | Steam App ID  | Game Code |
|------------------------------------|---------------|-----------|
| DOOM Eternal                       | 782330        | MANCUBUS  |
| Indiana Jones and the Great Circle | 2677660       | SUKHOTHAI |
| DOOM The Dark Ages                 | 3017860       | MANCUBUS  |

## Supported platforms
Out of the box, it only supports SaveData files from the Steam PC version. Why? Simply because that’s the platform I chose to purchase these games on.

# 🤯 Why was it created :interrobang:
I wanted to share a SaveData file with a friend, but it isn't possible by default.

# :scream: Is it safe?
The short answer is: **No.** 
> [!CAUTION]
> If you unreasonably edit your SaveData files, you risk corrupting them or getting banned from playing online. In both cases, you will lose your progress.

> [!IMPORTANT]
> Always create a backup of any files before editing them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You’ve been warned. Now that you fully understand the possible consequences, you may proceed to the next chapter.

# :scroll: How to use this tool
This utility processes files based on the selected mode (`-m`). Each mode requires a game code (`-g`) and an input file path (`-p`). Mode-specific required parameters must be provided accordingly. Additional optional parameters may also be included.

> [!NOTE]
> All processed files are saved in a newly created folder within the **`_OUTPUT`** directory, located in the program's root directory.

> [!NOTE]
> Log files are saved in CSV format in the program's root directory. Only the two most recent log files are retained.

> [!TIP]
You can use the SteamDB calculator at [steamdb.info](https://steamdb.info/calculator/) to find your 64-bit SteamID.

## Resigning (`-m r`)
Resigns all files in the specified directory from one user ID to another.
### Usage:
```sh
.\idSaveDataResigner -m r -p "FOLDER_PATH" -g "GAME_CODE" -uI 76561197960265729 -uO 76561197960265730 -v
```
### Parameters:
- `-p` (Required) – Path to the directory containing files to resign.
- `-g` (Required) – Game code identifier.
- `-uI` (Required) – Original User ID.
- `-uO` (Required) – New User ID.
- `-v` (Optional) – Enables verbose console window output.

## Decryption (`-m d`)
Decrypts all SaveData files in the specified directory.
### Usage:
```sh
.\idSaveDataResigner -m d -p "FOLDER_PATH" -g "GAME_CODE" -u 76561197960265729 -v
```
### Parameters:
- `-p` (Required) – Path to the directory containing files to decrypt.
- `-g` (Required) – Game Code identifier.
- `-u` (Required) – User ID related to decryption.
- `-v` (Optional) – Enables verbose console window output.

## Encryption (`-m e`)
Encrypts all files in the specified directory.
### Usage:
```sh
.\idSaveDataResigner -m e -p "FOLDER_PATH" -g "GAME_CODE" -u 76561197960265729 -v
```
### Parameters:
- `-p` (Required) – Path to the directory containing files to encrypt.
- `-g` (Required) – Game Code identifier.
- `-u` (Required) – User ID related to encryption.
- `-v` (Optional) – Enables verbose console window output.

# :fire: Issues
All the issues I encountered during testing were fixed on the spot. If you happen to find any other problems (though I hope you won’t), feel free to report them [there](https://github.com/mi5hmash/idSaveDataResigner/issues).
