using System;
using System.Collections.Generic;
using System.Linq;
using HMM;

namespace NLP
{
    public class DictionaryEntry
    {
        public DictionaryEntry(string word)
        {
            Word = word;
        }
        public readonly string Word;
        private readonly Dictionary<Tags, int> _counts = new Dictionary<Tags, int>();
        private Dictionary<Tags, double> _normalizedCounts = null;
        public void UpdateCount(Word word)
        {
            if (word.Name.ToLower() != Word) throw new ArgumentException();
            int count;
            if (!_counts.TryGetValue(word.Tag, out count)) count = 0;
            _counts[word.Tag] = count + 1;
            _normalizedCounts = null;
        }
        public Dictionary<Tags, int> TagCounts
        {
            get { return _counts; }
        }
        public Dictionary<Tags, double> NormalizedTagCounts
        {
            get
            {
                if (_normalizedCounts != null)
                    return _normalizedCounts;
                int total = _counts.Sum(i => i.Value);
                _normalizedCounts = _counts.Select(i => new KeyValuePair<Tags, double>(i.Key, i.Value / (double)total))
                    .ToDictionary();
                return _normalizedCounts;
            }
        }
        public Tags MostCommonTag
        {
            get { return _counts.Largest(i => i.Value).Key; }
        }

    }
    public class Dictionary
    {
        private Dictionary<string, DictionaryEntry> dict = new Dictionary<string, DictionaryEntry>();
        public Dictionary()
        {
        }
        public DictionaryEntry Lookup(string word)
        {
            DictionaryEntry ret;
            dict.TryGetValue(word, out ret);
            return ret;
        }
        public void UpdateCount(IEnumerable<Word> words)
        {
            foreach (var word in words) UpdateCount(word);
        }
        public void UpdateCount(Word word)
        {
            DictionaryEntry entry;
            if (!dict.TryGetValue(word.Name.ToLower(), out entry))
            {
                entry = new DictionaryEntry(word.Name.ToLower());
                dict[entry.Word] = entry;
            }
            entry.UpdateCount(word);
        }
    }
}

