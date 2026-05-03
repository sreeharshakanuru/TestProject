using OpenQA.Selenium;
using System;
using System.IO;
using TestProject.Common;

namespace TestProject.Drivers
{
    public static class ScreenshotHelper
    {
        private static readonly string ScreenshotDirectory = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");

        static ScreenshotHelper()
        {
            Directory.CreateDirectory(ScreenshotDirectory);
        }

        public static string TakeScreenshot(IWebDriver driver, string testName = "")
        {
            var screenshotName = GenerateScreenshotName(testName);
            var filePath = Path.Combine(ScreenshotDirectory, screenshotName);

            return ExecuteTakeScreenshot(driver, filePath)
                .Match(
                    onSuccess: _ => filePath,
                    onFailure: ex => HandleScreenshotFailure(ex, testName)
                );
        }

        public static void TakeScreenshotOnFailure(IWebDriver driver, string scenarioName)
        {
            var screenshotName = $"{scenarioName}_FAILED_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            var filePath = Path.Combine(ScreenshotDirectory, screenshotName);

            ExecuteTakeScreenshot(driver, filePath)
                .Match(
                    onSuccess: _ => Console.WriteLine($"❌ Screenshot saved: {filePath}"),
                    onFailure: ex => Console.WriteLine($"❌ Failed to take screenshot: {ex.Message}")
                );
        }

        private static Result<string> ExecuteTakeScreenshot(IWebDriver driver, string filePath)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(filePath);
                return Result<string>.SuccessOf(filePath);
            }
            catch (Exception ex)
            {
                return Result<string>.FailureOf(ex);
            }
        }

        private static string GenerateScreenshotName(string testName)
        {
            var sanitizedName = string.IsNullOrWhiteSpace(testName) 
                ? "screenshot" 
                : testName.Replace(" ", "_").Replace("\"", "");
            
            return $"{sanitizedName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        }

        private static string HandleScreenshotFailure(Exception ex, string testName)
        {
            Console.WriteLine($"⚠️ Screenshot failed for '{testName}': {ex.Message}");
            return string.Empty;
        }
    }
}
