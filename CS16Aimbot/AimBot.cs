using Swed32;
using System.Numerics;

namespace CS16Aimbot
{



    public static class AimBot
    {
        public static void Aimbot(Swed swed, IntPtr hw, List<Entity> entities, Entity Player)
        {



            Console.Clear();
            IntPtr entityList = swed.ReadPointer(hw, Offset.EntityList);
            Player.Address = swed.ReadPointer(entityList, Offset.MyPlayer);
            Player.Team = swed.ReadInt(Player.Address, Offset.Team);
            Player.Position = swed.ReadVec(Player.Address, Offset.PositionNormal);
            entities.Clear();
            for (int ia = 1; ia <= 12; ia++)
            {


                var Address = swed.ReadPointer(entityList, Offset.MyPlayer + ia * Offset.step);
                if (Address == IntPtr.Zero) continue;

                var Health = swed.ReadInt(Address, Offset.Life);


                if (Health > 1 && Health <= 100)
                {

                    Entity entity = new Entity();
                    entity.Address = Address;
                    entity.ID = ia;
                    entity.Health = Health;
                    entity.Team = swed.ReadInt(entity.Address, Offset.Team);
                    entity.Position = swed.ReadVec(entity.Address, Offset.PositionNormal);
                    entity.Distance = Vector3.Distance(Player.Position, entity.Position);


                    // Adiciona a entidade à lista apenas se for de outro time
                    if (entity.Team != Player.Team)
                    {
                        entities.Add(entity);
                    }




                }



            }

            entities = entities.OrderBy(o => o.Distance).ToList();


            if (entities.Count > 0)
            {
                // Pega o inimigo mais próximo
                Entity closestEnemy = entities[0];

                // Calcula os ângulos necessários para mirar
                Vector3 aimAngles = CalculateAngles(Player.Position, closestEnemy.Position);

                var compview = swed.ReadPointer(hw, Offset.ViewAngles);
                // var view = swed.ReadVec(compview, 0xC4);
                // Console.WriteLine(view);

                // Escreve os ângulos na memória do jogo

                swed.WriteVec(compview, 0xC4, aimAngles);
                //Console.WriteLine("Arma: "+swed.ReadFloat(Player.Address, 0x34));
                //Console.WriteLine("Arma: "+swed.ReadInt(Player.Address, Offset.Life));

                Console.WriteLine($"Mirando em inimigo a {closestEnemy.Distance} unidades.");


                // List<Entity> entities2 = entities.OrderBy(o => o.ID).ToList();
                //foreach (var ent in entities2)
                //{

                //    Console.WriteLine(ent);


                //}


                Thread.Sleep(10); // 




            }
            static Vector3 CalculateAngles(Vector3 playerPos, Vector3 enemyPos)
            {
                Vector3 delta = enemyPos - playerPos;

                // Calcula a distância horizontal (plano X-Y)
                float distanceXY = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

                // Calcula o ângulo de Pitch (vertical)
                float pitch = -(float)(Math.Atan2(delta.Z, distanceXY) * (180 / Math.PI));

                // Calcula o ângulo de Yaw (horizontal)
                float yaw = (float)(Math.Atan2(delta.Y, delta.X) * (180 / Math.PI));

                // Normaliza o Yaw para estar entre 0 e 360 graus
                if (yaw < 0) yaw += 360;

                return new Vector3(pitch, yaw, 0);
            }



        }
        public static void AimBot2(Swed swed, IntPtr hw, Entity entity, Entity Player)
        {
            if (entity != null && Player.Health > 1)
            {

                // Pega o inimigo mais próximo
                // Calcula os ângulos necessários para mirar
                Vector3 aimAngles = CalculateAngles(Player.Position, entity.Position);
                var compview = swed.ReadPointer(hw, Offset.ViewAngles);
                // var view = swed.ReadVec(compview, 0xC4);
                // Console.WriteLine(view);
                // Escreve os ângulos na memória do jogo
                swed.WriteVec(compview, 0xC4, aimAngles);
                //Console.WriteLine("Arma: "+swed.ReadFloat(Player.Address, 0x34));
                //Console.WriteLine("Arma: "+swed.ReadInt(Player.Address, Offset.Life));
                //swed.WriteInt(Player.Address, Offset.Life);
                Console.WriteLine($"Mirando em inimigo a {entity.Distance} unidades.");
            }
            
                }

                // List<Entity> entities2 = entities.OrderBy(o => o.ID).ToList();
                //foreach (var ent in entities2)
                //{
                //    Console.WriteLine(ent);
                //}
            
            static Vector3 CalculateAngles(Vector3 playerPos, Vector3 enemyPos)
            {
                Vector3 delta = enemyPos - playerPos;

                // Calcula a distância horizontal (plano X-Y)
                float distanceXY = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

                // Calcula o ângulo de Pitch (vertical)
                float pitch = -(float)(Math.Atan2(delta.Z, distanceXY) * (180 / Math.PI));

                // Calcula o ângulo de Yaw (horizontal)
                float yaw = (float)(Math.Atan2(delta.Y, delta.X) * (180 / Math.PI));

                // Normaliza o Yaw para estar entre 0 e 360 graus
                if (yaw < 0) yaw += 360;

                return new Vector3(pitch, yaw, 0);
            }

        
    }
}