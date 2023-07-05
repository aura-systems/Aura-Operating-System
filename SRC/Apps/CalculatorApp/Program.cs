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
                int result = Evaluate(expression);
                Console.WriteLine("The result is: " + result);
            }
            catch
            {
                Console.WriteLine("Error: Invalid expression");
            }
        }

        static int Evaluate(string expression)
        {
            int number = 0;
            int result = 0;
            char operation = '+';

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (c >= '0' && c <= '9')
                {
                    number = number * 10 + (c - '0');
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    result = PerformOperation(result, number, operation);
                    operation = c;
                    number = 0;
                }
            }

            result = PerformOperation(result, number, operation);

            return result;
        }

        static int PerformOperation(int left, int right, char operation)
        {
            if (operation == '+')
            {
                return left + right;
            }
            else if (operation == '-')
            {
                return left - right;
            }
            else if (operation == '*')
            {
                return left * right;
            }
            else if (operation == '/')
            {
                if (right != 0)
                {
                    return left / right;
                };
                throw new Exception("Division by zero");
            }
            else
            {
                throw new Exception("Invalid operation");
            }
        }
    }
}
