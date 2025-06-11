using Moq;
using SCCMS.Domain.Services.Implements;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Test
{
    [TestFixture]
    public class StudentGroupAssignmentTest
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private StudentGroupAssignmentService _service;

        [SetUp]
        public void Setup()
        {
            // Mock UnitOfWork
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // Initialize service
            //_service = new StudentGroupAssignmentService(_unitOfWorkMock.Object, null);
        }


    }
}
