
public class HelpCommand : Command
{
    public HelpCommand()
    {
    }

    public override string GetCommandName()
    {
        return "help";
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override Task<object> ExecuteAsync(bool interactive)
    {
        throw new NotImplementedException();
    }
}