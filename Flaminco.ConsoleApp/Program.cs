using Flaminco.Resultify;
using System.Text.Json;

namespace Flaminco.ConsoleApp
{

    internal class Program
    {
        static async Task Main(string[] args)
        {





            List<Result> result = new List<Result>
            {
                Result.Failure(Error.Validation("one","one")),
                Result.Failure(Error.Validation("one","one")),
                Result.Failure(Error.Validation("one","one")),
                Result.Failure(Error.Validation("one","one")),
                Result.Failure(Error.Validation("one","one")),
            };



            ErrorCollection errorCollection = result.ToErrorCollection("Generia", "Gpo");


            var xx = JsonSerializer.Serialize(errorCollection);

            Console.WriteLine(xx);

            //int[] XArray = [1, 2, 3];

            //Workflow workflow = new Workflow
            //{
            //    Name = "Workflow",
            //    WorkflowType = WorkflowType.Rules,
            //    Rules =
            //    [
            //        new Rule
            //        {
            //            Key = "RuleKey",
            //            Expression = "true",
            //            Order = 1,
            //            ExpressionOutput = "X.Length",
            //            Inputs = new Dictionary<string, object>
            //            {
            //                { "X" , XArray }
            //            },
            //            OutputType = OutputType.Expression
            //        }
            //    ],
            //};


            //Dictionary<string, object?> result = Evaluator.EvaluateRules(workflow);

            //foreach (KeyValuePair<string, object?> item in result)
            //{
            //    Console.WriteLine(item);
            //}
        }
    }
}
