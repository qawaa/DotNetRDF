using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VDS.Common;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;

namespace DT
{
    /// <summary>
    /// Диалектическое дерево, индуцирует Д-топологию.
    /// </summary>
    public class DTree : Graph
    {
        // Поле содержит словарь пар: ключ, данные,
        // обеспечивает быструю выборку данных по ключу.
        // Ключ - строковое имя понятия, без префикса - двоеточия.
        // Данные - объект класса Notion.
        // Класс Notion содержит поля параметров понятия. В данной реализации это имя понятия - противоположности, 
        // sn - уровень в ДД. Корень имеет уровень 1, его непосредственные потомки уровень 2 и т.д.
        // Словарь понятий должен содержать все понятия, таким образом, пары противоположностей представлены дважды,
        // как данные о каждой из противоположностей.
        
        IDictionary<string,Notion> notion = new Dictionary<string,Notion>();

        /// <summary>
        /// Загрузка словаря понятий из файла.<br/>
        /// Возвращает null в случае успешного занесения данных в словарь.<br/> 
        /// Если понятие уже содержится в словаре, возвращает имя понятия и прекращает ввод данных.<br/>
        /// </summary>
        public string? LoadNotionsDictionary(string path)
        {
            string? s;
            string notion_name;
            notion.Clear(); 
            using (StreamReader sr = new StreamReader(path))
            {
                while ((s = sr.ReadLine()) != null)
                {
                    // что-нибудь делаем с прочитанной строкой s
                    Notion nt = new Notion();
                    string[] param = s.Split('\t');
                    notion_name = param[0]; 
                    nt.opposite = param[1];
                    nt.sn = long.Parse(param[2]);

                    // Занесение данных понятия notion_name в словарь.
                    if (notion.TryAdd<string, Notion>(notion_name, nt) == false)
                        // Выбросить исключение: понятие с именем notion_name уже есть в словаре.
                        return notion_name;
                }
                return null;    
            }
        }

        /// <summary>
        /// Сохранение словаря понятий в файле.
        /// </summary>
        public void SaveNotionsDictionary(string path)
        {
            using (StreamWriter file = new StreamWriter(path))
                foreach (var entry in notion)
                {
                    Notion nt = entry.Value;
                    string? opposite = nt.opposite;
                    long sn = nt.sn; 

                    file.WriteLine("{0}\t{1}\t{2}", entry.Key, opposite, sn);
                }
        }

        /// <summary>
        /// Проверка того, что все противоположности понятий словаря содержатся в словаре.<br/>
        /// В случае успеха возвращает null.<br/>
        /// Если противоположность не содержится в словаре, возвращается ее имя.<br/>
        /// </summary>
        public string? All_opposites_is_present()
        {
            foreach (var entry in notion)
            {
                Notion nt = entry.Value;
                Notion? value;
                string? opposite = nt.opposite;
                if (notion.TryGetValue(opposite, out value) == false)
                    return opposite;

            }
            return null;
        }

        /// <summary>
        /// Возвращает понятие, противоположность данному name.
        /// </summary>
        public string? Opposite(string name)
        {
            Notion? nt;
            if (notion.TryGetValue(name, out nt) == false)
                return "";
            return nt.opposite;
        }

        /// <summary>
        /// Возвращает семантический номер понятия name.<br/>
        /// Корень дерева имеет sn = 1.<br/>
        /// sn непосредственных потомков корня = 2 и т.д по иерархии.<br/>
        /// </summary>
        public long Sn(string name)
        {
            Notion? nt;
            if (notion.TryGetValue(name, out nt) == false)
                return -1;
            return nt.sn;
        }

        /// <summary>
        /// Возвращает строковое имя понятия, отбрасывая префикс url.<br/>
        /// Выполняет перекодирование кириллицы из escape последовательностей в UTF8.<br/>
        /// </summary>
        public string? DName(string? uriname)
        {
            if (uriname == null) return null;
            uriname = Uri.UnescapeDataString(uriname);
            int index_prefix_end = uriname.LastIndexOf('/') + 1;
            if (index_prefix_end == -1) return null;
            return uriname.Substring(index_prefix_end, uriname.Length - index_prefix_end);
        }

        /// <summary>
        /// Возвращает строковое имя понятия, отбрасывая префикс url.<br/>
        /// Выполняет перекодирование кириллицы из escape последовательностей в UTF8.<br/>
        /// </summary>
        public string? DName(INode? uri)
        {
            if (uri == null) return null;
            return DName(uri.ToString());
        }
        /// <summary>
        /// Возвращает субъект д-определения понятия - name.<br/>
        /// Возвращает "" если name корень дерева.<br/>
        /// Возвращает null если name = null.<br/>
        /// </summary>
        public string? Def_subject(string? name)
        {
            if (name == null) return null;
            if (name == "одно") return "";
            IUriNode uri = this.CreateUriNode(":" + name);
            IEnumerable<Triple> t = this.GetTriplesWithObject(uri);
            if (t.Count() > 1) return null; // Выбросить исключение - некорректное дерево.

            return DName(t.ElementAt<Triple>(0).Subject);
        }

        /// <summary>
        /// true, если d потомок (descendant) a предок (ancestor). false иначе.
        /// </summary>
        public bool DA(string? d, string? a)
        {
            if (d == null || a == null) return false;
            if (d == "" || a == "") return false;
            if (d == a) return true;
            if (Sn(a) > Sn(d)) return false;
            
            return DA(Def_subject(d), a); 
        }
    }

    /// <summary>
    /// Параметры понятия в словаре понятий, на который ссылается поле notion объекта класса DTree.
    /// </summary>
    public class Notion
    {
        /// <summary>
        /// имя противоположности данного понятия.
        /// </summary>
        public string? opposite { get; set; }

        /// <summary>
        /// Семантический номер данного понятия. Корень ДД имеет sn = 1.<br/>
        /// Непосредственные потомки корня имеют sn = 2 и далее по иерархии дерева.<br/>
        /// </summary>
        public long sn { get; set; }
    }
}
