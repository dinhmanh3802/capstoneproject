// File: SCCMS.Domain.DTOs.EmailDtos/VolunteerApplicationResultRequest.cs

using System;

namespace SCCMS.Domain.DTOs.EmailDtos
{
	public class VolunteerApplicationResultRequest
	{
		public int[] ListVolunteerApplicationId { get; set; }
		public int CourseId { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
	}
}
