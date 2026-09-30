public class SampleMessage()
{
    private string message =
    """
    import twccommon
    twccommon.Log.info("THIS IS WINDSOCK SAYING HELLO :)")
    """;

    public List<string> Serialize()
    {
        return new List<string>() {message};
    }
}