using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Xerxes.Domain
{

    public class Block : IEquatable<Block>
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Poster { get; set; }
        public string Post { get; set; }
        public string Hash { get; set; }
        public string PrevHash { get; set; }

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
            return string.Format("{0} {1} {2} {3} {4}", Index, PrevHash, TimeStamp, Poster, Post);
        }
    }
}