using PanGu;
using PanGu.Match;
using SimilarCompareCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimilarCompareCore
{
    public class CompareCore
    {
        /// <summary>
        /// 默认最小匹配数
        /// </summary>
        private int DefaultMinSameCount = 4;
        /// <summary>
        /// 默认最小相似度
        /// </summary>
        private double DefaultMinMatchPercent = 0.6;
        /// <summary>
        /// 默认行长度比：对比的行长度/目标行长度
        /// </summary>
        private double DefaultMinLineLengthPercent = 0.6;
        /// <summary>
        /// 设置最小匹配字数
        /// </summary>
        /// <param name="value"></param>
        public void SetDefaultMinSameCount(int value)
        {
            if (value < 1) value = 1;
            DefaultMinSameCount = value;
        }
        /// <summary>
        /// 设置最小匹配相似度
        /// </summary>
        /// <param name="value"></param>
        public void SetDefaultMinMatchPercent(double value)
        {
            if (value <=0) value = 0.1;
            DefaultMinMatchPercent = value;
        }
        /// <summary>
        /// 设置默认行长度比
        /// </summary>
        /// <param name="value"></param>
        public void SetDefaultMinLineLengthPercent(double value)
        {
            if (value <= 0) value = 0.5;
            DefaultMinLineLengthPercent = value;
        }

        /// <summary>
        /// 通过分词将内容拆分
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual IEnumerable<string> GetWordList(string content)
        {
            //TODO 将分词结果缓存起来

            var matchOptions = new MatchOptions();
            matchOptions.FrequencyFirst = true;
            Segment segment = new Segment();
            var words = segment.DoSegment(content, matchOptions).Select(word => word.Word);
            return words;
        }

        /// <summary>
        /// 拆分内容成行
        /// </summary>
        /// <param name="content"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public  List<LineModel> GetLines(string content, string pattern)
        {
            /*
             * 根据正则来拆分，同时，根据正则匹配的结果，把拆分的分隔符记录下来
             * 因为每次拆分都会是根据匹配的符号拆分，所以必定是拆分的每一段内容都有相应的分隔符
             * 将内容和分割符单独保存下来，方便后面重新将一句句的重新拼接起来
             * 
             */
            var regex = new Regex(pattern);
            if (!regex.IsMatch(content)) return null;
            var matches = regex.Matches(content);
            var lines = regex.Split(content);

            var LineList = new List<LineModel>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;

                var lineModel = new LineModel { Content = lines[i], RedTagIDList = new List<int>(), SameWordCountList = new List<int>(), SimilarPercentList = new List<double>() };
                if (i < matches.Count)
                {
                    var spliter = matches[i].Value;
                    if (string.IsNullOrEmpty(spliter) || string.IsNullOrWhiteSpace(spliter)) 
                        spliter = "\r\n";
                    lineModel.Spliter = spliter;
                }
                LineList.Add(lineModel);
            }
            return LineList;
        }

        /// <summary>
        /// 行跟行进行比较
        /// </summary>
        /// <param name="fromLine"></param>
        /// <param name="toLine"></param>
        private void CompareLine(ref LineModel fromLine, ref LineModel toLine, int redTagID)
        {
            if (string.IsNullOrEmpty(fromLine.Content) || string.IsNullOrWhiteSpace(fromLine.Content)) return;
            if (string.IsNullOrEmpty(toLine.Content) || string.IsNullOrWhiteSpace(toLine.Content)) return;

            var lineResult = CompareLineHandler(fromLine.Content, toLine.Content);
            if (lineResult == null) return;

            if(lineResult.IsMatch)
            {
                fromLine.RedTagIDList.Add(redTagID);
                fromLine.SimilarPercentList.Add(lineResult.MatchPercent);
                fromLine.SameWordCountList.Add(lineResult.SameCount);

                toLine.RedTagIDList.Add(redTagID);
                toLine.SimilarPercentList.Add(lineResult.MatchPercent);
                toLine.SameWordCountList.Add(lineResult.SameCount);
            }
        }

        /// <summary>
        /// 比较行
        /// </summary>
        /// <param name="fromLine"></param>
        /// <param name="toLine"></param>
        /// <returns></returns>
        public virtual CompareLineResultModel CompareLineHandler(string fromLine,string toLine)
        {
            //如果待比较的内容比原句短太多则不进行比较，因为长的那句出现的词多，容易造成短的那句重复率高，至于这个比例，看情况 
            if (toLine.Length * 1.00 / fromLine.Length < this.DefaultMinLineLengthPercent) return null;
            //暂时以自己写的比对算法来模拟,比对包含的字，不一定连续
            var wordList = GetWordList(fromLine);

            var regex = new Regex(string.Format("{0}", string.Join("|", wordList)));
            if (regex.IsMatch(toLine))
            {

                var matches = regex.Matches(toLine);
                var samecount = 0;
                foreach (Match match in matches)
                {
                    samecount += match.Value.Length;
                }
                //指定匹配到的字数范围
                if (samecount < this.DefaultMinSameCount) return null;
                var percent = samecount * 1.00 / toLine.Length;

                //如果匹配 则 
                if (percent >= this.DefaultMinMatchPercent)
                {
                    return new CompareLineResultModel
                    {
                        MatchPercent = percent,
                        SameCount = samecount,
                        IsMatch = true
                    };
                }
            }
            return null;
        }


        /// <summary>
        /// 比较文章内容
        /// </summary>
        /// <param name="FromContent"></param>
        /// <param name="ToContent"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public CompareArticalResultModel CompareArtical(string FromContent, string ToContent, string pattern = null)
        {
            if (string.IsNullOrEmpty(pattern) || string.IsNullOrWhiteSpace(pattern))
                pattern = @"[\,\，\.\。\;\；\?\!\！\：\:\r\n]";
            try
            {
                var fromLines = GetLines(FromContent, pattern);
                var toLines = GetLines(ToContent, pattern);
                return CompareArtical(fromLines, toLines);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 比较文章内容
        /// </summary>
        /// <param name="FromLines"></param>
        /// <param name="ToLines"></param>
        /// <returns></returns>
        public CompareArticalResultModel CompareArtical(List<LineModel> FromLines, List<LineModel> ToLines)
        {
            //每份比较都创建新的，避免对原数据产生影响
            var newFromLines = FromLines.Select(item => (LineModel)item.Clone()).ToList();
            var newToLines = ToLines.Select(item => (LineModel)item.Clone()).ToList();
            //标红 id，如果fromLine 跟 toLine 里多行匹配，那匹配的行将标记对应的id
            var redTagID = 1;

            for (var fromIndex = 0; fromIndex < newFromLines.Count; fromIndex++, redTagID++)
            {
                var fromLine = newFromLines[fromIndex];
                for (var toIndex = 0; toIndex < newToLines.Count; toIndex++)
                {
                    var toLine = ToLines[toIndex];
                    CompareLine(ref fromLine, ref toLine, redTagID);
                }
            }
            return new CompareArticalResultModel { FromLines = newFromLines, ToLines = newToLines };
        }

        /// <summary>
        /// 重排序标记顺序
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public CompareArticalResultModel ReOrderRedTagIndex(CompareArticalResultModel result)
        {
            var map = new Dictionary<int, int>();
            var index = 1;
            //将原标记跟新的 index 映射起来：建立新index与旧id的映射关系，并替换 fromlines 的旧id
            for (int i = 0; i < result.FromLines.Count; i++)
            {
                var line = result.FromLines[i];
                if(line.RedTagIDList.Count>0)
                {
                    for (int j = 0; j < line.RedTagIDList.Count; j++)
                    {
                        var oldID = line.RedTagIDList[j];
                        if(map.ContainsKey(oldID))
                        {
                            line.RedTagIDList[j] = map[line.RedTagIDList[j]];
                        }
                        else
                        {
                            line.RedTagIDList[j] = index;
                            map.Add(oldID, index);
                            index++;
                        }
                        
                    }
                }
            }
            //将原标记 更新未新的index
            for (int i = 0; i < result.ToLines.Count; i++)
            {
                var line = result.ToLines[i];
                if (line.RedTagIDList.Count > 0)
                {
                    for (int j = 0; j < line.RedTagIDList.Count; j++)
                    {
                        line.RedTagIDList[j] = map[line.RedTagIDList[j]];
                    }
                }
            }

            return result;
        }
    }
}
