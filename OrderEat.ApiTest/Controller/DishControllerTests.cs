﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderEat.ApiTest.Helpers;
using OrderEat.Application.Dish;
using OrderEat.Domain.Entities;
using OrderEat.Domain.Models;
using OrderEat.Infrastructure.Persistance;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace OrderEat.ApiTest.Controller
{
    public class DishControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _adminClient;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IConfiguration _configuration;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _ownerClient;

        public DishControllerTests(WebApplicationFactory<Program> factory)
        {
            _configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();
            var authenticationSettings = new AuthenticationSettings();

            _configuration.GetSection("Authentication").Bind(authenticationSettings);
            _authenticationSettings = authenticationSettings;
            _factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var dbContextOptions = services
                            .SingleOrDefault(service => service.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<OrderEatDbContext>));
                        services.Remove(dbContextOptions);

                        services
                         .AddDbContext<OrderEatDbContext>(options => options.UseInMemoryDatabase("FastFoodDb"));
                    });
                });

            _adminClient = _factory.CreateClient();
            var adminToken = GenerateJwtToken("Admin", "1");
            _adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            _ownerClient = _factory.CreateClient();
            var ownerToken = GenerateJwtToken("Owner", "2");
            _ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        }

        [Theory]
        [InlineData(null, "TestDescription", 10.50, 1000, true, true)]
        [InlineData("", "TestDescription", 10.50, 1000, true, true)]
        [InlineData("Name", null, 10.50, 1000, true, true)]
        [InlineData("Name", "", 10.50, 1000, true, true)]
        [InlineData("N", "TestDescription", 10.50, 1000, true, true)]
        [InlineData("Name", "T", 10.50, 1000, true, true)]
        [InlineData("Name", "TestDescription", -1, 1000, true, true)]
        [InlineData("Name", "TestDescription", 10.50, 0, true, true)]
        public async Task Create_ForInvalidModel_ReturnsBadRequest(string name, string description, decimal basePrize, int baseCaloriValue, bool allowedCustomization, bool isAvilible)
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);
            var dto = new GetDishDto()
            {
                Name = name,
                Description = description,
                BasePrize = basePrize,
                BaseCaloricValue = baseCaloriValue,
                AllowedCustomization = allowedCustomization,
                IsAvilable = isAvilible
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PostAsync($"/api/restaurant/{restaurant.Id}/dish", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_ForNotRestaurantOwner_ReturnsForbidden()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _ownerClient.PostAsync($"/api/restaurant/{restaurant.Id}/dish", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_ForValidModel_ReturnsCreated()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PostAsync($"/api/restaurant/{restaurant.Id}/dish", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_InvalidRestaurantId_ReturnsNotFound()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PostAsync("/api/restaurant/34563/dish", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ForInvalidId_ReturnsNotFound()
        {
            //arrange
            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
            };
            await Seed(dish);
            //act

            var response = await _adminClient.DeleteAsync("api/dish/23452");
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ForNonRestaurantOwner_ReturnsForbidden()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "RestaurantName",
                Description = "TestDescription",
                CreatedById = 3,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            //act

            var response = await _ownerClient.DeleteAsync($"api/dish/{dish.Id}");
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_ForValidid_ReturnsNoContent()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            //act

            var response = await _adminClient.DeleteAsync($"api/dish/{dish.Id}");
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_ForInvalidId_ReturnsNotFound()
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "TestName",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            //act

            var response = await _adminClient.GetAsync("api/dish/3456");
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("?PageNumber=1&PageSize=7")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=InvalidProperty")]
        public async Task Get_ForInvalidQueryParams_RetursBadRequest(string query)
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);
            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
            };
            await Seed(dish);

            //act
            var response = await _adminClient.GetAsync($"api/restaurant/{restaurant.Id}/dish/{query}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("?PageNumber=1&PageSize=15")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=Name")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=Description")]
        public async Task Get_ForValidQueryParams_ReturnsOK(string query)
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);
            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
            };
            await Seed(dish);

            //act
            var response = await _adminClient.GetAsync($"api/restaurant/{restaurant.Id}/dish/{query}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_ForValidId_ReturnsOk()
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "TestName",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            //act

            var response = await _adminClient.GetAsync($"api/dish/{dish.Id}");
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("?PageNumber=1&PageSize=7")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=InvalidProperty")]
        public async Task GetSmart_ForInvalidQueryParams_ReturnsBadRequest(string query)
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "TestName",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            var user = new User()
            {
                Id = 1,
                Name = "John Doe",
                Email = "test@email.com"
            };
            await SeedUser(user);

            //act
            var response = await _adminClient.GetAsync($"/api/restaurant/{restaurant.Id}/dishSmart{query}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("?PageNumber=1&PageSize=15")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=Name")]
        [InlineData("?SearchPhrase=phrase&PageNumber=1&PageSize=15&SortBy=Description")]
        public async Task GetSmart_ForValidQueryParams_ReturnsOK(string query)
        {
            //arrange
            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "TestName",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);
            var user = new User()
            {
                Id = 1,
                Name = "John Doe",
                Email = "test@email.com"
            };
            await SeedUser(user);

            //act
            var response = await _adminClient.GetAsync($"/api/restaurant/{restaurant.Id}/dishSmart{query}");
            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_ForInvalidId_ReturnsNotFound()
        {
            //arrange

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            await Seed(dish);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PutAsync("api/dish/5342", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(null, "TestDescription", 10.50, 1000, true, true)]
        [InlineData("", "TestDescription", 10.50, 1000, true, true)]
        [InlineData("Name", null, 10.50, 1000, true, true)]
        [InlineData("Name", "", 10.50, 1000, true, true)]
        [InlineData("N", "TestDescription", 10.50, 1000, true, true)]
        [InlineData("Name", "T", 10.50, 1000, true, true)]
        [InlineData("Name", "TestDescription", -1, 1000, true, true)]
        [InlineData("Name", "TestDescription", 10.50, 0, true, true)]
        public async Task Update_ForInvalidModel_ReturnsBadRequest(string name, string description, decimal basePrize, int baseCaloriValue, bool allowedCustomization, bool isAvilible)
        {
            //arrange

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            await Seed(dish);

            var dto = new GetDishDto()
            {
                Name = name,
                Description = description,
                BasePrize = basePrize,
                BaseCaloricValue = baseCaloriValue,
                AllowedCustomization = allowedCustomization,
                IsAvilable = isAvilible
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PutAsync($"api/dish/{dish.Id}", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_ForNonRestaurantOwner_ReturnsForbidden()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 1,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _ownerClient.PutAsync($"api/dish/{dish.Id}", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Update_ForRestaurantOwner_ReturnsOk()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Name",
                Description = "TestDescription",
                CreatedById = 2,
                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "Name",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _ownerClient.PutAsync($"api/dish/{dish.Id}", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Update_ForValidModel_ReturnsOK()
        {
            //arrange

            var restaurant = new Restaurant()
            {
                Name = "Test Name",
                Description = "TestDescription",

                ContactDetails = new RestaurantContactDetails
                {
                    ContactNumber = "111111111",
                    Email = "test@email.com",
                    Country = "TestCountry",
                    City = "TestCity",
                    Street = "TestStreet",
                    ApartmentNumber = "1/10"
                }
            };
            await Seed(restaurant);

            var dish = new Dish()
            {
                Name = "TestName",
                Description = "description",

                BasePrize = (decimal)10.56,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true,
                RestaurantId = restaurant.Id
            };
            await Seed(dish);

            var dto = new GetDishDto()
            {
                Name = "TestName",
                Description = "TestDescription",

                BasePrize = (decimal)10.50,
                BaseCaloricValue = 1000,

                AllowedCustomization = true,
                IsAvilable = true
            };
            var httpContent = dto.ToJsonHttpContent();
            //act

            var response = await _adminClient.PutAsync($"api/dish/{dish.Id}", httpContent);
            //assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        private string GenerateJwtToken(string roleName, string userId)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email,"test@email.com"),
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.Role, roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private async Task SeedUser(User user)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<OrderEatDbContext>();
            if (!_dbContext.Users.Contains(user))
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task Seed<T>(T obj) where T : class
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<OrderEatDbContext>();

            _dbContext.Add(obj);

            await _dbContext.SaveChangesAsync();
        }
    }
}