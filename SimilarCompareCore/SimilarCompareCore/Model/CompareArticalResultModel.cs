using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimilarCompareCore.Model
{
    public class CompareArticalResultModel
    {
        public List<LineModel> FromLines { get; set; }
        public List<LineModel> ToLines { get; set; }
    }
}
