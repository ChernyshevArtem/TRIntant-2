using System;
using System.Collections.Generic;

namespace TRIntant2
{
    class Program
    {
        public readonly static Dictionary<string, string> ConsoleCommand = new Dictionary<string, string>()
        {
            { "cmd: -new_profile", "Заполнить новую анкет" },
            { "cmd: -statistics", "Показать статистику всех заполненных анкет" },
            { "cmd: -save", "Сохранить заполненную анкет" },
            { "cmd: -goto_question <Номер вопроса>", "Вернуться к указанному вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)" },
            { "cmd: -goto_prev_question", "Вернуться к предыдущему вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)" },
            { "cmd: -restart_profile", "Заполнить анкету заново (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)" },
            { "cmd: -find <Имя файла анкеты>", "Найти анкету и показать данные анкеты в консоль" },
            { "cmd: -delete <Имя файла анкеты>", "Удалить указанную анкету" },
            { "cmd: -list <Текущая страница – если параметр отсутвует, то первая страница>", "Показать список названий файлов всех сохранённых анкет с пагинацией по 10" },
            { "cmd: -list_today", "Показать список названий файлов всех сохранённых анкет, созданных сегодня" },
            { "cmd: -zip <Имя файла анкеты> <Путь для сохранения архива>", "Запаковать указанную анкету в архив и сохранить архив по указанному пути. Если указать только имя архив сохраниться в каталоге приложения" },
            { "cmd: -help", "Показать список доступных команд с описанием" },
            { "cmd: -exit", "Выйти из приложения / Режима заполнения анкеты" },
        };
        public static Questionnaire questionnaire = new Questionnaire();

        private static DataProcessing dataProcessing = new DataProcessing();

        static void Main(string[] args)
        {
            Console.WriteLine("Выберите действие:");
            UserResponseHandler.CommandChek(Console.ReadLine().Trim());

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                UserResponseHandler.CommandChek(Console.ReadLine().Trim());
            }
        }

        static public void СommandInput(string command)
        {
            string commandWithoutKeys = UserResponseHandler.exextractCommand.Match(command).ToString();
            switch (commandWithoutKeys)
            {
                case "cmd: -new_profile":
                    if (questionnaire.IsNull())
                    {
                        UserResponseHandler filling = new UserResponseHandler(questionnaire);
                        filling.FillOutQuestionnaire();
                    }
                    else
                    {
                        Console.WriteLine("Данные о уже заполненной анкете будут потеряны\nСохранить заполненную анкету: да / нет");
                        while (true)
                        {
                            string answer = Console.ReadLine();
                            if (answer == "да")
                            {
                                СommandInput("cmd: -save");
                                СommandInput("cmd: -new_profile");
                                return;
                            }
                            else if (answer == "нет")
                            {
                                questionnaire = new Questionnaire();
                                Console.WriteLine("Прошлая анкета была удалена");
                                СommandInput("cmd: -new_profile");
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Введите да или нет");
                            }
                        }
                    }
                    break;
                case "cmd: -save":
                    if (!questionnaire.IsNull())
                    {
                        questionnaire.Sent += dataProcessing.SaveToTXT;
                        questionnaire.SendData();
                        questionnaire.Sent -= dataProcessing.SaveToTXT;
                        questionnaire = new Questionnaire();
                    }
                    else
                    {
                        Console.WriteLine("Анкета не заполнена");
                    }
                    break;
                case "cmd: -delete":
                    string fileNameDelete;
                    try
                    {
                        fileNameDelete = UserResponseHandler.extractOneKey.Match(command).Groups[1].ToString();
                        if (string.IsNullOrWhiteSpace(fileNameDelete))
                        {
                            throw new Exception("Необходимо ввести имя файла");
                        }
                        dataProcessing.DeleteTXT(fileNameDelete);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "cmd: -find":
                    string fileNameFind;
                    try
                    {
                        fileNameFind = UserResponseHandler.extractOneKey.Match(command).Groups[1].ToString();
                        if (string.IsNullOrWhiteSpace(fileNameFind))
                        {
                            throw new Exception("Необходимо ввести имя файла");
                        }
                        dataProcessing.FindTXT(fileNameFind);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "cmd: -list_today":
                    dataProcessing.PrintNameFilesCreatedToday();
                    break;
                case "cmd: -list":
                    try
                    {
                        string Key = UserResponseHandler.extractOneKey.Match(command).Groups[1].ToString();
                        Console.WriteLine(Key);
                        if (string.IsNullOrEmpty(Key))
                        {
                            Key = "0";
                        }
                        if (int.TryParse(Key, out int pageNumber))
                        {
                            dataProcessing.PrintNameAllFiles(pageNumber);
                        }
                        else
                        {
                            throw new Exception("Номер страницы задан не корректно, необходимо ввести цифры");

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "cmd: -zip":
                    string fileName = UserResponseHandler.extractTwoKeys.Match(command).Groups[1].ToString();
                    string archivePath = UserResponseHandler.extractTwoKeys.Match(command).Groups[2].ToString();
                    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(archivePath))
                    {
                        Console.WriteLine("Не указаны все параметры");
                    }
                    else
                    {
                        dataProcessing.Archiving(fileName, archivePath);
                    }
                    break;
                case "cmd: -statistics":
                    dataProcessing.Statistics();
                    break;
                case "cmd: -help":
                    Console.WriteLine();
                    foreach (var Command in ConsoleCommand)
                    {
                        Console.WriteLine($"{Command.Key} - {Command.Value}");
                    }
                    break;
                case "cmd: -exit":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Команда не найдена.\nЧтобы ознакомиться со списком доступных команд введите cmd: -help");
                    break;
            }
        }
    }
}
