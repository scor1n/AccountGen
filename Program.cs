using AccountGen.Modules.Dawn;
using AccountGen.Modules.Grass;
using AccountGen.Utils;
using System.Globalization;

namespace AccountGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SettingsHelper.InitSettings();
            Console.WriteLine("Press 1 to generate dawn accounts or 2 for grass accounts");
            var answer = Console.ReadLine();
            if (answer?.Contains("1") ?? false)
            {
                Console.WriteLine("How many accounts would you like to generate?");
                answer = Console.ReadLine();

                if (int.TryParse(answer, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                {
                    var dawnModule = new Dawn();
                    dawnModule.GenerateAccounts(count);
                }
                else
                {
                    Console.WriteLine("Invalid option");
                }
            }
            else if (answer?.Contains("2") ?? false)
            {
                Console.WriteLine("How many accounts would you like to generate?");
                answer = Console.ReadLine();

                if (int.TryParse(answer, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                {
                    var grassModule = new Grass();
                    grassModule.GenerateAccounts(count);
                }
                else
                {
                    Console.WriteLine("Invalid option");
                }
            }
            else
            {
                Console.WriteLine("Invalid option");
            }
        }
    }
}
