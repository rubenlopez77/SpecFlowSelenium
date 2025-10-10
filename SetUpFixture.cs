using System;
using DotNetEnv;
using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;

namespace SpecFlowSelenium;

[SetUpFixture]
public class TestSuiteSetup
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        try
        {
            Env.Load();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: No se encontró .env ({ex.Message})");
            throw;
        }

        Debug.Log("Arrancan las pruebas...");
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        Debug.Log("Finalizan las pruebas");
    }
}
