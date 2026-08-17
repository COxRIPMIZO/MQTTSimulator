using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTPublisher.Helper
{
    public class GetRandomData
    {
        public int GetRandomInteger(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
