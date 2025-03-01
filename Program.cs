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
            LoggingHelper.Log("Press 1 to generate dawn accounts or 2 for grass accounts or 3 to get dawn referral codes");
            var answer = Console.ReadLine();
            if (answer?.Contains("1") ?? false)
            {
                LoggingHelper.Log("How many accounts would you like to generate?");
                answer = Console.ReadLine();

                if (int.TryParse(answer, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                {
                    var dawnModule = new Dawn();
                    dawnModule.GenerateAccounts(count);
                }
                else
                {
                    LoggingHelper.Log("Invalid option", LoggingHelper.LogType.Error);
                }
            }
            else if (answer?.Contains("2") ?? false)
            {
                LoggingHelper.Log("How many accounts would you like to generate?");
                answer = Console.ReadLine();

                if (int.TryParse(answer, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                {
                    var grassModule = new Grass();
                    grassModule.GenerateAccounts(count);
                }
                else
                {
                    LoggingHelper.Log("Invalid option", LoggingHelper.LogType.Error);
                }
            }
            else if (answer?.Contains("3") ?? false)
            {
                var dawnModule = new Dawn();
                dawnModule.GetReferralCodes();
            }
            else
            {
                LoggingHelper.Log("Invalid option", LoggingHelper.LogType.Error);
            }

            LoggingHelper.Log("Press enter to exit", LoggingHelper.LogType.Error);
            Console.ReadLine();
        }
    }
}
