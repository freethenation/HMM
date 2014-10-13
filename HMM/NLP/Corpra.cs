using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using HMM;

namespace NLP
{
    public class Word
    {   
        public Word(string name, Tags tag)
        {
            Name = name;
            Tag = tag;
        }
        public readonly string Name;
        public readonly Tags Tag;
    }

    public class Corpra
    {
        public List<List<Word>> Sentences;
        public Corpra()
        {

        }
        public void Load(string path, Func<string, Tags, Tags> tagFunc = null)
        {
            if (tagFunc == null) tagFunc = (word, tag) => tag;
            Sentences = new List<List<Word>>();
            XDocument doc = XDocument.Load(path);
            var ns = doc.Root.Name.Namespace;
            foreach (var s in doc.Root.Descendants(ns + "s")) {
                List<Word> sentence = new List<Word>();
                foreach (var w in s.Descendants()) {
                    if (w.Attribute("type") != null)
                    {
                        Tags tag = tagFunc(w.Value, Corpra.ParseTag(w.Attribute("type").Value));
                        sentence.Add(new Word(w.Value, tag));
                    }
                    else
                    {
                        Tags tag = tagFunc(w.Value, Corpra.ParseTag(w.Attribute("pos").Value));
                        sentence.Add(new Word(w.Value, tag));
                    }
                }
                Sentences.Add(sentence);
            }
        }
        public Tuple<int, int> PercentCorrect(Corpra compareTo)
        {
            int totalWords = Sentences.SelectMany(i => i).Count();
            int correctCount = Sentences.SelectMany(i => i)
                .Zip(compareTo.Sentences.SelectMany(i => i),
                    (i, j) => i.Tag == j.Tag)
                .Where(i => i).Count();
            return Tuple.Create(correctCount, totalWords);
        }

        public static Tags ParseTag(string tag)
        {
            tag = tag.ToUpper().Replace(' ', '+').Replace("+*", "*");
            if (tag.StartsWith("FW-"))
                return Tags.OTHER;
            switch (tag)
            {
                case "(":
                case ")":
                case ",":
                case ".":
                case "--":
                case ":":
                case "PCT":
                    return Tags.PUNCTUATION;
                //case "*":
                case "NEG":
                    return Tags.NEGATOR;
                case "ABL":
                case "ABN":
                case "ABX":
                    return Tags.PREQUANTIFIER;
                case "AP":
                case "APG":
                case "AP+AP":
                    return Tags.POSTDETERMINER;
                case "AT":
                    return Tags.ARTICLE;
                case "BE":
                case "BED":
                case "BED*":
                case "BEDZ":
                case "BEDZ*":
                case "BEG":
                case "BEM":
                case "BEM*":
                case "BEN":
                case "BER":
                case "BER*":
                case "WDT+BER+PPS":
                case "BEZ":
                case "BEZ*":
                case "WDT+BER":
                case "WDT+BER+PP":
                case "WDT+BEZ":
                    return Tags.TOBE;
                case "CC":
                case "CS":
                    return Tags.CONJUNCTION;
                case "CD":
                //case "CD$":
                case "CDG":
                case "OD":
                    return Tags.NUMERAL;
                case "DO":
                case "DO*":
                case "DO+PPSS":
                case "DOD":
                case "DOD*":
                case "DOZ":
                case "DOZ*":
                case "WDT+DO+PPS":
                case "WDT+DOD":
                    return Tags.TODO;
                case "DT":
                //case "DT$":
                case "DTG":
                case "DT+BEZ":
                case "DT+MD":
                case "DTI":
                case "DTS":
                case "DTS+BEZ":
                case "DTX":
                //case "PP$":
                case "PPG":
                    return Tags.DETERMINER;
                case "EX":
                case "EX+BEZ":
                case "EX+HVD":
                case "EX+HVZ":
                case "EX+MD":
                case "WDT":
                    return Tags.THERE;
                case "HV":
                case "HV*":
                case "HV+TO":
                case "HVD":
                case "HVD*":
                case "HVG":
                case "HVN":
                case "HVZ":
                case "HVZ*":
                case "WDT+HVZ":
                    return Tags.TOHAVE;
                case "IN":
                case "IN+IN":
                case "IN+PPO":
                case "IN+NN":
                case "IN+AT":
                case "IN+NP":
                    return Tags.PREPOSITION;
                case "JJ":
                //case "JJ$":
                case "JJG":
                case "JJ+JJ":
                case "JJR":
                case "JJR+CS":
                case "JJS":
                case "JJT":
                    return Tags.ADJECTIVE;
                case "MD":
                case "MD*":
                case "MD+HV":
                case "MD+PPSS":
                case "MD+TO":
                    return Tags.AUXILIARY_VERB;
                case "NN":
                //case "NN$":
                case "NNG":
                case "NN+BEZ":
                case "NN+HVD":
                case "NN+HVZ":
                case "NN+IN":
                case "NN+MD":
                case "NN+NN":
                case "NNS":
                //case "NNS$":
                case "NNSG":
                case "NNS+MD":
                case "AT+NN":
                    return Tags.NOUN;
                case "NP":
                //case "NP$":
                case "NPG":
                case "NP+BEZ":
                case "NP+HVZ":
                case "NP+MD":
                case "NPS":
                //case "NPS$":
                case "NPSG":
                case "AT+NP":
                    return Tags.NOUN_PROPER;
                case "NR":
                //case "NR$":
                case "NRG":
                case "NR+MD":
                case "NRS":
                    return Tags.NOUN_ADVERBIAL;
                case "PN":
                //case "PN$":
                case "PNG":
                case "PN+BEZ":
                case "PN+HVD":
                case "PN+HVZ":
                case "PN+MD":
                //case "PP$$":
                case "PPGG":
                case "PPL":
                case "PPL+VBZ":
                case "PPLS":
                case "PPO":
                case "PPO+IN":
                case "PPS":
                case "PPS+BEZ":
                case "PPS+HVD":
                case "PPS+HVZ":
                case "PPS+MD":
                case "PPSS":
                case "PPSS+BEM":
                case "PPSS+BER":
                case "PPSS+BEZ":
                case "PPSS+BEZ*":
                case "PPSS+HV":
                case "PPSS+HVD":
                case "PPSS+MD":
                case "PPSS+VB":
                //case "WP$":
                case "WPG":
                case "WPO":
                case "WPS":
                case "WPS+BEZ":
                case "WPS+HVD":
                case "WPS+HVZ":
                case "WPS+MD":
                    return Tags.PRONOUN;
                case "QL":
                case "QLP":
                case "WQL":
                    return Tags.QUALIFIER;
                case "RB":
                //case "RB$":
                case "RBG":
                case "RB+BEZ":
                case "RB+CS":
                case "RB+CC":
                case "RBR":
                case "RBR+CS":
                case "RBT":
                case "RN":
                case "RP":
                case "RP+IN":
                case "TO":
                case "WRB":
                case "WRB+BER":
                case "WRB+BEZ":
                case "WRB+DO":
                case "WRB+DOD":
                case "WRB+DOD*":
                case "WRB+DOZ":
                case "WRB+IN":
                case "WRB+MD":
                    return Tags.ADVERB;
                case "UH":
                case "NIL":
                    return Tags.OTHER;
                case "VB":
                case "VB+AT":
                case "VB+IN":
                case "VB+JJ":
                case "VB+PPO":
                case "VB+RP":
                case "VB+TO":
                case "TO+VB":
                case "VB+VB":
                case "VBD":
                case "VBG":
                case "VBG+TO":
                case "VBN":
                case "VBN+TO":
                case "VBZ":
                    return Tags.VERB;
                default:
                    throw new System.NotImplementedException(string.Format("Shit missed '{0}' tag!", tag));
            }
        }
    }
}

public enum Tags
{
    PUNCTUATION, //punctuation marks EXP  . , ; !
    NEGATOR, //negator EXP not n't
    PREQUANTIFIER, //determiner/pronoun, pre-qualifier EXP quite such rather
    POSTDETERMINER, //determiner/pronoun, post-determiner EXP many other next more last former little several 
    ARTICLE, //article the, a, some, most, every, no, which
    TOBE, //verb "to be"
    CONJUNCTION, //conjunction, coordinating EXP and or but plus & either neither nor yet 'n' and/or minus an'
    NUMERAL,  //EXP two one 1 four 2 1913 71
    TODO, //verb "to do" EXP do dost
    DETERMINER, //EXP quite such rather
    THERE, //existential there
    TOHAVE, //verb "to have" EXP have hast
    PREPOSITION, // EXP of in for by considering to on among at through with under into
    ADJECTIVE, // EXP recent over-all possible hard-fought favorable hard meager fit such widespread
    AUXILIARY_VERB, //Helping verbs or auxiliary verbs EXP may might will would must can could shall ought need wilt
    NOUN,
    NOUN_PROPER,
    NOUN_ADVERBIAL, //EXP Sundays Mondays Saturdays Wednesdays Souths Fridays
    PRONOUN, //EXP none something everything one anyone nothing 
    QUALIFIER,  //EXP remarkably somewhat more completely too thus ill deeply little overly halfway almost
    ADVERB, //EXP only often generally also nevertheless upon
    VERB,
    OTHER
}
