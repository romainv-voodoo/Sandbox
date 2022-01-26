namespace Voodoo.Store
{
	public static class PushStateExtension
	{
		public static string ToFriendlyString(this PushState state)
		{
			switch (state)
			{
				case PushState.ADDING_FILES:
					return "Adding the files that needs to be pushed";
				case PushState.START_PUSHING:
					return "Start pushing your package to it's github repository";
				case PushState.CREATING_TREE:
					return "Creating the tree that have all the files of your package";
				case PushState.CREATING_COMMIT:
					return "Creating the commit with the message you entered";
				case PushState.COMPARING_RESULT:
					return "Comparing your commit with the previous one";
				case PushState.PUSHING:
					return "Pushing your commit to the github repository";
				case PushState.PUSH_STOPPED:
					return "Your package has not been exported since it was identical to the previous one";
				case PushState.PUSH_SUCCESSFULL:
					return "Your package has successfully been exported";
				default:
					return "I don't know what the script is doing";
			}
		}
	}
		
	public enum PushState
	{
		ADDING_FILES     = 0,
		START_PUSHING    = 1,
		CREATING_TREE    = 2,
		CREATING_COMMIT  = 3,
		COMPARING_RESULT = 4,
		PUSHING          = 5,
		PUSH_STOPPED     = 6,
		PUSH_SUCCESSFULL = 7,
	}
}