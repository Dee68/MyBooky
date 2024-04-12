using MyBooky.Models;
using MyBookyWeb.Areas.Admin.Controllers;
using Moq;
using MyBooky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace MyBookyWebUnitTest
{
    public class CategoryControllerUnitTest
    {
        //Arrangment
        Category c1;
        Category c2;
        Category c3;
        List<Category> categoryList;

        CategoryController categoryCtrl;
        Mock<IUnitofWork> _unitofwork;
        public CategoryControllerUnitTest()
        {
            c1 = new Category { Id = 1, Name = "Mathematics", DisplayOrder = 1 };
            c2 = new Category { Id= 2, Name ="History", DisplayOrder= 2 };
            c3 = new Category { Id=3, Name ="Physics", DisplayOrder= 3 };
            categoryList = new List<Category>();
            categoryList.Add(c1);
            categoryList.Add(c2);
            categoryList.Add(c3);

            _unitofwork = new Mock<IUnitofWork>();
            categoryCtrl = new CategoryController(_unitofwork.Object);
        }
        [Fact]
        public void Index()
        {
            //setup
            _unitofwork.Setup(u => u.Category.GetAll()).Returns(categoryList);

            //Action
            var result = categoryCtrl.Index() as ViewResult;
            var model = result.Model as List<Category>;

            //Assertion
            Assert.Equal(categoryList.Count, model.Count); // Check if the counts are equal
            for (int i = 0; i < categoryList.Count; i++)
            {
                Assert.Equal(categoryList[i].Id, model[i].Id); // Check if Ids match
                Assert.Equal(categoryList[i].Name, model[i].Name); // Check if Names match
                Assert.Equal(categoryList[i].DisplayOrder, model[i].DisplayOrder); // Check if DisplayOrders match
            }
        }
    }
}