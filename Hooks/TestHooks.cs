using Reqnroll;
using NUnit.Framework;
using OpenQA.Selenium;
using TestProject.Drivers;
using TestProject.Common;
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
            Console.WriteLine($"🖥️  OS: {Environment.OSVersion.VersionString}");
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

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            _driver = WebDriverSetup.CreateChromeDriver();
            scenarioContext.Set(_driver, "WebDriver");

            Console.WriteLine($"\n📝 Scenario: {scenarioContext.ScenarioInfo.Title}");
            Console.WriteLine("🌐 WebDriver initialized");
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            HandleScenarioCompletion(scenarioContext);
            CleanupWebDriver();
        }

        private void HandleScenarioCompletion(ScenarioContext scenarioContext)
        {
            GetWebDriver(scenarioContext)
                .Match(
                    onSuccess: driver => ExecuteOnScenarioFailure(driver, scenarioContext),
                    onFailure: ex => Console.WriteLine($"⚠️ Error retrieving WebDriver: {ex.Message}")
                );
        }

        private static Result<IWebDriver> GetWebDriver(ScenarioContext context)
        {
            return context.TryGetValue("WebDriver", out IWebDriver driver)
                ? Result<IWebDriver>.SuccessOf(driver)
                : Result<IWebDriver>.FailureOf(new Exception("WebDriver not found in context"));
        }

        private void ExecuteOnScenarioFailure(IWebDriver driver, ScenarioContext scenarioContext)
        {
            scenarioContext.IsScenarioSuccess()
                .Match(
                    onSuccess: () => Console.WriteLine("✅ Scenario passed"),
                    onFailure: ex => TakeFailureScreenshot(driver, scenarioContext)
                );
        }

        private void TakeFailureScreenshot(IWebDriver driver, ScenarioContext scenarioContext)
        {
            Console.WriteLine("❌ Scenario failed");
            ScreenshotHelper.TakeScreenshotOnFailure(driver, scenarioContext.ScenarioInfo.Title);
        }

        private void CleanupWebDriver()
        {
            _driver?.Let(d => WebDriverSetup.QuitDriver(d));
            _driver = null;
            Console.WriteLine("🧹 Scenario cleanup completed\n");
        }
    }

    public static class ScenarioContextExtensions
    {
        public static Result IsScenarioSuccess(this ScenarioContext context)
        {
            return (context.TestError == null)
                ? Result.SuccessOf()
                : Result.FailureOf(context.TestError);
        }

        public static Result TryGetValue<T>(this ScenarioContext context, string key, out T value)
        {
            value = default!;
            return context.TryGetValue(key, out value)
                ? Result.SuccessOf()
                : Result.FailureOf(new Exception($"Key '{key}' not found in ScenarioContext"));
        }
    }

    public static class NullableExtensions
    {
        public static void Let<T>(this T? source, Action<T> action) where T : class
        {
            source?.Run(action);
        }

        private static void Run<T>(this T source, Action<T> action)
        {
            action(source);
        }
    }
}

