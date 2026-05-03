using Reqnroll;
using NUnit.Framework;

namespace TestProject.Hooks
{
    [Binding]
    public class TestHooks
    {
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Setup before all tests
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Cleanup after all tests
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Setup before each scenario
        }

        [AfterScenario]
        public void AfterScenario()
        {
            // Cleanup after each scenario
        }
    }
}