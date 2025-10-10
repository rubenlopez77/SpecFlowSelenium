Feature: Login functionality
  In order to access the application
  As a registered user
  I want to log in and handle errors correctly

  @smoke @login
  Scenario Outline: Successful login
    Given I am on the login page
    When I enter valid credentials
    Then I should see the dashboard


  @regression @login
  Scenario Outline: Login with invalid password
    Given I am on the login page
    When I enter wrong password
    Then I should see an error message


  @regression @login
  Scenario Outline: Login with empty credentials
    Given I am on the login page
    When I leave credentials empty
    Then I should see a validation message
