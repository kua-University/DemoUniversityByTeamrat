using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentAssertions;
using Xunit;

namespace DemoUniversity.Test
{
    public class ComponentTestsBy
    {

       

        [Theory]
        [InlineData("Test", "Student")]
        public async Task StudentsIndexPage_DisplaysStudentList(string firstName, string lastName)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_StudentsIndex")
                .Options;

            using var context = new SchoolContext(options);
            context.Students.Add(new Student { FirstMidName = firstName, LastName = lastName, EnrollmentDate = DateTime.Now });
            await context.SaveChangesAsync();

            // Act
            var students = await context.Students.ToListAsync();

            // Assert
            students.Should().NotBeNull();
            students.Count.Should().Be(1);
            students.First().LastName.Should().Be(lastName);
        }

        [Theory]
        [InlineData("New", "Student")]
        public async Task CreateStudent_ValidData_SavesToDatabase(string firstName, string lastName)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateStudent")
                .Options;

            using var context = new SchoolContext(options);

            var newStudent = new Student { FirstMidName = firstName, LastName = lastName, EnrollmentDate = DateTime.Now };

            // Act
            context.Students.Add(newStudent);
            await context.SaveChangesAsync();

            // Assert
            var createdStudent = await context.Students.FirstOrDefaultAsync(s => s.LastName == lastName);
            createdStudent.Should().NotBeNull();
            createdStudent.FirstMidName.Should().Be(firstName);
        }

        [Theory]
        [InlineData("Old", "Updated", "Student")]
        public async Task UpdateStudent_ValidData_UpdatesDatabase(string originalFirstName, string updatedFirstName, string lastName)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateStudent")
                .Options;

            using var context = new SchoolContext(options);
            var student = new Student { ID = 1, FirstMidName = originalFirstName, LastName = lastName, EnrollmentDate = DateTime.Now };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            // Act
            student.FirstMidName = updatedFirstName;
            context.Entry(student).State = EntityState.Modified;
            await context.SaveChangesAsync();

            // Assert
            var updatedStudent = await context.Students.FirstOrDefaultAsync(s => s.ID == 1);
            updatedStudent.Should().NotBeNull();
            updatedStudent.FirstMidName.Should().Be(updatedFirstName);
        }

        [Theory]
        [InlineData("Delete", "Student")]
        public async Task DeleteStudent_ValidId_RemovesFromDatabase(string firstName, string lastName)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_DeleteStudent")
                .Options;

            using var context = new SchoolContext(options);
            var student = new Student { ID = 1, FirstMidName = firstName, LastName = lastName, EnrollmentDate = DateTime.Now };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            // Act
            context.Students.Remove(student);
            await context.SaveChangesAsync();

            // Assert
            var deletedStudent = await context.Students.FirstOrDefaultAsync(s => s.ID == 1);
            deletedStudent.Should().BeNull();
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("John", "")]
        [InlineData("", "Doe")]
        public async Task CreateStudent_InvalidData_ReturnsError(string firstName, string lastName)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_InvalidInput")
                .Options;

            using var context = new SchoolContext(options);

            var invalidStudent = new Student { FirstMidName = firstName, LastName = lastName };

            // Act & Assert
            Func<Task> act = async () =>
            {
                context.Students.Add(invalidStudent);
                await context.SaveChangesAsync(); // This should throw a DbUpdateException
            };

            await act.Should().ThrowAsync<DbUpdateException>();
        }



    }
}
