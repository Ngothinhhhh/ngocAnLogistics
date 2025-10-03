using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication3.Helpers
{
    public static class DictionaryHelper
    {
        public static void userDataPrintDictionary(IDictionary<string, string> data)
        {
            if (data == null)
            {
                Console.WriteLine("Dictionary is null");
                return;
            }

            foreach (var kvp in data)
            {
                Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
            }
        }


        

    }

}
