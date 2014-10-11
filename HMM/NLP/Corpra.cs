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
            switch (tag)
            {
                case "":
                    return Tags.ADJ;
                default:
                    throw new System.NotImplementedException(string.Format("Shit missed '{0}' tag!", tag));
                    return Tags.OTHER;
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
        ADJ, //adjective   new, good, high, special, big, local
        ADP, //adposition  on, of, at, with, by, into, under
        ADV, //adverb  really, already, still, early, now
        CONJ, //conjunction and, or, but, if, while, although
        DET, //determiner, article the, a, some, most, every, no, which
        NOUN, //noun year, home, costs, time, Africa
        NUM, //numeral twenty-four, fourth, 1991, 14:24
        PRT, //particle    at, on, out, over per, that, up, with
        PRON, //pronoun he, their, her, its, my, I, us
        VERB, //verb    is, say, told, given, playing, would
        PUNCT, //punctuation marks   . , ; !
        OTHER, //other   ersatz, esprit, dunno, gr8, univeristy
    }
