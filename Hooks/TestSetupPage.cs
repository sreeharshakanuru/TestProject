using OpenQA.Selenium;
using Reqnroll;
using System;
using System.Collections.Generic;
using TestProject.Common;
using TestProject.Drivers;
using TestProject.Pages;
using TestProject.Shared;

namespace TestProject.HOOKS
{
    /// <summary>
    /// TestSetupPage manages all test lifecycle hooks and page initialization.
    /// Handles: BeforeTestRun, AfterTestRun, BeforeScenario, AfterScenario
    /// Also manages page object initialization for step definitions.
    /// </summary>
    public class TestSetupPage : BasePage
    {
        private readonly ScenarioContext _scenarioContext;
        private Dictionary<string, object> _pageObjects;

        // Private fields for page object instances
        private AmazonPage? _amazonPage;

        public TestSetupPage(IWebDriver driver, ScenarioContext scenarioContext) : base(driver)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _pageObjects = new Dictionary<string, object>();
        }

        #region Test Run Lifecycle

        /// <summary>
        /// Called before the entire test run starts.
        /// Logs test execution start information.
        /// </summary>
        public void InitializeTestRun()
        {
            PrintTestRunHeader("START");
        }

        /// <summary>
        /// Called after the entire test run completes.
        /// Logs test execution completion information.
        /// </summary>
        public void FinalizeTestRun()
        {
            PrintTestRunHeader("COMPLETE");
        }

        /// <summary>
        /// Prints formatted test run header with timestamp and environment info.
        /// </summary>
        private void PrintTestRunHeader(string status)
        {
            var message = status == "START" ? "🚀 TEST RUN STARTED" : "✅ TEST RUN COMPLETED";
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine(message);
            Console.WriteLine($"⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"🖥️  OS: {Environment.OSVersion.VersionString}");
            Console.WriteLine("═════════════════════════════════════════");
        }

        #endregion

        #region Scenario Lifecycle

        /// <summary>
        /// Called before each scenario starts.
        /// Sets up WebDriver and initializes all page objects.
        /// </summary>
        public void InitializeScenario()
        {
            try
            {
                Console.WriteLine($"\n📝 Scenario: {_scenarioContext.ScenarioInfo.Title}");
                Console.WriteLine("🌐 Chrome browser launched for scenario");
                Console.WriteLine("📦 Initializing page objects...");

                // Initialize all page objects
                InitializePageObjects();

                Console.WriteLine("✅ Scenario initialization complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing scenario: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Called after each scenario completes.
        /// Handles cleanup, screenshots on failure, and WebDriver disposal.
        /// </summary>
        public void FinalizeScenario()
        {
            try
            {
                // Check if scenario passed or failed
                if (_scenarioContext.TestError == null)
                {
                    Console.WriteLine("✅ Scenario passed");
                }
                else
                {
                    Console.WriteLine("❌ Scenario failed, capturing screenshot...");
                    TakeFailureScreenshot();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during scenario finalization: {ex.Message}");
            }
            finally
            {
                // Always cleanup
                CleanupScenario();
            }
        }

        /// <summary>
        /// Takes a screenshot on scenario failure for debugging.
        /// </summary>
        private void TakeFailureScreenshot()
        {
            try
            {
                string scenarioName = _scenarioContext.ScenarioInfo.Title;
                ScreenshotHelper.TakeScreenshotOnFailure(Driver, scenarioName);
                Console.WriteLine($"📸 Screenshot saved for scenario: {scenarioName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error taking screenshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up resources after scenario completion.
        /// </summary>
        private void CleanupScenario()
        {
            try
            {
                // Clear page objects
                _amazonPage = null;
                _pageObjects.Clear();

                // Close WebDriver
                try
                {
                    WebDriverSetup.QuitDriver(Driver);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error closing WebDriver: {ex.Message}");
                }

                Console.WriteLine("🧹 Scenario cleanup completed\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during cleanup: {ex.Message}");
            }
        }

        #endregion

        #region Page Object Management

        /// <summary>
        /// Initializes all page objects and stores them in the scenario context.
        /// This allows step definitions to access initialized page objects.
        /// </summary>
        private void InitializePageObjects()
        {
            try
            {
                // Initialize AmazonPage
                _amazonPage = new AmazonPage(Driver);
                RegisterPageObject("AmazonPage", _amazonPage);
                Console.WriteLine("  ✓ AmazonPage initialized");

                // Add more page objects here as you create new pages
                // Example:
                // var checkoutPage = new CheckoutPage(Driver);
                // RegisterPageObject("CheckoutPage", checkoutPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing page objects: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Registers a page object in the scenario context for access by step definitions.
        /// </summary>
        public void RegisterPageObject(string key, object pageObject)
        {
            _pageObjects[key] = pageObject;
            _scenarioContext.Set(pageObject, key);
        }

        /// <summary>
        /// Gets a page object from the scenario context by key.
        /// </summary>
        public T GetPageObject<T>(string key) where T : class
        {
            if (_scenarioContext.TryGetValue(key, out T? pageObject))
            {
                return pageObject ?? throw new InvalidOperationException($"Page object '{key}' is null");
            }
            throw new InvalidOperationException($"Page object '{key}' not found in scenario context");
        }

        /// <summary>
        /// Gets the AmazonPage instance.
        /// </summary>
        public AmazonPage GetAmazonPage()
        {
            return _amazonPage ?? throw new InvalidOperationException("AmazonPage not initialized");
        }

        #endregion

        #region Test Context Information

        /// <summary>
        /// Gets the scenario title from context.
        /// </summary>
        public string GetScenarioTitle()
        {
            return _scenarioContext.ScenarioInfo.Title;
        }

        /// <summary>
        /// Gets whether the current scenario passed.
        /// </summary>
        public bool IsScenarioPassed()
        {
            return _scenarioContext.TestError == null;
        }

        /// <summary>
        /// Gets the current test error if scenario failed.
        /// </summary>
        public Exception? GetTestError()
        {
            return _scenarioContext.TestError;
        }

        /// <summary>
        /// Logs a test step with timestamp.
        /// </summary>
        public void LogTestStep(string stepDescription)
        {
            Console.WriteLine($"  → {stepDescription} [{DateTime.Now:HH:mm:ss.fff}]");
        }

        /// <summary>
        /// Logs test result.
        /// </summary>
        public void LogTestResult(string message, bool success = true)
        {
            string icon = success ? "✅" : "❌";
            Console.WriteLine($"{icon} {message}");
        }

        #endregion

        #region Scenario Data Management

        /// <summary>
        /// Stores data that needs to be shared across steps in a scenario.
        /// </summary>
        public void SetScenarioData(string key, object value)
        {
            try
            {
                _scenarioContext.Set(value, key);
                Console.WriteLine($"  📌 Scenario data saved: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error setting scenario data '{key}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves data from scenario context.
        /// </summary>
        public T GetScenarioData<T>(string key)
        {
            try
            {
                return _scenarioContext.Get<T>(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving scenario data '{key}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if scenario data exists.
        /// </summary>
        public bool HasScenarioData(string key)
        {
            return _scenarioContext.ContainsKey(key);
        }

        #endregion

        #region Test Execution Helpers

        /// <summary>
        /// Waits for a specified time in milliseconds with logging.
        /// </summary>
        public void WaitWithLogging(int milliseconds, string reason = "")
        {
            var message = string.IsNullOrEmpty(reason) 
                ? $"Waiting for {milliseconds}ms" 
                : $"Waiting {milliseconds}ms - {reason}";
            
            Console.WriteLine($"  ⏳ {message}");
            Sleep(milliseconds);
        }

        /// <summary>
        /// Executes an action and logs its completion.
        /// </summary>
        public void ExecuteAction(Action action, string actionDescription)
        {
            try
            {
                Console.WriteLine($"  ➤ {actionDescription}");
                action();
                Console.WriteLine($"  ✓ {actionDescription} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {actionDescription} failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes an action and returns a result with logging.
        /// </summary>
        public T ExecuteAction<T>(Func<T> action, string actionDescription)
        {
            try
            {
                Console.WriteLine($"  ➤ {actionDescription}");
                var result = action();
                Console.WriteLine($"  ✓ {actionDescription} completed");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {actionDescription} failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retries an action with specified number of attempts.
        /// </summary>
        public void RetryAction(Action action, int maxAttempts = 3, int delayMs = 1000)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxAttempts)
            {
                try
                {
                    action();
                    Console.WriteLine($"  ✓ Action succeeded on attempt {attempt + 1}");
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    Console.WriteLine($"  ⚠️ Attempt {attempt} failed: {ex.Message}");
                    
                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"  ⏳ Retrying in {delayMs}ms...");
                        Sleep(delayMs);
                    }
                }
            }

            throw new InvalidOperationException(
                $"Action failed after {maxAttempts} attempts", 
                lastException);
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Asserts a condition and logs the result.
        /// </summary>
        public void AssertCondition(bool condition, string assertionDescription)
        {
            if (condition)
            {
                Console.WriteLine($"  ✓ Assertion passed: {assertionDescription}");
            }
            else
            {
                Console.WriteLine($"  ✗ Assertion failed: {assertionDescription}");
                throw new InvalidOperationException($"Assertion failed: {assertionDescription}");
            }
        }

        /// <summary>
        /// Asserts two values are equal.
        /// </summary>
        public void AssertEqual<T>(T expected, T actual, string description)
        {
            if ((expected == null && actual == null) || (expected?.Equals(actual) ?? false))
            {
                Console.WriteLine($"  ✓ Values equal: {description}");
            }
            else
            {
                Console.WriteLine($"  ✗ Values not equal: {description}");
                Console.WriteLine($"    Expected: {expected}");
                Console.WriteLine($"    Actual: {actual}");
                throw new InvalidOperationException(
                    $"{description} - Expected: {expected}, Actual: {actual}");
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Prints a detailed session summary at the end of execution.
        /// </summary>
        public void PrintSessionSummary()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("TEST SESSION SUMMARY");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Scenario: {GetScenarioTitle()}");
            Console.WriteLine($"Status: {(IsScenarioPassed() ? "✅ PASSED" : "❌ FAILED")}");
            Console.WriteLine($"Completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            if (!IsScenarioPassed())
            {
                Console.WriteLine($"Error: {GetTestError()?.Message}");
            }
            
            Console.WriteLine(new string('=', 50) + "\n");
        }

        #endregion
    }
}
