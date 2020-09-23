using Microsoft.AspNetCore.Mvc;
using Moq.Web.Controllers;
using Moq.Web.Models;
using Moq.Web.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Moq.Web.Tests
{
    public class ProductApiControllerTests
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;

        private List<Product> products;

        public ProductApiControllerTests()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            products = new List<Product>()
            {
                new Product {Id = 1, Price = 500, Name = "Table", Color = "Red", Stock = 19},
                new Product {Id = 2, Price = 1130, Name = "Laptop", Color = "Black", Stock = 9},
                new Product {Id = 3, Price = 200, Name = "Mouse", Color = "Blue", Stock = 39}
            };
        }
        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _controller.GetProduct();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal<int>(3, returnProducts.ToList().Count());
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOkResult(int productId)
        {
            Product product = products.First(p => p.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProducts = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(productId, returnProducts.Id);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdInvalid_ReturnBadRequestResult(int productId)
        {
            Product product = products.First(p => p.Id == productId);

            var result = _controller.PutProduct(2, product);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            Product product = products.First(p => p.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));

            var result = _controller.PutProduct(productId, product);

            _mockRepo.Verify(p => p.Update(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnNoContent()
        {
            Product product = products.First();
            _mockRepo.Setup(repo => repo.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var createdActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(p => p.Create(product), Times.Once);

            Assert.Equal("GetProduct", createdActionResult.ActionName);
        }
    }
}
