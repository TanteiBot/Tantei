namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	public readonly struct Reprogressing
	{
		public readonly bool IsReprogressing;

		public Reprogressing(string value)
		{
			if (value == "" || value == "0")
				this.IsReprogressing = false;
			else
				this.IsReprogressing = true;
		}

		public Reprogressing(int value) => this.IsReprogressing = value == 1;
		
		public Reprogressing(byte value) => this.IsReprogressing = value == 1;

		public Reprogressing(bool value) => this.IsReprogressing = value;

		/// <inheritdoc />
		public override string ToString() => this.IsReprogressing.ToString();
	}
}