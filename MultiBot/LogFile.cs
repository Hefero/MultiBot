using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EnvControllers
{
    public class LogFile
    {
        public LogFile(string Location)
        {
            fileLocation = Location;
            AllLines = ReadAllLines();
        }

        private List<string> AllLines { get; set; }

        private string fileLocation { get; set; }

        public List<string> NewLines { get { return GetNewLines(); } set { } }

        private List<string> ReadAllLines()
        {
            try
            {
                int numberOfLines = 50;
                var queue = new Queue<string>(numberOfLines);

                using (FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))  // May not make much difference.
                using (StreamReader sr = new StreamReader(bs))
                {
                    while (!sr.EndOfStream)
                    {
                        if (queue.Count == numberOfLines)
                        {
                            queue.Dequeue();
                        }

                        queue.Enqueue(sr.ReadLine());
                    }
                }
                List<string> output = new List<string>();
                do
                {
                    output.Add(queue.Dequeue());
                } while (queue.Count > 0);

                return output;
            }
            catch { return null; }
        }

        private List<string> GetNewLines()
        {
            try
            {
                int numberOfLines = 50;
                var queue = new Queue<string>(numberOfLines);

                using (FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))  // May not make much difference.
                using (StreamReader sr = new StreamReader(bs))
                {
                    while (!sr.EndOfStream)
                    {
                        if (queue.Count == numberOfLines)
                        {
                            queue.Dequeue();
                        }

                        queue.Enqueue(sr.ReadLine());
                    }
                }
                List<string> output = new List<string>();
                do
                {
                    output.Add(queue.Dequeue());
                } while (queue.Count > 0);

                List<string> toReturn = output.Except(AllLines).ToList();
                AllLines = output;
                return toReturn;
            }
            catch { return null; }
        }
        public static bool LookForString(List<string> list, string myString)
        {
            try
            {
                if (list.FirstOrDefault(stringToCheck => stringToCheck.ToLower().Contains(myString.ToLower())) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }
    }
}