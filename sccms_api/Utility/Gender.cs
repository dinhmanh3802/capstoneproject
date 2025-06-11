using System.ComponentModel.DataAnnotations;

namespace Utility
{
	public enum Gender
	{
		[Display(Name = "Nam")]
		Male,

		[Display(Name = "Nữ")]
		Female,
	}
}
