using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS16Aimbot
{
    public class Entity
    {
        public IntPtr Address { get; set; }
        public Vector3 Position { get; set; }
        
        public int Team { get; set; }
        public int Health { get; set; }
        public int ID { get; set; }
        public float Distance { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("Id: "+ID);
            stringBuilder.Append(" Endereço: "+Address);
            stringBuilder.Append(" Posicao: "+Position);
            stringBuilder.Append(" Team: " + Team);
            stringBuilder.Append(" Health: " + Health); 
            stringBuilder.Append(" Distance: " + Distance);
            return stringBuilder.ToString();

        }
    }
}
