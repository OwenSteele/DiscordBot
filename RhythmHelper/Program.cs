namespace RhythmHelper
{
    public class Program
    {
        public static void Main()
            => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}
