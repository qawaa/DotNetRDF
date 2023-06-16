using System;
using System.Data;
using System.IO;
//using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using VDS.RDF.Writing;


namespace DTopology
{

    /// <summary>
    /// Обеспечивает возможность выполнения автоматического запуска множества тестов,<br/>
    /// сгруппированных по темам.<br/>
    /// Каждая тема представлена стандартно оформленным методом.<br/>
    /// Формирует отчет о выполнении тестов.<br/>
    /// Общие методы обеспечивают формирование файла<br/>
    /// с именами групп тестов и номерами тестов, завершившихся неудачей.<br/>
    /// </summary>
    public class TestDriver
	{
        /// <summary>
        /// Посимвольное сравнение 2х текстовых файлов.
        /// </summary>
        public bool CompareTestEtalFiles(string fName_1, String fName_2)
		{
			// Читаем тестовый и эталонный файлы и сравниваем 
			bool testOK = true;
			StreamReader sr1 = File.OpenText(fName_1);
			StreamReader sr2 = File.OpenText(fName_2);
			string std = null,sed = null;
			while((std = sr1.ReadLine()) != null)
			{
				if((sed = sr2.ReadLine()) == null){testOK = false;break;}
				if(std != sed)testOK = false;
			}
			if((sed = sr2.ReadLine()) != null)testOK = false;
			sr1.Close();
			sr2.Close();

			return testOK;
		}

        /// <summary>
        /// Запись в файл отчета sw о неудаче теста.<br/>
        /// nTest - номер не прошедшего теста в группе testedObjectName.<br/>
        /// Метод увеличивает на 1 значение общего числа ошибок faults.<br/>
        /// </summary>
        public void Fault(StreamWriter sw, string testedObjectName, ref int faults, int nTest)
		{
			sw.Write(testedObjectName);
			sw.WriteLine("	{0}",nTest);
			faults++;
		
		}

        /// <summary>
        /// Вывод в файл общего числа тестов tests и числа не прошедших тестов faults.
        /// </summary>
        public void TestResults(int tests, int faults)
		{
            System.IO.StringWriter str = new System.IO.StringWriter();
            String str1;
			if(faults == 0)
			{
                str.Write("\nAll {0} tests OK",tests);
				str1 = str.ToString();
                Console.WriteLine(str1);
			}
			else
			{
				str.Write("\n{0} / {1} fault tests",faults,tests);
				str1 = str.ToString();
                Console.WriteLine(str1);
            }
              
						
		}// TestResults.

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Группа тестов Common.
        /// </summary>
        public void Test_Common(StreamWriter testResults, ref int tests, ref int faults)
        {
            int nTest = 0;// Нумерация тестов в пределах каждой группы начинется с 1.
            // В данной группе тесты не нумеруются.
            // Группа содержит примеры применения некоторых методов dotNetRDF. 
            string testedObjectName = "Common";

            DTree dtree = new DTree();

            // Загрузка дерева (графа) в формате turtle из файла.
            FileLoader.Load(dtree, "../../../Data/data.ttl");

            // Пример сериализации графа в формате Turtle.
            // Извлечение данных из zip архива надо выполнять архиватором 7-Zip.
            // Обратите внимание, что триплеты сортированы в лексикографическом порядке. Как это отключить?
            GZippedTurtleWriter turtle_1 = new GZippedTurtleWriter();
            turtle_1.Save(dtree, "../../../Data/HelloWorld.zip");

            foreach (Triple t in dtree.Triples)
            {
                // Uri.UnescapeDataString перекодирует символы из кодировки URL в UTF8.
                // Кодировка URL символы кириллицы кодирует escape последовательностями,
                // например, символ 'д' кодируется в URL: %D0%B4 .
                string s = t.Subject.ToString();
                string sub = Uri.UnescapeDataString(s);

                string? ss = dtree.DName(t.Subject.ToString());
                string? sss = dtree.DName(t.Subject);

                sub = sub.Substring(sub.LastIndexOf('/') + 1, sub.Length - sub.LastIndexOf('/') - 1);
                string pred = Uri.UnescapeDataString(t.Predicate.ToString());
                string obj = Uri.UnescapeDataString(t.Object.ToString());
            }


            /////////////////////////////////////////////////////////////////////////////
            tests += nTest;
        }

        /// <summary>
        /// Группа тестов класса DTree.
        /// </summary>
        public void Test_DTree(StreamWriter testResults, ref int tests, ref int faults)
        {
            int nTest = 0;// Нумерация тестов в пределах каждой группы начинется с 1.
            string testedObjectName = "DTree";

            DTree dtree = new DTree();

            // Загрузка дерева (графа) в формате turtle из файла.
            FileLoader.Load(dtree, "../../../DTree/DTree.ttl");

            // 1 DName(string? uriname)
            nTest++;
            IUriNode uri = dtree.CreateUriNode(":кириллица");
            if(dtree.DName(uri.ToString()) != "кириллица")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 2 DName(INode uri)
            nTest++;
            uri = dtree.CreateUriNode(":кириллица");
            if (dtree.DName(uri) != "кириллица")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 3 Def_subject
            nTest++;
            if(dtree.Def_subject("становление") != "бытие")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 4 Def_subject null
            nTest++;
            if (dtree.Def_subject(null) != null)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 5 Def_subject аргумент - корень, "одно".
            nTest++;
            if (dtree.Def_subject("одно") != "")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 6 DA true
            nTest++;
            if(dtree.DA("становление", "одно") != true)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 7 DA false
            nTest++;
            if (dtree.DA("становление", "иное") != false)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 8 Загрузка словаря понятий из текстового файла.
            nTest++;
            if(dtree.LoadNotionsDictionary("../../../DTree/DTreeDictionaryV2.txt") != null)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 9 Сохранение словаря понятий в файле SaveNotionsDictionary(string path)
            nTest++;
            dtree.SaveNotionsDictionary("../../../DTree/DTreeDictionary_SaveV2.txt");
            if(CompareTestEtalFiles("../../../DTree/DTreeDictionaryV2.txt", 
                "../../../DTree/DTreeDictionary_SaveV2.txt") == false)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 10 Загрузка словаря с дубликатом понятия.
            nTest++;
            if (dtree.LoadNotionsDictionary("../../../DTree/DTreeDictionary_doubleV2.txt") == "одно")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 11 Проверка того, что все противоположности содержатся в словаре. All_opposites_is_present() 
            nTest++;
            dtree.LoadNotionsDictionary("../../../DTree/DTreeDictionaryV2.txt");
            if (dtree.All_opposites_is_present() != null)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 12 Проверка того, что все противоположности содержатся в словаре. All_opposites_is_present() 
            // случай, когда противоположность отсутствует.
            nTest++;
            dtree.LoadNotionsDictionary("../../../DTree/DTreeDictionary_opposite_absentV2.txt");
            if (dtree.All_opposites_is_present() == "нечто")
                Fault(testResults, testedObjectName, ref faults, nTest);

            // Необходима еще проверка того, что все понятия и противоположности словаря д-определены.

            // 13 Ветка DA, когда Sn(a) > Sn(d), т.е. предок на пути ниже потомка, случай false.
            nTest++;
            if (dtree.DA("одно", "становление") != false)
                Fault(testResults, testedObjectName, ref faults, nTest);

            /////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            // Проверка новых тестов

            //NCA - nearest common ancestor, ближайший общий предок 2х понятий
            //Для понятий "становление" и "иное_становлению" -
            //Путь от "становление" к корню: становление -> бытие -> одно
            //Путь от "иное_становлению" к корню: иное_становлению -> бытие -> одно
            //ближайшим общим предком будет "бытие".
            nTest++;    //14
            if (dtree.NCA("становление", "иное_становлению") != "бытие")
                Fault(testResults, testedObjectName, ref faults, nTest);

            nTest++;    //15
            if (dtree.NCA("ничто", "иное_становлению") != "одно")
                Fault(testResults, testedObjectName, ref faults, nTest);

            nTest++;    //16
            if (dtree.NCA("бытие", "ничто") != "одно")
                Fault(testResults, testedObjectName, ref faults, nTest);

            nTest++;    //17
            if (dtree.NCA("прехождение", "бытие") != "бытие")
                Fault(testResults, testedObjectName, ref faults, nTest);

            //NearestNotion - Выбор из пары понятий ближайшего к третьему (nt0,nt1,nt)            
            //Возвращает 0, если понятия сравнимы и nt0 ближе к nt, чем nt1.
            //Возвращает 1, если понятия сравнимы и nt1 ближе к nt, чем nt0.
            //Возвращает 2, если понятия сравнимы и nt0 и nt1 одинаково близки к nt.
            //Расстояние между "становление" и "бытие" равно 1 , а расстояние между "становление" и "возникновение" равно 0
            //Ближайшее понятие к "становление" в данной паре понятий - "возникновение".

            nTest++;    //18
            if (dtree.NearestNotion("бытие", "возникновение", "становление") != 1)
                Fault(testResults, testedObjectName, ref faults, nTest);

            nTest++;    //19
            if (dtree.NearestNotion("бытие", "возникновение", "одно") != 2)
                Fault(testResults, testedObjectName, ref faults, nTest);

            nTest++;    //20
            if (dtree.NearestNotion("бытие", "становление", "одно") != 2)
                Fault(testResults, testedObjectName, ref faults, nTest);

            //NearestVec - Выбор из пары векторов понятий ближайшего к третьему
            //v0 и v1 равноудалены от v - return 2
            //v0 ближе к v, чем v1 - return 0
            //v1 ближе к v, чем v0 - return 1
            //v0, v1 и v не сравнимы - return "не сравнимы"
            nTest++;    //21
            string[] v0 = new string[] { "одно", "иное", "бытие" };
            string[] v1 = new string[] { "бытие", "иное_становлению", "ничто" };
            string[] v = new string[] { "ничто", "прехождение", "становление" };
            if (dtree.NearestVec(v0, v1, v) != 1)
                Fault(testResults, testedObjectName, ref faults, nTest);

            //NearestArrVec - Выбор из массива векторов понятий ближайшего к вектору v
            nTest++;    //22
            string[][] arr = new string[][]
            {
                new string[] {"одно", "иное_становлению", "бытие"},
                new string[] {"бытие", "ничто", "становление"},
                new string[] {"становление", "бытие", "возникновение"}
            };

            string[] vec = new string[] { "ничто", "иное_становлению", "бытие" };
            if (dtree.NearestArrVec(arr, vec) == null)
                Fault(testResults, testedObjectName, ref faults, nTest);


            //SortVec – Сортировка массива векторов понятий по близости к вектору v
            nTest++;   //23         
            string[][]? sortedArr = dtree.SortVec(arr, vec);

            StreamWriter sw = new StreamWriter("../../../DTree/SortVec_1.txt");
                       
            if (sortedArr == null)
            {
                sw.WriteLine("Ошибка: переданы некорректные данные.");
            }
            else
            {
                sw.WriteLine("Тест 23 - Сортировка массива векторов понятий по близости к вектору v");
                sw.WriteLine();
                sw.WriteLine("\t Массив:");
                foreach (var item in arr)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("\t Отсортированный массив:");
                foreach (var item in sortedArr)
                {
                    sw.WriteLine(string.Join(", ", item));
                }
            }
            sw.Close();

            if (CompareTestEtalFiles("../../../DTree/SortVec_1.txt",
                "../../../DTree/SortVec_1_etal.txt") == false)
                Fault(testResults, testedObjectName, ref faults, nTest);

            //
            tests += nTest;

        }

        public void Test_SemanticSelectVec(StreamWriter testResults, ref int tests, ref int faults)
        {
            int nTest = 0;// Нумерация тестов в пределах каждой группы начинется с 1.
            string testedObjectName = "SemanticSelectVec";

            DTree dtree = new DTree();

            // Загрузка дерева (графа) в формате turtle из файла.
            FileLoader.Load(dtree, "../../../DTree/DTree.ttl");

            // 1 Загрузка словаря понятий из текстового файла.
            nTest++;
            if (dtree.LoadNotionsDictionary("../../../DTree/DTreeDictionaryV2.txt") != null)
                Fault(testResults, testedObjectName, ref faults, nTest);

            // 4 SemanticSelectVec – Семантический поиск аналога вектора
            nTest++;   

            // База данных деталей группы 7411. Пример форматирования выборки теста.
            string[][] data7411 = new string[][]
            {
                new string[]
                {
                    // группа_741111 штамповка
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "штамповка",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "без_отверстий",
                    "nil"
                },

                new string[]
                {
                    // группа_741111 фрезерование
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "фрезерование",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "без_отверстий",
                    "nil"
                },

                new string[]
                {
                    // группа_741114
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "nil"
                },

                new string[]
                {
                    // группа_741114 отверстия с резьбой
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "резьба"
                },

                new string[]
                {
                    // группа_741116 фрезерование некруглых отверстий
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "с_отверстиями_некруглыми",
                    "фрезерование"
                },

                new string[]
                {
                    // группа_741118 отверстия круглые и некруглые
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "треугольник",
                    "nil",
                    "nil",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми_и_некруглыми",
                    "nil"
                },

                new string[]
                {
                    // группа_741121 
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "четырехугольник",
                    "прямой_угол",
                    "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "без_отверстий",
                    "nil"
                },

                new string[]
                {
                    // группа_741124 
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "nil",
                    
                    // Тип=многоугольник
                    "четырехугольник",
                    "прямой_угол",
                    "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "резьба"
                },

                new string[]
                {
                    // группа_741124 штамповка, без резьбы 
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "штамповка",
                    
                    // Тип=многоугольник
                    "четырехугольник",
                    "прямой_угол",
                    "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "без_резьбы"
                },

                new string[]
                {
                    // группа_741124 фрезерование, без резьбы 
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "фрезерование",
                    
                    // Тип=многоугольник
                    "четырехугольник",
                    "прямой_угол",
                    "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "без_резьбы"
                }
            };

            // Запрос выборки группы 7411
            string[] request7411 = new string[]
            {
                    // Запрос_1 
                    // Тип=деталь
                    "группа_7411_ОК_ЕСКД",
                    "фрезерование",
                    
                    // Тип=многоугольник
                    "четырехугольник",
                    "прямой_угол",
                    "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми",
                    "без_резьбы"
             };

            //// База данных деталей группы 7413.
            string[][] data7413 = new string[][]
            {
                new string[]
                {
                    // группа_741311 штамповка
                    // Тип=деталь
                    "группа_7413_ОК_ЕСКД", "штамповка",
                    
                    // Тип=криволинейная
                    "криволинейная", "выпуклая", "nil",
                    
                    // Тип=отверстие
                    "без_отверстий", "nil"
                },
                new string[]
                {
                    // группа_741314 фрезерование
                    // Тип=деталь
                    "группа_7413_ОК_ЕСКД", "фрезерование",
                    
                    // Тип=криволинейная
                    "криволинейная", "вогнутая", "nil",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми", "резьба"
                },
                new string[]
                {
                    // группа_741331
                    // Тип=деталь
                    "группа_7413_ОК_ЕСКД", "штамповка",
                    
                    // Тип=криволинейная
                    "комбинированная_выпуклая", "nil", "l_до_3в_включ",
                    
                    // Тип=отверстие
                    "без_отверстий", "nil"
                },
                new string[]
                {
                    // группа_741344 отверстия с резьбой
                    // Тип=деталь
                    "группа_7413_ОК_ЕСКД", "фрезерование",
                    
                    // Тип=криволинейная
                    "комбинированная_выпуклая", "nil", "l_св_3в",
                    
                    // Тип=отверстие
                    "с_отверстиями_круглыми", "резьба"
                },               
            };

            // Запрос выборки группы 7413
            string[] request7413 = new string[]
            {
                    // Запрос_1 
                    // Тип=деталь
                    "группа_7413_ОК_ЕСКД", "фрезерование",
                    
                    // Тип=криволинейная
                    "комбинированная_выпуклая", "nil", "l_до_3в_включ",
                   
                    // Тип=отверстие
                    "с_отверстиями_круглыми_и_некруглыми","резьба"
             };

            ////// Тест 1 операции семантического поиска группы 7411. /////////////////////////////////////////          
            int n = 3;
            string[][]? selection7411 = dtree.SemanticSelectVec(data7411, request7411, n);


            //////// Тест 2 операции семантического поиска группы 7413. /////////////////////////////////////////
            string[][]? selection7413 = dtree.SemanticSelectVec(data7413, request7413, n);

            StreamWriter sw = new StreamWriter("../../../DTree/SemantickSearch.txt");

            if (selection7411 == null || selection7413 == null)
            {
                sw.WriteLine("Ошибка: переданы некорректные данные.");
            }
            else
            {
                sw.WriteLine("Семантический поиск аналога вектора, детали группы 7411");
                sw.WriteLine();
                sw.WriteLine("\t Массив:");
                foreach (var item in data7411)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("\t Запрос:");
                foreach (var item in request7411)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("\t Выборка:");
                foreach (var item in selection7411)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("Семантический поиск аналога вектора, детали группы 7413");
                sw.WriteLine();
                sw.WriteLine("\t Массив:");
                foreach (var item in data7413)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("\t Запрос:");
                foreach (var item in request7413)
                {
                    sw.WriteLine(string.Join(", ", item));
                }

                sw.WriteLine();
                sw.WriteLine("\t Выборка:");
                foreach (var item in selection7413)
                {
                    sw.WriteLine(string.Join(", ", item));
                }
            }
            sw.Close();

            if (CompareTestEtalFiles("../../../DTree/SemantickSearch.txt",
                "../../../DTree/EtalSemantickSearch.txt") == false)
                Fault(testResults, testedObjectName, ref faults, nTest);
            //
            tests += nTest;
        }

    }// Class Test.
}
