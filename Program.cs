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
        private const string ShukusenPath = @"..\Shukusen\ShukuSen.exe";

        private const string ArrangayPath = @"..\Arrangay\Arrangay.exe";

        private const string SevenZaPath = @"7za.exe";

        static void Main(string[] args)
        {
            // 作業ﾃﾞｨﾚｸﾄﾘを取得
            var work = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            // ｵﾌﾟｼｮﾝ
            var isExeShukusen = true;

            // ｵﾌﾟｼｮﾝを選択
            Console.WriteLine("起動ｵﾌﾟｼｮﾝを選択してください。");
            Console.WriteLine("0: 全て実行する。(ﾃﾞﾌｫﾙﾄ)");
            Console.WriteLine("1: 画像縮小をｽｷｯﾌﾟする。");

            // ｵﾌﾟｼｮﾝ読取&判定
            if (!CheckOptions(Console.ReadLine(), ref isExeShukusen))
            {
                return;
            }

            try
            {
                // 実行ﾊﾟﾗﾒｰﾀに対して処理実行
                args.AsParallel()
                    .ForAll(arg => Execute(work, arg, isExeShukusen));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }

        }

        /// <summary>
        /// 入力されたｵﾌﾟｼｮﾝの判定を行います。
        /// </summary>
        /// <param name="o">Console.ReadLine</param>
        /// <param name="isExeShukusen">縮小専用を実行するかどうか(参照)</param>
        /// <returns>true: OK / false: NG</returns>
        private static bool CheckOptions(string o, ref bool isExeShukusen)
        {
            switch (o)
            {
                case "":
                case "0":
                    // 全て実行する
                    // (空文字ならﾃﾞﾌｫﾙﾄ)
                    isExeShukusen = true;
                    return true;
                case "1":
                    // 画像縮小をｽｷｯﾌﾟする。
                    isExeShukusen = false;
                    return true;
                default:
                    // ｴﾗｰ
                    Console.WriteLine("認識できないｵﾌﾟｼｮﾝが指定されたのでｱﾌﾟﾘｹｰｼｮﾝを終了します。");
                    return false;
            }
        }

        /// <summary>
        /// 実行ﾊﾟﾗﾒｰﾀに対して処理を実行します。
        /// </summary>
        /// <param name="work">作業ﾃﾞｨﾚｸﾄﾘ</param>
        /// <param name="target">処理対象ﾌｧｲﾙ(ﾌｫﾙﾀﾞ)</param>
        /// <param name="isExeShukusen">縮小専用を実行するかどうか</param>
        private static void Execute(string work, string target, bool isExeShukusen)
        {
            Console.WriteLine($"* 解凍処理を開始します");

            // ﾌｧｲﾙ名→ﾌｫﾙﾀﾞ名変更ﾃﾞﾘｹﾞｰﾄ
            Func<string, string> to_directory = (file) => 
                Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Trim(' ', '　', '.'));

            // 圧縮ﾌｧｲﾙ→ﾌｫﾙﾀﾞ
            target = Execute(target,
                (file) => File.Exists(file),
                (file) => StartProcess(work, SevenZaPath, $"x -y -r -aoa \"{file}\" -o\"{to_directory(file)}\""),
                (file) => file
            );

            Console.WriteLine($"* 解凍したﾌｧｲﾙを削除します");
            Console.WriteLine($"file: {target}");

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
                (file) => StartProcess(work, Path.Combine(work, ArrangayPath), $"\"{file}\""),
                (file) => file
            );

            Console.WriteLine($"* ﾌｧｲﾙ内の画像を縮小します");

            // 縮小専用
            target = Execute(target,
                (file) => isExeShukusen && Directory.Exists(file) && File.Exists(Path.Combine(work, ShukusenPath)),
                (file) => StartShukusen(work, file),
                (file) => file
            );

            Console.WriteLine($"* ﾌｧｲﾙ内を整形します");

            // ﾌｧｲﾙ内整形
            target = Execute(target,
                (file) => Directory.Exists(file),
                (file) => StartProcess(work, Path.Combine(work, ArrangayPath), $"\"{file}\""),
                (file) => file
            );

            Console.WriteLine($"* 圧縮処理を開始します");

            // ﾌｫﾙﾀﾞ→圧縮ﾌｧｲﾙ
            target = Execute(target,
                (file) => Directory.Exists(file),
                (file) => StartProcess(work, SevenZaPath, $"a -mmt=on -y -r \"{file}.zip\"  \"{file}\""),
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

        /// <summary>
        /// ﾌﾟﾛｾｽを実行します。
        /// </summary>
        /// <param name="work">作業ﾃﾞｨﾚｸﾄﾘ</param>
        /// <param name="file">実行するﾌﾟﾛｾｽのﾊﾟｽ</param>
        /// <param name="argument">ﾌﾟﾛｾｽに渡す実行引数</param>
        private static void StartProcess(string work, string file, string argument)
        {
            Console.WriteLine($"work: {work}");
            Console.WriteLine($"exe : {file}");
            Console.WriteLine($"arg : {argument}");

            var process = new ProcessStartInfo();

            process.WorkingDirectory = work;
            process.FileName = file;
            process.Arguments = argument;
            process.UseShellExecute = false;
            process.CreateNoWindow = true;
            process.ErrorDialog = true;
            process.RedirectStandardError = true;

            Process.Start(process).WaitForExit();
        }

        /// <summary>
        /// 縮小専用を実行します。
        /// </summary>
        /// <param name="work">作業ﾃﾞｨﾚｸﾄﾘ</param>
        /// <param name="directory">処理対象ﾃﾞｨﾚｸﾄﾘ</param>
        private static void StartShukusen(string work, string directory)
        {
            Chunk(Directory.GetFiles(directory)).AsParallel().ForAll(files =>
            {
                var arg = string.Join(" ", files.Select(file => $"\"{file}\""));

                // 縮小専用を実行
                StartProcess(work, Path.Combine(work, ShukusenPath), arg);

                files.AsParallel().ForAll(src =>
                {
                    // 縮小後ﾌｧｲﾙ名を作成
                    var dst = Path.Combine(
                        Path.GetDirectoryName(src),
                        $"s-{Path.GetFileNameWithoutExtension(src)}.jpg"
                    );

                    var fi = new FileInfo(dst);

                    if (fi.Exists && fi.Length != 0)
                    {
                        // 縮小が成功していたら元ﾌｧｲﾙを削除
                        File.Delete(src);
                    }
                    else
                    {
                        // 縮小が失敗していて、且つ、縮小後ﾌｧｲﾙが残っていたら後ﾌｧｲﾙを削除
                        if (fi.Exists) fi.Delete();

                        // 縮小前のﾌｧｲﾙをﾘﾈｰﾑ
                        File.Move(src, dst);
                    }
                });
            });
            //foreach (var files in Chunk(Directory.GetFiles(directory)))
            //{
            //    var arg = string.Join(" ", files.Select(file => $"\"{file}\""));

            //    // 縮小専用を実行
            //    StartProcess(work, Path.Combine(work, ShukusenPath), arg);

            //    files.AsParallel().ForAll(src =>
            //    {
            //        // 縮小後ﾌｧｲﾙ名を作成
            //        var dst = Path.Combine(
            //            Path.GetDirectoryName(src),
            //            $"s-{Path.GetFileNameWithoutExtension(src)}.jpg"
            //        );

            //        var fi = new FileInfo(dst);

            //        if (fi.Exists && fi.Length != 0)
            //        {
            //            // 縮小が成功していたら元ﾌｧｲﾙを削除
            //            File.Delete(src);
            //        }
            //        else
            //        {
            //            // 縮小が失敗していて、且つ、縮小後ﾌｧｲﾙが残っていたら後ﾌｧｲﾙを削除
            //            if (fi.Exists) fi.Delete();

            //            // 縮小前のﾌｧｲﾙをﾘﾈｰﾑ
            //            File.Move(src, dst);
            //        }
            //    });
            //}
        }

        /// <summary>
        /// 処理を実行する
        /// </summary>
        /// <param name="target">対象ﾌｧｲﾙ(ﾌｫﾙﾀﾞ)</param>
        /// <param name="checker">処理を実行するか確認するﾃﾞﾘｹﾞｰﾄ</param>
        /// <param name="executer">処理を実行するﾃﾞﾘｹﾞｰﾄ</param>
        /// <param name="resulter">次に実行するﾌｧｲﾙ(ﾌｫﾙﾀﾞ)を取得するﾃﾞﾘｹﾞｰﾄ</param>
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

        /// <summary>
        /// 処理対象ﾌｧｲﾙの配列を一度に処理できる範囲で分割します。
        /// </summary>
        /// <param name="source">ｿｰｽ配列</param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<string>> Chunk(IEnumerable<string> source)
        {
            var target = new List<string>();

            foreach (var file in source)
            {
                if (8000 < target.Sum(a => a.Length + 3))
                {
                    // ｺﾏﾝﾄﾞﾗｲﾝ引数の上限は8192文字なので、それを超えない範囲で配列を分割する。
                    yield return target;

                    target.Clear();
                }

                target.Add(file);
            }

            if (target.Any())
            {
                yield return target;
            }
        }

    }
}
