namespace AlboV2.Shared.Helpers;

public static class ImageHelpers
{
    public static string GetRngAlboFilename()
    {
        int number = GetRandomNumber();
        return  $"albo{number}.mov";
    }


    private static int GetRandomNumber()
    {
        return Random.Shared.Next(1, 7);
    }
}