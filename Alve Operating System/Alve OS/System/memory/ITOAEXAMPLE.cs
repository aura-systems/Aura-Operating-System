/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ItoaExample - Itoa
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

namespace Aura_OS
{
    public class ITOAEXAMPLE
    {
        /// <summary>
        /// Converts an integer value to a null-terminated string using the specified base and stores the result in the array given by str parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str"></param>
        /// <param name="base"></param>
        /// <returns></returns>
        public static unsafe char itoa(int value, char* str, int @base)
        {
            char* rc;
            char* ptr;
            char* low;

            // Check for supported base.
            if (@base < 2 || @base > 36)
            {
                *str = '\0';
                return *str;
            }
            rc = ptr = str;

            // Set '-' for negative decimals.
            if (value < 0 && @base == 10)
            {
                *ptr++ = '-';
            }

            // Remember where the numbers start.
            low = ptr;

            // The actual conversion
            do
            {
                // Modulo is negative for negative value. This trick makes abs() unnecessary.
                *ptr++ = "zyxwvutsrqponmlkjihgfedcba9876543210123456789abcdefghijklmnopqrstuvwxyz"[35 + value % @base];
                value /= @base;
            }
            while (value != 0); // I think.

            // Terminating the string.
            *ptr-- = '\0';
            //Invert the numbers.
            while (low < ptr)
            {
                char tmp = *low;
                *low++ = *ptr;
                *ptr-- = tmp;
            }

            return *rc;
        }
    }
}
