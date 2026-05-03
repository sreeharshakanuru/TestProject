using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TestProject.Drivers
{
    public class WebDriverSetup
    {
        public static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            // Add options if needed, e.g., headless
            // options.AddArgument("--headless");
            return new ChromeDriver(options);
        }
    }
}