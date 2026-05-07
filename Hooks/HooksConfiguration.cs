using OpenQA.Selenium;
using Reqnroll;
using System;
using TestProject.Drivers;

namespace TestProject.HOOKS
{
    /// <summary>
    /// HooksConfiguration integrates TestSetupPage with SpecFlow hooks.
    /// This class contains all SpecFlow hook bindings that manage test lifecycle.
    /// </summary>
    [Binding]
    public class HooksConfiguration
    {
        private TestSetupPage? _testSetupPage;
        private IWebDriver? _driver;

        #region Test Run Hooks

        /// <summary>
        /// Executes before the entire test run starts.
        /// Initializes test run environment.
        /// </summary>
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Console.WriteLine("\n");
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine("🚀 INITIALIZING TEST RUN");
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Environment: {Environment.OSVersion}");
            Console.WriteLine("═════════════════════════════════════════\n");
        }

        /// <summary>
        /// Executes after the entire test run completes.
        /// Finalizes and reports test run results.
        /// </summary>
        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("\n");
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine("✅ TEST RUN COMPLETED");
            Console.WriteLine("═════════════════════════════════════════");
            Console.WriteLine($"End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("═════════════════════════════════════════\n");
        }

        #endregion

        #region Scenario Hooks

        /// <summary>
        /// Executes before each scenario starts.
        /// Order = 0 ensures this runs before other BeforeScenario hooks.
        /// Sets up WebDriver and initializes the test setup page.
        /// </summary>
        [BeforeScenario(Order = 0)]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            try
            {
                // Create WebDriver
                _driver = WebDriverSetup.CreateChromeDriver();
                
                // Store WebDriver in scenario context for step definitions
                scenarioContext.Set(_driver, "WebDriver");
                
                // Create and initialize TestSetupPage
                _testSetupPage = new TestSetupPage(_driver, scenarioContext);
                scenarioContext.Set(_testSetupPage, "TestSetupPage");
                
                // Log scenario start
                Console.WriteLine("\n" + new string('-', 50));
                _testSetupPage.LogTestStep($"Starting Scenario: {scenarioContext.ScenarioInfo.Title}");
                Console.WriteLine(new string('-', 50));
                
                // Initialize all page objects
                _testSetupPage.InitializeScenario();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in BeforeScenario: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Executes after each scenario completes.
        /// Order = 100 ensures this runs after other AfterScenario hooks.
        /// Handles cleanup, screenshots on failure, and WebDriver disposal.
        /// </summary>
        [AfterScenario(Order = 100)]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            try
            {
                // Get TestSetupPage from context
                if (scenarioContext.TryGetValue("TestSetupPage", out TestSetupPage? testSetupPage) && testSetupPage != null)
                {
                    // Finalize scenario (includes screenshot on failure and cleanup)
                    testSetupPage.FinalizeScenario();
                    
                    // Print session summary
                    testSetupPage.PrintSessionSummary();
                }
                else
                {
                    Console.WriteLine("⚠️ TestSetupPage not found in scenario context");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AfterScenario: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                // Ensure WebDriver is cleaned up
                try
                {
                    if (_driver != null)
                    {
                        WebDriverSetup.QuitDriver(_driver);
                        _driver = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error cleaning up WebDriver: {ex.Message}");
                }
            }
        }

        #endregion

        #region Step Hooks

        /// <summary>
        /// Executes before each step.
        /// Can be used for logging or validation before step execution.
        /// </summary>
        [BeforeStep]
        public void BeforeStep(ScenarioContext scenarioContext)
        {
            try
            {
                if (scenarioContext.TryGetValue("TestSetupPage", out TestSetupPage? testSetupPage) && testSetupPage != null)
                {
                    // Log current step (optional)
                    // testSetupPage.LogTestStep($"Executing step: {scenarioContext.CurrentScenarioBlock}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in BeforeStep: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes after each step.
        /// Can be used for validation or logging after step execution.
        /// </summary>
        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext)
        {
            try
            {
                if (scenarioContext.TryGetValue("TestSetupPage", out TestSetupPage? testSetupPage) && testSetupPage != null)
                {
                    // Log step completion (optional)
                    // testSetupPage.LogTestStep($"Step completed successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in AfterStep: {ex.Message}");
            }
        }

        #endregion

        #region Block Hooks (Optional)

        /// <summary>
        /// Executes before each scenario block (Given, When, Then, etc.).
        /// Useful for setup specific to each block type.
        /// </summary>
        [BeforeScenarioBlock]
        public void BeforeScenarioBlock(ScenarioContext scenarioContext)
        {
            try
            {
                // Currently not used, but available for future use
                // Examples:
                // - Reset test data before Given block
                // - Log the type of block being executed
                // - Validate preconditions before When block
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in BeforeScenarioBlock: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes after each scenario block (Given, When, Then, etc.).
        /// Useful for validation specific to each block type.
        /// </summary>
        [AfterScenarioBlock]
        public void AfterScenarioBlock(ScenarioContext scenarioContext)
        {
            try
            {
                // Currently not used, but available for future use
                // Examples:
                // - Validate data after Given block
                // - Take intermediate screenshots
                // - Log state between blocks
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in AfterScenarioBlock: {ex.Message}");
            }
        }

        #endregion
    }
}
