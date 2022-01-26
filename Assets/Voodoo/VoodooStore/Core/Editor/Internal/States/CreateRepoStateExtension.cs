namespace Voodoo.Store
{
    public static class CreateRepoStateExtension
    {
        public static string ToFriendlyString(this CreateRepoState state)
        {
            switch (state)
            {
                case CreateRepoState.CREATE_REPOSITORY:
                    return "Start creating your github repository";
                case CreateRepoState.GRANT_ACCESS:
                    return "Granting access to the appropriate teams";
                case CreateRepoState.CHANGE_DEFAULT_BRANCH:
                    return "Changing default branch from master to main";
                case CreateRepoState.CREATE_STOPPED:
                    return "Stop creating repository due to an error";
                case CreateRepoState.CREATE_SUCCESSFULL:
                    return "Your repository has successfully been created";
                default:
                    return "I don't know what the script is doing";
            }
        }
    }
    
    public enum CreateRepoState
    {
        CREATE_REPOSITORY     = 0,
        GRANT_ACCESS          = 1,
        CHANGE_DEFAULT_BRANCH = 2,
        CREATE_STOPPED        = 3,
        CREATE_SUCCESSFULL    = 4,
    }
}