using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace Xerxes.Domain
{

    [ZeroFormattable]
    public class Block : IBlock, IEquatable<Block>
    {
        [Index(0)]
        public virtual int Index { get; set; }
        [Index(1)]
        public virtual DateTime TimeStamp { get; set; }
        [Index(2)]
        public virtual string Poster { get; set; }
        [Index(3)]
        public virtual string Post { get; set; }
        [Index(4)]
        public virtual string Hash { get; set; }
        [Index(5)]
        public virtual string PrevHash { get; set; }
        [Index(6)]
        public virtual string Guid { get; set; }

        public Block()
        {

        }

        public Block(string Guid, string Poster, string Post)
        {
            this.Guid = Guid;
            this.Poster = Poster;
            this.Post = Post;
        }

        public Block(int Index, DateTime TimeStamp, string Poster, string Post, string Hash, string PrevHash)
        {
            this.Index = Index;
            this.TimeStamp = TimeStamp;
            this.Poster = Poster;
            this.Post = Post;
            this.Hash = Hash;
            this.PrevHash = PrevHash;
        }

        public static string HashBlock(int Index, string PreviousHash, DateTime TimeStamp, string Poster, string Post)
        {
            string Value = string.Format("{0}{1}{2}{3}{4}", Index, PreviousHash, TimeStamp, Poster, Post);
            return RawHash(Value);
        }

        public static string RawHash(string Value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(Value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public bool Equals(Block other)
        {
            return this.Index.Equals(other.Index) && this.TimeStamp.Equals(other.TimeStamp) 
                                                  && this.Poster.Equals(other.Poster) 
                                                  && this.Post.Equals(other.Post) 
                                                  && this.Hash.Equals(other.Hash) 
                                                  && this.PrevHash.Equals(other.PrevHash);
        }

        public override string ToString()
        {
            //return string.Format("{0} {1} {2} {3} {4} {5}", Index, PrevHash, TimeStamp, Poster, Post, (Guid!=null)?Guid:"");
            return string.Format("{0} {1} {2} [{3}: {4}]", Index, PrevHash, TimeStamp, Poster, Post);
        }
    }
}