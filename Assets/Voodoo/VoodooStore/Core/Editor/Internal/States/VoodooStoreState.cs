using System;

namespace Voodoo.Store
{
	public static class VoodooStoreState
	{
		public static State previous { get; private set; }

		private static State current = State.AUTHENTICATION;

		public static State Current
		{
			get => current;
			set
			{
				if (current == value)
				{
					return;
				}

				previous = current;

				current = value;
				OnStateChanged?.Invoke(current);
			}
		}

		public static event Action<State> OnStateChanged;
		public static event Action AskForRepaint;

		public static void RepaintWindow()
		{
			AskForRepaint?.Invoke();
		}
	}
	
    public enum State
    {
        AUTHENTICATION,
        FETCHING,
        STORE,
        EXPORTER,
        OPTIONS
        // DOWNLOAD
    }
}