# TestProject

This is a BDD testing project using ReqNroll and Selenium.

## Structure

- **Features/**: Contains .feature files written in Gherkin.
- **StepDefinitions/**: Contains the step definition classes that bind to the feature steps.
- **Hooks/**: Contains hooks for setup and teardown.
- **Drivers/**: Contains WebDriver setup utilities.

## Prerequisites

- .NET 8 or later
- Chrome browser (for Selenium)

## Running Tests

1. Restore packages: `dotnet restore`
2. Run tests: `dotnet test`

## Adding New Features

1. Create a new .feature file in the Features folder.
2. Implement step definitions in the StepDefinitions folder.
3. Use hooks in the Hooks folder for common setup/teardown.