# KnowledgeBase - 智能知识库管理系统

一个基于.NET 9和Vue 3的现代化知识库管理系统，集成了AI智能搜索、富文本编辑、标签管理等功能。

## 🚀 功能特性

- **智能笔记管理**: 支持富文本编辑、Markdown渲染、代码高亮
- **AI智能搜索**: 基于向量数据库的语义搜索，支持自然语言查询
- **标签系统**: 灵活的标签分类和管理
- **用户认证**: JWT身份验证，支持用户注册和登录
- **文件附件**: 支持笔记附件上传和管理
- **响应式设计**: 现代化的UI界面，支持多设备访问
- **实时通知**: 操作反馈和状态提示

## 🛠️ 技术栈

### 后端 (KnowledgeBase.API)
- **.NET 9**: 最新的.NET框架
- **Entity Framework Core**: ORM框架，支持PostgreSQL
- **JWT认证**: 基于Token的身份验证
- **Redis缓存**: 高性能缓存支持
- **Qdrant向量数据库**: 用于AI语义搜索
- **QuestPDF**: PDF生成功能
- **Scalar**: API文档生成

### 前端 (knowledgebase.ui)
- **Vue 3**: 渐进式JavaScript框架
- **Vue Router**: 单页应用路由
- **Pinia**: 状态管理
- **Tailwind CSS**: 实用优先的CSS框架
- **Toast UI Editor**: 富文本编辑器
- **Axios**: HTTP客户端
- **Vite**: 快速构建工具

## 📋 系统要求

- .NET 9.0 SDK
- Node.js 18+ 
- PostgreSQL 12+
- Redis 6+
- Qdrant向量数据库

## 🚀 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/Mettiriolo/KnowledgeBase.git
cd KnowledgeBase
```

### 2. 后端设置

```bash
# 进入API项目目录
cd src/KnowledgeBase.API

# 安装依赖
dotnet restore

# 配置数据库连接
# 编辑 appsettings.json 文件，设置数据库连接字符串

# 运行数据库迁移
dotnet ef database update

# 启动API服务
dotnet run
```

### 3. 前端设置

```bash
# 进入UI项目目录
cd src/knowledgebase.ui

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

### 4. 访问应用

- 前端应用: http://localhost:5173
- API文档: https://localhost:5000/scalar

## ⚙️ 配置说明

### 数据库配置

在 `src/KnowledgeBase.API/appsettings.json` 中配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=knowledgebase;Username=your_username;Password=your_password",
    "Redis": "localhost:6379"
  }
}
```

### JWT配置

```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-here",
    "Issuer": "KnowledgeBase",
    "Audience": "KnowledgeBaseUsers",
    "ExpirationMinutes": 60
  }
}
```

### Qdrant向量数据库配置

```json
{
  "QdrantSettings": {
    "Host": "localhost",
    "Port": 6333,
    "CollectionName": "knowledgebase_embeddings"
  }
}
```

## 📁 项目结构

```
KnowledgeBase/
├── src/
│   ├── KnowledgeBase.API/          # 后端API项目
│   │   ├── Controllers/            # API控制器
│   │   ├── Data/                   # 数据访问层
│   │   ├── Models/                 # 数据模型
│   │   ├── Services/               # 业务逻辑服务
│   │   └── Middleware/             # 中间件
│   └── knowledgebase.ui/           # 前端Vue项目
│       ├── src/
│       │   ├── components/         # Vue组件
│       │   ├── views/              # 页面视图
│       │   ├── stores/             # Pinia状态管理
│       │   ├── services/           # API服务
│       │   └── router/             # 路由配置
│       └── public/                 # 静态资源
└── README.md
```

## 🔧 开发指南

### API开发

1. 在 `Controllers` 目录下创建新的控制器
2. 在 `Services` 目录下实现业务逻辑
3. 在 `Models` 目录下定义数据模型
4. 使用Entity Framework进行数据库操作

### 前端开发

1. 在 `components` 目录下创建可复用组件
2. 在 `views` 目录下创建页面组件
3. 在 `stores` 目录下管理状态
4. 在 `services` 目录下封装API调用

### 数据库迁移

```bash
# 创建新的迁移
dotnet ef migrations add MigrationName --output-dir Data/Migrations

# 更新数据库
dotnet ef database update
```

## 🧪 测试

```bash
# 运行后端测试
cd src/KnowledgeBase.API
dotnet test

# 运行前端测试
cd src/knowledgebase.ui
npm run test
```

## 📦 部署

### 后端部署

```bash
# 发布应用
dotnet publish -c Release -o ./publish

# 部署到服务器
# 将publish目录内容复制到服务器
```

### 前端部署

```bash
# 构建生产版本
npm run build

# 部署dist目录到Web服务器
```

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 📞 联系方式

如有问题或建议，请通过以下方式联系：

- 提交 Issue
- 发送邮件至: [yangnfan.xa@qq.com]

## 🙏 致谢

感谢所有为这个项目做出贡献的开发者和用户！