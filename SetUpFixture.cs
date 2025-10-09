using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;


[SetUpFixture]
    public class SetUpFixture
    {

       
    public static readonly string username = Environment.GetEnvironmentVariable("USER")!;
    public static readonly string pass = Environment.GetEnvironmentVariable("PASS")!;
   


    [OneTimeSetUp] 
        public void RunBeforeAnyTests()
        {
        Debug.Log($"Arrancan las pruebas......");

    }

        [OneTimeTearDown] // TODO acciones al final todo
        public void RunAfterAllTests()
        {
        Debug.Log($"Finalizan las pruebas");

    }


  

    }


