namespace Flaminco.Logging
{
    public interface ClassFeature
    {
        static sealed void M() => Console.WriteLine("Default behavior");

        static abstract void Show();
    }
    public class Class1 : ClassFeature
    {
        public static void Show()
        {
            throw new NotImplementedException();
        }
    }





}
