# HOOKS Folder Documentation

## Overview

The `HOOKS` folder contains centralized test lifecycle management with two key files:

1. **TestSetupPage.cs** - Page class for managing test setup and teardown
2. **HooksConfiguration.cs** - SpecFlow hooks bindings that use TestSetupPage

This architecture separates the page logic (TestSetupPage) from the SpecFlow hooks (HooksConfiguration), following the Page Object Model pattern.

---

## Files in HOOKS Folder

### 1. TestSetupPage.cs

**Purpose**: Manages all test setup, teardown, and page initialization logic.

**Key Responsibilities**:
- Initialize WebDriver
- Initialize all page objects
- Handle scenario start/completion
- Capture screenshots on failure
- Manage scenario data/context
- Provide helper methods for test execution
- Log test steps and results

**Methods**:

#### Test Run Lifecycle
- `InitializeTestRun()` - Called at test run start
- `FinalizeTestRun()` - Called at test run end

#### Scenario Lifecycle
- `InitializeScenario()` - Called before each scenario
- `FinalizeScenario()` - Called after each scenario
- `TakeFailureScreenshot()` - Captures screenshot on failure

#### Page Object Management
- `InitializePageObjects()` - Creates all page instances
- `RegisterPageObject(key, pageObject)` - Register page in context
- `GetPageObject<T>(key)` - Retrieve page from context
- `GetAmazonPage()` - Get AmazonPage instance

#### Test Context
- `GetScenarioTitle()` - Get current scenario title
- `IsScenarioPassed()` - Check if scenario passed
- `GetTestError()` - Get test error (if any)
- `LogTestStep(description)` - Log test step
- `LogTestResult(message, success)` - Log result

#### Scenario Data Management
- `SetScenarioData(key, value)` - Store data in context
- `GetScenarioData<T>(key)` - Retrieve data from context
- `HasScenarioData(key)` - Check if data exists

#### Test Execution Helpers
- `WaitWithLogging(milliseconds, reason)` - Wait with logging
- `ExecuteAction(action, description)` - Execute and log action
- `RetryAction(action, maxAttempts, delayMs)` - Retry with logging

#### Validation
- `AssertCondition(condition, description)` - Assert condition
- `AssertEqual<T>(expected, actual, description)` - Assert equality

#### Utilities
- `PrintSessionSummary()` - Print test summary

---

### 2. HooksConfiguration.cs

**Purpose**: Contains SpecFlow hook bindings that integrate TestSetupPage.

**Hook Types**:

#### Test Run Hooks
```csharp
[BeforeTestRun]
public static void BeforeTestRun()
{
    // Executes once before entire test run starts
    // Sets up test environment
}

[AfterTestRun]
public static void AfterTestRun()
{
    // Executes once after entire test run completes
    // Generates final reports
}
```

#### Scenario Hooks
```csharp
[BeforeScenario(Order = 0)]
public void BeforeScenario(ScenarioContext scenarioContext)
{
    // Executes before each scenario
    // Creates WebDriver
    // Initializes TestSetupPage
    // Initializes page objects
}

[AfterScenario(Order = 100)]
public void AfterScenario(ScenarioContext scenarioContext)
{
    // Executes after each scenario
    // Handles cleanup
    // Takes screenshots on failure
    // Disposes WebDriver
}
```

#### Step Hooks
```csharp
[BeforeStep]
public void BeforeStep(ScenarioContext scenarioContext)
{
    // Executes before each step
    // Can be used for logging
}

[AfterStep]
public void AfterStep(ScenarioContext scenarioContext)
{
    // Executes after each step
    // Can be used for validation
}
```

#### Block Hooks
```csharp
[BeforeScenarioBlock]
public void BeforeScenarioBlock(ScenarioContext scenarioContext)
{
    // Executes before each block (Given, When, Then)
    // Useful for block-specific setup
}

[AfterScenarioBlock]
public void AfterScenarioBlock(ScenarioContext scenarioContext)
{
    // Executes after each block
    // Useful for block-specific validation
}
```

---

## How to Use

### 1. In Step Definitions

```csharp
[Binding]
public class LoginSteps
{
    private AmazonPage? _amazonPage;
    private TestSetupPage? _testSetupPage;

    [BeforeScenario]
    public void Setup(ScenarioContext scenarioContext)
    {
        _testSetupPage = scenarioContext.Get<TestSetupPage>("TestSetupPage");
        _amazonPage = _testSetupPage.GetAmazonPage();
    }

    [Given("I open the Amazon home page")]
    public void GivenIOpenTheAmazonHomePage()
    {
        _testSetupPage!.ExecuteAction(
            () => _amazonPage!.NavigateToAmazonHomePage(),
            "Navigate to Amazon home page"
        );
    }

    [When("I search for \"(.*)\"")]
    public void WhenISearchFor(string productName)
    {
        _testSetupPage!.LogTestStep($"Searching for product: {productName}");
        _amazonPage!.SearchForProduct(productName);
    }

    [Then("the cart should contain at least one item")]
    public void ThenTheCartShouldContainAtLeastOneItem()
    {
        var hasItems = _amazonPage!.CartHasItems();
        _testSetupPage!.AssertCondition(hasItems, "Cart has at least one item");
    }
}
```

### 2. Storing and Retrieving Scenario Data

```csharp
[When("I save the product name")]
public void WhenISaveTheProductName(string productName)
{
    _testSetupPage!.SetScenarioData("ProductName", productName);
    _testSetupPage.LogTestResult($"Product name saved: {productName}");
}

[Then("the saved product name should match")]
public void ThenTheSavedProductNameShouldMatch()
{
    string savedName = _testSetupPage!.GetScenarioData<string>("ProductName");
    string currentName = _amazonPage!.GetCurrentProductName();
    _testSetupPage.AssertEqual(savedName, currentName, "Product names match");
}
```

### 3. Using Retry Logic

```csharp
[When("I add product to cart with retry")]
public void WhenIAddProductToCartWithRetry()
{
    _testSetupPage!.RetryAction(
        () => _amazonPage!.SelectFirstProductAndAddToCart(),
        maxAttempts: 3,
        delayMs: 1000
    );
}
```

### 4. Adding New Page Objects

**Step 1**: Create new page class (inherits from BasePage)
```csharp
public class CheckoutPage : BasePage
{
    // Page implementation
}
```

**Step 2**: Add to TestSetupPage initialization
```csharp
private CheckoutPage? _checkoutPage;

private void InitializePageObjects()
{
    // ... existing pages ...
    
    _checkoutPage = new CheckoutPage(Driver);
    RegisterPageObject("CheckoutPage", _checkoutPage);
    Console.WriteLine("  ✓ CheckoutPage initialized");
}
```

**Step 3**: Access in step definitions
```csharp
[When("I proceed to checkout")]
public void WhenIProceedToCheckout()
{
    var checkoutPage = _testSetupPage!.GetPageObject<CheckoutPage>("CheckoutPage");
    checkoutPage.ProceedToCheckout();
}
```

---

## Test Execution Flow

```
Test Run Starts
    ↓
[BeforeTestRun] - Sets up test environment
    ↓
Scenario 1 Starts
    ↓
[BeforeScenario] - Creates WebDriver, initializes TestSetupPage
    ↓
Step 1
    ├─ [BeforeStep]
    ├─ Execute step
    └─ [AfterStep]
    ↓
Step 2
    ├─ [BeforeScenario Block] (for new block type)
    ├─ [BeforeStep]
    ├─ Execute step
    ├─ [AfterStep]
    └─ [AfterScenarioBlock]
    ↓
... more steps ...
    ↓
[AfterScenario]
    ├─ Check if passed/failed
    ├─ Take screenshot if failed
    ├─ Cleanup resources
    └─ Print summary
    ↓
Scenario 2 Starts
    ↓
... repeat ...
    ↓
[AfterTestRun] - Finalizes test run, generates reports
    ↓
Test Run Ends
```

---

## Logging Output Example

```
==================================================
🚀 INITIALIZING TEST RUN
==================================================
Start Time: 2026-05-06 10:30:45
Environment: Microsoft Windows NT 10.0.19045.0
==================================================

--------------------------------------------------
  ➤ Starting Scenario: Search and Add to Cart
--------------------------------------------------
📝 Scenario: Search and Add to Cart
🌐 Chrome browser launched for scenario
📦 Initializing page objects...
  ✓ AmazonPage initialized
✅ Scenario initialization complete

  ➤ Navigate to Amazon home page
  ✓ Navigate to Amazon home page completed

  ➤ Searching for product: phone
  ✓ Values equal: Cart has items

✅ Scenario passed

==================================================
TEST SESSION SUMMARY
==================================================
Scenario: Search and Add to Cart
Status: ✅ PASSED
Completed at: 2026-05-06 10:31:12
==================================================

==================================================
✅ TEST RUN COMPLETED
==================================================
End Time: 2026-05-06 10:31:15
==================================================
```

---

## Best Practices

1. **Always use ExecuteAction** for better logging
```csharp
// ❌ Don't
_amazonPage.SearchForProduct("phone");

// ✅ Do
_testSetupPage.ExecuteAction(
    () => _amazonPage.SearchForProduct("phone"),
    "Search for product"
);
```

2. **Use AssertCondition/AssertEqual** instead of raw asserts
```csharp
// ❌ Don't
Assert.That(hasItems, Is.True);

// ✅ Do
_testSetupPage.AssertCondition(hasItems, "Cart has items");
```

3. **Store shared data using SetScenarioData**
```csharp
// Share product name between steps
_testSetupPage.SetScenarioData("ProductName", productName);
```

4. **Retry on flaky operations**
```csharp
_testSetupPage.RetryAction(
    () => _amazonPage.SelectFirstProductAndAddToCart(),
    maxAttempts: 3
);
```

5. **Use WaitWithLogging** for explicit waits
```csharp
_testSetupPage.WaitWithLogging(2000, "Wait for page to load");
```

---

## Migration from Old Hooks

### Old Way (TestHooks.cs)
```csharp
[BeforeScenario]
public void Setup(ScenarioContext scenarioContext)
{
    _driver = WebDriverSetup.CreateChromeDriver();
    scenarioContext.Set(_driver, "WebDriver");
}
```

### New Way (HooksConfiguration.cs + TestSetupPage.cs)
```csharp
[BeforeScenario(Order = 0)]
public void BeforeScenario(ScenarioContext scenarioContext)
{
    _driver = WebDriverSetup.CreateChromeDriver();
    _testSetupPage = new TestSetupPage(_driver, scenarioContext);
    _testSetupPage.InitializeScenario();
}
```

---

## Benefits of This Architecture

✅ **Separation of Concerns**
- Hooks logic separate from page setup logic
- Each file has single responsibility

✅ **Reusable Setup Methods**
- ExecuteAction, AssertCondition, WaitWithLogging can be used anywhere
- Reduces boilerplate in step definitions

✅ **Better Logging**
- Consistent logging throughout execution
- Easy to see what's happening at each step

✅ **Easier Debugging**
- Clear failure messages
- Screenshots on failure
- Detailed session summary

✅ **Scalable**
- Easy to add new page objects
- Easy to extend hooks with new functionality
- Easy to add new assertion methods

✅ **Type-Safe**
- GetPageObject<T> provides type safety
- GetScenarioData<T> provides type safety

---

## Files Reference

- `TestSetupPage.cs` - 350+ lines, ~40 methods
- `HooksConfiguration.cs` - 250+ lines, all SpecFlow hooks

Total: ~600 lines of comprehensive test lifecycle management
