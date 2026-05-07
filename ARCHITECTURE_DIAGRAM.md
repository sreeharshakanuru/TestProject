# Architecture Layers Diagram

## Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    FEATURE FILES (BDD)                       │
│            Features/Login.feature                            │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Scenario: Search and Add to Cart                        ││
│  │   Given I open the Amazon home page                     ││
│  │   When I search for "phone"                             ││
│  │   And I select the first product and add to cart        ││
│  │   Then the cart should contain at least one item        ││
│  └─────────────────────────────────────────────────────────┘│
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│           STEP DEFINITIONS (Test Layer)                      │
│        StepDefinitions/LoginSteps.cs                        │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ [Given] GivenIOpenTheAmazonHomePage()                   ││
│  │ ├─→ _amazonPage?.NavigateToAmazonHomePage()            ││
│  │                                                          ││
│  │ [When] WhenISearchFor(string productName)               ││
│  │ ├─→ _amazonPage?.SearchForProduct(productName)         ││
│  │                                                          ││
│  │ [When] WhenISelectFirstProductAndAddToCart()            ││
│  │ ├─→ _amazonPage?.SelectFirstProductAndAddToCart()      ││
│  │                                                          ││
│  │ [Then] ThenTheCartShouldContainAtLeastOneItem()        ││
│  │ ├─→ _amazonPage?.CartHasItems()                        ││
│  └─────────────────────────────────────────────────────────┘│
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│          PAGE CLASSES (Business Logic Layer)                 │
│            Pages/LoginPage.cs (AmazonPage)                  │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ public class AmazonPage : BasePage                      ││
│  │                                                          ││
│  │ public AmazonPage NavigateToAmazonHomePage()            ││
│  │ ├─→ NavigateToUrl(url)              [BasePage]         ││
│  │ ├─→ Wait(2000)                      [BasePage]         ││
│  │                                                          ││
│  │ public AmazonPage SearchForProduct(string name)         ││
│  │ ├─→ SendText(SearchBoxLocator, name) [BasePage]        ││
│  │ ├─→ Click(SearchButtonLocator)       [BasePage]        ││
│  │ ├─→ WaitForElementsCount(...)        [BasePage]        ││
│  │                                                          ││
│  │ public AmazonPage SelectFirstProductAndAddToCart()      ││
│  │ ├─→ FindFirstProduct()               [Helper]          ││
│  │ ├─→ ScrollIntoView(...)              [BasePage]        ││
│  │ ├─→ ClickWithFallback(...)           [BasePage]        ││
│  │                                                          ││
│  │ public bool CartHasItems()                              ││
│  │ ├─→ FindElements(CartItemsLocator)   [BasePage]        ││
│  │                                                          ││
│  │ LOCATORS (Field Definitions)                             ││
│  │ • SearchBoxLocator = By.Id(...)                          ││
│  │ • SearchButtonLocator = By.Id(...)                       ││
│  │ • CartItemsLocator = By.XPath(...)                       ││
│  └─────────────────────────────────────────────────────────┘│
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│   SELENIUM ABSTRACTION LAYER (Shared Code)                   │
│            Shared/BasePage.cs                               │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ ELEMENT FINDING                                         ││
│  │ • FindElement(By) → IWebElement                         ││
│  │ • FindElements(By) → IList<IWebElement>                 ││
│  │ • IsElementPresent(By) → bool                           ││
│  │                                                          ││
│  │ WAITS                                                    ││
│  │ • WaitForElementToBeVisible(By)                         ││
│  │ • WaitForElementToBeClickable(By)                       ││
│  │ • WaitForElementsCount(By, count)                       ││
│  │                                                          ││
│  │ INTERACTIONS                                             ││
│  │ • SendText(By, string)                                  ││
│  │ • Click(By)                                             ││
│  │ • ClickUsingJavaScript(By)                              ││
│  │ • ClickWithFallback(By)                                 ││
│  │ • GetText(By) → string                                  ││
│  │ • GetAttribute(By, name) → string                       ││
│  │                                                          ││
│  │ JAVASCRIPT                                               ││
│  │ • ExecuteScript(string, args)                           ││
│  │ • ScrollIntoView(By)                                    ││
│  │ • ScrollToTop()                                         ││
│  │ • ScrollToBottom()                                      ││
│  │                                                          ││
│  │ NAVIGATION                                               ││
│  │ • NavigateToUrl(string)                                 ││
│  │ • GetCurrentUrl() → string                              ││
│  └─────────────────────────────────────────────────────────┘│
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│           WEBDRIVER (Selenium Core)                          │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ IWebDriver (Chrome/Firefox/Edge)                        ││
│  │ ↓                                                        ││
│  │ WebDriverWait                                            ││
│  │ ↓                                                        ││
│  │ Selenium WebDriver Protocol                             ││
│  │ ↓                                                        ││
│  │ Browser Automation                                       ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

---

## Data Flow Example: Search Operation

```
User runs test scenario
    ↓
Reqnroll/SpecFlow parses feature file
    ↓
Executes [When] step: "I search for 'phone'"
    ↓
Step Definition calls:
    _amazonPage?.SearchForProduct("phone")
    ↓
Page Method (LoginPage.cs):
    public AmazonPage SearchForProduct(string productName)
    {
        SendText(SearchBoxLocator, "phone");      ← Call 1
        Click(SearchButtonLocator);               ← Call 2
        WaitForElementsCount(...);                ← Call 3
    }
    ↓ (Each call goes to BasePage)
BasePage Method: SendText(By, string)
    {
        var element = WaitForElementToBeVisible(locator);
        element.Clear();
        element.SendKeys(text);
    }
    ↓
BasePage Method: Click(By)
    {
        var element = WaitForElementToBeClickable(locator);
        element.Click();
    }
    ↓
BasePage Method: WaitForElementsCount(By, count)
    {
        Wait.Until(d => d.FindElements(locator).Count >= count);
    }
    ↓
IWebDriver (Chrome)
    ↓
Browser automation
    ↓
✅ Search completed successfully
```

---

## Class Hierarchy

```
IWebDriver (Selenium Interface)
    ↓
WebDriver Implementation (ChromeDriver, FirefoxDriver, etc.)
    ↓
┌───────────────────────────────────┐
│         BasePage                  │
│    (Shared/BasePage.cs)           │
│                                   │
│  protected IWebDriver Driver      │
│  protected WebDriverWait Wait     │
│                                   │
│  protected methods:               │
│  • FindElement()                  │
│  • SendText()                     │
│  • Click()                        │
│  • etc...                         │
└───────────┬───────────────────────┘
            │
            ├─────────────────────┐
            ↓                     ↓
    ┌─────────────────┐   ┌──────────────┐
    │  AmazonPage     │   │  ProductPage │
    │  (inherits      │   │  (inherits   │
    │   BasePage)     │   │   BasePage)  │
    │                 │   │              │
    │ public methods: │   │ public       │
    │ • Search()      │   │ methods:     │
    │ • AddToCart()   │   │ • GetPrice() │
    │ • ViewCart()    │   │ • AddToCart()│
    └─────────────────┘   └──────────────┘
            ↑                   ↑
            │                   │
    Used by AmazonSteps  Used by ProductSteps
```

---

## Key Principles

```
┌──────────────────────────────────────┐
│   SEPARATION OF CONCERNS             │
└──────────────────────────────────────┘

Selenium Code Layer
└─→ Contains: All driver operations, waits, element handling
└─→ Location: Shared/BasePage.cs
└─→ Access: Protected methods
└─→ Usage: Only from page classes

Page Logic Layer
└─→ Contains: Page-specific actions, locators
└─→ Location: Pages/*.cs
└─→ Access: Public methods
└─→ Usage: Only from step definitions

Test Logic Layer
└─→ Contains: BDD step implementations
└─→ Location: StepDefinitions/*.cs
└─→ Access: SpecFlow bindings
└─→ Usage: From feature files

┌──────────────────────────────────────┐
│   EACH LAYER FOCUSES ON:             │
│   • ONE RESPONSIBILITY               │
│   • ONE REASON TO CHANGE             │
│   • EASY TO TEST INDEPENDENTLY       │
│   • EASY TO REUSE                    │
└──────────────────────────────────────┘
```

---

## Before vs After Architecture

### BEFORE (Tightly Coupled)
```
Feature File
    ↓
Step Definition
    ↓
Page Class
    ├─ Selenium Code
    ├─ Page Logic
    ├─ WebDriver calls
    └─ Mixed concerns
    ↓
IWebDriver
```

**Problem**: Selenium code scattered, hard to maintain, difficult to reuse


### AFTER (Well-Structured)
```
Feature File
    ↓
Step Definition
    ↓
Page Class (Business Logic Only)
    ├─ Locators
    ├─ Page Actions
    └─ Calls BasePage methods
        ↓
    BasePage (Selenium Only)
    ├─ All WebDriver operations
    ├─ All wait strategies
    └─ All element interactions
        ↓
    IWebDriver
```

**Benefit**: Clear separation, easy to maintain, highly reusable ✅
