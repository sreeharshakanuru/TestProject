Feature: Amazon Shopping

  Scenario: Search for iPhone 17 Pro Max and add to cart
    Given I have an Amazon web page
    When I go to the Amazon URL
    And I search for "iPhone 17 Pro Max"
    And I add the first product to the cart
    Then I should verify the product is added in the cart