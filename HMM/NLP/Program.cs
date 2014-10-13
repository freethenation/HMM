using System;
using System.IO;

namespace NLP
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            foreach (var file in Directory.GetFiles("/home/freethenation/Downloads/brown_tei/", "*.xml"))
            {
                Corpra corpra = new Corpra();
                corpra.Load(file); 
            }

            int i = 1;
        }
    }
}
