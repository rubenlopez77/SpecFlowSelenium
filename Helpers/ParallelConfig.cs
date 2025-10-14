using NUnit.Framework;


[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]

namespace SpecFlowSelenium.Helpers
{

    public static class ParallelConfig
    {
        
    }
}
