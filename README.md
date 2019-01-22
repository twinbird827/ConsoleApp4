ConsoleApp4 - ArrangayEx
====

## Overview

このアプリはコンソールアプリです。
GitHub - ConsoleApp2 - Arrangay の拡張版です
生成された実行ファイルにディレクトリ、また圧縮ファイルをドロップすると処理が実行されます。

## Description

1)
圧縮ファイルがドロップされると、7za.exeを使用して解凍します。
元の圧縮ファイルは削除し、解凍後のフォルダ名を次の処理で使用します。

ディレクトリがドロップされると、そのフォルダ名を次の処理で使用します。

2)
ディレクトリを対象に、Arrangay.exeを実行します。
処理の詳細はArrangayの説明を参照してください。

3)
ディレクトリをzipファイルに圧縮しなおします。
ディレクトリは削除します。

## Demo

## VS. 

## Requirement

.NET Framework 4.5.2

## Usage

1. C:\Testを対象にして実行します。
ArrangayEx.exe "C:\Test"

2. C:\Test1とC:\Test2を対象にして実行します。
ArrangayEx.exe "C:\Test1" "C:\Test2" ...

## Install

本プロジェクトとConsoleApp2を同じフォルダにcloneして、
各プロジェクトをビルドして生成した実行ファイルを任意のディレクトリに配置してください。

## Contribution

## Licence

[MIT](https://github.com/twinbird827/ConsoleApp4/blob/master/LICENSE)

## Author

[twinbird827](https://github.com/twinbird827)