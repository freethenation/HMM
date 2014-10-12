using System;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NLP
{
    public class Corpra
    {
        public IList<Word> Sentences;
        public Corpra()
        {

        }
        public void Load(string path)
        {
            XDocument doc = XDocument.Load(path);
            foreach (var s in doc.Descendants("s")) {
                List<Word> sentence = new List<Word>();
                foreach (var w in s.Descendants()) {
                    //sentence.Add(new Word(w.Attribute("type"), 
                }
            }
        }
        public Tags ParseTag(string tag)
        {
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
                    return Tags.PUNCTUATION;
                case "*":
                    return Tags.NEGATOR;
                case "ABL":
                case "ABN":
                case "ABX":
                    return Tags.PREQUANTIFIER;
                case "AP":
                case "AP$":
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
                case "CD$":
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
                case "DT$":
                case "DT+BEZ":
                case "DT+MD":
                case "DTI":
                case "DTS":
                case "DTS+BEZ":
                case "DTX":
                case "PP$":
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
                    return Tags.PREPOSITION;
                case "JJ":
                case "JJ$":
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
                case "NN$":
                case "NN+BEZ":
                case "NN+HVD":
                case "NN+HVZ":
                case "NN+IN":
                case "NN+MD":
                case "NN+NN":
                case "NNS":
                case "NNS$":
                case "NNS+MD":
                    return Tags.NOUN;
                case "NP":
                case "NP$":
                case "NP+BEZ":
                case "NP+HVZ":
                case "NP+MD":
                case "NPS":
                case "NPS$":
                    return Tags.NOUN_PROPER;
                case "NR":
                case "NR$":
                case "NR+MD":
                case "NRS":
                    return Tags.NOUN_ADVERBIAL;
                case "PN":
                case "PN$":
                case "PN+BEZ":
                case "PN+HVD":
                case "PN+HVZ":
                case "PN+MD":
                case "PP$$":
                case "PPL":
                case "PPLS":
                case "PPO":
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
                case "WP$":
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
                case "RB$":
                case "RB+BEZ":
                case "RB+CS":
                case "RBR":
                case "RPR+CS":
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

public interface ISentence : IEnumerable<Word> {} 

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
