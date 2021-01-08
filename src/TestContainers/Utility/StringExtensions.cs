using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Utility
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string input, string oldValue, string newValue)
        {
            int pos;
            if ((pos = input.IndexOf(oldValue)) >= 0)
            {
                return input.Substring(0, pos) + newValue + input.Substring(pos + oldValue.Length);
            }
            return input;
        }


    }
}
