Feature: Login functionality
  In order to access the application
  As a registered user
  I want to log in and handle errors correctly

  @smoke @login
  Scenario Outline: Successful login
    Given I am on the login page
    When I enter valid credentials
    And I click the login button
    Then I should see the dashboard


  @regression @login
  Scenario Outline: Login with invalid password
    Given I am on the login page
    When I enter wrong password
    And I click the login button
    Then I should see an error message


  @regression @login
  Scenario Outline: Login with empty credentials
    Given I am on the login page
    When I leave credentials empty
    And I click the login button
    Then I should see a validation message
