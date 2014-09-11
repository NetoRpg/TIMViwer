using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAT_Unpacker
{
    class Indexes
    {
        public int offset;
        public List<int> subOffsets = new List<int>();

        public Indexes(int offset)
        {
            this.offset = offset;
        }

    }
}
