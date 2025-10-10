using NUnit.Framework;
using SpecFlowLogin.Helpers.DebugTools;


    [SetUpFixture]
    public class SetUpFixture
    {



    [OneTimeSetUp] 
        public void RunBeforeAnyTests()
        {
   


        Debug.Log($"Arrancan las pruebas... ");

         }

        [OneTimeTearDown] // TODO acciones al final todo
        public void RunAfterAllTests()
        {
        Debug.Log($"Finalizan las pruebas");

         }


  
    }       


