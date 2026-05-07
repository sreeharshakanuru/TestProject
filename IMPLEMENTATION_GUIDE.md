# Implementation Guide: Using the New Shared Architecture

## Overview
This guide demonstrates how to use the new `Shared/BasePage.cs` architecture for creating page objects and step definitions.

---

## Example 1: Adding a New Page Class

### Step 1: Create Checkout Page

**File**: `Pages/CheckoutPage.cs`

```csharp
using OpenQA.Selenium;
using System;
using TestProject.Shared;

namespace TestProject.Pages
{
    /// <summary>
    /// CheckoutPage handles all checkout-related operations.
    /// All Selenium code is inherited from BasePage.
    /// </summary>
    public class CheckoutPage : BasePage
    {
        // Define all page locators as readonly fields
        private readonly By ProceedToCheckoutButton = By.XPath("//button[contains(text(), 'Proceed')]");
        private readonly By EmailInputField = By.Id("checkout-email");
        private readonly By FirstNameInput = By.Id("checkout-first-name");
        private readonly By LastNameInput = By.Id("checkout-last-name");
        private readonly By AddressInput = By.Id("checkout-address");
        private readonly By ZipCodeInput = By.Id("checkout-zip");
        private readonly By PlaceOrderButton = By.Id("place-order-btn");
        private readonly By OrderConfirmationMessage = By.XPath("//div[@id='order-confirmation']");
        private readonly By OrderNumberText = By.XPath("//span[@class='order-number']");

        // Constructor - Pass IWebDriver to base class
        public CheckoutPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Clicks the Proceed to Checkout button.
        /// Returns this for method chaining (Fluent API).
        /// </summary>
        public CheckoutPage ProceedToCheckout()
        {
            try
            {
                Click(ProceedToCheckoutButton);
                Console.WriteLine("Proceeded to checkout");
                Wait(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error proceeding to checkout: {ex.Message}");
                throw;
            }
            return this;
        }

        /// <summary>
        /// Fills in all checkout details.
        /// </summary>
        public CheckoutPage FillCheckoutDetails(string email, string firstName, 
                                                string lastName, string address, string zipCode)
        {
            try
            {
                SendText(EmailInputField, email);
                SendText(FirstNameInput, firstName);
                SendText(LastNameInput, lastName);
                SendText(AddressInput, address);
                SendText(ZipCodeInput, zipCode);
                
                Console.WriteLine("Checkout details filled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filling checkout details: {ex.Message}");
                throw;
            }
            return this;
        }

        /// <summary>
        /// Places the order by clicking the Place Order button.
        /// </summary>
        public CheckoutPage PlaceOrder()
        {
            try
            {
                ClickWithFallback(PlaceOrderButton);  // Uses BasePage method
                Console.WriteLine("Order placed");
                Wait(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing order: {ex.Message}");
                throw;
            }
            return this;
        }

        /// <summary>
        /// Verifies that the order confirmation page is displayed.
        /// </summary>
        public bool IsOrderConfirmed()
        {
            try
            {
                return IsElementDisplayed(OrderConfirmationMessage);  // BasePage method
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the order number from the confirmation page.
        /// </summary>
        public string GetOrderNumber()
        {
            try
            {
                return GetText(OrderNumberText);  // BasePage method
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting order number: {ex.Message}");
                throw;
            }
        }
    }
}
```

---

## Example 2: Adding Step Definitions for Checkout

**File**: `StepDefinitions/CheckoutSteps.cs`

```csharp
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using TestProject.Pages;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class CheckoutSteps
    {
        private IWebDriver? _driver;
        private CheckoutPage? _checkoutPage;

        [BeforeScenario]
        public void Setup(ScenarioContext scenarioContext)
        {
            // Get the WebDriver from scenario context
            _driver = scenarioContext.Get<IWebDriver>("WebDriver");
            _checkoutPage = new CheckoutPage(_driver);
        }

        [When("I proceed to checkout")]
        public void WhenIProceedToCheckout()
        {
            _checkoutPage?.ProceedToCheckout();
        }

        [When("I fill in the checkout details")]
        public void WhenIFillCheckoutDetails(Table table)
        {
            var row = table.Rows[0];
            _checkoutPage?.FillCheckoutDetails(
                email: row["Email"],
                firstName: row["FirstName"],
                lastName: row["LastName"],
                address: row["Address"],
                zipCode: row["ZipCode"]
            );
        }

        [When("I place the order")]
        public void WhenIPlaceTheOrder()
        {
            _checkoutPage?.PlaceOrder();
        }

        [Then("the order should be confirmed")]
        public void ThenOrderShouldBeConfirmed()
        {
            var isConfirmed = _checkoutPage?.IsOrderConfirmed() ?? false;
            Assert.That(isConfirmed, Is.True, "Order confirmation not displayed");
        }

        [Then("I should see order number")]
        public void ThenIShouldSeeOrderNumber()
        {
            var orderNumber = _checkoutPage?.GetOrderNumber();
            Assert.That(orderNumber, Is.Not.Null.And.Not.Empty, "Order number not found");
            Console.WriteLine($"Order number: {orderNumber}");
        }
    }
}
```

---

## Example 3: Using Multiple Pages in One Scenario

**Feature File**: `Features/CompleteCheckout.feature`

```gherkin
Feature: Complete Product Checkout

  Scenario: Search, add to cart, and complete checkout
    Given I open the Amazon home page
    When I search for "laptop"
    And I select the first product from search results and add it to the cart
    And I return to the Amazon home page
    And I go to the cart
    And I proceed to checkout
    And I fill in the checkout details
      | Email    | FirstName | LastName | Address        | ZipCode |
      | test@test.com | John   | Doe      | 123 Main St     | 12345   |
    And I place the order
    Then the order should be confirmed
    And I should see order number
```

**Step Definition File**: `StepDefinitions/CompleteCheckoutSteps.cs`

```csharp
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using TestProject.Pages;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class CompleteCheckoutSteps
    {
        private IWebDriver? _driver;
        private AmazonPage? _amazonPage;
        private CheckoutPage? _checkoutPage;

        [BeforeScenario]
        public void Setup(ScenarioContext scenarioContext)
        {
            _driver = scenarioContext.Get<IWebDriver>("WebDriver");
            _amazonPage = new AmazonPage(_driver);
            _checkoutPage = new CheckoutPage(_driver);
        }

        // Amazon page steps
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
        public void WhenISelectFirstProductAndAddToCart()
        {
            _amazonPage?.SelectFirstProductAndAddToCart();
        }

        [When("I return to the Amazon home page")]
        public void WhenIReturnToHomePage()
        {
            _amazonPage?.ReturnToHomePage();
        }

        [When("I go to the cart")]
        public void WhenIGoToCart()
        {
            _amazonPage?.GoToCart();
        }

        // Checkout page steps
        [When("I proceed to checkout")]
        public void WhenIProceedToCheckout()
        {
            _checkoutPage?.ProceedToCheckout();
        }

        [When("I fill in the checkout details")]
        public void WhenIFillCheckoutDetails(Table table)
        {
            var row = table.Rows[0];
            _checkoutPage?.FillCheckoutDetails(
                email: row["Email"],
                firstName: row["FirstName"],
                lastName: row["LastName"],
                address: row["Address"],
                zipCode: row["ZipCode"]
            );
        }

        [When("I place the order")]
        public void WhenIPlaceTheOrder()
        {
            _checkoutPage?.PlaceOrder();
        }

        [Then("the order should be confirmed")]
        public void ThenOrderShouldBeConfirmed()
        {
            var isConfirmed = _checkoutPage?.IsOrderConfirmed() ?? false;
            Assert.That(isConfirmed, Is.True);
        }

        [Then("I should see order number")]
        public void ThenIShouldSeeOrderNumber()
        {
            var orderNumber = _checkoutPage?.GetOrderNumber();
            Assert.That(orderNumber, Is.Not.Null.And.Not.Empty);
        }
    }
}
```

---

## Example 4: Common Patterns for Page Classes

### Pattern 1: Element Visibility Check

```csharp
public bool IsLoginFormVisible()
{
    return IsElementDisplayed(LoginFormLocator);  // BasePage method
}
```

### Pattern 2: Waiting for Element After Action

```csharp
public CheckoutPage ClickSubmitAndWait()
{
    Click(SubmitButton);
    WaitForElementToBeVisible(ConfirmationMessageLocator);  // BasePage method
    return this;
}
```

### Pattern 3: Getting Multiple Elements

```csharp
public int GetProductCount()
{
    var products = FindElements(ProductItemsLocator);  // BasePage method
    return products.Count;
}
```

### Pattern 4: JavaScript Execution

```csharp
public CheckoutPage ScrollToBottom()
{
    ScrollToBottom();  // BasePage method
    return this;
}

public CheckoutPage ScrollToElement(By locator)
{
    ScrollIntoView(locator);  // BasePage method
    return this;
}
```

### Pattern 5: Handling Optional Elements

```csharp
public CheckoutPage HandleOptionalPopup()
{
    try
    {
        if (IsElementPresent(PopupCloseButtonLocator))
        {
            Click(PopupCloseButtonLocator);
            Console.WriteLine("Popup closed");
        }
    }
    catch
    {
        Console.WriteLine("No popup to close");
    }
    return this;
}
```

### Pattern 6: Getting Element Attributes

```csharp
public string GetProductPrice()
{
    return GetAttribute(PriceLocator, "data-price");  // BasePage method
}

public string GetButtonText()
{
    return GetText(ButtonLocator);  // BasePage method
}
```

---

## Example 5: Advanced: Page with Dynamic Elements

**File**: `Pages/SearchResultsPage.cs`

```csharp
public class SearchResultsPage : BasePage
{
    private readonly By ProductItemsLocator = By.XPath("//div[@class='product-item']");
    private readonly By ProductNameInItem = By.XPath("./span[@class='product-name']");
    private readonly By ProductPriceInItem = By.XPath("./span[@class='price']");

    public SearchResultsPage(IWebDriver driver) : base(driver)
    {
    }

    /// <summary>
    /// Finds a product by name in the search results.
    /// </summary>
    public IWebElement FindProductByName(string productName)
    {
        var products = FindElements(ProductItemsLocator);  // BasePage method
        
        foreach (var product in products)
        {
            var nameElement = product.FindElement(ProductNameInItem);
            if (nameElement.Text.Contains(productName))
            {
                return product;
            }
        }
        
        throw new InvalidOperationException($"Product '{productName}' not found");
    }

    /// <summary>
    /// Gets all product names from the search results.
    /// </summary>
    public List<string> GetAllProductNames()
    {
        var products = FindElements(ProductItemsLocator);  // BasePage method
        var names = new List<string>();
        
        foreach (var product in products)
        {
            var nameElement = product.FindElement(ProductNameInItem);
            names.Add(nameElement.Text);
        }
        
        return names;
    }

    /// <summary>
    /// Clicks on a product by name.
    /// </summary>
    public void ClickProductByName(string productName)
    {
        var product = FindProductByName(productName);
        product.Click();
        Wait(2000);
    }
}
```

---

## Checklist: Creating a New Page Class

- [ ] Create new file in `Pages/` folder
- [ ] Inherit from `BasePage`
- [ ] Define all locators as `private readonly By` fields
- [ ] Implement constructor with `IWebDriver` parameter
- [ ] Call `base(driver)` in constructor
- [ ] Create public methods for page actions
- [ ] Use only BasePage methods (no direct Selenium calls)
- [ ] Add XML documentation to public methods
- [ ] Return `this` for method chaining where appropriate
- [ ] Include try-catch blocks with meaningful error messages
- [ ] Test the page class

---

## BasePage Methods Cheat Sheet

```csharp
// Finding Elements
FindElement(By locator)
FindElements(By locator)
IsElementPresent(By locator)

// Waiting
WaitForElementToBeVisible(By locator)
WaitForElementToBeClickable(By locator)
WaitForElementsCount(By locator, int count)

// Interactions
SendText(By locator, string text)
Click(By locator)
ClickUsingJavaScript(By locator)
ClickWithFallback(By locator)

// Info
GetText(By locator)
GetValue(By locator)
GetAttribute(By locator, string name)
IsElementDisplayed(By locator)
IsElementEnabled(By locator)

// JavaScript
ExecuteScript(string script, params object[] args)
ScrollIntoView(By locator)
ScrollToTop()
ScrollToBottom()

// Navigation
NavigateToUrl(string url)
GetCurrentUrl()

// Utility
Wait(int milliseconds)
GetPageTitle()
GetPageSource()
CloseBrowser()
```

---

## Tips & Best Practices

✅ **DO:**
- Define locators at the top of the page class
- Make locators readonly to prevent modification
- Use meaningful names for methods and locators
- Return `this` from methods for method chaining
- Add XML documentation to public methods
- Include error handling with console logging
- Use BasePage methods exclusively
- Group related locators together

❌ **DON'T:**
- Use direct Selenium calls in page classes
- Create new By objects inline repeatedly
- Make locators public
- Use vague method names like "click1()" or "fill()"
- Ignore exceptions
- Mix business logic with Selenium code
- Create large page classes with too many methods
- Hardcode wait times

---

## Extending BasePage for Custom Functionality

If you need custom Selenium operations, extend BasePage:

```csharp
public class CustomBasePage : BasePage
{
    public CustomBasePage(IWebDriver driver) : base(driver)
    {
    }
    
    /// <summary>
    /// Custom method: Switch to iframe and find element
    /// </summary>
    protected IWebElement FindElementInIframe(By iframeLocator, By elementLocator)
    {
        var iframe = FindElement(iframeLocator);
        Driver.SwitchTo().Frame(iframe);
        var element = FindElement(elementLocator);
        Driver.SwitchTo().DefaultContent();
        return element;
    }
}

// Use in your page class:
public class IframePage : CustomBasePage
{
    public IframePage(IWebDriver driver) : base(driver)
    {
    }
    
    public void InteractWithIframeElement()
    {
        var element = FindElementInIframe(IframeLocator, ElementLocator);
        element.Click();
    }
}
```
