using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;


    [SetUpFixture]
    public class SetUpFixture
    {



    [OneTimeSetUp] 
        public void RunBeforeAnyTests()
        {
   
        try { 
            DotNetEnv.Env.Load(); 
            // TODO verificar que si existen todas las variables etc
        } 
        catch { 
            
            Console.WriteLine("ERROR: No se encontró .env");
            throw; //FailFast
        }

        Debug.Log($"Arrancan las pruebas... ");

         }

        [OneTimeTearDown] // TODO acciones al final todo
        public void RunAfterAllTests()
        {
        Debug.Log($"Finalizan las pruebas");

         }


  
    }       


