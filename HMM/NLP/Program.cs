using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using HMM;

namespace NLP
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //DumpTagger();
            MarkovTagger();
        }

        public static void MarkovTagger()
        {
            //Build Dicts
            WordDict dict = new WordDict();
            WordDict dictInital = new WordDict(); //stores words occur first in a sentence
            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/", "*.xml"))
            {
                Corpra corpra = new Corpra();
                corpra.Load(file); 
                dict.UpdateCount(corpra.Sentences.SelectMany(i => i));
                dictInital.UpdateCount(corpra.Sentences.Select(i => i.First()));
            }
            //Create Markov Model
            HMM.HMM hmm = new HMM.HMM(Enum.GetNames(typeof(Tags)), Enum.GetNames(typeof(Tags)));
            //Set Inital Probabilities
            {
                var tagCounts = dictInital.Words
                    .SelectMany(i => i.Value.TagCounts.AsEnumerable())
                    .GroupBy(i => i.Key)
                    .OrderBy(i => (int)i.Key)
                    .Select(i => i.Sum(j => j.Value))
                    .ToArray();
                var tagTotal = tagCounts.Sum();
                hmm.IntialStateProbabilities.SetValues(tagCounts.Select(i => i / (double)tagTotal).ToArray());
            }
            //hmm.IntialStateProbabilities.SetValues(dictInital.Words.SelectMany(i => i.Value));
        }

        /// <summary>
        /// Tagger that just chooses the most common way a word is used
        /// </summary>
        public static void DumpTagger()
        {
            // Build Dict
            WordDict dict = new WordDict();
            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/", "*.xml"))
            {
                Corpra corpra = new Corpra();
                corpra.Load(file); 
                dict.UpdateCount(corpra.Sentences.SelectMany(i => i));
            }
            // Test Dict on unseen text
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
            Console.WriteLine("Dump Tagger");
            Console.WriteLine(string.Format("{0} / {1}", totalCorrect.Item1, totalCorrect.Item2));
            Console.WriteLine(totalCorrect.Item1 / (double)totalCorrect.Item2);
            Console.WriteLine();
        }
    }
}
