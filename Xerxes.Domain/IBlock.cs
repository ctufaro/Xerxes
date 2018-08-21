using System;
using System.Collections.Generic;
using System.Text;

namespace Xerxes.Domain
{
    public interface IBlock
    {
        int Index { get; set; }
        DateTime TimeStamp { get; set; }
        string Poster { get; set; }
        string Post { get; set; }
        string Hash { get; set; }
        string PrevHash { get; set; }
        string Guid { get; set; }
    }
}
