using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gov.Jag.Embc.Public.Utils
{
    public static class CsvConverter
    {
        public static void CreateCSV<T>(this IEnumerable<T> list, string filePath)
        {
            using var csvStream = list.ToCSVStream();
            using var fs = File.Create(filePath);
            csvStream.CopyTo(fs);
        }

        public static string ToCSV<T>(this IEnumerable<T> list)
        {
            using var csvStream = list.ToCSVStream();
            using var sr = new StreamReader(csvStream);
            return sr.ReadToEnd();
        }

        public static Stream ToCSVStream<T>(this IEnumerable<T> list)
        {
            var st = new MemoryStream();
            var sw = new StreamWriter(st);
            sw.WriteLine(Header<T>());
            foreach (var item in Rows(list))
            {
                sw.WriteLine(item);
            }
            st.Seek(0, SeekOrigin.Begin);
            return st;
        }

        private static string Header<T>()
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                sb.Append(properties[i].Name + ",");
            }
            sb.Append(properties[properties.Length - 1].Name);
            return sb.ToString();
        }

        private static IEnumerable<string> Rows<T>(IEnumerable<T> list)
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();
            foreach (var item in list)
            {
                sb.Clear();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var prop = properties[i];
                    sb.Append($"{prop.GetValue(item)},");
                }
                sb.Append(properties[properties.Length - 1].GetValue(item));
                yield return sb.ToString();
            }
        }
    }
}
