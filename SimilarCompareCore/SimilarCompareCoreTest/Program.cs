using PanGu;
using PanGu.Match;
using SimilarCompareCore;
using SimilarCompareCore.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimilarCompareCoreTest
{
    class Program
    {
        

        static void Main(string[] args)
        {

            var compareCore = new CompareCore();
            compareCore.SetDefaultMinSameCount(4);
            compareCore.SetDefaultMinMatchPercent(0.6);
            compareCore.SetDefaultMinLineLengthPercent(0.6);


            var fromContent = string.Empty;
            var toContent = string.Empty;
            var file = Path.Combine(Environment.CurrentDirectory, "Data", "content.txt");
            using (var reader = new StreamReader(File.OpenRead(file)))
            {
                fromContent = reader.ReadToEnd();
            }

            var file2 = Path.Combine(Environment.CurrentDirectory, "Data", "content2.txt");
            using (var reader = new StreamReader(File.OpenRead(file2)))
            {
                toContent = reader.ReadToEnd();
            }

            var model = compareCore.CompareArtical(fromContent, toContent);
            compareCore.ReOrderRedTagIndex(model);


            var htmltemplate = Path.Combine(Environment.CurrentDirectory, "Data", "htmltemplate.html");
            using (var reader = new StreamReader(File.OpenRead(htmltemplate)))
            {
                var content = reader.ReadToEnd();

                var html = Path.Combine(Environment.CurrentDirectory, "Data", "out.html");
                using (var writer = new StreamWriter(File.Open(html, FileMode.Create)))
                {
                    var builder = new StringBuilder();
                    OutPutHtml(model.FromLines, builder);
                    content = content.Replace("<!--[fromcontent]-->", builder.ToString());
                    builder.Clear();
                    OutPutHtml(model.ToLines, builder);
                    content = content.Replace("<!--[tocontent]-->", builder.ToString());
                    writer.Write(content);
                }
                
            }

            //Console.ReadLine();
        }

        private static void OutPutHtml(List<LineModel> lines, StringBuilder writer)
        {
            foreach (var item in lines)
            {
                if (item.RedTagIDList.Count > 0)
                {
                    writer.Append(string.Format("<span class='red {0}' style='color:#ff0000; cursor:pointer;'>{1}{2}</span>", string.Join(" ", item.RedTagIDList.Select(index => string.Format("red_{0}", index))), item.Content, string.Join("", item.RedTagIDList.Distinct().Select(index => string.Format("[{0}]", index)))));
                }
                else
                {
                    writer.Append(item.Content);
                }
                writer.Append(item.Spliter!=null&&item.Spliter.Equals("\r\n")?"<br/>": item.Spliter);
            }
        }
    }
}
