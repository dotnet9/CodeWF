# 贡献指南

感谢你帮助改进 CodeWF。

## 开发环境

```powershell
npm install
dotnet restore CodeWF.slnx
```

启动后端、前台和后台：

```powershell
npm run dev:api
npm run dev:frontend
npm run dev:admin
```

## 质量检查

提交合并请求前，请先运行：

```powershell
dotnet test CodeWF.slnx
npm run build:frontend
npm run build:admin
```

## 合并请求规范

- 保持改动聚焦，便于审查。
- 优先沿用项目已有范式，不轻易引入新的抽象。
- 修改解析逻辑、接口行为或写入操作时，应补充或更新测试。
- 涉及公开接口时，尽量保持向后兼容。
- 不要提交密钥、生产账号、构建产物或本地资源仓库。

## 代码风格

- 后端使用 ASP.NET Core Minimal API 和服务类组织业务逻辑。
- 前台使用 Next.js App Router。
- 后台使用 React 和 Ant Design。
- 源码和文档统一使用 UTF-8 编码。
