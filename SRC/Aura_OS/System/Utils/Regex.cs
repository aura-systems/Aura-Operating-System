/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Implement Regex
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Utils
{
    public class Regex
    {
        private readonly List<string> patterns;

        public Regex(params string[] patterns)
        {
            this.patterns = new List<string>(patterns);
        }

        public bool IsMatch(string input)
        {
            foreach (var pattern in patterns)
            {
                if (IsMatchPattern(input, pattern))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMatchPattern(string input, string pattern)
        {
            int inputIndex = 0;
            int patternIndex = 0;

            while (inputIndex < input.Length && patternIndex < pattern.Length)
            {
                if (pattern[patternIndex] == '.')
                {
                    // Dot matches any character
                    patternIndex++;
                    inputIndex++;
                }
                else if (pattern[patternIndex] == '*')
                {
                    // Asterisk matches zero or more occurrences of the previous character
                    char prevChar = pattern[patternIndex - 1];

                    while (inputIndex < input.Length && input[inputIndex] == prevChar)
                    {
                        inputIndex++;
                    }

                    patternIndex++;
                }
                else
                {
                    // Normal character, must match exactly
                    if (pattern[patternIndex] != input[inputIndex])
                    {
                        return false;
                    }

                    patternIndex++;
                    inputIndex++;
                }
            }

            // Check for remaining characters in the pattern after the main loop
            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return inputIndex == input.Length && patternIndex == pattern.Length;
        }
    }
}