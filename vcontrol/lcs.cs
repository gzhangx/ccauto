using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    class LCS
    {
        private static int MAX(int a, int b)
        {
            return a > b ? a : b;
        }

        public static string LongestCommonSubsequence(string s1, string s2)
        {
            int i, j, k, t;
            int s1Len = s1.Length;
            int s2Len = s2.Length;
            int[] z = new int[(s1Len + 1) * (s2Len + 1)];
            int[,] c = new int[(s1Len + 1), (s2Len + 1)];

            for (i = 0; i <= s1Len; ++i)
                c[i, 0] = z[i * (s2Len + 1)];

            for (i = 1; i <= s1Len; ++i)
            {
                for (j = 1; j <= s2Len; ++j)
                {
                    if (s1[i - 1] == s2[j - 1])
                        c[i, j] = c[i - 1, j - 1] + 1;
                    else
                        c[i, j] = MAX(c[i - 1, j], c[i, j - 1]);
                }
            }

            t = c[s1Len, s2Len];
            char[] outputSB = new char[t];

            for (i = s1Len, j = s2Len, k = t - 1; k >= 0;)
            {
                if (s1[i - 1] == s2[j - 1])
                {
                    outputSB[k] = s1[i - 1];
                    --i;
                    --j;
                    --k;
                }
                else if (c[i, j - 1] > c[i - 1, j])
                    --j;
                else
                    --i;
            }

            return new string(outputSB);
        }
    }
}
