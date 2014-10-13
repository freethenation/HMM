using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace NLP
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            WordDict dict = new WordDict();
            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/", "*.xml"))
            {
                Corpra corpra = new Corpra();
                corpra.Load(file); 
                dict.UpdateCount(corpra.Sentences.SelectMany(i => i));
            }
            Tuple<int, int> totalCorrect = Tuple.Create(0, 0);
            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/validate", "*.xml"))
            {
                Corpra correctCorpra = new Corpra();
                correctCorpra.Load(file);
                Corpra guessCorpra = new Corpra();
                guessCorpra.Load(file, (word, trash) => 
                    {
                        if(dict.Words.ContainsKey(word.ToLower()))
                            return dict.Words[word.ToLower()].MostCommonTag.Key;
                        return Tags.OTHER;
                    });
                var correct = correctCorpra.PercentCorrect(guessCorpra);
                totalCorrect = Tuple.Create(totalCorrect.Item1 + correct.Item1, totalCorrect.Item2 + correct.Item2);
            }
            Console.WriteLine(string.Format("{0} / {1}", totalCorrect.Item1, totalCorrect.Item2));
            Console.WriteLine(totalCorrect.Item1 / (double)totalCorrect.Item2);
        }
    }
}
