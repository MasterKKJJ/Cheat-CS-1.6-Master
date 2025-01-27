using Swed32;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading;

namespace CS16Aimbot
{
    class Program
    {
        static volatile List<Entity> entities = new List<Entity>(); // Lista compartilhada, sempre atualizada

        static void Main(string[] args)
        {
            Swed swed = new Swed("cs");
            IntPtr hw = swed.GetModuleBase("hw.dll");

            Entity Player = new Entity();


            // Thread para atualizar a lista de entidades
            entities.Clear();
            Thread atualizarEntidadesThread = new Thread(() =>
            {
                AtualizarEntidades(swed, hw, Player);
            });
            atualizarEntidadesThread.IsBackground = true;
            atualizarEntidadesThread.Start();

            // thread para o aimbot

            Thread aimbotthread = new Thread(() =>
            {
                while (true)
                {
                    List<Entity> currententities = entities;
              
                    

                    
                    if (currententities != null && currententities.Any())
                    {
                        
                        AimBot.AimBot2(swed, hw, currententities.First(), Player);
                    }
                    else
                    {
                        Console.WriteLine("Nenhum inimigo encontrado.");
                
                    }



                    Thread.Sleep(10); // evita uso excessivo da cpu
                }
            });
            aimbotthread.IsBackground
                = true;
            aimbotthread.Start();

            //// Thread para teletransporte
            Thread teleporteThread = new Thread(() =>
           {
               while (true)
               {
                   List<Entity> currentEntities = entities; // Pega a lista mais recente
                   if (currentEntities != null && currentEntities.Any())
                   {
                       TeletransportarComSeguranca
                       (swed, hw, Player, currentEntities.First());
                       Thread.Sleep(100);
                   }
                   else
                   {
                       Console.WriteLine("Nenhum inimigo encontrado.");
                       Thread.Sleep(15000);

                   }

                   Thread.Sleep(1000); // Evita uso excessivo da CPU
               }
           });
            teleporteThread.IsBackground = true;
            teleporteThread.Start();

            // Aguarda o programa principal (mantém ativo)

            //swed.WriteInt(Player.Address, Offset.LifeEdit, 200);

            Console.ReadLine();
        }

        /// <summary>
        /// Atualiza continuamente a lista de entidades
        /// </summary>
        static void AtualizarEntidades(Swed swed, IntPtr hw, Entity Player)
        {
            while (true)
            {
                List<Entity> updatedEntities = new List<Entity>();
                updatedEntities.Clear();
                IntPtr entityList = swed.ReadPointer(hw, Offset.EntityList);
                Player.Address = swed.ReadPointer(entityList, Offset.MyPlayer);
                Player.Team = swed.ReadInt(Player.Address, Offset.Team);
                Player.Position = swed.ReadVec(Player.Address, Offset.PositionNormal);
                Player.Health = swed.ReadInt(Player.Address, Offset.Life);
                


                for (int ia = 1; ia <= 12; ia++)
                {
                    IntPtr address = swed.ReadPointer(entityList, Offset.MyPlayer + ia * Offset.step);
                    if (address == IntPtr.Zero) continue;

                    int health = swed.ReadInt(address, Offset.Life);
                    if (health > 1 && health <= 100)
                    {
                        Entity entity = new Entity
                        {
                            ID = ia,
                            Address = address,
                            Health = health,
                            Team = swed.ReadInt(address, Offset.Team),
                            Position = swed.ReadVec(address, Offset.PositionNormal),
                            Distance = Vector3.Distance(Player.Position, swed.ReadVec(address, Offset.PositionNormal))
                        };

                        if (entity.Team != Player.Team)
                        {
                            updatedEntities.Add(entity);
                        }
                    }
                }

                updatedEntities.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                // Atualiza a lista de forma atômica

                //Console.Clear();
                //Console.WriteLine("Meu Jogador: ");
                //Console.WriteLine(Player    );
                //Console.WriteLine("Inimigos: ");
                //foreach (Entity entity in updatedEntities) {
                //    Console.WriteLine(entity);
                //}
                entities = updatedEntities;

                Console.Clear();
                Console.WriteLine("Meu Player: ");
                Console.WriteLine(Player);
                Console.WriteLine("Inimgos: ");
                foreach (Entity entity in updatedEntities) {
                    Console.WriteLine(entity);


                }
                Thread.Sleep(10); // Delay para evitar uso excessivo da CPU
            }

        }
        static void TeletransportarComSeguranca(Swed swed, IntPtr hw, Entity player, Entity enemy)
        {
            // Salva a posição inicial do jogador
            Vector3 posicaoInicial = player.Position;
            IntPtr positionEdit = swed.ReadPointer(hw, Offset.EntityList);
            Console.WriteLine("Salvando sua posição inicial: " + posicaoInicial);

            // Teletransporta atrás do inimigo
            bool teleportado = TeleportBehindEnemy(swed, hw, player, enemy);
            Thread.Sleep(1000);
            if (!teleportado)
            {
                Console.WriteLine("Falha ao teletransportar para o inimigo.");
                return;
            }

            // Aguarda um tempo para garantir que o teletransporte seja processado
            Thread.Sleep(100);

            // Retorna para a posição inicial
            bool voltou = swed.WriteVec(positionEdit, Offset.PositionEdit, posicaoInicial);
            

            if (voltou)
            {
                Console.WriteLine("Você voltou para a posição inicial com segurança.");
            }
            else
            {
                Console.WriteLine("Falha ao retornar à posição inicial.");
            }
        }

        static bool TeleportBehindEnemy(Swed swed, IntPtr hw, Entity player, Entity enemy)
        {
            try
            {
                // Calcula a posição atrás do inimigo
                Vector3 direction = Vector3.Normalize(enemy.Position - player.Position);
                Vector3 newPosition = enemy.Position - direction;
                IntPtr positionEdit = swed.ReadPointer(hw, Offset.EntityList);

                // Escreve a nova posição na memória do jogo
                bool sucesso = swed.WriteVec(positionEdit, Offset.PositionEdit, newPosition);

                if (sucesso)
                {
                    Console.WriteLine("Teletransportado para trás do inimigo com sucesso!");
                    return true;
                }
                else
                {
                    Console.WriteLine("Falha ao teletransportar para trás do inimigo.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro durante o teletransporte: " + ex.Message);
                return false;
            }
        }




    }


}
