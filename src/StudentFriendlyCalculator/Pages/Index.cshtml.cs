using System.Net.NetworkInformation;
using Antlr.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NCalc;
namespace StudentFriendlyCalculator.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public class UserInput
    {
        public required string Name { get; set; }
    }


    //The function below is meant to help parse the user input so that it matches the capitalization NCalc is expecting.
    //This is proving difficult because I cannot find a list which states NCalc's functions anywhere.
    //Another unique problem I have is that words are not always separated by spaces, but may be separated by paranthesis or operators. Using the split function would not do, the deliminators are still critical parts of the string.
    //See Allyn for questions.
    // private string ReformatString(string originalMessage)
    // {
    //     originalMessage = originalMessage.ToLower();
    //     List<string> strings = new();
    //     string parseMe = "";
    //     string newMessage = "";
    //     foreach (char c in originalMessage)
    //     {
    //         if (Char.IsLetter(c))
    //         {
    //             parseMe += c;
    //         }
    //         else
    //         {
    //             if (parseMe != "")
    //             {
    //                 newMessage += capitalizeWord(parseMe);
    //                 parseMe = "";
    //             }
    //             if (parseMe != "x")
    //                 newMessage += c;
    //         }
    //     }
    //     //This next line is just in case the final letter in the string was a letter, not a symbol. The above loop would have never caught such a situation. For example, "pi" or "e" would fit this case.
    //     if (parseMe != "")
    //     {
    //         newMessage += capitalizeWord(parseMe);
    //     }
    //     Console.WriteLine("Parsed \"" + originalMessage + "\", into:\"" + newMessage + "\"");
    //     return newMessage;
    // }


    //See Allyn for questions.
    //Capitalizes the word if it is not a pi or e
    private string capitalizeWord(string parseMe)
    {
        char firstLetter = parseMe[0];
        string otherLetters = parseMe.Remove(0, 1);
        parseMe = firstLetter.ToString().ToUpper() + otherLetters;
        return parseMe;
    }

    private double Factorial(double n)
    {
        if (n < 0 || n != Math.Floor(n))
            throw new ArgumentException("Factorial only defined for non-negative integers.");

        double result = 1;
        for (int i = 2; i <= (int)n; i++)
        {
            result *= i;
        }
        return result;
    }


    public IActionResult OnPostCalcSubmission([FromBody] UserInput input)
    {
        Console.WriteLine("Received: " + input.Name); // The math expression

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return new JsonResult(new { error = "Expression cannot be empty." });
        }

        // input.Name = ReformatString(input.Name);

        var expr = new Expression(input.Name);

        // Add support for constants like pi
        expr.EvaluateParameter += (name, args) =>
        {
            if (name.ToLower() == "pi")
                args.Result = Math.PI;
            if (name.ToLower() == "e")
                args.Result = Math.E;
        };

        // Add support for factorial(n)
        EvaluateParameterHandler parameterHandler = (name, args) =>
        {
            if (name.ToLower() == "pi")
                args.Result = Math.PI;
            else if (name.ToLower() == "e")
                args.Result = Math.E;
        };

        // process factorial with recursive evaluate
        EvaluateFunctionHandler functionHandler = null;
        functionHandler = (name, args) =>
        {
            if (name.ToLower() == "factorial")
            {
                if (args.Parameters.Length != 1)
                    throw new ArgumentException("factorial() requires exactly one argument.");

                object evaluated = args.Parameters[0].Evaluate();

                if (evaluated is int intVal)
                {
                    args.Result = Factorial(intVal);
                }
                else if (evaluated is long longVal)
                {
                    args.Result = Factorial((double)longVal);
                }
                else if (evaluated is double doubleVal)
                {
                    args.Result = Factorial(doubleVal);
                }
                else
                {
                    throw new ArgumentException("Invalid argument to factorial()");
                }

            }
        };


        expr.EvaluateParameter += parameterHandler;
        expr.EvaluateFunction += functionHandler;

        try
        {
            var result = expr.Evaluate();
            Console.WriteLine($"Result: {result}");
            return new JsonResult(new { result });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Error: " + ex.Message);
            return new JsonResult(new { error = "Invalid expression: " + ex.Message });
        }
    }

}
