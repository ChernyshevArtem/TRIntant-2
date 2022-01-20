using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TRIntant2
{
    class UserResponseHandler
    {
        private Questionnaire _questionnaire;
        private string[,] _questions;
        private static bool _fillingMode = false;

        private readonly static Regex _checkCommand = new Regex(@"(^cmd: -\S+$)|(^cmd: -\S+\s<[^<>]+>$)|(^cmd: -\S+\s<[^<>]*>\s<[^<>]+>$)");
        private readonly static Regex _checkDate = new Regex(@"\d\d\.\d\d.\d\d\d\d");
        private readonly static Regex _checkPhoneNumber = new Regex(@"^[\d\W]+$");
        private readonly static Regex _checkFullName = new Regex(@"^([^\d\W]|[\s])+$");

        public readonly static Regex exextractCommand = new Regex(@"^(?:cmd: -)(\S+)");
        public readonly static Regex extractOneKey = new Regex(@"(?:<)([^<>]+)(?:>$)");
        public readonly static Regex extractTwoKeys = new Regex(@"(?:<)([^<>]+)(?:>\s<)([^<>]+)(?:>)");

        public UserResponseHandler(Questionnaire questionnaire)
        {
            this._questionnaire = questionnaire;
            _fillingMode = true;
            _questions = new string[5, 2];
            _questions[0, 0] = "1. ФИО:";
            _questions[1, 0] = "2. Дата рождения: (дд.мм.гггг)";
            _questions[2, 0] = "3. Любимый язык программирования:";
            _questions[3, 0] = "4. Опыт программирования на указанном языке:";
            _questions[4, 0] = "5. Мобильный телефон:";
        }

        public void FillOutQuestionnaire()
        {
            for (int i = 0; i < _questions.GetLength(0); i++)
            {
                if (_fillingMode)
                {
                    i = GetAnswer(i);
                }
            }
            if (_fillingMode)
            {
                if (CheckFillOutQuestionnaire())
                {
                    _fillingMode = false;
                    _questionnaire.DateFillingOut = DateTime.Today.Date;
                    Console.WriteLine("\nАнкета заполнена");
                }
            }
        }
        private int GetAnswer(int questionNumber)
        {
            string answer;
            Console.WriteLine("\n" + _questions[questionNumber, 0]);
            answer = Console.ReadLine();
            if (CommandChek(answer.Trim()) == true)
            {
                return ComandChekFillingMode(answer.Trim(), questionNumber);
            }
            else
            {
                if (!InsertInQuestionnaire(answer.Trim(), questionNumber))
                {
                    GetAnswer(questionNumber);
                    return questionNumber;
                }
                else
                {
                    _questions[questionNumber, 1] = answer;
                    return questionNumber;
                }
            }
        }
        static public bool CommandChek(string answer)
        {
            if (answer.StartsWith("cmd: -"))
            {
                string command = exextractCommand.Match(answer).ToString();

                if (UserResponseHandler._checkCommand.IsMatch(answer) &&
                    Program.ConsoleCommand.Keys.Any(u => u.Contains(command)) && _fillingMode == false)
                {
                    Program.СommandInput(answer);
                    return true;
                }
                else if (UserResponseHandler._checkCommand.IsMatch(answer) &&
                         Program.ConsoleCommand.Keys.Any(u => u.Contains(command)) && _fillingMode == true)
                {
                    return true;
                }
                else if (UserResponseHandler._checkCommand.IsMatch(answer) &&
                         !Program.ConsoleCommand.Keys.Any(u => u.Contains(command)) && _fillingMode == true)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("\nКоманда не найдена.\nЧтобы ознакомиться со списком доступных команд введите: cmd: -help");
                    return false;
                }
            }
            else if (_fillingMode == true)
            {
                return false;
            }
            else if (_fillingMode == false)
            {
                Console.WriteLine("\nНеобходимо ввести команду.\nЧтобы ознакомиться со списком доступных команд введите: cmd: -help");
                return false;
            }
            return false;
        }
        private int ComandChekFillingMode(string answer, int questionNumber)
        {
            switch (exextractCommand.Match(answer).ToString())
            {
                case "cmd: -exit":
                    _fillingMode = false;
                    return questionNumber - 1;
                case "cmd: -help":
                    Program.СommandInput(answer);
                    return questionNumber - 1;
                case "cmd: -goto_prev_question":
                    if (questionNumber == 0)
                    {
                        Console.WriteLine("Вы итак на первом вопросе");
                        return questionNumber - 1;
                    }
                    return questionNumber - 2;
                case "cmd: -goto_question":
                    try
                    {
                        int newQuestionNumber = Convert.ToInt32(extractOneKey.Match(answer).Groups[1].ToString());
                        if (newQuestionNumber >= 1 && newQuestionNumber <= 5)
                        {
                            if (newQuestionNumber >= questionNumber + 1)
                            {
                                Console.WriteLine("Вы еще не заполнили этот вопрос");
                                return questionNumber - 1;
                            }
                            else
                            {
                                ChangeАnswer(newQuestionNumber - 1);
                                return questionNumber - 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Можно выбрать только номера от 1 до 5");
                            return questionNumber - 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return questionNumber - 1;
                    }
                case "cmd: -restart_profile":
                    _questionnaire = new Questionnaire();
                    return -1;
                default:
                    Console.WriteLine("\nКоманда недосупна.\nЧтобы выйти из режима заполнения анкеты введите команду: cmd: -exit");
                    return questionNumber - 1;
            }
        }
        private bool InsertInQuestionnaire(string answer, int questionNumber)
        {
            try
            {
                switch (questionNumber)
                {
                    case 0:
                        if (_checkFullName.IsMatch(answer))
                        {
                            _questionnaire.FullName = answer;
                            break;
                        }
                        else
                        {
                            throw new Exception("ФИО введен в неверном формате");
                        }
                    case 1:
                        if (_checkDate.IsMatch(answer))
                        {
                            _questionnaire.BirthDate = Convert.ToDateTime(answer).Date;
                            break;
                        }
                        else
                        {
                            throw new Exception("Дата введена в неверном формате");
                        }
                    case 2:
                        _questionnaire.FavoriteLanguage = answer;
                        break;
                    case 3:
                        _questionnaire.ProgrammingExperience = Convert.ToInt32(answer);
                        break;
                    case 4:
                        if (_checkPhoneNumber.IsMatch(answer))
                        {
                            _questionnaire.PhoneNumber = answer;
                            break;
                        }
                        else
                        {
                            throw new Exception("Номер введен в неверном формате");
                        }
                        
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        private void ChangeАnswer(int questionNumber)
        {
            Console.WriteLine("\nИзменение вопроса:");
            GetAnswer(questionNumber);
        }
        private bool CheckFillOutQuestionnaire()
        {
            for (int i = 0; i < _questions.GetLength(0); i++)
            {
                if (String.IsNullOrWhiteSpace(_questions[i, 1]))
                {
                    Console.WriteLine($"\nЗаполните вопрос {i + 1}");
                    GetAnswer(i);
                }
            }
            return true;
        }
    }
}

