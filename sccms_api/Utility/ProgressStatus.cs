using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public enum ProgressStatus
    {
		[Display(Name = "Đang chờ")]
		Pending,

		[Display(Name = "Được duyệt")]
		Approved,

		[Display(Name = "Bị từ chối")]
		Rejected,

        Enrolled,
        Graduated,
        DropOut,
		Delete
	}
}
