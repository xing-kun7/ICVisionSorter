using System;
using HalconDotNet;

class Test
{
    [STAThread]
    static void Main()
    {
        try
        {
            var t = typeof(HSmartWindowControlWPF);
            Console.WriteLine("Type: " + t.FullName);
            var obj = Activator.CreateInstance(t);
            Console.WriteLine("Created: " + obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Caught exception: " + ex.GetType().FullName);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner: " + ex.InnerException.GetType().FullName + " -> " + ex.InnerException.Message);
            }
        }
    }
}
