using Api;
using Api.Extensions;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddApplication()          // Đăng ký các services tầng Application: MediatR, Validators, Pipeline Behaviors
	.AddInfrastructure(builder.Configuration)  // Đăng ký các services tầng Infrastructure: DbContext, JWT, PasswordHasher, ...
	.AddPresentation(builder.Configuration);   // Đăng ký các services tầng API: Endpoints, Swagger, Authorization, ExceptionHandlers

var app = builder.Build();

// Chỉ bật Swagger UI trong môi trường Development
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Bắt toàn bộ exception, trả về ProblemDetails chuẩn (400/500)
app.UseExceptionHandler();

// Chuyển hướng HTTP sang HTTPS
app.UseHttpsRedirection();

// Cho phép frontend gọi API theo danh sách origins trong cấu hình
app.UseCors(Api.DependencyInjection.CorsPolicyName);

// Xác thực người dùng (JWT Bearer)
app.UseAuthentication();

// Kiểm tra quyền truy cập (Permissions)
app.UseAuthorization();

// Đăng ký tất cả các Minimal API Endpoints
app.MapEndpoints();

app.Run();
