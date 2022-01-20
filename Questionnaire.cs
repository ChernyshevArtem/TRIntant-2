using System;
using System.Collections.Generic;

namespace TRIntant2
{
    class Questionnaire
    {
        private readonly List<string> _allowedLanguages = new List<string>() { "PHP", "JavaScript", "C", "C++", "Java", "C#", "Python", "Ruby" };

        public event OptionDateProcessing Sent;
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateFillingOut { get; set; }

        private string _favoriteLanguage;
        public string FavoriteLanguage
        {
            get { return _favoriteLanguage; }
            set
            {
                if (_allowedLanguages.Contains(value))
                {
                    _favoriteLanguage = value;
                }
                else
                {
                    throw new Exception($"Язык не найден");
                }
            }
        }

        private int _programmingExperience;
        public int ProgrammingExperience
        {
            get
            {
                return _programmingExperience;
            }
            set 
            {
                if (value >= 0 && value <= 100)
                {
                    _programmingExperience = value;
                }
                else
                {
                    throw new Exception($"Вы ввели недопустимое значение");
                }
            }
        }

        public Questionnaire() { }
        public Questionnaire(string FullName, DateTime BirthDate, string FavoriteLanguage, int ProgrammingExperience, string PhoneNumber, DateTime DateFillingOut)
        {
            this.FullName = FullName;
            this.BirthDate = BirthDate;
            this.FavoriteLanguage = FavoriteLanguage;
            this.ProgrammingExperience = ProgrammingExperience;
            this.PhoneNumber = PhoneNumber;
            this.DateFillingOut = DateFillingOut;
        }
        public override string ToString()
        {
            return $"1. ФИО: <{FullName}>\n" +
                   $"2. Дата рождения: <{BirthDate.ToString("dd/MM/yyyy")}>\n" +
                   $"3. Любимый язык программирования: <{FavoriteLanguage}>\n" +
                   $"4. Опыт программирования на указанном языке: <{ProgrammingExperience.ToString()}>\n" +
                   $"5. Мобильный телефон: <{PhoneNumber}>\n" +
                   $"Анкета заполнена: <{DateFillingOut.ToString("dd/MM/yyyy")}>";

        }

        public void SendData()
        {
            if (Sent != null)
            {
                Sent(ToString());
            }
        }
        public bool IsNull()
        {
            if (string.IsNullOrWhiteSpace(FullName) || BirthDate == DateTime.MinValue || string.IsNullOrWhiteSpace(FavoriteLanguage) 
                || string.IsNullOrWhiteSpace(PhoneNumber) || DateFillingOut == DateTime.MinValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
