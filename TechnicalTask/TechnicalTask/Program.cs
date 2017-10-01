using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TechnicalTask
{
    // перечисление для типа организации - компания или ИП
    public enum OrganizationType
    {
        Company,
        Entrepreneur
    }

    // перечисление для состояния - действующая организация или нет
    public enum CurrentState
    {
        Opened,
        Closed
    }

    // класс организации - родительский для компании и ИП и содержит их общие поля
    class Organization
    {
        protected OrganizationType orgType;
        public OrganizationType OrgType { get { return orgType; } private set { orgType = value; } }

        protected int id;
        public int ID { get { return id; } private set { id = value; } }

        protected double profit;
        public double Profit { get { return profit; } private set { profit = value; } }

        protected DateTime creationDate;
        public DateTime CreationDate { get { return creationDate; } private set { creationDate = value; } }

        protected DateTime reportDate;
        public DateTime ReportDate { get { return reportDate; } private set { reportDate = value; } }

        protected DateTime closingDate;
        public DateTime ClosingDate { get { return closingDate; } set { closingDate = value; } }

        protected bool closed;
        public bool Closed { get { return closed; } set { closed = value; } }

        public Organization() { } // дефолтный конструктор, т.к. родительский класс для Company и Entrepreneur
    }

    // класс компании, наследуется от Organization
    class Company : Organization
    {
        protected string name;
        public string Name { get { return name; } private set { name = value; } }

        // конструктор класса
        public Company(int _Id, string _Name, double _Profit, DateTime _CreationDate, DateTime _ReportDate)
        {
            orgType = OrganizationType.Company;
            id = _Id;
            name = _Name;
            profit = _Profit;
            creationDate = _CreationDate;
            reportDate = _ReportDate;
            closed = false;
        }
    }

    // класс ИП, наследуется от Organization
    class Entrepreneur : Organization
    {
        protected string firstName;
        public string FirstName { get { return firstName; } private set { firstName = value; } }

        protected string secondName;
        public string SecondName { get { return secondName; } private set { secondName = value; } }

        // конструктор класса
        public Entrepreneur(int _Id, string _FirstName, string _SecondName, double _Profit, DateTime _CreationDate, DateTime _ReportDate)
        {
            orgType = OrganizationType.Entrepreneur;
            id = _Id;
            firstName = _FirstName;
            secondName = _SecondName;
            profit = _Profit;
            creationDate = _CreationDate;
            reportDate = _ReportDate;
            closed = false;
        }
    }

    // класс, в котором хранится список всех организаций и в котором происходит обработка данных XML-файлов
    class Task
    {
        List<Organization> data; // список всех организаций

        // Конструктор класса
        public Task()
        {
            data = new List<Organization>();
            this.LoadData(OrganizationType.Company); // загрузка данных о компаниях
            this.LoadData(OrganizationType.Entrepreneur); // загрузка данных об ИП
        }

        // метод для загрузки и предварительной обработки данных
        public void LoadData(OrganizationType organizationType)
        {
            // загрузка данных из companies.xml или entrepreneurs.xml в зависимости от параметра метода
            XmlDocument document = new XmlDocument();
            switch (organizationType)
            {
                case OrganizationType.Company:
                    document.Load("Data/companies.xml");
                    break;
                case OrganizationType.Entrepreneur:
                    document.Load("Data/entrepreneurs.xml");
                    break;
            }
            int id;
            double profit;
            DateTime creationDate;
            DateTime reportDate;
            foreach (XmlNode node in document.DocumentElement)
            {
                id = int.Parse(node["Id"].InnerText);
                profit = Convert.ToDouble(node["Profit"].InnerText);
                creationDate = Convert.ToDateTime(node["CreationDate"].InnerText);
                reportDate = Convert.ToDateTime(node["ReportDate"].InnerText);
                switch (organizationType)
                {
                    case OrganizationType.Company:
                        string name = node["Name"].InnerText;
                        data.Add(new Company(id, name, profit, creationDate, reportDate));
                        break;
                    case OrganizationType.Entrepreneur:
                        string firstName = node["FirstName"].InnerText;
                        string secondName = node["SecondName"].InnerText;
                        data.Add(new Entrepreneur(id, firstName, secondName, profit, creationDate, reportDate));
                        break;
                }
            }
            // нахожу организации с одинаковыми Id и сохраняю тот, у которого дата отчета раньше (т.е. информация устарела), в доп. список повторяющихся организаций 
            List<Organization> repetitionInData = new List<Organization>();
            foreach (Organization org1 in data)
                foreach (Organization org2 in data)
                    if (org1.ID == org2.ID)
                        if (org1.ReportDate < org2.ReportDate)
                            repetitionInData.Add(org1);
            // здесь сравниваются 2 списка и из data удаляются ненужные организации, информация о которых устарела
            foreach (Organization unnecessaryOrg in repetitionInData)
                for (int i = 0; i < data.Count; i++)
                    if (unnecessaryOrg == data[i])
                        data.RemoveAt(i--);
            // загрузка данных об организациях, прекративших свое существование
            XmlDocument closeInfo = new XmlDocument();
            closeInfo.Load("Data/closeinfo.xml");
            DateTime closingDate;
            foreach (XmlNode node in closeInfo.DocumentElement)
            {
                id = int.Parse(node["Id"].InnerText);
                closingDate = Convert.ToDateTime(node["CloseDate"].InnerText);
                for (int i = 0; i < data.Count; i++)
                     if (data[i].ID == id)
                     {
                          data[i].ClosingDate = closingDate;
                          data[i].Closed = true;
                     }
            }             
        }

        // метод, возвращающий количество действующих или недействующих компаний или ИП (в зависимости от параметров метода)
        public int Count(CurrentState currentState, OrganizationType organizationType)
        {
            int count = 0; // переменная, где хранится количество организаций заданного типа
            // прохожу по списку организаций, выбираю организации заданного типа и увеличиваю счетчик
            foreach (Organization org in data)
                switch (currentState)
                {
                    case CurrentState.Opened:
                        switch (organizationType)
                        {
                            case OrganizationType.Company:
                                if (org.OrgType == OrganizationType.Company && org.Closed == false) count++;
                                break;
                            case OrganizationType.Entrepreneur:
                                if (org.OrgType == OrganizationType.Entrepreneur && org.Closed == false) count++;
                                break;
                        }
                        break;
                    case CurrentState.Closed:
                        switch (organizationType)
                        {
                            case OrganizationType.Company:
                                if (org.OrgType == OrganizationType.Company && org.Closed == true) count++;
                                break;
                            case OrganizationType.Entrepreneur:
                                if (org.OrgType == OrganizationType.Entrepreneur && org.Closed == true) count++;
                                break;
                        }
                        break;
                }
            return count; // возвращаю количество организаций заданного типа
        }

        // метод, возвращающий значение средней прибыли компаний и ИП, принимает параметр состояние организации (действующая или нет)
        public double AverageProfit(CurrentState currentState)
        {
            int count = 0;  // переменная, где хранится количество организаций заданного типа
            double sumProfit = 0; // переменная, где хранится суммарная прибыль
            double averageProfit = 0; // переменная, где хранится средняя прибыль организаций заданного типа
            // прохожу по списку организаций, выбираю организации заданного типа, увеличиваю счетчик и суммирую прибыль
            foreach (Organization org in data)
                switch (currentState)
                {
                case CurrentState.Opened:
                    if (org.Closed == false)
                    {
                        count++;
                        sumProfit += org.Profit;
                    }
                    break;
                case CurrentState.Closed:
                     if (org.Closed == true)
                     {
                         count++;
                         sumProfit += org.Profit;
                     }
                    break;
                }
            averageProfit = sumProfit / count;
            return averageProfit; // возвращаю среднюю прибыль
        }

        // метод, возвращающий значение среднего количества дней существования компаний и ИП
        public double AverageTimeOfWork()
        {
            int count = 0; // переменная, где хранится количество недействующих организаций
            double sumDaysOfOrganizations = 0; // переменная, где хранится суммарное количество дней существования компаний и ИП
            double averageDaysOfOrganizations = 0; // переменная, где хранится среднее количество дней существования компаний и ИП
            TimeSpan time; // переменная, где хранится разность между днем закрытия и открытия организации
            foreach (Organization org in data)
                if (org.Closed == true)
                {
                    time = org.ClosingDate - org.CreationDate;
                    sumDaysOfOrganizations += time.Days;
                    count++;
                }
            averageDaysOfOrganizations = sumDaysOfOrganizations / count;
            return averageDaysOfOrganizations; // среднее количество дней существования компаний и ИП
        } 
    }

    class Program
    {
        static void Main(string[] args)
        {
            // создаю объект класса Task и вызываю его разные методы в соответствии с заданием
            Task t = new Task();
            Console.WriteLine("Количество действующих компаний " + t.Count(CurrentState.Opened, OrganizationType.Company));
            Console.WriteLine("Количество действующих ИП " + t.Count(CurrentState.Opened, OrganizationType.Entrepreneur));
            Console.WriteLine("Количество недействующих компаний " + t.Count(CurrentState.Closed, OrganizationType.Company));
            Console.WriteLine("Количество недействующих ИП " + t.Count(CurrentState.Closed, OrganizationType.Entrepreneur));
            Console.WriteLine("Средняя прибыль действующих компаний и ИП " + t.AverageProfit(CurrentState.Opened));
            Console.WriteLine("Средняя прибыль недействующих компаний и ИП " + t.AverageProfit(CurrentState.Closed));
            Console.WriteLine("Среднее время существования компаний и ИП в днях " + t.AverageTimeOfWork());
            Console.ReadLine();
        }
    }
}
