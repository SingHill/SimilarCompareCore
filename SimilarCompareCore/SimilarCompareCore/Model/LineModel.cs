using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimilarCompareCore.Model
{
    public class LineModel : BaseModel
    {
        public string Content { get; set; }
        public string Spliter { get; set; }
        /// <summary>
        /// 默认为 0 大于0 的认为 是有匹配的，将做标红处理
        /// </summary>
        public List<int> RedTagIDList { get; set; }
        public List<double> SimilarPercentList { get; set; }
        public List<int> SameWordCountList { get; set; }
    }
}
