using JiebaNet.Segmenter;
using System.Collections.Generic;
using WangJun.Utility;

namespace WangJun.AI
{
    /// <summary>
    /// 中文分词
    /// </summary>
    public static class FenCi
    {
        /// <summary>
        /// 获取分词结果
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetResult(string input, string mode = "", bool checkRepetitiveWord = false)
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            var segmenter = new JiebaSegmenter();

            var words = segmenter.Cut(input);

            var wordDict = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (2<=word.Length&& StringChecker.IsHanZi(word) || StringChecker.IsEnglish(word))
                {
                    wordDict[word] = 0;
                }

            }

            res = FenCi.GetRepetitiveWordCount(input, wordDict);

            return res;
        }


        #region 检测词汇重复情况
        /// <summary>
        /// 检测词汇重复情况
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetRepetitiveWordCount(string input, Dictionary<string, int> words)
        {
            var res = DictTools.Clone<Dictionary<string, int>>(words);
            if (!string.IsNullOrWhiteSpace(input) && null != words)
            {
                for (int k = 0; k < input.Length; k++)
                {
                    foreach (var item in words)
                    {

                        var wordLength = item.Key.Length;
                        if (k + wordLength <= input.Length)
                        {
                            var substr = input.Substring(k, wordLength);
                            if (substr == item.Key && words.ContainsKey(item.Key))
                            {
                                res[item.Key] += 1; ///添加一个计数
                            }
                        }
                    }
                }
            }
            return res;
        }

        #endregion

        #region 无用汉字

        #endregion

    }
}

     
