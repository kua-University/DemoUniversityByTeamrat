using Castle.Core.Configuration;
using ContosoUniversity.Models;
using ContosoUniversity.Pages;
using Microsoft.AspNetCore.Identity;
using Moq;
using Stripe.Checkout;
using Stripe.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUniversity.Test
{
    public class ComponentTests
    {
        [Fact]
        public void OnGet_ShouldProcessPaymentSuccessfully()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config["Stripe:PublishableKey"]).Returns("test_publishable_key");

            var mockSessionService = new Mock<SessionService>();
            mockSessionService.Setup(service => service.Create(It.IsAny<SessionCreateOptions>()))
                              .Returns(new Session { Id = "mock_session_id" });

            var paymentModel = new PaymentModel(mockConfiguration.Object) { SessionService = mockSessionService.Object };

            // Act
            paymentModel.OnGet();

            // Assert
            Assert.Equal("test_publishable_key", paymentModel.PublishableKey);
            Assert.Equal("mock_session_id", paymentModel.CheckoutSessionId);
        }

        [Fact]
        public void ShouldRegisterStudentSuccessfully()
        {
            // Arrange
            var mockStudentRepository = new Mock<IStudentRepository>();
            var mockCourseRepository = new Mock<ICourseRepository>();

            var registrationService = new RegistrationService(mockStudentRepository.Object, mockCourseRepository.Object);

            var student = new Student { Id = 1, Name = "John Doe" };
            var course = new Course { Id = 101, Name = "Math 101" };

            mockStudentRepository.Setup(repo => repo.RegisterStudent(It.IsAny<Student>())).Returns(student);
            mockCourseRepository.Setup(repo => repo.GetCourseById(It.IsAny<int>())).Returns(course);

            // Act
            var result = registrationService.RegisterStudentToCourse(1, 101);

            // Assert
            Assert.Equal(student, result.Student);
            Assert.Equal(course, result.Course);
        }

        [Fact]
        public void ShouldAuthenticateUserSuccessfully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var userManager = new UserManager(mockUserRepository.Object);
            var user = new User { Username = "johndoe", Password = "password123" };

            mockUserRepository.Setup(repo => repo.AuthenticateUser("johndoe", "password123")).Returns(user);

            // Act
            var result = userManager.AuthenticateUser("johndoe", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("johndoe", result.Username);
        }

        [Fact]
        public void ShouldRegisterCourseSuccessfully()
        {
            // Arrange
            var mockCourseRepository = new Mock<ICourseRepository>();
            var courseService = new CourseService(mockCourseRepository.Object);
            var course = new Course { Id = 101, Name = "Math 101" };

            mockCourseRepository.Setup(repo => repo.GetCourseById(It.IsAny<int>())).Returns(course);

            // Act
            var result = courseService.RegisterCourse(101);

            // Assert
            Assert.Equal(course, result);
        }

        [Fact]
        public void ShouldCreateStripePaymentSession()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config["Stripe:PublishableKey"]).Returns("test_publishable_key");

            var mockStripeService = new Mock<IStripeSessionService>();
            mockStripeService.Setup(service => service.CreateSession(It.IsAny<SessionCreateOptions>()))
                             .Returns(new Session { Id = "mock_session_id" });

            var paymentService = new StripePaymentService(mockConfiguration.Object, mockStripeService.Object);

            // Act
            var result = paymentService.CreatePaymentSession();

            // Assert
            Assert.Equal("mock_session_id", result.Id);
        }

    }
}
