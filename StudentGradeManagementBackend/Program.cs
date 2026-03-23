using StudentGradeManagementBackend.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 配置Web根目录
builder.WebHost.UseWebRoot(Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));

// 读取数据库连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 添加Razor Pages支持
builder.Services.AddRazorPages();

// 添加CSRF保护配置
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.HeaderName = "X-CSRF-TOKEN";
    options.FormFieldName = "__RequestVerificationToken";
});

// 注册所有业务服务
builder.Services.AddScoped<StudentService>(provider => new StudentService(connectionString));
builder.Services.AddScoped<CourseService>(provider => new CourseService(connectionString));
builder.Services.AddScoped<ScoreService>(provider => new ScoreService(connectionString));
builder.Services.AddScoped<UserService>(provider => new UserService(connectionString));
builder.Services.AddScoped<ClassService>(provider => new ClassService(connectionString));
builder.Services.AddScoped<DepartmentService>(provider => new DepartmentService(connectionString));

// 添加Session支持
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 配置HTTP请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // 静态文件中间件
app.UseSession();     // Session中间件
app.UseRouting();     // 路由中间件
app.UseAuthorization(); // 授权中间件

// 映射Razor Pages
app.MapRazorPages();

// 启动应用
app.Run();

// using StudentGradeManagementBackend.Services;
// using Microsoft.Extensions.Configuration;

// var builder = WebApplication.CreateBuilder(args);

// // 设置WebRootPath
// builder.WebHost.UseWebRoot(Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));

// // 读取连接字符串
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// // Add services to the container.
// builder.Services.AddRazorPages();
// builder.Services.AddScoped<StudentService>(provider => new StudentService(connectionString));
// builder.Services.AddScoped<CourseService>(provider => new CourseService(connectionString));
// builder.Services.AddScoped<ScoreService>(provider => new ScoreService(connectionString));
// builder.Services.AddScoped<UserService>(provider => new UserService(connectionString));
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30);
// });

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// app.UseSession();
// app.UseRouting();

// app.UseAuthorization();

// app.MapStaticAssets();
// app.MapRazorPages()
//    .WithStaticAssets();

// app.Run();

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services.AddRazorPages();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// app.UseRouting();

// app.UseAuthorization();

// app.MapStaticAssets();
// app.MapRazorPages()
//    .WithStaticAssets();

// app.Run();
