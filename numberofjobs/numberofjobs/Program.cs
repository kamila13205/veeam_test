using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Threading;
using send_manually = System.Windows.Forms.SendKeys;

namespace db_retriever
{
    class Program
    {
        static readonly string departmentSwitchXPath = 
            @"/html/body/div[1]/div/div[1]/div/div/div[1]/div/div[2]/div/div/button";
        static readonly string RnDXPath =
            @"/html/body/div[1]/div/div[1]/div/div/div[1]/div/div[2]/div/div/div/a[3]";

        public static bool SwitchToRnD(IWebDriver driver)
        {
            IWebElement departmentSwitch = driver.FindElement(By.XPath(departmentSwitchXPath));
            departmentSwitch.Click();

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    IWebElement RnDButton = driver.FindElement(By.XPath(RnDXPath));
                    RnDButton.Click();
                    return true;
                }
                catch (ElementClickInterceptedException)
                {
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        static readonly string languageSwitchXPath =
            @"/html/body/div[1]/div/div[1]/div/div/div[1]/div/div[3]/div/div/button";
        static readonly string EnglishXPath =
            @"/html/body/div[1]/div/div[1]/div/div/div[1]/div/div[3]/div/div/div/div[1]";

        public static void SwitchToEnglish(IWebDriver driver)
        {
            IWebElement languageSwitch = driver.FindElement(By.XPath(languageSwitchXPath));
            languageSwitch.Click();
            IWebElement EngishCheckBox = driver.FindElement(By.XPath(EnglishXPath));
            EngishCheckBox.Click();
            send_manually.SendWait("{ESC}");
        }

        static readonly string JobsParentXPath =
            @"/html/body/div[1]/div/div[1]/div/div/div[2]/div";
        static readonly string MaxJobsXPath =
            @"/html/body/div[1]/div/div[1]/div/h3/span";

        public static int CountJobs(IWebDriver driver)
        {
            // max jobs
            IWebElement maxjobselem = driver.FindElement(By.XPath(MaxJobsXPath));
            int.TryParse(maxjobselem.Text, out int maxjobs);
            
            for (int number = 1; number < maxjobs + 1; number++)
            {
                try
                {
                    string JobXPath = $@"{JobsParentXPath}/a[{number.ToString()}]";
                    driver.FindElement(By.XPath(JobXPath));
                }
                catch (NoSuchElementException)
                {
                    return number - 1;
                }
            }
            return 0;
        }

        static int Main(string[] args)
        {
            int jobspresent = 0;

            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            
            string link = @"https://cz.careers.veeam.com/vacancies";
            driver.Navigate().GoToUrl(link);

            bool rndpresent = SwitchToRnD(driver);
            SwitchToEnglish(driver);

            if (rndpresent)
                jobspresent = CountJobs(driver);

            driver.Quit();
            return jobspresent;
        }
    }
}
