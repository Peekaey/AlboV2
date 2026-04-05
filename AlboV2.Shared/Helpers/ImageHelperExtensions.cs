namespace AlboV2.Shared.Helpers;

public static class ImageHelperExtensions
{
    public static string GetRngAlboFilename()
    {
        int number = GetRandomNumber();
        return  $"albo{number}.mov";
    }


    private static int GetRandomNumber()
    {
        Random rnd = new Random();
        return rnd.Next(1, 6);
    }
}