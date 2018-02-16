namespace Suguya
{
    class Program
    {
        static void Main(string[] args) =>  new SuguyaCore().StartAsync().GetAwaiter().GetResult();
    }
}
