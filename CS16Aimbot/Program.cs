using Swed32;
using System.Numerics;
using ImGuiNET;
using ClickableTransparentOverlay;
namespace CS16Aimbot
{


	public class Program : Overlay
	{
		//listas e entidades
		static List<Entity> EntidadesDaPartida = new List<Entity>();
		public static Entity Player = new Entity();

		//codigo
		public static Swed swed = new Swed("cs");
		static IntPtr hw;
		
		//variaveis ImGUI
		public bool AimBotVariavel = false;
		public bool tpVariavel = false;
		public int milisecounds = 0;


		public static void Main(string[] args)
		{
			Program program = new Program();
			program.Start().Wait();
			Thread ThreadMainLogic = new Thread(program.MainLogic) { IsBackground = true };
			ThreadMainLogic.Start();
		}
		void MainLogic()
		{
			hw = swed.GetModuleBase("hw.dll");
			while (true)
			{
				PegarEntidades();
				AimBotAtivar();
				Teletransportar();
			}
		}
		void PegarEntidades()
		{
			// limpeza
			EntidadesDaPartida.Clear();
			Player.Address = swed.ReadPointer(hw, Offset.EntityList, Offset.MyPlayer);
			UpdateEntity(Player);
			UpdateEntities();
			MostrarNaTelaCMD();
		}
		void Teletransportar()
		{
			if (EntidadesDaPartida.Count > 0 && tpVariavel)
			{
				TeletransportarComSeguranca(swed, hw, Player, EntidadesDaPartida[0]);
			}
		}
		static void TeletransportarComSeguranca(Swed swed, IntPtr hw, Entity player, Entity enemy)
		{
			Vector3 posicaoInicial = player.Position;
			IntPtr positionEdit = swed.ReadPointer(hw, Offset.EntityList);
			Console.WriteLine("Salvando sua posição inicial: " + posicaoInicial);
			// Teletransporta atrás do inimigo
			bool teleportado = TeleportBehindEnemy(swed, hw, player, enemy);
			if (!teleportado)
			{
				Console.WriteLine("Falha ao teletransportar para o inimigo.");
				return;
			}
			// Aguarda um tempo para garantir que o teletransporte seja processado
			// Retorna para a posição inicial
			bool voltou = swed.WriteVec(positionEdit, Offset.PositionEdit, posicaoInicial);
			// Salva a posição inicial do jogador
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


		void AimBotAtivar()
		{
			if (EntidadesDaPartida.Count > 0 && AimBotVariavel)
			{
				Entity enemy = EntidadesDaPartida.First();
				if (enemy != null && enemy.Health > 1 && enemy.Health < 101)
				{
					AimAt(enemy);
				}
			}
		}
		void AimAt(Entity entity)
		{
			AimBot.AimBot2(swed, hw, entity, Player);
		}
		void MostrarNaTelaCMD()
		{
			Console.Clear();
			Console.WriteLine("Meu Player: ");
			Console.WriteLine(Player);
			Console.WriteLine("Inimigos mais pertos: ");
			foreach (Entity a in EntidadesDaPartida)
			{
				Console.WriteLine(a);
			}
		}
		public static void UpdateEntities()
		{
			for (int i = 1; i <= 32; i++)
			{
				IntPtr entityList = swed.ReadPointer(hw, Offset.EntityList);
				Entity entity = new Entity();
				entity.Address = swed.ReadPointer(entityList, Offset.MyPlayer + i * 0x324);
				if (entity.Address == IntPtr.Zero)
					continue;
				entity.ID = i;
				UpdateEntity(entity);
				if (entity.Health > 1 && entity.Health < 101 && entity.Team != Player.Team
						) // Adiciona apenas entidades válidas
					EntidadesDaPartida.Add(entity);
			}
			EntidadesDaPartida.Sort((a, b) => a.Distance.CompareTo(b.Distance));

		}
		public static void UpdateEntity(Entity entity)
		{
			entity.Health = swed.ReadInt(entity.Address, Offset.Life);
			if (entity.Health > 1 && entity.Health < 101)
			{
				entity.Team = swed.ReadInt(entity.Address, Offset.Team);
				entity.Position = swed.ReadVec(entity.Address, Offset.PositionNormal);
				entity.Distance = Vector3.Distance(Player.Position, entity.Position);
			}
		}
		protected override void Render()
		{
			DrawMenu();
			ImGui.End();
		}
		void DrawMenu()
		{
			ImGui.Begin("Cheat CS 1.6");
			if (ImGui.BeginTabBar("Tabs"))
			{
				if (ImGui.BeginTabItem("General"))
				{
					ImGui.Checkbox("AimBot", ref AimBotVariavel);
					ImGui.Checkbox("Teletransportar", ref tpVariavel);
					ImGui.InputInt("MiliSegundos: ", ref milisecounds);
				}
				ImGui.EndTabItem();
			}

			ImGui.EndTabBar();
		}
	}



}
