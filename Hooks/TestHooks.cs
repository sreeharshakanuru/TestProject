using Reqnroll;
using OpenQA.Selenium;
using TestProject.Drivers;
using System;

namespace TestProject.Hooks
{
    [Binding]
    public class TestHooks
    {
        private IWebDriver? _driver;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine("🚀 TEST RUN STARTED");
            Console.WriteLine($"⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("═════════════════════════════════════════");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine("✅ TEST RUN COMPLETED");
            Console.WriteLine($"⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("═════════════════════════════════════════");
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            _driver = WebDriverSetup.CreateChromeDriver();
            scenarioContext.Set(_driver, "WebDriver");

            Console.WriteLine($"\n📝 Scenario: {scenarioContext.ScenarioInfo.Title}");
            Console.WriteLine("🌐 Chrome browser launched");
        }

        [AfterScenario(Order = 100)]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            try
            {
                if (scenarioContext.TestError != null)
                {
                    Console.WriteLine("❌ Scenario failed, capturing screenshot...");
                    if (_driver != null)
                    {
                        ScreenshotHelper.TakeScreenshotOnFailure(_driver, scenarioContext.ScenarioInfo.Title);
                    }
                }
                else
                {
                    Console.WriteLine("✅ Scenario passed");
                }
            }
            finally
            {
                CleanupWebDriver();
            }
        }

        private void CleanupWebDriver()
        {
            if (_driver != null)
            {
                WebDriverSetup.QuitDriver(_driver);
                _driver = null;
            }
            Console.WriteLine("🧹 Cleanup completed\n");
        }
    }
}

