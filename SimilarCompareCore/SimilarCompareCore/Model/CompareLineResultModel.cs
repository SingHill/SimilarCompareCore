using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimilarCompareCore.Model
{
    public class CompareLineResultModel
    {
        /// <summary>
        /// 相同字数
        /// </summary>
        public int SameCount { get; set; }
        /// <summary>
        /// 相似比
        /// </summary>
        public double MatchPercent { get; set; }
        /// <summary>
        /// 是否匹配：视具体需求标记，标记为 true 则认为是重复行
        /// </summary>
        public bool IsMatch { get; set; }
    }
}
