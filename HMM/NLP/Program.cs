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

            Corpora[] texts = Directory.GetFiles("/home/freethenation/Downloads/brown_tei/", "*.xml")
                .Select(i => new Corpora(i))
                .ToArray();
            //Create Markov Model
            HMM.HMM hmm = new HMM.HMM(Enum.GetNames(typeof(Tags)), Enum.GetNames(typeof(Tags)));
            //Set Inital Probabilities
            {
                WordDict dictInital = new WordDict(); //stores words that are the first word in a sentence
                dictInital.UpdateCount(texts.SelectMany(i => i.Sentences).Select(i => i.First()));
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
                Corpora corpra = new Corpora();
                corpra.Load(file); 
                dict.UpdateCount(corpra.Sentences.SelectMany(i => i));
            }
            // Test Dict on unseen text
            Tuple<int, int> totalCorrect = Tuple.Create(0, 0);
            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/validate", "*.xml"))
            {
                Corpora correctCorpra = new Corpora();
                correctCorpra.Load(file);
                Corpora guessCorpra = new Corpora();
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
