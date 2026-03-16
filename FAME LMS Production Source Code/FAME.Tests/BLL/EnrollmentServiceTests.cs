using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Moq;
using NUnit.Framework;

namespace FAME.Tests.BLL
{
    [TestFixture]
    public class EnrollmentServiceTests
    {
        private Mock<IEnrollmentRepository> _mockEnrollmentRepo;
        private Mock<ICourseRepository> _mockCourseRepo;
        private Mock<ISectionRepository> _mockSectionRepo;
        private Mock<IPackageRepository> _mockPackageRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private EnrollmentService _service;

        [SetUp]
        public void Setup()
        {
            _mockEnrollmentRepo = new Mock<IEnrollmentRepository>();
            _mockCourseRepo = new Mock<ICourseRepository>();
            _mockSectionRepo = new Mock<ISectionRepository>();
            _mockPackageRepo = new Mock<IPackageRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _service = new EnrollmentService(
                _mockEnrollmentRepo.Object,
                _mockCourseRepo.Object,
                _mockSectionRepo.Object,
                _mockPackageRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Test]
        public void ToggleStudentBlockStatus_ReturnsRepoResult()
        {
            // Arrange
            string email = "test@example.com";
            bool isActive = false;
            _mockEnrollmentRepo.Setup(r => r.Block(email, isActive)).Returns(true);

            // Act
            var result = _service.ToggleStudentBlockStatus(email, isActive);

            // Assert
            Assert.IsTrue(result);
            _mockEnrollmentRepo.Verify(r => r.Block(email, isActive), Times.Once);
        }

        [Test]
        public void ProcessEnrollment_CallsRepoWithCorrectParams()
        {
            // Arrange
            var model = new AddSubscriptioVM { StudentFid = "student-123" };
            _mockEnrollmentRepo.Setup(r => r.Enroll(model, true, false, 100m)).Returns(true);

            // Act
            var result = _service.ProcessEnrollment(model, true, false, 100m);

            // Assert
            Assert.IsTrue(result);
            _mockEnrollmentRepo.Verify(r => r.Enroll(model, true, false, 100m), Times.Once);
        }
    }
}
