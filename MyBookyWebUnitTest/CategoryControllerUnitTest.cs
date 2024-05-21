using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using MyBooky.Models;
using MyBookyWeb.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;



namespace MyBookyWebUnitTest
{
    public class CategoryControllerUnitTest
    {

        [Fact]
        public void Index()
        {
            // Arrangement
            var c1 = new Category { Id = 1, Name = "Mathematics", DisplayOrder = 1 };
            var c2 = new Category { Id = 2, Name = "History", DisplayOrder = 2 };
            var c3 = new Category { Id = 3, Name = "Physics", DisplayOrder = 3 };
            var categoryList = new List<Category> { c1, c2, c3 };

            var _unitofwork = new Mock<IUnitofWork>();
            _unitofwork.Setup(u => u.Category.GetAll()).Returns(categoryList);

            var categoryCtrl = new CategoryController(_unitofwork.Object);

            // Action
            var result = categoryCtrl.Index() as ViewResult;
            var model = result.Model as List<Category>;

            // Assertion
            Assert.Equal(categoryList.Count, model.Count); // Check if the counts are equal
            for (int i = 0; i < categoryList.Count; i++)
            {
                Assert.Equal(categoryList[i].Id, model[i].Id); // Check if Ids match
                Assert.Equal(categoryList[i].Name, model[i].Name); // Check if Names match
                Assert.Equal(categoryList[i].DisplayOrder, model[i].DisplayOrder); // Check if DisplayOrders match
            }
        }
        [Fact]
        public void Create_ReturnsCorrectView()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitofWork>();
            var categoryController = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = categoryController.Create() as ViewResult;

            // Assert
            Assert.NotNull(result); // Check if the result is not null
            Assert.Null(result.ViewName); // Check if the view name is null (default behavior)
        }

        [Fact]
        public void Create()
        {
            // Arrangement
            var c1 = new Category { Id = 1, Name = "Mathematics", DisplayOrder = 1 };
            var c2 = new Category { Id = 2, Name = "History", DisplayOrder = 2 };
            var c3 = new Category { Id = 3, Name = "Physics", DisplayOrder = 3 };
            var categoryList = new List<Category> { c1, c2, c3 };

            var _unitofwork = new Mock<IUnitofWork>();
            var categoryCtrl = new CategoryController(_unitofwork.Object);
            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Setup
            var c4 = new Category { Id = 4, Name = "Programming", DisplayOrder = 4 };
            _unitofwork.Setup(u => u.Category.Add(c4)).Callback((Category item) =>
            {
                categoryList.Add(item);
            });

            // Action
            categoryCtrl.TempData = tempData; // Set TempData in the controller
            categoryCtrl.Create(c4);

            // Assertion
            Assert.Contains(c4, categoryList);
            Assert.Equal("Category successfully created", categoryCtrl.TempData["success"]);
        }
        [Fact]
        public void EditGet()
        {
            // Arrange
            var categoryId = 1; // The category Id to be retrieved

            var categoryFromDb = new Category { Id = categoryId, Name = "Mathematics", DisplayOrder = 1 };

            var _unitofwork = new Mock<IUnitofWork>();
            var categoryCtrl = new CategoryController(_unitofwork.Object);

            _unitofwork.Setup(u => u.Category.GetById(categoryId)).Returns(categoryFromDb);

            // Action

            // Assert

            if (categoryCtrl.Edit(categoryId) is ViewResult result)
            {
                // Check if the model is of the expected type
                Assert.IsType<Category>(result.Model);

                // Check individual properties of the category model
                var model = result.Model as Category;
                Assert.Equal(categoryFromDb.Id, model.Id);
                Assert.Equal(categoryFromDb.Name, model.Name);
                Assert.Equal(categoryFromDb.DisplayOrder, model.DisplayOrder);
            }

        }
        [Fact]
        public void DeleteGet()
        {
            // Arrange
            var categoryId = 1; // The category Id to be retrieved

            var categoryFromDb = new Category { Id = categoryId, Name = "Mathematics", DisplayOrder = 1 };

            var _unitofwork = new Mock<IUnitofWork>();
            var categoryCtrl = new CategoryController(_unitofwork.Object);

            _unitofwork.Setup(u => u.Category.GetById(categoryId)).Returns(categoryFromDb);

            // Action

            // Assert

            if (categoryCtrl.Delete(categoryId) is ViewResult result)
            {
                // Check if the model is of the expected type
                Assert.IsType<Category>(result.Model);

                // Check if the model is of the expected type
                Assert.IsType<Category>(result.Model);

                // Check individual properties of the category model
                var model = result.Model as Category;
                if (model != null) // Add null check here
                {
                    Assert.Equal(categoryFromDb.Id, model.Id);
                    Assert.Equal(categoryFromDb.Name, model.Name);
                    Assert.Equal(categoryFromDb.DisplayOrder, model.DisplayOrder);
                }
                else
                {
                    // Log or assert if model is null
                    Assert.NotNull(model); // Assert to fail the test and log if model is null
                }
            }
        }

        [Fact]
        public void Delete()
        {
            // Arrange

            var categoryId = 1; // The category Id to be deleted

            var categoryFromDb = new Category { Id = categoryId, Name = "Mathematics", DisplayOrder = 1 };

            var _unitofwork = new Mock<IUnitofWork>();
            var categoryCtrl = new CategoryController(_unitofwork.Object);
            //Setup
            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            categoryCtrl.TempData = tempData; // Set TempData in the controller

            _unitofwork.Setup(u => u.Category.Get(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>(),false)).Returns(categoryFromDb);

            //Action
            var result = categoryCtrl.DeletePost(categoryId);

            // Assertion
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult)?.ActionName);
            Assert.Equal("Category successfully deleted", categoryCtrl.TempData["success"]);

        }
        [Fact]
        public void EditPost()
        {
            // Arrange
            var c1 = new Category { Id = 1, Name = "Mathematics", DisplayOrder = 1 };
            var c2 = new Category { Id = 2, Name = "History", DisplayOrder = 2 };
            var c3 = new Category { Id = 3, Name = "Physics", DisplayOrder = 3 };
            _ = new List<Category> { c1, c2, c3 };

            var categoryId = 1; // The category Id to be retrieved
            var categoryToUpdate = new Category { Id = categoryId, Name = "Mathematics", DisplayOrder = 1 };

            var _unitofwork = new Mock<IUnitofWork>();
            var categoryCtrl = new CategoryController(_unitofwork.Object);

            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            categoryCtrl.TempData = tempData; // Set TempData in the controller

            // Ensure ModelState is valid
            categoryCtrl.ModelState.Clear();

            _unitofwork.Setup(u => u.Category.Update(categoryToUpdate));
            _unitofwork.Setup(u => u.Save());

            // Action
            var result = categoryCtrl.Edit(categoryToUpdate) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Category successfully updated", categoryCtrl.TempData["success"]);
        }

    }
}