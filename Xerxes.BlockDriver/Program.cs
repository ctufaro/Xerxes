using System;
using Xerxes.Domain;

namespace Xerxes.BlockDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            BlockChain myChain = new BlockChain();
            myChain.AddBlock("Chris", "Second Post");
            myChain.AddBlock("Chris", "Third Post");
            myChain.AddBlock("Chris", "Fourth Post");
            myChain.PrintChain();

            var myChain2 = myChain.DownloadChain();

            myChain2.AddBlock("Chris", "Fifth Post");
            myChain2.PrintChain();
            Console.ReadLine();
        }
    }
}
