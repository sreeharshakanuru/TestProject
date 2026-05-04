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

        [When("I select the first iPhone from search results and add it to the cart")]
        public void WhenISelectTheFirstIPhoneFromSearchResultsAndAddItToTheCart()
        {
            _amazonPage?.SelectFirstIPhoneAndAddToCart();
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

        [Then("the cart should contain an item with text \"(.*)\"")]
        public void ThenTheCartShouldContainAnItemWithText(string expectedText)
        {
            var itemFound = _amazonPage?.IsProductInCart(expectedText) ?? false;
            Assert.That(itemFound, Is.True, $"Expected cart to contain an item with text '{expectedText}'");
        }

        [Then("I close the browser")]
        public void ThenICloseBrowser()
        {
            _amazonPage?.CloseBrowser();
        }
    }
}
