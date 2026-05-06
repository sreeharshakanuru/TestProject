using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using TestProject.Common;
using TestProject.Pages;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class AmazonSteps
    {
        private IWebDriver? _driver;
        private AmazonPage? _amazonPage;

        [BeforeScenario]
        public void Setup(ScenarioContext scenarioContext)
        {
            if (scenarioContext.TryGetValue("WebDriver", out IWebDriver driver))
            {
                _driver = driver;
            }
            else
            {
                try
                {
                    _driver = scenarioContext.Get<IWebDriver>("WebDriver");
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine($"⚠️ WebDriver not found in ScenarioContext: {ex.Message}");
                    throw;
                }
            }

            if (_driver == null)
            {
                throw new InvalidOperationException("WebDriver could not be initialized for the scenario.");
            }

            _amazonPage = new AmazonPage(_driver);
        }

        [Given("I open the Amazon home page")]
        public void GivenIOpenTheAmazonHomePage()
        {
            _amazonPage?.NavigateToAmazonHomePage();
        }

        [When("I search for \"(.*)\"")]
        public void WhenISearchFor(string productName)
        {
            _amazonPage?.SearchForProduct(productName);
        }

        [When("I select the first product from search results and add it to the cart")]
        public void WhenISelectTheFirstProductFromSearchResultsAndAddItToTheCart()
        {
            _amazonPage?.SelectFirstProductAndAddToCart();
        }

        [When("I return to the Amazon home page")]
        public void WhenIReturnToTheAmazonHomePage()
        {
            _amazonPage?.ReturnToHomePage();
        }

        [When("I go to the cart")]
        public void WhenIGoToTheCart()
        {
            _amazonPage?.GoToCart();
        }

        [Then("the cart should contain at least one item")]
        public void ThenTheCartShouldContainAtLeastOneItem()
        {
            var hasItems = _amazonPage?.CartHasItems() ?? false;
            Assert.That(hasItems, Is.True, "Expected cart to contain at least one item");
        }

        [Then("I close the browser")]
        public void ThenICloseBrowser()
        {
            _amazonPage?.CloseBrowser();
        }
    }
}
