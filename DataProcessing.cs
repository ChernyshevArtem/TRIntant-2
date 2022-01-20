using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace TRIntant2
{
    class DataProcessing
    {
        private string _pathDirectory;
        private string[] _pathsAllSavedFiles;
        private DirectoryInfo _directoryInfo;
        private static List<Questionnaire> _questionnaires = new List<Questionnaire>();
        private readonly static Regex _extractФИО = new Regex(@"(?:1\. ФИО: <)([\D]*)(?:>)");
        private readonly static Regex _extractAllQuestionnaire = new Regex(@"(?:1\. ФИО: <)(.+)(?:>\n)" +
                                                                           @"(?:2\. Дата рождения: <)(.+)(?:>\n)" +
                                                                           @"(?:3\. Любимый язык программирования: <)(.+)(?:>\n)" +
                                                                           @"(?:4\. Опыт программирования на указанном языке: <)(.+)(?:>\n)" +
                                                                           @"(?:5\. Мобильный телефон: <)(.+)(?:>\n)" +
                                                                           @"(?:Анкета заполнена: <)(.+)(?:>.*)");

        public DataProcessing()
        {
            _pathDirectory = Directory.GetCurrentDirectory() + @"\Анкеты";
            _directoryInfo = new DirectoryInfo(_pathDirectory);
        }
        public void SaveToTXT(string date)
        {
            GreateDirectoryQuestionnaire();

            string pathSave = Path.ChangeExtension(Path.Combine(_pathDirectory, _extractФИО.Match(date).Groups[1].ToString()), ".txt");

            pathSave = CheckPathSave(pathSave);

            File.WriteAllText(pathSave, date);

            Console.WriteLine("Анкета сохранена");
        }
        public void DeleteTXT(string name)
        {
            string path = CheckExsistTXTFileAndReturnPath(name);
            if (path != null)
            {
                File.Delete(path);
                Console.WriteLine($"\nУдален файл с названием: {name}\n");
            }
        }
        public void FindTXT(string name)
        {
            string path = CheckExsistTXTFileAndReturnPath(name);
            if (path != null)
            {
                Console.WriteLine("\n" + File.ReadAllText(path) + "\n");
            }
        }
        public void Statistics()
        {
            ReadAllFileTXT();

            try
            {
                if (_questionnaires.Count == 0)
                {
                    throw new Exception("Не найдено заполненных анкет");
                }
                else
                {
                    Console.WriteLine($"1. Средний возраст всех опрошенных: <{CalculationAverageAge()}>\n" +
                                      $"2. Самый популярный язык программирования: <{FindMostPopularProgrammingLanguage()}>\n" +
                                      $"3. Самый опытный программист: <{FindFullNameWithMaxExperienced()}>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
        public void PrintNameFilesCreatedToday()
        {
            GreateDirectoryQuestionnaire();
            var todayFiles = Directory.GetFiles(_pathDirectory, "*.txt").Where(x => new FileInfo(x).CreationTime.Date == DateTime.Today.Date);
            if (todayFiles.Count() == 0)
            {
                Console.WriteLine("Файлов созданных сегодня не найдено");
            }
            else
            {
                Console.WriteLine("Названия файлов, созданных сегодня:");
                Console.WriteLine(string.Join("\n", (todayFiles.Select(f => Path.GetFileNameWithoutExtension(f)))));
            }
        }
        public void PrintNameAllFiles(int pageNumber)
        {
            CheckChangePathFiles();
            if (_pathsAllSavedFiles.Count() == 0)
            {
                Console.WriteLine("Файлов не найдено");
                return;
            }
            if (pageNumber * 10 > _pathsAllSavedFiles.Count())
            {
                pageNumber = _pathsAllSavedFiles.Count() / 10;
                Console.WriteLine("Показан последний номер страницы");
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            Console.WriteLine($"Страница {pageNumber}");

            for (int i = pageNumber * 10 - 10; i < pageNumber * 10; i++)
            {
                if (i <= _pathsAllSavedFiles.Count() - 1)
                {
                    Console.WriteLine(Path.GetFileNameWithoutExtension(_pathsAllSavedFiles[i]));
                }
                else
                {
                    return;
                }
            }
        }
        public async void Archiving(string fileName, string archivePath)
        {
            int cursorTop = Console.CursorTop + 1;
            byte[] allBytes;
            double copmressBytes = 0;
            int procent;
            string questionnairePath = CheckExsistTXTFileAndReturnPath(fileName);
            if (questionnairePath != null)
            {
                allBytes = Encoding.Default.GetBytes(File.ReadAllText(questionnairePath));
            }
            else
            {
                return;
            }
            try
            {
                using (var inpStr = new MemoryStream(allBytes))
                {
                    using (FileStream outStr = File.Create(archivePath))
                    {
                        using (var zipStr = new GZipStream(outStr, CompressionMode.Compress))
                        {
                            var buffer = new byte[32];
                            int count = 0;
                            while ((count = inpStr.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                copmressBytes += count;
                                await zipStr.WriteAsync(buffer, 0, count);
                                procent = Convert.ToInt32(copmressBytes / allBytes.Length * 100);
                                int cursorTopNow = Console.CursorTop;
                                int cursorLestNow = Console.CursorLeft;
                                Console.SetCursorPosition(0, cursorTop);
                                Console.Write($"Прогресс архивации: {procent} %");
                                Console.SetCursorPosition(cursorLestNow, cursorTopNow);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private string CheckPathSave(string pathSave)
        {
            if (File.Exists(pathSave))
            {
                int countFile = 2;
                string newPath = pathSave;
                while (true)
                {
                    if (File.Exists(newPath))
                    {
                        newPath = Path.Combine(Path.GetDirectoryName(pathSave),
                                  Path.GetFileNameWithoutExtension(pathSave) + $"({countFile++})" + Path.GetExtension(pathSave));
                    }
                    else
                    {
                        return newPath;
                    }
                }
            }
            else
            {
                return pathSave;
            }
        }
        private void GreateDirectoryQuestionnaire()
        {
            if (!_directoryInfo.Exists)
            {
                try
                {
                    _directoryInfo.Create();
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                return;
            }
        }
        private string CheckExsistTXTFileAndReturnPath(string name)
        {
            string path = Path.ChangeExtension(Path.Combine(_pathDirectory, name), ".txt");
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                Console.WriteLine("Файла с таким названием не найдено");
                return null;
            }
        }
        private void ReadAllFileTXT()
        {
            GreateDirectoryQuestionnaire();

            if (CheckChangePathFiles())
            {
                _questionnaires = new List<Questionnaire>();
                foreach (var filePath in _pathsAllSavedFiles)
                {
                    try
                    {
                        string dateQuestionnaire = File.ReadAllText(filePath);
                        GroupCollection Parameters = _extractAllQuestionnaire.Match(dateQuestionnaire).Groups;

                        Questionnaire questionnaire = new Questionnaire(Parameters[1].ToString(), Convert.ToDateTime(Parameters[2].ToString()),
                                                          Parameters[3].ToString(), Convert.ToInt32(Parameters[4].ToString()),
                                                          Parameters[5].ToString(), Convert.ToDateTime(Parameters[6].ToString()));

                        _questionnaires.Add(questionnaire);
                    }
                    catch
                    {
                        Console.WriteLine($"Ошибка при чтении файла: {filePath}\n. Файл был пропущен");
                    }
                }
            }
            else
            {
                return;
            }
        }
        private bool CheckChangePathFiles()
        {
            try
            {
                string[] pathsFiles = Directory.GetFiles(_pathDirectory, "*.txt");
                if (_pathsAllSavedFiles == null || !pathsFiles.SequenceEqual(_pathsAllSavedFiles))
                {
                    _pathsAllSavedFiles = pathsFiles;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private string CalculationAverageAge()
        {
            int averageAge;
            int totalAge = 0;
            int Age;
            foreach (Questionnaire questionnaire in _questionnaires)
            {
                Age = DateTime.Now.Year - questionnaire.BirthDate.Year;
                if (DateTime.Now.DayOfYear < questionnaire.BirthDate.DayOfYear)
                    Age++;
                totalAge += Age;
            }
            averageAge = totalAge / _questionnaires.Count;

            if (averageAge == 11 || averageAge == 12 || averageAge == 13 || averageAge == 14)
            {
                return $"{averageAge} лет";
            }
            else if (averageAge % 10 == 1)
            {
                return $"{averageAge} год";
            }
            else if (averageAge % 10 == 2 || averageAge % 10 == 3 || averageAge % 10 == 4)
            {
                return $"{averageAge} года";
            }
            else
            {
                return $"{averageAge} лет";
            }
        }
        private string FindFullNameWithMaxExperienced()
        {
            var fullNames = _questionnaires.Where(g => g.ProgrammingExperience == _questionnaires.Max(e => e.ProgrammingExperience)).Select(f => f.FullName).ToList();

            return string.Join(", ", fullNames);
        }
        private string FindMostPopularProgrammingLanguage()
        {
            var mostPopularProgrammingLanguage = _questionnaires.GroupBy(f => f.FavoriteLanguage).Where(g => g.Count() > 1).OrderByDescending(g => g.Count()).Select(g => g.Key).ToList();

            return string.Join(", ", mostPopularProgrammingLanguage);
        }
    }
}
