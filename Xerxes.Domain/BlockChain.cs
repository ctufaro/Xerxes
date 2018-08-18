using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xerxes.Domain
{

    public class BlockChain : IEnumerable<Block>
    {
        private List<Block> MasterChain { get; set; }

        private void Init()
        {
            MasterChain = new List<Block>();
            MasterChain.Add(GenerateGenesisBlock());
        }

        public BlockChain()
        {
            Init();
        }
        
        public BlockChain DownloadChain()
        {
            return this;
        }

        public void PrintChain()
        {
            foreach (Block b in MasterChain)
            {
                Console.WriteLine(b);
            }
        }

        private Block GenerateGenesisBlock()
        {
            return new Block(0, new DateTime(2013, 7, 22), "Chris", "Rejoice the Genesis Block!", "816534932c2b7154836da6afc367695e6337db8a921823784c14378abed4f7d7", ":)");
        }

        public bool AddBlock(string poster, string post)
        {
            Block previousBlock = GetLatestBlock();
            int nextIndex = previousBlock.Index + 1;
            DateTime nextTimeStamp = DateTime.Now;
            string nextHash = Block.HashBlock(nextIndex, previousBlock.Hash, nextTimeStamp, poster, post);
            Block newBlock = new Block(nextIndex, nextTimeStamp, poster, post, nextHash, previousBlock.Hash);
            bool isValidBlock = IsValidNewBlock(newBlock, GetLatestBlock());
            if(isValidBlock)
                MasterChain.Add(newBlock);
            return isValidBlock;
        }

        private string CalcHashBlock(Block block)
        {
            return Block.HashBlock(block.Index, block.PrevHash, block.TimeStamp, block.Poster, block.Post);
        }

        private Block GetLatestBlock()
        {
            return MasterChain[MasterChain.Count - 1];
        }

        private bool IsValidNewBlock(Block newBlock, Block prevBlock)
        {
            if (prevBlock.Index + 1 != newBlock.Index)
            {
                System.Console.WriteLine("Invalid Index");
                return false;
            }
            else if (!prevBlock.Hash.Equals(newBlock.PrevHash))
            {
                System.Console.WriteLine("Invalid Previous Hash");
                return false;
            }
            else if (!CalcHashBlock(newBlock).Equals(newBlock.Hash))
            {
                System.Console.WriteLine("Invalid Hash");
                return false;
            }
            return true;
        }

        private bool IsValidChain(BlockChain newChain)
        {
            Block genesis = GenerateGenesisBlock();
            if (!newChain.MasterChain[0].Equals(genesis))
            {
                return false;
            }
            if (newChain.MasterChain.Count > 1)
            {
                for (int i = 0; i < newChain.MasterChain.Count; i++)
                {
                    if (i == newChain.MasterChain.Count - 1)
                        break;

                    var prev = newChain.MasterChain[i];
                    var newb = newChain.MasterChain[i + 1];

                    if (!IsValidNewBlock(newb, prev))
                        return false;
                }
            }
            return true;
        }

        private void Replace(BlockChain newChain)
        {
            if (IsValidChain(newChain) && newChain.MasterChain.Count > this.MasterChain.Count)
            {
                this.MasterChain = newChain.MasterChain;
            }
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return MasterChain.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}