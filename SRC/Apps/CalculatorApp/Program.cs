using System;

namespace TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your mathematical expression:");
            string expression = Console.ReadLine();
            
            try
            {
                Console.WriteLine("Calculating " + expression + " ...");
                double result = Evaluate(expression);
                Console.WriteLine("The result is: " + result);
            }
            catch
            {
                Console.WriteLine("Error: Invalid expression");
            }
        }

        static double Evaluate(string expression)
        {
            double number = 0;
            double result = 0;
            char operation = '+';

            Console.WriteLine("Evaluate 1");

            for (int i = 0; i < expression.Length; i++)
            {
                Console.WriteLine("Evaluate 1.1");

                char c = expression[i];

                Console.WriteLine("Evaluate 1.2");

                Console.WriteLine("Evaluate 2, Expression = " + c.ToString());

                if (c >= '0' && c <= '9')
                {
                    Console.WriteLine("Evaluate 3");

                    number = number * 10 + (c - '0');

                    Console.WriteLine("Evaluate 4");
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    Console.WriteLine("Evaluate 5");

                    result = PerformOperation(result, number, operation);

                    Console.WriteLine("Evaluate 6");

                    operation = c;
                    number = 0;

                    Console.WriteLine("Evaluate 7");
                }
            }

            Console.WriteLine("Evaluate 8");

            result = PerformOperation(result, number, operation);

            Console.WriteLine("Evaluate 9");

            return result;
        }

        static double PerformOperation(double left, double right, char operation)
        {
            switch (operation)
            {
                case '+':
                    Console.WriteLine("PerformOperation +");
                    return left + right;
                case '-':
                    Console.WriteLine("PerformOperation -");
                    return left - right;
                case '*':
                    Console.WriteLine("PerformOperation *");
                    return left * right;
                case '/':
                    Console.WriteLine("PerformOperation /");
                    if (right != 0)
                    {
                        return left / right;
                    };
                    throw new Exception("Division by zero");
                default:
                    throw new Exception("Invalid operation");
            }
        }
    }
}
