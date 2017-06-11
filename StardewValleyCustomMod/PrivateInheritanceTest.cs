using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewValleyCustomMod
{
    public class PrivateInheritanceTest : PrivateTest
    {
        private int a = 2;

        public PrivateInheritanceTest()
        {
            this.a = 20;
        }
    }
}
