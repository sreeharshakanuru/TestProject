using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using TestProject.Common;

namespace TestProject.Drivers
{
    public class WebDriverSetup
    {
        public static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-logging");

            return new ChromeDriver(options);
        }

        public static void QuitDriver(IWebDriver driver)
        {
            ExecuteQuitOperation(driver)
                .Match(
                    onSuccess: () => Console.WriteLine("✅ WebDriver closed successfully"),
                    onFailure: ex => Console.WriteLine($"⚠️ Error closing WebDriver: {ex.Message}")
                );
        }

        private static Result ExecuteQuitOperation(IWebDriver driver)
        {
            return ExecuteSafely(() => 
            {
                driver.Quit();
                driver.Dispose();
            });
        }

        private static Result ExecuteSafely(Action operation)
        {
            try
            {
                operation();
                return Result.SuccessOf();
            }
            catch (Exception ex)
            {
                return Result.FailureOf(ex);
            }
        }
    }
}


