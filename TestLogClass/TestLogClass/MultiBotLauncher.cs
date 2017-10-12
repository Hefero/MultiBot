using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultibotPrograms
{
    class MultiBotLauncher
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client or Server?"); // Prompt
            Console.WriteLine("1 - Server , 2 - Client"); // Prompt
            int option = Convert.ToInt32(Console.ReadLine());
            if (option == 1)
            {
                ServerProgram serverProgram = new ServerProgram();
            }
            else
            {
                ClientProgram clientProgram = new ClientProgram();
            }
        }
    }
}
