using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;


[SetUpFixture]
    public class SetUpFixture
    {
    public static string username { get; private set; } = "user";
    public static string pass { get; private set; } = "user";



    [OneTimeSetUp] 
        public void RunBeforeAnyTests()
        {
   
        try { DotNetEnv.Env.Load(); } catch { Console.WriteLine("ERROR: No se encontró .env"); }

        username = Environment.GetEnvironmentVariable("USER") ?? "user";
        pass = Environment.GetEnvironmentVariable("PASS") ?? "pass";


        Debug.Log($"Arrancan las pruebas... ");

    }

        [OneTimeTearDown] // TODO acciones al final todo
        public void RunAfterAllTests()
        {
        Debug.Log($"Finalizan las pruebas");

    }


  

    }


