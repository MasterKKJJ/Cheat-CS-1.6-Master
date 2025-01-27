using Swed32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS16Aimbot
{
    public class Teletransport
    {
        public static void Teletransportar(Swed swed,IntPtr hw, Vector3 EnemyPosition)
        {
            IntPtr posicao = swed.ReadPointer(hw, Offset.EntityList);
            Console.WriteLine(posicao);
            swed.WriteVec(posicao, EnemyPosition);
        }

    }
}
