using BenchmarkDotNet.Attributes;
using Flaminco.CommitResult;
using Flaminco.CommitResult.ValueCommitResultTypes;

namespace ConsoleApp
{

    public record WithdrawalSuccess(decimal NewBalance) : TypedResult(true);


    [MemoryDiagnoser(false)]
    public class ResultPatterBenchmark
    {

        [Benchmark]
        public void CommitResultWithValue()
        {
            for (int i = 0; i < 1000; i++)
            {
                ICommitResult commitResult = new SuccessValueCommitResult<int>(5);
            }
        }


        [Benchmark]
        public void ResultWithValue()
        {
            for (int i = 0; i < 1000; i++)
            {
                TypedResult commitResult = new WithdrawalSuccess(5);
            }
        }

    }
}
