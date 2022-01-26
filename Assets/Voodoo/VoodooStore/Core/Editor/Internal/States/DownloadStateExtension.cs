namespace Voodoo.Store
{
	public static class DownloadStateExtension
	{
		public static string ToFriendlyString(this DownloadState state)
		{
			switch (state)
			{
				case DownloadState.START_DOWNLOADING:
					return "Start downloading your package";
				case DownloadState.CREATE_ARCHIVE:
					return "Creating zip file";
				case DownloadState.EXTRACT_ARCHIVE:
					return "Extracting files from the zip";
				case DownloadState.RENAME_ARCHIVE:
					return "Renaming the parent folder";
				case DownloadState.DOWNLOAD_STOPPED:
					return "Your package has not been downloaded due to an error.";
				case DownloadState.DOWNLOAD_FINISHED:
					return "Finished downloading your package";
				default:
					return "I don't know what the script is doing";
			}
		}
	}
    
	public enum DownloadState
	{
		START_DOWNLOADING = 0,
		CREATE_ARCHIVE    = 1,
		EXTRACT_ARCHIVE   = 2,
		RENAME_ARCHIVE    = 3,
		DOWNLOAD_STOPPED  = 4,
		DOWNLOAD_FINISHED = 5
	}
}