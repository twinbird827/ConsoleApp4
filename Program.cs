using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            // 作業ﾃﾞｨﾚｸﾄﾘを取得
            var work = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            foreach (var arg in args)
            {
                var target = arg;

                Console.WriteLine($"* 解凍処理を開始します");

                // ﾌｧｲﾙ名→ﾌｫﾙﾀﾞ名変更ﾃﾞﾘｹﾞｰﾄ
                Func<string, string> to_directory = (file) => Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Trim(' ', '　', '.'));

                // ﾌﾟﾛｾｽ実行ﾃﾞﾘｹﾞｰﾄ
                Action<string, Func<string>> process_start = (file, argumenter) =>
                {
                    Console.WriteLine($"work: {work}");
                    Console.WriteLine($"exe : {file}");
                    Console.WriteLine($"arg : {argumenter()}");
                    var process = new ProcessStartInfo();

                    process.WorkingDirectory = work;
                    process.FileName = file;
                    process.Arguments = argumenter();
                    process.UseShellExecute = false;
                    process.CreateNoWindow = true;

                    Process.Start(process).WaitForExit();
                };

                // 圧縮ﾌｧｲﾙ→ﾌｫﾙﾀﾞ
                target = Execute(target,
                    (file) => File.Exists(file),
                    (file) => process_start("7za.exe", () => $"x -y -r -aoa \"{file}\" -o\"{to_directory(file)}"),
                    (file) => file
                );

                Console.WriteLine($"* 解凍したﾌｧｲﾙを削除します");

                // 元のﾌｧｲﾙを削除
                target = Execute(target,
                    (file) => File.Exists(file),
                    (file) => File.Delete(file),
                    (file) => to_directory(file)
                );

                Console.WriteLine($"* ﾌｧｲﾙ内を整形します");

                // ﾌｧｲﾙ内整形
                target = Execute(target,
                    (file) => Directory.Exists(file),
                    (file) => process_start(Path.Combine(work, @"..\..\..\ConsoleApp2\bin\Debug\Arrangay.exe"), () => $"\"{file}\""),
                    (file) => file
                );

                Console.WriteLine($"* 圧縮処理を開始します");

                // ﾌｫﾙﾀﾞ→圧縮ﾌｧｲﾙ
                target = Execute(target,
                    (file) => Directory.Exists(file),
                    (file) => process_start(@"7za.exe", () => $"a -mmt=on -y -r \"{file}.zip\"  \"{file}\""),
                    (file) => file
                );

                Console.WriteLine($"* 解凍したﾌｫﾙﾀﾞを削除します");

                // 解凍したﾌｫﾙﾀﾞを削除
                target = Execute(target,
                    (file) => Directory.Exists(file),
                    (file) => DeleteDirectory(file),
                    (file) => file
                );

            }
        }

        /// <summary>
        /// 処理を実行する
        /// </summary>
        /// <param name="target">対象ﾌｧｲﾙ(ﾌｫﾙﾀﾞ)</param>
        /// <param name="checker">処理を実行するか確認するﾃﾞﾘｹﾞｰﾄ</param>
        /// <param name="executer">処理を実行するﾃﾞﾘｹﾞｰﾄ</param>
        /// <param name="resulter">次に実行するﾌｧｲﾙ(ﾌｫﾙﾀﾞ)</param>
        /// <returns></returns>
        private static string Execute(string target, Func<string, bool> checker, Action<string> executer, Func<string, string> resulter)
        {
            if (!checker(target))
            {
                return target;
            }

            executer(target);

            return resulter(target);
        }

        /// <summary>
        /// ﾃﾞｨﾚｸﾄﾘを削除します。
        /// </summary>
        /// <param name="directory">削除するﾃﾞｨﾚｸﾄﾘ</param>
        private static void DeleteDirectory(string directory)
        {
            var info = new DirectoryInfo(directory);

            DeleteAttributes(info);

            info.Delete(true);
        }

        /// <summary>
        /// 指定したﾃﾞｨﾚｸﾄﾘの属性を削除します。
        /// </summary>
        /// <param name="info"></param>
        private static void DeleteAttributes(DirectoryInfo info)
        {
            info.Attributes = FileAttributes.Normal;

            info.GetFiles().AsParallel()
                .ForAll(file => file.Attributes = FileAttributes.Normal);

            info.GetDirectories().AsParallel()
                .ForAll(child => DeleteAttributes(child));
        }
    }
}
