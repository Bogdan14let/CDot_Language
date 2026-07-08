using CDot;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace CDotNew
{
    public static class Cipher
    {
        private static string key = "b37377o34474463g347da7347334n15474344773733le347t73sdhjs473";

        public static byte[] EncryptDecrypt(byte[] data)
        {
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] output = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                output[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return output;
        }
    }

    class Program
    {
        private static void HandleIcon(string configIconPath, string outputDirectory)
        {
            string iconPath = "";

            // 1. Если путь указан в конфиге — используем его
            if (!string.IsNullOrWhiteSpace(configIconPath))
            {
                iconPath = CleanPath(configIconPath);
            }
            // 2. Если поле пустое, ищем "icon.ico" в папке проекта
            else
            {
                iconPath = Path.Combine(Environment.CurrentDirectory, "icon.ico");
            }

            // 3. Копируем, если файл существует
            if (File.Exists(iconPath))
            {
                // Получаем оригинальное имя файла (например, "my_cool_icon.ico")
                string fileName = Path.GetFileName(iconPath);
                
                // Создаем путь назначения с тем же именем
                string destPath = Path.Combine(outputDirectory, fileName);
                
                File.Copy(iconPath, destPath, true);
                Console.WriteLine($"Icon copied like: {fileName}");
            }
            else if (!string.IsNullOrWhiteSpace(configIconPath))
            {
                Console.WriteLine($"Warining: Icon by path '{configIconPath}' not found.");
            }
        }

        // Уникальный маркер "CDOT" (0x544F4443 в Little Endian) для проверки хвоста файла
        private const uint MAGIC_MARKER = 0x544F4443;

        private static string CleanPath(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string result = Regex.Replace(input, @"(?<!\\)""|(?<!\\)'", "");
            result = result.Replace("\\\"", "\"").Replace("\\'", "'");
            return result;
        }

        // Вспомогательный метод для поиска .ddl файла
        private static string? ResolveDdlPath(string fName)
        {
            string exeDir = AppContext.BaseDirectory;
            string includePath = Path.Combine(exeDir, "Include", Path.GetFileName(fName));

            if (File.Exists(fName))
                return fName;
            else if (File.Exists(includePath))
                return includePath;
            else
                return null;
        }

        // Метод проверки: запущен ли файл как автономный скомпилированный EXE
        private static bool TryLoadEmbeddedScript(out string? code)
        {
            code = null;
            try
            {
                string? myPath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(myPath) || !File.Exists(myPath)) return false;

                using (FileStream fs = new FileStream(myPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 8) return false;

                    // Читаем последние 8 байт файла
                    fs.Seek(-8, SeekOrigin.End);
                    byte[] meta = new byte[8];
                    fs.Read(meta, 0, 8);

                    uint magic = BitConverter.ToUInt32(meta, 0);
                    int length = BitConverter.ToInt32(meta, 4);

                    // Если маркер совпал, значит в хвосте сидит скомпилированный скрипт!
                    if (magic == MAGIC_MARKER && length > 0 && length < fs.Length)
                    {
                        fs.Seek(-(length + 8), SeekOrigin.End);
                        byte[] scriptBytes = new byte[length];
                        fs.Read(scriptBytes, 0, length);

                        // Расшифровываем встроенный скрипт
                        byte[] decryptedBytes = Cipher.EncryptDecrypt(scriptBytes);
                        code = Encoding.UTF8.GetString(decryptedBytes);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static async Task Main(string[] args)
        {
            // 1. Если файл запущен напрямую (как игра), проверяем встроенный скрипт в хвосте
            if (TryLoadEmbeddedScript(out string? embeddedCode))
            {
                Parser cdot = new Parser();
                await cdot.Interpretator(embeddedCode!, 100, true);
                return;
            }

            string title = "";
            bool pause = false;
            bool clear = false;
            bool cur = false;
            bool ending = true;
            string version = "v3.3.9";

            if (args.Length == 0) return;

            if (args[0] == "-v" && args.Length == 1)
            {
                Console.WriteLine($"CDot version: {version}");
            }
            else if (args[0] == "-c")
            {
                if (args.Length >= 2)
                {
                    string cfgPath = Path.Combine(Environment.CurrentDirectory, "config.cfg");
                    if (File.Exists(cfgPath))
                    {
                        var config = File.ReadAllLines(cfgPath)
                            .Select(l => l.Split(';')[0].Trim())
                            .Where(l => l.Contains('='))
                            .ToDictionary(l => l.Split('=')[0].Trim(), l => l.Split('=')[1].Trim());

                        string mainFile = config.GetValueOrDefault("MainStartFile", "").Replace("\"", "").Replace("'", "");
                        string aditional = config.GetValueOrDefault("AditionalContent", "[]");

                        bool paus = config.GetValueOrDefault("Pause", "false").ToLower() == "true";
                        bool clr = config.GetValueOrDefault("Clear", "false").ToLower() == "true";
                        cur = config.GetValueOrDefault("Cursor", "false").ToLower() == "true";

                        title = config.GetValueOrDefault("Title", "");

                        bool allowCMD = config.GetValueOrDefault("AllowCMD", "true").ToLower() == "true";
                        int maxMemory = int.TryParse(config.GetValueOrDefault("MaxMemory", "100"), out int mm) ? mm : 100;
                        ending = config.GetValueOrDefault("Ending", "false").ToLower() == "true";

                        if (!string.IsNullOrWhiteSpace(title)) Console.Title = CleanPath(title);

                        string fullCode = "";

                        if (File.Exists(mainFile))
                        {
                            fullCode += File.ReadAllText(mainFile) + "\n";
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error: Main file '{mainFile}' not found!");
                            Console.ResetColor();
                            if (pause) Console.ReadKey();
                            return;
                        }

                        var match = Regex.Match(aditional, @"\[(.*)\]");
                        if (match.Success)
                        {
                            string filesRaw = match.Groups[1].Value;
                            if (!string.IsNullOrWhiteSpace(filesRaw))
                            {
                                if (!filesRaw.Contains(mainFile))
                                {
                                    string[] files = filesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string f in files)
                                    {
                                        string fName = f.Trim().Replace("\"", "").Replace("'", "");

                                        if (fName.EndsWith(".ddl"))
                                        {
                                            string? resolved = ResolveDdlPath(fName);
                                            if (resolved != null)
                                            {
                                                fullCode += File.ReadAllText(resolved) + "\n";
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"Error: Import file '{fName}' not found.");
                                                Console.ResetColor();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"Error: File '{fName}' has an invalid extension must be '.ddl'.");
                                            Console.ResetColor();
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error: File '{mainFile}' is main file.");
                                    Console.ResetColor();
                                    return;
                                }
                            }
                        }

                        // Проверяем наличие флага -p
                        bool isPublish = args.Length == 3 && args[2] == "-p";
                        string outputName = args[1];

                        // Корректируем расширение в зависимости от флага
                        if (isPublish)
                        {
                            if (!outputName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                outputName += ".exe";
                            }
                        }
                        else
                        {
                            if (!outputName.EndsWith(".dme", StringComparison.OrdinalIgnoreCase))
                            {
                                outputName += ".dme";
                            }
                        }

                        // ОБРАБОТКА СБОРКИ С АВТО-РАСШИРЕНИЯМИ

                        if (isPublish)
                        {
                            // Раз мы здесь и флаг -p активен, значит outputName гарантированно заканчивается на .exe
                            fullCode = $"mov 4m {title}\n# MaxMemory {maxMemory}\n# AllowCMD {allowCMD}\n# Pause {paus}\n# Clear {clr}\n# Cursor {cur}\n# Ending {ending}\n" + fullCode;

                            byte[] cleanBytes = Encoding.UTF8.GetBytes(fullCode);
                            byte[] encryptedBytes = Cipher.EncryptDecrypt(cleanBytes);

                            if (!Directory.Exists("output")) Directory.CreateDirectory("output");
                            string outputExePath = Path.Combine("output", outputName);

                            string currentExePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName!;
                            byte[] runnerBytes = File.ReadAllBytes(currentExePath);

                            // Защита от матрешки (отсекаем старый хвост, если он был)
                            int cleanLength = runnerBytes.Length;
                            if (runnerBytes.Length > 8)
                            {
                                uint checkMagic = BitConverter.ToUInt32(runnerBytes, runnerBytes.Length - 8);
                                int checkLen = BitConverter.ToInt32(runnerBytes, runnerBytes.Length - 4);
                                if (checkMagic == MAGIC_MARKER)
                                {
                                    cleanLength = runnerBytes.Length - (checkLen + 8);
                                }
                            }

                            // Склеиваем движок, зашифрованный байт-код и метаданные длины
                            using (FileStream fs = new FileStream(outputExePath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(runnerBytes, 0, cleanLength);
                                fs.Write(encryptedBytes, 0, encryptedBytes.Length);
                                fs.Write(BitConverter.GetBytes(MAGIC_MARKER), 0, 4);
                                fs.Write(BitConverter.GetBytes(encryptedBytes.Length), 0, 4);
                            }

                            // Получаем путь к иконке из словаря config (который у тебя уже есть выше в коде)
                            string iconFromConfig = config.GetValueOrDefault("Icon", "");

                            // Вызываем обработку иконки перед тем, как завершить метод
                            HandleIcon(iconFromConfig, "output");

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Standalone published EXE successfully compiled to: {outputExePath}");
                            Console.ResetColor();
                            return;
                        }
                        else
                        {
                            // Без флага -p файл гарантированно будет иметь расширение .dme
                            fullCode = $"mov 4m {title}\n# MaxMemory {maxMemory}\n# AllowCMD {allowCMD}\n# Pause {paus}\n# Clear {clr}\n# Cursor {cur}\n# Ending {ending}\n" + fullCode;

                            byte[] cleanBytes = Encoding.UTF8.GetBytes(fullCode);
                            byte[] encryptedBytes = Cipher.EncryptDecrypt(cleanBytes);

                            if (!Directory.Exists("output")) Directory.CreateDirectory("output");
                            File.WriteAllBytes($"output/{outputName}", encryptedBytes);

                            // Получаем путь к иконке из словаря config (который у тебя уже есть выше в коде)
                            string iconFromConfig = config.GetValueOrDefault("Icon", "");

                            // Вызываем обработку иконки перед тем, как завершить метод
                            HandleIcon(iconFromConfig, "output");

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Compiled script file created at: output/{outputName} (Run it via interpreter or rebuild with -p to get standalone EXE)");
                            Console.ResetColor();
                            return;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: config.cfg not found!");
                        Console.ResetColor();
                    }
                }
            }
            else if (args[0] == "new" || args[0] == "run")
            {
                if (args[0] == "run")
                {
                    if (args.Length == 1)
                    {
                        string cfgPath = Path.Combine(Environment.CurrentDirectory, "config.cfg");
                        if (File.Exists(cfgPath))
                        {
                            var config = File.ReadAllLines(cfgPath)
                                .Select(l => l.Split(';')[0].Trim())
                                .Where(l => l.Contains('='))
                                .ToDictionary(l => l.Split('=')[0].Trim(), l => l.Split('=')[1].Trim());

                            string mainFile = config.GetValueOrDefault("MainStartFile", "").Replace("\"", "").Replace("'", "");
                            string aditional = config.GetValueOrDefault("AditionalContent", "[]");
                            pause = config.GetValueOrDefault("Pause", "false").ToLower() == "true";
                            clear = config.GetValueOrDefault("Clear", "false").ToLower() == "true";
                            cur = config.GetValueOrDefault("Cursor", "false").ToLower() == "true";
                            ending = config.GetValueOrDefault("Ending", "false").ToLower() == "true";

                            title = config.GetValueOrDefault("Title", "");

                            bool allowCMD = config.GetValueOrDefault("AllowCMD", "true").ToLower() == "true";
                            int maxMemory = int.TryParse(config.GetValueOrDefault("MaxMemory", "100"), out int mm) ? mm : 100;

                            if (!string.IsNullOrWhiteSpace(title)) Console.Title = CleanPath(title);

                            string fullCode = "";

                            if (File.Exists(mainFile))
                            {
                                fullCode += File.ReadAllText(mainFile) + "\n";
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Error: Main file '{mainFile}' not found!");
                                Console.ResetColor();
                                if (pause) Console.ReadKey();
                                return;
                            }

                            var match = Regex.Match(aditional, @"\[(.*)\]");
                            if (match.Success)
                            {
                                string filesRaw = match.Groups[1].Value;
                                if (!string.IsNullOrWhiteSpace(filesRaw))
                                {
                                    if (!filesRaw.Contains(mainFile))
                                    {
                                        if (!mainFile.EndsWith(".ddl"))
                                        {
                                            string[] files = filesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                            foreach (string f in files)
                                            {
                                                string fName = f.Trim().Replace("\"", "").Replace("'", "");

                                                if (fName.EndsWith(".ddl"))
                                                {
                                                    string? resolved = ResolveDdlPath(fName);
                                                    if (resolved != null)
                                                    {
                                                        fullCode += File.ReadAllText(resolved) + "\n";
                                                    }
                                                    else
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine($"Error: Import file '{fName}' not found.");
                                                        Console.ResetColor();
                                                        if (pause) Console.ReadKey();
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"Error: File '{fName}' has an invalid extension must be '.ddl'.");
                                                    Console.ResetColor();
                                                    if (pause) Console.ReadKey();
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"Error: File '{mainFile}' has an invalid extension must be '.cdt/cdot'.");
                                            Console.ResetColor();
                                            if (pause) Console.ReadKey();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"Error: File '{mainFile}' is main file.");
                                        Console.ResetColor();
                                        if (pause) Console.ReadKey();
                                        return;
                                    }
                                }
                            }

                            Parser cdot = new Parser();
                            await cdot.Interpretator(fullCode, maxMemory, allowCMD, clear, cur, ending: ending);
                            if (pause) Console.ReadKey();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error: config.cfg not found!");
                            Console.ResetColor();
                            if (pause) Console.ReadKey();
                        }
                    }
                }
                else
                {
                    if (args.Length == 2)
                    {
                        string path = CleanPath(args[1]);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                            string fileName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                            char symbol = path.Contains('/') ? '/' : '\\';

                            string fullPath = path.EndsWith(symbol.ToString()) ? path + "main.cdt" : path + symbol + "main.cdt";
                            string fullCfgPath = path.EndsWith(symbol.ToString()) ? path + "config.cfg" : path + symbol + "config.cfg";

                            using (File.Create(fullPath)) { }

                            string cgfText = $"""
                            ; Main setings
                            MainStartFile = "main.cdt"
                            AditionalContent = []
                            Pause = false
                            Clear = false

                            ; Window setings
                            Title = ""
                            Icon = ""

                            ; Language setings
                            AllowCMD = true
                            MaxMemory = 100
                            Cursor = true
                            Ending = true
                            """;
                            File.WriteAllText(fullCfgPath, cgfText);
                        }
                    }
                }
            }
            else
            {
                Parser cdot = new Parser();
                string path = CleanPath(args[0]);

                if (path.EndsWith(".cdt") || path.EndsWith(".cdot") || path.EndsWith(".dme") || path.EndsWith(".exe"))
                {
                    string fileName = Path.GetFileName(path);
                    string exeDir = AppContext.BaseDirectory;
                    string includePath = Path.Combine(exeDir, "Include", fileName);

                    string finalPath = "";

                    if (File.Exists(path))
                    {
                        finalPath = path;
                    }
                    else if (File.Exists(includePath))
                    {
                        finalPath = includePath;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: File not found! Checked:\n  - {path}\n  - {includePath}");
                        Console.ResetColor();
                        if (pause) Console.ReadKey();
                        return;
                    }

                    if (finalPath.EndsWith(".dme") || finalPath.EndsWith(".exe"))
                    {
                        byte[] encryptedBytes = File.ReadAllBytes(finalPath);
                        byte[] decryptedBytes = Cipher.EncryptDecrypt(encryptedBytes);
                        string cleanCode = System.Text.Encoding.UTF8.GetString(decryptedBytes);

                        await cdot.Interpretator(cleanCode, 100, true);
                    }
                    else
                    {
                        string code = File.ReadAllText(finalPath);
                        await cdot.Interpretator(code, 100, true);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: File extension must be .cdt/.cdot, .dme or .exe!");
                    Console.ResetColor();
                    if (pause) Console.ReadKey();
                }
            }
        }
    }
}