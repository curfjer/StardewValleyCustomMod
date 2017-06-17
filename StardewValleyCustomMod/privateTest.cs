using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFarmBuildings
{
    public class PrivateTest
    {
        private int a = 1;

        public PrivateTest()
        {
            this.a = 10;
        }

        public int GetA()
        {
            return this.a;
        }
    }
}
