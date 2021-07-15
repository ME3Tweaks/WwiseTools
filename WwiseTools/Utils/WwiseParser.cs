﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using WwiseTools.Basics;

namespace WwiseTools.Utils
{
    /// <summary>
    /// 用于解析现有工作单元(Work Unit)的工具
    /// </summary>
    public class WwiseParser
    {
        public string[] WorkUnit => workUnit;
        string[] workUnit;

        public string FilePath => path;
        string path;

        public WwiseParser()
        {
            if (WwiseUtility.ProjectPath == null)
            {
                Console.WriteLine("WwiseUtility not initialized!");
                return;
            }
        }

        public XmlDocument Document => new XmlDocument(); // Temp

        /// <summary>
        /// 根据文件路径解析工作单元，并返回一个字符串数组
        /// </summary>
        /// <param name="file_path"></param>
        /// <returns></returns>
        public string[] Parse(string file_path)
        {
            string _path = Path.Combine(WwiseUtility.ProjectPath, file_path);
            
            //StreamReader f = new StreamReader();
            List<string> lines = new List<string>();
            StreamReader f = File.OpenText(_path);
            try
            {
                
                while (true)
                {
                    string line = f.ReadLine();
                    if (line == null || line.Trim() == "")
                    {
                        break;
                    }
                    lines.Add(line);
                }

                path = _path;

            }
            catch
            {
                Console.WriteLine("Invalid path!");
            }
            finally
            {
                f.Close();
            }

            return workUnit = lines.ToArray();
        }

        /// <summary>
        /// 根据指定字符串解析工作单元，并返回一个字符串数组
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string[] ParseText(string text)
        {
            var result = Regex.Split(text, "\r\n|\r|\n");
            return result;
        }

        /// <summary>
        /// 将对于该工作单元的修改保存，设置是否为原始工作单元创建备份(默认为true)
        /// </summary>
        /// <param name="backup"></param>
        public void CommitChange(bool backup = true)
        {
            File.Copy(path, path + ".backup", true);
            string text = "";
            foreach (var l in workUnit)
            {
                text += l + "\n";
            }
            File.WriteAllText(path, text, Encoding.UTF8);
        }


        /// <summary>
        /// 为工作单元添加子单元
        /// </summary>
        /// <param name="child"></param>
        public void AddChildToWorkUnit(WwiseUnit child)
        {
            if (WorkUnit == null)
            {
                Console.WriteLine("Parse the file first!");
                return;
            }

            if (WorkUnit[3].Contains("/>"))
            {
                WwiseWorkUnit wu = GetWorkUnit();
                wu.AddChild(child);
                workUnit = ParseText(wu.Print());

            }
            else
            {
                List<string> lines = new List<string>();
                for (int i = 0; i < 5; i++)
                {
                    lines.Add(workUnit[i]);
                }
                child.tabs = 4;
                string[] newLines = ParseText(child.Print(false));
                foreach (var l in newLines)
                {
                    lines.Add(l);
                }
                for (int i = 5; i < workUnit.Length; i++)
                {
                    lines.Add(workUnit[i]);
                }
                workUnit = lines.ToArray();
            }
        }

        /// <summary>
        /// 为单元添加子单元，需要该单元的名称、类型以及子单元
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="type"></param>
        /// <param name="child"></param>
        public void AddChildToUnit(string unitName, string type, WwiseUnit child)
        {
            if (WorkUnit == null)
            {
                Console.WriteLine("Parse the file first!");
                return;
            }
            string line = "";
            int index = 0;
            for (int i = 0; i < workUnit.Length; i++)
            {

                
                if (workUnit[i].Contains("Name=" + String.Format("\"{0}\"", unitName)))
                {
                    line = workUnit[i];
                    index = i;
                    break;
                }
            }

            if (line == "")
            {
                Console.WriteLine(type + " " +  unitName + " not found!");
                return;
            }

            int tabs = GetTabCount(line);

            if (line.Contains("/>"))
            {
                workUnit[index] = line.Replace("/>", ">");
                WwiseNode list = WwiseNode.NewChildrenList(new List<IWwisePrintable>()
                {
                    child
                });
                list.tabs = tabs + 1;
                List<string> newLines = ParseText(list.Print(false)).ToList();
                string t = "";
                for (int i = 0; i < tabs; i++)
                {
                    t += "\t";
                }
                newLines.Add(t + String.Format("</{0}>", type));

                List<string> lines = new List<string>();
                for (int i = 0; i < index + 1; i++)
                {
                    lines.Add(workUnit[i]);
                }
                foreach (var l in newLines)
                {
                    lines.Add(l);
                }
                for (int i = index + 1; i < workUnit.Length; i++)
                {
                    lines.Add(workUnit[i]);
                }
                workUnit = lines.ToArray();
            }
            else
            {
                int endIndex = -1;
                for (int i = index + 1; i < workUnit.Length; i ++)
                {
                    if (GetTabCount(workUnit[index]) == GetTabCount(workUnit[i]) && workUnit[i].Replace("\t", "").Trim() == String.Format("</{0}>", type))
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (endIndex == -1)
                {
                    Console.WriteLine("File is broken!");
                    return;
                }
                int childIndex = -1;
                for (int i = index + 1; i < endIndex; i++)
                {
                    if (GetTabCount(workUnit[index]) + 1 == GetTabCount(workUnit[i]) && workUnit[i].Replace("\t", "").Trim() == "<ChildrenList>")
                    {
                        childIndex = i;
                        break;
                    }
                }
                if (childIndex == -1) // No ChildrenList
                {
                    WwiseNode list = WwiseNode.NewChildrenList(new List<IWwisePrintable>()
                    {
                        child
                    });
                    list.tabs = tabs + 1;
                    List<string> newLines = ParseText(list.Print(false)).ToList();

                    List<string> lines = new List<string>();
                    for (int i = 0; i < endIndex; i++)
                    {
                        lines.Add(workUnit[i]);
                    }
                    foreach (var l in newLines)
                    {
                        lines.Add(l);
                    }
                    for (int i = endIndex; i < workUnit.Length; i++)
                    {
                        lines.Add(workUnit[i]);
                    }
                    workUnit = lines.ToArray();
                }
                else // Has ChildrenList
                {
                    child.tabs = tabs + 2;
                    List<string> newLines = ParseText(child.Print(false)).ToList();


                    List<string> lines = new List<string>();
                    for (int i = 0; i < childIndex + 1; i++)
                    {
                        lines.Add(workUnit[i]);
                    }
                    foreach (var l in newLines)
                    {
                        lines.Add(l);
                    }
                    for (int i = childIndex + 1; i < workUnit.Length; i++)
                    {
                        lines.Add(workUnit[i]);
                    }
                    workUnit = lines.ToArray();
                }
            }
        }

        /// <summary>
        /// 将Parser转换成字符串输出
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            foreach (var l in workUnit)
            {
                result += l + "\n";
            }
            return result;
        }

        /// <summary>
        /// 通过名称搜索单元，需要一个parser的字符串数组
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public WwiseUnit GetUnitByName(string name, string[] file)
        {
            string type = "";
            string guid = "";
            foreach (var l in file)
            {
                if (l.Contains(name))
                {   
                    var properties = l.Replace('\t', ' ').Trim().Split(' ');
                    string t = properties[0].Trim().Replace("<", "");
                    type = t.Trim();

                    int id_index = 0;
                    string n = null;



                    n = GetName(ref id_index, properties);

                    n = n.Replace("Name=\"", "");
                    n = n.Replace("\"", "");
                    name = n.Trim();

                    string g = properties[id_index].Replace("ID=\"{", "");
                    g = g.Replace("}\">", "");
                    guid = g.Trim();
                    break;
                }
            }

            return new WwiseUnit(name, type, guid);
        }

        /// <summary>
        /// 获取Parser中的工作单元信息
        /// </summary>
        /// <returns></returns>
        public WwiseWorkUnit GetWorkUnit()
        {
            return GetWorkUnit(ToString());
        }

        /// <summary>
        /// 获取XML模式版本
        /// </summary>
        /// <returns></returns>
        public int GetSchemaVersion()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ToString());
            return Int32.Parse(doc.ChildNodes[1].Attributes[2].Value);
        }

        /// <summary>
        /// 获取指定字符串中的工作单元信息
        /// </summary>
        /// <param name="txt_file"></param>
        /// <returns></returns>
        public WwiseWorkUnit GetWorkUnit(string txt_file)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(txt_file);
            //Console.WriteLine(doc.ChildNodes[1].Attributes[0].Value);

            string name = doc.ChildNodes[1].FirstChild.FirstChild.Attributes[0].Value;
            string id = doc.ChildNodes[1].FirstChild.FirstChild.Attributes[1].Value.Replace("{", "").Replace("}", "").Trim();
            string type = doc.ChildNodes[1].FirstChild.Name;

            return new WwiseWorkUnit(name, type, id);

        }

        private string GetName(ref int index, string[] properties)
        {
            string n = "";
            foreach (var s in properties)
            {
                if (s.StartsWith("Name="))
                {
                    n = "";
                }
                if (n != null && !s.StartsWith("ID="))
                {
                    n += s + " ";
                }
                if (s.StartsWith("ID="))
                {
                    foreach (var j in properties)
                    {
                        if (j == s)
                        {
                            break;
                        }
                        index += 1;
                    }
                    break;
                }

            }

            return n;
        }

        private int GetTabCount(string line)
        {
            return line.Count(c => c == '\t');
        }
    }
}
