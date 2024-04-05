using System.Reflection;
using System.Text;
using BepInEx;
using CustomRegions.Collectables;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace ModTextDecryption;

[BepInPlugin("ModTextDecryption", "Mod Text Decryption", "2024.4.5")]
public class ModTextDecryption : BaseUnityPlugin
{
	private const string exc = "加载时发生了错误！请尝试上报给作者";

	public void OnEnable()
	{
		Logger.LogMessage("正在加载…");
		try
		{
			Modify(
				typeof(Encryption).GetMethod(nameof(Encryption.DecryptCustomText), BindingFlags.Public | BindingFlags.Static) ??
				throw new MissingMethodException(nameof(Encryption.DecryptCustomText)), il =>
				{
					var c = new ILCursor(il);
					c.EmitDelegate<Action>(() => Logger.LogMessage("Method Called"));
					c.GotoNext(i => i.MatchRet());

					c.Emit(OpCodes.Ldarg_0);
					c.EmitDelegate<Action<string, string>>((text, path) => File.WriteAllText(path + ".decrypted", text, Encoding.UTF8));
					c.Emit(OpCodes.Ldloc_2);
				}
			);
			Logger.LogMessage("加载完成！");
		}
		catch (Exception e)
		{
			Logger.LogFatal(exc);
			Logger.LogFatal(e);
		}
	}

	private static void Modify(MethodBase from, ILContext.Manipulator manipulator)
	{
		new ILHook(from, manipulator).Apply();
	}
}