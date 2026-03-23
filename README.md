
# 学生成绩管理系统 (StudentGradeManagement)

--若要本地演示，请务必阅读本手册，配置好相应环境

## 项目介绍

本项目是为HEU信息管理系统实践课程制作的一个轻量级的学生成绩管理系统服务，基于ASP.NET Core开发，适配学校日常的学生、班级、课程、成绩管理场景，支持管理员和学生双角色的权限管控，提供完整的增删改查及数据统计能力，可快速部署并完成本地演示（建议使用VSCode装配.NET和c#插件使用，详见微软官方c#教程--设计报告中有链接）。

## 技术栈

- **后端框架**：ASP.NET Core 10.0
- **数据库**：MySQL 5.7+
- **前端依赖**：jQuery 3.6+、Bootstrap 5.x、jQuery Validation、jQuery Validation Unobtrusive
- **数据库访问**：MySql.Data 8.0+
- **运行环境**：跨平台（Windows/Linux/macOS）（Windows平台开发，理论上基于最新ASP.NET Core能实现跨平台，不过没测试）

## 功能模块

| 角色   | 核心功能                                                     |
| ------ | ------------------------------------------------------------ |
| 管理员 | 学生/课程信息管理、成绩录入/修改、用户账号管理、成绩统计查询 |
| 学生   | 个人信息查看、个人成绩查询                                   |

## 环境要求

- 已安装 .NET 10.0 SDK（验证方式：终端执行 `dotnet --version` 显示10.0+版本）
- 已安装 MySQL 5.7/8.0（服务正常运行，验证方式：`mysql -u root -p` 可登录）
- 浏览器：Chrome/Firefox/Edge（推荐Chrome 90+）
- 操作系统：Windows 10+/macOS 12+/Ubuntu 20.04+

## 安装与配置

### 1. 项目文件准备

将项目源代码解压至本地目录（如 `D:\Projects\`），确保目录结构完整，且不要包含中文，核心目录如下：

```
DataBase/                # 数据库脚本
└── creat_table.sql      # 数据库表结构及初始化脚本
StudentGradeManagementBackend/
├── wwwroot/                 # 静态资源目录
│   ├── lib/                 # 前端依赖库
│   │   ├── bootstrap/       # Bootstrap框架及许可证文件
│   │   ├── jquery/          # jQuery库及许可证
│   │   └── jquery-validation/ # jQuery验证插件及许可证
│   ├── css/                 # 自定义样式
│   └── js/                  # 自定义脚本
├── Pages/                   # Razor Pages页面及后台逻辑
│   ├── CourseManagement.cshtml.cs  # 课程管理页面逻辑
│   ├── Index.cshtml.cs             # 首页逻辑
│   └── ...                  # 其他功能页面
├── Services/                # 业务逻辑服务类
│   ├── StudentService.cs    # 学生管理服务
│   ├── CourseService.cs     # 课程管理服务
│   └── ...                  # 其他服务类
├── Models/                  # 数据模型类
├── obj/                     # 编译输出目录（自动生成）
├── appsettings.json         # 应用配置文件（含数据库连接字符串）
├── appsettings.Development.json # 开发环境配置
├── Program.cs               # 应用入口及服务配置
└── StudentGradeManagementBackend.csproj # 项目配置文件
```

### 2. 依赖包还原

打开终端（Windows：CMD/PowerShell，macOS/Linux：Terminal），进入项目根目录StudentGradeManagementBackend/，执行：

```bash
# 还原项目所有NuGet依赖
dotnet restore
```

执行完成后无报错即表示依赖还原成功。

### 3. 数据库配置

#### 步骤1：创建数据库并初始化数据

1. 登录MySQL客户端（以root账号为例）：
   ```bash
   mysql -u root -p
   # 输入MySQL密码后回车
   ```
2. 执行数据库初始化脚本：
   ```sql
   -- 执行项目内的creat_table.sql脚本（替换为你的脚本实际路径）
   source D:\Projects\DataBase\creat_table.sql;
   ```

   脚本执行完成后，会自动创建 `school_db` 数据库，并生成以下核心表：- `departments`（院系）、`classes`（班级）、`students`（学生）
   - `courses`（课程）、`scores`（成绩）、`users`（登录用户）
     同时初始化管理员账号：`admin` / 密码：`123456`，以及测试用院系、班级、学生、课程数据。

#### 步骤2：修改数据库连接配置

打开项目根目录的 `appsettings.json` 文件，修改 `DefaultConnection` 为你的本地MySQL配置（修改对应参数为你的实际环境的）：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=school_db;Uid=root;Pwd=你的MySQL密码;CharSet=utf8mb4;"
  }
}
```

> 核心修改项：`password` 替换为本地MySQL root密码，`port` 若不是3306则同步修改。

## 本地演示说明

### 1. 启动后端服务

1. 保持终端在项目根目录，执行启动命令：
   ```bash
   dotnet run
   ```
2. 等待服务启动，控制台输出如下信息即表示启动成功：
   ```
   Building...
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5000
         Now listening on: https://localhost:5001
         Application started. Press Ctrl+C to shut down.
   ```

### 2. 访问系统并完成演示

#### 演示1：管理员角色操作

1. 打开浏览器，访问地址：`http://localhost:5000`（或HTTPS地址 `https://localhost:5001`）
2. 登录页面输入管理员账号：`admin`，密码：`123456`，点击“登录”
3. 核心操作演示：
   - **学生管理**：点击“学生管理”→“导入测试学生”（或手动新增），录入学号、姓名、所属班级等信息，提交后可在列表查看；
   - **课程管理**：新增课程（如“C#程序设计”，学分4），保存后查看课程列表；
   - **成绩管理**：选择学生、课程，输入成绩（如85），提交后可查看该学生的成绩；
   - **用户管理**：对网站的登录账号进行管理，包括管理员和学生两种账号类型。

#### 演示2：学生角色操作

1. 管理员先创建学生账号：
   - 进入“用户管理”→“新增用户”，选择角色“学生”，输入用户名（如学生学号“20230001”），密码（如“123456”），关联对应学生信息；
2. 退出管理员登录，重新在登录页输入学生账号：`20230001`，密码：`123456`，点击“登录”；
3. 登陆后即可查看本人已录入成绩，若无成绩，则显示暂时没有成绩信息。

### 3. 演示注意事项

- 演示过程中若出现“数据库连接失败”：检查MySQL服务是否启动（Windows：服务列表中查看“MySQL80”状态；），或连接字符串中的账号/密码/端口是否正确；
- 若页面样式错乱：检查前端依赖文件（jQuery/Bootstrap）是否完整，或清除浏览器缓存后刷新；
- 演示完成后，可按 `Ctrl+C` 终止后端服务，也可保留服务持续测试。

## 数据库结构

核心数据表字段说明：

| 表名        | 主键          | 核心字段                                                                                 | 关联表                              |
| ----------- | ------------- | ---------------------------------------------------------------------------------------- | ----------------------------------- |
| departments | department_id | department_name（院系名称）                                                              | -                                   |
| classes     | class_id      | class_name（班级名称）、department_id（院系 ID）                                         | departments                         |
| students    | student_id    | name（姓名）、class_id（班级 ID）、department_id（院系 ID）、enrollment_year（入学年份） | classes、departments                |
| courses     | course_id     | course_name（课程名称）、credit（学分）                                                  | -                                   |
| scores      | id            | student_id（学号）、course_id（课程号）、score（成绩）                                   | students、courses                   |
| users       | user_id       | password（密码）、role（角色：admin/student）                                            | students（user_id 关联 student_id） |

## 许可证说明

- 项目中使用的前端依赖（jQuery、Bootstrap等）遵循MIT开源许可证；
- 项目自研源代码为非商用学习用途，可自由修改和二次开发（但不要不加说明地直接搬用）。

## 常见问题与备注

1. 本项目为极为简易的课程实践项目，有许多待改进的地方，如需加密存储密码（如MD5/SHA256），部分统计信息功能不完备等等未实现；
2. 若 `dotnet run` 提示端口被占用：修改 `Properties/launchSettings.json` 中的端口号，或关闭占用端口的程序；
3. 测试数据可通过 `DataBase/creat_data.sql` 批量导入（可自行增添），快速完成演示环境搭建；
4. 班级和院系的数据必须提前通过sql数据库操作提前导入（至少一条，预备数据creat_data.sql中有），不然学生信息没法正常录入（网页操作数据提交存在关联校验）；
5. 同理管理员账号也至少得有一个，默认 `DataBase/creat_data.sql`中管理员账号：`admin`，密码：`123456`；
6. 启动报错「数据库连接失败」：检查MySQL服务是否启动；核对 `appsettings.json` 中密码/端口是否正确；确认MySQL允许本地连接；
   --7. 暂未上线github，但编写此README.md是为了预计上线github(qwq)。
